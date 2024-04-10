using System.Collections.Specialized;
using System.Web;

using Microsoft.Windows.AppNotifications;

using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;

namespace Task_List_App.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        // Gets called when app is launched through a Toast Notification.
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
    }

	/// <summary>
	/// TODO: Handle notification invocations when your app is already running.
	/// </summary>
	public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // Navigate to a specific page based on the notification arguments.
        if (ParseArguments(args.Argument)["action"] == "Settings")
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
            });
        }
        else
        {
			_ = App.ShowDialogBox("Notification Activation", "TODO: Handle notification invocations when your app is already running.", "OK", "Cancel", null, null);
		}
	}

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
