using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Shared.Auth;

namespace UserManagement.Mobile.Core.Helpers;

public static class PermissionHelper
{
    public static bool IsAdmin(ISessionService session) =>
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.UsersManage);

    public static bool CanViewAttendance(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Attendance) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.AttendanceView, AppPermissions.AttendanceManage);

    public static bool CanViewLeave(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Leave) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.LeaveView, AppPermissions.LeaveManage,
            AppPermissions.LeaveApproveTeam, AppPermissions.LeaveApproveAll);

    public static bool CanViewEmployees(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Employees) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.EmployeesView, AppPermissions.EmployeesManage);

    public static bool CanViewNotifications(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Notifications) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.NotificationsView);

    public static bool CanViewTeam(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Team) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.TeamView);

    public static bool CanViewHelpdesk(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Helpdesk) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.HelpdeskView, AppPermissions.HelpdeskManage);

    public static bool CanViewAnnouncements(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Announcements) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.AnnouncementsView, AppPermissions.AnnouncementsManage);

    /// <summary>Can the user enroll their own biometric data (face/fingerprint).</summary>
    public static bool CanEnrollBiometric(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Biometric) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.BiometricEnroll, AppPermissions.BiometricManage);

    /// <summary>Can the user manage biometric enrollments for other users (admin).</summary>
    public static bool CanManageBiometric(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Biometric) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.BiometricManage);

    /// <summary>Is biometric attendance available for this user (module enabled + has attendance + enrollment permission).</summary>
    public static bool IsBiometricAttendanceEnabled(ISessionService session) =>
        session.IsModuleEnabled(AppModules.Biometric) &&
        CanViewAttendance(session) &&
        session.HasAnyPermission(AppPermissions.WorkforceAdmin, AppPermissions.BiometricEnroll, AppPermissions.BiometricManage);
}
