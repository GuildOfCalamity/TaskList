using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.Foundation;
using Windows.Foundation.Collections;

using Microcharts;
using SkiaSharp;

using Task_List_App.Models;
using Task_List_App.ViewModels;
using Task_List_App.Helpers;
using Windows.Services.Maps;
using Task_List_App.Contracts.Services;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Task_List_App.Views;

/// <summary>
/// This is a scratch-pad area for testing different features.
/// </summary>
public sealed partial class AlternatePage : Page
{
    #region [Properties]
    int closeCount = 0;
    const int notifyHangTime = 7;
    DispatcherTimer? _timerMsg;
    TaskItem? SelectedTask = null;
    Queue<Dictionary<string, string>> noticeQueue = new();

    public AlternateViewModel ViewModel { get; private set; }
    public TasksViewModel TasksViewModel { get; private set; }
    public SettingsViewModel ConfigViewModel { get; private set; }
    public LoginViewModel LoginModel { get; private set; }
    public INavigationService? NavService { get; private set; }

    public List<ChartEntry> _entriesComp = new();
    public Chart CompChart
    {
        get
        {
            var chart = new LineChart
            {
                Entries = _entriesComp,
                LabelOrientation = Microcharts.Orientation.Horizontal,
                ValueLabelOrientation = Microcharts.Orientation.Horizontal,
                LabelTextSize = 11,
                EnableYFadeOutGradient = true,
                Typeface = SKTypeface.FromFile(System.IO.Path.Combine(Environment.CurrentDirectory, "Assets\\Fonts\\Aptos.ttf")),
                IsAnimated = false,
                //Margin = -10,
                //AnimationDuration = TimeSpan.FromMilliseconds(250)
            };

            return chart;
        }
    }
    #endregion

    public AlternatePage()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        ViewModel = App.GetService<AlternateViewModel>();
        TasksViewModel = App.GetService<TasksViewModel>();
        ConfigViewModel = App.GetService<SettingsViewModel>();
        NavService = App.GetService<INavigationService>();
        LoginModel = App.GetService<LoginViewModel>();

        this.InitializeComponent();

        TaskItemRepeater.ItemsSource = ViewModel.Samples;
        SelectedTask = ViewModel.Samples[0];
        // We're now setting this from the XAML.
        //TaskItemList.ItemsSource = ViewModel.Samples;
        //CardItemList.ItemsSource = ViewModel.Samples;

