using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter == null)
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        else
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (Visibility)value == Visibility.Collapsed;
    }
}

public class BooleanToVisibilityInverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter == null)
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        else
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (Visibility)value == Visibility.Collapsed;
    }
}
