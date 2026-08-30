using UserManagement.Mobile.Core.Services.Implementations;

namespace UserManagement.Mobile.Handlers;

public sealed class MauiConnectivityService : ConnectivityService, IDisposable
{
    public MauiConnectivityService()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        UpdateConnectivity(Connectivity.Current.NetworkAccess == NetworkAccess.Internet);
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        UpdateConnectivity(e.NetworkAccess == NetworkAccess.Internet);
    }

    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}
