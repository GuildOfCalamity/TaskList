using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

/// <summary>
/// Removes a bracket preamble.
/// </summary>
/// <example>
/// "[1]Some info here" => "Some info here"
/// </example>
public class PreambleConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return null;

        if (value is string text && text.StartsWith("["))
        {
            var shorten = text.Substring(text.IndexOf("]"));
            return shorten;
        }

        if (value is string[] texts && texts.Length > 0)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] = texts[i].Substring(texts[i].IndexOf("]"));
            }
            return texts;
        }

        if (value is List<string> list && list.Count > 0)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                result.Add(list[i].Substring(list[i].IndexOf("]")));
            }
            return result;
        }

        Debug.WriteLine($"value is of type '{value.GetType()}'");

        return value;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}