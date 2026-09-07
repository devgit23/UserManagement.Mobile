using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using UserManagement.Common.Attendance;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class AttendanceViewModel(
    IAttendanceApi attendanceApi,
    IWorkforceApi workforceApi,
    ILocalAttendanceRepository localRepo,
    IConnectivityService connectivity,
    ISessionService sessionService,
    IDialogService dialogService,
    IGeofenceMonitoringService geofenceService,
    LocalDatabase db) : ViewModelBase
{
    [ObservableProperty]
    private bool _isClockedIn;

    [ObservableProperty]
    private DateTimeOffset? _clockInTime;

    [ObservableProperty]
    private string _clockDuration = "--:--";

    [ObservableProperty]
    private bool _isOnline = true;

    [ObservableProperty]
    private bool _isGeofenceActive;

    [ObservableProperty]
    private string? _geofenceWarning;

    private GeofenceConfig? _geofenceConfig;
    private CancellationTokenSource? _timerCts;

    public ObservableCollection<AttendanceLogModel> RecentLogs { get; } = [];

    partial void OnIsClockedInChanged(bool value)
    {
        if (value && ClockInTime.HasValue)
        {
            StartDurationTimer();
        }
        else
        {
            StopDurationTimer();
            ClockDuration = "--:--";
        }
    }

    partial void OnClockInTimeChanged(DateTimeOffset? value)
    {
        if (IsClockedIn && value.HasValue)
        {
            UpdateDurationDisplay();
            StartDurationTimer();
        }
    }

    private void StartDurationTimer()
    {
        StopDurationTimer();
        _timerCts = new CancellationTokenSource();
        var token = _timerCts.Token;

        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    UpdateDurationDisplay();
                }
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    private void StopDurationTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    private void UpdateDurationDisplay()
    {
        if (!ClockInTime.HasValue) return;
        var elapsed = DateTimeOffset.UtcNow - ClockInTime.Value;
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        ClockDuration = elapsed.TotalHours >= 1
            ? $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
            : $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    public override async Task InitializeAsync()
    {
        Title = "Attendance";

        // Subscribe to geofence events
        geofenceService.GeofenceExited += OnGeofenceExited;
        geofenceService.GeofenceReEntered += OnGeofenceReEntered;
        geofenceService.GraceExpired += OnGeofenceGraceExpired;
        geofenceService.MonitoringError += OnGeofenceError;

        // Fetch geofence config
        if (connectivity.IsConnected)
        {
            try
            {
                _geofenceConfig = await workforceApi.GetGeofenceConfigAsync();
            }
            catch { /* non-critical */ }
        }

        await LoadDataAsync();

        // Start monitoring if already clocked in
        if (IsClockedIn && _geofenceConfig is { IsFullyConfigured: true })
        {
            await StartGeofenceAsync();
        }
    }

    protected override async Task OnRefreshAsync() => await LoadDataAsync();

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ClearError();
        IsOnline = connectivity.IsConnected;

        try
        {
            if (connectivity.IsConnected)
            {
                var logs = await attendanceApi.GetLogsAsync(1, 10);
                RecentLogs.Clear();
                if (logs is not null)
                {
                    foreach (var log in logs)
                    {
                        RecentLogs.Add(log);
                    }

                    // Check if currently clocked in (most recent log with no clock out)
                    var openLog = logs.FirstOrDefault(l => l.ClockOutAt is null);
                    IsClockedIn = openLog is not null;
                    ClockInTime = openLog?.ClockInAt;
                }
            }
            else
            {
                // Load from local cache
                var localLogs = await localRepo.GetAllAsync();
                RecentLogs.Clear();
                foreach (var log in localLogs.Take(10))
                {
                    RecentLogs.Add(new AttendanceLogModel
                    {
                        Id = Guid.Parse(log.Id),
                        UserId = Guid.Parse(log.UserId),
                        WorkDate = DateOnly.Parse(log.WorkDate),
                        ClockInAt = log.ClockInAt,
                        ClockOutAt = log.ClockOutAt,
                        Status = log.Status,
                        Remarks = log.Remarks
                    });
                }

                // Check local open log for clocked-in state
                var openLocal = localLogs.FirstOrDefault(l => l.Status == "open");
                IsClockedIn = openLocal is not null;
                ClockInTime = openLocal?.ClockInAt;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load attendance: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClockInAsync()
    {
        var remark = await PromptForRemarkAsync("Clock In");
        if (remark is null) return; // user cancelled

        IsBusy = true;
        ClearError();

        try
        {
            var request = new AttendanceClockRequest
            {
                ClientTimeUtc = DateTimeOffset.UtcNow,
                Remarks = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim()
            };

            if (connectivity.IsConnected)
            {
                var log = await attendanceApi.ClockInAsync(request);
                IsClockedIn = true;
                ClockInTime = log.ClockInAt;
                await LoadDataAsync();

                if (_geofenceConfig is { IsFullyConfigured: true })
                {
                    await StartGeofenceAsync();
                }
            }
            else
            {
                var entityId = Guid.NewGuid().ToString();
                var nowUtc = DateTimeOffset.UtcNow;
                var userId = sessionService.CurrentUser?.Id.ToString() ?? string.Empty;

                // Create local entity so offline list shows the record
                var localLog = new LocalAttendanceLog
                {
                    Id = entityId,
                    UserId = userId,
                    WorkDate = DateOnly.FromDateTime(nowUtc.LocalDateTime).ToString("yyyy-MM-dd"),
                    ClockInAt = nowUtc,
                    Status = "open",
                    Remarks = request.Remarks,
                    SyncStatus = SyncStatus.PendingUpload,
                    LastModifiedUtc = nowUtc
                };
                await localRepo.UpsertAsync(localLog);

                // Queue for server sync
                var queueItem = new SyncQueueItem
                {
                    EntityType = "AttendanceLog",
                    EntityId = entityId,
                    OperationType = "ClockIn",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = nowUtc
                };
                await db.GetConnection().InsertAsync(queueItem);

                IsClockedIn = true;
                ClockInTime = nowUtc;
            }
        }
        catch (ApiException apiEx)
        {
            SetError($"Clock in failed: {ExtractServerMessage(apiEx)}");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Clock in error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClockOutAsync()
    {
        var remark = await PromptForRemarkAsync("Clock Out");
        if (remark is null) return; // user cancelled

        IsBusy = true;
        ClearError();

        try
        {
            var request = new AttendanceClockRequest
            {
                ClientTimeUtc = DateTimeOffset.UtcNow,
                Remarks = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim()
            };

            if (connectivity.IsConnected)
            {
                await attendanceApi.ClockOutAsync(request);
                geofenceService.StopMonitoring();
                IsGeofenceActive = false;
                GeofenceWarning = null;
                IsClockedIn = false;
                ClockInTime = null;
                await LoadDataAsync();
            }
            else
            {
                var entityId = Guid.NewGuid().ToString();
                var nowUtc = DateTimeOffset.UtcNow;

                // Update the local open log to reflect clock-out
                var userId = sessionService.CurrentUser?.Id.ToString() ?? string.Empty;
                var openLog = await localRepo.GetTodayOpenLogAsync(userId);
                if (openLog is not null)
                {
                    openLog.ClockOutAt = nowUtc;
                    openLog.Status = "completed";
                    openLog.SyncStatus = SyncStatus.PendingUpload;
                    openLog.LastModifiedUtc = nowUtc;
                    if (!string.IsNullOrWhiteSpace(request.Remarks))
                    {
                        openLog.Remarks = string.IsNullOrWhiteSpace(openLog.Remarks)
                            ? request.Remarks.Trim()
                            : $"{openLog.Remarks} | Clock-out remark: {request.Remarks.Trim()}";
                    }
                    await localRepo.UpsertAsync(openLog);
                    entityId = openLog.Id;
                }

                // Queue for server sync
                var queueItem = new SyncQueueItem
                {
                    EntityType = "AttendanceLog",
                    EntityId = entityId,
                    OperationType = "ClockOut",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = nowUtc
                };
                await db.GetConnection().InsertAsync(queueItem);

                IsClockedIn = false;
                ClockInTime = null;
            }
        }
        catch (ApiException apiEx)
        {
            SetError($"Clock out failed: {ExtractServerMessage(apiEx)}");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Clock out error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string ExtractServerMessage(ApiException apiEx)
    {
        if (!string.IsNullOrWhiteSpace(apiEx.Content))
        {
            try
            {
                using var doc = JsonDocument.Parse(apiEx.Content);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? apiEx.Message;
                }
            }
            catch (JsonException) { }
        }

        return apiEx.Message;
    }

    private async Task<string?> PromptForRemarkAsync(string action)
    {
        return await dialogService.PromptAsync(
            action,
            "Add a remark (optional):",
            accept: action,
            cancel: "Cancel",
            placeholder: "e.g., Working from home today",
            maxLength: 500);
    }

    // --- Geofence ---

    private async Task StartGeofenceAsync()
    {
        if (_geofenceConfig is not { IsFullyConfigured: true })
            return;

        await geofenceService.StartMonitoringAsync(
            _geofenceConfig.OfficeLatitude!.Value,
            _geofenceConfig.OfficeLongitude!.Value,
            _geofenceConfig.RadiusMeters,
            _geofenceConfig.GracePeriodMinutes);

        IsGeofenceActive = geofenceService.IsMonitoring;
    }

    private void OnGeofenceExited(object? sender, GeofenceEventArgs e)
    {
        GeofenceWarning = $"You are {e.Distance:N0}m from the office. Auto punch-out in {_geofenceConfig?.GracePeriodMinutes ?? 5} min.";
    }

    private void OnGeofenceReEntered(object? sender, EventArgs e)
    {
        GeofenceWarning = null;
    }

    private async void OnGeofenceGraceExpired(object? sender, EventArgs e)
    {
        GeofenceWarning = null;
        IsGeofenceActive = false;

        try
        {
            if (connectivity.IsConnected)
            {
                await attendanceApi.ClockOutAsync(new AttendanceClockRequest
                {
                    ClientTimeUtc = DateTimeOffset.UtcNow,
                    Remarks = "Auto punch-out: left geofence area"
                });
            }
            else
            {
                var nowUtc = DateTimeOffset.UtcNow;
                var userId = sessionService.CurrentUser?.Id.ToString() ?? string.Empty;
                var openLog = await localRepo.GetTodayOpenLogAsync(userId);
                if (openLog is not null)
                {
                    openLog.ClockOutAt = nowUtc;
                    openLog.Status = "completed";
                    openLog.Remarks = string.IsNullOrWhiteSpace(openLog.Remarks)
                        ? "Auto punch-out: left geofence area"
                        : $"{openLog.Remarks} | Auto punch-out: left geofence area";
                    openLog.SyncStatus = SyncStatus.PendingUpload;
                    openLog.LastModifiedUtc = nowUtc;
                    await localRepo.UpsertAsync(openLog);

                    await db.GetConnection().InsertAsync(new SyncQueueItem
                    {
                        EntityType = "AttendanceLog",
                        EntityId = openLog.Id,
                        OperationType = "ClockOut",
                        SerializedPayload = JsonSerializer.Serialize(new AttendanceClockRequest
                        {
                            ClientTimeUtc = nowUtc,
                            Remarks = "Auto punch-out: left geofence area"
                        }),
                        CreatedUtc = nowUtc
                    });
                }
            }

            IsClockedIn = false;
            ClockInTime = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Auto clock-out failed: {ex.Message}");
        }
    }

    private void OnGeofenceError(object? sender, string message)
    {
        IsGeofenceActive = false;
        GeofenceWarning = null;
    }

    public override Task OnDisappearingAsync()
    {
        geofenceService.GeofenceExited -= OnGeofenceExited;
        geofenceService.GeofenceReEntered -= OnGeofenceReEntered;
        geofenceService.GraceExpired -= OnGeofenceGraceExpired;
        geofenceService.MonitoringError -= OnGeofenceError;
        return Task.CompletedTask;
    }
}
