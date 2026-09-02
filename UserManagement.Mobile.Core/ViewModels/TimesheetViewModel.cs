using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Timesheet;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class TimesheetViewModel(
    ITimesheetApi timesheetApi,
    ILocalTimesheetRepository localRepo,
    IConnectivityService connectivity,
    ISessionService sessionService,
    LocalDatabase db) : ViewModelBase
{
    // ── Week Navigation ──
    [ObservableProperty]
    private DateOnly _weekStart;

    [ObservableProperty]
    private DateOnly _weekEnd;

    [ObservableProperty]
    private string _weekLabel = string.Empty;

    // ── Grid Data ──
    [ObservableProperty]
    private TimesheetStatus _weekStatus = TimesheetStatus.Draft;

    [ObservableProperty]
    private Guid? _weekId;

    [ObservableProperty]
    private decimal _totalHours;

    [ObservableProperty]
    private decimal _billableHours;

    [ObservableProperty]
    private bool _isOnline = true;

    // ── Timer ──
    [ObservableProperty]
    private bool _isTimerRunning;

    [ObservableProperty]
    private string _timerDuration = "00:00";

    [ObservableProperty]
    private string? _timerProjectName;

    [ObservableProperty]
    private Guid? _runningEntryId;

    private DateTimeOffset? _timerStartedAt;
    private CancellationTokenSource? _timerCts;

    // ── Dashboard ──
    [ObservableProperty]
    private decimal _todayHours;

    [ObservableProperty]
    private decimal _weekHours;

    [ObservableProperty]
    private decimal _utilizationPercent;

    // ── Entry Editing ──
    [ObservableProperty]
    private bool _showEntryEditor;

    [ObservableProperty]
    private decimal _editHours;

    [ObservableProperty]
    private int _editMinutes;

    [ObservableProperty]
    private string? _editDescription;

    [ObservableProperty]
    private bool _editIsBillable = true;

    private Guid? _editEntryId;
    private Guid _editProjectId;
    private Guid? _editTaskId;
    private DateOnly _editDate;

    public ObservableCollection<TimesheetGridRow> Rows { get; } = [];
    public ObservableCollection<TimesheetProjectModel> Projects { get; } = [];
    public ObservableCollection<DailyHoursSummary> DailyBreakdown { get; } = [];

    public bool IsEditable => WeekStatus is TimesheetStatus.Draft or TimesheetStatus.Rejected;

    partial void OnWeekStartChanged(DateOnly value)
    {
        WeekEnd = value.AddDays(6);
        WeekLabel = $"{value:MMM dd} – {WeekEnd:MMM dd, yyyy}";
    }

    public override async Task InitializeAsync()
    {
        Title = "Timesheet";
        SetCurrentWeek();
        await LoadDataAsync();
    }

    protected override async Task OnRefreshAsync() => await LoadDataAsync();

    private void SetCurrentWeek()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var dayOfWeek = today.DayOfWeek;
        var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
        WeekStart = today.AddDays(-daysToMonday);
    }

    [RelayCommand]
    private async Task NavigateWeekAsync(string direction)
    {
        WeekStart = direction == "next" ? WeekStart.AddDays(7) : WeekStart.AddDays(-7);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToCurrentWeekAsync()
    {
        SetCurrentWeek();
        await LoadDataAsync();
    }

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
                await LoadOnlineDataAsync();
            }
            else
            {
                await LoadOfflineDataAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load timesheet: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadOnlineDataAsync()
    {
        // Load grid + dashboard in parallel — userId omitted so the server defaults to the JWT user
        var gridTask = timesheetApi.GetWeekGridAsync(
            weekStart: WeekStart.ToString("yyyy-MM-dd"));
        var dashTask = timesheetApi.GetDashboardAsync();
        var projectsTask = timesheetApi.GetProjectsAsync();

        await Task.WhenAll(gridTask, dashTask, projectsTask);

        var grid = await gridTask;
        var dash = await dashTask;
        var projects = await projectsTask;

        // Grid
        Rows.Clear();
        if (grid is not null)
        {
            WeekId = grid.WeekId;
            WeekStatus = grid.Status;
            TotalHours = grid.TotalHours;
            BillableHours = grid.BillableHours;
            foreach (var row in grid.Rows)
                Rows.Add(row);
        }
        OnPropertyChanged(nameof(IsEditable));

        // Dashboard
        if (dash is not null)
        {
            TodayHours = dash.TodayHours;
            WeekHours = dash.WeekHours;
            UtilizationPercent = dash.UtilizationPercent;

            DailyBreakdown.Clear();
            foreach (var d in dash.WeekDailyBreakdown)
                DailyBreakdown.Add(d);

            // Timer state
            if (dash.RunningTimer is not null)
            {
                IsTimerRunning = true;
                _runningEntryId = dash.RunningTimer.Id;
                _timerStartedAt = dash.RunningTimer.TimerStartedAt;
                TimerProjectName = dash.RunningTimer.ProjectName;
                StartTimerTick();
            }
            else
            {
                StopTimerTick();
                IsTimerRunning = false;
                TimerDuration = "00:00";
                TimerProjectName = null;
            }
        }

        // Projects
        Projects.Clear();
        if (projects is not null)
        {
            foreach (var p in projects)
                Projects.Add(p);
        }
    }

    private async Task LoadOfflineDataAsync()
    {
        var userId = sessionService.CurrentUser?.Id.ToString() ?? string.Empty;
        var fromDate = WeekStart.ToString("yyyy-MM-dd");
        var toDate = WeekEnd.ToString("yyyy-MM-dd");

        var localEntries = await localRepo.GetByDateRangeAsync(userId, fromDate, toDate);

        // Group into rows by ProjectId + TaskId
        Rows.Clear();
        var grouped = localEntries.GroupBy(e => (e.ProjectId, e.TaskId));
        foreach (var group in grouped)
        {
            var first = group.First();
            var row = new TimesheetGridRow
            {
                ProjectId = Guid.Parse(first.ProjectId),
                TaskId = first.TaskId is not null ? Guid.Parse(first.TaskId) : null,
                ProjectName = first.ProjectName ?? string.Empty,
                ProjectCode = first.ProjectCode ?? string.Empty,
                ProjectColorHex = first.ProjectColorHex,
                TaskName = first.TaskName,
                IsBillable = first.IsBillable,
            };

            foreach (var entry in group)
            {
                var entryDate = DateOnly.Parse(entry.WorkDate);
                var dayIndex = (int)(entryDate.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)entryDate.DayOfWeek - 1);
                if (dayIndex >= 0 && dayIndex < 7)
                {
                    row.Days[dayIndex] = new TimesheetGridCell
                    {
                        EntryId = Guid.Parse(entry.Id),
                        Hours = entry.Hours,
                        Minutes = entry.Minutes,
                        Description = entry.Description
                    };
                }
            }

            Rows.Add(row);
        }

        TotalHours = localEntries.Sum(e => e.Hours + e.Minutes / 60m);
        WeekStatus = TimesheetStatus.Draft;
        OnPropertyChanged(nameof(IsEditable));
    }

    // ── Cell Editing ──

    [RelayCommand]
    private void OpenCellEditor(CellEditorArgs args)
    {
        if (!IsEditable) return;

        _editEntryId = args.Cell?.EntryId;
        _editProjectId = args.Row.ProjectId;
        _editTaskId = args.Row.TaskId;
        _editDate = WeekStart.AddDays(args.DayIndex);

        EditHours = args.Cell?.Hours ?? 0;
        EditMinutes = args.Cell?.Minutes ?? 0;
        EditDescription = args.Cell?.Description;
        EditIsBillable = args.Row.IsBillable;
        ShowEntryEditor = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ShowEntryEditor = false;
    }

    [RelayCommand]
    private async Task SaveEntryAsync()
    {
        ShowEntryEditor = false;
        IsBusy = true;

        try
        {
            var editModel = new TimesheetEntryEditModel
            {
                Id = _editEntryId,
                ProjectId = _editProjectId,
                TaskId = _editTaskId,
                WorkDate = _editDate,
                Hours = EditHours,
                Minutes = EditMinutes,
                Description = EditDescription,
                IsBillable = EditIsBillable
            };

            if (connectivity.IsConnected)
            {
                await timesheetApi.SaveEntryAsync(editModel);
            }
            else
            {
                // Queue for sync
                var queueItem = new SyncQueueItem
                {
                    EntityType = "TimesheetEntry",
                    EntityId = _editEntryId?.ToString() ?? Guid.NewGuid().ToString(),
                    OperationType = _editEntryId.HasValue ? "Update" : "Create",
                    SerializedPayload = JsonSerializer.Serialize(editModel),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save entry: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Timer ──

    [RelayCommand]
    private async Task StartTimerAsync(TimesheetProjectModel project)
    {
        IsBusy = true;
        ClearError();

        try
        {
            var request = new TimerStartRequest
            {
                ProjectId = project.Id,
                IsBillable = project.IsBillable
            };

            if (connectivity.IsConnected)
            {
                var entry = await timesheetApi.StartTimerAsync(request);
                RunningEntryId = entry.Id;
                _timerStartedAt = entry.TimerStartedAt;
                TimerProjectName = entry.ProjectName;
                IsTimerRunning = true;
                StartTimerTick();
            }
            else
            {
                var queueItem = new SyncQueueItem
                {
                    EntityType = "TimesheetTimer",
                    EntityId = Guid.NewGuid().ToString(),
                    OperationType = "Start",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);

                _timerStartedAt = DateTimeOffset.UtcNow;
                TimerProjectName = project.Name;
                IsTimerRunning = true;
                StartTimerTick();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to start timer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopTimerAsync()
    {
        if (!IsTimerRunning) return;
        IsBusy = true;
        ClearError();

        try
        {
            if (connectivity.IsConnected && RunningEntryId.HasValue)
            {
                var request = new TimerStopRequest { EntryId = RunningEntryId.Value };
                await timesheetApi.StopTimerAsync(request);
            }
            else if (RunningEntryId.HasValue)
            {
                var request = new TimerStopRequest { EntryId = RunningEntryId.Value };
                var queueItem = new SyncQueueItem
                {
                    EntityType = "TimesheetTimer",
                    EntityId = RunningEntryId.Value.ToString(),
                    OperationType = "Stop",
                    SerializedPayload = JsonSerializer.Serialize(request),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);
            }

            StopTimerTick();
            IsTimerRunning = false;
            RunningEntryId = null;
            TimerDuration = "00:00";
            TimerProjectName = null;

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to stop timer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StartTimerTick()
    {
        StopTimerTick();
        _timerCts = new CancellationTokenSource();
        var token = _timerCts.Token;

        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    UpdateTimerDisplay();
                }
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    private void StopTimerTick()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    private void UpdateTimerDisplay()
    {
        if (!_timerStartedAt.HasValue) return;
        var elapsed = DateTimeOffset.UtcNow - _timerStartedAt.Value;
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        TimerDuration = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    // ── Week Submit ──

    [RelayCommand]
    private async Task SubmitWeekAsync()
    {
        if (WeekId is null || !IsEditable) return;
        IsBusy = true;
        ClearError();

        try
        {
            if (connectivity.IsConnected)
            {
                await timesheetApi.SubmitWeekAsync(WeekId.Value, new TimesheetSubmitRequest());
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit timesheet: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RecallWeekAsync()
    {
        if (WeekId is null || WeekStatus != TimesheetStatus.Submitted) return;
        IsBusy = true;
        ClearError();

        try
        {
            if (connectivity.IsConnected)
            {
                await timesheetApi.RecallWeekAsync(WeekId.Value);
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to recall timesheet: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Helper record for cell editor ──
    public sealed record CellEditorArgs(TimesheetGridRow Row, int DayIndex, TimesheetGridCell? Cell);
}
