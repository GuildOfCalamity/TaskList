using System.Text;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

using CommunityToolkit.WinUI.Helpers;

using Task_List_App.Helpers;
using Task_List_App.ViewModels;

namespace Task_List_App.Views;

/// <summary>
/// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; private set; }

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

        // Upon first load read in the markdown file contents and render
        // render it using the CommunityToolkit.WinUI.UI.Controls.Markdown.
        if (mdReadMe.Text.Length == 0)
        {
            StringBuilder sb = new StringBuilder();

            if (App.IsPackaged)
            {
                //target = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "ReadMe.md");
                target = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Assets", "ReadMe.md");
            }
            else
            {
                target = Path.Combine(AppContext.BaseDirectory, "Assets", "ReadMe.md");
            }

            if (File.Exists(target))
            {
                var lines = System.IO.File.ReadAllLines(target, Encoding.UTF8);
                foreach (var line in lines) { sb.AppendLine(line); }
                if (sb.Length > 0)
                    mdReadMe.Text = sb.ToString();
                else
                    mdReadMe.Text = "## File could not be read";
            }
        }
    }

    #region [CommunityToolkit.WinUI.UI.Controls.Markdown Events]
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
    /// <remarks>
    /// We'll need to resolve to an <see cref="Microsoft.UI.Xaml.Media.ImageSource"/>.
    /// We can use a <see cref="Microsoft.UI.Xaml.Media.Imaging.BitmapImage"/> for this.
    /// </remarks>
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
                
                if (url != null)
                    Debug.WriteLine($"Trying local image from scheme '{url.Scheme}' ...");

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
                    else // Check local "Assets" folder.
                    {
                        try
                        {
                            /* 
                             *  Unfortunately this throws an InvalidOperationException with no explanation.
                             *  The problem is the method still assumes the app is packaged.
                             *  The documentation is sorely lacking...
                             *  https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.helpers.streamhelper.getlocalfilestreamasync?view=win-comm-toolkit-dotnet-7.0#communitytoolkit-winui-helpers-streamhelper-getlocalfilestreamasync(system-string-windows-storage-fileaccessmode)
                             */
                            //string target = Path.Combine(AppContext.BaseDirectory, "Assets", e.Url.Replace("./", ""));
                            //var imageStream = await StreamHelper.GetLocalFileStreamAsync(target);
                            //if (imageStream != null)
                            //{
                            //    image = new BitmapImage();
                            //    await image.SetSourceAsync(imageStream);
                            //    Debug.WriteLine($"Image resolved!");
                            //}

                            #region [Fixing the CommunityToolkit method]
                            string target = Path.Combine("Assets", e.Url.Replace("./", ""));
                            var imageStream = await GetLocalFileStreamAsync(target);
                            if (imageStream != null)
                            {
                                image = new BitmapImage();
                                await image.SetSourceAsync(imageStream);
                                Debug.WriteLine($"Image resolved!");
                            }
                            #endregion

                            #region [Simpler extension method]
                            //image = e.Url.GetImageFromAssets();
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"StreamHelper: {ex.Message}");
                }
            }
            catch (Exception ex) 
            {
                App.DebugLog($"MarkDown_ImageResolving: {ex.Message}");
            }
        }
        else if (url != null && url.Scheme == "ms-appx")
        {
            try
            {
                image = new BitmapImage(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{url.Scheme}: {ex.Message}");
            }
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
    #endregion

    /// <summary>
    /// Gets the image data from a Uri.
    /// NOTE: The issue with many of the CommunityToolkit file access routines is that they do not
    /// handle unpackaged apps, so you will see I added logic switches for most of these methods.
    /// </summary>
    /// <param name="uri">Image Uri</param>
    /// <returns>Image Stream as <see cref="IRandomAccessStream"/></returns>
    public async Task<IRandomAccessStream?> GetImageStream(Uri uri)
    {
        IRandomAccessStream? imageStream = null;
        var localPath = $"{uri.Host}/{uri.LocalPath}".Replace("//", "/");

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
                imageStream = await StreamHelper.GetPackagedFileStreamAsync(localPath);
            }
            else
            {
                /*
                    "StreamHelper.GetLocalFileStreamAsync" contains the following...
                    StorageFolder workingFolder = ApplicationData.Current.LocalFolder;
                    return GetFileStreamAsync(fileName, accessMode, workingFolder);
                */
                imageStream = await StreamHelper.GetLocalFileStreamAsync(localPath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetImageStream: {ex.Message}");
        }

        return imageStream;
    }

    #region [Customized from CommunityToolkit]
    /// <summary>
    /// Gets a stream to a specified file from the application local folder.
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/rel/winui/7.1.2/CommunityToolkit.WinUI/Helpers/StreamHelper.cs
    /// </summary>
    /// <param name="fileName">Relative name of the file to open. Can contains subfolders.</param>
    /// <param name="accessMode">File access mode. Default is read.</param>
    /// <returns>The file stream as <see cref="IRandomAccessStream"/></returns>
    async Task<IRandomAccessStream> GetLocalFileStreamAsync(string fileName, FileAccessMode accessMode = FileAccessMode.Read)
    {
        if (App.IsPackaged)
        {
            StorageFolder workingFolder = ApplicationData.Current.LocalFolder;
            return await GetFileStreamAsync(fileName, accessMode, workingFolder);
        }
        else
        {
            var workingFolder = await StorageFolder.GetFolderFromPathAsync(AppContext.BaseDirectory);
            return await GetFileStreamAsync(fileName, accessMode, workingFolder);
        }
    }

    /// <summary>
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/rel/winui/7.1.2/CommunityToolkit.WinUI/Helpers/StreamHelper.cs
    /// </summary>
    /// <param name="fullFileName"></param>
    /// <param name="accessMode"></param>
    /// <param name="workingFolder"></param>
    /// <returns>The file stream as <see cref="IRandomAccessStream"/></returns>
    async Task<IRandomAccessStream> GetFileStreamAsync(string fullFileName, FileAccessMode accessMode, StorageFolder workingFolder)
    {
        var fileName = Path.GetFileName(fullFileName);
        workingFolder = await GetSubFolderAsync(fullFileName, workingFolder);
        var file = await workingFolder.GetFileAsync(fileName);
        return await file.OpenAsync(accessMode);
    }

    /// <summary>
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/rel/winui/7.1.2/CommunityToolkit.WinUI/Helpers/StreamHelper.cs
    /// </summary>
    /// <returns><see cref="StorageFolder"/></returns>
    async Task<StorageFolder> GetSubFolderAsync(string fullFileName, StorageFolder workingFolder)
    {
        var folderName = Path.GetDirectoryName(fullFileName);

        if (!string.IsNullOrEmpty(folderName) && folderName != @"\")
            return await workingFolder.GetFolderAsync(folderName);

        return workingFolder;
    }
    #endregion
}
