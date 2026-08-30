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
    }
}
