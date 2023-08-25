using System.Text;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Task_List_App.ViewModels;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;

namespace Task_List_App.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		ViewModel = App.GetService<SettingsViewModel>();

        InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        this.Loaded += SettingsPage_Loaded;
    }

    void SettingsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        string target = "";

        if (mdReadMe.Text.Length == 0)
        {
            StringBuilder sb = new StringBuilder();

            if (App.IsPackaged)
                target = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Assets", "ReadMe.md");
            else
                target = Path.Combine(AppContext.BaseDirectory, "Assets", "ReadMe.md");

            if (File.Exists(target))
            {
                var lines = System.IO.File.ReadAllLines(target, Encoding.UTF8);
                foreach (var line in lines) { sb.AppendLine(line); }
                if (sb.Length > 0)
                    mdReadMe.Text = sb.ToString();
            }
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.ui.controls?view=win-comm-toolkit-dotnet-7.0
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/rel/winui/7.1.2
    /// </summary>
    async void mdReadMe_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
    {
        var link = e.Link;
        if (Uri.TryCreate(link, UriKind.Absolute, out Uri result))
        {
            await Launcher.LaunchUriAsync(result);
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.ui.controls?view=win-comm-toolkit-dotnet-7.0
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/rel/winui/7.1.2
    /// </summary>
    async void mdReadMe_ImageResolving(object sender, CommunityToolkit.WinUI.UI.Controls.ImageResolvingEventArgs e)
    {
        var deferral = e.GetDeferral();
        BitmapImage? image = null;

        // Determine if the link is not absolute, meaning it is relative.
        if (!Uri.TryCreate(e.Url, UriKind.Absolute, out Uri url))
        {
            try
            {
                var imageStream = await GetImageStream(new Uri(e.Url));
                if (imageStream != null)
                {
                    image = new BitmapImage();
                    await image.SetSourceAsync(imageStream);
                }
            }
            catch (UriFormatException fex)
            {
                Debug.WriteLine($"GetImageStream: {fex.Message}");
                Debug.WriteLine($"Trying local image...");
                try
                {
                    if (App.IsPackaged)
                    {
                        string target = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Assets", e.Url.Replace("./", ""));
                        // https://github.com/MicrosoftDocs/WindowsCommunityToolkitDocs/blob/main/dotnet/xml/CommunityToolkit.WinUI.Helpers/StreamHelper.xml
                        var imageStream = await StreamHelper.GetPackagedFileStreamAsync(target);
                        if (imageStream != null)
                        {
                            image = new BitmapImage();
                            await image.SetSourceAsync(imageStream);
                        }
                    }
                    else
                    {
                        string target = Path.Combine(AppContext.BaseDirectory, "Assets", e.Url.Replace("./", ""));
                        // https://github.com/MicrosoftDocs/WindowsCommunityToolkitDocs/blob/main/dotnet/xml/CommunityToolkit.WinUI.Helpers/StreamHelper.xml
                        var imageStream = await StreamHelper.GetLocalFileStreamAsync(target);
                        if (imageStream != null)
                        {
                            image = new BitmapImage();
                            await image.SetSourceAsync(imageStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"StreamHelper: {ex.Message}");
                }
            }
        }
        else if (url.Scheme == "ms-appx")
        {
            image = new BitmapImage(url);
        }
        else
        {
            try
            {
                // Cache a remote image from the internet.
                var imageStream = await GetImageStream(url);
                if (imageStream != null)
                {
                    image = new BitmapImage();
                    await image.SetSourceAsync(imageStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetImageStream: {ex.Message}");
            }
        }

        // Handle only if no exceptions occur.
        if (image != null)
        {
            e.Image = image;
            e.Handled = true;
        }

        deferral.Complete();
    }

    /// <summary>
    /// Gets the image data from a Uri, with Caching.
    /// </summary>
    /// <param name="uri">Image Uri</param>
    /// <returns>Image Stream</returns>
    public async Task<IRandomAccessStream> GetImageStream(Uri uri)
    {
        IRandomAccessStream? imageStream = null;
        var localPath = $"{uri.Host}/{uri.LocalPath}".Replace("//", "/");

        // If we don't have internet, then try to see if we have a packaged copy
        if (imageStream == null)
        {
            try
            {
                if (App.IsPackaged)
                    imageStream = await StreamHelper.GetPackagedFileStreamAsync(localPath);
                else
                    imageStream = await StreamHelper.GetLocalFileStreamAsync(localPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetImageStream: {ex.Message}");
            }
        }

        return imageStream;
    }

    /// <summary>
    /// Helper for web images.
    /// </summary>
    public async Task<Stream> CopyStream(HttpContent source)
    {
        var stream = new MemoryStream();
        await source.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
