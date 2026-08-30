using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class LeaveListViewModel(
    ILeaveApi leaveApi,
    ILocalLeaveRepository localRepo,
    IConnectivityService connectivity,
    ISessionService sessionService) : ViewModelBase
{
    public ObservableCollection<LeaveRequestModel> Requests { get; } = [];
    public ObservableCollection<LeaveBalanceModel> Balances { get; } = [];

    public Func<string, Task>? NavigateAsync { get; set; }

    public override async Task InitializeAsync()
    {
        Title = "Leave";
        await LoadDataAsync();
    }

    protected override async Task OnRefreshAsync() => await LoadDataAsync();

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ClearError();

        try
        {
            var userId = sessionService.CurrentUser?.Id;

            if (connectivity.IsConnected && userId.HasValue)
            {
                var requests = await leaveApi.GetRequestsAsync(userId: userId.Value);
                if (requests is not null)
                {
                    Requests.Clear();
                    foreach (var request in requests)
                        Requests.Add(request);
                }

                var balances = await leaveApi.GetBalancesAsync(userId.Value);
                if (balances is not null)
                {
                    Balances.Clear();
                    foreach (var balance in balances)
                        Balances.Add(balance);
                }
            }
            else
            {
                var localRequests = await localRepo.GetAllAsync();
                Requests.Clear();
                foreach (var local in localRequests)
                {
                    Requests.Add(new LeaveRequestModel
                    {
                        Id = Guid.Parse(local.Id),
                        LeaveCode = local.LeaveCode,
                        StartDate = DateOnly.Parse(local.StartDate),
                        EndDate = DateOnly.Parse(local.EndDate),
                        RequestedDays = local.RequestedDays,
                        Reason = local.Reason,
                        Status = Enum.TryParse<LeaveRequestStatus>(local.Status, out var s) ? s : LeaveRequestStatus.Submitted,
                        RequestedOn = local.RequestedOn
                    });
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load leave data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToNewRequestAsync()
    {
        if (NavigateAsync is not null)
            await NavigateAsync("LeaveRequestPage");
    }
}
