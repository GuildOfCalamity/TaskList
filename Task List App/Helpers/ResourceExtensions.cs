using Microsoft.UI.Xaml.Markup;
using Windows.ApplicationModel.Resources;

namespace Task_List_App.Helpers;

/// <summary>
/// For more info on resource files/strings, see https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/mrtcore/localize-strings
/// </summary>
/// <remarks>
/// There are more robust solutions for this, e.g. https://github.com/AndrewKeepCoding/WinUI3Localizer
/// </remarks>
public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();
    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}
