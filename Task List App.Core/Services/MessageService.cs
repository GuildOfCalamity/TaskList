using System;
using System.Diagnostics;
using Task_List_App.Core.Contracts.Services;
using Windows.UI.Popups; // We've set the target OS to "Windows" in the project props so our DLL scope can resolve this.

namespace Task_List_App.Core.Services;

/// <summary>
/// The <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> is unknown from the DLL 
/// scope, but the <see cref="Windows.UI.Popups.MessageDialog"/> is not, so in the 
/// interest of simplicity we will use the <see cref="Windows.UI.Popups.MessageDialog"/>
/// even though it is the uglier of the two. We could try adding WinUI NuGets to this
/// sub-project, but that could become overly complicated.
/// </summary>
public class MessageService : IMessageService
{
    /// <summary>
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> does not look as nice as the
    /// <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> and is not part of the native Microsoft.UI.Xaml.Controls.
    /// The <see cref="Windows.UI.Popups.MessageDialog"/> offers the <see cref="Windows.UI.Popups.UICommandInvokedHandler"/> 
    /// callback, but this could be replaced with actions. Both can be shown asynchronously.
    /// </summary>
    /// <remarks>
    /// You'll need to call <see cref="WinRT.Interop.InitializeWithWindow.Initialize"/> when using the <see cref="Windows.UI.Popups.MessageDialog"/>,
    /// because the <see cref="Microsoft.UI.Xaml.XamlRoot"/> does not exist and an owner must be defined.
    /// The <paramref name="windowHandle"/> is a must for this dialog service abstraction.
    /// </remarks>
    public async Task ShowMessageBoxAsync(IntPtr windowHandle, string title, string message, string okText, string cancelText, Action? okAction, Action? cancelAction)
    {
        if (windowHandle == IntPtr.Zero)
            return;

        // Create the dialog.
        var messageDialog = new MessageDialog($"{message}");
        messageDialog.Title = title;
        //messageDialog.Commands.Add(new UICommand($"{okText}", new UICommandInvokedHandler(DialogCommandHandler)));
        messageDialog.Commands.Add(new UICommand($"{okText}", (c) =>
        {
            if (okAction != null)
            {
                try
                {
                    okAction?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] {nameof(okAction)}: {ex.Message}");
                }
            }
        }));
        messageDialog.Commands.Add(new UICommand($"{cancelText}", (c) =>
        {
            if (cancelAction != null)
            {
                try
                {
                    cancelAction?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] {nameof(cancelAction)}: {ex.Message}");
                }
            }
        }));
        messageDialog.DefaultCommandIndex = 1;

        // We must initialize the dialog with an owner.
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, windowHandle);

        // Show the message dialog.
        await messageDialog.ShowAsync();

        // We could force the result in a separate timer...
        //DialogCommandHandler(new UICommand("time-out"));
    }

    public void ShowMessageBox(IntPtr windowHandle, string title, string message, string okText, string cancelText, Action? okAction, Action? cancelAction)
    {
        if (windowHandle == IntPtr.Zero)
            return;

        // Create the dialog.
        var messageDialog = new MessageDialog($"{message}");
        messageDialog.Title = !string.IsNullOrEmpty(title) ? title : "Application";
        //messageDialog.Commands.Add(new UICommand($"{okText}", new UICommandInvokedHandler(DialogCommandHandler)));
        if (!string.IsNullOrEmpty(okText))
        {
            messageDialog.Commands.Add(new UICommand($"{okText}", (c) =>
            {
                if (okAction != null)
                {
                    try
                    {
                        okAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] {nameof(okAction)}: {ex.Message}");
                    }
                }
            }));
        }
        if (!string.IsNullOrEmpty(cancelText))
        {
            messageDialog.Commands.Add(new UICommand($"{cancelText}", (c) =>
            {
                if (cancelAction != null)
                {
                    try
                    {
                        cancelAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] {nameof(cancelAction)}: {ex.Message}");
                    }
                }
            }));
        }
        messageDialog.DefaultCommandIndex = 1;

        // We must initialize the dialog with an owner.
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, windowHandle);

        // Show the message dialog.
        var uic = messageDialog.ShowAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Callback for the selected option from the user.
    /// </summary>
    void DialogCommandHandler(IUICommand command)
    {
        Debug.WriteLine($"User selected '{command.Label}'");
    }

    #region [Specific to Microsoft.WindowsAppSDK]
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
    //public static async Task ShowDialogBox(Microsoft.UI.Xaml.XamlRoot xamlRoot, (Microsoft.UI.Xaml.ElementTheme theme, string title, string message, string primaryText, string cancelText, Action? onPrimary, Action? onCancel)
    //{
    //    // NOTE: Content dialogs will automatically darken the background.
    //    Microsoft.UI.Xaml.Controls.ContentDialog contentDialog = new Microsoft.UI.Xaml.Controls.ContentDialog()
    //    {
    //        Title = title,
    //        PrimaryButtonText = primaryText,
    //        CloseButtonText = cancelText,
    //        Content = new TextBlock()
    //        {
    //            Text = message,
    //            FontSize = (double)App.Current.Resources["MediumFontSize"],
    //            FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["CustomFont"],
    //            TextWrapping = TextWrapping.Wrap
    //        },
    //        XamlRoot = xamlRoot,
    //        RequestedTheme = theme
    //    };
    //
    //    Microsoft.UI.Xaml.Controls.ContentDialogResult result = await contentDialog.ShowAsync();
    //
    //    switch (result)
    //    {
    //        case Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary:
    //            onPrimary?.Invoke();
    //            break;
    //        //case ContentDialogResult.Secondary:
    //        //    onSecondary?.Invoke();
    //        //    break;
    //        case Microsoft.UI.Xaml.Controls.ContentDialogResult.None: // Cancel
    //            onCancel?.Invoke();
    //            break;
    //        default:
    //            Debug.WriteLine($"Dialog result not defined.");
    //            break;
    //    }
    //}
    #endregion
}

