using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Handlers;

public sealed class SecureStorageService : ISecureStorageService
{
    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await SecureStorage.Default.SetAsync("access_token", accessToken);
        await SecureStorage.Default.SetAsync("refresh_token", refreshToken);
    }

    public async Task<(string? AccessToken, string? RefreshToken)> GetTokensAsync()
    {
        var access = await SecureStorage.Default.GetAsync("access_token");
        var refresh = await SecureStorage.Default.GetAsync("refresh_token");
        return (access, refresh);
    }

    public Task ClearTokensAsync()
    {
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }

    public async Task SaveAsync(string key, string value)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await SecureStorage.Default.GetAsync(key);
    }

    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        return Task.CompletedTask;
    }
}
