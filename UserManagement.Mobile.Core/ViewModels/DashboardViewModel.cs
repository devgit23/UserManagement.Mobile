using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Helpers;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class DashboardViewModel(
    ISessionService sessionService,
    IWorkforceApi workforceApi,
    ILeaveApi leaveApi,
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

            // Load pending leave requests count
            if (ShowLeave && userId.HasValue)
            {
                try
                {
                    var requests = await leaveApi.GetRequestsAsync(userId: userId.Value);
                    PendingLeaveRequests = requests?.Count(r => r.Status == Common.Workforce.LeaveRequestStatus.Submitted) ?? 0;
                }
                catch { /* Non-critical */ }
            }

            // Load unread notifications count
            if (ShowNotifications)
            {
                try
                {
                    var notifications = await notificationsApi.GetNotificationsAsync(includeRead: false);
                    UnreadNotifications = notifications?.Count ?? 0;
                }
                catch { /* Non-critical */ }
            }
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
}
