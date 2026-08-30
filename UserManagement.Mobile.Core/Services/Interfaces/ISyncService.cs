namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface ISyncService
{
    bool IsSyncing { get; }
    DateTimeOffset? LastSyncedAt { get; }
    int PendingUploadCount { get; }

    Task SyncAsync(CancellationToken ct = default);
    Task StartPeriodicSyncAsync(TimeSpan interval, CancellationToken ct = default);
    void StopPeriodicSync();

    event EventHandler? SyncStatusChanged;
}
