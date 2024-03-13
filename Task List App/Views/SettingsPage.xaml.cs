﻿using System.Text;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

using CommunityToolkit.WinUI.Helpers;

using Task_List_App.Helpers;
using Task_List_App.ViewModels;
using System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Task_List_App.Models;

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
            if (result != null)
                await Launcher.LaunchUriAsync(result);
        }
    }

    void mdReadMe_ImageClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
    {
        Debug.WriteLine($"ImageClicked: {e.Link}");
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
                        Debug.WriteLine($"[OK] UNC image resolved => '{e.Url}'");
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
                    var imageStream = await url.GetImageStream();
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

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.selectorbar.selectionchanged?view=windows-app-sdk-1.5
    /// </summary>
    async void sbTest_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender != null)
        {
            mdReadMe.Text = $"## SelectorBar selection: {sender.SelectedItem.Text}";
            //await App.MainRoot?.MessageDialogAsync("Info", $"SelectorBar selection: {sender.SelectedItem.Text}");
        }
    }

    /// <summary>
    /// <see cref="Pivot"/> control event.
    /// </summary>
    void settingPivotOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PivotItem? selectedPivot = settingPivot.SelectedItem as PivotItem;

        // Make the active pivot image colored, and inactive items grayed.
        // You could also do this with the VisualStateManager.
        foreach (PivotItem item in settingPivot.Items)
        {
            if (item == selectedPivot)
            {
                var header = item.Header as Controls.TabHeader;
                if (header != null)
                    header.SetSelectedItem(true);
            }
            else
            {
                var header = item.Header as Controls.TabHeader;
                if (header != null)
                    header.SetSelectedItem(false);
            }
        }
    }

    async void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var abb = sender as AppBarButton;
        if (abb != null )
        {
            await App.MainRoot?.MessageDialogAsync("AppBarButton", $"{abb.Label} clicked");
        }
    }

    public static string LocalMethodSample(int flag)
    {
        if (flag == 1)
            return $"{App.DatabaseTasks}";
        else if (flag == 2)
            return $"{App.DatabaseNotes}";
        else
            return $"No logic match for '{flag}'";
    }
}

/// <summary>
/// Support class for method invoking directly from the XAML.
/// This could be done using converters, but I like to show different techniques offering the same result.
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// Return the declaring type's version.
    /// </summary>
    /// <remarks>Includes string formatting.</remarks>
    public static string GetVersion()
    {
        var ver = App.GetCurrentAssemblyVersion();
        return $"Version {ver}";
    }

    /// <summary>
    /// Return the declaring type's namespace.
    /// </summary>
    public static string? GetNamespace()
    {
        var assembly = App.GetCurrentNamespace();
        return assembly ?? "Unknown";
    }

    /// <summary>
    /// Return the declaring type's assembly name.
    /// </summary>
    public static string? GetAssemblyName()
    {
        var assembly = App.GetCurrentAssemblyName()?.Split(',')[0].SeparateCamelCase();
        return assembly ?? "Unknown";
    }

    /// <summary>
    /// Returns <see cref="DateTime.Now"/> in a long format, e.g. "Wednesday, August 30, 2023"
    /// </summary>
    /// <remarks>Includes string formatting.</remarks>
    public static string GetFormattedDate()
    {
        return String.Format("{0:dddd, MMMM d, yyyy}", DateTime.Now);
    }
}


