using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

public class BooleanToOpacityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is bool boolValue && targetType == typeof(double))
		{
			return boolValue ? 1.0d : 0.25d;
		}

		return 0.25d;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return 1.0d;
	}
}

public class BooleanToOpacityInverseConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is bool boolValue && targetType == typeof(double))
		{
			return boolValue ? 0.25d : 1.0d;
		}

		return 1.0d;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return 0.25d;
	}
}
