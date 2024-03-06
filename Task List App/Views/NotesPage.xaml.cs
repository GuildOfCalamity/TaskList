using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.System;
using Windows.UI.Core;

namespace Task_List_App.Views;

/// <summary>
/// All controls can be found here:
/// https://github.com/MicrosoftDocs/winui-api/tree/docs/microsoft.ui.xaml.controls
/// </summary>
public sealed partial class NotesPage : Page
{
    bool _doneLoading = false;

    public static event EventHandler<bool>? TextChangedEvent;
    public static event EventHandler<bool>? PageUnloadedEvent;
    public NotesViewModel ViewModel { get; private set; }
    public LoginViewModel LoginModel { get; private set; }
    public INavigationService? NavService { get; private set; }

    public NotesPage()
    {
        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        
        NavService = App.GetService<INavigationService>();
        LoginModel = App.GetService<LoginViewModel>();
        ViewModel = App.GetService<NotesViewModel>();

        this.Loaded += NotesPage_Loaded;
        this.Unloaded += NotesPage_Unloaded;
    }

    /// <summary>
    /// Signal any changes back to the <see cref="NotesViewModel"/> from the <see cref="Page"/>.
    /// </summary>
    void NotesPage_Unloaded(object sender, RoutedEventArgs e) => PageUnloadedEvent?.Invoke(this, true);

    void NotesPage_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] {MethodBase.GetCurrentMethod()?.Name}");
        try
        {
            if (!LoginModel.IsLoggedIn)
            {
                NavService?.NavigateTo(typeof(LoginViewModel).FullName!);
                return;
            }

            _doneLoading = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Signal any changes back to the <see cref="NotesViewModel"/> from the <see cref="TextBox"/>.
    /// </summary>
    void TextBox_TitleChanged(object sender, TextChangedEventArgs e)
    {
        if (_doneLoading)
            TextChangedEvent?.Invoke(this, true);
        else
            TextChangedEvent?.Invoke(this, false);
    }

    /// <summary>
    /// Handle TAB press so we don't switch focus to another control.
    /// </summary>
    void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Tab)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
                var selStart = tb.SelectionStart;
                var content = tb.Text;
                tb.Text = content.Insert(selStart, "\t");
                tb.SelectionStart = selStart + 1;
                e.Handled = true;
            }
        }
    }
}
