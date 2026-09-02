using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Sync;

namespace UserManagement.Mobile.Core.Offline.Repositories;

public interface ILocalTimesheetRepository
{
    Task<List<LocalTimesheetEntry>> GetAllAsync();
    Task<LocalTimesheetEntry?> GetByIdAsync(string id);
    Task<List<LocalTimesheetEntry>> GetByDateRangeAsync(string userId, string fromDate, string toDate);
    Task<List<LocalTimesheetEntry>> GetBySyncStatusAsync(SyncStatus status);
    Task<LocalTimesheetEntry?> GetRunningTimerAsync(string userId);
    Task UpsertAsync(LocalTimesheetEntry entity);
    Task DeleteAsync(string id);
}

public sealed class LocalTimesheetRepository(Database.LocalDatabase db) : ILocalTimesheetRepository
{
    public async Task<List<LocalTimesheetEntry>> GetAllAsync() =>
        await db.GetConnection().Table<LocalTimesheetEntry>().OrderByDescending(x => x.WorkDate).ToListAsync();

    public async Task<LocalTimesheetEntry?> GetByIdAsync(string id) =>
        await db.GetConnection().Table<LocalTimesheetEntry>().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<LocalTimesheetEntry>> GetByDateRangeAsync(string userId, string fromDate, string toDate) =>
        await db.GetConnection().Table<LocalTimesheetEntry>()
            .Where(x => x.UserId == userId
                && string.Compare(x.WorkDate, fromDate, StringComparison.Ordinal) >= 0
                && string.Compare(x.WorkDate, toDate, StringComparison.Ordinal) <= 0)
            .OrderBy(x => x.WorkDate)
            .ToListAsync();

    public async Task<List<LocalTimesheetEntry>> GetBySyncStatusAsync(SyncStatus status) =>
        await db.GetConnection().Table<LocalTimesheetEntry>().Where(x => x.SyncStatusValue == (int)status).ToListAsync();

    public async Task<LocalTimesheetEntry?> GetRunningTimerAsync(string userId) =>
        await db.GetConnection().Table<LocalTimesheetEntry>()
            .Where(x => x.UserId == userId && x.TimerStartedAt != null && x.TimerStoppedAt == null)
            .FirstOrDefaultAsync();

    public async Task UpsertAsync(LocalTimesheetEntry entity) =>
        await db.GetConnection().InsertOrReplaceAsync(entity);

    public async Task DeleteAsync(string id) =>
        await db.GetConnection().DeleteAsync<LocalTimesheetEntry>(id);
}
