using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("attendance_logs")]
public sealed class LocalAttendanceLog : LocalEntityBase
{
    public string UserId { get; set; } = string.Empty;
    public string? UserDisplayName { get; set; }
    public string WorkDate { get; set; } = string.Empty; // stored as yyyy-MM-dd
    public string? ShiftId { get; set; }
    public DateTimeOffset ClockInAt { get; set; }
    public DateTimeOffset? ClockOutAt { get; set; }
    public int? GrossMinutes { get; set; }
    public int? EffectiveMinutes { get; set; }
    public int LateArrivalMinutes { get; set; }
    public int EarlyExitMinutes { get; set; }
    public string Status { get; set; } = "open";
    public string? Remarks { get; set; }

    /// <summary>Biometric verification method: "face", "fingerprint", or "manual".</summary>
    public string? VerificationMethod { get; set; }

    /// <summary>Biometric match confidence score (0.0 – 1.0).</summary>
    public double? VerificationConfidence { get; set; }
}
