using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigitalWellbeingWPF.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert an empty string to Collapsed, otherwise Visible.
            if (string.IsNullOrEmpty(value as string))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need two-way conversion.
        }
    }
}