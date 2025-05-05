using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections.Generic;

namespace EHRp.ViewModels
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string paramString)
                {
                    var parts = paramString.Split(';');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double trueValue) && double.TryParse(parts[1], out double falseValue))
                    {
                        return boolValue ? trueValue : falseValue;
                    }
                }
                
                return boolValue ? 1.0 : 0.5; // Default values
            }
            
            return 1.0; // Default value
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string paramString)
                {
                    var parts = paramString.Split(';');
                    if (parts.Length == 2)
                    {
                        string weightStr = boolValue ? parts[0] : parts[1];
                        // Parse the font weight string to a FontWeight value
                        switch (weightStr.ToLowerInvariant())
                        {
                            case "thin":
                                return FontWeight.Thin;
                            case "extralight":
                            case "ultralight":
                                return FontWeight.ExtraLight;
                            case "light":
                                return FontWeight.Light;
                            case "normal":
                            case "regular":
                                return FontWeight.Normal;
                            case "medium":
                                return FontWeight.Medium;
                            case "semibold":
                            case "demibold":
                                return FontWeight.SemiBold;
                            case "bold":
                                return FontWeight.Bold;
                            case "extrabold":
                            case "ultrabold":
                                return FontWeight.ExtraBold;
                            case "black":
                            case "heavy":
                                return FontWeight.Black;
                            default:
                                return FontWeight.Normal;
                        }
                    }
                }
                
                return boolValue ? FontWeight.Bold : FontWeight.Normal; // Default values
            }
            
            return FontWeight.Normal; // Default value
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Map different strings to different colors
                switch (stringValue.ToLowerInvariant())
                {
                    case "appointment":
                    case "check-up":
                    case "checkup":
                        return new SolidColorBrush(Color.Parse("#4CAF50")); // Green
                    
                    case "follow-up":
                    case "followup":
                        return new SolidColorBrush(Color.Parse("#2196F3")); // Blue
                    
                    case "consultation":
                        return new SolidColorBrush(Color.Parse("#FF9800")); // Orange
                    
                    case "emergency":
                    case "urgent":
                        return new SolidColorBrush(Color.Parse("#F44336")); // Red
                    
                    case "annual physical":
                    case "physical":
                        return new SolidColorBrush(Color.Parse("#9C27B0")); // Purple
                    
                    case "lab":
                    case "laboratory":
                    case "test":
                        return new SolidColorBrush(Color.Parse("#00BCD4")); // Cyan
                    
                    case "vaccination":
                    case "vaccine":
                    case "immunization":
                        return new SolidColorBrush(Color.Parse("#8BC34A")); // Light Green
                    
                    case "procedure":
                    case "surgery":
                        return new SolidColorBrush(Color.Parse("#FF5722")); // Deep Orange
                    
                    default:
                        return new SolidColorBrush(Color.Parse("#607D8B")); // Blue Grey
                }
            }
            
            return new SolidColorBrush(Colors.Gray); // Default value
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
                
            // Handle numeric comparisons
            if (value is int intValue && int.TryParse(parameter.ToString(), out int intParam))
            {
                return intValue == intParam;
            }
            
            // Handle string comparisons
            if (value is string stringValue && parameter is string stringParam)
            {
                return stringValue.Equals(stringParam, StringComparison.OrdinalIgnoreCase);
            }
            
            // Default comparison
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return parameter;
            }
            
            return Avalonia.AvaloniaProperty.UnsetValue;
        }
    }
}