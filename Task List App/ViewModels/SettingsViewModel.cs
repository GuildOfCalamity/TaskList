using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    #region [Properties]
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private bool _showNotifications;
    private bool _showOverdueSummary;
    private bool _acrylicBackdrop;
    private bool _persistLogin;
    private Type? _lastPage;
    private bool _isBusy = false;
    private SelectorBarItem? _barItem;
    private Core.Contracts.Services.IFileService? fileService { get; set; }
    private readonly IThemeSelectorService _themeSelectorService;
    public event EventHandler<string>? SettingChangedEvent;
    public Action? ProgressButtonClickEvent { get; set; }

    public bool AcrylicBackdrop
    {
        get => _acrylicBackdrop;
        set
        {
            SetProperty(ref _acrylicBackdrop, value);
            SettingChangedEvent?.Invoke(this, nameof(AcrylicBackdrop) + " was changed");
        }
    }

    public bool PersistLogin
    {
        get => _persistLogin;
        set
        {
            SetProperty(ref _persistLogin, value);
            SettingChangedEvent?.Invoke(this, nameof(PersistLogin) + " was changed");
        }
    }

    public bool ShowNotifications
	{
		get => _showNotifications;
        set
        {
            SetProperty(ref _showNotifications, value);
            SettingChangedEvent?.Invoke(this, nameof(ShowNotifications) + " was changed");
        }
    }

    public bool ShowOverdueSummary
    {
        get => _showOverdueSummary;
        set
        {
            SetProperty(ref _showOverdueSummary, value);
            SettingChangedEvent?.Invoke(this, nameof(ShowOverdueSummary) + " was changed");
        }
    }

    public ElementTheme ElementTheme

    {
        get => _elementTheme;
        set
        {
            SetProperty(ref _elementTheme, value);
            SettingChangedEvent?.Invoke(this, nameof(ElementTheme) + " was changed");
        }
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set
        {
            SetProperty(ref _versionDescription, value);
            SettingChangedEvent?.Invoke(this, nameof(VersionDescription) + " was changed");
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set 
        { 
            SetProperty(ref _isBusy, value);
            SettingChangedEvent?.Invoke(this, nameof(IsBusy) + " was changed");
        }
    }

    public SelectorBarItem? BarItem
    {
        get => _barItem;
        set
        {
            SetProperty(ref _barItem, value);
            SettingChangedEvent?.Invoke(this, nameof(BarItem) + " was changed");
        }
    }

    public Type? LastPage
    {
        get => _lastPage;
        set
        {   // There is no UI control for this parameter so
            // we'll save it when ever it's value is updated.
            _ = _themeSelectorService.SetLastPageAsync(value);

            SetProperty(ref _lastPage, value);
            SettingChangedEvent?.Invoke(this, nameof(LastPage) + " was changed");
        }
    }
    #endregion

    #region [Commands]
    public ICommand SwitchThemeCommand { get; }
    public ICommand ToggleNotificationsCommand { get; }
    public ICommand ToggleOverdueSummaryCommand { get; }
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
        _showOverdueSummary = _themeSelectorService.OverdueSummary;
        _persistLogin = _themeSelectorService.PersistLogin;
        _lastPage = _themeSelectorService.LastPage;
        _acrylicBackdrop = _themeSelectorService.AcrylicBackdrop;
        _versionDescription = GetVersionDescription();
        _lastPage = _themeSelectorService.LastPage;

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

        // Configure overdue summary command.
        ToggleOverdueSummaryCommand = new RelayCommand<bool>(async (param) =>
        {
            ShowOverdueSummary = param;
            await _themeSelectorService.SetOverdueSummaryAsync(param);
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

        // Action example for our ProgressButton.
        ProgressButtonClickEvent += async () => 
        {
            IsBusy = true;
            await Task.Delay(3000);
            IsBusy = false;
        };

        try
        {
            fileService = App.GetService<Core.Contracts.Services.IFileService>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARNING] SettingsViewModel: {ex.Message}");
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

        SettingChangedEvent?.Invoke(this, "Running restore database operation.");

        if (App.IsPackaged)
            baseFolder = ApplicationData.Current.LocalFolder.Path;
        else
            baseFolder = Directory.GetCurrentDirectory();

        var result = fileService?.Restore(baseFolder, App.DatabaseTasks);
        await Task.Delay(1000); // prevent spamming
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
