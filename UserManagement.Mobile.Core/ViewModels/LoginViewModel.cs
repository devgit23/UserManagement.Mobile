using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class LoginViewModel(IAuthenticationService authService) : ViewModelBase
{
    [ObservableProperty]
    private string _userNameOrEmail = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _requiresTwoFactor;

    [ObservableProperty]
    private string _twoFactorCode = string.Empty;

    [ObservableProperty]
    private string? _twoFactorToken;

    // Navigation callback set by the View/Shell
    public Func<string, Task>? NavigateAsync { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UserNameOrEmail) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter username and password.");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var result = await authService.LoginAsync(UserNameOrEmail.Trim(), Password);

            if (!result.IsSuccess)
            {
                SetError(result.Error ?? "Login failed.");
                return;
            }

            var response = result.Value;

            if (response?.RequiresTwoFactor == true)
            {
                TwoFactorToken = response.TwoFactorToken;
                RequiresTwoFactor = true;
                return;
            }

            if (response?.IsLockedOut == true)
            {
                SetError("Account is locked. Please try again later.");
                return;
            }

            if (response?.MustChangePassword == true)
            {
                SetError("Password change required. Please use the web portal.");
                return;
            }

            if (response?.Authenticated == true)
            {
                if (NavigateAsync is not null)
                    await NavigateAsync("//Dashboard");
            }
            else
            {
                SetError("Invalid credentials.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Connection error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VerifyTwoFactorAsync()
    {
        if (string.IsNullOrWhiteSpace(TwoFactorCode) || string.IsNullOrEmpty(TwoFactorToken))
        {
            SetError("Please enter the verification code.");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var result = await authService.VerifyTwoFactorAsync(TwoFactorToken, TwoFactorCode.Trim());

            if (!result.IsSuccess)
            {
                SetError(result.Error ?? "Verification failed.");
                return;
            }

            if (result.Value?.Authenticated == true)
            {
                if (NavigateAsync is not null)
                    await NavigateAsync("//Dashboard");
            }
            else
            {
                SetError("Invalid verification code.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Connection error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
