using Microsoft.UI.Xaml;

using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;

namespace Task_List_App.Services;

/// <summary>
/// This service is used in tandem with the <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
/// </summary>
public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKeyTheme = "AppBackgroundRequestedTheme";
    private const string SettingsKeyNotifications = "AppToastNotification";
    private const string SettingsKeyPersist = "AppPersistLogin";

    public ElementTheme Theme { get; set; } = ElementTheme.Dark;
	public bool Notifications { get; set; } = true;
	public bool PersistLogin { get; set; } = true;

    private readonly ILocalSettingsService _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        Notifications = await LoadNotificationsFromSettingsAsync();
        PersistLogin = await LoadPersistLoginFromSettingsAsync();

        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

	public async Task SetNotificationsAsync(bool enabled)
	{
		Notifications = enabled;
        await SaveNotificationsInSettingsAsync(Notifications);
    }
    public async Task SetPersistLoginAsync(bool enabled)
    {
        PersistLogin = enabled;
        await SavePersistLoginInSettingsAsync(PersistLogin);
    }

    #region [Theme settings]
    /// <summary>
    /// Update the theme immediately from the service.
    /// </summary>
    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;
            TitleBarHelper.UpdateTitleBar(Theme);
        }

        await Task.CompletedTask;
    }

    async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKeyTheme);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKeyTheme, $"{theme}");
    }
    #endregion

    #region [Notifications setting]
    async Task<bool> LoadNotificationsFromSettingsAsync()
    {
        var notify = await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyNotifications);
        return notify;
    }
    async Task SaveNotificationsInSettingsAsync(bool notify)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKeyNotifications, $"{notify}");
    }
    #endregion

    #region [Persist setting]
    async Task<bool> LoadPersistLoginFromSettingsAsync()
    {
        var persist = await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyPersist);
        return persist;
    }
    async Task SavePersistLoginInSettingsAsync(bool persist)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKeyPersist, $"{persist}");
    }
    #endregion
}
