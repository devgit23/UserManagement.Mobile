using System.Text.Json;
using UserManagement.Common.Attendance;
using UserManagement.Common.Timesheet;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Mappers;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.Offline.Sync;

public sealed class SyncEngine(
    LocalDatabase db,
    ILocalLeaveRepository leaveRepo,
    ILocalAttendanceRepository attendanceRepo,
    ILocalEmployeeRepository employeeRepo,
    ILocalTimesheetRepository timesheetRepo,
    ILeaveApi leaveApi,
    IAttendanceApi attendanceApi,
    ITimesheetApi timesheetApi) : ISyncEngine
{
    private const int MaxRetries = 5;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PushAsync(CancellationToken ct = default)
    {
        var conn = db.GetConnection();
        var pendingItems = await conn.Table<SyncQueueItem>()
            .OrderBy(x => x.CreatedUtc)
            .ToListAsync();

        foreach (var item in pendingItems)
        {
            if (ct.IsCancellationRequested) break;

            // Skip and remove permanently failed items
            if (item.RetryCount >= MaxRetries)
            {
                await UpdateEntitySyncStatusAsync(item.EntityType, item.EntityId, SyncStatus.Failed);
                await conn.DeleteAsync(item);
                continue;
            }

            try
            {
                await ProcessQueueItemAsync(item, ct);
                await conn.DeleteAsync(item);
                await UpdateEntitySyncStatusAsync(item.EntityType, item.EntityId, SyncStatus.Synced);
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.LastError = ex.Message;

                if (item.RetryCount >= MaxRetries)
                {
                    await UpdateEntitySyncStatusAsync(item.EntityType, item.EntityId, SyncStatus.Failed);
                    await conn.DeleteAsync(item);
                }
                else
                {
                    await conn.UpdateAsync(item);
                }
            }
        }
    }

    public async Task PullAsync(CancellationToken ct = default)
    {
        // Pull attendance logs
        try
        {
            var logs = await attendanceApi.GetLogsAsync(1, 50, ct: ct);
            if (logs is not null)
            {
                foreach (var log in logs)
                {
                    var local = EntityMapper.ToLocal(log);
                    local.SyncStatus = SyncStatus.Synced;
                    local.LastSyncedUtc = DateTimeOffset.UtcNow;
                    await attendanceRepo.UpsertAsync(local);
                }
            }
        }
        catch { /* Non-critical */ }

        // Pull timesheet entries (current week)
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday) weekStart = weekStart.AddDays(-7);
            var weekEnd = weekStart.AddDays(6);
            var grid = await timesheetApi.GetWeekGridAsync(weekStart: weekStart.ToString("yyyy-MM-dd"), ct: ct);
            if (grid?.Rows is not null)
            {
                foreach (var row in grid.Rows)
                {
                    for (int d = 0; d < 7; d++)
                    {
                        var cell = row.Days[d];
                        if (cell?.EntryId is not null)
                        {
                            var local = new LocalTimesheetEntry
                            {
                                Id = cell.EntryId.Value.ToString(),
                                ProjectId = row.ProjectId.ToString(),
                                TaskId = row.TaskId?.ToString(),
                                ProjectName = row.ProjectName,
                                ProjectCode = row.ProjectCode,
                                ProjectColorHex = row.ProjectColorHex,
                                TaskName = row.TaskName,
                                WorkDate = weekStart.AddDays(d).ToString("yyyy-MM-dd"),
                                Hours = cell.Hours,
                                Minutes = cell.Minutes,
                                Description = cell.Description,
                                IsBillable = row.IsBillable,
                                SyncStatus = SyncStatus.Synced,
                                LastSyncedUtc = DateTimeOffset.UtcNow,
                                LastModifiedUtc = DateTimeOffset.UtcNow
                            };
                            await timesheetRepo.UpsertAsync(local);
                        }
                    }
                }
            }
        }
        catch { /* Non-critical */ }

        // Pull leave requests
        try
        {
            var requests = await leaveApi.GetRequestsAsync(ct: ct);
            if (requests is not null)
            {
                foreach (var request in requests)
                {
                    var local = EntityMapper.ToLocal(request);
                    local.SyncStatus = SyncStatus.Synced;
                    local.LastSyncedUtc = DateTimeOffset.UtcNow;
                    await leaveRepo.UpsertAsync(local);
                }
            }
        }
        catch { /* Non-critical */ }
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await db.GetConnection().Table<SyncQueueItem>().CountAsync();
    }

    private async Task ProcessQueueItemAsync(SyncQueueItem item, CancellationToken ct)
    {
        switch (item.EntityType)
        {
            case "LeaveRequest" when item.OperationType == "Create":
                var leaveCreate = JsonSerializer.Deserialize<LeaveRequestDraftModel>(item.SerializedPayload, JsonOptions);
                if (leaveCreate is null) throw new InvalidOperationException("Failed to deserialize leave request.");
                await leaveApi.CreateRequestAsync(leaveCreate, ct);
                break;

            case "LeaveRequest" when item.OperationType == "Cancel":
                await leaveApi.CancelRequestAsync(Guid.Parse(item.EntityId), new LeaveCancelRequest(), ct);
                break;

            case "AttendanceLog" when item.OperationType == "ClockIn":
                var clockInReq = JsonSerializer.Deserialize<AttendanceClockRequest>(item.SerializedPayload, JsonOptions);
                if (clockInReq is null) throw new InvalidOperationException("Failed to deserialize clock-in request.");
                await attendanceApi.ClockInAsync(clockInReq, ct);
                break;

            case "AttendanceLog" when item.OperationType == "ClockOut":
                var clockOutReq = JsonSerializer.Deserialize<AttendanceClockRequest>(item.SerializedPayload, JsonOptions);
                if (clockOutReq is null) throw new InvalidOperationException("Failed to deserialize clock-out request.");
                await attendanceApi.ClockOutAsync(clockOutReq, ct);
                break;

            case "TimesheetEntry" when item.OperationType is "Create" or "Update":
                var tsEntry = JsonSerializer.Deserialize<TimesheetEntryEditModel>(item.SerializedPayload, JsonOptions);
                if (tsEntry is null) throw new InvalidOperationException("Failed to deserialize timesheet entry.");
                await timesheetApi.SaveEntryAsync(tsEntry, ct);
                break;

            case "TimesheetEntry" when item.OperationType == "Delete":
                await timesheetApi.DeleteEntryAsync(Guid.Parse(item.EntityId), ct);
                break;

            case "TimesheetTimer" when item.OperationType == "Start":
                var timerStart = JsonSerializer.Deserialize<TimerStartRequest>(item.SerializedPayload, JsonOptions);
                if (timerStart is null) throw new InvalidOperationException("Failed to deserialize timer start request.");
                await timesheetApi.StartTimerAsync(timerStart, ct);
                break;

            case "TimesheetTimer" when item.OperationType == "Stop":
                var timerStop = JsonSerializer.Deserialize<TimerStopRequest>(item.SerializedPayload, JsonOptions);
                if (timerStop is null) throw new InvalidOperationException("Failed to deserialize timer stop request.");
                await timesheetApi.StopTimerAsync(timerStop, ct);
                break;

            default:
                throw new InvalidOperationException($"Unknown sync operation: {item.EntityType}/{item.OperationType}");
        }
    }

    private async Task UpdateEntitySyncStatusAsync(string entityType, string entityId, SyncStatus status)
    {
        switch (entityType)
        {
            case "LeaveRequest":
                var leave = await leaveRepo.GetByIdAsync(entityId);
                if (leave is not null)
                {
                    leave.SyncStatus = status;
                    await leaveRepo.UpsertAsync(leave);
                }
                break;

            case "AttendanceLog":
                var attendance = await attendanceRepo.GetByIdAsync(entityId);
                if (attendance is not null)
                {
                    attendance.SyncStatus = status;
                    await attendanceRepo.UpsertAsync(attendance);
                }
                break;

            case "TimesheetEntry":
            case "TimesheetTimer":
                var tsEntry = await timesheetRepo.GetByIdAsync(entityId);
                if (tsEntry is not null)
                {
                    tsEntry.SyncStatus = status;
                    await timesheetRepo.UpsertAsync(tsEntry);
                }
                break;
        }
    }
}
