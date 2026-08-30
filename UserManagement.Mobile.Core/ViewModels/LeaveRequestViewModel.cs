using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class LeaveRequestViewModel(
    ILeaveApi leaveApi,
    ILocalLeaveRepository localRepo,
    IConnectivityService connectivity,
    ISessionService sessionService,
    LocalDatabase db) : ViewModelBase
{
    [ObservableProperty]
    private string _selectedLeaveCode = string.Empty;

    [ObservableProperty]
    private LeavePolicyModel? _selectedLeaveType;

    partial void OnSelectedLeaveTypeChanged(LeavePolicyModel? value)
    {
        SelectedLeaveCode = value?.Code ?? string.Empty;
    }

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private string _startSession = "FullDay";

    [ObservableProperty]
    private string _endSession = "FullDay";

    [ObservableProperty]
    private string? _reason;

    public ObservableCollection<LeavePolicyModel> LeaveTypes { get; } = [];

    public Func<Task>? GoBackAsync { get; set; }

    public override async Task InitializeAsync()
    {
        Title = "New Leave Request";
        await LoadLeaveTypesAsync();
    }

    private async Task LoadLeaveTypesAsync()
    {
        if (!connectivity.IsConnected) return;

        try
        {
            var policies = await leaveApi.GetPoliciesAsync();
            if (policies is not null)
            {
                LeaveTypes.Clear();
                foreach (var policy in policies)
                    LeaveTypes.Add(policy);

                if (policies.Count > 0)
                    SelectedLeaveType = policies[0];
            }
        }
        catch { /* Non-critical */ }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedLeaveCode))
        {
            SetError("Please select a leave type.");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var draft = new LeaveRequestDraftModel
            {
                UserId = sessionService.CurrentUser?.Id ?? Guid.Empty,
                LeaveCode = SelectedLeaveCode,
                StartDate = DateOnly.FromDateTime(StartDate),
                EndDate = DateOnly.FromDateTime(EndDate),
                StartSession = Enum.TryParse<LeaveSessionType>(StartSession, out var ss) ? ss : LeaveSessionType.FullDay,
                EndSession = Enum.TryParse<LeaveSessionType>(EndSession, out var es) ? es : LeaveSessionType.FullDay,
                Reason = Reason
            };

            if (connectivity.IsConnected)
            {
                await leaveApi.CreateRequestAsync(draft);
                if (GoBackAsync is not null) await GoBackAsync();
            }
            else
            {
                var localId = Guid.NewGuid().ToString();
                var localLeave = new LocalLeaveRequest
                {
                    Id = localId,
                    UserId = draft.UserId.ToString(),
                    LeaveCode = draft.LeaveCode,
                    StartDate = draft.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = draft.EndDate.ToString("yyyy-MM-dd"),
                    StartSession = draft.StartSession.ToString(),
                    EndSession = draft.EndSession.ToString(),
                    Reason = draft.Reason,
                    Status = "Submitted",
                    RequestedOn = DateTimeOffset.UtcNow,
                    SyncStatus = Offline.Sync.SyncStatus.PendingUpload,
                    LastModifiedUtc = DateTimeOffset.UtcNow
                };
                await localRepo.UpsertAsync(localLeave);

                var queueItem = new SyncQueueItem
                {
                    EntityType = "LeaveRequest",
                    EntityId = localId,
                    OperationType = "Create",
                    SerializedPayload = JsonSerializer.Serialize(draft),
                    CreatedUtc = DateTimeOffset.UtcNow
                };
                await db.GetConnection().InsertAsync(queueItem);

                if (GoBackAsync is not null) await GoBackAsync();
            }
        }
        catch (ApiException apiEx)
        {
            var serverError = apiEx.Content;
            if (!string.IsNullOrEmpty(serverError))
            {
                // Try to extract a meaningful error message from the server response
                try
                {
                    using var doc = JsonDocument.Parse(serverError);
                    var msg = doc.RootElement.TryGetProperty("error", out var errProp) ? errProp.GetString()
                            : doc.RootElement.TryGetProperty("title", out var titleProp) ? titleProp.GetString()
                            : doc.RootElement.TryGetProperty("detail", out var detailProp) ? detailProp.GetString()
                            : serverError;
                    SetError($"Error: {msg}");
                }
                catch
                {
                    SetError($"Error: {serverError}");
                }
            }
            else
            {
                SetError($"Error: {apiEx.StatusCode} - {apiEx.Message}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
