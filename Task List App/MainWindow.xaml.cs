using System;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

using Windows.UI.Popups;

using Task_List_App.Helpers;
using Task_List_App.ViewModels;

namespace Task_List_App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        InitializeComponent();

        // We're setting the icon in the ActivationService now.
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        #region [SystemBackdrop was added starting with WinAppSDK 1.3.230502 and higher]
        //if (ApplicationSettings != null && ApplicationSettings.AcrylicBackdrop)
        //{
        //    if (GeneralExtensions.IsWindows11OrGreater())
        //        SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        //    else
        //        SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
        //}
        #endregion
    }

    #region [Superfluous testing]
    void TestMethod01(string logName = "Debug.log")
    {
        Task.Run(async () =>
        {
            var fileData = await GeneralExtensions.ReadLocalFileAsync(logName);
            Debug.WriteLine($"> {((ulong)fileData.Length).ToFileSize()} bytes read from '{logName}'");
        });
    }

    async Task<int> TestDispatcherEnqueueAsyncExtension(TextBox textBox)
    {
        int crossThreadValue = await Task.Run<int>(async () =>
        {
            int fromUIThread = await textBox.DispatcherQueue.EnqueueAsync<int>(() =>
            {
                textBox.Text = "Updated from another thread.";
                return Task.FromResult(1);

            }, Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal);

            return fromUIThread + 1;
        });
        
        await Task.Delay(200);
        
        Debug.WriteLine($"The value {crossThreadValue} was returned successfully.");
        
        return crossThreadValue;
    }
    #endregion
}
