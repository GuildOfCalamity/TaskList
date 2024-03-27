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
using Task_List_App.ViewModels;

namespace Task_List_App.Controls;

public sealed partial class ConfigFlyout : Flyout
{
    public ControlsViewModel ViewModel { get; private set; }

    public ConfigFlyout()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        ViewModel = App.GetService<ControlsViewModel>();

        this.InitializeComponent();
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SampleMenuCommand.Execute(this);
    }
}
