using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Mibar
{

public class ThemeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool && (bool)value)
        {
            // 暗色主题
            return (SolidColorBrush)Application.Current.Resources["DarkBackgroundBrush"];
        }
        else
        {
            // 亮色主题
            return (SolidColorBrush)Application.Current.Resources["LightBackgroundBrush"];
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
    }    
}