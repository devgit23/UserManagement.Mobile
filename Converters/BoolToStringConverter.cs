using System.Globalization;

namespace UserManagement.Mobile.Converters;

public sealed class BoolToStringConverter : IValueConverter
{
    public string TrueString { get; set; } = "True";
    public string FalseString { get; set; } = "False";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? (b ? TrueString : FalseString) : FalseString;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
