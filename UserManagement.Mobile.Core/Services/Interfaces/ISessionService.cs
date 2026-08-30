using UserManagement.Shared.Models;

namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface ISessionService
{
    CurrentUserModel? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Dictionary<string, string> ModuleSettings { get; }

    Task<bool> RefreshAsync(CancellationToken ct = default);
    void SetUser(CurrentUserModel user);
    void SetModuleSettings(Dictionary<string, string> settings);
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool IsModuleEnabled(string moduleKey);
    void Clear();

    event EventHandler? Changed;
}
