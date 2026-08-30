using System.Globalization;

namespace UserManagement.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
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

        return window;
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
