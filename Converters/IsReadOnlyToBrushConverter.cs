using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace Task_List_App;

public class IsReadOnlyToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        SolidColorBrush scb = new SolidColorBrush((Windows.UI.Color.FromArgb(255, 250, 250, 250)));

        if (value == null)
            return scb;

        //Debug.WriteLine($"Converting {value} to brush.");

        switch ((bool)value)
        {
            case true:
                scb = new SolidColorBrush(Colors.Silver);
                break;
            case false:
                scb = new SolidColorBrush(Colors.White);
                break;
        }

        return scb;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
