using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Task_List_App.Models;
using Task_List_App.ViewModels;
using Microcharts;
using SkiaSharp;
using System.Collections.Concurrent;
using ColorCode.Compilation.Languages;
using System.Runtime.ConstrainedExecution;
using Task_List_App.Helpers;

namespace Task_List_App.Views;

/// <summary>
/// This is a scratch-pad area for testing different features.
/// </summary>
public sealed partial class AlternatePage : Page
{
    #region [Properties]
    int closeCount = 0;
    int notifyHangTime = 7;
    DispatcherTimer? _timerMsg;
    TaskItem? SelectedTask = null;
    Queue<Dictionary<string, string>> noticeQueue = new();

    public AlternateViewModel ViewModel { get; private set; }
    public TasksViewModel TasksViewModel { get; private set; }
    public SettingsViewModel ConfigViewModel { get; private set;
    }

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

        this.Loaded += (_, _) =>
        { 
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
                        var clr = GeneralExtensions.GetRandomColorString(ConfigViewModel.ElementTheme);
                        _entriesComp.Add(new ChartEntry((float)diff.Value.TotalDays)
                        {
                            Label = " ",
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
        };

        this.Unloaded += (_, _) => 
        { 
            _timerMsg?.Stop();
            _entriesComp.Clear();
        };
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
}
