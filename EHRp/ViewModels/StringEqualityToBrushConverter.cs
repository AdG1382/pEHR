using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace EHRp.ViewModels
{
    public class StringEqualityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string parameterValue)
            {
                return stringValue == parameterValue ? new SolidColorBrush(Color.Parse("#2a3f54")) : new SolidColorBrush(Colors.Transparent);
            }
            
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}