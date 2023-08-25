using System;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

public class DaysToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string result = string.Empty;

        if (value.GetType() != typeof(int))
        {
            return $"wrong type";
        }

        switch (value)
        {
            case int d when d >= 728:
                result = "2 years or more";
                break;
            case int d when d >= 546:
                result = "a year and a half";
                break;
            case int d when d >= 364:
                result = "a year";
                break;
            case int d when d >= 182:
                result = "half a year";
                break;
            case int d when d >= 89:
                result = "3 months";
                break;
            case int d when d >= 59:
                result = "2 months";
                break;
            case int d when d >= 28:
                result = "a month";
                break;
            default:
                result = $"{value} days";
                break;
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}