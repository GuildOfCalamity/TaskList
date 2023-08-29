using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

using Windows.UI.Notifications;
using Windows.UI.Popups;

using Task_List_App.Contracts.Services;
using Task_List_App.Models;
using Task_List_App.ViewModels;

namespace Task_List_App.Views;

/// <summary>
/// This is considered the main landing page, so we will require other view 
/// models in order to determine the system state, e.g. Is the user logged
/// in? Does the user desire toast notifications? et al.
/// </summary>
public sealed partial class TasksPage : Page
{
    #region [Properties]
    int closeCount = 0;
    int cycleCount = 4;
    int notifyDelay = 4;
    string newTaskPrompt = "Enter title here";
	string toastTemplate = "<toast launch=\"action=ToastClick\"><visual><binding template=\"ToastGeneric\"><text>{0}</text><text>{1}</text><image placement=\"appLogoOverride\" hint-crop=\"circle\" src=\"{2}Assets/WindowIcon.ico\"/></binding></visual><actions><action content=\"Settings\" arguments=\"action=Settings\"/></actions></toast>";
    string toastImage = Path.Combine(AppContext.BaseDirectory.Replace(@"\","/"), "Assets/StoreLogo.png");
    bool initFinished = false;
    bool useMessageBox = false;
    bool useCodeBehindDialog = false;
    DateTime _lastActivity = DateTime.Now;
    Queue<Dictionary<string,InfoBarSeverity>> noticeQueue = new();
	DispatcherTimer? _timerPoll;
	DispatcherTimer? _timerMsg;
	public bool SaveNeeded { get; set; } = false; // for timer
    public static FrameworkElement? MainRoot { get; private set; } = null;
    public TasksViewModel ViewModel { get; private set; }
    public ShellViewModel ShellModel { get; private set; }
    public LoginViewModel LoginModel { get; private set; }
    public SettingsViewModel ApplicationSettings { get; private set; }
    public INavigationService? NavService { get; private set; }
    #endregion

    public TasksPage()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        InitializeComponent();
        
        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel = App.GetService<TasksViewModel>();
        ShellModel = App.GetService<ShellViewModel>();
        ApplicationSettings = App.GetService<SettingsViewModel>();
        NavService = App.GetService<INavigationService>();
        LoginModel = App.GetService<LoginViewModel>();
        ViewModel.TasksLoadedEvent += ViewModel_TasksLoadedEvent;

        MainRoot = this.Content as FrameworkElement; // for local dialogs

        this.Loaded += TasksPage_Loaded;
        this.Unloaded += TasksPage_Unloaded;
        TaskListView.Tapped += TaskListView_Tapped;
		ShellPage.MainWindowActivatedEvent += MainWindow_ActivatedEvent;
        ShellPage.ShellKeyboardEvent += ShellPage_ShellKeyboardEvent;
        ShellPage.ShellPointerEvent += ShellPage_ShellPointerEvent;

        ConfigureSystemTimers();
	}

    #region [External Events]
    async void ShellPage_ShellKeyboardEvent(object? sender, Windows.System.VirtualKey e)
    {
        _lastActivity = DateTime.Now;

        // Don't trigger action shortcuts if we're at the login page.
        if (!string.IsNullOrEmpty(NavService?.CurrentRoute) && NavService.CurrentRoute.Contains(nameof(LoginViewModel)))
            return;

        if (e == Windows.System.VirtualKey.A)
        {
            Debug.WriteLine($"[Received Keyboard Add Event]");
            if (useCodeBehindDialog)
            {
                await ShowAddTaskDialogBox("Add Task", $"{newTaskPrompt}", "OK", "Cancel", (result) =>
                {
                    Debug.WriteLine($"User selected 'OK'");
                    if (!string.IsNullOrEmpty(result))
                    {
                        noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task successfully created.", InfoBarSeverity.Success } });
                        ViewModel.AddTaskItemCommand.Execute(new TaskItem { Title = result, Time = ViewModel.Times[1], Created = DateTime.Now, Status = ViewModel.Status[1] });
                    }
                    return true;
                },
                () =>
                {
                    Debug.WriteLine($"User selected 'Cancel'");
                    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "The task was not created because it was cancelled.", InfoBarSeverity.Warning } });
                });
            }
            else
            {
                // Setup our custom ContentDialog.
                ContentDialog dialog = new CreateTaskDialog();
                dialog.CloseButtonStyle = (Style)Application.Current.Resources["ButtonStyle1"];
                dialog.XamlRoot = this.XamlRoot;

                // Stores result for use in statement
                var result = await dialog.ShowAsync();
                // User may have closed using the Enter key
                var customResult = (dialog as CreateTaskDialog)?.Result;

                // Statement to manage state detection and string handler
                if (result == ContentDialogResult.Primary || customResult == ContentDialogResult.Primary)
                {
                    string? title = (dialog as CreateTaskDialog)?.SelectedTitle;
                    string? time = (dialog as CreateTaskDialog)?.SelectedTime;
                    string? status = (dialog as CreateTaskDialog)?.SelectedStatus;
                    if (!string.IsNullOrEmpty(title))
                    {
                        ViewModel.TaskItems.Add(new TaskItem { Title = title, Time = time ?? ViewModel.Times[1], Created = DateTime.Now, Status = status ?? ViewModel.Status[1] });
                        ViewModel.ResortAllTasks();
                        noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was created successfully.", InfoBarSeverity.Success } });
                    }
                    else
                    {
                        noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was not created because the title was empty.", InfoBarSeverity.Warning } });
                    }
                }
                else
                {
                    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was not created because it was cancelled.", InfoBarSeverity.Warning } });
                }
            }
        }
        else if (e == Windows.System.VirtualKey.S)
        {
            Debug.WriteLine($"[Received Keyboard Save/Update Event]");
            SaveTask_Click(this, new RoutedEventArgs());
        }
        else if (e == Windows.System.VirtualKey.X)
        {
            App.DebugLog($"User exited through keypress.");
            await Task.Delay(1000);
			App.Current.Exit();
		}
    }

    void ShellPage_ShellPointerEvent(object? sender, Microsoft.UI.Input.PointerDeviceType e)
    {
        _lastActivity = DateTime.Now;
    }

    /// <summary>
    /// We'll save the user's data when the main window is deactivated.
    /// </summary>
    void MainWindow_ActivatedEvent(object? sender, WindowActivatedEventArgs e)
	{
        Debug.WriteLine($"[MainWindowActivatedEvent] {e.WindowActivationState}");

        if (e.WindowActivationState == WindowActivationState.Deactivated && initFinished && !App.IsClosing)
        {
            // Add debounce in scenarios where this event could be hammered.
            var idleTime = DateTime.Now - _lastActivity;
            if (idleTime.TotalSeconds >= 5 && !App.IsClosing && ViewModel != null)
            {
                Debug.WriteLine($"[MainWindowActivatedEvent] Saving current tasks.");
                // Make sure to commit any unsaved changes on exit.
                noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Saving current tasks.", InfoBarSeverity.Informational } });
				ViewModel.SignalBusyCycle(null);
                ViewModel.SaveTaskItemsJson();
            }
        }
		_lastActivity = DateTime.Now;
    }

	/// <summary>
	/// This will serve as our refresh event when the collection has changed.
	/// </summary>
	void ViewModel_TasksLoadedEvent(object? sender, bool success)
	{
		if (success)
        {
			// Since we're not using an ObservableCollection, we'll need to trigger the UI to refresh.
			TaskListView.DispatcherQueue.TryEnqueue(() =>
			{
				TaskListView.ItemsSource = null;
				TaskListView.ItemsSource = ViewModel.TaskItems;
			});
			noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Tasks have been loaded.", InfoBarSeverity.Informational } });
		}
        else
        {
            noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Tasks failed to load. See debug log for details.", InfoBarSeverity.Error } });
        }
    }
    #endregion

    #region [Button Events]
    void SaveTask_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"Calling '{nameof(ViewModel.UpdateTaskItemCommand)}'");
        _lastActivity = DateTime.Now;
        
        // Passing null to this command indicates to update all and re-load data.
        ViewModel.UpdateTaskItemCommand.Execute(null);
    }

    async void CompleteTask_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"CompleteTask_Click");
        _lastActivity = DateTime.Now;

        bool result = false;
        int amount = ViewModel.TallyCompletedTaskItems();

        if (amount > 0 && useMessageBox)
        {
            await ShowMessageBox("Finished?", $"Confirm completion of marked tasks.", "Yes", "No", 
            () => // User selected 'YES'
			{
                result = ViewModel.CompleteSelectedTaskItems();
                if (result)
				    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Completed tasks have been updated.", InfoBarSeverity.Informational } });
                else
					noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Completed tasks could not be updated.", InfoBarSeverity.Error } });
			},
            () => // User selected 'NO'
			{
				noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task marking was canceled.", InfoBarSeverity.Warning } });
			});
        }
		else if (amount > 0 && !useMessageBox)
		{
            await ShowDialogBox("Finished?", $"Confirm completion of marked tasks.", "Yes", "No", 
            () => // User selected 'YES'
			{
                result = ViewModel.CompleteSelectedTaskItems();
				if (result)
					noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Completed tasks have been updated.", InfoBarSeverity.Informational } });
				else
					noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Completed tasks could not be updated.", InfoBarSeverity.Error } });
			},
            () => // User selected 'NO'
			{
				noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task marking was canceled.", InfoBarSeverity.Warning } });
			});
		}
		else
        {
			await ShowDialogBox("Notice", $"You must select tasks before they can be marked as completed.", "OK", "Cancel", 
            () => {	Debug.WriteLine($"User selected 'OK'");	},
			() => { Debug.WriteLine($"User selected 'Cancel'"); });
		}
	}

    /// <summary>
    /// This also happens when list is fully saved.
    /// </summary>
    void SortTask_Click(object sender, RoutedEventArgs e)
    {
        _lastActivity = DateTime.Now;

        if (ViewModel.ResortAllTasks())
		    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Re-sort was successful.", InfoBarSeverity.Informational } });
        else
            noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Failed to re-sort task items.", InfoBarSeverity.Warning } });
    }

    /// <summary>
    /// Event handler for "Add" button on Task page
    /// </summary>
    async void AddTask_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _lastActivity = DateTime.Now;

        if (useCodeBehindDialog)
        {
            await ShowAddTaskDialogBox("Add Task", $"{newTaskPrompt}", "OK", "Cancel", (result) =>
            {
                Debug.WriteLine($"User selected 'OK'");
                if (!string.IsNullOrEmpty(result))
                {
                    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task successfully created.", InfoBarSeverity.Success } });
                    ViewModel.AddTaskItemCommand.Execute(new TaskItem { Title = result, Time = ViewModel.Times[1], Created = DateTime.Now, Status = ViewModel.Status[1] });
                }
                return true;
            },
            () =>
            {
                Debug.WriteLine($"User selected 'Cancel'");
                noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was not created because it was cancelled.", InfoBarSeverity.Warning } });
            });
            return;
        }
        else
        {
            // Setup our custom ContentDialog.
            ContentDialog dialog = new CreateTaskDialog();
            dialog.CloseButtonStyle = (Style)Application.Current.Resources["ButtonStyle1"];
            dialog.XamlRoot = this.XamlRoot;

            // Stores result for use in statement
            var result = await dialog.ShowAsync();
            // User may have closed using the Enter key
            var customResult = (dialog as CreateTaskDialog)?.Result;

            // Statement to manage state detection and string handler
            if (result == ContentDialogResult.Primary || customResult == ContentDialogResult.Primary)
            {
                string? title = (dialog as CreateTaskDialog)?.SelectedTitle;
                string? time = (dialog as CreateTaskDialog)?.SelectedTime;
                string? status = (dialog as CreateTaskDialog)?.SelectedStatus;
                if (!string.IsNullOrEmpty(title))
                {
                    ViewModel.TaskItems.Add(new TaskItem { Title = title, Time = time ?? ViewModel.Times[1], Created = DateTime.Now, Status = status ?? ViewModel.Status[1] });
                    ViewModel.ResortAllTasks();
                    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was created successfully.", InfoBarSeverity.Success } });
                }
                else
                {
                    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was not created because the title was empty.", InfoBarSeverity.Warning } });
                }
            }
            else
            {
                noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { "Task was not created because it was cancelled.", InfoBarSeverity.Warning } });
            }
        }
	}
    #endregion

    #region [Control Events]
    void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
        if (initFinished)
            _lastActivity = DateTime.Now;
	}

    /// <summary>
    /// This will trigger an update by the viewmodel, so we're signaling that it'll be busy soon.
    /// </summary>
    async void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && !ViewModel.IsBusy)
            await ViewModel.SignalBusyCycle(TimeSpan.FromSeconds(1.5));
    }
	/// <summary>
	/// Show an attribute of selected <see cref="TaskItem"/>.
	/// </summary>
	void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e) // Event handler
    {
        if (initFinished && TaskListView.SelectedIndex > -1)
        {
            // The IList<T> generic interface is a descendant of the ICollection<T>
            // generic interface and is the base interface of all generic lists.
            var list = e.AddedItems as IList<object>;

            // If there was no data model in the ItemTemplate...
            //foreach (ListViewItem item in TestView.Items) { Debug.WriteLine($"[ItemType] {item.GetType()}"); }

            // There could be multiple items in the IList, e.g. if SelectionMode="Multiple".
            foreach (var item in list)
            {
                if (initFinished && item is TaskItem tsk)
                {
				    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { $"Task was created {tsk.Created.ToLongDateString()}", InfoBarSeverity.Informational } });
                    Task.Run(async () => { await LocateAndLaunchUrlFromString(tsk.Title); });
                }
            }
		}
	}
    void cbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (initFinished && SaveNeeded == false)
            {
                //SaveNeeded = true;
                var cb = sender as ComboBox;
                string? text = e.AddedItems[0] as string;
				if (cb != null && !string.IsNullOrEmpty(text) && (text.StartsWith("Complete") || text.StartsWith("Finished")))
                {
                    //var kids = spItems.GetChildren();
                    //foreach (var uie in kids)
                    //{
                    //    if (uie is CheckBox cb)
                    //        cb.IsChecked = true;
                    //}

                    // This is not needed as we have directly bound the status of the TaskItem to the ComboBox.
                    //if (!ViewModel.UpdateTaskStatus(cb?.Tag as TaskItem, true))
                    //    noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { $"Status could not be changed to '{e.AddedItems[0]}'", InfoBarSeverity.Warning } });
                }
            }
        }
        catch (Exception) { }
    }
    void cbTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (initFinished && SaveNeeded == false)
            {
                var cb = sender as ComboBox;
                var td = ViewModel.FindTaskItem(cb?.Tag as string);
                if (td != null)
                {
                    Debug.WriteLine($"Found task => {td}");
                    //noticeQueue.Enqueue(new Dictionary<string, InfoBarSeverity> { { $"Time was changed to '{e.AddedItems[0]}'", InfoBarSeverity.Informational } });
                }
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// In WinUI3 desktop apps TaskListView_PointerPressed indicates 
    /// a right-click and TaskListView_Tapped indicates a left-click.
    /// </summary>
    void TaskListView_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (initFinished)
        {
            _lastActivity = DateTime.Now;
            Debug.WriteLine($"[TaskListView_Tapped]");
        }
    }
    #endregion

    #region [Page Events]
    /// <summary>
    /// Start system timers on page load.
    /// </summary>
    void TasksPage_Loaded(object sender, RoutedEventArgs e)
	{
		Debug.WriteLine($"TasksPage_Loaded");
        App.DebugLog($"TasksPage_Loaded");

        _lastActivity = DateTime.Now;
        UpdateBadgeIcon(ViewModel.TallyUncompletedTaskItems());

        if (!LoginModel.IsLoggedIn)
            NavService?.NavigateTo(typeof(LoginViewModel).FullName!);

        _timerPoll?.Start();
        _timerMsg?.Start();
    }

    /// <summary>
    /// Stop system timers on page unload.
    /// </summary>
    void TasksPage_Unloaded(object sender, RoutedEventArgs e)
	{
        Debug.WriteLine($"TasksPage_Unloaded");
        App.DebugLog($"TasksPage_Unloaded");

		_timerPoll?.Stop();
		_timerMsg?.Stop();
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
        messageDialog.Commands.Add(new UICommand($"{yesText}", (opt) => { yesAction?.Invoke(); }));
        messageDialog.Commands.Add(new UICommand($"{noText}", (opt) => { noAction?.Invoke(); }));
        messageDialog.DefaultCommandIndex = 1;
        // We must initialize the dialog with an owner.
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.WindowHandle);
        // Show the message dialog. Our DialogDismissedHandler will deal with what selection the user wants.
        await messageDialog.ShowAsync();
        // We could force the result in a separate timer...
        //DialogDismissedHandler(new UICommand("time-out"));
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
        if (MainRoot?.XamlRoot == null) { return; }

        // NOTE: Content dialogs will automatically darken the background.
        ContentDialog contentDialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryText,
            CloseButtonText = cancelText,
            CloseButtonStyle = (Style)Application.Current.Resources["ButtonStyle1"],
		    Content = new TextBlock()
            {
                Text = message,
                FontSize = 15 /*(double)App.Current.Resources["FontSizeTwo"]*/,
                /*FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["CustomFont"],*/
                TextWrapping = TextWrapping.Wrap
            },
            XamlRoot = MainRoot?.XamlRoot,
            RequestedTheme = MainRoot?.ActualTheme ?? ElementTheme.Default
        };

        ContentDialogResult result = await contentDialog.ShowAsync();

        switch (result)
        {
            case ContentDialogResult.Primary: // OK
                onPrimary?.Invoke();
                break;
            case ContentDialogResult.None:   // Cancel
                onCancel?.Invoke();
                break;
            //case ContentDialogResult.Secondary:
            //    onSecondary?.Invoke();
            //    break;
            default:
                Debug.WriteLine($"WARNING: Dialog result not defined.");
                break;
        }
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
	public static async Task ShowAddTaskDialogBox(string title, string message, string primaryText, string cancelText, Func<string, bool> primaryButtonClicked, Action? onCancel)
	{
		if (MainRoot?.XamlRoot == null) { return; }

        bool closedWithKeyStroke = false;
        ContentDialog? contentDialog = null;

		var tb = new TextBox()
        {
            Text = message,
            FontSize = (double)App.Current.Resources["MediumFontSize"],
            FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)App.Current.Resources["CustomFont"],
            TextWrapping = TextWrapping.Wrap
        };
        tb.Loaded += (s, e) => { tb.SelectAll(); };
        tb.KeyDown += (s, e) =>
        {
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				string textBoxContent = tb.Text; // Retrieve the contents of the TextBox
				closedWithKeyStroke = primaryButtonClicked?.Invoke(textBoxContent) ?? false;
				contentDialog?.Hide(); // Close the dialog
			}
		};

		// Create the content dialog.
		contentDialog = new ContentDialog()
		{
			Title = title,
			PrimaryButtonText = primaryText,
			CloseButtonText = cancelText,
			Content = tb,
			XamlRoot = MainRoot?.XamlRoot,
			RequestedTheme = MainRoot?.ActualTheme ?? ElementTheme.Default
		};

		ContentDialogResult result = await contentDialog.ShowAsync();

		switch (result)
		{
			case ContentDialogResult.Primary: // OK
				string textBoxContent = tb.Text; // Retrieve the contents of the TextBox
                var success = primaryButtonClicked?.Invoke(textBoxContent) ?? false;
                Debug.WriteLine($"Func result is {success}");
				break;
			case ContentDialogResult.None:   // Cancel
                if (!closedWithKeyStroke)
				    onCancel?.Invoke();
				break;
			//case ContentDialogResult.Secondary:
			//    onSecondary?.Invoke();
			//    break;
			default:
				Debug.WriteLine($"WARNING: Dialog result not defined.");
				break;
		}
	}
	#endregion

	#region [Toast Test]
	/// <summary>
	/// The ToastNotificationManager.GetHistory() feature does not 
	/// seem to work, probably because we are not a UWP application.
	/// [Notifications Visualizer]
	/// https://apps.microsoft.com/store/detail/notifications-visualizer/9NBLGGH5XSL1?hl=en-us&gl=us&SilentAuth=1&wa=wsignin1.0&activetab=pivot%3Aoverviewtab
	/// </summary>
	void ShowToastHistory()
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            var notes = ToastNotificationManager.History.GetHistory(App.GetCurrentAssemblyName());
            foreach (var item in notes)
            {
                // Sample one of the fields...
                var et = item.ExpirationTime;
                sb.AppendLine($"Expires: {et}");
            }
            ViewModel.Message = sb.ToString();
        }
        catch (Exception ex)
        {
            ViewModel.Message = ex.Message;
        }

        Debug.WriteLine(ViewModel.Message);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/
    /// </summary>
    void ToastTest_ImageAndText(int now, int later)
    {
        Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

        Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
        stringElements.Item(0).AppendChild(toastXml.CreateTextNode(String.Format(
            CultureInfo.InvariantCulture,
            "{0} tasks are due now!",
            now)));
        stringElements.Item(1).AppendChild(toastXml.CreateTextNode(String.Format(
            CultureInfo.InvariantCulture,
            "{0} tasks are imminent.",
            later)));

        #region [Set image tag]
        // https://learn.microsoft.com/en-us/uwp/schemas/tiles/toastschema/element-image
        var imgElement = toastXml.GetElementsByTagName("image");
        // Attribute[0] = id
        // Attribute[1] = src
        imgElement[0].Attributes[1].NodeValue = toastImage; // AppContext.BaseDirectory.Replace(@"\","/") + "Assets/StoreLogo.png"
        /*
          ===[NOTES]===
          - "ms-appx:///Assets/AppIcon.png" 
                Does not seem to work (I've tried setting the asset to Content and Resource with no luck).
                This may work with the AppNotificationBuilder, but my work PC does not support AppNotificationBuilder.
          - "file:///D:/AppIcon.png" 
                Does work, however it is read at runtime so if the asset is missing it could be a problem (notification will not show).
          - "ms-appdata:///local/Assets/AppIcon.png"
                Would be for packaged apps, I have not tested this.
          - "https://static.wixstatic.com/media/someImage.png"
                I have not tested this much.
          - Path.Combine(AppContext.BaseDirectory.Replace(@"\",@"/"), "Assets/StoreLogo.png")
                Does work and is the current technique being used here in this application.
        */
        #endregion

        ToastNotification toast = new ToastNotification(toastXml);
        toast.Activated += ToastOnActivated;
        toast.Dismissed += ToastOnDismissed;

        // NOTE: It is critical that you provide the applicationID during CreateToastNotifier().
        // It is the name that will be used in the action center to group your toasts (so in general, you put the name of your app).
        ToastNotificationManager.CreateToastNotifier(App.GetCurrentAssemblyName()).Show(toast);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/uwp/schemas/tiles/toastschema/element-image
    /// </summary>
    void TestToastXmlString(string message)
    {
        // From C:\Users\Name\source\repos\WPF\ToastNotifications\MainWindow.xaml.cs
        var toastXmlString = "<toast><visual>"
                              + "<binding template='ToastImageAndText01'>"
                              + $"<image id='1' src='{toastImage}'/>"
                              + $"<text id='1'>{message}</text>"
                              + "</binding></visual>"
                              + "<actions>"
                              + "<input id='snoozeTime' type='selection' defaultInput='15'>"
                              + "<selection id='1' content='1 minute'/>"
                              + "<selection id='15' content='15 minutes'/>"
                              + "<selection id='60' content='1 hour'/>"
                              + "<selection id='240' content='4 hours'/>"
                              + "<selection id='1440' content='1 day'/>"
                              + "</input></actions>"
                              + "</toast>";

        Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
        toastDOM.LoadXml(toastXmlString);

        ToastNotification toast = new ToastNotification(toastDOM);
        // NOTE: It is critical that you provide the applicationID during CreateToastNotifier().
        // It is the name that will be used in the action center to group your toasts (so in general, you put the name of your app).
        var tnm = ToastNotificationManager.CreateToastNotifier("MenuDemo");
        if (tnm == null)
        {
            App.DebugLog($"Could not create ToastNotificationManager.");
            return;
        }

        try
        {
            var canShow = tnm.Setting; // can throw an exception on some systems
            if (canShow != NotificationSetting.Enabled)
            {
                App.DebugLog($"Not allowed to show notifications because '{canShow}'.");
            }
            else
            {
                toast.Activated += ToastOnActivated;
                toast.Dismissed += ToastOnDismissed;
                tnm.Show(toast);
            }
        }
        catch (Exception ex)
        {
            App.DebugLog($"NotificationSetting: {ex.Message}");
            toast.Activated += ToastOnActivated;
            toast.Dismissed += ToastOnDismissed;
            tnm.Show(toast);
        }
    }

    void ToastOnActivated(ToastNotification sender, object args)
    {
        try
        {
            ToastActivatedEventArgs? tea = args as ToastActivatedEventArgs;
            App.DebugLog($"Toast activated arguments: '{tea?.Arguments}'");
            Windows.Foundation.Collections.ValueSet? vs = tea?.UserInput;
            App.DebugLog($"User input set contains {vs?.Count} elements.");

            #region [More Involved]
            // Obtain the arguments from the notification:
            //ToastArguments targs = ToastArguments.Parse(args.Argument);
            //string strAction = args.Get("action");

            // Obtain any user input (text boxes, menu selections) from the notification:
            //Windows.Foundation.Collections.ValueSet userInput = args.UserInput;
            //userInput.TryGetValue("time", out object objTime);
            #endregion
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ToastOnActivated: {ex.Message}");
        }
    }

    void ToastOnDismissed(ToastNotification sender, ToastDismissedEventArgs args)
    {
        App.DebugLog($"Toast Dismissed: reason is '{args.Reason}'");
    }


    /// <summary>
    /// The ToastNotificationManager.AddToSchedule() feature does not 
    /// seem to work, probably because we are not a UWP application.
    /// </summary>
    /// <param name="when"><see cref="DateTimeOffset"/></param>
    void ScheduleToast(DateTimeOffset when)
    {
        StringBuilder template = new StringBuilder();
        template.Append("<toast><visual version='1'><binding template='ToastText01'>");
        template.Append("<text id='1'>Enter Message:</text>");
        template.Append("</binding></visual><actions>");
        template.Append("<input id='message' type='text'/>");
        template.Append("<action activationType='foreground' content='ok' arguments='ok'/>");
        template.Append("</actions></toast>");

        Windows.Data.Xml.Dom.XmlDocument xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
        xmlDoc.LoadXml(template.ToString());

        try
        {
            ScheduledToastNotification toast = new ScheduledToastNotification(xmlDoc, when);
            ToastNotificationManager.CreateToastNotifier("MenuDemo").AddToSchedule(toast);
            if (toast.SnoozeInterval != null)
                App.DebugLog($"SnoozeInterval: {toast.SnoozeInterval}");
        }
        catch (Exception ex)
        {
            string? err = string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.Message : ex.InnerException?.Message;
            App.DebugLog($"ScheduleToast: {err ?? "null"}");
        }
    }

    void IfUsingNuGetMicrosoftToolkitUwpNotifications()
    {
        /*
        var toastContent = new ToastContent() {
            Visual = new ToastVisual() {
                BindingGeneric = new ToastBindingGeneric() {
                    Children = {
                        new AdaptiveText() { Text = "Andrew Bares" },
                        new AdaptiveText() { Text = "Incoming Call - Mobile" },
                        new AdaptiveImage() { HintCrop = AdaptiveImageCrop.Circle, Source = "https://unsplash.it/100?image=883" }
                    }
                }
            },
            Actions = new ToastActionsCustom() {
                Buttons = {
                    new ToastButton("Text reply", "action=textReply&callId=938163") { ActivationType = ToastActivationType.Foreground, ImageUri = "Assets/Icons/message.png" },
                    new ToastButton("Reminder", "action=reminder&callId=938163") { ActivationType = ToastActivationType.Background, ImageUri = "Assets/Icons/reminder.png" },
                    new ToastButton("Ignore", "action=ignore&callId=938163") { ActivationType = ToastActivationType.Background, ImageUri = "Assets/Icons/cancel.png" },
                    new ToastButton("Answer", "action=answer&callId=938163") { ActivationType = ToastActivationType.Foreground, ImageUri = "Assets/Icons/telephone.png" }
                }
            },
            Launch = "action=answer&callId=938163",
            Scenario = ToastScenario.IncomingCall
        };

        // Create the toast notification
        var toastNotif = new ToastNotification(toastContent.GetXml());

        // And send the notification
        ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        */
    }

    /// <summary>
    /// Windows 11 only?
    /// https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=appsdk
    /// </summary>
    async Task TestAppNotificationBuilder()
    {
        if (AppNotificationManager.IsSupported())
        {
            var builder = new Microsoft.Windows.AppNotifications.Builder.AppNotificationBuilder()
                .AddText("Adaptive Tiles Meeting", new Microsoft.Windows.AppNotifications.Builder.AppNotificationTextProperties().SetMaxLines(1))
                .AddText("Conference Room 2001 / Building 135")
                .AddText("10:00 AM - 10:30 AM")
                .SetInlineImage(new Uri("ms-appx:///Assets/OCR.png"))
                .AddButton(new Microsoft.Windows.AppNotifications.Builder.AppNotificationButton("Archive")
                .AddArgument("action", "archive"));
            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
        else
        {
            await ShowDialogBox("Notice", "AppNotificationManager is not supported on this platform.", "OK", "Cancel", null, null);
        }
    }

    #endregion

    #region [Misc Methods]
    /// <summary>
    /// Setup our task system timer and our messaging system timer.
    /// These timers handle other features, e.g. updating our completion time statistic.
    /// </summary>
    void ConfigureSystemTimers()
    {
        #region [Polling System]
        _timerPoll = new DispatcherTimer();
        _timerPoll.Interval = TimeSpan.FromSeconds(2.0);
        _timerPoll.Tick += (_, _) =>
        {
            try
            {
                // These logic branches could be condensed, but I have left them open for flexibility.

                if (DispatcherQueue != null && !App.IsClosing && SaveNeeded)
                {
                    #region [Check if saving needed]
                    _timerPoll.Stop();
                    Debug.WriteLine($"[UpdateTaskItem]");
                    ViewModel.UpdateTaskItemCommand.Execute(null);
                    SaveNeeded = false;
                    _timerPoll.Start();
                    #endregion
                }
                else if (DispatcherQueue != null && !App.IsClosing && ViewModel.RefreshNeeded)
                {
                    #region [Check for refresh of ListView]
                    _timerPoll.Stop();
                    Debug.WriteLine($"[RefreshNeeded]");
                    if (initFinished && ViewModel.RefreshNeeded)
                    {
                        ViewModel.RefreshNeeded = false;
                        // Since we're not using an ObservableCollection, we'll need to trigger the UI to refresh.
                        TaskListView.DispatcherQueue.TryEnqueue(() =>
                        {
                            TaskListView.ItemsSource = null;
                            TaskListView.ItemsSource = ViewModel.TaskItems;
                        });
                    }
                    _timerPoll.Start();
                    #endregion
                }
                else if (--cycleCount <= 0 && !App.IsClosing)
                {
                    #region [Check messaging and update statistics]
                    _timerPoll.Stop();
                    cycleCount = 1800; // after initial notification, slow down the occurrence

                    CalculateAverageTimeStatistic();

                    var tsk = ViewModel.GetPendingTaskItems();
                    if (tsk.Count > 0 && ApplicationSettings.ShowNotifications && LoginModel.IsLoggedIn)
                    {
                        //App.GetService<IAppNotificationService>().Show(string.Format(toastTemplate, $"Overdue: {tsk.Title}", tsk.Created.ToLongDateString(), AppContext.BaseDirectory));
                        ToastTest_ImageAndText(tsk.Count, ViewModel.TallyUncompletedTaskItems());
                        //TestToastXmlString("Is there anybody out there?");
                        //ScheduleToast(DateTimeOffset.Now.Add(new TimeSpan(0, 1, 0)));
                        //ShowToastHistory();
                    }
                    _timerPoll.Start();
                    #endregion
                }
                else if (App.IsClosing)
                {
                    _timerPoll.Stop();
                }

                #region [Check idle trigger]
                var idleTime = DateTime.Now - _lastActivity;
                if (idleTime.TotalMinutes >= 15 && !App.IsClosing)
                {
                    _lastActivity = DateTime.Now;
                    Debug.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss.fff tt")}] System Idle Detected");
                    // Switch to login page once an idle period is detected.
                    if (!ApplicationSettings.PersistLogin && !string.IsNullOrEmpty(NavService?.CurrentRoute) && !NavService.CurrentRoute.Contains(nameof(LoginViewModel)))
                    {
                        LoginModel.IsLoggedIn = false;
                        NavService?.NavigateTo(typeof(LoginViewModel).FullName!);
                    }
                }
                #endregion
            }
            catch (Exception)
            {
                Debug.WriteLine($"Application may be in the process of closing.");
            }
        };
        #endregion

        #region [Messaging System]
        _timerMsg = new DispatcherTimer();
        _timerMsg.Interval = TimeSpan.FromSeconds(1.0);
        _timerMsg.Tick += (_, _) =>
        {
            try
            {
                if (!initFinished)
                    initFinished = true;

                if (DispatcherQueue != null && !App.IsClosing)
                {   // There may be calls in the future that exceed the
                    // timer window, so stop the timer just to be safe.
                    _timerMsg.Stop();
                    if (noticeQueue.Count > 0)
                    {
                        try
                        {
                            foreach (KeyValuePair<string, InfoBarSeverity> item in noticeQueue.Dequeue().ToList())
                            {
                                switch (item.Value)
                                {
                                    case InfoBarSeverity.Informational:
                                        closeCount = 0;
                                        InfoBar.DispatcherQueue.TryEnqueue(() =>
                                        {
                                            InfoBar.Message = item.Key;
                                            InfoBar.IsOpen = true;
                                        });
                                        break;
                                    case InfoBarSeverity.Success:
                                        closeCount = 0;
                                        SuccessBar.DispatcherQueue.TryEnqueue(() =>
                                        {
                                            SuccessBar.Message = item.Key;
                                            SuccessBar.IsOpen = true;
                                        });
                                        break;
                                    case InfoBarSeverity.Warning:
                                        closeCount = 0;
                                        WarningBar.DispatcherQueue.TryEnqueue(() =>
                                        {
                                            WarningBar.Message = item.Key;
                                            WarningBar.IsOpen = true;
                                        });
                                        break;
                                    case InfoBarSeverity.Error:
                                        closeCount = 0;
                                        ErrorBar.DispatcherQueue.TryEnqueue(() =>
                                        {
                                            ErrorBar.Message = item.Key;
                                            ErrorBar.IsOpen = true;
                                        });
                                        break;
                                    default:
                                        closeCount = 0;
                                        Debug.WriteLine($"No case match.");
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"NoticeQueue: {ex.Message}");
                        }
                    }
                    else // close any InfoBars that are still open
                    {
                        if (++closeCount > notifyDelay)
                        {
                            closeCount = 0;
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                if (ErrorBar.IsOpen) { ErrorBar.IsOpen = false; }
                                if (WarningBar.IsOpen) { WarningBar.IsOpen = false; }
                                if (SuccessBar.IsOpen) { SuccessBar.IsOpen = false; }
                                if (InfoBar.IsOpen) { InfoBar.IsOpen = false; }
                            });

                            UpdateBadgeIcon(ViewModel.TallyUncompletedTaskItems());
                        }
                    }
                    _timerMsg.Start();
                }
                else if (App.IsClosing)
                {
                    _timerMsg.Stop();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine($"Application may be in the process of closing.");
            }
        };
        #endregion
    }

    void UpdateBadgeIcon(int count)
    {
        if (ShellModel.BadgeTotal != count)
            ShellModel.BadgeTotal = count;
    }

    void CalculateAverageTimeStatistic()
    {
        int count = 0;
        double tally = 0.0;

        var items = ViewModel.GetCompletionTimes();
        foreach (var item in items)
        {
            count++;
            var diff = item.Completion - item.Created;
            if (diff.HasValue)
                tally += diff.Value.TotalDays;
        }

        if (count > 0)
        {
            double avg = tally / (double)count;
            if (avg > 0.01) 
            {
                string text = $"completion average is {avg:N1} days";
                if (ShellModel.Average != text)
                    ShellModel.Average = text;
            }
        }
    }

    async Task LaunchUrlFromTextBox(TextBox textBox)
    {
        string text = "";
        textBox.DispatcherQueue.TryEnqueue(() => { text = textBox.Text; });
        Uri? uriResult;
        bool isValidUrl = Uri.TryCreate(text, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (isValidUrl)
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        else
            await Task.CompletedTask;
    }

    async Task LocateAndLaunchUrlFromTextBox(TextBox textBox)
    {
        string text = "";
        textBox.DispatcherQueue.TryEnqueue(() => { text = textBox.Text; });
        List<string> urls = ExtractUrls(text);
        if (urls.Count > 0)
        {
            Uri uriResult = new Uri(urls[0]);
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        }
        else
            await Task.CompletedTask;
    }

    async Task LocateAndLaunchUrlFromString(string text)
    {
        List<string> urls = ExtractUrls(text);
        if (urls.Count > 0)
        {
            Uri uriResult = new Uri(urls[0]);
            await Windows.System.Launcher.LaunchUriAsync(uriResult);
        }
        else
            await Task.CompletedTask;
    }

    private List<string> ExtractUrls(string text)
    {
        List<string> urls = new List<string>();
        Regex urlRx = new Regex(@"((https?|ftp|file)\://|www\.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
        MatchCollection matches = urlRx.Matches(text);
        foreach (Match match in matches) { urls.Add(match.Value); }
        return urls;
    }
    #endregion
}

