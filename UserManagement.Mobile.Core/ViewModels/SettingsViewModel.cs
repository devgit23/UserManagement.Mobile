using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Helpers;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class SettingsViewModel(
    IAuthenticationService authService,
    ISyncService syncService,
    IConnectivityService connectivity,
    ISessionService sessionService) : ViewModelBase
{
    [ObservableProperty]
    private bool _isOnline;

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private bool _showBiometricSettings;

    [ObservableProperty]
    private bool _showBiometricAdmin;

    [ObservableProperty]
    private string? _lastSynced;

    [ObservableProperty]
    private int _pendingUploads;

    public override Task InitializeAsync()
    {
        Title = "Settings";
        IsOnline = connectivity.IsConnected;
        ShowBiometricSettings = PermissionHelper.CanEnrollBiometric(sessionService);
        ShowBiometricAdmin = PermissionHelper.CanManageBiometric(sessionService);
        UpdateSyncStatus();
        return Task.CompletedTask;
    }

    private void UpdateSyncStatus()
    {
        IsSyncing = syncService.IsSyncing;
        LastSynced = syncService.LastSyncedAt?.ToString("g") ?? "Never";
        PendingUploads = syncService.PendingUploadCount;
    }

    [RelayCommand]
    private async Task SyncNowAsync()
    {
        if (!connectivity.IsConnected)
        {
            SetError("No internet connection.");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            await syncService.SyncAsync();
            UpdateSyncStatus();
        }
        catch (Exception ex)
        {
            SetError($"Sync failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        try
        {
            await authService.LogoutAsync();
            await NavigateAsync("//Login");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
