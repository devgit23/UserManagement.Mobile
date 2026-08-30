using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;
using UserManagement.Shared.Models;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class EmployeeDirectoryViewModel(
    IUsersApi usersApi,
    ILocalEmployeeRepository localRepo,
    IConnectivityService connectivity) : ViewModelBase
{
    [ObservableProperty]
    private string? _searchQuery;

    public ObservableCollection<EmployeeListItem> Employees { get; } = [];

    public override async Task InitializeAsync()
    {
        Title = "Employee Directory";
        await LoadDataAsync();
    }

    protected override async Task OnRefreshAsync() => await LoadDataAsync();

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ClearError();

        try
        {
            if (connectivity.IsConnected)
            {
                var paged = await usersApi.GetUsersAsync(1, 50, SearchQuery);
                if (paged?.Items is { } users)
                {
                    Employees.Clear();
                    foreach (var user in users)
                    {
                        Employees.Add(new EmployeeListItem
                        {
                            Id = user.Id.ToString(),
                            FullName = user.DisplayName ?? $"{user.FirstName} {user.LastName}".Trim(),
                            Email = user.Email,
                            Department = user.Department,
                            Designation = user.Designation
                        });
                    }
                }
            }
            else
            {
                var locals = await localRepo.SearchAsync(SearchQuery, null);
                Employees.Clear();
                foreach (var emp in locals)
                {
                    Employees.Add(new EmployeeListItem
                    {
                        Id = emp.Id,
                        FullName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        Email = emp.Email,
                        Department = emp.Department,
                        Designation = emp.Designation,
                        PhotoUrl = emp.PhotoUrl,
                        EmployeeCode = emp.EmployeeCode
                    });
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load employees: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class EmployeeListItem
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? PhotoUrl { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
}
