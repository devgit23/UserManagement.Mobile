using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Services;

public class DialogService : IDialogService
{
    public async Task<string?> PromptAsync(string title, string message, string accept, string cancel, string? placeholder = null, int maxLength = -1)
    {
        var page = Shell.Current as Page ?? Application.Current?.MainPage;
        if (page is null) return string.Empty;

        return await page.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength);
    }
}
