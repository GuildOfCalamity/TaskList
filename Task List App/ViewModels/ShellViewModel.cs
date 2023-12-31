﻿using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

using Task_List_App.Contracts.Services;
using Task_List_App.Views;

namespace Task_List_App.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    #region [Properties]
    private bool _isBackEnabled;
    private bool _collapseOnActivate;
    private object? _selected;
    private int _badgeTotal = 0;
    private string _average = "";

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    /// <summary>
    /// Will contain the <see cref="Microsoft.UI.Xaml.Controls.NavigationViewItem"/>.
    /// </summary>
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    /// <summary>
    /// Will be displayed in the navbar as a number.
    /// </summary>
    public int BadgeTotal
    {
        get => _badgeTotal;
        set => SetProperty(ref _badgeTotal, value);
    }

    /// <summary>
    /// Will be displayed in the topmost right corner for statistics.
    /// </summary>
    public string Average
    {
        get => _average;
        set => SetProperty(ref _average, value);
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public bool CollapseOnActivate
    {
        get => _collapseOnActivate;
        set => SetProperty(ref _collapseOnActivate, value);
    }
    #endregion

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        // We must treat settings page differently
        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
