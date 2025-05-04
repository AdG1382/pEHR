using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EHRp.Converters
{
    /// <summary>
    /// Converts a boolean value to a brush for error messages
    /// </summary>
    public class BoolToErrorBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a brush
        /// </summary>
        /// <param name="value">The boolean value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The parameter</param>
        /// <param name="culture">The culture</param>
        /// <returns>A brush for error messages</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError && isError)
            {
                return new SolidColorBrush(Color.Parse("#F44336")); // Red for errors
            }
            
            return new SolidColorBrush(Color.Parse("#4CAF50")); // Green for success
        }
        
        /// <summary>
        /// Converts a brush back to a boolean value
        /// </summary>
        /// <param name="value">The brush</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The parameter</param>
        /// <param name="culture">The culture</param>
        /// <returns>A boolean value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}