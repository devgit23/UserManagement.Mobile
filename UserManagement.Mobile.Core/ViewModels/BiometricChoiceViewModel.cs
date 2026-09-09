using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class BiometricChoiceViewModel(
    IBiometricApi biometricApi,
    ISessionService sessionService,
    IConnectivityService connectivityService,
    LocalDatabase localDatabase) : ViewModelBase
{
    [ObservableProperty] private bool _faceEnrolled;
    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private string _mode = "clockin"; // "clockin" or "clockout"
    [ObservableProperty] private string _statusText = "Not Clocked In";

    public override async Task InitializeAsync()
    {
        Title = "Mark Attendance";
        IsBusy = true;
        ClearError();
        IsOffline = !connectivityService.IsConnected;

        try
        {
            if (connectivityService.IsConnected)
            {
                var status = await biometricApi.GetEnrollmentStatusAsync();
                FaceEnrolled = status.FaceEnrolled;
            }
            else
            {
                var conn = localDatabase.GetConnection();
                var localEmbedding = await conn.Table<LocalFaceEmbedding>()
                    .Where(e => e.UserId == "current")
                    .FirstOrDefaultAsync();

                FaceEnrolled = localEmbedding is not null;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load biometric status: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var mode))
            Mode = mode?.ToString() ?? "clockin";
        if (query.TryGetValue("status", out var status))
            StatusText = status?.ToString() ?? "Not Clocked In";
    }

    [RelayCommand]
    private async Task SelectFaceAsync()
    {
        if (!FaceEnrolled)
        {
            if (IsOffline)
            {
                SetError("Face enrollment requires an internet connection.");
                return;
            }
            await NavigateAsync("FaceEnrollmentPage");
            return;
        }

        await NavigateAsync($"FaceCapturePage?mode={Mode}");
    }

    [RelayCommand]
    private async Task SetUpFaceAsync()
    {
        if (IsOffline)
        {
            SetError("Face enrollment requires an internet connection.");
            return;
        }
        await NavigateAsync("FaceEnrollmentPage");
    }
}
