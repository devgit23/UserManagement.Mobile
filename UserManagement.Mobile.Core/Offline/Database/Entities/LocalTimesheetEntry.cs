using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("timesheet_entries")]
public sealed class LocalTimesheetEntry : LocalEntityBase
{
    public string UserId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? TaskId { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectCode { get; set; }
    public string? ProjectColorHex { get; set; }
    public string? TaskName { get; set; }
    public string? TaskCode { get; set; }
    public string WorkDate { get; set; } = string.Empty; // stored as yyyy-MM-dd
    public decimal Hours { get; set; }
    public int Minutes { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    public bool IsOvertime { get; set; }
    public string? TimerStartedAt { get; set; } // stored as ISO 8601
    public string? TimerStoppedAt { get; set; } // stored as ISO 8601
    public string EntryMethod { get; set; } = "Manual";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
