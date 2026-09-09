using UserManagement.Mobile.Views;

namespace UserManagement.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register detail routes (not in tab bar)
        Routing.RegisterRoute("LeaveRequestPage", typeof(LeaveRequestPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
        Routing.RegisterRoute("EmployeeDirectoryPage", typeof(EmployeeDirectoryPage));
        Routing.RegisterRoute("TimesheetPage", typeof(TimesheetPage));

        // Biometric routes
        Routing.RegisterRoute("BiometricChoicePage", typeof(BiometricChoicePage));
        Routing.RegisterRoute("FaceCapturePage", typeof(FaceCapturePage));
        Routing.RegisterRoute("FaceEnrollmentPage", typeof(FaceEnrollmentPage));
        Routing.RegisterRoute("ClockResultPage", typeof(ClockResultPage));
        Routing.RegisterRoute("BiometricSettingsPage", typeof(BiometricSettingsPage));
        Routing.RegisterRoute("AdminBiometricPage", typeof(AdminBiometricPage));
    }
}
