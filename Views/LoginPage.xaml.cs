using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.NavigateAsync = route => Shell.Current.GoToAsync(route);
    }
}
