#define DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION

using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
/// </summary>
public partial class App : Application
{
    public static UIElement? shell = null;
    public static string DatabaseName { get; private set; } = "TaskItems.json";
    public static Window? MainWindow { get; set; } = new MainWindow();
    public static IntPtr WindowHandle { get; set; }
    public static FrameworkElement? MainRoot { get; set; }
	public static bool IsClosing { get; set; } = false;
    public static bool ToastLaunched { get; set; } = false;
    static ValueStopwatch stopWatch { get; set; } = ValueStopwatch.StartNew();

    // https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/#advantages-and-disadvantages-of-packaging-your-app
#if IS_UNPACKAGED // We're using a custom PropertyGroup Condition we defined in the csproj to help us with the decision.
    public static bool IsPackaged { get => false; }
#else
    public static bool IsPackaged { get => true; }
#endif

    // The .NET Microsoft.Extensions.Hosting.Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if (typeof(T).IsGenericType)
            Debug.WriteLine($"'{typeof(T).Name}' is a generic type");
        else
            Debug.WriteLine($"'{typeof(T).Name}' is not a generic type");

        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }


    #region [IoC]
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider IoCServices { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        // We could also use the CommunityToolkit's IoC Dependency Injection...
        //Ioc.Default.ConfigureServices(new ServiceCollection()
        //    .AddSingleton<IAppNotificationService, AppNotificationService>()
        //    .AddSingleton<SettingsViewModel>()
        //    .AddSingleton<SettingsPage>()
        //    .BuildServiceProvider());

        
        // Using Dependency Injection...
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IAppNotificationService, AppNotificationService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<INavigationViewService, NavigationViewService>();
        return services.BuildServiceProvider();
    }

    #endregion


    public App()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        #region [Exception handlers]
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        UnhandledException += ApplicationUnhandledException;
        #endregion

        WinRT.ComWrappersSupport.InitializeComWrappers();

        App.Current.DebugSettings.FailFastOnErrors = false;

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

            #endregion

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

            // Dump all configs...
            foreach(var cfg in context.Configuration.GetChildren())
            {
                Debug.WriteLine($"{cfg.Key} => {cfg.Value}");
            }
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        InitializeComponent();

		// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.focusvisualkind?view=windows-app-sdk-1.3
		this.FocusVisualKind = FocusVisualKind.Reveal;
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
    /// </summary>
    /// <example>
    /// var service1 = App.GetService{SomeViewModel}();
    /// var service2 = App.GetService{FileLogger}();
    /// </example>
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
            }

            // Try and locate the desired service. We're not using FirstOrDefault
            // here so that a null will be returned when an exception is thrown.
            var vm = ServicesHost?.Where(o => o.GetType() == typeof(T)).First();

            if (vm != null)
                return (T)vm;
            else
                throw new ArgumentException($"{typeof(T)} must be registered first within {MethodBase.GetCurrentMethod()?.Name}.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
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
        Debug.WriteLine($"[UnhandledException]: {e.Message}");
        Debug.WriteLine($"Unhandled exception of type {ex?.GetType()}: {ex}", $"{nameof(App)}");
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
        Debug.WriteLine($"First chance exception: {e.Exception}", $"{nameof(App)}");
        DebugLog($"First chance exception from {sender?.GetType()}: {e.Exception}");
    }

    void CurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        Debug.WriteLine($"Thread exception of type {ex?.GetType()}: {ex}", $"{nameof(App)}");
		DebugLog($"Thread exception of type {ex?.GetType()}: {ex}");
	}

	void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Debug.WriteLine($"Unobserved task exception: {e.Exception}", $"{nameof(App)}");
		DebugLog($"Unobserved task exception: {e.Exception}");
		e.SetObserved(); // suppress and handle manually
    }
    #endregion

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
    /// <param name="message">the text to write into the file</param>
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
	public static async Task ShowMessageBox(string title, string message, string primaryText, string cancelText)
	{
		// Create the dialog.
		var messageDialog = new MessageDialog($"{message}");
		messageDialog.Title = title;
		messageDialog.Commands.Add(new UICommand($"{primaryText}", new UICommandInvokedHandler(DialogDismissedHandler)));
		messageDialog.Commands.Add(new UICommand($"{cancelText}", new UICommandInvokedHandler(DialogDismissedHandler)));
		messageDialog.DefaultCommandIndex = 1;
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
		//Windows.UI.Popups.IUICommand defaultCommand = new Windows.UI.Popups.UICommand("OK");

		// NOTE: Content dialogs will automatically darken the background.
		ContentDialog contentDialog = new ContentDialog()
		{
			Title = title,
			PrimaryButtonText = primaryText,
			CloseButtonText = cancelText,
			Content = new TextBlock()
			{
				Text = message,
				FontSize = (double)App.Current.Resources["MediumFontSize"],
				FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["CustomFont"],
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
}
