﻿using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App;

public class LevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        SolidColorBrush scb = new SolidColorBrush(Colors.Gray);

        if (value == null)
            return scb;

        switch ((string)value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 148, 148, 148));
                break;
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 87, 119, 198));
                break;
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 255, 180));
                break;
            case string time when time.Contains("two months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 87, 119, 198));
                break;
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 87, 198, 103));
                break;
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 147, 198, 103));
                break;
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 197, 198, 87));
                break;
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 198, 168, 87));
                break;
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 198, 142, 87));
                break;
            default:
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 198, 87, 88)); // Red
                //scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 131, 87, 198)); // Purple
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
        SolidColorBrush scb = new SolidColorBrush(Colors.DimGray);

        if (value == null)
            return scb;

        switch ((string)value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 85, 85, 85));
                break;
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 43, 59, 99));
                break;
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 128, 90));
                break;
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 43, 99, 51));
                break;
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 73, 99, 51));
                break;
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 98, 99, 43));
                break;
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 84, 43));
                break;
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 71, 43));
                break;
            default:
                scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 43, 44)); // Red
                //scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 65, 43, 99)); // Purple
                break;
        }

        return scb;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}