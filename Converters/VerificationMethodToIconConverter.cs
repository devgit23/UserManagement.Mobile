using System.Globalization;

namespace UserManagement.Mobile.Converters;

/// <summary>
/// Converts a biometric verification method string to its Material Design Icon glyph.
/// "face" → face-recognition icon, "fingerprint" → fingerprint icon, "manual" → tap icon.
/// </summary>
public sealed class VerificationMethodToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var method = value?.ToString()?.ToLowerInvariant();
        return method switch
        {
            "face" => "\uF0A71",           // mdi-face-recognition
            "manual" => "\uF0A05",         // mdi-gesture-tap
            _ => "\uF0150"                 // mdi-clock (default)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
