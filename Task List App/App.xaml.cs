﻿#define DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION

using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Task_List_App.Activation;
using Task_List_App.Contracts.Services;
using Task_List_App.Core.Contracts.Services;
using Task_List_App.Core.Services;
using Task_List_App.Helpers;
using Task_List_App.Models;
using Task_List_App.Notifications;
using Task_List_App.Services;
using Task_List_App.ViewModels;
using Task_List_App.Views;

using Windows.UI.Popups;

namespace Task_List_App;

/*
 *  ===[order of operations for debugging]===
 *  App__.ctor                    [03:38:28.761 PM]
 *  PageService__.ctor            [03:38:29.467 PM]
 *  NavigationService__.ctor      [03:38:29.471 PM]
 *  AppNotificationService__.ctor [03:38:29.473 PM]
 *  LocalSettingsService__.ctor   [03:38:29.658 PM]
 *  ThemeSelectorService__.ctor   [03:38:29.669 PM]
 *  ActivationService__.ctor      [03:38:29.671 PM]
 *  NavigationViewService__.ctor  [03:38:29.930 PM]
 *  ShellViewModel__.ctor         [03:38:29.934 PM]
 *  ShellPage__.ctor              [03:38:29.941 PM]
 *  TasksPage__.ctor              [03:38:30.116 PM]
 *  TasksViewModel__.ctor         [03:38:30.150 PM]
 *  TasksPage_Loaded              [Main Window Visible]
 */

/// <summary>
/// See <see cref="Task_List_App.Services.ActivationService.ActivateAsync(object)"/> for the main
/// window instantiation and other window functions. I have removed 'WinUIEx' from this project as
/// it was causing issues with the build-up and tear-down of <see cref="Microsoft.UI.Xaml.Window"/>
/// when updating the WindowsAppSDK to newer versions (and yes, I updated the WinUIEx NuGet also).
/// The Windows App SDK is currently backward compatible to Windows 10 version 1809.
/// For more info on the SDK visit https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/release-channels
/// For latest release notes visit https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel
/// For latest downloads visit https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel#downloads-for-windows-app-sdk
/// WinUI3 Apps (TemplateStudio) https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/readme.md
/// </summary>
public partial class App : Application
{
    #region [Properties]
    public static UIElement? shell = null;
    public static string DatabaseTasks { get; private set; } = "TaskItems.json";
    public static string DatabaseNotes { get; private set; } = "NoteItems.json";
    public static Window? MainWindow { get; set; } = new MainWindow();
    public static IntPtr WindowHandle { get; set; }
    public static FrameworkElement? MainRoot { get; set; }
    public static bool IsClosing { get; set; } = false;
    public static bool ToastLaunched { get; set; } = false;
    static ValueStopwatch stopWatch { get; set; } = ValueStopwatch.StartNew();
    public static EventBus RootEventBus { get; set; } = new();

    /// <summary>
    /// The .NET Microsoft.Extensions.Hosting.Host provides dependency injection, configuration, logging, and other services.
    /// https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    /// https://docs.microsoft.com/dotnet/core/extensions/configuration
    /// https://docs.microsoft.com/dotnet/core/extensions/generic-host
    /// https://docs.microsoft.com/dotnet/core/extensions/logging
    /// </summary>
    public Microsoft.Extensions.Hosting.IHost Host { get; }

    // https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/#advantages-and-disadvantages-of-packaging-your-app
#if IS_UNPACKAGED // We're using a custom PropertyGroup Condition we defined in the csproj to help us with the decision.
    public static bool IsPackaged { get => false; }
#else
    public static bool IsPackaged { get => true; }
#endif

    #endregion

    /// <summary>
    /// Uses the <see cref="Microsoft.Extensions.Hosting.IHost"/>
    /// and <see cref="System.IServiceProvider"/> to return a service
    /// object of type <typeparamref name="T"/> -or- null if there is
    /// no service object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">the service type</typeparam>
    public static T GetService<T>() where T : class
    {
        if (typeof(T).IsGenericType)
            Debug.WriteLine($"[NOTE] '{typeof(T).Name}' is a generic type.");
        else
            Debug.WriteLine($"[NOTE] '{typeof(T).Name}' is not a generic type.");

        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }
        
