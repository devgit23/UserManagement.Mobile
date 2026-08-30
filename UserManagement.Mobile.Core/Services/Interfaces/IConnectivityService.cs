namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool>? ConnectivityChanged;
}
