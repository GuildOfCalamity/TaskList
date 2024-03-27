using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App;

/// <summary>
/// Provides elementary Modal View services: display message, request confirmation, request input.
/// All dialogs are based on the <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/> control.
/// </summary>
public static class Modals
{
    public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message)
    {
        await MessageDialogAsync(element, title, message, "OK");
    }

    public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message, string buttonText)
    {
        if (element == null)
            return;

        try
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = buttonText,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.ActualTheme
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }
    }

    public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title)
    {
        return await ConfirmationDialogAsync(element, title, "OK", string.Empty, "Cancel");
    }

    public static async Task<bool> ConfirmationDialogAsync(this FrameworkElement element, string title, string yesButtonText, string noButtonText)
    {
        return (await ConfirmationDialogAsync(element, title, yesButtonText, noButtonText, string.Empty)).Value;
    }

    public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string yesButtonText, string noButtonText, string cancelButtonText)
    {
        if (element == null)
            return null;

        var dialog = new ContentDialog
        {
            Title = title,
            PrimaryButtonText = yesButtonText,
            SecondaryButtonText = noButtonText,
            CloseButtonText = cancelButtonText,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        try
        {
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
                return null;

            return (result == ContentDialogResult.Primary);
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }

        return null;
    }

    public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title)
    {
        return await element.InputStringDialogAsync(title, string.Empty);
    }

    public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title, string defaultText)
    {
        return await element.InputStringDialogAsync(title, defaultText, "OK", "Cancel");
    }

    public static async Task<string> InputStringDialogAsync(this FrameworkElement element, string title, string defaultText, string okButtonText, string cancelButtonText)
    {
        if (element == null)
            return string.Empty;

        var inputTextBox = new TextBox
        {
            AcceptsReturn = false,
            Height = 32,
            Text = defaultText,
            SelectionStart = defaultText.Length
        };
        var dialog = new ContentDialog
        {
            Content = inputTextBox,
            Title = title,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = okButtonText,
            SecondaryButtonText = cancelButtonText,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        try
        {
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return string.Empty;
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }

        return string.Empty;
    }

    public static async Task<string> InputTextDialogAsync(this FrameworkElement element, string title)
    {
        return await element.InputTextDialogAsync(title, string.Empty);
    }

    public static async Task<string> InputTextDialogAsync(this FrameworkElement element, string title, string defaultText)
    {
        if (element == null)
            return string.Empty;

        var inputTextBox = new TextBox
        {
            AcceptsReturn = true,
            Height = 32 * 6,
            Text = defaultText,
            TextWrapping = TextWrapping.Wrap,
            SelectionStart = defaultText.Length
        };
        var dialog = new ContentDialog
        {
            Content = inputTextBox,
            Title = title,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = "Ok",
            SecondaryButtonText = "Cancel",
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        try
        {
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return string.Empty;
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }

        return string.Empty;
    }
}

#region [Additional Dialogs]
public class NoticeDialog : ContentDialog
{
    public bool IsAborted = false;

    readonly SolidColorBrush _darkModeBackgroundBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 10, 10));
    readonly SolidColorBrush _lightModeBackgroundBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245));

    public NoticeDialog()
    {
        Background = App.Current.RequestedTheme == ApplicationTheme.Dark ? _darkModeBackgroundBrush : _lightModeBackgroundBrush;
        ActualThemeChanged += OnActualThemeChanged;
    }

    void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        Background = ActualTheme == ElementTheme.Dark ? _darkModeBackgroundBrush : _lightModeBackgroundBrush;
    }

    internal static Style GetButtonStyle(Windows.UI.Color backgroundColor)
    {
        var buttonStyle = new Microsoft.UI.Xaml.Style(typeof(Button));
        buttonStyle.Setters.Add(new Setter(Control.CornerRadiusProperty, new CornerRadius(4d)));
        buttonStyle.Setters.Add(new Setter(Control.BackgroundProperty, backgroundColor));
        buttonStyle.Setters.Add(new Setter(Control.ForegroundProperty, Colors.White));
        return buttonStyle;
    }
}
#endregion

