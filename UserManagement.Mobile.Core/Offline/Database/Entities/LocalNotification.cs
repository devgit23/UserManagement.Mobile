using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("notifications")]
public sealed class LocalNotification : LocalEntityBase
{
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
}
