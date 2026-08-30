using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Sync;

namespace UserManagement.Mobile.Core.Offline.Repositories;

public interface ILocalLeaveRepository
{
    Task<List<LocalLeaveRequest>> GetAllAsync();
    Task<LocalLeaveRequest?> GetByIdAsync(string id);
    Task<List<LocalLeaveRequest>> GetBySyncStatusAsync(SyncStatus status);
    Task UpsertAsync(LocalLeaveRequest entity);
    Task DeleteAsync(string id);
}

public sealed class LocalLeaveRepository(Database.LocalDatabase db) : ILocalLeaveRepository
{
    public async Task<List<LocalLeaveRequest>> GetAllAsync() =>
        await db.GetConnection().Table<LocalLeaveRequest>().OrderByDescending(x => x.RequestedOn).ToListAsync();

    public async Task<LocalLeaveRequest?> GetByIdAsync(string id) =>
        await db.GetConnection().Table<LocalLeaveRequest>().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<LocalLeaveRequest>> GetBySyncStatusAsync(SyncStatus status) =>
        await db.GetConnection().Table<LocalLeaveRequest>().Where(x => x.SyncStatusValue == (int)status).ToListAsync();

    public async Task UpsertAsync(LocalLeaveRequest entity) =>
        await db.GetConnection().InsertOrReplaceAsync(entity);

    public async Task DeleteAsync(string id) =>
        await db.GetConnection().DeleteAsync<LocalLeaveRequest>(id);
}
