using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

using Task_List_App.Helpers;
using Task_List_App.Models;
using Microsoft.UI.Xaml;

namespace Task_List_App;

/// <summary>
/// Provides an indication of how overdue an uncompleted <see cref="TaskItem"/> is.
/// </summary>
public class TimeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        SolidColorBrush scb = new SolidColorBrush(Colors.WhiteSmoke);

        if (value == null)
            return scb;

        //Debug.WriteLine($"[INFO] Got value of type \"{value.GetType()}\"");

        if (parameter != null)
        {
            Debug.WriteLine($"[INFO] Got parameter of type \"{parameter.GetType()}\"");
        }

        if (value is TaskItem ti)
        {
            // We'll only color task items that have not been completed yet.
            if (ti != null && ti.Completion == null)
            {

                var diff = DateTime.Now - ti.Created;
                Debug.WriteLine($"[INFO] {ti.Time} => {diff.TotalDays} days");
                var severity = GeneralExtensions.GetInfoBarSeverity(ti.Time, diff);
                switch (severity)
                {
                    case Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success:       // green
                        scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 194, 255, 168));
                        break;
                    case Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational: // yellow
                        scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 241, 168));
                        break;
                    case Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning:       // orange
                        scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 208, 158));
                        break;
                    case Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error:         // red
                        scb = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 174, 148));
                        break;
                    default:
                        Debug.WriteLine($"[WARNING] Not defined: \"{severity}\"");
                        break;
                }
            }
        }

        var currentTheme = App.MainRoot?.ActualTheme ?? ElementTheme.Default;
        if (currentTheme == ElementTheme.Light)
            scb = new SolidColorBrush(scb.Color.DarkerBy(0.4f));

        return scb;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}

/// <summary>
/// Comming Soon?
/// https://github.com/microsoft/microsoft-ui-xaml/issues/8334
/// https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/multibinding?view=net-maui-8.0
/// </summary>
//public class TimeToBrushConverter : IMultiValueConverter
//{
//    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
//    {
//        string name;
//
//        switch ((string)parameter)
//        {
//            case "FormatNormal":
//                name = values[0] + " " + values[1];
//                break;
//            case "FormatSpecial":
//                name = values[1] + ", " + values[0];
//                break;
//            default:
//                name = values[0] + " " + values[1];
//                break;
//        }
//
//        return name;
//    }
//
//    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
//    {
//        string[] splitValues = ((string)value).Split(' ');
//        return splitValues;
//    }
//}
