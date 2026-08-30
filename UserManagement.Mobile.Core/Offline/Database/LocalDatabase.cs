using SQLite;
using UserManagement.Mobile.Core.Offline.Database.Entities;

namespace UserManagement.Mobile.Core.Offline.Database;

public sealed class LocalDatabase
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;

    public LocalDatabase(string dbPath)
    {
        _dbPath = dbPath;
    }

    private SQLiteAsyncConnection Connection => _connection
        ?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        if (_connection is not null) return;

        _connection = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _connection.CreateTableAsync<LocalAttendanceLog>();
        await _connection.CreateTableAsync<LocalLeaveRequest>();
        await _connection.CreateTableAsync<LocalEmployee>();
        await _connection.CreateTableAsync<LocalNotification>();
        await _connection.CreateTableAsync<SyncQueueItem>();
    }

    public SQLiteAsyncConnection GetConnection() => Connection;

    public async Task ClearAllAsync()
    {
        await Connection.DeleteAllAsync<LocalAttendanceLog>();
        await Connection.DeleteAllAsync<LocalLeaveRequest>();
        await Connection.DeleteAllAsync<LocalEmployee>();
        await Connection.DeleteAllAsync<LocalNotification>();
        await Connection.DeleteAllAsync<SyncQueueItem>();
    }
}
