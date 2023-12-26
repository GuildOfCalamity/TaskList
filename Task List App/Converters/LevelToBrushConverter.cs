using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App;

public class LevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int alpha = 255;
        SolidColorBrush scb = new SolidColorBrush(Colors.Gray);

        if (value == null)
            return scb;

       if (parameter != null && int.TryParse($"{parameter}", out alpha))
       {
           if (alpha > 255)
               alpha = 255;
       }

        switch ((string)value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 148, 148, 148));
                break;
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 87, 129, 198));
                break;
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 0, 255, 180));
                break;
            case string time when time.Contains("two months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 87, 119, 198));
                break;
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 87, 198, 103));
                break;
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 147, 198, 103));
                break;
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 197, 198, 87));
                break;
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 198, 168, 87));
                break;
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 198, 142, 87));
                break;
            default:
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 198, 87, 88)); // Red
                //scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 131, 87, 198)); // Purple
                break;
        }

        return scb;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}

public class LevelToBrushBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int alpha = 255;
        SolidColorBrush scb = new SolidColorBrush(Colors.DimGray);

        if (value == null)
            return scb;

       if (parameter != null && int.TryParse($"{parameter}", out alpha))
       {
           if (alpha > 255)
               alpha = 255;
       }


        switch ((string)value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 85, 85, 85));
                break;
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 43, 69, 99));
                break;
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 0, 128, 90));
                break;
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 43, 99, 51));
                break;
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 73, 99, 51));
                break;
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 98, 99, 43));
                break;
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 99, 84, 43));
                break;
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 99, 71, 43));
                break;
            default:
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 99, 43, 44)); // Red
                //scb = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)alpha, 65, 43, 99)); // Purple
                break;
        }

        return scb;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
