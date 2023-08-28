using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Task_List_App.Helpers;

public static class GeneralExtensions
{
    public static BitmapImage? GetImageFromAssets(this string assetName)
    {
        BitmapImage? img = null;

        try
        {
            Uri? uri = new Uri("ms-appx:///Assets/" + assetName.Replace("./", ""));
            img = new BitmapImage(uri);
            Debug.WriteLine($"Image resolved!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetImageFromAssets: {ex.Message}");
        }

        return img;
    }

    /// <summary>
    /// Convert a <see cref="DateTime"/> object into an ISO 8601 formatted string.
    /// </summary>
    /// <param name="dateTime"><see cref="DateTime"/></param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToJsonFriendlyFormat(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Converts long file size into typical browser file size.
    /// </summary>
    public static string ToFileSize(this ulong size)
    {
        if (size < 1024) { return (size).ToString("F0") + " Bytes"; }
        if (size < Math.Pow(1024, 2)) { return (size / 1024).ToString("F0") + "KB"; }
        if (size < Math.Pow(1024, 3)) { return (size / Math.Pow(1024, 2)).ToString("F0") + "MB"; }
        if (size < Math.Pow(1024, 4)) { return (size / Math.Pow(1024, 3)).ToString("F0") + "GB"; }
        if (size < Math.Pow(1024, 5)) { return (size / Math.Pow(1024, 4)).ToString("F0") + "TB"; }
        if (size < Math.Pow(1024, 6)) { return (size / Math.Pow(1024, 5)).ToString("F0") + "PB"; }
        return (size / Math.Pow(1024, 6)).ToString("F0") + "EB";
    }

