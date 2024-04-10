using System;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

public class StringFormatConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return null;

        if (parameter == null)
            return value;

        return string.Format((string)parameter, value);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
