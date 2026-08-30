using Refit;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Shared.ApiContracts;
using UserManagement.Shared.Models;

namespace UserManagement.Mobile.Core.Services.Implementations;

public sealed class AuthenticationService(
    IAuthApi authApi,
    ISecureStorageService secureStorage,
    ISessionService sessionService) : IAuthenticationService
{
    public async Task<ApiResult<LoginResponse>> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default)
    {
        var request = new LoginRequest
        {
            UserNameOrEmail = userNameOrEmail,
            Password = password
        };

        try
        {
            var response = await authApi.LoginAsync(request, ct);
            if (response is { Authenticated: true })
            {
                if (!string.IsNullOrEmpty(response.AccessToken) && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    await secureStorage.SaveTokensAsync(response.AccessToken, response.RefreshToken);

                    // Load user profile
                    try
                    {
                        var user = await authApi.GetCurrentUserAsync(ct);
                        if (user is not null)
                        {
                            sessionService.SetUser(user);
                        }
                    }
                    catch
                    {
                        // Non-fatal: profile load can be retried later
                    }
                }
            }

            return ApiResult<LoginResponse>.Success(response);
        }
        catch (ApiException ex)
        {
            var error = await ExtractErrorMessageAsync(ex);
            return ApiResult<LoginResponse>.Failure(error);
        }
    }

    public async Task<ApiResult<LoginResponse>> VerifyTwoFactorAsync(string twoFactorToken, string code, bool useBackupCode = false, CancellationToken ct = default)
    {
        var request = new TwoFactorLoginRequest
        {
            TwoFactorToken = twoFactorToken,
            Code = code,
            UseBackupCode = useBackupCode
        };

        try
        {
            var response = await authApi.VerifyTwoFactorLoginAsync(request, ct);
            if (response is { Authenticated: true })
            {
                if (!string.IsNullOrEmpty(response.AccessToken) && !string.IsNullOrEmpty(response.RefreshToken))
                {
                    await secureStorage.SaveTokensAsync(response.AccessToken, response.RefreshToken);

                    try
                    {
                        var user = await authApi.GetCurrentUserAsync(ct);
                        if (user is not null)
                        {
                            sessionService.SetUser(user);
                        }
                    }
                    catch
                    {
                        // Non-fatal
                    }
                }
            }

            return ApiResult<LoginResponse>.Success(response);
        }
        catch (ApiException ex)
        {
            var error = await ExtractErrorMessageAsync(ex);
            return ApiResult<LoginResponse>.Failure(error);
        }
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
    {
        var (_, refreshToken) = await secureStorage.GetTokensAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        try
        {
            var result = await authApi.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken }, ct);
            if (result is not null)
            {
                await secureStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
                return true;
            }
        }
        catch
        {
            // Token refresh failed
        }

        return false;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var (_, refreshToken) = await secureStorage.GetTokensAsync();

        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await authApi.LogoutAsync(new LogoutRequest { RefreshToken = refreshToken }, ct);
            }
        }
        catch
        {
            // Best-effort server logout
        }

        await secureStorage.ClearTokensAsync();
        sessionService.Clear();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var (accessToken, _) = await secureStorage.GetTokensAsync();
        return !string.IsNullOrEmpty(accessToken);
    }

    private static async Task<string> ExtractErrorMessageAsync(ApiException ex)
    {
        try
        {
            // Try to read the error body (e.g. {"error": "..."})
            var content = ex.Content;
            if (!string.IsNullOrEmpty(content))
            {
                // Simple extraction for {"error":"message"} pattern
                var doc = System.Text.Json.JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    return errorProp.GetString() ?? ex.Message;
            }
        }
        catch
        {
            // Fallback to status-based message
        }

        return ex.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Invalid credentials.",
            System.Net.HttpStatusCode.Forbidden => "Access denied.",
            System.Net.HttpStatusCode.BadRequest => "Invalid request.",
            _ => ex.Message
        };
    }
}