#region [Inherited Samples]
public class GenericDialog : NoticeDialog
{
    public GenericDialog(string yesText, Action yesAction, string noText, Action noAction, string cancelText, Action cancelAction, string title, string content)
    {
        Title = title;
        HorizontalAlignment = HorizontalAlignment.Center;
        Content = content;
        PrimaryButtonText = yesText ?? "Yes";
        SecondaryButtonText = noText ?? "No";
        CloseButtonText = cancelText ?? "Cancel";
        PrimaryButtonStyle = GetButtonStyle(Windows.UI.Color.FromArgb(255, 38, 114, 201)); // light blue (#FF2672C9)

        // Configure event delegates
        PrimaryButtonClick += (dialog, eventArgs) => yesAction();
        SecondaryButtonClick += (dialog, eventArgs) => noAction();
        CloseButtonClick += (dialog, eventArgs) => cancelAction();
    }
}

public class SaveCloseDiscardDialog : NoticeDialog
{
    public SaveCloseDiscardDialog(Action saveAndExitAction, Action discardAndExitAction, Action cancelAction, string content)
    {
        Title = App.GetCurrentAssemblyName();
        HorizontalAlignment = HorizontalAlignment.Center;
        Content = content;
        PrimaryButtonText = "Save";
        SecondaryButtonText = "Discard";
        CloseButtonText = "Close";
        PrimaryButtonStyle = GetButtonStyle(Windows.UI.Color.FromArgb(255, 38, 114, 201)); // light blue (#FF2672C9)

        // Configure event delegates
        PrimaryButtonClick += (dialog, eventArgs) => saveAndExitAction();
        SecondaryButtonClick += (dialog, eventArgs) => discardAndExitAction();
        CloseButtonClick += (dialog, eventArgs) => cancelAction();
    }
}
#endregion

#region [Testing]
public static class DialogManager
{
    static TaskCompletionSource<bool> _dialogAwaiter = new TaskCompletionSource<bool>();

    public static NoticeDialog? ActiveDialog;

    /// <summary>
    /// Test driver method for showing a custom <see cref="ContentControl"/>
    /// </summary>
    /// <param name="dialog"><see cref="NoticeDialog"/></param>
    /// <param name="awaitPreviousDialog">force-hide any previously open ContentDialog</param>
    /// <returns></returns>
    public static async Task<ContentDialogResult?> OpenDialogAsync(NoticeDialog dialog, bool awaitPreviousDialog)
    {
        try
        {
            // NOTE: We must set the XamlRoot in WinUI3 (this was not needed in UWP)
            if (App.MainRoot != null)
                dialog.XamlRoot = App.MainRoot.XamlRoot;

            return await OpenDialog(dialog, awaitPreviousDialog);
        }
        catch (Exception ex)
        {
            var activeDialogTitle = string.Empty;
            var pendingDialogTitle = string.Empty;

            if (ActiveDialog?.Title is string activeTitle)
                activeDialogTitle = activeTitle;

            if (dialog?.Title is string pendingTitle)
                pendingDialogTitle = pendingTitle;

            Debug.WriteLine($"[ERROR] FailedToOpenDialog: {ex.Message}");
        }

        return null;
    }

    static async Task<ContentDialogResult> OpenDialog(NoticeDialog dialog, bool awaitPreviousDialog)
    {
        TaskCompletionSource<bool> currentAwaiter = _dialogAwaiter;
        TaskCompletionSource<bool> nextAwaiter = new TaskCompletionSource<bool>();
        _dialogAwaiter = nextAwaiter;

        // Check for previous dialogs.
        if (ActiveDialog != null)
        {
            if (awaitPreviousDialog)
            {
                await currentAwaiter.Task;
            }
            else
            {
                ActiveDialog.IsAborted = true;
                ActiveDialog.Hide();
            }
        }

        ActiveDialog = dialog;

        // Show the dialog.
        try
        {
            return await ActiveDialog.ShowAsync(ContentDialogPlacement.Popup);
        }
        finally
        {
            nextAwaiter.SetResult(true);
        }
    }
}
#endregion
