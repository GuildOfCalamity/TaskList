using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;

using Windows.ApplicationModel;

namespace Task_List_App.ViewModels;

/// <summary>
/// This view model is used in tandem with the <see cref="Task_List_App.Services.ThemeSelectorService"/>.
/// </summary>
public class SettingsViewModel : ObservableRecipient
{
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private bool _showNotifications;
    private bool _persistLogin;

    private readonly IThemeSelectorService _themeSelectorService;

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

    public ICommand SwitchThemeCommand { get; }
    public ICommand ToggleNotificationsCommand { get; }
    public ICommand PersistLoginCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _showNotifications = _themeSelectorService.Notifications;
        _persistLogin = _themeSelectorService.PersistLogin;
        _versionDescription = GetVersionDescription();

        // Configure theme command.
        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        // Configure notification command.
        ToggleNotificationsCommand = new RelayCommand<bool>(
            async (param) =>
            {
                ShowNotifications = param;
                await _themeSelectorService.SetNotificationsAsync(param);
            });

        // Configure stay logged in command.
        PersistLoginCommand = new RelayCommand<bool>(
            async (param) =>
            {
                PersistLogin = param;
                await _themeSelectorService.SetPersistLoginAsync(param);
            });
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
