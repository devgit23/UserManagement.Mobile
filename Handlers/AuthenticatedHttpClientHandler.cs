using System.Net;
using System.Net.Http.Headers;
using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Handlers;

public sealed class AuthenticatedHttpClientHandler(ISecureStorageService secureStorage) : DelegatingHandler
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private IAuthenticationService? _authService;

    public void SetAuthService(IAuthenticationService authService) => _authService = authService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var (accessToken, _) = await secureStorage.GetTokensAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && _authService is not null)
        {
            await _refreshLock.WaitAsync(ct);
            try
            {
                var refreshed = await _authService.RefreshTokenAsync(ct);
                if (refreshed)
                {
                    var (newToken, _) = await secureStorage.GetTokensAsync();
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        response = await base.SendAsync(request, ct);
                    }
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        return response;
    }
}