        #region [Handle Message Queue]
        _timerMsg = new DispatcherTimer();
        _timerMsg.Interval = TimeSpan.FromSeconds(0.75d);
        _timerMsg.Tick += (_, _) =>
        {
            try
            {
                if (this.DispatcherQueue != null && !App.IsClosing)
                {
                    _timerMsg.Stop();
                    if (noticeQueue.Count > 0)
                    {
                        try
                        {
                            foreach (KeyValuePair<string, string> item in noticeQueue.Dequeue().ToList())
                            {
                                closeCount = 0;
                                InfoBar.DispatcherQueue.TryEnqueue(() =>
                                {
                                    InfoBar.Message = item.Key;
                                    InfoBar.Title = item.Value;
                                    InfoBar.IsOpen = true;
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"NoticeQueue: {ex.Message}");
                        }
                    }
                    else // close any InfoBars that are still open
                    {
                        if (++closeCount > notifyHangTime)
                        {
                            closeCount = 0;
                            DispatcherQueue.TryEnqueue(() => { if (InfoBar.IsOpen) { InfoBar.IsOpen = false; } });
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

        this.Loaded += AlternatePage_Loaded;
        this.Unloaded += (_, _) => 
        { 
            _timerMsg?.Stop();
            _entriesComp.Clear();
        };
    }

    void AlternatePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!LoginModel.IsLoggedIn)
        {
            NavService?.NavigateTo(typeof(LoginViewModel).FullName!);
            return;
        }

        _timerMsg?.Start();

        // Fetch and render our statistic graph.
        var items = TasksViewModel.GetCompletionTimes().OrderBy(t => t.Created);
        if (items.Any())
        {
            foreach (var item in items)
            {
                var diff = item.Completion - item.Created;
                if (diff.HasValue)
                {
                    string title = "";
                    if (item.Title.Length > 8)
                        title = String.Format("{0}{1}",  item.Title.Substring(0, 9).Trim(), '\u2026');
                    else
                        title = item.Title;

                    // If you want a random color for each element on the graph.
                    var clr = GeneralExtensions.GetRandomColorString(ConfigViewModel.ElementTheme);
                    // If you want the color to indicate how close to the estimate you were.
                    clr = GetColorTime(item.Time, diff).ToString();

                    _entriesComp.Add(new ChartEntry((float)diff.Value.TotalDays)
                    {
                        Label = $"{title}",
                        TextColor = SKColor.Parse(clr),
                        ValueLabel = $"{diff.Value.TotalDays:N1} days",
                        ValueLabelColor = SKColor.Parse(clr),
                        Color = SKColor.Parse(clr)
                    });
                }
            }
        }
        else
        {
            noticeQueue.Enqueue(new Dictionary<string, string> { { $"You must complete tasks before their time scale can be graphed.", "No Data" } });
        }
    }

    /// <summary>
    /// Change event for the <see cref="ItemsRepeater"/> control.
    /// </summary>
    void VariedRepeaterOnElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
    {
        Debug.WriteLine($"Element is a '{args.Element.GetType().FullName}'");
        noticeQueue.Enqueue(new Dictionary<string, string> { { $"Element is a '{args.Element.GetType().FullName}'", "RepeaterIndexChanged" } });
    }

    /// <summary>
    /// Tap event for page resource <see cref="UserControl"/>.
    /// </summary>
    void OnTemplatePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.Samples.Count > 0)
        {
            if (SelectedTask != null)
            {
                var oldIndex = ViewModel.Samples.IndexOf(SelectedTask);
                var previousItem = TaskItemRepeater.TryGetElement(oldIndex);
                if (previousItem != null)
                    MoveToNoticeSelection(previousItem, false); // de-select previous task item
            }
            var itemIndex = TaskItemRepeater.GetElementIndex(sender as UIElement);
            SelectedTask = ViewModel.Samples[itemIndex != -1 ? itemIndex : 0];
            MoveToNoticeSelection(sender as UIElement, true); // select current task item
            noticeQueue.Enqueue(new Dictionary<string, string> { { $"{SelectedTask.Title}", "TemplatePointerPressed" } });
        }
        else
        {
            Debug.WriteLine($"TaskItem data is absent");
            noticeQueue.Enqueue(new Dictionary<string, string> { { $"TaskItem data is absent", "TemplatePointerPressed" } });
        }
    }

    /// <summary>
    /// Activate the proper <see cref="VisualStateGroup"/> for the control.
    /// </summary>
    static void MoveToNoticeSelection(UIElement previousItem, bool isSelected)
    {
        try { VisualStateManager.GoToState(previousItem as Control, isSelected ? "Selected" : "Default", false); }
        catch (NullReferenceException ex) { Debug.WriteLine($"[{previousItem.GetType().Name}] {ex.Message}"); }
    }

    /// <summary>
    /// Demonstrate extracting the data from the selected element of the <see cref="UserControl"/>.
    /// </summary>
    void TaskItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TaskItem ti)
        {
            if (ti != null)
            {
                Debug.WriteLine($"[TaskItemList_SelectionChanged] => {ti.Title}");
                noticeQueue.Enqueue(new Dictionary<string, string> { { $"{ti.Title}", "SampleListView" } });
            }
        }
    }

    /// <summary>
    /// Demonstrate extracting the data from the selected element of the <see cref="UserControl"/>.
    /// </summary>
    void CardItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TaskItem ti)
        {
            if (ti != null)
            {
                Debug.WriteLine($"[TaskItemList_SelectionChanged] => {ti.Title}");
                noticeQueue.Enqueue(new Dictionary<string, string> { { $"{ti.Title}", "CardListView" } });
            }
        }
    }

    /// <summary>
    /// Returns a <see cref="Windows.UI.Color"/> based on the window of time met from the initial task.
    /// </summary>
    Windows.UI.Color GetColorTime(string value, TimeSpan? amount)
    {
        if (string.IsNullOrEmpty(value) || amount == null)
            return Windows.UI.Color.FromArgb(255, 75, 10, 255);

        switch (value)
        {
            case string time when time.Contains("a year", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 172)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 250)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 365)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("six months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 60)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 90)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 180)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("a month", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 10)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 20)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 30)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("two months", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 20)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 40)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 60)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("two weeks", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 7)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 10)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 14)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("a week", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 3.5)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 5)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 7)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("few days", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 3)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 4)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 5)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("tomorrow", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 1.0)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 2.0)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 3.0)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            case string time when time.Contains("soon", StringComparison.OrdinalIgnoreCase) || time.Contains("today", StringComparison.OrdinalIgnoreCase):
                {
                    if (amount?.TotalDays < 1.0)
                        return Windows.UI.Color.FromArgb(255, 76, 255, 10);  // green
                    else if (amount?.TotalDays < 1.5)
                        return Windows.UI.Color.FromArgb(255, 255, 216, 10); // yellow
                    else if (amount?.TotalDays < 2.0)
                        return Windows.UI.Color.FromArgb(255, 255, 106, 10); // orange
                    else
                        return Windows.UI.Color.FromArgb(255, 255, 10, 10);  // red
                }
            default:
                return Windows.UI.Color.FromArgb(255, 75, 10, 255);          // purple
        }
    }
}
