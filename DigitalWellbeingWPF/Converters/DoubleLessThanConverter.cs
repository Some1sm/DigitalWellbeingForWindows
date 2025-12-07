using System;
using System.Globalization;
using System.Windows.Data;

namespace DigitalWellbeingWPF.Converters
{
    public class DoubleLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string parameterString)
            {
                if (double.TryParse(parameterString, NumberStyles.Any, CultureInfo.InvariantCulture, out double threshold))
                {
                    return doubleValue < threshold;
                }
            }
            return false; // Default to false if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need two-way conversion
        }
    }
}