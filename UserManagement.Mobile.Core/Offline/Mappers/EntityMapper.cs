using UserManagement.Common.Attendance;
using UserManagement.Common.EmployeeProfiles;
using UserManagement.Common.Timesheet;
using UserManagement.Common.Workforce;
using UserManagement.Mobile.Core.Offline.Database.Entities;

namespace UserManagement.Mobile.Core.Offline.Mappers;

public static class EntityMapper
{
    // Attendance
    public static LocalAttendanceLog ToLocal(AttendanceLogModel model) => new()
    {
        Id = model.Id.ToString(),
        UserId = model.UserId.ToString(),
        UserDisplayName = model.UserDisplayName,
        WorkDate = model.WorkDate.ToString("yyyy-MM-dd"),
        ShiftId = model.ShiftId?.ToString(),
        ClockInAt = model.ClockInAt,
        ClockOutAt = model.ClockOutAt,
        GrossMinutes = model.GrossMinutes,
        EffectiveMinutes = model.EffectiveMinutes,
        LateArrivalMinutes = model.LateArrivalMinutes,
        EarlyExitMinutes = model.EarlyExitMinutes,
        Status = model.Status,
        Remarks = model.Remarks,
        LastModifiedUtc = DateTimeOffset.UtcNow
    };

    // Leave
    public static LocalLeaveRequest ToLocal(LeaveRequestModel model) => new()
    {
        Id = model.Id.ToString(),
        UserId = model.UserId.ToString(),
        LeaveCode = model.LeaveCode,
        StartDate = model.StartDate.ToString("yyyy-MM-dd"),
        EndDate = model.EndDate.ToString("yyyy-MM-dd"),
        StartSession = model.StartSession.ToString(),
        EndSession = model.EndSession.ToString(),
        RequestedDays = model.RequestedDays,
        Reason = model.Reason,
        Status = model.Status.ToString(),
        RequestedOn = model.RequestedOn,
        LastModifiedUtc = DateTimeOffset.UtcNow
    };

    // Timesheet Entry
    public static LocalTimesheetEntry ToLocal(TimesheetEntryModel model) => new()
    {
        Id = model.Id.ToString(),
        UserId = model.UserId.ToString(),
        ProjectId = model.ProjectId.ToString(),
        TaskId = model.TaskId?.ToString(),
        ProjectName = model.ProjectName,
        ProjectCode = model.ProjectCode,
        ProjectColorHex = model.ProjectColorHex,
        TaskName = model.TaskName,
        TaskCode = model.TaskCode,
        WorkDate = model.WorkDate.ToString("yyyy-MM-dd"),
        Hours = model.Hours,
        Minutes = model.Minutes,
        Description = model.Description,
        IsBillable = model.IsBillable,
        IsOvertime = model.IsOvertime,
        TimerStartedAt = model.TimerStartedAt?.ToString("O"),
        TimerStoppedAt = model.TimerStoppedAt?.ToString("O"),
        EntryMethod = model.EntryMethod.ToString(),
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt,
        LastModifiedUtc = DateTimeOffset.UtcNow
    };

    // Employee
    public static LocalEmployee ToLocal(EmployeeProfileModel model) => new()
    {
        Id = model.UserId.ToString(),
        UserName = model.UserName,
        FirstName = model.FirstName,
        LastName = model.LastName,
        Email = model.Email,
        PhotoUrl = model.PhotoUrl,
        EmployeeCode = model.EmployeeCode,
        Designation = model.Designation,
        Department = model.Department,
        EmploymentStatus = model.EmploymentStatus,
        IsActive = model.IsActive,
        LastModifiedUtc = DateTimeOffset.UtcNow
    };
}
