using Microsoft.UI.Xaml;

namespace Task_List_App.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme { get; }
    bool Notifications { get; }
    bool PersistLogin { get; }

    Task InitializeAsync();

    Task SetThemeAsync(ElementTheme theme);

    Task SetRequestedThemeAsync();

    Task SetNotificationsAsync(bool enabled);
    Task SetPersistLoginAsync(bool enabled);
}
