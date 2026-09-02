using UserManagement.Common.Timesheet;
using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class TimesheetPage : ContentPage
{
    private readonly TimesheetViewModel _viewModel;

    public TimesheetPage(TimesheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();

        // Show/hide recall button based on status
        RecallButton.IsVisible = _viewModel.WeekStatus == TimesheetStatus.Submitted;
    }

    private async void OnStartTimerClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Projects.Count == 0)
        {
            await DisplayAlertAsync("No Projects", "No active projects available to start a timer.", "OK");
            return;
        }

        var projectNames = _viewModel.Projects.Select(p => p.Name).ToArray();
        var selected = await DisplayActionSheetAsync("Select Project", "Cancel", null, projectNames);

        if (selected is null or "Cancel") return;

        var project = _viewModel.Projects.FirstOrDefault(p => p.Name == selected);
        if (project is not null)
        {
            await _viewModel.StartTimerCommand.ExecuteAsync(project);
        }
    }
}
