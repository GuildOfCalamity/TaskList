using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;
using Task_List_App.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;

namespace Task_List_App.Views;

/// <summary>
/// All controls can be found here:
/// https://github.com/MicrosoftDocs/winui-api/tree/docs/microsoft.ui.xaml.controls
/// </summary>
public sealed partial class NotesPage : Page
{
    bool _doneLoading = false;

    public static event EventHandler<bool>? TextChangedEvent;
    public static event EventHandler<bool>? EditRequestEvent;
    public static event EventHandler<bool>? FinishedLoadingEvent;
    public NotesViewModel ViewModel { get; private set; }
    public LoginViewModel LoginModel { get; private set; }
    public INavigationService? NavService { get; private set; }

    public NotesPage()
    {
        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        
        NavService = App.GetService<INavigationService>();
        LoginModel = App.GetService<LoginViewModel>();
        ViewModel = App.GetService<NotesViewModel>();
        this.Loaded += NotesPage_Loaded;
        tbTitle.GotFocus += tbTitle_GotFocus;
    }

    #region [Native Events]
    /// <summary>
    /// Verify we're allowed to be here.
    /// </summary>
    void NotesPage_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] {MethodBase.GetCurrentMethod()?.Name}");
        try
        {
            if (!LoginModel.IsLoggedIn)
            {
                NavService?.NavigateTo(typeof(LoginViewModel).FullName!);
                return;
            }

            //List<string> output = new();
            //var lines = File.ReadAllLines(@".\MaterialIcons-codepoints.txt");
            //foreach (var line in lines)
            //{
            //    var tmp = line.Split(" ");
            //    output.Add($"public const string {tmp[0]} = \"\\u{tmp[1]}\";");
            //}
            //File.WriteAllLines(@".\MaterialIcons-codepoints.formatted.txt", output.ToArray());

            _doneLoading = true;
            FinishedLoadingEvent?.Invoke(this, _doneLoading);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] {MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// For switching between edit and display modes.
    /// </summary>
    void tbTitle_GotFocus(object sender, RoutedEventArgs e) => EditRequestEvent?.Invoke(this, true);

    /// <summary>
    /// For switching between edit and display modes.
    /// </summary>
    void TextBox_LostFocus(object sender, RoutedEventArgs e) => EditRequestEvent?.Invoke(this, false);

    /// <summary>
    /// Signal any changes back to the <see cref="NotesViewModel"/> from the <see cref="TextBox"/>.
    /// </summary>
    void TextBox_TitleChanged(object sender, TextChangedEventArgs e)
    {
        if (_doneLoading)
            TextChangedEvent?.Invoke(this, true);
        else
            TextChangedEvent?.Invoke(this, false);
    }

    /// <summary>
    /// Handle TAB press so we don't switch focus to another control.
    /// </summary>
    void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Tab)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
                var selStart = tb.SelectionStart;
                var content = tb.Text;
                tb.Text = content.Insert(selStart, "\t");
                tb.SelectionStart = selStart + 1;
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// For switching between edit and display modes.
    /// The <see cref="CommunityToolkit.WinUI.UI.Controls.MarkdownTextBlock"/> does not offer a "Clicked" event.
    /// </summary>
    void ContentArea_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
        {
            var point = e.GetCurrentPoint((UIElement)sender).Position.ToVector2();
            var rect = mdTextBlock.GetBoundingRect((FrameworkElement)sender);
            if ((point.X > rect.Left && point.X < rect.Width) &&
                (point.Y > rect.Top && point.Y < rect.Height))
            {
                Debug.WriteLine($"[INFO] Mouse press detected at {point}");
                EditRequestEvent?.Invoke(this, true);
            }
        }
    }
    #endregion

    #region [CommunityToolkit.WinUI.UI.Controls.Markdown Events]
    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.ui.controls?view=win-comm-toolkit-dotnet-7.0
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/rel/winui/7.1.2
    /// </summary>
    async void mdTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
    {
        var link = e.Link;
        if (Uri.TryCreate(link, UriKind.Absolute, out Uri result))
        {
            if (result != null)
                await Launcher.LaunchUriAsync(result);
        }
    }

    /// <summary>
    /// Trigger edit mode when clicked.
    /// </summary>
    void mdTextBlock_ImageClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e) => EditRequestEvent?.Invoke(this, true);

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.ui.controls?view=win-comm-toolkit-dotnet-7.0
    /// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/rel/winui/7.1.2
    /// </summary>
    /// <remarks>
    /// We'll need to resolve to an <see cref="Microsoft.UI.Xaml.Media.ImageSource"/>.
    /// We can use a <see cref="Microsoft.UI.Xaml.Media.Imaging.BitmapImage"/> for this.
    /// If the image stream of an absolute URI cannot be resolved, then the image will be 
    /// searched for in the local "Assets" folder.
    /// </remarks>
    async void mdTextBlock_ImageResolving(object sender, CommunityToolkit.WinUI.UI.Controls.ImageResolvingEventArgs e)
    {
        bool notUNC = true;
        var deferral = e.GetDeferral();
        BitmapImage? image = null;

        // Check if we have a UNC asset first.
        if (Uri.TryCreate(e.Url, UriKind.Relative, out Uri urlRel))
        {
            if (urlRel != null && urlRel.OriginalString.StartsWith("\\"))
            {
                notUNC = false;

                try
                {
                    // https://learn.microsoft.com/en-us/uwp/api/windows.storage.storagefolder.getfolderfrompathasync?view=winrt-22621#parameters
                    string fullFileName = $"{urlRel.OriginalString}";
                    var fullPathName = $"\\{Path.GetDirectoryName(fullFileName)}";
                    StorageFolder workingFolder = await StorageFolder.GetFolderFromPathAsync(fullPathName);
                    var fileName = Path.GetFileName(fullFileName);
                    var working = await GetSubFolderAsync(fileName, workingFolder);
                    var file = await working.GetFileAsync(fileName);
                    var imgStream = await file.OpenAsync(FileAccessMode.Read);
                    if (imgStream != null)
                    {
                        image = new BitmapImage();
                        await image.SetSourceAsync(imgStream);
                        Debug.WriteLine($"[SUCCESS] UNC image resolved => '{e.Url}'");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WARNING] ImageResolving: {ex.Message}");
                }
            }
            else
            {
                notUNC = true;
            }
        }

        // Proceed as if local asset.
        if (notUNC)
        {
            // Determine if the link is not absolute, meaning it is relative.
            if (!Uri.TryCreate(e.Url, UriKind.Absolute, out Uri url))
            {
                try
                {
                    var imageStream = await new Uri(e.Url).GetImageStream();
                    if (imageStream != null)
                    {
                        image = new BitmapImage();
                        await image.SetSourceAsync(imageStream);
                    }
                }
                catch (UriFormatException fex)
                {
                    Debug.WriteLine($"[WARNING] GetImageStream: {fex.Message}");

                    if (url != null)
                        Debug.WriteLine($"[INFO] Trying local image from scheme '{url.Scheme}'");

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
                                    Debug.WriteLine($"[SUCCESS] Image resolved => '{e.Url}'");
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
                        Debug.WriteLine($"[WARNING] StreamHelper: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    App.DebugLog($"MarkDownTextBlock_ImageResolving: {ex.Message}");
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
                    var imageStream = await url.GetImageStream();
                    if (imageStream != null)
                    {
                        image = new BitmapImage();
                        await image.SetSourceAsync(imageStream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] GetImageStream: {ex.Message}");
                }
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