    /// <summary>
    /// Display a readable sentence as to when that time happened.
    /// e.g. "5 minutes ago" or "in 2 days"
    /// </summary>
    /// <param name="value"><see cref="DateTime"/>the past/future time to compare from now</param>
    /// <returns>human friendly format</returns>
    public static string ToReadableTime(this DateTime value, bool useUTC = false)
    {
        TimeSpan ts;
        if (useUTC)
            ts = new TimeSpan(DateTime.UtcNow.Ticks - value.Ticks);
        else
            ts = new TimeSpan(DateTime.Now.Ticks - value.Ticks);

        double delta = ts.TotalSeconds;
        if (delta < 0) // in the future
        {
            delta = Math.Abs(delta);
            if (delta < 60) { return Math.Abs(ts.Seconds) == 1 ? "in one second" : "in " + Math.Abs(ts.Seconds) + " seconds"; }
            if (delta < 120) { return "in a minute"; }
            if (delta < 3000) { return "in " + Math.Abs(ts.Minutes) + " minutes"; } // 50 * 60
            if (delta < 5400) { return "in an hour"; } // 90 * 60
            if (delta < 86400) { return "in " + Math.Abs(ts.Hours) + " hours"; } // 24 * 60 * 60
            if (delta < 172800) { return "tomorrow"; } // 48 * 60 * 60
            if (delta < 2592000) { return "in " + Math.Abs(ts.Days) + " days"; } // 30 * 24 * 60 * 60
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)Math.Abs(ts.Days) / 30));
                return months <= 1 ? "in one month" : "in " + months + " months";
            }
            int years = Convert.ToInt32(Math.Floor((double)Math.Abs(ts.Days) / 365));
            return years <= 1 ? "in one year" : "in " + years + " years";
        }
        else // in the past
        {
            if (delta < 60) { return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago"; }
            if (delta < 120) { return "a minute ago"; }
            if (delta < 3000) { return ts.Minutes + " minutes ago"; } // 50 * 60
            if (delta < 5400) { return "an hour ago"; } // 90 * 60
            if (delta < 86400) { return ts.Hours + " hours ago"; } // 24 * 60 * 60
            if (delta < 172800) { return "yesterday"; } // 48 * 60 * 60
            if (delta < 2592000) { return ts.Days + " days ago"; } // 30 * 24 * 60 * 60
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }



    /// <summary>
    /// Gets the default member name that is used for an indexer (e.g. "Item").
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>Default member name.</returns>
    public static string? GetDefaultMemberName(this Type type)
    {
        DefaultMemberAttribute? defaultMemberAttribute = type.GetTypeInfo().GetCustomAttributes().OfType<DefaultMemberAttribute>().FirstOrDefault();
        return defaultMemberAttribute == null ? null : defaultMemberAttribute.MemberName;
    }

    /// <summary>
    /// Returns an <see cref="Uri"/> that starts with the ms-appx:// prefix
    /// https://learn.microsoft.com/en-us/windows/uwp/app-resources/uri-schemes
    /// Use the ms-appx or the ms-appx-web URI scheme to refer to a file that comes from your app's package.
    /// Query parameters are ignored during retrieval of resources. Query parameters are not ignored during comparison.
    /// For information on storing strings in a resource file see...
    /// https://learn.microsoft.com/en-us/windows/uwp/app-resources/localize-strings-ui-manifest
    /// </summary>
    /// <param name="uri">The input <see cref="Uri"/> to process</param>
    /// <returns>A <see cref="Uri"/> equivalent to the first but relative to ms-appx://</returns>
    /// <remarks>This is needed because the XAML converter doesn't use the ms-appx:// prefix</remarks>
    public static Uri? ToAppxUri(this Uri uri)
    {
        if (uri.Scheme.Equals("ms-resource")) // ms-resource:///Hello%23World/String1
        {
            Debug.WriteLine($"Authority (ms-resource)");

            string path = uri.AbsolutePath.StartsWith("/Files")
                ? uri.AbsolutePath.Replace("/Files", string.Empty)
                : uri.AbsolutePath;

            return new Uri($"ms-appx://{path}");
        }
        else if (uri.Scheme.Equals("ms-appx")) // ms-appx:///images/logo.png
        {
            Debug.WriteLine($"Authority (ms-appx)");
        }
        else if (uri.Scheme.Equals("ms-appx-web")) // ms-appx-web:///images/logo.png
        {
            Debug.WriteLine($"Authority (ms-appx-web)");
        }
        else if (uri.Scheme.Equals("ms-appdata"))
        {
            // ms-appdata://local/Hello%23World.html
            // ms-appdata://temp/Hello%23World.html
            // ms-appdata://roaming/Hello%23World.html
            Debug.WriteLine($"Authority (ms-appdata)");
        }

        return uri;
    }

    /// <summary>
    /// Merges the two input <see cref="IList{T}"/> instances and makes sure no duplicate items are present
    /// </summary>
    /// <typeparam name="T">The type of elements in the input collections</typeparam>
    /// <param name="a">The first <see cref="IList{T}"/> to merge</param>
    /// <param name="b">The second <see cref="IList{T}"/> to merge</param>
    /// <returns>An <see cref="IList{T}"/> instance with elements from both <paramref name="a"/> and <paramref name="b"/></returns>
    public static IList<T> Merge<T>(this IList<T> a, IList<T> b)
    {
        if (a.Any(b.Contains))
            Debug.WriteLine("WARNING: The input collection has at least an item already present in the second collection");

        return a.Concat(b).ToArray();
    }

    /// <summary>
    /// Merges the two input <see cref="IEnumerable{T}"/> instances and makes sure no duplicate items are present
    /// </summary>
    /// <typeparam name="T">The type of elements in the input collections</typeparam>
    /// <param name="a">The first <see cref="IEnumerable{T}"/> to merge</param>
    /// <param name="b">The second <see cref="IEnumerable{T}"/> to merge</param>
    /// <returns>An <see cref="IEnumerable{T}"/> instance with elements from both <paramref name="a"/> and <paramref name="b"/></returns>
    public static IEnumerable<T> Merge<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        if (a.Any(b.Contains))
            Debug.WriteLine("WARNING: The input collection has at least an item already present in the second collection");

        return a.Concat(b).ToArray();
    }

    /// <summary>
    /// Merges the two input <see cref="IReadOnlyCollection{T}"/> instances and makes sure no duplicate items are present
    /// </summary>
    /// <typeparam name="T">The type of elements in the input collections</typeparam>
    /// <param name="a">The first <see cref="IReadOnlyCollection{T}"/> to merge</param>
    /// <param name="b">The second <see cref="IReadOnlyCollection{T}"/> to merge</param>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> instance with elements from both <paramref name="a"/> and <paramref name="b"/></returns>
    public static IReadOnlyCollection<T> Merge<T>(this IReadOnlyCollection<T> a, IReadOnlyCollection<T> b)
    {
        if (a.Any(b.Contains))
            Debug.WriteLine("WARNING: The input collection has at least an item already present in the second collection");

        return a.Concat(b).ToArray();
    }

    /// <summary>
    /// Helper for web images.
    /// </summary>
    /// <returns><see cref="Stream"/></returns>
    public static async Task<Stream> CopyStream(this HttpContent source)
    {
        var stream = new MemoryStream();
        await source.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    /// <summary>
    /// Gets a string value from a <see cref="StorageFile"/> located in the application local folder.
    /// </summary>
    /// <param name="fileName">
    /// The relative <see cref="string"/> file path.
    /// </param>
    /// <returns>
    /// The stored <see cref="string"/> value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Exception thrown if the <paramref name="fileName"/> is null or empty.
    /// </exception>
    public static async Task<string> ReadLocalFileAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        if (App.IsPackaged)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(fileName);
            return await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }
        else
        {
            using (TextReader reader = File.OpenText(Path.Combine(AppContext.BaseDirectory, fileName)))
            {
                return await reader.ReadToEndAsync(); // uses UTF8 by default
            }
        }
    }


    /// <summary>
    /// IEnumerable file reader.
    /// </summary>
    public static IEnumerable<string> ReadFileLines(string path)
    {
        string line = string.Empty;

        if (!File.Exists(path))
            yield return line;
        else
        {
            using (TextReader reader = File.OpenText(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }

    /// <summary>
    /// IAsyncEnumerable file reader.
    /// </summary>
    public static async IAsyncEnumerable<string> ReadFileLinesAsync(string path)
    {
        string line = string.Empty;

        if (!File.Exists(path))
            yield return line;
        else
        {
            using (TextReader reader = File.OpenText(path))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    yield return line;
                }
            }
        }
    }

    /// <summary>
    /// Starts an animation and returns a <see cref="Task"/> that reports when it completes.
    /// </summary>
    /// <param name="storyboard">The target storyboard to start.</param>
    /// <returns>A <see cref="Task"/> that completes when <paramref name="storyboard"/> completes.</returns>
    public static Task BeginAsync(this Storyboard storyboard)
    {
        TaskCompletionSource<object?> taskCompletionSource = new TaskCompletionSource<object?>();

        void OnCompleted(object? sender, object e)
        {
            if (sender is Storyboard storyboard)
                storyboard.Completed -= OnCompleted;

            taskCompletionSource.SetResult(null);
        }

        storyboard.Completed += OnCompleted;
        storyboard.Begin();

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// To get all buttons contained in a StackPanel:
    /// IEnumerable{Button} kids = GetChildren(rootStackPanel).Where(ctrl => ctrl is Button).Cast{Button}();
    /// </summary>
    /// <remarks>You must call this on a UI thread.</remarks>
    public static IEnumerable<UIElement> GetChildren(this UIElement parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is UIElement child)
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Walks the visual tree to determine if a particular child is contained within a parent DependencyObject.
    /// </summary>
    /// <param name="element">Parent DependencyObject</param>
    /// <param name="child">Child DependencyObject</param>
    /// <returns>True if the parent element contains the child</returns>
    public static bool ContainsChild(this DependencyObject element, DependencyObject child)
    {
        if (element != null)
        {
            while (child != null)
            {
                if (child == element)
                    return true;

                // Walk up the visual tree.  If the root is hit, try using the framework element's
                // parent.  This is done because Popups behave differently with respect to the visual tree,
                // and it could have a parent even if the VisualTreeHelper doesn't find it.
                DependencyObject parent = VisualTreeHelper.GetParent(child);
                if (parent == null)
                {
                    FrameworkElement? childElement = child as FrameworkElement;
                    if (childElement != null)
                    {
                        parent = childElement.Parent;
                    }
                }
                child = parent;
            }
        }
        return false;
    }

    /// <summary>
    /// Provides the distance in a <see cref="Point"/> from the passed in element to the element being called on.
    /// For instance, calling child.CoordinatesFrom(container) will return the position of the child within the container.
    /// Helper for <see cref="UIElement.TransformToVisual(UIElement)"/>.
    /// </summary>
    /// <param name="target">Element to measure distance.</param>
    /// <param name="parent">Starting parent element to provide coordinates from.</param>
    /// <returns><see cref="Point"/> containing difference in position of elements.</returns>
    public static Windows.Foundation.Point CoordinatesFrom(this UIElement target, UIElement parent)
    {
        return target.TransformToVisual(parent).TransformPoint(default(Windows.Foundation.Point));
    }

    /// <summary>
    /// Provides the distance in a <see cref="Point"/> to the passed in element from the element being called on.
    /// For instance, calling container.CoordinatesTo(child) will return the position of the child within the container.
    /// Helper for <see cref="UIElement.TransformToVisual(UIElement)"/>.
    /// </summary>
    /// <param name="parent">Starting parent element to provide coordinates from.</param>
    /// <param name="target">Element to measure distance to.</param>
    /// <returns><see cref="Point"/> containing difference in position of elements.</returns>
    public static Windows.Foundation.Point CoordinatesTo(this UIElement parent, UIElement target)
    {
        return target.TransformToVisual(parent).TransformPoint(default(Windows.Foundation.Point));
    }


    /// <summary>
    /// I created this to show what controls are members of <see cref="Microsoft.UI.Xaml.FrameworkElement"/>.
    /// </summary>
    public static void FindControlsInheritingFromFrameworkElement()
    {
        var controlAssembly = typeof(Microsoft.UI.Xaml.Controls.Control).GetTypeInfo().Assembly;
        var controlTypes = controlAssembly.GetTypes()
            .Where(type => type.Namespace == "Microsoft.UI.Xaml.Controls" &&
            typeof(Microsoft.UI.Xaml.FrameworkElement).IsAssignableFrom(type));

        foreach (var controlType in controlTypes)
        {
            Debug.WriteLine($"[FrameworkElement] {controlType.FullName}", $"ControlInheritingFrom");
        }
    }

    /// <summary>
    /// Returns a random selection from <see cref="Microsoft.UI.Colors"/>.
    /// </summary>
    /// <returns><see cref="Windows.UI.Color"/></returns>
    public static Windows.UI.Color GetRandomMicrosoftUIColor()
    {
        try
        {
            var colorType = typeof(Microsoft.UI.Colors);
            var colors = colorType.GetProperties()
                .Where(p => p.PropertyType == typeof(Windows.UI.Color) && p.GetMethod != null && p.GetMethod.IsStatic && p.GetMethod.IsPublic)
                .Select(p => (Windows.UI.Color?)p.GetValue(null) ?? Windows.UI.Color.FromArgb(255, 255, 0, 0))
                .ToList();

            if (colors.Count > 0)
            {
                var randomIndex = Random.Shared.Next(colors.Count);
                var randomColor = colors[randomIndex];
                return randomColor;
            }
            else
            {
                return Microsoft.UI.Colors.Gray;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetRandomColor: {ex.Message}", $"{nameof(GeneralExtensions)}");
            return Microsoft.UI.Colors.Red;
        }
    }

    /// <summary>
    /// Creates a Color object from the hex color code and returns the result.
    /// </summary>
    /// <param name="hexColorCode"></param>
    /// <returns></returns>
    public static Windows.UI.Color? GetColorFromHexString(string hexColorCode)
    {
        if (string.IsNullOrEmpty(hexColorCode)) { return null; }

        try
        {
            byte a = 255; byte r = 0; byte g = 0; byte b = 0;

            if (hexColorCode.Length == 9)
            {
                hexColorCode = hexColorCode.Substring(1, 8);
            }

            if (hexColorCode.Length == 8)
            {
                a = Convert.ToByte(hexColorCode.Substring(0, 2), 16);
                hexColorCode = hexColorCode.Substring(2, 6);
            }

            if (hexColorCode.Length == 6)
            {
                r = Convert.ToByte(hexColorCode.Substring(0, 2), 16);
                g = Convert.ToByte(hexColorCode.Substring(2, 2), 16);
                b = Convert.ToByte(hexColorCode.Substring(4, 2), 16);
            }

            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", $"{nameof(GeneralExtensions)}");
            return null;
        }
    }

    /// <summary>
    /// Generates a 6 digit color string and may include the # sign.
    /// The 0 and 1 options have been removed so dark colors such as #000000/#111111 are not possible.
    /// </summary>
    public static string GetRandomColorString(bool includePound = true)
    {
        StringBuilder sb = new StringBuilder();
        const string pwChars = "2346789ABCDEF";
        char[] charArray = pwChars.Distinct().ToArray();

        var result = new char[7];
        var rng = new Random();

        if (includePound)
            sb.Append("#");

        for (int x = 0; x < 6; x++)
            sb.Append(pwChars[rng.Next() % pwChars.Length]);

        return sb.ToString();
    }

    /// <summary>
    /// This performs no conversion, it reboxes the same value in another type.
    /// </summary>
    /// <example>
    /// object? enumTest = LogLevel.Notice.GetBoxedEnumValue();
    /// Debug.WriteLine($"{enumTest} ({enumTest.GetType()})");
    /// Output: "5 (System.Int32)"
    /// </example>
    public static object GetBoxedEnumValue(this Enum anyEnum)
    {
        Type intType = Enum.GetUnderlyingType(anyEnum.GetType());
        return Convert.ChangeType(anyEnum, intType);
    }

    /// <summary>
    /// Get OS version by way of <see cref="Windows.System.Profile.AnalyticsInfo"/>.
    /// </summary>
    /// <returns><see cref="Version"/></returns>
    public static Version GetWindowsVersionUsingAnalyticsInfo()
    {
        try
        {
            ulong version = ulong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            var Major = (ushort)((version & 0xFFFF000000000000L) >> 48);
            var Minor = (ushort)((version & 0x0000FFFF00000000L) >> 32);
            var Build = (ushort)((version & 0x00000000FFFF0000L) >> 16);
            var Revision = (ushort)(version & 0x000000000000FFFFL);

            return new Version(Major, Minor, Build, Revision);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetWindowsVersionUsingAnalyticsInfo: {ex.Message}", $"{nameof(GeneralExtensions)}");
            return new Version(); // 0.0
        }
    }

    /// <summary>
    /// Macro for <see cref="IEnumerable{T}"/> collections.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
        {
            try { action(i); }
            catch (Exception ex) { Debug.WriteLine($"{ex.GetType()}: {ex.Message}", $"{nameof(GeneralExtensions)}"); }
        }
    }

    /// <summary>
    /// Will retry each operation with a 2 second delay between attempts.
    /// </summary>
    public static T Retry<T>(this Func<T> operation, int attempts)
    {
        while (true)
        {
            try
            {
                attempts--;
                return operation();
            }
            catch (Exception ex) when (attempts > 0)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", $"{nameof(GeneralExtensions)}");
                Thread.Sleep(2000);
            }
        }
    }

    /// <summary>
    /// Use this if you only have a root resource dictionary.
    /// var rdBrush = Extensions.GetResource{SolidColorBrush}("PrimaryBrush");
    /// </summary>
    public static T? GetResource<T>(string resourceName) where T : class
    {
        try
        {
            if (Application.Current.Resources.TryGetValue($"{resourceName}", out object value))
                return (T)value;
            else
                return default(T);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetResource: {ex.Message}", $"{nameof(GeneralExtensions)}");
            return null;
        }
    }

    /// <summary>
    /// Use this if you have merged theme resource dictionaries.
    /// var darkBrush = Extensions.GetThemeResource{SolidColorBrush}("PrimaryBrush", ElementTheme.Dark);
    /// var lightBrush = Extensions.GetThemeResource{SolidColorBrush}("PrimaryBrush", ElementTheme.Light);
    /// </summary>
    public static T? GetThemeResource<T>(string resourceName, ElementTheme? theme) where T : class
    {
        try
        {
            if (theme == null) { theme = ElementTheme.Default; }

            var dictionaries = Application.Current.Resources.MergedDictionaries;
            foreach (var item in dictionaries)
            {
                // Do we have any themes in this resource dictionary?
                if (item.ThemeDictionaries.Count > 0)
                {
                    if (theme == ElementTheme.Dark)
                    {
                        if (item.ThemeDictionaries.TryGetValue("Dark", out var drd))
                        {
                            ResourceDictionary? dark = drd as ResourceDictionary;
                            if (dark != null)
                            {
                                Debug.WriteLine($"Found dark theme resource dictionary", $"{nameof(GeneralExtensions)}");
                                if (dark.TryGetValue($"{resourceName}", out var tmp))
                                    return (T)tmp;
                                else
                                    Debug.WriteLine($"Could not find '{resourceName}'", $"{nameof(GeneralExtensions)}");
                            }
                        }
                        else { Debug.WriteLine($"{nameof(ElementTheme.Dark)} theme was not found", $"{nameof(GeneralExtensions)}"); }
                    }
                    else if (theme == ElementTheme.Light)
                    {
                        if (item.ThemeDictionaries.TryGetValue("Light", out var lrd))
                        {
                            ResourceDictionary? light = lrd as ResourceDictionary;
                            if (light != null)
                            {
                                Debug.WriteLine($"Found light theme resource dictionary", $"{nameof(GeneralExtensions)}");
                                if (light.TryGetValue($"{resourceName}", out var tmp))
                                    return (T)tmp;
                                else
                                    Debug.WriteLine($"Could not find '{resourceName}'", $"{nameof(GeneralExtensions)}");
                            }
                        }
                        else { Debug.WriteLine($"{nameof(ElementTheme.Light)} theme was not found", $"{nameof(GeneralExtensions)}"); }
                    }
                    else if (theme == ElementTheme.Default)
                    {
                        if (item.ThemeDictionaries.TryGetValue("Default", out var drd))
                        {
                            ResourceDictionary? dflt = drd as ResourceDictionary;
                            if (dflt != null)
                            {
                                Debug.WriteLine($"Found default theme resource dictionary", $"{nameof(GeneralExtensions)}");
                                if (dflt.TryGetValue($"{resourceName}", out var tmp))
                                    return (T)tmp;
                                else
                                    Debug.WriteLine($"Could not find '{resourceName}'", $"{nameof(GeneralExtensions)}");
                            }
                        }
                        else { Debug.WriteLine($"{nameof(ElementTheme.Default)} theme was not found", $"{nameof(GeneralExtensions)}"); }
                    }
                    else
                        Debug.WriteLine($"No theme to match", $"{nameof(GeneralExtensions)}");
                }
                else
                    Debug.WriteLine($"No theme dictionaries found", $"{nameof(GeneralExtensions)}");
            }

            return default(T);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetThemeResource: {ex.Message}", $"{nameof(GeneralExtensions)}");
            return null;
        }
    }

    /// <summary>
    /// Chainable task helper.
    /// var result = await SomeLongAsyncFunction().WithTimeout(TimeSpan.FromSeconds(2));
    /// </summary>
    /// <typeparam name="TResult">the type of task result</typeparam>
    /// <returns><see cref="Task"/>TResult</returns>
    public async static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        Task winner = await (Task.WhenAny(task, Task.Delay(timeout)));

        if (winner != task)
            throw new TimeoutException();

        return await task;   // Unwrap result/re-throw
    }

    /// <summary>
    /// Task extension to add a timeout.
    /// </summary>
    /// <returns>The task with timeout.</returns>
    /// <param name="task">Task.</param>
    /// <param name="timeoutInMilliseconds">Timeout duration in Milliseconds.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
    {
        var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds))
            .ConfigureAwait(false);

        #pragma warning disable CS8603 // Possible null reference return.
        return retTask is Task<T> ? task.Result : default;
        #pragma warning restore CS8603 // Possible null reference return.
    }

    /// <summary>
    /// Chainable task helper.
    /// var result = await SomeLongAsyncFunction().WithCancellation(cts.Token);
    /// </summary>
    /// <typeparam name="TResult">the type of task result</typeparam>
    /// <returns><see cref="Task"/>TResult</returns>
    public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancelToken)
    {
        var tcs = new TaskCompletionSource<TResult>();
        var reg = cancelToken.Register(() => tcs.TrySetCanceled());
        task.ContinueWith(ant =>
        {
            reg.Dispose();
            if (ant.IsCanceled)
                tcs.TrySetCanceled();
            else if (ant.IsFaulted)
                tcs.TrySetException(ant.Exception?.InnerException ?? new Exception("Antecendent faulted."));
            else
                tcs.TrySetResult(ant.Result);
        });
        return tcs.Task;  // Return the TaskCompletionSource result
    }

    public static Task<T> WithAllExceptions<T>(this Task<T> task)
    {
        TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

        task.ContinueWith(ignored =>
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    Debug.WriteLine($"[TaskStatus.Canceled]", $"{nameof(GeneralExtensions)}");
                    tcs.SetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    tcs.SetResult(task.Result);
                    //Debug.WriteLine($"[TaskStatus.RanToCompletion({task.Result})]");
                    break;
                case TaskStatus.Faulted:
                    // SetException will automatically wrap the original AggregateException
                    // in another one. The new wrapper will be removed in TaskAwaiter, leaving
                    // the original intact.
                    Debug.WriteLine($"[TaskStatus.Faulted: {task.Exception?.Message}]", $"{nameof(GeneralExtensions)}");
                    tcs.SetException(task.Exception ?? new Exception("Task faulted."));
                    break;
                default:
                    Debug.WriteLine($"[TaskStatus: Continuation called illegally.]", $"{nameof(GeneralExtensions)}");
                    tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                    break;
            }
        });

        return tcs.Task;
    }

    #pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
    /// <summary>
    /// Attempts to await on the task and catches exception
    /// </summary>
    /// <param name="task">Task to execute</param>
    /// <param name="onException">What to do when method has an exception</param>
    /// <param name="continueOnCapturedContext">If the context should be captured.</param>
    public static async void SafeFireAndForget(this Task task, Action<Exception>? onException = null, bool continueOnCapturedContext = false)
    #pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex) when (onException != null)
        {
            onException.Invoke(ex);
        }
        catch (Exception ex) when (onException == null)
        {
            Debug.WriteLine($"SafeFireAndForget: {ex.Message}", $"{nameof(GeneralExtensions)}");
        }
    }

    public static string RemoveExtraSpaces(this string strText)
    {
        if (!string.IsNullOrEmpty(strText))
            strText = Regex.Replace(strText, @"\s+", " ");

        return strText;
    }

    /// <summary>
    /// ExampleTextSample => Example Text Sample
    /// </summary>
    /// <param name="input"></param>
    /// <returns>space delimited string</returns>
    public static string SeparateCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder result = new StringBuilder();
        result.Append(input[0]);

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
                result.Append(' ');

            result.Append(input[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Helper for parsing command line arguments.
    /// </summary>
    /// <param name="inputArray"></param>
    /// <returns>string array of args excluding the 1st arg</returns>
    public static string[] IgnoreFirstTakeRest(this string[] inputArray)
    {
        if (inputArray.Length > 1)
            return inputArray.Skip(1).ToArray();
        else
            return new string[0];
    }

    /// <summary>
    /// Returns the first element from a tokenized string, e.g.
    /// Input:"{tag}"  Output:"tag"
    /// </summary>
    /// <example>
    /// var clean = ExtractFirst("{tag}", '{', '}');
    /// </example>
    public static string ExtractFirst(this string text, char start, char end)
    {
        string pattern = @"\" + start + "(.*?)" + @"\" + end; //pattern = @"\{(.*?)\}"
        Match match = Regex.Match(text, pattern);
        if (match.Success)
            return match.Groups[1].Value;
        else
            return "";
    }

    /// <summary>
    /// Returns the last element from a tokenized string, e.g.
    /// Input:"{tag}"  Output:"tag"
    /// </summary>
    /// <example>
    /// var clean = ExtractLast("{tag}", '{', '}');
    /// </example>
    public static string ExtractLast(this string text, char start, char end)
    {
        string pattern = @"\" + start + @"(.*?)\" + end; //pattern = @"\{(.*?)\}"
        MatchCollection matches = Regex.Matches(text, pattern);
        if (matches.Count > 0)
        {
            Match lastMatch = matches[matches.Count - 1];
            return lastMatch.Groups[1].Value;
        }
        else
            return "";
    }

    /// <summary>
    /// Returns all the elements from a tokenized string, e.g.
    /// Input:"{tag}"  Output:"tag"
    /// </summary>
    public static string[] ExtractAll(this string text, char start, char end)
    {
        string pattern = @"\" + start + @"(.*?)\" + end; //pattern = @"\{(.*?)\}"
        MatchCollection matches = Regex.Matches(text, pattern);
        string[] results = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
            results[i] = matches[i].Groups[1].Value;

        return results;
    }

    /// <summary>
    /// Returns the specified occurrence of a character in a string.
    /// </summary>
    /// <returns>
    /// Index of requested occurrence if successful, -1 otherwise.
    /// </returns>
    /// <example>
    /// If you wanted to find the second index of the percent character in a string:
    /// int index = "blah%blah%blah".IndexOfNth('%', 2);
    /// </example>
    public static int IndexOfNth(this string input, char character, int position)
    {
        int index = -1;

        if (string.IsNullOrEmpty(input))
            return index;

        for (int i = 0; i < position; i++)
        {
            index = input.IndexOf(character, index + 1);
            if (index == -1)
                break;
        }

        return index;
    }

    public static async Task<byte[]> AsPng(this UIElement control)
    {
        // Get XAML Visual in BGRA8 format
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(control, (int)control.ActualSize.X, (int)control.ActualSize.Y);

        // Encode as PNG
        var pixelBuffer = (await rtb.GetPixelsAsync()).ToArray();
        IRandomAccessStream mraStream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, mraStream);
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)rtb.PixelWidth,
            (uint)rtb.PixelHeight,
            184,
            184,
            pixelBuffer);
        await encoder.FlushAsync();

        // Transform to byte array
        var bytes = new byte[mraStream.Size];
        await mraStream.ReadAsync(bytes.AsBuffer(), (uint)mraStream.Size, InputStreamOptions.None);

        return bytes;
    }

    /// <summary>
    /// Task.Factory.StartNew (() => { throw null; }).IgnoreExceptions();
    /// </summary>
    public static void IgnoreExceptions(this Task task)
    {
        task.ContinueWith(t =>
        {
            var ignore = t.Exception;
            var inners = ignore?.Flatten()?.InnerExceptions;
            if (inners != null)
            {
                foreach (Exception ex in inners)
                    Debug.WriteLine($"[{ex.GetType()}]: {ex.Message}", $"{nameof(GeneralExtensions)}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
