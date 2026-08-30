using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class NotificationsViewModel(
    INotificationsApi notificationsApi,
    IConnectivityService connectivity) : ViewModelBase
{
    [ObservableProperty]
    private int _unreadCount;

    public ObservableCollection<WorkforceNotificationModel> Notifications { get; } = [];

    public override async Task InitializeAsync()
    {
        Title = "Notifications";
        await LoadDataAsync();
    }

    protected override async Task OnRefreshAsync() => await LoadDataAsync();

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (!connectivity.IsConnected) return;

        IsBusy = true;
        ClearError();

        try
        {
            var notifications = await notificationsApi.GetNotificationsAsync();
            if (notifications is not null)
            {
                Notifications.Clear();
                foreach (var notification in notifications)
                    Notifications.Add(notification);

                UnreadCount = notifications.Count(n => !n.IsRead);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load notifications: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(WorkforceNotificationModel notification)
    {
        if (!connectivity.IsConnected) return;

        try
        {
            await notificationsApi.MarkAsReadAsync(notification.Id);
            notification.IsRead = true;
            UnreadCount = Math.Max(0, UnreadCount - 1);
        }
        catch { /* Non-critical */ }
    }

    [RelayCommand]
    private async Task MarkAllAsReadAsync()
    {
        if (!connectivity.IsConnected) return;

        try
        {
            await notificationsApi.MarkAllAsReadAsync();
            foreach (var notification in Notifications)
                notification.IsRead = true;
            UnreadCount = 0;
        }
        catch { /* Non-critical */ }
    }
}
