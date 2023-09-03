using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Task_List_App.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Task_List_App.Controls;

public sealed partial class CardControl : UserControl
{
    public TaskItem? SourceTaskItems
    {
        get => GetValue(SourceTaskItemsProperty) as TaskItem;
        set => SetValue(SourceTaskItemsProperty, value);
    }

    public static readonly DependencyProperty SourceTaskItemsProperty = DependencyProperty.Register(
        nameof(SourceTaskItems),
        typeof(TaskItem),
        typeof(SampleControl),
        new PropertyMetadata(null, OnSourceTaskItemsPropertyChanged));

    public CardControl()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="SampleControl"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="TaskItem"/> object contained within.
    /// </summary>
    static void OnSourceTaskItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is TaskItem ti)
            Debug.WriteLine($"[OnSourceTaskItemsPropertyChanged] => {ti.Title}");
    }
}
