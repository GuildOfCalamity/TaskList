namespace Task_List_App.Activation;

/// <summary>
/// The ActivationService is in charge of handling the application's initialization and activation.
/// Extend this class to implement new ActivationHandlers.
/// See DefaultActivationHandler for an example:
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md
/// General documentation on WinUI navigation:
/// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
/// </summary>
public abstract class ActivationHandler<T> : IActivationHandler where T : class
{
    /// <summary>
    /// Override this method to add the logic for whether to handle the activation.
    /// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
    /// </summary>
    protected virtual bool CanHandleInternal(T args) => true;

    /// <summary>
    /// Override this method to add the logic for your activation handler.
    /// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
    /// </summary>
    protected abstract Task HandleInternalAsync(T args);

    /// <summary>
    /// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
    /// </summary>
    public bool CanHandle(object args) => args is T && CanHandleInternal((args as T)!);

    /// <summary>
    /// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md#activationhandlers
    /// </summary>
    public async Task HandleAsync(object args) => await HandleInternalAsync((args as T)!);
}
