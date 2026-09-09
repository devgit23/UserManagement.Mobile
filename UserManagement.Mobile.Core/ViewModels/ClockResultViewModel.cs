using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Attendance;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class ClockResultViewModel(
    IAttendanceApi attendanceApi,
    IConnectivityService connectivityService,
    ISessionService sessionService,
    ILocalAttendanceRepository localAttendanceRepo,
    LocalDatabase db) : ViewModelBase
{
    [ObservableProperty] private string _mode = "clockin";
    [ObservableProperty] private string _method = "face";
    [ObservableProperty] private decimal _confidence;
    [ObservableProperty] private bool _success;
    [ObservableProperty] private string _resultMessage = "";
    [ObservableProperty] private string _clockTime = "";
    [ObservableProperty] private string _statusLabel = "";
    [ObservableProperty] private string _methodDisplay = "";
    [ObservableProperty] private bool _isClocking = true;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var mode))
            Mode = mode?.ToString() ?? "clockin";
        if (query.TryGetValue("method", out var method))
            Method = method?.ToString() ?? "face";
        if (query.TryGetValue("confidence", out var conf) && decimal.TryParse(conf?.ToString(), out var c))
            Confidence = c;
    }

    public override async Task InitializeAsync()
    {
        Title = Mode == "clockin" ? "Clock In" : "Clock Out";
        MethodDisplay = Method switch
        {
            "face" => "Face Recognition",
            _ => "Manual"
        };

        IsClocking = true;
        ClearError();

        try
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var request = new AttendanceClockRequest
            {
                ClientTimeUtc = nowUtc,
                VerificationMethod = Method,
                VerificationConfidence = Confidence
            };

            if (connectivityService.IsConnected)
            {
                await ClockOnlineAsync(request, nowUtc);
            }
            else
            {
                await ClockOfflineAsync(request, nowUtc);
            }
        }
        catch (Exception ex)
        {
            Success = false;
            ResultMessage = Mode == "clockin" ? "Clock In Failed" : "Clock Out Failed";
            StatusLabel = ex.Message;
        }
        finally
        {
            IsClocking = false;
        }
    }

    private async Task ClockOnlineAsync(AttendanceClockRequest request, DateTimeOffset nowUtc)
    {
        AttendanceLogModel? result;

        if (Mode == "clockout")
        {
            result = await attendanceApi.ClockOutAsync(request);
        }
        else
        {
            result = await attendanceApi.ClockInAsync(request);
        }

        Success = true;
        ClockTime = result.ClockInAt.ToLocalTime().ToString("hh:mm tt");
        ResultMessage = Mode == "clockin" ? "Clocked In Successfully!" : "Clocked Out Successfully!";
        StatusLabel = result.LateArrivalMinutes > 0
            ? $"Late by {result.LateArrivalMinutes} min"
            : "On Time";

        // Auto-navigate back after delay
        await Task.Delay(3000);
        await NavigateAsync("//Dashboard");
    }

    private async Task ClockOfflineAsync(AttendanceClockRequest request, DateTimeOffset nowUtc)
    {
        var userId = sessionService.CurrentUser?.Id.ToString() ?? string.Empty;

        if (Mode == "clockout")
        {
            // Find and update the open local attendance log
            var openLog = await localAttendanceRepo.GetTodayOpenLogAsync(userId);
            if (openLog is not null)
            {
                openLog.ClockOutAt = nowUtc;
                openLog.Status = "completed";
                openLog.VerificationMethod = Method;
                openLog.VerificationConfidence = (double)Confidence;
                openLog.SyncStatus = SyncStatus.PendingUpload;
                openLog.LastModifiedUtc = nowUtc;
                await localAttendanceRepo.UpsertAsync(openLog);

                await QueueSyncItemAsync("ClockOut", openLog.Id, request, nowUtc);
            }
        }
        else
        {
            // Create a new local attendance log for clock-in
            var entityId = Guid.NewGuid().ToString();
            var localLog = new LocalAttendanceLog
            {
                Id = entityId,
                UserId = userId,
                WorkDate = DateOnly.FromDateTime(nowUtc.LocalDateTime).ToString("yyyy-MM-dd"),
                ClockInAt = nowUtc,
                Status = "open",
                VerificationMethod = Method,
                VerificationConfidence = (double)Confidence,
                SyncStatus = SyncStatus.PendingUpload,
                LastModifiedUtc = nowUtc
            };
            await localAttendanceRepo.UpsertAsync(localLog);

            await QueueSyncItemAsync("ClockIn", entityId, request, nowUtc);
        }

        Success = true;
        ClockTime = nowUtc.ToLocalTime().ToString("hh:mm tt");
        ResultMessage = Mode == "clockin"
            ? "Clocked In (Offline)"
            : "Clocked Out (Offline)";
        StatusLabel = "Will sync when connected";

        // Auto-navigate back after delay
        await Task.Delay(3000);
        await NavigateAsync("//Dashboard");
    }

    private async Task QueueSyncItemAsync(string operationType, string entityId, AttendanceClockRequest request, DateTimeOffset nowUtc)
    {
        var queueItem = new SyncQueueItem
        {
            EntityType = "AttendanceLog",
            EntityId = entityId,
            OperationType = operationType,
            SerializedPayload = JsonSerializer.Serialize(request),
            CreatedUtc = nowUtc
        };
        await db.GetConnection().InsertAsync(queueItem);
    }

    [RelayCommand]
    private async Task GoToDashboardAsync()
    {
        await NavigateAsync("//Dashboard");
    }
}
