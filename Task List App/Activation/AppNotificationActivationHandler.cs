using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;

namespace Task_List_App.Activation;

/// <summary>
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
/// </summary>
public class AppNotificationActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    private readonly IAppNotificationService _notificationService;

    public AppNotificationActivationHandler(INavigationService navigationService, IAppNotificationService notificationService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_navigationService = navigationService;
        _notificationService = notificationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        // TODO: Handle notification activations.

        /*
        // Access the AppNotificationActivatedEventArgs.
        var activatedEventArgs = (AppNotificationActivatedEventArgs)AppInstance.GetCurrent().GetActivatedEventArgs().Data;

        // Navigate to a specific page based on the notification arguments.
        if (_notificationService.ParseArguments(activatedEventArgs.Argument)["action"] == "Settings")
        {
            // Queue navigation with low priority to allow the UI to initialize.
            App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
            });
        }
        */

        await App.ShowDialogBox("Notification Activation", "TODO: Handle notification activations.", "OK", "Cancel", null, null);

        await Task.CompletedTask;
    }
}
