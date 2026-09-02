using System.Globalization;

namespace UserManagement.Mobile.Converters;

/// <summary>
/// Converts a TimesheetStatus enum or string to a background/foreground color for badge display.
/// Parameter: "bg" (default) returns background color, "fg" returns text color.
/// </summary>
public sealed class TimesheetStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLowerInvariant() ?? "";
        var mode = parameter?.ToString()?.ToLowerInvariant() ?? "bg";

        return status switch
        {
            "approved" or "locked" => mode == "fg"
                ? Color.FromArgb("#107C10")
                : Color.FromArgb("#DFF6DD"),
            "submitted" => mode == "fg"
                ? Color.FromArgb("#D48C00")
                : Color.FromArgb("#FFF4CE"),
            "rejected" => mode == "fg"
                ? Color.FromArgb("#D13438")
                : Color.FromArgb("#FDE7E9"),
            "draft" => mode == "fg"
                ? Color.FromArgb("#616161")
                : Color.FromArgb("#F3F3F3"),
            _ => mode == "fg"
                ? Color.FromArgb("#0078D4")
                : Color.FromArgb("#DEECF9")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
