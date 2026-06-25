using System.Globalization;
using System.Windows.Data;

namespace CriGes.Desktop.Converters;

public sealed class LocalDateTimeDisplayConverter : IValueConverter
{
    private const string DisplayFormat = "dd/MM/yyyy HH:mm";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToLocalTime().ToString(DisplayFormat, culture),
            DateTime dateTime => ToLocalDateTime(dateTime).ToString(DisplayFormat, culture),
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static DateTime ToLocalDateTime(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Local
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
    }
}
