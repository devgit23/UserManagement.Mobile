using UserManagement.Common.Attendance;
using UserManagement.Mobile.Core.Services.Implementations;

namespace UserManagement.Mobile.Handlers;

public sealed class MauiGeofenceMonitoringService : GeofenceMonitoringService, IDisposable
{
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Timer? _graceTimer;

    private double _officeLat;
    private double _officeLng;
    private double _radiusMeters;
    private int _gracePeriodMinutes;
    private bool _isOutside;

    public override async Task StartMonitoringAsync(double officeLat, double officeLng, double radiusMeters, int gracePeriodMinutes)
    {
        StopMonitoring();

        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            RaiseMonitoringError("Location permission not granted");
            return;
        }

        _officeLat = officeLat;
        _officeLng = officeLng;
        _radiusMeters = radiusMeters;
        _gracePeriodMinutes = gracePeriodMinutes;
        _isOutside = false;
        IsMonitoring = true;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = Task.Run(async () =>
        {
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            try
            {
                await CheckLocationAsync();
                while (await _timer.WaitForNextTickAsync(token))
                {
                    await CheckLocationAsync();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    RaiseMonitoringError(ex.Message));
                StopMonitoring();
            }
        }, token);
    }

    private async Task CheckLocationAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(15)
            });

            if (location is null)
                return;

            var distance = GeoUtils.HaversineDistance(
                location.Latitude, location.Longitude,
                _officeLat, _officeLng);

            var isOutside = distance > _radiusMeters;

            if (isOutside && !_isOutside)
            {
                _isOutside = true;
                MainThread.BeginInvokeOnMainThread(() =>
                    RaiseGeofenceExited(distance));

                _graceTimer?.Dispose();
                _graceTimer = new Timer(_ =>
                {
                    if (_isOutside)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                            RaiseGraceExpired());
                        StopMonitoring();
                    }
                }, null, TimeSpan.FromMinutes(_gracePeriodMinutes), Timeout.InfiniteTimeSpan);
            }
            else if (!isOutside && _isOutside)
            {
                _isOutside = false;
                _graceTimer?.Dispose();
                _graceTimer = null;
                MainThread.BeginInvokeOnMainThread(() =>
                    RaiseGeofenceReEntered());
            }
        }
        catch (FeatureNotSupportedException)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                RaiseMonitoringError("Geolocation not supported on this device"));
            StopMonitoring();
        }
        catch (PermissionException)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                RaiseMonitoringError("Location permission was revoked"));
            StopMonitoring();
        }
    }

    public override void StopMonitoring()
    {
        base.StopMonitoring();
        _isOutside = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _timer?.Dispose();
        _timer = null;
        _graceTimer?.Dispose();
        _graceTimer = null;
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}
