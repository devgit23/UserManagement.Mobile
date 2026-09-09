using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UserManagement.Mobile.Core.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isRefreshing;

    /// <summary>
    /// Navigation delegate — wired up from page code-behind to Shell.Current.GoToAsync.
    /// </summary>
    public Func<string, Task> NavigateAsync { get; set; } = _ => Task.CompletedTask;

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task OnAppearingAsync() => Task.CompletedTask;

    public virtual Task OnDisappearingAsync() => Task.CompletedTask;

    protected void ClearError() => ErrorMessage = null;

    protected void SetError(string message) => ErrorMessage = message;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            await OnRefreshAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    protected virtual Task OnRefreshAsync() => Task.CompletedTask;
}
