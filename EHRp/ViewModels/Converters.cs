using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EHRp.ViewModels
{
    public class BoolToWidthConverter : IValueConverter
    {
        public double CollapsedWidth { get; set; } = 60;
        public double ExpandedWidth { get; set; } = 220;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? CollapsedWidth : ExpandedWidth;
            }
            return ExpandedWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "True";
        public string FalseValue { get; set; } = "False";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class AlwaysVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Always return true to make the element visible
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converts a boolean value to a color.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a color.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">A parameter in the format "TrueColor;FalseColor".</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <returns>The color value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParam)
            {
                var colors = colorParam.Split(';');
                if (colors.Length == 2)
                {
                    var colorName = boolValue ? colors[0] : colors[1];
                    return SolidColorBrush.Parse(colorName);
                }
            }
            
            return new SolidColorBrush(Colors.Black);
        }
        
        /// <summary>
        /// Not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}