using System.Globalization;

namespace UserManagement.Mobile.Converters;

/// <summary>
/// Converts an attendance status string to a semantic color.
/// Parameter: "bg" (default) returns background color, "fg" returns text color.
/// </summary>
public sealed class AttendanceStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLowerInvariant() ?? "";
        var mode = parameter?.ToString()?.ToLowerInvariant() ?? "bg";

        return status switch
        {
            "present" or "completed" => mode == "fg"
                ? Color.FromArgb("#107C10")
                : Color.FromArgb("#DFF6DD"),
            "late" or "halfday" => mode == "fg"
                ? Color.FromArgb("#D48C00")
                : Color.FromArgb("#FFF4CE"),
            "absent" or "missed" => mode == "fg"
                ? Color.FromArgb("#D13438")
                : Color.FromArgb("#FDE7E9"),
            "active" or "clockedin" => mode == "fg"
                ? Color.FromArgb("#0F6CBD")
                : Color.FromArgb("#DEECF9"),
            _ => mode == "fg"
                ? Color.FromArgb("#6E6E6E")
                : Color.FromArgb("#E1E1E1")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
