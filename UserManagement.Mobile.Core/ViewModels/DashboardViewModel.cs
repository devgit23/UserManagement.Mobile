using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Attendance;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Helpers;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class DashboardViewModel(
    ISessionService sessionService,
    IWorkforceApi workforceApi,
    ILeaveApi leaveApi,
    IAttendanceApi attendanceApi,
    INotificationsApi notificationsApi,
    ISyncService syncService,
    IConnectivityService connectivity) : ViewModelBase
{
    [ObservableProperty]
    private string _welcomeMessage = "Welcome";

    [ObservableProperty]
    private int _pendingLeaveRequests;

    [ObservableProperty]
    private int _unreadNotifications;

    [ObservableProperty]
    private bool _isOnline = true;

    [ObservableProperty]
    private bool _showAttendance;

    [ObservableProperty]
    private bool _showLeave;

    [ObservableProperty]
    private bool _showNotifications;

    // Upcoming holidays
    [ObservableProperty]
    private bool _hasUpcomingHolidays;

    public ObservableCollection<HolidayModel> UpcomingHolidays { get; } = [];

    // Leave balances
    [ObservableProperty]
    private bool _hasLeaveBalances;

    [ObservableProperty]
    private string _totalAvailableLeave = "0";

    [ObservableProperty]
    private string _totalUsedLeave = "0";

    [ObservableProperty]
    private string _totalPendingLeave = "0";

    [ObservableProperty]
    private bool _hasPendingLeave;

    public ObservableCollection<LeaveBalanceDisplayItem> LeaveBalances { get; } = [];

    // Attendance summary
    [ObservableProperty]
    private bool _hasAttendanceStats;

    [ObservableProperty]
    private int _daysPresent;

    [ObservableProperty]
    private int _daysAbsent;

    [ObservableProperty]
    private string _onTimePercent = "0";

    [ObservableProperty]
    private string _avgHours = "0";

    [ObservableProperty]
    private string _attendancePeriod = string.Empty;

    // Existing stats
    [ObservableProperty]
    private int _presentToday;

    [ObservableProperty]
    private int _onLeaveToday;

    [ObservableProperty]
    private int _totalEmployees;

    public override async Task InitializeAsync()
    {
        Title = "Dashboard";
        UpdatePermissions();
        await LoadDataAsync();
    }

    protected override async Task OnRefreshAsync()
    {
        await LoadDataAsync();
    }

    private void UpdatePermissions()
    {
        ShowAttendance = PermissionHelper.CanViewAttendance(sessionService);
        ShowLeave = PermissionHelper.CanViewLeave(sessionService);
        ShowNotifications = PermissionHelper.CanViewNotifications(sessionService);

        var user = sessionService.CurrentUser;
        if (user is not null)
        {
            WelcomeMessage = $"Welcome, {user.UserName}";
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsOnline = connectivity.IsConnected;

        if (!connectivity.IsConnected)
        {
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var userId = sessionService.CurrentUser?.Id;

            // Fire all API calls in parallel
            var leaveTask = (ShowLeave && userId.HasValue)
                ? SafeAsync(() => leaveApi.GetDashboardAsync(userId.Value))
                : Task.FromResult<LeaveDashboardModel?>(null);

            var attendanceTask = ShowAttendance
                ? SafeAsync(() => attendanceApi.GetStatsAsync("month"))
                : Task.FromResult<AttendanceStatsResponse?>(null);

            var notificationsTask = ShowNotifications
                ? SafeListAsync(() => notificationsApi.GetNotificationsAsync(includeRead: false))
                : Task.FromResult<IReadOnlyList<WorkforceNotificationModel>?>(null);

            await Task.WhenAll(leaveTask, attendanceTask, notificationsTask);

            // Leave dashboard (balances + holidays + pending count)
            var leaveDashboard = await leaveTask;
            if (leaveDashboard is not null)
            {
                PendingLeaveRequests = leaveDashboard.Requests
                    .Count(r => r.Status == LeaveRequestStatus.Submitted);

                // Leave balances
                LeaveBalances.Clear();
                var colorPalette = new[] { "Primary", "Success", "Info", "Warning", "Danger" };
                for (var i = 0; i < leaveDashboard.Balances.Count; i++)
                {
                    LeaveBalances.Add(new LeaveBalanceDisplayItem(
                        leaveDashboard.Balances[i],
                        colorPalette[i % colorPalette.Length]));
                }
                HasLeaveBalances = LeaveBalances.Count > 0;
                TotalAvailableLeave = leaveDashboard.Balances
                    .Sum(b => Math.Max(0, b.BalanceDays)).ToString("0.#");
                TotalUsedLeave = leaveDashboard.Balances
                    .Sum(b => b.UsedDays).ToString("0.#");
                var pending = leaveDashboard.Balances.Sum(b => b.PendingDays);
                TotalPendingLeave = pending.ToString("0.#");
                HasPendingLeave = pending > 0;

                // Upcoming holidays
                UpcomingHolidays.Clear();
                var upcoming = leaveDashboard.UpcomingHolidays
                    .OrderBy(h => h.Date)
                    .Take(5);
                foreach (var holiday in upcoming)
                {
                    UpcomingHolidays.Add(holiday);
                }
                HasUpcomingHolidays = UpcomingHolidays.Count > 0;
            }

            // Attendance stats
            var stats = await attendanceTask;
            if (stats?.Personal is { } personal)
            {
                DaysPresent = personal.DaysPresent;
                DaysAbsent = Math.Max(0, personal.DaysCompleted - personal.DaysPresent);
                OnTimePercent = personal.OnTimePercent.ToString("0.#");
                AvgHours = personal.AvgEffectiveHours.ToString("0.#");
                AttendancePeriod = $"{personal.From:dd MMM} - {personal.To:dd MMM yyyy}";
                HasAttendanceStats = true;
            }

            // Notifications
            var notifications = await notificationsTask;
            UnreadNotifications = notifications?.Count ?? 0;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load dashboard: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<T?> SafeAsync<T>(Func<Task<T>> call) where T : class
    {
        try { return await call(); }
        catch { return null; }
    }

    private static async Task<IReadOnlyList<T>?> SafeListAsync<T>(Func<Task<IReadOnlyList<T>>> call)
    {
        try { return await call(); }
        catch { return null; }
    }
}

/// <summary>Display wrapper for leave balances with pre-computed UI properties.</summary>
public sealed class LeaveBalanceDisplayItem
{
    public LeaveBalanceDisplayItem(LeaveBalanceModel balance, string colorKey)
    {
        LeaveCode = balance.LeaveCode;
        BalanceDays = Math.Max(0, balance.BalanceDays);
        UsedDays = balance.UsedDays;
        PendingDays = balance.PendingDays;
        ColorKey = colorKey;

        var totalPool = balance.OpeningBalanceDays + balance.CarriedForwardDays
                        + balance.AdjustmentDays + balance.AccruedDays;
        TotalEntitlement = totalPool > 0 ? totalPool : BalanceDays + UsedDays + PendingDays;
        UsageProgress = TotalEntitlement > 0
            ? Math.Min(1.0, (double)UsedDays / (double)TotalEntitlement)
            : 0;

        BalanceText = BalanceDays.ToString("0.#");
        UsedText = UsedDays.ToString("0.#");
        PendingText = PendingDays.ToString("0.#");
        EntitlementText = TotalEntitlement.ToString("0.#");
        HasPending = PendingDays > 0;
    }

    public string LeaveCode { get; }
    public decimal BalanceDays { get; }
    public decimal UsedDays { get; }
    public decimal PendingDays { get; }
    public decimal TotalEntitlement { get; }
    public double UsageProgress { get; }
    public string ColorKey { get; }

    public string BalanceText { get; }
    public string UsedText { get; }
    public string PendingText { get; }
    public string EntitlementText { get; }
    public bool HasPending { get; }
}
