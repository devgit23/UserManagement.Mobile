namespace UserManagement.Mobile.Core.Offline.Sync;

public interface ISyncEngine
{
    Task PushAsync(CancellationToken ct = default);
    Task PullAsync(CancellationToken ct = default);
    Task<int> GetPendingCountAsync();
}
