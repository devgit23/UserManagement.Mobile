using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

[Table("employees")]
public sealed class LocalEmployee : LocalEntityBase
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}
