using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("leave_requests")]
public sealed class LocalLeaveRequest : LocalEntityBase
{
    public string UserId { get; set; } = string.Empty;
    public string LeaveCode { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty; // yyyy-MM-dd
    public string EndDate { get; set; } = string.Empty;   // yyyy-MM-dd
    public string StartSession { get; set; } = "FullDay";
    public string EndSession { get; set; } = "FullDay";
    public decimal RequestedDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTimeOffset RequestedOn { get; set; }
}
