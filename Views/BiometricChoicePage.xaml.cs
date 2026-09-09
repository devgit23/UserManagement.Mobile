using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class BiometricChoicePage : ContentPage, IQueryAttributable
{
    private readonly BiometricChoiceViewModel _viewModel;

    public BiometricChoicePage(BiometricChoiceViewModel viewModel)
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.ApplyQueryAttributes(query);
    }
}
