using System;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

using Windows.UI.Popups;

using Task_List_App.Helpers;
using Task_List_App.ViewModels;
using Microsoft.UI.Xaml.Hosting;

namespace Task_List_App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        InitializeComponent();

        // I'm setting the icon in the ActivationService now.
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        var wxm = WindowsXamlManager.InitializeForCurrentThread();

        DispatcherQueue.ShutdownStarting += DispatcherQueue_ShutdownStarting;
        DispatcherQueue.FrameworkShutdownStarting += DispatcherQueue_FrameworkShutdownStarting;

        #region [SystemBackdrop was added starting with WinAppSDK 1.3.230502+]
        // NOTE: I've moved this to the ShellPage constructor since we're using a Activation/Navigation style app.
        //if (ApplicationSettings != null && ApplicationSettings.AcrylicBackdrop)
        //{
        //    if (GeneralExtensions.IsWindows11OrGreater())
        //        SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        //    else
        //        SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
        //}
        #endregion
    }

    void DispatcherQueue_ShutdownStarting(Microsoft.UI.Dispatching.DispatcherQueue sender, Microsoft.UI.Dispatching.DispatcherQueueShutdownStartingEventArgs args)
    {
        Debug.WriteLine($"[INFO] DispatcherQueue_ShutdownStarting at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
    }

    void DispatcherQueue_FrameworkShutdownStarting(Microsoft.UI.Dispatching.DispatcherQueue sender, Microsoft.UI.Dispatching.DispatcherQueueShutdownStartingEventArgs args)
    {
        Debug.WriteLine($"[INFO] DispatcherQueue_FrameworkShutdownStarting at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
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
