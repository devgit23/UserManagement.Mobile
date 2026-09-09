using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Biometric;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class AdminBiometricViewModel(IBiometricApi biometricApi) : ViewModelBase
{
    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _faceEnrolled;
    [ObservableProperty] private int _notEnrolled;
    [ObservableProperty] private int _todayFaceClockIns;
    [ObservableProperty] private int _todayManualClockIns;
    [ObservableProperty] private double _enrollmentPercent;
    [ObservableProperty] private string _searchText = "";

    [ObservableProperty]
    private List<UserBiometricSummary> _allUsers = [];

    [ObservableProperty]
    private List<UserBiometricSummary> _filteredUsers = [];

    public override async Task InitializeAsync()
    {
        Title = "Biometric Management";
        await LoadDataAsync();
    }

    protected override Task OnRefreshAsync() => LoadDataAsync();

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ClearError();

        try
        {
            // Load stats and user list in parallel
            var statsTask = biometricApi.GetAdminStatsAsync();
            var usersTask = biometricApi.GetAllUsersStatusAsync();

            await Task.WhenAll(statsTask, usersTask);

            var stats = statsTask.Result;
            TotalEmployees = stats.TotalEmployees;
            FaceEnrolled = stats.FaceEnrolled;
            NotEnrolled = stats.NotEnrolled;
            TodayFaceClockIns = stats.TodayFaceClockIns;
            TodayManualClockIns = stats.TodayManualClockIns;
            EnrollmentPercent = TotalEmployees > 0
                ? (double)(TotalEmployees - NotEnrolled) / TotalEmployees
                : 0;

            AllUsers = usersTask.Result.ToList();
            ApplyFilter();
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

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredUsers = AllUsers;
            return;
        }

        var q = SearchText.Trim();
        FilteredUsers = AllUsers
            .Where(u =>
                (u.UserDisplayName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Department?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    [RelayCommand]
    private async Task ResetUserEnrollmentAsync(Guid userId)
    {
        IsBusy = true;
        try
        {
            await biometricApi.ResetEnrollmentAsync(userId);
            await LoadDataAsync(); // Refresh list
        }
        catch (Exception ex)
        {
            SetError($"Reset failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
