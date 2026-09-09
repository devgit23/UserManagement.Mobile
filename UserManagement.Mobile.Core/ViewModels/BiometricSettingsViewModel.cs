using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class BiometricSettingsViewModel(
    IBiometricApi biometricApi,
    IConnectivityService connectivityService,
    LocalDatabase localDatabase) : ViewModelBase
{
    [ObservableProperty] private bool _faceEnrolled;
    [ObservableProperty] private string? _faceEnrolledDate;
    [ObservableProperty] private decimal? _faceQualityScore;

    public override async Task InitializeAsync()
    {
        Title = "Biometric Settings";
        IsBusy = true;
        ClearError();

        try
        {
            if (connectivityService.IsConnected)
            {
                var status = await biometricApi.GetEnrollmentStatusAsync();
                FaceEnrolled = status.FaceEnrolled;
                FaceQualityScore = status.FaceQualityScore;
                FaceEnrolledDate = status.FaceEnrolledAt?.ToLocalTime().ToString("dd MMM yyyy");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReEnrollFaceAsync()
    {
        await NavigateAsync("FaceEnrollmentPage");
    }

    [RelayCommand]
    private async Task RemoveFaceAsync()
    {
        IsBusy = true;
        try
        {
            var conn = localDatabase.GetConnection();
            await conn.DeleteAllAsync<Offline.Database.Entities.LocalFaceEmbedding>();

            FaceEnrolled = false;
            FaceEnrolledDate = null;
            FaceQualityScore = null;
        }
        catch (Exception ex)
        {
            SetError($"Failed to remove: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
