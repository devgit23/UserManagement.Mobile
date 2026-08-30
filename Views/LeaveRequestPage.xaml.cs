using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class LeaveRequestPage : ContentPage
{
    private readonly LeaveRequestViewModel _viewModel;

    public LeaveRequestPage(LeaveRequestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        viewModel.GoBackAsync = () => Shell.Current.GoToAsync("..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
