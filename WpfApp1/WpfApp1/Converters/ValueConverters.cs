using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.Converters
{
    /// <summary>
    /// Converts boolean IsBlocked value to a user status text
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
            {
                return isBlocked ? "Заблокирован" : "Активен";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status == "Заблокирован";
            }
            return false;
        }
    }

    /// <summary>
    /// Converts boolean IsAvailable value to an availability text
    /// </summary>
    public class BoolToAvailabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAvailable)
            {
                return isAvailable ? "Доступен" : "Занят";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string availability)
            {
                return availability == "Доступен";
            }
            return false;
        }
    }
}