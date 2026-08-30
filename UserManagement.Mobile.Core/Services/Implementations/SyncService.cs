using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Core.Services.Implementations;

public sealed class SyncService(
    ISyncEngine syncEngine,
    IConnectivityService connectivity) : ISyncService, IDisposable
{
    private CancellationTokenSource? _periodicCts;
    private bool _isSyncing;

    public bool IsSyncing => _isSyncing;
    public DateTimeOffset? LastSyncedAt { get; private set; }
    public int PendingUploadCount { get; private set; }

    public event EventHandler? SyncStatusChanged;

    public async Task SyncAsync(CancellationToken ct = default)
    {
        if (_isSyncing || !connectivity.IsConnected) return;

        try
        {
            _isSyncing = true;
            SyncStatusChanged?.Invoke(this, EventArgs.Empty);

            // Push local changes first
            await syncEngine.PushAsync(ct);

            // Then pull server updates
            await syncEngine.PullAsync(ct);

            LastSyncedAt = DateTimeOffset.UtcNow;
            PendingUploadCount = await syncEngine.GetPendingCountAsync();
        }
        finally
        {
            _isSyncing = false;
            SyncStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task StartPeriodicSyncAsync(TimeSpan interval, CancellationToken ct = default)
    {
        StopPeriodicSync();
        _periodicCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Initial sync
        await SyncAsync(_periodicCts.Token);

        // Periodic sync loop
        _ = Task.Run(async () =>
        {
            while (!_periodicCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, _periodicCts.Token);
                    await SyncAsync(_periodicCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Log and continue
                }
            }
        }, _periodicCts.Token);
    }

    public void StopPeriodicSync()
    {
        _periodicCts?.Cancel();
        _periodicCts?.Dispose();
        _periodicCts = null;
    }

    public void Dispose()
    {
        StopPeriodicSync();
    }
}
