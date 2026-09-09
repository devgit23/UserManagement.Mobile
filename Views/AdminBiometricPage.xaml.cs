using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class AdminBiometricPage : ContentPage
{
    private readonly AdminBiometricViewModel _viewModel;

    public AdminBiometricPage(AdminBiometricViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
