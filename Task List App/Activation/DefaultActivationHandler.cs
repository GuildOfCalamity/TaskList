using Microsoft.UI.Xaml;

using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;

namespace Task_List_App.Activation;

/// <summary>
/// The ActivationService is in charge of handling the application's initialization and activation.
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
/// ActivationHandlers documentation:
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
/// </summary>
public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_navigationService = navigationService;
    }

    /// <summary>
    /// Validation 
    /// </summary>
    /// <param name="args"><see cref="LaunchActivatedEventArgs"/></param>
    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    /// <summary>
    /// This will be our first navigation event during startup.
    /// </summary>
    /// <param name="args"><see cref="LaunchActivatedEventArgs"/></param>
    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, args.Arguments);
        await Task.CompletedTask;
    }
}
