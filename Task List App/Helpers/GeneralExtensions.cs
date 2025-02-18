using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Task_List_App.Helpers;

public static class GeneralExtensions
{
    /// <summary>
    /// Copies one <see cref="List{T}"/> to another <see cref="List{T}"/> by value (deep copy).
    /// </summary>
    /// <returns><see cref="List{T}"/></returns>
    /// <remarks>
    /// If your model does not inherit from <see cref="ICloneable"/>
    /// then a manual DTO copying technique could be used instead.
    /// </remarks>
    public static List<T> DeepCopy<T>(this List<T> source) where T : ICloneable
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        List<T> destination = new List<T>(source.Count);
        foreach (T item in source)
        {
            if (item is ICloneable cloneable)
                destination.Add((T)cloneable.Clone());
            else
                throw new InvalidOperationException($"Type {typeof(T).FullName} does not implement ICloneable.");
        }

        return destination;
    }

    /// <summary>
    /// The BinaryFormatter type is dangerous and is not recommended 
    /// for data processing. Applications should stop using BinaryFormatter 
    /// as soon as possible, even if they believe the data they're processing 
    /// to be trustworthy. BinaryFormatter is insecure and can't be made secure.
    /// The following serializers all perform unrestricted polymorphic deserialization and are dangerous:
    ///  - SoapFormatter
    ///  - LosFormatter
    ///  - NetDataContractSerializer
    ///  - ObjectStateFormatter
    ///  - BinaryFormatter
    /// </summary>
    /// <remarks>
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
    /// </remarks>
    public static List<T> DeepCopyWithoutICloneable<T>(this List<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (List<T>)formatter.Deserialize(stream);
        }
    }

    public static List<string> ExtractUrls(this string text)
    {
        List<string> urls = new List<string>();
        Regex urlRx = new Regex(@"((https?|ftp|file)\://|www\.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
        MatchCollection matches = urlRx.Matches(text);
        foreach (Match match in matches) { urls.Add(match.Value); }
        return urls;
    }

    public static async Task LaunchUrlFromTextBox(Microsoft.UI.Xaml.Controls.TextBox textBox)
    {
        string text = "";
        textBox.DispatcherQueue.TryEnqueue(() => { text = textBox.Text; });
        Uri? uriResult;
        bool isValidUrl = Uri.TryCreate(text, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (isValidUrl)
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        else
            await Task.CompletedTask;
    }

    public static async Task LocateAndLaunchUrlFromTextBox(Microsoft.UI.Xaml.Controls.TextBox textBox)
    {
        string text = "";
        textBox.DispatcherQueue.TryEnqueue(() => { text = textBox.Text; });
        List<string> urls = text.ExtractUrls();
        if (urls.Count > 0)
        {
            Uri uriResult = new Uri(urls[0]);
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        }
        else
            await Task.CompletedTask;
    }

    public static async Task LocateAndLaunchUrlFromString(string text)
    {
        List<string> urls = text.ExtractUrls();
        if (urls.Count > 0)
        {
            Uri uriResult = new Uri(urls[0]);
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        }
        else
            await Task.CompletedTask;
    }

    #region [Duplicate Helpers]
    /// <summary>
    /// Returns a <see cref="Tuple{T1, T2}"/> representing the <paramref name="list"/>
    /// where <b>Item1</b> is the clean set and <b>Item2</b> is the duplicate set.
    /// </summary>
    public static (List<T>, List<T>) RemoveDuplicates<T>(this List<T> list)
    {
        HashSet<T> seen = new HashSet<T>();
        List<T> dupes = new List<T>();
        List<T> clean = new List<T>();
        foreach (T item in list)
        {
            if (seen.Contains(item))
            {
                dupes.Add(item);
            }
            else
            {
                seen.Add(item);
                clean.Add(item);
            }
        }
        return (clean, dupes);
    }

    /// <summary>
    /// Returns a <see cref="Tuple{T1, T2}"/> representing the <paramref name="enumerable"/>
    /// where <b>Item1</b> is the clean set and <b>Item2</b> is the duplicate set.
    /// </summary>
    public static (List<T>, List<T>) RemoveDuplicates<T>(this IEnumerable<T> enumerable)
    {
        HashSet<T> seen = new HashSet<T>();
        List<T> dupes = new List<T>();
        List<T> clean = new List<T>();
        foreach (T item in enumerable)
        {
            if (seen.Contains(item))
            {
                dupes.Add(item);
            }
            else
            {
                seen.Add(item);
                clean.Add(item);
            }
        }
        return (clean, dupes);
    }

    /// <summary>
    /// Returns a <see cref="Tuple{T1, T2}"/> representing the <paramref name="array"/>
    /// where <b>Item1</b> is the clean set and <b>Item2</b> is the duplicate set.
    /// </summary>
    public static (T[], T[]) RemoveDuplicates<T>(this T[] array)
    {
        HashSet<T> seen = new HashSet<T>();
        List<T> dupes = new List<T>();
        List<T> clean = new List<T>();
        foreach (T item in array)
        {
            if (seen.Contains(item))
            {
                dupes.Add(item);
            }
            else
            {
                seen.Add(item);
                clean.Add(item);
            }
        }
        return (clean.ToArray(), dupes.ToArray());
    }

    /// <summary>
    /// Returns a <see cref="IEnumerable{T}"/> representing the <paramref name="input"/> with duplicates removed.
    /// </summary>
    public static IEnumerable<T> DedupeUsingHashSet<T>(this IEnumerable<T> input)
    {
        if (input == null)
            yield return (T)Enumerable.Empty<T>();

        var values = new HashSet<T>();
        foreach (T item in input)
        {
            // The add function returns false if the item already exists.
            if (values.Add(item))
                yield return item;
        }
    }

    /// <summary>
    /// Returns a <see cref="List{T}"/> representing the <paramref name="input"/> with duplicates removed.
    /// </summary>
    public static List<T> DedupeUsingLINQ<T>(this List<T> input)
    {
        if (input == null)
            return new List<T>();

        return input.Distinct().ToList();
    }

    /// <summary>
    /// Returns a <see cref="List{T}"/> representing the <paramref name="input"/> with duplicates removed.
    /// </summary>
    public static List<T> DedupeUsingHashSet<T>(this List<T> input)
    {
        if (input == null)
            return new List<T>();

        return (new HashSet<T>(input)).ToList();
    }

    /// <summary>
    /// Returns a <see cref="List{T}"/> representing the <paramref name="input"/> with duplicates removed.
    /// </summary>
    public static List<T> DedupeUsingDictionary<T>(this List<T> input)
    {
        if (input == null)
            return new List<T>();

        Dictionary<T, bool> seen = new Dictionary<T, bool>();
        List<T> result = new List<T>();

        foreach (T item in input)
        {
            if (!seen.ContainsKey(item))
            {
                seen[item] = true;
                result.Add(item);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns true if the <paramref name="input"/> contains duplicates, false otherwise.
    /// </summary>
    public static bool HasDuplicates<T>(this IEnumerable<T> input)
    {
        var knownKeys = new HashSet<T>();
        return input.Any(item => !knownKeys.Add(item));
    }

    /// <summary>
    /// Returns true if the <paramref name="input"/> contains duplicates, false otherwise.
    /// </summary>
    public static bool HasDuplicates<T>(this List<T> input)
    {
        var knownKeys = new HashSet<T>();
        return input.Any(item => !knownKeys.Add(item));
    }
    #endregion

    /// <summary>
    /// Determine if one number is greater than another.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>
    /// True if the first number is greater than the second, false otherwise.
    /// </returns>
    public static bool IsGreaterThan(double left, double right)
    {
        return (left > right) && !AreClose(left, right);
    }

    /// <summary>
    /// Determine if one number is less than or close to another.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>
    /// True if the first number is less than or close to the second, false otherwise.
    /// </returns>
    public static bool IsLessThanOrClose(double left, double right)
    {
        return (left < right) || AreClose(left, right);
    }

    /// <summary>
    /// Determine if two numbers are close in value.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>
    /// True if the first number is close in value to the second, false otherwise.
    /// </returns>
    public static bool AreClose(double left, double right)
    {
        if (left == right)
        {
            return true;
        }

        double a = (Math.Abs(left) + Math.Abs(right) + 10.0) * Epsilon;
        double b = left - right;
        return (-a < b) && (a > b);
    }

    /// <summary>
    /// Check if a number is zero.
    /// </summary>
    /// <param name="value">The number to check.</param>
    /// <returns>
    /// True if the number is zero, false otherwise.
    /// </returns>
    public static bool IsZero(this double value)
    {
        // We actually consider anything within an order of magnitude of epsilon to be zero
        return Math.Abs(value) < Epsilon;
    }
    public const double Epsilon = 0.000000000001;

    public static bool IsInvalid(this double value)
    {
        if (value == double.NaN || value == double.NegativeInfinity || value == double.PositiveInfinity)
            return true;

        return false;
    }

    #region [Helper for CommunityToolkit]
    /// <summary>
    /// <para>
    /// Gets the image data from a Uri.
    /// </para>
    /// <para>
    /// The issue with many of the CommunityToolkit file access routines is that they do not
    /// handle unpackaged apps, so you will see I added logic switches for most of these methods.
    /// </para>
    /// </summary>
    /// <param name="uri">Image Uri</param>
    /// <returns>Image Stream as <see cref="IRandomAccessStream"/></returns>
    public static async Task<IRandomAccessStream?> GetImageStream(this Uri uri)
    {
        IRandomAccessStream? imageStream = null;
        string localPath = string.Empty;
        if (uri.LocalPath.StartsWith("\\\\"))
            localPath = $"{uri.LocalPath}".Replace("//", "/");
        else
            localPath = $"{uri.Host}/{uri.LocalPath}".Replace("//", "/");

        // If we don't have internet, then try to see if we have a packaged copy
        try
        {
            if (App.IsPackaged)
            {
                /*
                    "StreamHelper.GetPackagedFileStreamAsync" contains the following...
                    StorageFolder workingFolder = Package.Current.InstalledLocation;
                    return GetFileStreamAsync(fileName, accessMode, workingFolder);
                */
                imageStream = await CommunityToolkit.WinUI.Helpers.StreamHelper.GetPackagedFileStreamAsync(localPath);
            }
            else
            {
                /*
                    "StreamHelper.GetLocalFileStreamAsync" contains the following...
                    StorageFolder workingFolder = ApplicationData.Current.LocalFolder;
                    return GetFileStreamAsync(fileName, accessMode, workingFolder);
                */
                imageStream = await CommunityToolkit.WinUI.Helpers.StreamHelper.GetLocalFileStreamAsync(localPath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[INFO] {localPath}");
            Debug.WriteLine($"[WARNING] GetImageStream: {ex.Message}");
        }

        return imageStream;
    }
    #endregion

    /// <summary>
    /// Returns the <see cref="Microsoft.UI.Xaml.PropertyPath"/> based on the provided <see cref="Microsoft.UI.Xaml.Data.Binding"/>.
    /// </summary>
    public static string? GetBindingPropertyName(this Microsoft.UI.Xaml.Data.Binding binding)
    {
        return binding?.Path?.Path?.Split('.')?.LastOrDefault();
    }

    public static Windows.Foundation.Size GetTextSize(FontFamily font, double fontSize, string text)
    {
        var tb = new TextBlock { Text = text, FontFamily = font, FontSize = fontSize };
        tb.Measure(new Windows.Foundation.Size(Double.PositiveInfinity, Double.PositiveInfinity));
        return tb.DesiredSize;
    }

    public static bool IsMonospacedFont(FontFamily font)
    {
        var tb1 = new TextBlock { Text = "(!aiZ%#BIm,. ~`", FontFamily = font };
        tb1.Measure(new Windows.Foundation.Size(Double.PositiveInfinity, Double.PositiveInfinity));
        var tb2 = new TextBlock { Text = "...............", FontFamily = font };
        tb2.Measure(new Windows.Foundation.Size(Double.PositiveInfinity, Double.PositiveInfinity));
        var off = Math.Abs(tb1.DesiredSize.Width - tb2.DesiredSize.Width);
        return off < 0.01;
    }

    /// <summary>
    /// Gets a list of the specified FrameworkElement's DependencyProperties. This method will return all
    /// DependencyProperties of the element unless 'useBlockList' is true, in which case all bindings on elements
    /// that are typically not used as input controls will be ignored.
    /// </summary>
    /// <param name="element">FrameworkElement of interest</param>
    /// <param name="useBlockList">If true, ignores elements not typically used for input</param>
    /// <returns>List of DependencyProperties</returns>
    public static List<DependencyProperty> GetDependencyProperties(this FrameworkElement element, bool useBlockList)
    {
        List<DependencyProperty> dependencyProperties = new List<DependencyProperty>();

        bool isBlocklisted = useBlockList &&
            (element is Panel || element is Button || element is Image || element is ScrollViewer ||
             element is TextBlock || element is Border || element is Microsoft.UI.Xaml.Shapes.Shape || element is ContentPresenter);

        if (!isBlocklisted)
        {
            Type type = element.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(DependencyProperty))
                {
                    var dp = (DependencyProperty)field.GetValue(null);
                    if (dp != null)
                        dependencyProperties.Add(dp);
                }
            }
        }

        return dependencyProperties;
    }

    public static bool IsXamlRootAvailable(bool UWP = false)
    {
        if (UWP)
            return Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot");
        else
            return Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Microsoft.UI.Xaml.UIElement", "XamlRoot");
    }

    /// <summary>
    /// Helper function to calculate an element's rectangle in root-relative coordinates.
    /// </summary>
    public static Windows.Foundation.Rect GetElementRect(this Microsoft.UI.Xaml.FrameworkElement element)
    {
        try
        {
            Microsoft.UI.Xaml.Media.GeneralTransform transform = element.TransformToVisual(null);
            Windows.Foundation.Point point = transform.TransformPoint(new Windows.Foundation.Point());
            return new Windows.Foundation.Rect(point, new Windows.Foundation.Size(element.ActualWidth, element.ActualHeight));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetElementRect: {ex.Message}");
            return new Windows.Foundation.Rect(0, 0, 0, 0);
        }
    }

    public static IconElement? GetIcon(string imagePath, string imageExt = ".png")
    {
        IconElement? result = null;

        try
        {
            result = imagePath.ToLowerInvariant().EndsWith(imageExt) ?
                        (IconElement)new BitmapIcon() { UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute), ShowAsMonochrome = false } :
                        (IconElement)new FontIcon() { Glyph = imagePath };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] {MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
        }

        return result;
    }

    public static FontIcon GenerateFontIcon(Windows.UI.Color brush, string glyph = "\uF127", int width = 10, int height = 10)
    {
        return new FontIcon()
        {
            Glyph = glyph,
            FontSize = 1.5,
            Width = (double)width,
            Height = (double)height,
            Foreground = new SolidColorBrush(brush),
        };
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
    /// This is a redundant call from App.xaml.cs, but is here if you need it.
    /// </summary>
    /// <param name="window"><see cref="Microsoft.UI.Xaml.Window"/></param>
    /// <returns><see cref="Microsoft.UI.Windowing.AppWindow"/></returns>
    public static Microsoft.UI.Windowing.AppWindow GetAppWindow(this Microsoft.UI.Xaml.Window window)
    {
        System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        Microsoft.UI.WindowId wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
    }

    /// <summary>
    /// This assumes your images reside in an "Assets" folder.
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns><see cref="BitmapImage"/></returns>
    public static BitmapImage? GetImageFromAssets(this string assetName)
    {
        BitmapImage? img = null;

        try
        {
            Uri? uri = new Uri("ms-appx:///Assets/" + assetName.Replace("./", ""));
            img = new BitmapImage(uri);
            Debug.WriteLine($"[INFO] Image resolved for '{assetName}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARNING] GetImageFromAssets: {ex.Message}");
        }

        return img;
    }

    /// <summary>
    /// Creates a <see cref="LinearGradientBrush"/> from 3 input colors.
    /// </summary>
    /// <param name="c1">offset 0.0 color</param>
    /// <param name="c2">offset 0.5 color</param>
    /// <param name="c3">offset 1.0 color</param>
    /// <returns><see cref="LinearGradientBrush"/></returns>
    public static LinearGradientBrush CreateLinearGradientBrush(Windows.UI.Color c1, Windows.UI.Color c2, Windows.UI.Color c3)
    {
        var gs1 = new GradientStop(); gs1.Color = c1; gs1.Offset = 0.0;
        var gs2 = new GradientStop(); gs2.Color = c2; gs2.Offset = 0.5;
        var gs3 = new GradientStop(); gs3.Color = c3; gs3.Offset = 1.0;
        var gsc = new GradientStopCollection();
        gsc.Add(gs1); gsc.Add(gs2); gsc.Add(gs3);
        var lgb = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(0, 1),
            GradientStops = gsc
        };
        return lgb;
    }

    /// <summary>
    /// Creates a Color object from the hex color code and returns the result.
    /// </summary>
    /// <param name="hexColorCode">text representation of the color</param>
    /// <returns><see cref="Windows.UI.Color"/></returns>
    public static Windows.UI.Color? GetColorFromHexString(string hexColorCode)
    {
        if (string.IsNullOrEmpty(hexColorCode))
            return null;

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
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Uses the <see cref="System.Reflection.PropertyInfo"/> of the 
    /// <see cref="Microsoft.UI.Colors"/> class to return the matching 
    /// <see cref="Windows.UI.Color"/> object.
    /// </summary>
    /// <param name="colorName">name of color, e.g. "Aquamarine"</param>
    /// <returns><see cref="Windows.UI.Color"/></returns>
    public static Windows.UI.Color? GetColorFromNameString(string colorName)
    {
        if (string.IsNullOrEmpty(colorName))
            return Windows.UI.Color.FromArgb(255, 128, 128, 128);

        try
        {
            var prop = typeof(Microsoft.UI.Colors).GetTypeInfo().GetDeclaredProperty(colorName);
            if (prop != null)
            {
                var tmp = prop.GetValue(null);
                if (tmp != null)
                    return (Windows.UI.Color)tmp;
            }
            else
            {
                Debug.WriteLine($"[WARNING] \"{colorName}\" could not be resolved as a {nameof(Windows.UI.Color)}.");
            }

            return Windows.UI.Color.FromArgb(255, 128, 128, 128);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetColorFromNameString: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns the given <see cref="Windows.UI.Color"/> as a hex string.
    /// </summary>
    /// <param name="color">color to convert</param>
    /// <returns>hex string (including pound sign)</returns>
    public static string ToHexString(this Windows.UI.Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Calculates the linear interpolated Color based on the given Color values.
    /// </summary>
    /// <param name="colorFrom">Source Color.</param>
    /// <param name="colorTo">Target Color.</param>
    /// <param name="amount">Weightage given to the target color.</param>
    /// <returns>Linear Interpolated Color.</returns>
    public static Windows.UI.Color Lerp(this Windows.UI.Color colorFrom, Windows.UI.Color colorTo, float amount)
    {
        // Convert colorFrom components to lerp-able floats
        float sa = colorFrom.A, sr = colorFrom.R, sg = colorFrom.G, sb = colorFrom.B;

        // Convert colorTo components to lerp-able floats
        float ea = colorTo.A, er = colorTo.R, eg = colorTo.G, eb = colorTo.B;

        // lerp the colors to get the difference
        byte a = (byte)Math.Max(0, Math.Min(255, sa.Lerp(ea, amount))),
             r = (byte)Math.Max(0, Math.Min(255, sr.Lerp(er, amount))),
             g = (byte)Math.Max(0, Math.Min(255, sg.Lerp(eg, amount))),
             b = (byte)Math.Max(0, Math.Min(255, sb.Lerp(eb, amount)));

        // return the new color
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Darkens the color by the given percentage using lerp.
    /// </summary>
    /// <param name="color">Source color.</param>
    /// <param name="amount">Percentage to darken. Value should be between 0 and 1.</param>
    /// <returns>Color</returns>
    public static Windows.UI.Color DarkerBy(this Windows.UI.Color color, float amount)
    {
        return color.Lerp(Colors.Black, amount);
    }

    /// <summary>
    /// Lightens the color by the given percentage using lerp.
    /// </summary>
    /// <param name="color">Source color.</param>
    /// <param name="amount">Percentage to lighten. Value should be between 0 and 1.</param>
    /// <returns>Color</returns>
    public static Windows.UI.Color LighterBy(this Windows.UI.Color color, float amount)
    {
        return color.Lerp(Colors.White, amount);
    }

    /// <summary>
    /// Multiply color bytes by <paramref name="factor"/>, default value is 1.5
    /// </summary>
    public static Windows.UI.Color LightenColor(this Windows.UI.Color source, float factor = 1.5F)
    {
        var red = (int)((float)source.R * factor);
        var green = (int)((float)source.G * factor);
        var blue = (int)((float)source.B * factor);

        if (red == 0)   { red = 0x0F;   } else if (red > 255)   { red = 0xFF;   }
        if (green == 0) { green = 0x0F; } else if (green > 255) { green = 0xFF; }
        if (blue == 0)  { blue = 0x0F;  } else if (blue > 255)  { blue = 0xFF;  }

        return Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue);
    }

    /// <summary>
    /// Divide color bytes by <paramref name="factor"/>, default value is 1.5
    /// </summary>
    public static Windows.UI.Color DarkenColor(this Windows.UI.Color source, float factor = 1.5F)
    {
        if (source.R == 0) { source.R = 2; }
        if (source.G == 0) { source.G = 2; }
        if (source.B == 0) { source.B = 2; }

        var red = (int)((float)source.R / factor);
        var green = (int)((float)source.G / factor);
        var blue = (int)((float)source.B / factor);

        return Windows.UI.Color.FromArgb((byte)255, (byte)red, (byte)green, (byte)blue);
    }

    /// <summary>
    /// Generates a completely random <see cref="Windows.UI.Color"/>.
    /// </summary>
    /// <returns><see cref="Windows.UI.Color"/></returns>
    public static Windows.UI.Color GetRandomWinUIColor()
    {
        byte[] buffer = new byte[3];
        Random.Shared.NextBytes(buffer);
        return Windows.UI.Color.FromArgb(255, buffer[0], buffer[1], buffer[2]);
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
                .Where(p => p.PropertyType == typeof(Windows.UI.Color) && p.GetMethod.IsStatic && p.GetMethod.IsPublic)
                .Select(p => (Windows.UI.Color)p.GetValue(null))
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
            Debug.WriteLine($"[ERROR] GetRandomColor: {ex.Message}");
            return Microsoft.UI.Colors.Red;
        }
    }


    /// <summary>
    /// Creates a Color from the hex color code and returns the result 
    /// as a <see cref="Microsoft.UI.Xaml.Media.SolidColorBrush"/>.
    /// </summary>
    /// <param name="hexColorCode">text representation of the color</param>
    /// <returns><see cref="Microsoft.UI.Xaml.Media.SolidColorBrush"/></returns>
    public static Microsoft.UI.Xaml.Media.SolidColorBrush? GetBrushFromHexString(string hexColorCode)
    {
        if (string.IsNullOrEmpty(hexColorCode))
            return null;

        try
        {
            byte a = 255; byte r = 0; byte g = 0; byte b = 0;

            if (hexColorCode.Length == 9)
                hexColorCode = hexColorCode.Substring(1, 8);

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

            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetBrushFromHexString: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verifies if the given brush is a SolidColorBrush and its color does not include transparency.
    /// </summary>
    /// <param name="brush">Brush</param>
    /// <returns>true if yes, otherwise false</returns>
    public static bool IsOpaqueSolidColorBrush(this Microsoft.UI.Xaml.Media.Brush brush)
    {
        return (brush as Microsoft.UI.Xaml.Media.SolidColorBrush)?.Color.A == 0xff;
    }

    /// <summary>
    /// Finds the contrast ratio.
    /// This is helpful for determining if one control's foreground and another control's background will be hard to distinguish.
    /// https://www.w3.org/WAI/GL/wiki/Contrast_ratio
    /// (L1 + 0.05) / (L2 + 0.05), where
    /// L1 is the relative luminance of the lighter of the colors, and
    /// L2 is the relative luminance of the darker of the colors.
    /// </summary>
    /// <param name="first"><see cref="Windows.UI.Color"/></param>
    /// <param name="second"><see cref="Windows.UI.Color"/></param>
    /// <returns>ratio between relative luminance</returns>
    public static double CalculateContrastRatio(Windows.UI.Color first, Windows.UI.Color second)
    {
        double relLuminanceOne = GetRelativeLuminance(first);
        double relLuminanceTwo = GetRelativeLuminance(second);
        return (Math.Max(relLuminanceOne, relLuminanceTwo) + 0.05) / (Math.Min(relLuminanceOne, relLuminanceTwo) + 0.05);
    }

    /// <summary>
    /// Gets the relative luminance.
    /// https://www.w3.org/WAI/GL/wiki/Relative_luminance
    /// For the sRGB colorspace, the relative luminance of a color is defined as L = 0.2126 * R + 0.7152 * G + 0.0722 * B
    /// </summary>
    /// <param name="c"><see cref="Windows.UI.Color"/></param>
    public static double GetRelativeLuminance(Windows.UI.Color c)
    {
        double rSRGB = c.R / 255.0;
        double gSRGB = c.G / 255.0;
        double bSRGB = c.B / 255.0;

        // WebContentAccessibilityGuideline 2.x definition was 0.03928 (incorrect)
        // WebContentAccessibilityGuideline 3.x definition is 0.04045 (correct)
        double r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow(((rSRGB + 0.055) / 1.055), 2.4);
        double g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow(((gSRGB + 0.055) / 1.055), 2.4);
        double b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow(((bSRGB + 0.055) / 1.055), 2.4);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    /// <summary>
    /// Returns a new <see cref="Windows.Foundation.Rect(double, double, double, double)"/> representing the size of the <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vector"><see cref="System.Numerics.Vector2"/> vector representing object size for Rectangle.</param>
    /// <returns><see cref="Windows.Foundation.Rect(double, double, double, double)"/> value.</returns>
    public static Windows.Foundation.Rect ToRect(this System.Numerics.Vector2 vector)
    {
        return new Windows.Foundation.Rect(0, 0, vector.X, vector.Y);
    }

    /// <summary>
    /// Returns a new <see cref="System.Numerics.Vector2"/> representing the <see cref="Windows.Foundation.Size(double, double)"/>.
    /// </summary>
    /// <param name="size"><see cref="Windows.Foundation.Size(double, double)"/> value.</param>
    /// <returns><see cref="System.Numerics.Vector2"/> value.</returns>
    public static System.Numerics.Vector2 ToVector2(this Windows.Foundation.Size size)
    {
        return new System.Numerics.Vector2((float)size.Width, (float)size.Height);
    }

    /// <summary>
    /// Deflates rectangle by given thickness.
    /// </summary>
    /// <param name="rect">Rectangle</param>
    /// <param name="thick">Thickness</param>
    /// <returns>Deflated Rectangle</returns>
    public static Windows.Foundation.Rect Deflate(this Windows.Foundation.Rect rect, Microsoft.UI.Xaml.Thickness thick)
    {
        return new Windows.Foundation.Rect(
            rect.Left + thick.Left,
            rect.Top + thick.Top,
            Math.Max(0.0, rect.Width - thick.Left - thick.Right),
            Math.Max(0.0, rect.Height - thick.Top - thick.Bottom));
    }

    /// <summary>
    /// Inflates rectangle by given thickness.
    /// </summary>
    /// <param name="rect">Rectangle</param>
    /// <param name="thick">Thickness</param>
    /// <returns>Inflated Rectangle</returns>
    public static Windows.Foundation.Rect Inflate(this Windows.Foundation.Rect rect, Microsoft.UI.Xaml.Thickness thick)
    {
        return new Windows.Foundation.Rect(
            rect.Left - thick.Left,
            rect.Top - thick.Top,
            Math.Max(0.0, rect.Width + thick.Left + thick.Right),
            Math.Max(0.0, rect.Height + thick.Top + thick.Bottom));
    }

    /// <summary>
    /// Starts an <see cref="Microsoft.UI.Composition.ExpressionAnimation"/> to keep the size of the source <see cref="Microsoft.UI.Composition.CompositionObject"/> in sync with the target <see cref="UIElement"/>
    /// </summary>
    /// <param name="source">The <see cref="Microsoft.UI.Composition.CompositionObject"/> to start the animation on</param>
    /// <param name="target">The target <see cref="UIElement"/> to read the size updates from</param>
    public static void BindSize(this Microsoft.UI.Composition.CompositionObject source, UIElement target)
    {
        var visual = ElementCompositionPreview.GetElementVisual(target);
        var bindSizeAnimation = source.Compositor.CreateExpressionAnimation($"{nameof(visual)}.Size");
        bindSizeAnimation.SetReferenceParameter(nameof(visual), visual);
        // Start the animation
        source.StartAnimation("Size", bindSizeAnimation);
    }

    /// <summary>
    /// Starts an animation on the given property of a <see cref="Microsoft.UI.Composition.CompositionObject"/>
    /// </summary>
    /// <typeparam name="T">The type of the property to animate</typeparam>
    /// <param name="target">The target <see cref="Microsoft.UI.Composition.CompositionObject"/></param>
    /// <param name="property">The name of the property to animate</param>
    /// <param name="value">The final value of the property</param>
    /// <param name="duration">The animation duration</param>
    /// <returns>A <see cref="Task"/> that completes when the created animation completes</returns>
    public static Task StartAnimationAsync<T>(this Microsoft.UI.Composition.CompositionObject target, string property, T value, TimeSpan duration) where T : unmanaged
    {
        // Stop previous animations
        target.StopAnimation(property);

        // Setup the animation to run
        Microsoft.UI.Composition.KeyFrameAnimation animation;

        // Switch on the value to determine the necessary KeyFrameAnimation type
        switch (value)
        {
            case float f:
                var scalarAnimation = target.Compositor.CreateScalarKeyFrameAnimation();
                scalarAnimation.InsertKeyFrame(1f, f);
                animation = scalarAnimation;
                break;
            case Windows.UI.Color c:
                var colorAnimation = target.Compositor.CreateColorKeyFrameAnimation();
                colorAnimation.InsertKeyFrame(1f, c);
                animation = colorAnimation;
                break;
            case System.Numerics.Vector4 v4:
                var vector4Animation = target.Compositor.CreateVector4KeyFrameAnimation();
                vector4Animation.InsertKeyFrame(1f, v4);
                animation = vector4Animation;
                break;
            default: throw new ArgumentException($"Invalid animation type: {typeof(T)}", nameof(value));
        }

        animation.Duration = duration;

        // Get the batch and start the animations
        var batch = target.Compositor.CreateScopedBatch(Microsoft.UI.Composition.CompositionBatchTypes.Animation);

        // Create a TCS for the result
        var tcs = new TaskCompletionSource<object>();

        batch.Completed += (s, e) => tcs.SetResult(null);

        target.StartAnimation(property, animation);

        batch.End();

        return tcs.Task;
    }

    /// <summary>
    /// Creates a <see cref="Microsoft.UI.Composition.CompositionGeometricClip"/> from the specified <see cref="Windows.Graphics.IGeometrySource2D"/>.
    /// </summary>
    /// <param name="compositor"><see cref="Microsoft.UI.Composition.Compositor"/></param>
    /// <param name="geometry"><see cref="Windows.Graphics.IGeometrySource2D"/></param>
    /// <returns>CompositionGeometricClip</returns>
    public static Microsoft.UI.Composition.CompositionGeometricClip CreateGeometricClip(this Microsoft.UI.Composition.Compositor compositor, Windows.Graphics.IGeometrySource2D geometry)
    {
        // Create the CompositionPath
        var path = new Microsoft.UI.Composition.CompositionPath(geometry);
        // Create the CompositionPathGeometry
        var pathGeometry = compositor.CreatePathGeometry(path);
        // Create the CompositionGeometricClip
        return compositor.CreateGeometricClip(pathGeometry);
    }

    /// <summary>
    /// Returns whether the bit at the specified position is set.
    /// </summary>
    /// <typeparam name="T">Any integer type.</typeparam>
    /// <param name="t">The value to check.</param>
    /// <param name="pos">The position of the bit to check, 0 refers to the least significant bit.</param>
    /// <returns>true if the specified bit is on, otherwise false.</returns>
    public static bool IsBitSet<T>(this T t, int pos) where T : struct, IConvertible
    {
        var value = t.ToInt64(CultureInfo.CurrentCulture);
        return (value & (1 << pos)) != 0;
    }

    /// <summary>
    /// Converts a unsigned integer into a <see cref="Windows.UI.Color"/>.
    /// </summary>
    /// <param name="rgb">Blue: 0x2563EB</param>
    /// <returns><see cref="Windows.UI.Color"/></returns>
    public static Windows.UI.Color ConvertToColor(this uint rgb)
    {
        Windows.UI.Color color = default;
        color.A = 0xFF;
        color.R = (byte)(rgb >> 16);
        color.G = (byte)((rgb >> 8) & 0x000000FF);
        color.B = (byte)(rgb & 0x000000FF);
        return color;
    }

    /// <summary>
    /// Returns a <see cref="Windows.UI.Color"/> based on the window of time met from the initial task.
    /// </summary>
    public static InfoBarSeverity GetInfoBarSeverity(string value, TimeSpan? amount)
    {
        if (string.IsNullOrEmpty(value) || amount == null)
            return InfoBarSeverity.Informational;

        switch (value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_YEAR_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.ONE_YEAR_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.ONE_YEAR_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.SIX_MONTH_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.SIX_MONTH_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.SIX_MONTH_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_MONTH_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.ONE_MONTH_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.ONE_MONTH_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("two months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TWO_MONTH_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.TWO_MONTH_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.TWO_MONTH_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TWO_WEEK_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.TWO_WEEK_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.TWO_WEEK_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_WEEK_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.ONE_WEEK_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.ONE_WEEK_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.FEW_DAYS_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.FEW_DAYS_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.FEW_DAYS_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TOMORROW_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.TOMORROW_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.TOMORROW_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.SOON_MIN)
                        return InfoBarSeverity.Success;       // green
                    else if (amount?.TotalDays < Constants.SOON_MID)
                        return InfoBarSeverity.Informational; // yellow
                    else if (amount?.TotalDays < Constants.SOON_MAX)
                        return InfoBarSeverity.Warning;       // orange
                    else
                        return InfoBarSeverity.Error;         // red
                }
            default:
                return InfoBarSeverity.Informational;
        }
    }

    /// <summary>
    /// Returns a <see cref="Windows.UI.Color"/> based on the window of time met from the initial task.
    /// </summary>
    public static Windows.UI.Color GetColorTime(string value, TimeSpan? amount)
    {
        if (string.IsNullOrEmpty(value) || amount == null)
            return Windows.UI.Color.FromArgb(255, 75, 10, 255);

        switch (value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_YEAR_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.ONE_YEAR_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.ONE_YEAR_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.SIX_MONTH_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.SIX_MONTH_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.SIX_MONTH_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_MONTH_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.ONE_MONTH_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.ONE_MONTH_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("two months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TWO_MONTH_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.TWO_MONTH_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.TWO_MONTH_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TWO_WEEK_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.TWO_WEEK_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.TWO_WEEK_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.ONE_WEEK_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.ONE_WEEK_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.ONE_WEEK_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.FEW_DAYS_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.FEW_DAYS_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.FEW_DAYS_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.TOMORROW_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.TOMORROW_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.TOMORROW_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < Constants.SOON_MIN)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < Constants.SOON_MID)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < Constants.SOON_MAX)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            default:
                return Windows.UI.Color.FromArgb(255, 75, 10, 255);          // purple
        }
    }

    public static bool IsCompatible(this Version v, Version minimumVersion, Version exactVersion = null)
    {
        if (exactVersion != null)
            return v.Equals(exactVersion);

        return v >= minimumVersion;
    }

    /// <summary>
    /// This method will find all occurrences of a string pattern that starts with a double 
    /// quote, followed by any number of characters (non-greedy), and ends with a double 
    /// quote followed by zero or more spaces and a colon. This pattern matches the typical 
    /// format of keys in a JSON string.
    /// </summary>
    /// <param name="jsonString">JSON formatted text</param>
    /// <returns><see cref="List{T}"/> of each key</returns>
    public static List<string> ExtractKeys(string jsonString)
    {
        var keys = new List<string>();
        var matches = Regex.Matches(jsonString, "[,\\{]\"(.*?)\"\\s*:");
        foreach (Match match in matches) { keys.Add(match.Groups[1].Value); }
        return keys;
    }

    /// <summary>
    /// This method will find all occurrences of a string pattern that starts with a colon, 
    /// followed by zero or more spaces, followed by any number of characters (non-greedy), 
    /// and ends with a comma, closing brace, or closing bracket. This pattern matches the 
    /// typical format of values in a JSON string.
    /// </summary>
    /// <param name="jsonString">JSON formatted text</param>
    /// <returns><see cref="List{T}"/> of each value</returns>
    public static List<string> ExtractValues(string jsonString)
    {
        var values = new List<string>();
        var matches = Regex.Matches(jsonString, ":\\s*(.*?)(,|}|\\])");
        foreach (Match match in matches) { values.Add(match.Groups[1].Value.Trim()); }
        return values;
    }

    /// <summary>
    /// var stack = GeneralExtensions.GetStackTrace(new StackTrace());
    /// </summary>
    public static string GetStackTrace(StackTrace st)
    {
        string result = string.Empty;
        for (int i = 0; i < st.FrameCount; i++)
        {
            StackFrame? sf = st.GetFrame(i);
            result += sf?.GetMethod() + " <== ";
        }
        return result;
    }

    public static string Flatten(this Exception? exception)
    {
        var sb = new StringBuilder();
        while (exception != null)
        {
            sb.AppendLine(exception.Message);
            sb.AppendLine(exception.StackTrace);
            exception = exception.InnerException;
        }
        return sb.ToString();
    }

    public static string DumpFrames(this Exception exception)
    {
        var sb = new StringBuilder();
        var st = new StackTrace(exception, true);
        var frames = st.GetFrames();
        foreach (var frame in frames)
        {
            if (frame != null)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                sb.Append($"File: {frame.GetFileName()}")
                  .Append($", Method: {frame.GetMethod()?.Name}")
                  .Append($", LineNumber: {frame.GetFileLineNumber()}")
                  .Append($"{Environment.NewLine}");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Offers two ways to determine the local app folder.
    /// </summary>
    /// <returns></returns>
    public static string LocalApplicationDataFolder()
    {
        WindowsIdentity? currentUser = WindowsIdentity.GetCurrent();
        SecurityIdentifier? currentUserSID = currentUser.User;
        SecurityIdentifier? localSystemSID = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        if (currentUserSID != null && currentUserSID.Equals(localSystemSID))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        }
        else
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }

    public static string GetSelectedText(this ComboBox comboBox)
    {
        var item = comboBox.SelectedItem as ComboBoxItem;
        if (item != null)
        {
            return (string)item.Content;
        }

        return "";
    }

    #region [Expander Extensions]
    /// <summary>
    /// Enables or disables the Header.
    /// </summary>
    public static void IsLocked(this Expander expander, bool locked)
    {
        var ctrl = (expander.Header as FrameworkElement)?.Parent as Control;
        if (ctrl != null)
            ctrl.IsEnabled = locked;
    }

    /// <summary>
    /// Sets the desired Height for content when expanded.
    /// </summary>
    public static void SetContentHeight(this Expander expander, double contentHeight)
    {
        var ctrl = expander.Content as FrameworkElement;
        if (ctrl != null)
            ctrl.Height = contentHeight;
    }
    #endregion

    public static void SetOrientation(this VirtualizingLayout layout, Orientation orientation)
    {
        // Note:
        // The public properties of UniformGridLayout and FlowLayout interpret
        // orientation the opposite to how FlowLayoutAlgorithm interprets it. 
        // For simplicity, our validation code is written in terms that match
        // the implementation. For this reason, we need to switch the orientation
        // whenever we set UniformGridLayout.Orientation or StackLayout.Orientation.
        if (layout is StackLayout)
        {
            ((StackLayout)layout).Orientation = orientation;
        }
        else if (layout is UniformGridLayout)
        {
            ((UniformGridLayout)layout).Orientation = orientation;
        }
        else
        {
            throw new InvalidOperationException("layout unknown");
        }
    }

    public static void BindCenterPoint(this Microsoft.UI.Composition.Visual target)
    {
        var exp = target.Compositor.CreateExpressionAnimation("Vector3(this.Target.Size.X / 2, this.Target.Size.Y / 2, 0f)");
        target.StartAnimation("CenterPoint", exp);
    }

    public static void BindSize(this Microsoft.UI.Composition.Visual target, Microsoft.UI.Composition.Visual source)
    {
        var exp = target.Compositor.CreateExpressionAnimation("host.Size");
        exp.SetReferenceParameter("host", source);
        target.StartAnimation("Size", exp);
    }

    public static Microsoft.UI.Composition.ImplicitAnimationCollection CreateImplicitAnimation(this Microsoft.UI.Composition.ImplicitAnimationCollection source, string Target, TimeSpan? Duration = null)
    {
        Microsoft.UI.Composition.KeyFrameAnimation animation = null;
        switch (Target.ToLower())
        {
            case "offset":
            case "scale":
            case "centerPoint":
            case "rotationAxis":
                animation = source.Compositor.CreateVector3KeyFrameAnimation();
                break;

            case "size":
                animation = source.Compositor.CreateVector2KeyFrameAnimation();
                break;

            case "opacity":
            case "blueRadius":
            case "rotationAngle":
            case "rotationAngleInDegrees":
                animation = source.Compositor.CreateScalarKeyFrameAnimation();
                break;

            case "color":
                animation = source.Compositor.CreateColorKeyFrameAnimation();
                break;
        }

        if (animation == null) throw new ArgumentNullException("Unknown Target");
        if (!Duration.HasValue) Duration = TimeSpan.FromSeconds(0.2d);
        animation.InsertExpressionKeyFrame(1f, "this.FinalValue");
        animation.Duration = Duration.Value;
        animation.Target = Target;

        source[Target] = animation;
        return source;
    }

    /// <summary>
    /// To populate parameters with a typical URI assigning format.
    /// This method assumes the format is like "mode=1,state=2,theme=dark"
    /// </summary>
    public static Dictionary<string, string> ParseAssignedValues(string inputString, string delimiter = ",")
    {
        Dictionary<string, string> parameters = new();

        try
        {
            var parts = inputString.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            parameters = parts.Select(x => x.Split("=")).ToDictionary(x => x.First(), x => x.Last());
        }
        catch (Exception ex) { Debug.WriteLine($"ParseAssignedValues: {ex.Message}"); }

        return parameters;
    }

    /// <summary>
    /// Dictionary<char, int> charCount = GetCharacterCount("some input text string here");
    /// foreach (var kvp in charCount) { Debug.WriteLine($"Character: {kvp.Key}, Count: {kvp.Value}"); }
    /// </summary>
    /// <param name="input">the text string to analyze</param>
    /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
    public static Dictionary<char, int> GetCharacterCount(this string input)
    {
        Dictionary<char, int> charCount = new();

        if (string.IsNullOrEmpty(input))
            return charCount;

        foreach (var ch in input)
        {
            if (charCount.ContainsKey(ch))
                charCount[ch]++;
            else
                charCount[ch] = 1;
        }

        return charCount;
    }

    /// <summary>
    /// Gets all the paragraphs in a given markdown document.
    /// </summary>
    /// <param name="text">The input markdown document.</param>
    /// <returns>The raw paragraphs from <paramref name="text"/>.</returns>
    public static IReadOnlyDictionary<string, string> GetParagraphs(this string text)
    {
        return Regex.Matches(text, @"(?<=\W)#+ ([^\n]+).+?(?=\W#|$)", RegexOptions.Singleline)
           .OfType<Match>()
           .ToDictionary(
             m => Regex.Replace(m.Groups[1].Value.Trim().Replace("&lt;", "<"), @"\[([^]]+)\]\([^)]+\)", m => m.Groups[1].Value),
             m => m.Groups[0].Value.Trim().Replace("&lt;", "<").Replace("[!WARNING]", "**WARNING:**").Replace("[!NOTE]", "**NOTE:**"));
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

    public static T? ParseEnum<T>(this string value)
    {
        try
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    public static TEnum GetEnum<TEnum>(this string text) where TEnum : struct
    {
        if (!typeof(TEnum).GetTypeInfo().IsEnum)
            throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");

        return (TEnum)Enum.Parse(typeof(TEnum), text);
    }

    /// <summary>
    /// Clamping function for any value of type <see cref="IComparable{T}"/>.
    /// </summary>
    /// <param name="val">initial value</param>
    /// <param name="min">lowest range</param>
    /// <param name="max">highest range</param>
    /// <returns>clamped value</returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        return val.CompareTo(min) < 0 ? min : (val.CompareTo(max) > 0 ? max : val);
    }

    /// <summary>
    /// Scales a range of double. [baseMin to baseMax] will become [limitMin to limitMax]
    /// </summary>
    public static double Scale(this double valueIn, double baseMin, double baseMax, double limitMin, double limitMax) => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
    /// <summary>
    /// Scales a range of floats. [baseMin to baseMax] will become [limitMin to limitMax]
    /// </summary>
    public static float Scale(this float valueIn, float baseMin, float baseMax, float limitMin, float limitMax) => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
    /// <summary>
    /// Scales a range of integers. [baseMin to baseMax] will become [limitMin to limitMax]
    /// </summary>
    public static int Scale(this int valueIn, int baseMin, int baseMax, int limitMin, int limitMax) => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;

    /// <summary>
    /// Linear interpolation for a range of doubles.
    /// </summary>
    public static double Lerp(this double start, double end, double amount = 0.5D) => start + (end - start) * amount;
    /// <summary>
    /// Linear interpolation for a range of floats.
    /// </summary>
    public static float Lerp(this float start, float end, float amount = 0.5F) => start + (end - start) * amount;

    /// <summary>
    /// Vector2 LERP function.
    /// </summary>
    /// <param name="start"><see cref="Vector2"/></param>
    /// <param name="end"><see cref="Vector2"/></param>
    /// <param name="time">0.0 to 1.0</param>
    /// <returns><see cref="Vector2"/></returns>
    public static Vector2 LinearInterpolation(Vector2 start, Vector2 end, float time)
    {
        return start + (end - start) * time;
    }

    /// <summary>
    /// Vector2 Bezier function.
    /// </summary>
    /// <param name="point0"><see cref="Vector2"/></param>
    /// <param name="point1"><see cref="Vector2"/></param>
    /// <param name="point2"><see cref="Vector2"/></param>
    /// <param name="time">0.0 to 1.0</param>
    /// <returns></returns>
    public static Vector2 BezierInterpolation(Vector2 point0, Vector2 point1, Vector2 point2, float time)
    {
        Vector2 intermediateA = LinearInterpolation(point0, point1, time);
        Vector2 intermediateB = LinearInterpolation(point1, point2, time);
        return LinearInterpolation(intermediateA, intermediateB, time);
    }

    public static bool RandomBoolean()
    {
        if (Random.Shared.Next(100) > 49) 
            return true;

        return false;
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
    /// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.
    /// e.g. 420 milliseconds, 3.1 seconds, 2 minutes, 4.231 hours, etc.
    /// </summary>
    /// <param name="span"><see cref="TimeSpan"/></param>
    /// <param name="significantDigits">number of right side digits in output (precision)</param>
    /// <returns>human-friendly string</returns>
    public static string ToTimeString(this TimeSpan span, int significantDigits = 3)
    {
        var format = $"G{significantDigits}";
        return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + " seconds"
                : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + " minutes"
                : (span.TotalHours < 24 ? span.TotalHours.ToString(format) + " hours"
                : span.TotalDays.ToString(format) + " days")));
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
                ? uri.AbsolutePath.Replace("/Files", "/Assets")
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
    /// Creates a new <see cref="Span{T}"/> over an input <see cref="List{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input <see cref="List{T}"/> instance.</typeparam>
    /// <param name="list">The input <see cref="List{T}"/> instance.</param>
    /// <returns>A <see cref="Span{T}"/> instance with the values of <paramref name="list"/>.</returns>
    /// <remarks>
    /// Note that the returned <see cref="Span{T}"/> is only guaranteed to be valid as long as the items within
    /// <paramref name="list"/> are not modified. Doing so might cause the <see cref="List{T}"/> to swap its
    /// internal buffer, causing the returned <see cref="Span{T}"/> to become out of date. That means that in this
    /// scenario, the <see cref="Span{T}"/> would end up wrapping an array no longer in use. Always make sure to use
    /// the returned <see cref="Span{T}"/> while the target <see cref="List{T}"/> is not modified.
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T>? list)
    {
        return CollectionsMarshal.AsSpan(list);
    }

    /// <summary>
    /// Returns a simple string representation of an array.
    /// </summary>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <param name="array">The source array.</param>
    /// <returns>The <see cref="string"/> representation of the array.</returns>
    public static string ToArrayString<T>(this T?[] array)
    {
        // The returned string will be in the following format: [1, 2, 3]
        StringBuilder builder = new StringBuilder();
        builder.Append('[');
        for (int i = 0; i < array.Length; i++)
        {
            if (i != 0)
                builder.Append(",\t");

            builder.Append(array[i]?.ToString());
        }
        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    /// Reads a sequence of bytes from a given <see cref="Stream"/> instance.
    /// </summary>
    /// <param name="stream">The source <see cref="Stream"/> to read data from.</param>
    /// <param name="buffer">The target <see cref="Span{T}"/> to write data to.</param>
    /// <returns>The number of bytes that have been read.</returns>
    public static int Read(this Stream stream, Span<byte> buffer)
    {
        byte[] rent = ArrayPool<byte>.Shared.Rent(buffer.Length);

        try
        {
            int bytesRead = stream.Read(rent, 0, buffer.Length);
            if (bytesRead > 0)
                rent.AsSpan(0, bytesRead).CopyTo(buffer);

            return bytesRead;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <summary>
    /// Reads a value of a specified type from a source <see cref="Stream"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of value to read.</typeparam>
    /// <param name="stream">The source <see cref="Stream"/> instance to read from.</param>
    /// <returns>The <typeparamref name="T"/> value read from <paramref name="stream"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="stream"/> reaches the end.</exception>
    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        int length = Unsafe.SizeOf<T>();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            if (stream.Read(buffer, 0, length) != length)
                throw new InvalidOperationException("The stream didn't contain enough data to read the requested item");

            return Unsafe.ReadUnaligned<T>(ref buffer[0]);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Writes a sequence of bytes to a given <see cref="Stream"/> instance.
    /// </summary>
    /// <param name="stream">The destination <see cref="Stream"/> to write data to.</param>
    /// <param name="buffer">The source <see cref="Span{T}"/> to read data from.</param>
    public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
    {
        byte[] rent = ArrayPool<byte>.Shared.Rent(buffer.Length);

        try
        {
            buffer.CopyTo(rent);
            stream.Write(rent, 0, buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <summary>
    /// Writes a value of a specified type into a target <see cref="Stream"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of value to write.</typeparam>
    /// <param name="stream">The target <see cref="Stream"/> instance to write to.</param>
    /// <param name="value">The input value to write to <paramref name="stream"/>.</param>
    public static void Write<T>(this Stream stream, in T value) where T : unmanaged
    {
        int length = Unsafe.SizeOf<T>();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            Unsafe.WriteUnaligned(ref buffer[0], value);
            stream.Write(buffer, 0, length);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Write<{typeof(T)}>: {ex}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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

    public static IEnumerable<Type?> GetHierarchyFromUIElement(this Type element)
    {
        if (element.GetTypeInfo().IsSubclassOf(typeof(UIElement)) != true)
        {
            yield break;
        }

        Type current = element;

        while (current != null && current != typeof(UIElement))
        {
            yield return current;
            current = current.GetTypeInfo().BaseType;
        }
    }

    public static void DisplayRoutedEventsForUIElement()
    {
        Type uiElementType = typeof(UIElement);
        var routedEvents = uiElementType.GetEvents();
        Debug.WriteLine($"[All RoutedEvents for UIElement]");
        foreach (var routedEvent in routedEvents)
        {
            if (routedEvent.EventHandlerType == typeof(RoutedEventHandler) ||
                routedEvent.EventHandlerType == typeof(RoutedEvent) ||
                routedEvent.EventHandlerType == typeof(EventHandler))
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
            else if (routedEvent.MemberType == MemberTypes.Event)
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
        }
    }

    public static void DisplayRoutedEventsForFrameworkElement()
    {
        Type fwElementType = typeof(FrameworkElement);
        var routedEvents = fwElementType.GetEvents();
        Debug.WriteLine($"[All RoutedEvents for FrameworkElement]");
        foreach (var routedEvent in routedEvents)
        {
            if (routedEvent.EventHandlerType == typeof(RoutedEventHandler) ||
                routedEvent.EventHandlerType == typeof(RoutedEvent) ||
                routedEvent.EventHandlerType == typeof(EventHandler))
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
            else if (routedEvent.MemberType == MemberTypes.Event)
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
        }
    }

    public static void DisplayRoutedEventsForControl()
    {
        Type ctlElementType = typeof(Microsoft.UI.Xaml.Controls.Control);
        var routedEvents = ctlElementType.GetEvents();
        Debug.WriteLine($"[All RoutedEvents for Control]");
        foreach (var routedEvent in routedEvents)
        {
            if (routedEvent.EventHandlerType == typeof(RoutedEventHandler) ||
                routedEvent.EventHandlerType == typeof(RoutedEvent) ||
                routedEvent.EventHandlerType == typeof(EventHandler))
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
            else if (routedEvent.MemberType == MemberTypes.Event)
            {
                Debug.WriteLine($" - '{routedEvent.Name}'");
            }
        }
    }

    /// <summary>
    /// Generates a 7 digit color string including the # sign.
    /// If the <see cref="ElementTheme"/> is dark then 0, 1 & 2 options are 
    /// removed so dark colors such as 000000/111111/222222 are not possible.
    /// If the <see cref="ElementTheme"/> is light then D, E & F options are 
    /// removed so light colors such as DDDDDD/EEEEEE/FFFFFF are not possible.
    /// </summary>
    public static string GetRandomColorString(ElementTheme? theme)
    {
        StringBuilder sb = new StringBuilder();
        string pwChars = "012346789ABCDEF";

        if (theme.HasValue && theme == ElementTheme.Dark)
            pwChars = "346789ABCDEF";
        else if (theme.HasValue && theme == ElementTheme.Light)
            pwChars = "012346789ABC";

        char[] charArray = pwChars.Distinct().ToArray();
        var result = new char[7];

        for (int x = 0; x < 6; x++)
            sb.Append(pwChars[Random.Shared.Next() % pwChars.Length]);

        return $"#{sb}";
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to a hexadecimal string representation.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The hexadecimal string representation of the color.</returns>
    public static string ToHex(this Windows.UI.Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from a XAML color string.
    /// Any format used in XAML should work.
    /// </summary>
    /// <param name="colorString">The XAML color string.</param>
    /// <returns>The created <see cref="Color"/>.</returns>
    public static Windows.UI.Color? ToColor(this string colorString)
    {
        if (string.IsNullOrEmpty(colorString))
            throw new ArgumentException($"The parameter \"{nameof(colorString)}\" must not be null or empty.");

        if (colorString[0] == '#')
        {
            switch (colorString.Length)
            {
                case 9:
                    {
                        var cuint = Convert.ToUInt32(colorString.Substring(1), 16);
                        var a = (byte)(cuint >> 24);
                        var r = (byte)((cuint >> 16) & 0xff);
                        var g = (byte)((cuint >> 8) & 0xff);
                        var b = (byte)(cuint & 0xff);
                        return Windows.UI.Color.FromArgb(a, r, g, b);
                    }
                case 7:
                    {
                        var cuint = Convert.ToUInt32(colorString.Substring(1), 16);
                        var r = (byte)((cuint >> 16) & 0xff);
                        var g = (byte)((cuint >> 8) & 0xff);
                        var b = (byte)(cuint & 0xff);
                        return Windows.UI.Color.FromArgb(255, r, g, b);
                    }
                case 5:
                    {
                        var cuint = Convert.ToUInt16(colorString.Substring(1), 16);
                        var a = (byte)(cuint >> 12);
                        var r = (byte)((cuint >> 8) & 0xf);
                        var g = (byte)((cuint >> 4) & 0xf);
                        var b = (byte)(cuint & 0xf);
                        a = (byte)(a << 4 | a);
                        r = (byte)(r << 4 | r);
                        g = (byte)(g << 4 | g);
                        b = (byte)(b << 4 | b);
                        return Windows.UI.Color.FromArgb(a, r, g, b);
                    }
                case 4:
                    {
                        var cuint = Convert.ToUInt16(colorString.Substring(1), 16);
                        var r = (byte)((cuint >> 8) & 0xf);
                        var g = (byte)((cuint >> 4) & 0xf);
                        var b = (byte)(cuint & 0xf);
                        r = (byte)(r << 4 | r);
                        g = (byte)(g << 4 | g);
                        b = (byte)(b << 4 | b);
                        return Windows.UI.Color.FromArgb(255, r, g, b);
                    }
                default: return ThrowFormatException();
            }
        }

        if (colorString.Length > 3 && colorString[0] == 's' && colorString[1] == 'c' && colorString[2] == '#')
        {
            var values = colorString.Split(',');

            if (values.Length == 4)
            {
                var scA = double.Parse(values[0].Substring(3), CultureInfo.InvariantCulture);
                var scR = double.Parse(values[1], CultureInfo.InvariantCulture);
                var scG = double.Parse(values[2], CultureInfo.InvariantCulture);
                var scB = double.Parse(values[3], CultureInfo.InvariantCulture);

                return Windows.UI.Color.FromArgb((byte)(scA * 255), (byte)(scR * 255), (byte)(scG * 255), (byte)(scB * 255));
            }

            if (values.Length == 3)
            {
                var scR = double.Parse(values[0].Substring(3), CultureInfo.InvariantCulture);
                var scG = double.Parse(values[1], CultureInfo.InvariantCulture);
                var scB = double.Parse(values[2], CultureInfo.InvariantCulture);

                return Windows.UI.Color.FromArgb(255, (byte)(scR * 255), (byte)(scG * 255), (byte)(scB * 255));
            }

            return ThrowFormatException();
        }

        var prop = typeof(Microsoft.UI.Colors).GetTypeInfo().GetDeclaredProperty(colorString);

        if (prop != null)
            return (Windows.UI.Color?)prop.GetValue(null) ?? Windows.UI.Color.FromArgb(255, 198, 87, 88);

        return ThrowFormatException();

        static Windows.UI.Color ThrowFormatException() => throw new FormatException($"The parameter \"{nameof(colorString)}\" is not a recognized Color format.");
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
    /// Tries to get a boxed <typeparamref name="T"/> value from an input <see cref="object"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of value to try to unbox.</typeparam>
    /// <param name="obj">The input <see cref="object"/> instance to check.</param>
    /// <param name="value">The resulting <typeparamref name="T"/> value, if <paramref name="obj"/> was in fact a boxed <typeparamref name="T"/> value.</param>
    /// <returns><see langword="true"/> if a <typeparamref name="T"/> value was retrieved correctly, <see langword="false"/> otherwise.</returns>
    public static bool TryUnbox<T>(object obj, out T? value)
    {
        if (obj is T)
        {
            value = (T)obj;
            return true;
        }
        value = default;
        return false;
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
    /// Get OS version by way of <see cref="Environment.OSVersion"/>.
    /// </summary>
    /// <returns>true if Win11 or higher, false otherwise</returns>
    public static bool IsWindows11OrGreater() => Environment.OSVersion.Version >= new Version(10, 0, 22000, 0);

    public static UInt16 ReverseBytes(this UInt16 value)
    {
        return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }

    public static UInt32 ReverseBytes(this UInt32 value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
               (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }

    public static UInt64 ReverseBytes(this UInt64 value)
    {
        return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
               (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
               (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
               (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }

    public static byte[] StringToByteArray(this string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    public static string ByteArrayToHexString(this byte[] array)
    {
        if (array.Length == 0) { return string.Empty; }
        var hex = new StringBuilder(array.Length * 2);
        foreach (byte b in array) { hex.AppendFormat("{0:X2}", b); }
        return hex.ToString();
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
    /// Will retry each operation with a 2 second delay between attempts.
    /// </summary>
    public async static Task<T> RetryAsync<T>(this Func<T> operation, int attempts)
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
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
                await Task.Delay(2000);
            }
        }
    }

    /// <summary>
    /// Can be useful if you only have a root (not merged) resource dictionary.
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
    /// Can be useful if you have merged theme resource dictionaries.
    /// var darkBrush = Extensions.GetThemeResource{SolidColorBrush}("PrimaryBrush", ElementTheme.Dark);
    /// var lightBrush = Extensions.GetThemeResource{SolidColorBrush}("PrimaryBrush", ElementTheme.Light);
    /// </summary>
    public static T? GetThemeResource<T>(string resourceName, ElementTheme? theme) where T : class
    {
        try
        {
            theme ??= ElementTheme.Default;

            var dictionaries = Application.Current.Resources.MergedDictionaries;
            foreach (var item in dictionaries)
            {
                // A typical IList<ResourceDictionary> will contain:
                //   - 'Default'
                //   - 'Light'
                //   - 'Dark'
                //   - 'HighContrast'
                foreach (var kv in item.ThemeDictionaries.Keys)
                {
                    // Examine the ICollection<T> for the key names.
                    Debug.WriteLine($"ThemeDictionary is named '{kv}'", $"{nameof(GeneralExtensions)}");
                }

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
            return inputArray;
    }

    /// <summary>
    /// Helper for parsing command line arguments.
    /// </summary>
    /// <param name="inputArray"></param>
    /// <returns>string array of args excluding the 1st arg</returns>
    public static string[] IgnoreNthTakeRest(this string[] inputArray, int skip = 1)
    {
        if (inputArray.Length > skip)
            return inputArray.Skip(skip).ToArray();
        else
            return inputArray;
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

    /// <summary>
    /// Basic key generator for unique IDs.
    /// This employs the standard MS key table which accounts
    /// for the 36 Latin letters and Arabic numerals used in
    /// most Western European languages...
    /// 24 chars are favored: 2346789 BCDFGHJKMPQRTVWXY
    /// 12 chars are avoided: 015 AEIOU LNSZ
    /// Of the 24 favored chars, only two are occasionally
    /// mistaken: 8 & B, which depends mostly on the font.
    /// The base of possible codes is large, about 3.2 * 10^34.
    /// </summary>
    public static string KeyGen(int kLength = 6, long pSeed = 0)
    {
        const string pwChars = "2346789BCDFGHJKMPQRTVWXY";
        if (kLength < 6)
            kLength = 6; // minimum of 6 characters

        char[] charArray = pwChars.Distinct().ToArray();

        if (pSeed == 0)
        {
            pSeed = DateTime.Now.Ticks;
            //Thread.Sleep(1); // allow a tick to go by (if hammering)
        }

        var result = new char[kLength];
        var rng = new Random((int)pSeed);

        for (int x = 0; x < kLength; x++)
            result[x] = pwChars[rng.Next() % pwChars.Length];

        return (new string(result));
    }

    /// <summary>
    /// 64-bit hashing method.
    /// Modulus and shift each byte across the string length.
    /// </summary>
    /// <param name="input">string to hash</param>
    public static ulong BasicHash(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(input);
        ulong value = (ulong)utf8.Length;
        for (int n = 0; n < utf8.Length; n++)
        {
            value += (ulong)utf8[n] << ((n * 5) % 56);
        }
        return value;
    }

    public static string NumberToWord(int number)
    {
        if (number == 0) { return "zero"; }
        if (number < 0) { return "minus " + NumberToWord(Math.Abs(number)); }

        string words = "";

        if ((number / 1000000) > 0)
        {
            words += NumberToWord(number / 1000000) + " million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWord(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWord(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
                words += "and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
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

    /// <summary>
    /// Gets the result of a <see cref="Task"/> if available, or <see langword="null"/> otherwise.
    /// </summary>
    /// <param name="task">The input <see cref="Task"/> instance to get the result for.</param>
    /// <returns>The result of <paramref name="task"/> if completed successfully, or <see langword="default"/> otherwise.</returns>
    /// <remarks>
    /// This method does not block if <paramref name="task"/> has not completed yet. Furthermore, it is not generic
    /// and uses reflection to access the <see cref="Task{TResult}.Result"/> property and boxes the result if it's
    /// a value type, which adds overhead. It should only be used when using generics is not possible.
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? GetResultOrDefault(this Task task)
    {
        // Check if the instance is a completed Task
        if (
#if NETSTANDARD2_1
            task.IsCompletedSuccessfully
#else
            task.Status == TaskStatus.RanToCompletion
#endif
        )
        {
            // We need an explicit check to ensure the input task is not the cached
            // Task.CompletedTask instance, because that can internally be stored as
            // a Task<T> for some given T (e.g. on dotNET 5 it's VoidTaskResult), which
            // would cause the following code to return that result instead of null.
            if (task != Task.CompletedTask)
            {
                // Try to get the Task<T>.Result property. This method would've
                // been called anyway after the type checks, but using that to
                // validate the input type saves some additional reflection calls.
                // Furthermore, doing this also makes the method flexible enough to
                // cases whether the input Task<T> is actually an instance of some
                // runtime-specific type that inherits from Task<T>.
                PropertyInfo? propertyInfo =
#if NETSTANDARD1_4
                    task.GetType().GetRuntimeProperty(nameof(Task<object>.Result));
#else
                    task.GetType().GetProperty(nameof(Task<object>.Result));
#endif

                // Return the result, if possible
                return propertyInfo?.GetValue(task);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the result of a <see cref="Task{TResult}"/> if available, or <see langword="default"/> otherwise.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Task{TResult}"/> to get the result for.</typeparam>
    /// <param name="task">The input <see cref="Task{TResult}"/> instance to get the result for.</param>
    /// <returns>The result of <paramref name="task"/> if completed successfully, or <see langword="default"/> otherwise.</returns>
    /// <remarks>This method does not block if <paramref name="task"/> has not completed yet.</remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetResultOrDefault<T>(this Task<T?> task)
    {
#if NETSTANDARD2_1
        return task.IsCompletedSuccessfully ? task.Result : default;
#else
        return task.Status == TaskStatus.RanToCompletion ? task.Result : default;
#endif
    }


    /// <summary>
    /// Enqueues an action using the <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/>.
    /// Wraps the call in a try/catch and returns the result as a <see cref="Task"/>.
    /// This would typically be called from an asynchronous event, such as <see cref="Windows.System.Power.PowerManager.EnergySaverStatusChanged"/>
    /// </summary>
    /// <example>
    /// Dispatcher.CallOnUIThread(() => { TextBlock.SelectionHighlightColor = new SolidColorBrush(Colors.Yellow); });
    /// </example>
    public static Task CallOnUIThread(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Microsoft.UI.Dispatching.DispatcherQueueHandler handler)
    {
        try
        {
            _ = dispatcher.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, handler);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }

    /// <summary>
    /// Invokes a given function on the target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> and returns a
    /// <see cref="Task"/> that completes when the invocation of the function is completed.
    /// </summary>
    /// <param name="dispatcher">The target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> to invoke the code on.</param>
    /// <param name="function">The <see cref="Action"/> to invoke.</param>
    /// <param name="priority">The priority level for the function to invoke.</param>
    /// <returns>A <see cref="Task"/> that completes when the invocation of <paramref name="function"/> is over.</returns>
    /// <remarks>If the current thread has access to <paramref name="dispatcher"/>, <paramref name="function"/> will be invoked directly.</remarks>
    public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority = Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal)
    {
        // Run the function directly when we have thread access.
        // Also reuse Task.CompletedTask in case of success,
        // to skip an unnecessary heap allocation for every invocation.
        if (dispatcher.HasThreadAccess) //if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
        {
            try
            {
                function();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        static Task TryEnqueueAsync(Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority)
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();

            if (!dispatcher.TryEnqueue(priority, () =>
            {
                try
                {
                    function();

                    taskCompletionSource.SetResult(null);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }))
            {
                taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }

        return TryEnqueueAsync(dispatcher, function, priority);
    }

    /// <summary>
    /// Invokes a given function on the target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> and returns a
    /// <see cref="Task{TResult}"/> that completes when the invocation of the function is completed.
    /// </summary>
    /// <typeparam name="T">The return type of <paramref name="function"/> to relay through the returned <see cref="Task{TResult}"/>.</typeparam>
    /// <param name="dispatcher">The target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> to invoke the code on.</param>
    /// <param name="function">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <param name="priority">The priority level for the function to invoke.</param>
    /// <returns>A <see cref="Task"/> that completes when the invocation of <paramref name="function"/> is over.</returns>
    /// <remarks>If the current thread has access to <paramref name="dispatcher"/>, <paramref name="function"/> will be invoked directly.</remarks>
    public static Task<T> EnqueueAsync<T>(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<T> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority = Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal)
    {
        // If we have thread access, we can retrieve the task directly.
        // We don't use ConfigureAwait(false) in this case, in order
        // to let the caller continue its execution on the same thread
        // after awaiting the task returned by this function.
        if (dispatcher.HasThreadAccess) //if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
        {
            try
            {
                return Task.FromResult(function());
            }
            catch (Exception e)
            {
                return Task.FromException<T>(e);
            }
        }

        static Task<T> TryEnqueueAsync(Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<T> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            if (!dispatcher.TryEnqueue(priority, () =>
            {
                try
                {
                    taskCompletionSource.SetResult(function());
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }))
            {
                taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }

        return TryEnqueueAsync(dispatcher, function, priority);
    }

    /// <summary>
    /// Invokes a given function on the target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> and returns a
    /// <see cref="Task"/> that acts as a proxy for the one returned by the given function.
    /// </summary>
    /// <param name="dispatcher">The target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> to invoke the code on.</param>
    /// <param name="function">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <param name="priority">The priority level for the function to invoke.</param>
    /// <returns>A <see cref="Task"/> that acts as a proxy for the one returned by <paramref name="function"/>.</returns>
    /// <remarks>If the current thread has access to <paramref name="dispatcher"/>, <paramref name="function"/> will be invoked directly.</remarks>
    public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority = Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal)
    {
        // If we have thread access, we can retrieve the task directly.
        // We don't use ConfigureAwait(false) in this case, in order
        // to let the caller continue its execution on the same thread
        // after awaiting the task returned by this function.
        if (dispatcher.HasThreadAccess) //if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
        {
            try
            {
                if (function() is Task awaitableResult)
                {
                    return awaitableResult;
                }

                return Task.FromException(GetEnqueueException($"The Task returned by {nameof(function)} cannot be null."));
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        static Task TryEnqueueAsync(Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority)
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();

            if (!dispatcher.TryEnqueue(priority, async () =>
            {
                try
                {
                    if (function() is Task awaitableResult)
                    {
                        await awaitableResult.ConfigureAwait(false);

                        taskCompletionSource.SetResult(null);
                    }
                    else
                    {
                        taskCompletionSource.SetException(GetEnqueueException($"The Task returned by {nameof(function)} cannot be null."));
                    }
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }))
            {
                taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }

        return TryEnqueueAsync(dispatcher, function, priority);
    }

    /// <summary>
    /// Invokes a given function on the target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> and returns a
    /// <see cref="Task{TResult}"/> that acts as a proxy for the one returned by the given function.
    /// </summary>
    /// <typeparam name="T">The return type of <paramref name="function"/> to relay through the returned <see cref="Task{TResult}"/>.</typeparam>
    /// <param name="dispatcher">The target <see cref="Microsoft.UI.Dispatching.DispatcherQueue"/> to invoke the code on.</param>
    /// <param name="function">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <param name="priority">The priority level for the function to invoke.</param>
    /// <returns>A <see cref="Task{TResult}"/> that relays the one returned by <paramref name="function"/>.</returns>
    /// <remarks>If the current thread has access to <paramref name="dispatcher"/>, <paramref name="function"/> will be invoked directly.</remarks>
    public static Task<T> EnqueueAsync<T>(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task<T>> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority = Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal)
    {
        if (dispatcher.HasThreadAccess) //if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
        {
            try
            {
                if (function() is Task<T> awaitableResult)
                {
                    return awaitableResult;
                }

                return Task.FromException<T>(GetEnqueueException($"The Task returned by {nameof(function)} cannot be null."));
            }
            catch (Exception e)
            {
                return Task.FromException<T>(e);
            }
        }

        static Task<T> TryEnqueueAsync(Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task<T>> function, Microsoft.UI.Dispatching.DispatcherQueuePriority priority)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            if (!dispatcher.TryEnqueue(priority, async () =>
            {
                try
                {
                    if (function() is Task<T> awaitableResult)
                    {
                        var result = await awaitableResult.ConfigureAwait(false);

                        taskCompletionSource.SetResult(result);
                    }
                    else
                    {
                        taskCompletionSource.SetException(GetEnqueueException($"The Task returned by {nameof(function)} cannot be null."));
                    }
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }))
            {
                taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }

        return TryEnqueueAsync(dispatcher, function, priority);
    }

    /// <summary>
    /// Creates an <see cref="InvalidOperationException"/> to return when an enqueue operation fails.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <returns>An <see cref="InvalidOperationException"/> with a specified message.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)] // Prevent the JIT compiler from inlining this method with the caller.
    private static InvalidOperationException GetEnqueueException(string message)
    {
        return new InvalidOperationException(message);
    }

    /// <summary>
    /// Helper method for thread state.
    /// </summary>
    /// <param name="ts"></param>
    /// <returns><see cref="System.Threading.ThreadState"/></returns>
    public static System.Threading.ThreadState SimplifyState(System.Threading.ThreadState ts)
    {
        return ts & (System.Threading.ThreadState.Unstarted |
                     System.Threading.ThreadState.WaitSleepJoin |
                     System.Threading.ThreadState.Stopped);
    }

    /// <summary>
    /// You can test for a thread being blocked via its <see cref="System.Threading.ThreadState"/> property.
    /// </summary>
    /// <param name="ts">The <see cref="System.Threading.ThreadState"/> of the thread you want to test.</param>
    /// <returns>true if blocked, false otherwise</returns>
    public static bool IsThreadBlocked(this System.Threading.ThreadState ts)
    {
        return (ts & System.Threading.ThreadState.WaitSleepJoin) != 0;
    }

}
