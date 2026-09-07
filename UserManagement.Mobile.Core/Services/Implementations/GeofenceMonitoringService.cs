using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Core.Services.Implementations;

/// <summary>
/// Base geofence monitoring service. The MAUI project provides the platform-specific
/// implementation that uses Microsoft.Maui.Devices.Sensors.Geolocation.
/// </summary>
public class GeofenceMonitoringService : IGeofenceMonitoringService
{
    public bool IsMonitoring { get; protected set; }

    public event EventHandler<GeofenceEventArgs>? GeofenceExited;
    public event EventHandler? GeofenceReEntered;
    public event EventHandler? GraceExpired;
    public event EventHandler<string>? MonitoringError;

    public virtual Task StartMonitoringAsync(double officeLat, double officeLng, double radiusMeters, int gracePeriodMinutes)
    {
        MonitoringError?.Invoke(this, "Geofence monitoring requires a platform-specific implementation");
        return Task.CompletedTask;
    }

    public virtual void StopMonitoring()
    {
        IsMonitoring = false;
    }

    protected void RaiseGeofenceExited(double distance) =>
        GeofenceExited?.Invoke(this, new GeofenceEventArgs(distance));

    protected void RaiseGeofenceReEntered() =>
        GeofenceReEntered?.Invoke(this, EventArgs.Empty);

    protected void RaiseGraceExpired() =>
        GraceExpired?.Invoke(this, EventArgs.Empty);

    protected void RaiseMonitoringError(string message) =>
        MonitoringError?.Invoke(this, message);
}
