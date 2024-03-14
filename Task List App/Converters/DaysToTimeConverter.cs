using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

/// <summary>
/// Translates an integer day amount into a general time phrase.
/// </summary>
public class DaysToTimeConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string result = string.Empty;

        if (value.GetType() != typeof(int))
            return $"Wrong type, {nameof(System.Int32)} expected.";

        switch (value)
        {
            case int d when d >= 3640:
                result = "ten years or more";
                break;
            case int d when d >= 3270:
                result = "nine years";
                break;
            case int d when d >= 2910:
                result = "eight years";
                break;
            case int d when d >= 2540:
                result = "seven years";
                break;
            case int d when d >= 2180:
                result = "six years";
                break;
            case int d when d >= 2006:
                result = "five and a half years";
                break;
            case int d when d >= 1824:
                result = "five years";
                break;
            case int d when d >= 1641:
                result = "four and a half years";
                break;
            case int d when d >= 1459:
                result = "four years";
                break;
            case int d when d >= 1274:
                result = "three and a half years";
                break;
            case int d when d >= 1092:
                result = "three years";
                break;
            case int d when d >= 910:
                result = "two and a half years";
                break;
            case int d when d >= 728:
                result = "two years";
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
            case int d when d >= 145:
                result = "five months";
                break;
            case int d when d >= 116:
                result = "four months";
                break;
            case int d when d >= 88:
                result = "three months";
                break;
            case int d when d >= 59:
                result = "two months";
                break;
            case int d when d >= 28:
                result = "a month";
                break;
            case int d when d >= 13:
                result = "two weeks";
                break;
            case int d when d >= 6:
                result = "a week";
                break;
            default:
                result = $"{value} days";
                break;
        }

        return result;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}