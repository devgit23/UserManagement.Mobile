using UserManagement.Mobile.Core.Offline.Database.Entities;

namespace UserManagement.Mobile.Core.Offline.Sync;

/// <summary>
/// Default conflict resolver using server-wins strategy.
/// Conflicting local changes are marked with SyncStatus.Conflict for user review.
/// </summary>
public static class ConflictResolver
{
    public static T ResolveServerWins<T>(T localEntity, T serverEntity) where T : LocalEntityBase
    {
        // Server wins: use server data, mark as synced
        serverEntity.SyncStatus = SyncStatus.Synced;
        serverEntity.LastSyncedUtc = DateTimeOffset.UtcNow;
        return serverEntity;
    }

    public static T MarkConflict<T>(T localEntity) where T : LocalEntityBase
    {
        localEntity.SyncStatus = SyncStatus.Conflict;
        return localEntity;
    }
}
