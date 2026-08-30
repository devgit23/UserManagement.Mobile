using System.Globalization;
using UserManagement.Mobile.Helpers;

namespace UserManagement.Mobile.Converters;

/// <summary>
/// Converts a notification type string to a Material Design Icon glyph.
/// </summary>
public sealed class NotificationTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value?.ToString()?.ToLowerInvariant() ?? "";

        return type switch
        {
            "leave" or "leaveapproval" => IconFont.CalendarCheck,
            "attendance" => IconFont.ClockOutline,
            "announcement" => IconFont.Bell,
            "alert" or "warning" => IconFont.AlertCircleOutline,
            _ => IconFont.InformationOutline
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
