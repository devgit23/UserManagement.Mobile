using UserManagement.Mobile.Core.Offline.Database.Entities;

namespace UserManagement.Mobile.Core.Offline.Repositories;

public interface ILocalEmployeeRepository
{
    Task<List<LocalEmployee>> GetAllAsync();
    Task<List<LocalEmployee>> SearchAsync(string? query, string? department);
    Task<LocalEmployee?> GetByIdAsync(string id);
    Task UpsertAsync(LocalEmployee entity);
    Task UpsertManyAsync(IEnumerable<LocalEmployee> entities);
    Task DeleteAsync(string id);
}

public sealed class LocalEmployeeRepository(Database.LocalDatabase db) : ILocalEmployeeRepository
{
    public async Task<List<LocalEmployee>> GetAllAsync() =>
        await db.GetConnection().Table<LocalEmployee>().OrderBy(x => x.FirstName).ToListAsync();

    public async Task<List<LocalEmployee>> SearchAsync(string? query, string? department)
    {
        var all = await GetAllAsync();
        var results = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            results = results.Where(e =>
                (e.FirstName?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.LastName?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.Email?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.EmployeeCode?.Contains(q, StringComparison.OrdinalIgnoreCase) == true));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            results = results.Where(e =>
                string.Equals(e.Department, department, StringComparison.OrdinalIgnoreCase));
        }

        return results.ToList();
    }

    public async Task<LocalEmployee?> GetByIdAsync(string id) =>
        await db.GetConnection().Table<LocalEmployee>().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task UpsertAsync(LocalEmployee entity) =>
        await db.GetConnection().InsertOrReplaceAsync(entity);

    public async Task UpsertManyAsync(IEnumerable<LocalEmployee> entities)
    {
        var conn = db.GetConnection();
        await conn.RunInTransactionAsync(tran =>
        {
            foreach (var entity in entities)
            {
                tran.InsertOrReplace(entity);
            }
        });
    }

    public async Task DeleteAsync(string id) =>
        await db.GetConnection().DeleteAsync<LocalEmployee>(id);
}
