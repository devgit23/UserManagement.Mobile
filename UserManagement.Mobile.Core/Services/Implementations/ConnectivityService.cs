using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Core.Services.Implementations;

/// <summary>
/// Default connectivity service. The MAUI project should provide a platform-specific
/// implementation that wraps Microsoft.Maui.Networking.Connectivity.
/// </summary>
public class ConnectivityService : IConnectivityService
{
    private bool _isConnected = true;

    public bool IsConnected => _isConnected;

    public event EventHandler<bool>? ConnectivityChanged;

    public void UpdateConnectivity(bool isConnected)
    {
        if (_isConnected == isConnected) return;
        _isConnected = isConnected;
        ConnectivityChanged?.Invoke(this, isConnected);
    }
}
