using System.Globalization;

namespace UserManagement.Mobile.Converters;

/// <summary>
/// Converts a notification type string to a semantic color for the icon background.
/// </summary>
public sealed class NotificationTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value?.ToString()?.ToLowerInvariant() ?? "";

        return type switch
        {
            "leave" or "leaveapproval" => Color.FromArgb("#0F6CBD"),
            "attendance" => Color.FromArgb("#107C10"),
            "alert" or "warning" => Color.FromArgb("#D48C00"),
            _ => Color.FromArgb("#0078D4")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
