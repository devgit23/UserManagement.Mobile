using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Shared.Auth;
using UserManagement.Shared.ApiContracts;
using UserManagement.Shared.Models;

namespace UserManagement.Mobile.Core.Services.Implementations;

public sealed class SessionService(IAuthApi authApi, IWorkforceApi workforceApi) : ISessionService
{
    private Dictionary<string, string> _moduleSettings = new(StringComparer.OrdinalIgnoreCase);

    public CurrentUserModel? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public Dictionary<string, string> ModuleSettings => _moduleSettings;

    public event EventHandler? Changed;

    public async Task<bool> RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var user = await authApi.GetCurrentUserAsync(ct);
            if (user is not null)
            {
                CurrentUser = user;
            }
        }
        catch
        {
            Changed?.Invoke(this, EventArgs.Empty);
            return false;
        }

        try
        {
            var settings = await workforceApi.GetModuleSettingsAsync(ct);
            if (settings is not null)
            {
                _moduleSettings = new Dictionary<string, string>(settings, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Non-critical
        }

        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void SetUser(CurrentUserModel user)
    {
        CurrentUser = user;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetModuleSettings(Dictionary<string, string> settings)
    {
        _moduleSettings = new Dictionary<string, string>(settings, StringComparer.OrdinalIgnoreCase);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool HasPermission(string permission) =>
        CurrentUser?.Permissions.Any(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase)) == true;

    public bool HasAnyPermission(params string[] permissions) =>
        permissions.Any(HasPermission);

    public bool IsModuleEnabled(string moduleKey)
    {
        var settingsKey = AppModules.ToSettingsKey(moduleKey);
        if (_moduleSettings.TryGetValue(settingsKey, out var value))
        {
            return !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }
        return true; // Enabled by default
    }

    public void Clear()
    {
        CurrentUser = null;
        _moduleSettings = new(StringComparer.OrdinalIgnoreCase);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
