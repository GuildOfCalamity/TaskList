﻿using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using Task_List_App.Contracts.Services;
using Task_List_App.ViewModels;
using Task_List_App.Views;

namespace Task_List_App.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    /// <summary>
    /// NOTE: When adding new pages to the application do not forget to configure them here.
    /// </summary>
    public PageService()
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		Configure<TasksViewModel, TasksPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<LoginViewModel, LoginPage>();
        Configure<AlternateViewModel, AlternatePage>();
        Configure<NotesViewModel, NotesPage>();
        Configure<ControlsViewModel, ControlsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Don't forget to add the relevant Configure<ViewModel, Page>()");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>() where VM : ObservableObject where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
