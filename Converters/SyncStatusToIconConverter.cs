using System.Globalization;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Helpers;

namespace UserManagement.Mobile.Converters;

public sealed class SyncStatusToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SyncStatus status)
        {
            return status switch
            {
                SyncStatus.Synced => IconFont.CheckCircle,
                SyncStatus.PendingUpload => IconFont.CloudUpload,
                SyncStatus.PendingDelete => IconFont.CloudUpload,
                SyncStatus.Conflict => IconFont.Alert,
                SyncStatus.Failed => IconFont.CloseCircle,
                _ => IconFont.Information
            };
        }
        return IconFont.Information;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
