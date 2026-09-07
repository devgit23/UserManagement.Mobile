namespace UserManagement.Mobile.Core.Services.Interfaces;

public interface IGeofenceMonitoringService
{
    bool IsMonitoring { get; }

    event EventHandler<GeofenceEventArgs>? GeofenceExited;
    event EventHandler? GeofenceReEntered;
    event EventHandler? GraceExpired;
    event EventHandler<string>? MonitoringError;

    Task StartMonitoringAsync(double officeLat, double officeLng, double radiusMeters, int gracePeriodMinutes);
    void StopMonitoring();
}

public sealed class GeofenceEventArgs(double distance) : EventArgs
{
    public double Distance { get; } = distance;
}
