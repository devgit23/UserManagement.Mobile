namespace UserManagement.Mobile.Core.Offline.Sync;

public enum SyncStatus
{
    Synced,
    PendingUpload,
    PendingDelete,
    Conflict,
    Failed
}
