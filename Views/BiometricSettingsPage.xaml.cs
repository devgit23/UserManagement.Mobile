using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class BiometricSettingsPage : ContentPage
{
    private readonly BiometricSettingsViewModel _viewModel;

    public BiometricSettingsPage(BiometricSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        viewModel.NavigateAsync = route => Shell.Current.GoToAsync(route);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
