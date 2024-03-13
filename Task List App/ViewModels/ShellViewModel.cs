using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

using Task_List_App.Contracts.Services;
using Task_List_App.Views;
using Windows.Services.Maps;

namespace Task_List_App.ViewModels;

/// <summary>
/// Navigation View Model
/// </summary>
/// <remarks>
/// The properties contained within this class could be decorated with the 
/// <see cref="CommunityToolkit.Mvvm.ComponentModel.ObservablePropertyAttribute"/>
/// e.g. "[ObservableProperty]", but I have chosen to show both techniques in this
/// project. To see the classic method, refer to <see cref="Task_List_App.Models.NoteItem"/>
/// or <see cref="Task_List_App.Models.TaskItem"/>.
/// </remarks>
public class ShellViewModel : ObservableRecipient
{
    #region [Properties]
    private bool _isBackEnabled;
    private bool _collapseOnActivate;
    private object? _selected;
    private int _badgeTotal = 0;
    private int _noteTotal = 0;
    private string _average = "";

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    public SettingsViewModel ApplicationSettings { get; private set; }

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
    /// Will be displayed in the navbar as a number.
    /// </summary>
    public int NoteTotal
    {
        get => _noteTotal;
        set => SetProperty(ref _noteTotal, value);
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

        ApplicationSettings = App.GetService<SettingsViewModel>();

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
            // Save our last page for next login attempt.
            if (e.SourcePageType != typeof(LoginPage))
                ApplicationSettings.LastPage = e.SourcePageType;

            Debug.WriteLine($"[INFO] {e.SourcePageType.FullName}");
            Selected = selectedItem;
        }
    }
}
