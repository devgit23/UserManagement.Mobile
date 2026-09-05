using System.Globalization;
using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

#if WINDOWS
        // Constrain desktop window to a phone-like size
        window.Width = 420;
        window.Height = 860;
        window.MinimumWidth = 380;
        window.MinimumHeight = 600;
#endif

        window.Resumed += OnWindowResumed;

        return window;
    }

    private async void OnWindowResumed(object? sender, EventArgs e)
    {
        try
        {
            var session = _services.GetRequiredService<ISessionService>();
            if (!session.IsAuthenticated) return;

            var syncService = _services.GetRequiredService<ISyncService>();
            await syncService.SyncAsync();
        }
        catch
        {
            // Best-effort sync on resume; failures are non-critical
        }
    }
}

/// <summary>Simple converter that returns the first character of a string (for avatar initials).</summary>
public sealed class FirstCharConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && s.Length > 0)
            return s[0].ToString().ToUpper();
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
