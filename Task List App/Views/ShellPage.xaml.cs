using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;
using Task_List_App.ViewModels;

using Windows.System;

namespace Task_List_App.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    #region [Events that another page can subscribe to]
    public static event EventHandler<Microsoft.UI.Xaml.WindowActivatedEventArgs>? MainWindowActivatedEvent;
    public static event EventHandler<Windows.System.VirtualKey>? ShellKeyboardEvent;
    public static event EventHandler<Microsoft.UI.Input.PointerDeviceType>? ShellPointerEvent;
    #endregion

    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		ViewModel = viewModel;
        InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        if (App.MainWindow != null )
        {
            var appSettings = App.GetService<SettingsViewModel>();

            #region [Window.SystemBackdrop was added starting with WinAppSDK 1.3.230502+]
            if (appSettings != null && appSettings.AcrylicBackdrop)
            {
                if (GeneralExtensions.IsWindows11OrGreater())
                    App.MainWindow.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                else
                    App.MainWindow.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
            }
            else
            {
                SolidColorBrush? bkgndBrsh = GeneralExtensions.GetThemeResource<SolidColorBrush>("BackgroundBrush", appSettings.ElementTheme);
                if (bkgndBrsh is not null)
                    ThisPage.Background = bkgndBrsh;
                else
                    ThisPage.Background = (SolidColorBrush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"];

                // NOTE: You could also access the resource like so...
                //ThisPage.Background = (SolidColorBrush)App.Current.Resources["BackgroundBrush"];
            }
            #endregion

            // The title bar icon is currently set in the ActivationService.
            // A custom title bar is required for full window theme and Mica support.
            // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(AppTitleBar);
            App.MainWindow.Activated += MainWindow_Activated;
            App.MainWindow.VisibilityChanged += MainWindow_VisibilityChanged;
            AppTitleBarText.Text = "AppDisplayName".GetLocalized();
            this.ProcessKeyboardAccelerators += ShellPage_ProcessKeyboardAccelerators;
            this.Tapped += ShellPage_Tapped;
        }
        else
        {
            App.DebugLog($"ShellPage: MainWindow is null, cannot continue.");
        }
    }

    void MainWindow_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
        Debug.WriteLine($"[ShellPage] VisibilityChanged to {args.Visible}");
    }

    void ShellPage_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ShellPointerEvent?.Invoke(this, e.PointerDeviceType);
    }

    void ShellPage_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
    {
        Debug.WriteLine($"[ShellPage] {args.Modifiers} {args.Key}");
        //if (args.Modifiers == VirtualKeyModifiers.Menu || args.Modifiers == VirtualKeyModifiers.Control)
        if (args.Modifiers == VirtualKeyModifiers.Menu)
        {
            ShellKeyboardEvent?.Invoke(this, args.Key);
            args.Handled = true;
        }
    }

    void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
		MainWindowActivatedEvent?.Invoke(this, args);
		var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        if (ViewModel.CollapseOnActivate)
            NavigationViewControl.IsPaneOpen = false;
    }

    void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        KeyboardAccelerator? keyboardAccelerator = new() { Key = key };

        if (modifiers.HasValue)
            keyboardAccelerator.Modifiers = modifiers.Value;

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
        return keyboardAccelerator;
    }

    static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        INavigationService? navigationService = App.GetService<INavigationService>();
        bool result = navigationService.GoBack();
        args.Handled = result;
    }

    /// <summary>
    /// Change this logic to fit your needs.
    /// </summary>
    bool IsPaneToggleButtonVisible(NavigationViewDisplayMode mode)
    {
        return mode == NavigationViewDisplayMode.Expanded || mode == NavigationViewDisplayMode.Minimal;
    }

    /// <summary>
    /// Testing for future use.
    /// </summary>
    Thickness MainFrameMargin(NavigationViewDisplayMode mode)
    {
        switch (mode)
        {
            case NavigationViewDisplayMode.Expanded: 
                return new Thickness(0, 0, 0, 0);
            case NavigationViewDisplayMode.Minimal: 
                return new Thickness(0, 5, 0, 0);
            case NavigationViewDisplayMode.Compact: 
                return new Thickness(0, 10, 0, 0);
            default:
                return new Thickness(0, 15, 0, 0);
        }
    }

}
