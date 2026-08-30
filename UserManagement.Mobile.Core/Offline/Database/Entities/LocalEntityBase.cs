using SQLite;
using UserManagement.Mobile.Core.Offline.Sync;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

public abstract class LocalEntityBase
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public int SyncStatusValue { get; set; } = (int)SyncStatus.Synced;
    public DateTimeOffset LastModifiedUtc { get; set; }
    public DateTimeOffset? LastSyncedUtc { get; set; }

    [Ignore]
    public SyncStatus SyncStatus
    {
        get => (SyncStatus)SyncStatusValue;
        set => SyncStatusValue = (int)value;
    }
}
