using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
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

    private async void OnProfileTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    private async void OnDirectoryTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EmployeeDirectoryPage));
    }

    private async void OnTimesheetTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TimesheetPage));
    }
}
