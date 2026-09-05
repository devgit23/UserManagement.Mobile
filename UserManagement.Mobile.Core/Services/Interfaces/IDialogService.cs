namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface IDialogService
{
    Task<string?> PromptAsync(string title, string message, string accept, string cancel, string? placeholder = null, int maxLength = -1);
}
