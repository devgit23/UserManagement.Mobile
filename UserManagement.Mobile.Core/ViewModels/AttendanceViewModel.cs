using System.Collections.ObjectModel;
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

public partial class AttendanceViewModel(
    IAttendanceApi attendanceApi,
    ILocalAttendanceRepository localRepo,
    IConnectivityService connectivity,
    ISessionService sessionService,
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
        await LoadDataAsync();
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
        IsBusy = true;
        ClearError();

        try
        {
            var request = new AttendanceClockRequest
            {
                ClientTimeUtc = DateTimeOffset.UtcNow
            };

            if (connectivity.IsConnected)
            {
                var log = await attendanceApi.ClockInAsync(request);
                IsClockedIn = true;
                ClockInTime = log.ClockInAt;
                await LoadDataAsync();
            }
            else
            {
                var queueItem = new SyncQueueItem
                {
                    EntityType = "AttendanceLog",
                    EntityId = Guid.NewGuid().ToString(),
                    OperationType = "ClockIn",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);
                IsClockedIn = true;
                ClockInTime = DateTimeOffset.UtcNow;
            }
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
        IsBusy = true;
        ClearError();

        try
        {
            var request = new AttendanceClockRequest
            {
                ClientTimeUtc = DateTimeOffset.UtcNow
            };

            if (connectivity.IsConnected)
            {
                await attendanceApi.ClockOutAsync(request);
                IsClockedIn = false;
                ClockInTime = null;
                await LoadDataAsync();
            }
            else
            {
                var queueItem = new SyncQueueItem
                {
                    EntityType = "AttendanceLog",
                    EntityId = Guid.NewGuid().ToString(),
                    OperationType = "ClockOut",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);
                IsClockedIn = false;
                ClockInTime = null;
            }
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
}
