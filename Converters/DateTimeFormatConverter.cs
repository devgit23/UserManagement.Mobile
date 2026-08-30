using System.Globalization;

namespace UserManagement.Mobile.Converters;

public sealed class DateTimeFormatConverter : IValueConverter
{
    public string Format { get; set; } = "g";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTimeOffset dto => dto.ToLocalTime().ToString(Format),
            DateTime dt => dt.ToString(Format),
            DateOnly d => d.ToString(parameter as string ?? "d"),
            _ => value?.ToString()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
