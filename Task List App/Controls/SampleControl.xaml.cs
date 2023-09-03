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

using Windows.Foundation;
using Windows.Foundation.Collections;

using Task_List_App.Models;

namespace Task_List_App.Controls;

public sealed partial class SampleControl : UserControl
{
    public TaskItem? ListDetailsTaskItem
    {
        get => GetValue(ListDetailsTaskItemProperty) as TaskItem;
        set => SetValue(ListDetailsTaskItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsTaskItemProperty = DependencyProperty.Register(
        nameof(ListDetailsTaskItem),
        typeof(TaskItem),
        typeof(SampleControl),
        new PropertyMetadata(null, OnListDetailsTaskItemPropertyChanged));


    public SampleControl()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="SampleControl"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="TaskItem"/> object contained within.
    /// </summary>
    static void OnListDetailsTaskItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is TaskItem ti)
            Debug.WriteLine($"[OnListDetailsTaskItemPropertyChanged] => {ti.Title}");
    }
}
