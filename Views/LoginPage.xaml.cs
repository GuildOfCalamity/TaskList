using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.Foundation;
using Windows.Foundation.Collections;

using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;

namespace Task_List_App.Views;

/// <summary>
/// TODO: Add pasword lookup and verification.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; private set; }
    public SettingsViewModel ApplicationSettings { get; private set; }
    public INavigationService? NavService { get; private set; }

    public LoginPage()
    {
        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        ViewModel = App.GetService<LoginViewModel>();
        ApplicationSettings = App.GetService<SettingsViewModel>();
        NavService = App.GetService<INavigationService>();

        this.Loaded += LoginPage_Loaded;
        tbPassword.KeyDown += tbPassword_KeyDown;
    }

    void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            Debug.WriteLine($"Current user {(ViewModel.IsLoggedIn ? "is" : "is not")} logged in.");
        }
        tbPassword.Focus(FocusState.Programmatic);
        tbPassword.SelectAll();
    }


    void tbPassword_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (tbPassword.Password.Length > 0 && e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.IsLoggedIn = true;
            PerformLoginNavigation();
        }
        else
            ViewModel.IsLoggedIn = false;
    }

    #region [TODO: Consolidate to one button]
    void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (tbPassword.Password.Length > 0)
        {
            ViewModel.IsLoggedIn = true;
            PerformLoginNavigation();
        }
        else
            ViewModel.IsLoggedIn = false;
    }

    void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsLoggedIn)
        {
            ViewModel.IsLoggedIn = false;
            tbPassword.Focus(FocusState.Programmatic);
            tbPassword.SelectAll();
        }
    }
    #endregion

    void PerformLoginNavigation()
    {
        if (ApplicationSettings.LastPage != null)
        {   // Alternative navigation.
            NavService?.NavigateTo(ApplicationSettings.LastPage);
        }
        else
        {   // Standard navigation.
            NavService?.NavigateTo(typeof(TasksViewModel).FullName!);
        }
    }
}
