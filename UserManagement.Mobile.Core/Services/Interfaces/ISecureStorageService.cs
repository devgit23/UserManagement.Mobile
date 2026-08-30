namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface ISecureStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken);
    Task<(string? AccessToken, string? RefreshToken)> GetTokensAsync();
    Task ClearTokensAsync();
    Task SaveAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
