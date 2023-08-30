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
using Task_List_App.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Task_List_App.Views;

public sealed partial class CreateTaskDialog : ContentDialog
{
    public string? SelectedTitle { get; private set; }
    public string? SelectedTime { get; private set; }
    public string? SelectedStatus { get; private set; }
    public TasksViewModel ViewModel { get; private set; }
    public ContentDialogResult? Result { get; private set; }

    public CreateTaskDialog()
    {
        this.InitializeComponent();
        
        ViewModel = App.GetService<TasksViewModel>();
        //SelectedTime = ViewModel.Times[ViewModel.LastSelectedTime];
        //SelectedStatus = ViewModel.Times[ViewModel.LastSelectedStatus];

        this.Loaded += CreateTaskDialog_Loaded;
        this.PrimaryButtonClick += (s, e) => { Result = ContentDialogResult.Primary; };
        this.SecondaryButtonClick += (s, e) => { Result = ContentDialogResult.Secondary; };
        this.CloseButtonClick += (s, e) => { Result = ContentDialogResult.None; };
    }

    private void CreateTaskDialog_Loaded(object sender, RoutedEventArgs e)
    {
        cbTime.SelectedIndex = ViewModel.LastSelectedTime;
        cbStatus.SelectedIndex = ViewModel.LastSelectedStatus;
        Result = ContentDialogResult.None;
    }

    void newTaskCreate_TextChanged(object sender, TextChangedEventArgs e)
    {
        SelectedTitle = newTaskCreate.Text;
    }

    void newTaskCreate_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            // Because the Hide() method does not support passing a 
            // ContentDialogResult, we will set it manually for the caller.
            Result = ContentDialogResult.Primary;

            this.Hide(); // Close the dialog and return the result.
            /*  
             *  ===[NOTE]===
             *  We could also create a new TaskItem here and assign it to the
             *  Tag of this control and then extract it from the calling Page.
             *  
             */
        }
    }

    /// <summary>
    /// We have a two-way binding for <see cref="SelectedTime"/>, so we won't need 
    /// to update the property inside this event, but it is here if you need it.
    /// Currently we're only using this to remember the user's last selected time.
    /// </summary>
    void cbTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox;
        string? text = e.AddedItems[0] as string;
        if (!string.IsNullOrEmpty(text))
        {
            Debug.WriteLine($"Time_SelectionChanged => '{text}'");
            ViewModel.LastSelectedTime = cbTime.SelectedIndex;
        }
    }

    /// <summary>
    /// We have a two-way binding for <see cref="SelectedStatus"/>, so we won't need 
    /// to update the property inside this event, but it is here if you need it.
    /// Currently we're only using this to remember the user's last selected status.
    /// </summary>
    void cbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox;
        string? text = e.AddedItems[0] as string;
        if (!string.IsNullOrEmpty(text))
        {
            Debug.WriteLine($"Status_SelectionChanged => '{text}'");
            ViewModel.LastSelectedStatus = cbStatus.SelectedIndex;
        }
    }
}