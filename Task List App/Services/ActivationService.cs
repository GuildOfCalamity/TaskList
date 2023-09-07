using System.Diagnostics;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Task_List_App.Activation;
using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;
using Task_List_App.Views;

namespace Task_List_App.Services;

/// <summary>
/// See <see cref="ActivationService.ActivateAsync(object)"/> for the main window instantiation 
/// and other window functions. I have removed 'WinUIEx' from this project as it was causing issues 
/// with the build-up and tear-down of <see cref="Microsoft.UI.Xaml.Window"/> when updating the 
/// WindowsAppSDK to newer versions (and yes, I updated the WinUIEx NuGet with it).
/// </summary>
public class ActivationService : IActivationService
{
    private UIElement? _shell = null;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, IThemeSelectorService themeSelectorService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
		// Execute tasks before activation.
		await InitializeAsync();

        // If this was not new'd up in the App constructor process
        if (App.MainWindow == null)
        {
            App.MainWindow = new();
            App.MainWindow.Content = null;
        }

		// Set the MainWindow Content.
		if (App.MainWindow?.Content == null)
		{
            _shell = App.GetService<ShellPage>();
			App.MainWindow.Content = _shell ?? new Frame();
            // Save the FrameworkElement for future content dialogs.
            App.MainRoot = App.MainWindow.Content as FrameworkElement; 
			// We should set the title again since we just new'd up the MainWindow.
			App.MainWindow.Title = "AppDisplayName".GetLocalized();
		}

		var AppWin = GetAppWindow(App.MainWindow);
		if (AppWin != null)
		{
			AppWin.Closing += (s, e) => { App.IsClosing = true; };

			if (App.IsPackaged)
				AppWin.SetIcon(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets/WindowIcon.ico"));
			else
				AppWin.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
		}

		// Handle activation via ActivationHandlers.
		await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        try
        {
            App.MainWindow.Activate();
            AppWin?.Resize(new Windows.Graphics.SizeInt32(1400, 800));
            CenterWindow(App.MainWindow);
        }
        catch (Exception ex)
        {
			App.DebugLog($"MainWindow.Activate: {ex.Message}");
        }

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        try
        {
			if (activationArgs is Microsoft.UI.Xaml.LaunchActivatedEventArgs arg)
			{
				if (!string.IsNullOrEmpty(arg.Arguments))
					App.DebugLog($"Received arguments: {arg.Arguments}");
			}
			else
				App.DebugLog($"Received activationArgs of type {activationArgs.GetType()}");

            var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
                await activationHandler.HandleAsync(activationArgs);

            if (_defaultHandler.CanHandle(activationArgs))
                await _defaultHandler.HandleAsync(activationArgs);
        }
        catch (Exception ex)
        {
			App.DebugLog($"HandleActivationAsync: {ex.Message}");
        }
    }

    async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        await Task.CompletedTask;
    }

	#region [Window Helpers]
	/// <summary>
	/// This code example demonstrates how to retrieve an AppWindow from a WinUI3 window.
	/// The AppWindow class is available for any top-level HWND in your app.
	/// AppWindow is available only to desktop apps (both packaged and unpackaged), it's not available to UWP apps.
	/// https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/windowing/windowing-overview
	/// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.appwindow.create?view=windows-app-sdk-1.3
	/// </summary>
	public Microsoft.UI.Windowing.AppWindow? GetAppWindow(object window)
	{
		// Retrieve the window handle (HWND) of the current (XAML) WinUI3 window.
		var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

		// For other classes to use.
		App.WindowHandle = hWnd;

		// Retrieve the WindowId that corresponds to hWnd.
		Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

		// Lastly, retrieve the AppWindow for the current (XAML) WinUI3 window.
		Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

		if (appWindow != null)
		{
			// You now have an AppWindow object, and you can call its methods to manipulate the window.
			// As an example, let's change the title text of the window: appWindow.Title = "Title text updated via AppWindow!";
			//appWindow.Move(new Windows.Graphics.PointInt32(200, 100));
			appWindow?.MoveAndResize(new Windows.Graphics.RectInt32(250, 100, 1300, 800), Microsoft.UI.Windowing.DisplayArea.Primary);
		}

		return appWindow;
	}

	/// <summary>
	/// Centers a <see cref="Microsoft.UI.Xaml.Window"/> based on the <see cref="Microsoft.UI.Windowing.DisplayArea"/>.
	/// </summary>
	public void CenterWindow(Window window)
	{
		try
		{
			System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
			Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			if (Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId) is Microsoft.UI.Windowing.AppWindow appWindow &&
				Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest) is Microsoft.UI.Windowing.DisplayArea displayArea)
			{
				Windows.Graphics.PointInt32 CenteredPosition = appWindow.Position;
				CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
				CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
				appWindow.Move(CenteredPosition);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
		}
	}

	/// <summary>
	/// The <see cref="Microsoft.UI.Windowing.DisplayArea"/> exposes properties such as:
	/// OuterBounds     (Rect32)
	/// WorkArea.Width  (int)
	/// WorkArea.Height (int)
	/// IsPrimary       (bool)
	/// DisplayId.Value (ulong)
	/// </summary>
	/// <param name="window"></param>
	/// <returns><see cref="DisplayArea"/></returns>
	public Microsoft.UI.Windowing.DisplayArea? GetDisplayArea(Window window)
	{
		try
		{
			System.IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
			Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			var da = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
			return da;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"GetDisplayArea: {ex.Message}");
			return null;
		}
	}
	#endregion
}
