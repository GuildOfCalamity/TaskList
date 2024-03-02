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
    private const string SettingsKeyOverdueSummary = "AppOverdueSummary";
    private const string SettingsKeyPersist = "AppPersistLogin";
    private const string SettingsKeyBackdrop = "AppAcrylicBackdrop";

    public ElementTheme Theme { get; set; } = ElementTheme.Dark;
	public bool Notifications { get; set; } = true;
	public bool OverdueSummary { get; set; } = true;
    public bool PersistLogin { get; set; } = true;
	public bool AcrylicBackdrop { get; set; } = false;

    private readonly ILocalSettingsService _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_localSettingsService = localSettingsService;
    }

	/// <summary>
	/// Load settings upon activation from <see cref="Task_List_App.Services.ActivationService"/>.
	/// </summary>
	public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        Notifications = await LoadNotificationsFromSettingsAsync();
        OverdueSummary = await LoadOverdueSummaryFromSettingsAsync();
        PersistLogin = await LoadPersistLoginFromSettingsAsync();
        AcrylicBackdrop = await LoadAcrylicBackdropFromSettingsAsync();

        await Task.CompletedTask;
    }

	/// <summary>
	/// Bound to the <see cref="System.Windows.Input.ICommand"/> inside <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
	/// </summary>
	public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

	/// <summary>
	/// Bound to the <see cref="System.Windows.Input.ICommand"/> inside <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
	/// </summary>
	public async Task SetNotificationsAsync(bool enabled)
	{
		Notifications = enabled;
        await SaveNotificationsInSettingsAsync(Notifications);
    }

    /// <summary>
    /// Bound to the <see cref="System.Windows.Input.ICommand"/> inside <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
    /// </summary>
    public async Task SetOverdueSummaryAsync(bool enabled)
    {
        OverdueSummary = enabled;
        await SaveOverdueSummaryInSettingsAsync(OverdueSummary);
    }

    /// <summary>
    /// Bound to the <see cref="System.Windows.Input.ICommand"/> inside <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
    /// </summary>
    public async Task SetPersistLoginAsync(bool enabled)
    {
        PersistLogin = enabled;
        await SavePersistLoginInSettingsAsync(PersistLogin);
    }

    /// <summary>
    /// Bound to the <see cref="System.Windows.Input.ICommand"/> inside <see cref="Task_List_App.ViewModels.SettingsViewModel"/>.
    /// </summary>
    public async Task SetAcrylicBackdropAsync(bool enabled)
    {
        AcrylicBackdrop = enabled;
        await SaveAcrylicBackdropInSettingsAsync(AcrylicBackdrop);
    }

    #region [App theme settings]
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
    /// <summary>
    /// Loads theme parameter
    /// </summary>
    async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKeyTheme);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
            return cacheTheme;

        return ElementTheme.Default;
    }

	/// <summary>
	/// Saves theme parameter
	/// </summary>
	async Task SaveThemeInSettingsAsync(ElementTheme theme) => await _localSettingsService.SaveSettingAsync(SettingsKeyTheme, $"{theme}");
	#endregion

	#region [Toast notification setting]
	/// <summary>
	/// Loads notification parameter
	/// </summary>
	async Task<bool> LoadNotificationsFromSettingsAsync() => await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyNotifications);
	/// <summary>
	/// Saves notification parameter
	/// </summary>
	async Task SaveNotificationsInSettingsAsync(bool notify) => await _localSettingsService.SaveSettingAsync(SettingsKeyNotifications, $"{notify}");
    #endregion

    #region [Overdue Summary setting]
    /// <summary>
    /// Loads notification parameter
    /// </summary>
    async Task<bool> LoadOverdueSummaryFromSettingsAsync() => await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyOverdueSummary);
    /// <summary>
    /// Saves overdue summary parameter
    /// </summary>
    async Task SaveOverdueSummaryInSettingsAsync(bool summary) => await _localSettingsService.SaveSettingAsync(SettingsKeyOverdueSummary, $"{summary}");
    #endregion

    #region [Persist login setting]
    /// <summary>
    /// Loads login parameter
    /// </summary>
    async Task<bool> LoadPersistLoginFromSettingsAsync() => await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyPersist);
	/// <summary>
	/// Saves login parameter
	/// </summary>
	async Task SavePersistLoginInSettingsAsync(bool persist) => await _localSettingsService.SaveSettingAsync(SettingsKeyPersist, $"{persist}");
    #endregion

    #region [AcrylicBackdrop setting]
    /// <summary>
    /// Loads acrylic backdrop parameter
    /// </summary>
    async Task<bool> LoadAcrylicBackdropFromSettingsAsync() => await _localSettingsService.ReadSettingAsync<bool>(SettingsKeyBackdrop);
    /// <summary>
    /// Saves acrylic backdrop parameter
    /// </summary>
    async Task SaveAcrylicBackdropInSettingsAsync(bool acrylic) => await _localSettingsService.SaveSettingAsync(SettingsKeyBackdrop, $"{acrylic}");
    #endregion
}
