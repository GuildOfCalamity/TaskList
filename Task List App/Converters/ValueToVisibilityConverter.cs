using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

public class ValueToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value != null && value is string str)
        {
            if (string.IsNullOrEmpty(str))
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }
        else
            return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return Visibility.Visible;
    }
}

public class ValueToEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value != null || (value is string str && !string.IsNullOrEmpty(str)))
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return true;
    }
}