        return service;
    }


    #region [IoC]
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="System.IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public System.IServiceProvider? IoCServices { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    static System.IServiceProvider ConfigureServices()
    {
        // We could also use the CommunityToolkit's IoC Dependency Injection...
        //Ioc.Default.ConfigureServices(new ServiceCollection()
        //    .AddSingleton<IAppNotificationService, AppNotificationService>()
        //    .AddSingleton<SettingsViewModel>()
        //    .AddSingleton<SettingsPage>()
        //    .BuildServiceProvider());

        // Using Dependency Injection with IServiceProvider...
        ServiceCollection? services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IAppNotificationService, AppNotificationService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<INavigationViewService, NavigationViewService>();
        return services.BuildServiceProvider();
    }
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    public App()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        App.Current.DebugSettings.FailFastOnErrors = false;

        #region [New in SDK v1.5.x]
        // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel#improved-functionality-for-debugging-layout-cycles
        App.Current.DebugSettings.LayoutCycleTracingLevel = LayoutCycleTracingLevel.Low;
        App.Current.DebugSettings.LayoutCycleDebugBreakLevel = LayoutCycleDebugBreakLevel.Low;
        #endregion

        #region [Exception handlers]
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        UnhandledException += ApplicationUnhandledException;
        #endregion

        // May not be needed with newer Microsoft.WindowsAppSDK ?
        WinRT.ComWrappersSupport.InitializeComWrappers();
        
        // IoC testing
        //IoCServices = ConfigureServices();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            #region [Default Activation Handler]
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
            #endregion

            #region [Other Activation Handlers]
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();
            #endregion

            #region [Services]
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IFileService, FileService>(); // core project service
            services.AddSingleton<IMessageService, MessageService>(); // core project service
            #endregion

            #region [Views and ViewModels]
            // We could make these transient, but I want some things to be shared across the
            // app, such as the total task count for updating the <NavigationViewItem.InfoBadge>.

            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsPage>();

            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<ShellPage>();
            
            services.AddSingleton<TasksViewModel>();
            services.AddSingleton<TasksPage>();

            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<LoginPage>();

            services.AddSingleton<AlternateViewModel>();
            services.AddSingleton<AlternatePage>();

            services.AddSingleton<NotesViewModel>();
            services.AddSingleton<NotesPage>();

            services.AddSingleton<ControlsViewModel>();
            services.AddSingleton<ControlsPage>();

            services.AddSingleton<TabViewModel>();

            // NOTE: Don't forget to visit the "Task_List_App.Services.PageService" ctor and add any new pages.
            #endregion

            // Register config using the Task_List_App.Services.LocalSettingsService
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

            // Dump all configs from Microsoft.Extensions.Configuration.IConfiguration
            foreach (var cfg in context.Configuration.GetChildren())
            {
                Debug.WriteLine($"[INFO] {cfg.Key} => {cfg.Value}");
            }
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        InitializeComponent();

        if (!RootEventBus.IsSubscribed("EventBusMessage"))
            RootEventBus.Subscribe("EventBusMessage", EventBusHandlerMethod);

        // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.focusvisualkind?view=windows-app-sdk-1.3
        this.FocusVisualKind = FocusVisualKind.Reveal;

        if (Debugger.IsAttached) {
            this.DebugSettings.BindingFailed += DebugOnBindingFailed;
            this.DebugSettings.XamlResourceReferenceFailed += DebugOnXamlResourceReferenceFailed;
        }

        // PubSubService test
        _ = Task.Run(() => PubSubHeartbeat());
    }

    /// <summary>
    /// For <see cref="EventBus"/> model demonstration.
    /// </summary>
    async void EventBusHandlerMethod(object? sender, ObjectEventArgs e)
    {
        if (e.Payload == null)
            Debug.WriteLine($"Received null object event!");
        else if (e.Payload?.GetType() == typeof(System.String))
        {
            if (MainRoot != null)
                await MainRoot?.MessageDialogAsync("EventBusMessage", $"{e.Payload}");
            else
                Debug.WriteLine($"Received EventBus Payload: {e.Payload}");
        }
        else
            Debug.WriteLine($"Received EventBus Payload of type: {e.Payload?.GetType()}");
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var cmdArgs = Environment.GetCommandLineArgs();
        foreach(var arg in cmdArgs)
        {
            var assName = GetCurrentAssemblyName() ?? "N/A";
            if (!arg.Contains(assName))
            {
                if (arg.Contains("-Embedding"))
                    ToastLaunched = true;
                else
                {
                    if (App.IsPackaged)
                        System.IO.File.AppendAllText(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Debug.log"), $"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] OnLaunched: {arg}{System.Environment.NewLine}");
                    else
                        System.IO.File.AppendAllText(Path.Combine(System.AppContext.BaseDirectory, "Debug.log"), $"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] OnLaunched: {arg}{System.Environment.NewLine}");
                }
            }
        }

        base.OnLaunched(args);
        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        // This handles activating/showing our MainWindow from the ActivationService.
        await App.GetService<IActivationService>().ActivateAsync(args);

        Debug.WriteLine($"─── OnLaunched finished at {stopWatch.GetElapsedTime().ToTimeString()} ───");
    }

    #region [Home-brew service getter]
    /// <summary>
    /// Returns an instance of the desired service type. Use this if you can't/won't pass
    /// the service through the target's constructor. Can be used from any window/page/model.
    /// The 1st call to this method will add and instantiate the pre-defined services.
    /// <example><code>
    /// var service1 = App.GetService{SomeViewModel}();
    /// var service2 = App.GetService{FileLogger}();
    /// </code></example>
    /// </summary>
    /// <remarks>
    /// This is an alternative for the packages "Microsoft.Extensions.DependencyInjection" 
    /// and "Microsoft.Extensions.Hosting".
    /// </remarks>
    public static T? GetServiceAlt<T>() where T : class
    {
        try
        {
            // New-up the services container if needed.
            if (ServicesHost == null) { ServicesHost = new List<Object>(); }

            // If 1st time then add relevant services to the container.
            // This could be done elsewhere, e.g. in the main constructor.
            if (ServicesHost.Count == 0)
            {
                ServicesHost?.Add(new ShellViewModel(App.GetService<INavigationService>(), App.GetService<INavigationViewService>()));
                ServicesHost?.Add(new SettingsViewModel(App.GetService<IThemeSelectorService>()));
                ServicesHost?.Add(new NavigationService(App.GetService<IPageService>()));
                ServicesHost?.Add(new PageService());
                ServicesHost?.Add(new FileService());
                ServicesHost?.Add(new AlternateViewModel());
                ServicesHost?.Add(new NotesViewModel());
                ServicesHost?.Add(new LoginViewModel());
                ServicesHost?.Add(new TasksViewModel());
                ServicesHost?.Add(new ControlsViewModel());
            }

            // Try and locate the desired service. We're not using FirstOrDefault
            // here so that a null will be returned when an exception is thrown.
            var vm = ServicesHost?.Where(o => o.GetType() == typeof(T)).First();

            if (vm != null)
                return (T)vm;
            else
                throw new ArgumentException($"{typeof(T)} must be registered first within {System.Reflection.MethodBase.GetCurrentMethod()?.Name}.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
            return null;
        }
    }
    public static List<Object>? ServicesHost { get; private set; }
    #endregion

    #region [Domain Events]
    void ApplicationUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        Exception? ex = e.Exception;
        Debug.WriteLine($"[UnhandledException]: {ex?.Message}");
        Debug.WriteLine($"Unhandled exception of type {ex?.GetType()}: {ex}");
        DebugLog($"Unhandled Exception StackTrace: {Environment.StackTrace}");
        DebugLog($"{ex?.DumpFrames()}");
        e.Handled = true;
    }

    void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        if (!IsClosing)
            IsClosing = true;

        if (sender is null)
            return;

        if (sender is AppDomain ad)
        {
            Debug.WriteLine($"[OnProcessExit]", $"{nameof(App)}");
            Debug.WriteLine($"DomainID: {ad.Id}", $"{nameof(App)}");
            Debug.WriteLine($"FriendlyName: {ad.FriendlyName}", $"{nameof(App)}");
            Debug.WriteLine($"BaseDirectory: {ad.BaseDirectory}", $"{nameof(App)}");
        }
    }

    void CurrentDomainFirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        Debug.WriteLine($"[ERROR] First chance exception from {sender?.GetType()}: {e.Exception.Message}");
        DebugLog($"First chance exception from {sender?.GetType()}: {e.Exception.Message}");
        if (e.Exception.InnerException != null)
            DebugLog($"  => InnerException: {e.Exception.InnerException.Message}");
        DebugLog($"First Chance Exception StackTrace: {Environment.StackTrace}");
        DebugLog($"{e.Exception.DumpFrames()}");
    }

    void CurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        Debug.WriteLine($"[ERROR] Thread exception of type {ex?.GetType()}: {ex}");
        DebugLog($"Thread exception of type {ex?.GetType()}: {ex}");
        DebugLog($"{ex?.DumpFrames()}");
    }

    void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception is AggregateException aex)
        {
            aex?.Flatten().Handle(ex =>
            {
                Debug.WriteLine($"[ERROR] Unobserved task exception: {ex?.Message}");
                DebugLog($"Unobserved task exception: {ex?.Message}");
                DebugLog($"{ex?.DumpFrames()}");
                return true;
            });
        }
        e.SetObserved(); // suppress and handle manually
    }
    #endregion

    #region [Debugger Events]
    void DebugOnXamlResourceReferenceFailed(DebugSettings sender, XamlResourceReferenceFailedEventArgs args)
    {
        Debug.WriteLine($"[WARNING] XamlResourceReferenceFailed: {args.Message}");
        DebugLog($"OnXamlResourceReferenceFailed: {args.Message}");
    }

    void DebugOnBindingFailed(object sender, BindingFailedEventArgs args)
    {
        Debug.WriteLine($"[WARNING] BindingFailed: {args.Message}");
        DebugLog($"OnBindingFailed: {args.Message}");
    }
    #endregion

    #region [Public Static Helpers]
    public static TimeSpan GetStopWatch(bool reset = false)
    {
        var ts = stopWatch.GetElapsedTime();
        if (reset) { stopWatch = ValueStopwatch.StartNew(); }
        return ts;
    }

    /// <summary>
    /// Returns the declaring type's namespace.
    /// </summary>
    public static string? GetCurrentNamespace()
    {
        return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;
    }

    /// <summary>
    /// Returns the declaring type's assembly name.
    /// </summary>
    public static string? GetCurrentAssemblyName()
    {
        //return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName;
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
    }

    /// <summary>
    /// Returns the AssemblyVersion, not the FileVersion.
    /// </summary>
    public static Version GetCurrentAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
    }

    /// <summary>
    /// Simplified debug logger for app-wide use.
    /// </summary>
    /// <param name="message">the text to append to the file</param>
    public static void DebugLog(string message)
    {
        try
        {
            if (App.IsPackaged)
                System.IO.File.AppendAllText(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Debug.log"), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}{Environment.NewLine}");
            else
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, "Debug.log"), $"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}{Environment.NewLine}");
        }
        catch (Exception)
        {
            Debug.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt")}] {message}");
        }
    }
    #endregion

    #region [Dialog Helpers]
    /// <summary>
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> does not look as nice as the
    /// <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> and is not part of the native Microsoft.UI.Xaml.Controls.
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> offers the <see cref="Windows.UI.Popups.UICommandInvokedHandler"/> 
    /// callback, but this could be replaced with actions. Both can be shown asynchronously.
    /// </summary>
    /// <remarks>
    /// You'll need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Windows.UI.Popups.MessageDialog"/>,
    /// because the <see cref="Microsoft.UI.Xaml.XamlRoot"/> does not exist and an owner must be defined.
    /// </remarks>
    public static async Task ShowMessageBox(string title, string message, string yesText, string noText, Action? yesAction, Action? noAction)
    {
        if (App.WindowHandle == IntPtr.Zero) { return; }

        // Create the dialog.
        var messageDialog = new MessageDialog($"{message}");
        messageDialog.Title = title;

        if (!string.IsNullOrEmpty(yesText))
        {
            messageDialog.Commands.Add(new UICommand($"{yesText}", (opt) => { yesAction?.Invoke(); }));
            messageDialog.DefaultCommandIndex = 0;
        }

        if (!string.IsNullOrEmpty(noText))
        {
            messageDialog.Commands.Add(new UICommand($"{noText}", (opt) => { noAction?.Invoke(); }));
            messageDialog.DefaultCommandIndex = 1;
        }

        // We must initialize the dialog with an owner.
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.WindowHandle);
        // Show the message dialog. Our DialogDismissedHandler will deal with what selection the user wants.
        await messageDialog.ShowAsync();
        // We could force the result in a separate timer...
        //DialogDismissedHandler(new UICommand("time-out"));
    }

    /// <summary>
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> does not look as nice as the
    /// <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> and is not part of the native Microsoft.UI.Xaml.Controls.
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> offers the <see cref="Windows.UI.Popups.UICommandInvokedHandler"/> 
    /// callback, but this could be replaced with actions. Both can be shown asynchronously.
    /// </summary>
    /// <remarks>
    /// You'll need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Windows.UI.Popups.MessageDialog"/>,
    /// because the <see cref="Microsoft.UI.Xaml.XamlRoot"/> does not exist and an owner must be defined.
    /// You'll find other implementations in the <see cref="Task_List_App.Core.Services.MessageService"/>.
    /// </remarks>
    public static async Task ShowMessageBox(string title, string message, string primaryText, string cancelText)
    {
        // Create the dialog.
        var messageDialog = new MessageDialog($"{message}");
        messageDialog.Title = title;

        if (!string.IsNullOrEmpty(primaryText))
        {
            messageDialog.Commands.Add(new UICommand($"{primaryText}", new UICommandInvokedHandler(DialogDismissedHandler)));
            messageDialog.DefaultCommandIndex = 0;
        }

        if (!string.IsNullOrEmpty(cancelText))
        {
            messageDialog.Commands.Add(new UICommand($"{cancelText}", new UICommandInvokedHandler(DialogDismissedHandler)));
            messageDialog.DefaultCommandIndex = 1;
        }
        // We must initialize the dialog with an owner.
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.WindowHandle);
        // Show the message dialog. Our DialogDismissedHandler will deal with what selection the user wants.
        await messageDialog.ShowAsync();

        // We could force the result in a separate timer...
        //DialogDismissedHandler(new UICommand("time-out"));
    }

    /// <summary>
    /// Callback for the selected option from the user.
    /// </summary>
    static void DialogDismissedHandler(IUICommand command)
    {
        Debug.WriteLine($"UICommand.Label => {command.Label}");
    }

    /// <summary>
    /// The <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> looks much better than the
    /// <see cref="Windows.UI.Popups.MessageDialog"/> and is part of the native Microsoft.UI.Xaml.Controls.
    /// The <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> does not offer a <see cref="Windows.UI.Popups.UICommandInvokedHandler"/>
    /// callback, but in this example was replaced with actions. Both can be shown asynchronously.
    /// </summary>
    /// <remarks>
    /// There is no need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/>,
    /// but a <see cref="Microsoft.UI.Xaml.XamlRoot"/> must be defined since it inherits from <see cref="Microsoft.UI.Xaml.Controls.Control"/>.
    /// </remarks>
    public static async Task ShowDialogBox(string title, string message, string primaryText, string cancelText, Action? onPrimary, Action? onCancel)
    {
        double fontSize = 16;
        Microsoft.UI.Xaml.Media.FontFamily fontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas");

        if (App.Current.Resources["MediumFontSize"] is not null)
            fontSize = (double)App.Current.Resources["MediumFontSize"];

        if (App.Current.Resources["CustomFont"] is not null)
            fontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["CustomFont"];


        // NOTE: Content dialogs will automatically darken the background.
        ContentDialog contentDialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryText,
            CloseButtonText = cancelText,
            Content = new TextBlock()
            {
                Text = message,
                FontSize = fontSize,
                FontFamily = fontFamily,
                TextWrapping = TextWrapping.Wrap
            },
            XamlRoot = App.MainRoot?.XamlRoot,
            RequestedTheme = App.MainRoot?.ActualTheme ?? ElementTheme.Default
        };

        ContentDialogResult result = await contentDialog.ShowAsync();

        switch (result)
        {
            case ContentDialogResult.Primary:
                onPrimary?.Invoke();
                break;
            //case ContentDialogResult.Secondary:
            //    onSecondary?.Invoke();
            //    break;
            case ContentDialogResult.None: // Cancel
                onCancel?.Invoke();
                break;
            default:
                Debug.WriteLine($"Dialog result not defined.");
                break;
        }
    }
    #endregion

    /// <summary>
    /// If <see cref="App.WindowHandle"/> is set then a call to User32 <see cref="SetForegroundWindow(nint)"/> 
    /// will be invoked. I tried using the native OverlappedPresenter.Restore(true), but that does not work.
    /// </summary>
    public static void ActivateMainWindow()
    {
        if (App.WindowHandle != IntPtr.Zero)
            _ = SetForegroundWindow(App.WindowHandle);

        //if (AppWin is not null && AppWin.Presenter is not null && AppWin.Presenter is OverlappedPresenter op)
        //    op.Restore(true);
    }

    public async Task PubSubHeartbeat()
    {
        try
        {
            while (!IsClosing)
            {
                await Task.Delay(5000);
                PubSubEnhanced<ApplicationMessage>.Instance.SendMessage(new ApplicationMessage
                {
                    Module = ModuleId.App,
                    MessageText = $"🔔 Heartbeat",
                    MessageType = typeof(string),
                });
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// To my knowledge there is no way to get this natively via the WinUI3 SDK, so I'm adding a P/Invoke.
    /// </summary>
    /// <returns>the amount of displays the system recognizes</returns>
    public static int GetMonitorCount()
    {
        int count = 0;

        MonitorEnumProc callback = (IntPtr hDesktop, IntPtr hdc, ref ScreenRect prect, int d) => ++count > 0;

        if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0))
        {
            Debug.WriteLine($"[INFO] You have {count} {(count > 1 ? "monitors" : "monitor")}.");
            return count;
        }
        else
        {
            Debug.WriteLine("[WARNING] An error occurred while enumerating monitors.");
            return 1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ScreenRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref ScreenRect pRect, int dwData);

    #region [User32 Imports]
#pragma warning disable CS0414
    static int SW_HIDE = 0;
    static int SW_SHOWNORMAL = 1;
    static int SW_SHOWMINIMIZED = 2;
    static int SW_SHOWMAXIMIZED = 3;
    static int SW_SHOWNOACTIVATE = 4;
    static int SW_SHOW = 5;
    static int SW_MINIMIZE = 6;
    static int SW_SHOWMINNOACTIVE = 7;
    static int SW_SHOWNA = 8;
    static int SW_RESTORE = 9;
    static int SW_SHOWDEFAULT = 10;
    static int SW_FORCEMINIMIZE = 11;
#pragma warning restore CS0414
    [DllImport("User32.dll")]
    internal static extern bool ShowWindow(IntPtr handle, int nCmdShow);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
    #endregion
}
