﻿using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;
using Task_List_App.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Task_List_App.ViewModels;

/// <summary>
/// This view model is used in tandem with the <see cref="Task_List_App.Services.ThemeSelectorService"/>.
/// </summary>
public class SettingsViewModel : ObservableRecipient
{
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private bool _showNotifications;
    private bool _acrylicBackdrop;
    private bool _persistLogin;
    private bool _isBusy = false;
    private Core.Contracts.Services.IFileService? fileService { get; set; }
    private readonly IThemeSelectorService _themeSelectorService;

    public bool AcrylicBackdrop
    {
        get => _acrylicBackdrop;
        set => SetProperty(ref _acrylicBackdrop, value);
    }

    public bool PersistLogin
    {
        get => _persistLogin;
        set => SetProperty(ref _persistLogin, value);
    }

    public bool ShowNotifications
	{
		get => _showNotifications;
		set => SetProperty(ref _showNotifications, value);
	}

	public ElementTheme ElementTheme

    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    #region [Commands]
    public ICommand SwitchThemeCommand { get; }
    public ICommand ToggleNotificationsCommand { get; }
    public ICommand PersistLoginCommand { get; }
    public ICommand AcrylicBackdropCommand { get; }
    public ICommand RestoreDataCommand { get; }
    #endregion


    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_themeSelectorService = themeSelectorService;
        
        // Setup LocalSettings.json values for other callers. 
        _elementTheme = _themeSelectorService.Theme;
        _showNotifications = _themeSelectorService.Notifications;
        _persistLogin = _themeSelectorService.PersistLogin;
        _acrylicBackdrop = _themeSelectorService.AcrylicBackdrop;
        _versionDescription = GetVersionDescription();

        // Configure theme command.
        SwitchThemeCommand = new RelayCommand<ElementTheme>(async (param) =>
        {
            if (ElementTheme != param)
            {
                ElementTheme = param;
                await _themeSelectorService.SetThemeAsync(param);
            }
        });

        // Configure notification command.
        ToggleNotificationsCommand = new RelayCommand<bool>(async (param) =>
        {
            ShowNotifications = param;
            await _themeSelectorService.SetNotificationsAsync(param);
        });

        // Configure stay logged in command.
        PersistLoginCommand = new RelayCommand<bool>(async (param) =>
        {
            PersistLogin = param;
            await _themeSelectorService.SetPersistLoginAsync(param);
        });

        // Configure acrylic backdrop command.
        AcrylicBackdropCommand = new RelayCommand<bool>(async (param) =>
        {
            AcrylicBackdrop = param;
            await _themeSelectorService.SetAcrylicBackdropAsync(param);
        });

        // Configure restore database command.
        RestoreDataCommand = new RelayCommand(async () =>
        {
            await App.ShowDialogBox("Restore?", "Are you sure you want to restore the database from backup?\nThis action cannot be undone.", "YES", "Cancel", async () =>
            {
                await RestoreDataAsync();
            },
            () =>
            {
                App.DebugLog($"User canceled restore process.");
            });
        });

        try
        {
            fileService = App.GetService<Core.Contracts.Services.IFileService>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SettingsViewModel: {ex.Message}");
            fileService = new Core.Services.FileService();
        }
    }

    /// <summary>
    /// Gives the user the ability to restore a database from within the settings page.
    /// </summary>
    /// <remarks>
    /// We'll need the <see cref="Core.Services.FileService"/> for this action.
    /// </remarks>
    async Task RestoreDataAsync()
    {
        var baseFolder = "";
        
        IsBusy = true;

        if (App.IsPackaged)
            baseFolder = ApplicationData.Current.LocalFolder.Path;
        else
            baseFolder = Directory.GetCurrentDirectory();

        var result = fileService?.Restore(baseFolder, App.DatabaseName);
        await Task.Delay(900); // prevent spamming
        if (result.HasValue && result.Value)
        {
            App.DebugLog($"Database was restored from backup.");
            await App.ShowDialogBox("Restore", "Database was restored.\nClick OK to close the application.", "OK", "Cancel", async () => 
            {
                await Task.Delay(300);
                App.Current.Exit();
            }, 
            () => 
            {
                App.DebugLog($"Application exit suggestion was ignored.");
            });
        }
        else
        {
            App.DebugLog($"Database could not be restored from backup.");
            await App.ShowDialogBox("Restore", "Database could not be restored.\nMake sure that a valid backup exists.", "OK", "Cancel", null, null);
        }
        IsBusy = false;
    }

    static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
