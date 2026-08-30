using UserManagement.Shared.Models;

namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface IAuthenticationService
{
    Task<ApiResult<LoginResponse>> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default);
    Task<ApiResult<LoginResponse>> VerifyTwoFactorAsync(string twoFactorToken, string code, bool useBackupCode = false, CancellationToken ct = default);
    Task<bool> RefreshTokenAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> IsAuthenticatedAsync();
}
