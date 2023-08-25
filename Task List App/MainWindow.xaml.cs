using System;
using System.Diagnostics;
using System.Reflection;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

using Windows.UI.Popups;

using Task_List_App.Helpers;

namespace Task_List_App;

public sealed partial class MainWindow : Window
{
	public MainWindow()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        InitializeComponent();

        // We're setting the icon in the ActivationService now.
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
