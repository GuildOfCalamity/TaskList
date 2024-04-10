using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Task_List_App.Contracts.Services;
using Task_List_App.Contracts.ViewModels;
using Task_List_App.Helpers;

namespace Task_List_App.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private object? _lastParameterUsed;
    private Frame? _frame;
    private string? _currentRoute;

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get
        {
            if (_frame == null)
            {
                _frame = App.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }

            return _frame;
        }

        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public string CurrentRoute
    {
        get => _currentRoute ?? "Empty";
        set => _currentRoute = value;
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public NavigationService(IPageService pageService)
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

		_pageService = pageService;
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            var vmBeforeNavigation = _frame.GetPageViewModel();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// The core navigation method which utilizes the <see cref="PageService"/>
    /// to resolve the page type based on the viewmodel.
    /// </summary>
    /// <param name="pageKey">the viewmodel, not the page type</param>
    /// <param name="parameter">optional</param>
    /// <returns>true if navigated successfully, false otherwise</returns>
    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        Debug.WriteLine($"[INFO] Getting page type for '{pageKey}'");
        var pageType = _pageService.GetPageType(pageKey);

        if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
        {
            _frame.Tag = clearNavigation;
            _currentRoute = pageKey;
            var vmBeforeNavigation = _frame.GetPageViewModel();
            var navigated = _frame.Navigate(pageType, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                    navigationAware.OnNavigatedFrom();
            }

            return navigated;
        }

        return false;
    }

    /// <summary>
    /// I added this as a way to navigate to the last used page 
    /// without the need to use the <see cref="PageService"/>.
    /// </summary>
    /// <param name="pageType">the page type, not the viewmodel</param>
    /// <param name="parameter">optional</param>
    /// <returns>true if navigated successfully, false otherwise</returns>
    public bool NavigateTo(Type pageType, object? parameter = null)
    {
        Debug.WriteLine($"[INFO] Alternative navigation for '{pageType.FullName}'");

        if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
        {
            _currentRoute = pageType.FullName;
            var vmBeforeNavigation = _frame.GetPageViewModel();
            var navigated = _frame.Navigate(pageType, parameter);
            if (navigated)
            {
                if (vmBeforeNavigation is INavigationAware navigationAware)
                    navigationAware.OnNavigatedFrom();
            }
            return navigated;
        }
        return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
                frame.BackStack.Clear();

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
                navigationAware.OnNavigatedTo(e.Parameter);

            Navigated?.Invoke(sender, e);
        }
    }
}
