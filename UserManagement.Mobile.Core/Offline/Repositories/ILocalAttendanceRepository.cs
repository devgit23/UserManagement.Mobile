using UserManagement.Mobile.Core.Offline.Database.Entities;
using UserManagement.Mobile.Core.Offline.Sync;

namespace UserManagement.Mobile.Core.Offline.Repositories;

public interface ILocalAttendanceRepository
{
    Task<List<LocalAttendanceLog>> GetAllAsync();
    Task<LocalAttendanceLog?> GetByIdAsync(string id);
    Task<List<LocalAttendanceLog>> GetBySyncStatusAsync(SyncStatus status);
    Task<LocalAttendanceLog?> GetTodayOpenLogAsync(string userId);
    Task UpsertAsync(LocalAttendanceLog entity);
    Task DeleteAsync(string id);
}

public sealed class LocalAttendanceRepository(Database.LocalDatabase db) : ILocalAttendanceRepository
{
    public async Task<List<LocalAttendanceLog>> GetAllAsync() =>
        await db.GetConnection().Table<LocalAttendanceLog>().OrderByDescending(x => x.ClockInAt).ToListAsync();

    public async Task<LocalAttendanceLog?> GetByIdAsync(string id) =>
        await db.GetConnection().Table<LocalAttendanceLog>().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<LocalAttendanceLog>> GetBySyncStatusAsync(SyncStatus status) =>
        await db.GetConnection().Table<LocalAttendanceLog>().Where(x => x.SyncStatusValue == (int)status).ToListAsync();

    public async Task<LocalAttendanceLog?> GetTodayOpenLogAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        return await db.GetConnection().Table<LocalAttendanceLog>()
            .Where(x => x.UserId == userId && x.WorkDate == today && x.Status == "open")
            .FirstOrDefaultAsync();
    }

    public async Task UpsertAsync(LocalAttendanceLog entity) =>
        await db.GetConnection().InsertOrReplaceAsync(entity);

    public async Task DeleteAsync(string id) =>
        await db.GetConnection().DeleteAsync<LocalAttendanceLog>(id);
}
