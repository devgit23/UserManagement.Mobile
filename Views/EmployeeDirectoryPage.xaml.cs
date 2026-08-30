using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class EmployeeDirectoryPage : ContentPage
{
    private readonly EmployeeDirectoryViewModel _viewModel;

    public EmployeeDirectoryPage(EmployeeDirectoryViewModel viewModel)
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
