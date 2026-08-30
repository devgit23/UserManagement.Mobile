using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("sync_queue")]
public sealed class SyncQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty; // Create, Update, Delete
    public string SerializedPayload { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
