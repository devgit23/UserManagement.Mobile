using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.EmployeeProfiles;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class ProfileViewModel(
    IEmployeesApi employeesApi,
    ISessionService sessionService) : ViewModelBase
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string? _photoUrl;

    [ObservableProperty]
    private string? _department;

    [ObservableProperty]
    private string? _designation;

    [ObservableProperty]
    private string? _employeeCode;

    [ObservableProperty]
    private string? _contactNumber;

    [ObservableProperty]
    private string? _dateOfJoining;

    public override async Task InitializeAsync()
    {
        Title = "My Profile";
        await LoadProfileAsync();
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        var user = sessionService.CurrentUser;
        if (user is null) return;

        IsBusy = true;
        ClearError();

        try
        {
            var profile = await employeesApi.GetProfileAsync(user.Id);
            if (profile is not null)
            {
                DisplayName = $"{profile.FirstName} {profile.LastName}".Trim();
                Email = profile.Email;
                PhotoUrl = profile.PhotoUrl;
                Department = profile.Department;
                Designation = profile.Designation;
                EmployeeCode = profile.EmployeeCode;
                ContactNumber = profile.Personal?.ContactNumber;
                DateOfJoining = profile.Employment?.DateOfJoining?.ToString("d MMM yyyy");
            }
            else
            {
                // Fallback to basic user info
                DisplayName = user.UserName;
                Email = user.Email;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load profile: {ex.Message}");
            DisplayName = user.UserName;
            Email = user.Email;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
