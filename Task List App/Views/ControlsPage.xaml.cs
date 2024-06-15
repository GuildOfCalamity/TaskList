using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Task_List_App.Helpers;
using Task_List_App.ViewModels;
using Task_List_App.Contracts.Services;
using Task_List_App.Controls;
using CommunityToolkit.WinUI;

namespace Task_List_App.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ControlsPage : Page
{
    public ControlsViewModel ViewModel { get; private set; }
    public LoginViewModel LoginModel { get; private set; }
    public INavigationService? NavService { get; private set; }
    public Core.Contracts.Services.IMessageService MsgService { get; private set; }

    public ControlsPage()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        this.InitializeComponent();

        // Ensure that the Page is only created once, and cached during navigation.
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        this.ControlsNavigationView.SelectedItem = this.ControlsNavigationView.MenuItems
            .FirstOrDefault(o => o is NavigationViewItem { Content: "Tab1" });
        
        //var t = ContentFrame.ContentTransitions.FirstOrDefault(o => o is Microsoft.UI.Xaml.Media.Animation.Transition);

        NavService = App.GetService<INavigationService>();
        LoginModel = App.GetService<LoginViewModel>();
        ViewModel = App.GetService<ControlsViewModel>();
        ViewModel.ControlChangedEvent += ViewModelOnControlChanged;
        MsgService = App.GetService<Core.Contracts.Services.IMessageService>();

        this.Loaded += ControlsPage_Loaded;
        this.Unloaded += ControlsPage_Unloaded;
    }

    void ControlsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!LoginModel.IsLoggedIn)
        {
            NavService?.NavigateTo(typeof(LoginViewModel).FullName!);
            return;
        }

        // https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.animatedicon?view=winui-2.8#add-an-animatedicon-to-a-button
        abButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(AppBarButton_PointerPressed), true);
        abButton.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(AppBarButton_PointerReleased), true);
        abButton.AddHandler(UIElement.PointerEnteredEvent, new PointerEventHandler(AppBarButton_PointerEntered), true);
        abButton.AddHandler(UIElement.PointerExitedEvent, new PointerEventHandler(AppBarButton_PointerExited), true);


        // The XAML binding would sometimes throw a System.InvalidCastException:
        // Unable to cast object of type 'ComboBoxItem' to type 'InputSystemCursorShape'
        // so I have moved this to the Loaded event method for the Page.
        System.Collections.Immutable.ImmutableArray<Microsoft.UI.Input.InputSystemCursorShape> curs = CursorGrid.CursorOptions;
        if (curs != null)
            GrandParentAreaComboBox.ItemsSource = curs;
        else
            MsgService.ShowMessageBox(App.WindowHandle, "Warning", "Couldn't create ImmutableArray.", "OK", "", null, null);
    }

    void ControlsPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.animatedicon?view=winui-2.8#add-an-animatedicon-to-a-button
        abButton.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)AppBarButton_PointerPressed);
        abButton.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)AppBarButton_PointerReleased);
        abButton.RemoveHandler(UIElement.PointerEnteredEvent, (PointerEventHandler)AppBarButton_PointerEntered);
        abButton.RemoveHandler(UIElement.PointerExitedEvent, (PointerEventHandler)AppBarButton_PointerExited);
    }

    void ViewModelOnControlChanged(object? sender, string msg)
    {
        ShowInfoBar(msg, InfoBarSeverity.Informational);
    }

    public void ShowInfoBar(string message, InfoBarSeverity severity)
    {
        infoBar.DispatcherQueue?.TryEnqueue(() =>
        {
            infoBar.IsOpen = true;
            infoBar.Severity = severity;
            infoBar.Message = $"{message}";
        });
    }

    void AppBarButton_PointerEntered(object sender, PointerRoutedEventArgs e) => Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    void AppBarButton_PointerExited(object sender, PointerRoutedEventArgs e) => Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Normal");
    void AppBarButton_PointerPressed(object sender, PointerRoutedEventArgs e) => Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Pressed");
    void AppBarButton_PointerReleased(object sender, PointerRoutedEventArgs e) => Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Normal");

    /// <summary>
    /// ScrollViewer Event
    /// </summary>
    void ZoomValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (e.NewValue.IsInvalid())
            return;

        zoom.ChangeView(null, null, (float)e.NewValue);
        ViewModel.LastMessage = $"Zoom value changed to {e.NewValue}";
    }

    /// <summary>
    /// <see cref="Pivot"/> control event.
    /// </summary>
    void settingPivotOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PivotItem? selectedPivot = settingPivot.SelectedItem as PivotItem;

        ViewModel.LastMessage = $"PivotOnSelectionChanged";

        // Make the active pivot image colored, and inactive items grayed.
        // You could also do this with the VisualStateManager.
        foreach (PivotItem item in settingPivot.Items)
        {
            if (item == selectedPivot)
            {
                var header = item.Header as Controls.TabHeader;
                if (header != null)
                    header.SetSelectedItem(true);
            }
            else
            {
                var header = item.Header as Controls.TabHeader;
                if (header != null)
                    header.SetSelectedItem(false);
            }
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.selectorbar.selectionchanged?view=windows-app-sdk-1.5
    /// </summary>
    async void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender != null)
        {
            ViewModel.LastMessage = $"SelectorBar selection: {sender.SelectedItem.Text}";
            //await App.MainRoot?.MessageDialogAsync("Info", $"SelectorBar selection: {sender.SelectedItem.Text}");
        }
    }

    /// <summary>
    /// The default behavior of the <see cref="CommandBarFlyout"/> is to remain open after a menu item 
    /// is selected, we want the <see cref="CommandBarFlyout"/> to close after we've made a selection.
    /// </summary>
    /// <remarks>
    /// Refer to the <see cref="RelayCommand"/> in the <see cref="SettingsViewModel"/>.
    /// </remarks>
    void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        cbfSelector.Hide();
        ViewModel.LastMessage = $"AppBarButtonClick";
    }

    #region [Button Events - Spring Animation]
    /// <summary>
    /// Animation using the <see cref="Microsoft.UI.Composition.Compositor"/>.
    /// </summary>
    void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn != null)
        {
            ToColorStoryboard.Begin();
            CreateOrUpdateSpringAnimation(_springMultiplier);
            var uie = sender as UIElement;
            if (uie != null)
            {
                // We'll set the CenterPoint so the SpringAnimation does not start from offset 0,0.
                uie.CenterPoint = new System.Numerics.Vector3((float)(btn.ActualWidth / 2.0), (float)(btn.ActualHeight / 2.0), 1f);
                uie.StartAnimation(_springAnimation);
                if (_addOpacityAnimation)
                {
                    CreateOrUpdateScalarAnimation(true);
                    uie.StartAnimation(_scalarAnimation);
                }
            }
        }
    }

    /// <summary>
    /// Animation using the <see cref="Microsoft.UI.Composition.Compositor"/>.
    /// </summary>
    void Button_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn != null)
        {
            ToColorStoryboard.SkipToFill();
            FromColorStoryboard.Begin();
            CreateOrUpdateSpringAnimation(1.0f);
            var uie = sender as UIElement;
            if (uie != null)
            {
                // We'll set the CenterPoint so the SpringAnimation does not start from offset 0,0.
                uie.CenterPoint = new System.Numerics.Vector3((float)(btn.ActualWidth / 2.0), (float)(btn.ActualHeight / 2.0), 1f);
                uie.StartAnimation(_springAnimation);

                if (_addOpacityAnimation)
                {
                    CreateOrUpdateScalarAnimation(false);
                    uie.StartAnimation(_scalarAnimation);
                }
            }
        }
    }

    /// <summary>
    /// If testing the opacity animation we'll need to trigger this on start so the initial state is correct.
    /// </summary>
    void Button_Loaded(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn != null)
        {
            InitialColorStoryboard.Begin();
            if (_addOpacityAnimation)
            {
                CreateOrUpdateScalarAnimation(false);
                (sender as UIElement)?.StartAnimation(_scalarAnimation);
            }
        }
    }

    void ColorFlyoutButtonClick(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn != null)
        {
            ViewModel.LastMessage = $"ColorFlyoutButtonClick";

            if ($"{btn.Content}".ToLower().Contains("cancel"))
            {
                springButton.Flyout.Hide();
            }
            else
            {
                springButton.Foreground = new SolidColorBrush(colorPicker.Color);
                springButton.Flyout.Hide();
            }
        }
    }
    #endregion

    #region [Vector Animations]
    bool _addOpacityAnimation = false;
    float _springMultiplier = 1.125f;
    Microsoft.UI.Composition.ScalarKeyFrameAnimation? _scalarAnimation;
    Microsoft.UI.Composition.Vector3KeyFrameAnimation? _offsetAnimation;
    Microsoft.UI.Composition.SpringVector3NaturalMotionAnimation? _springAnimation;
    Microsoft.UI.Composition.Compositor? _compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread(); //App.CurrentWindow.Compositor;
    void CreateOrUpdateSpringAnimation(float finalValue)
    {
        if (_springAnimation == null && _compositor != null)
        {
            // When updating targets such as "Position" use a Vector3KeyFrameAnimation.
            //var positionAnim = _compositor.CreateVector3KeyFrameAnimation();
            // When updating targets such as "Opacity" use a ScalarKeyFrameAnimation.
            //var sizeAnim = _compositor.CreateScalarKeyFrameAnimation();

            _springAnimation = _compositor.CreateSpringVector3Animation();
            _springAnimation.Target = "Scale";
            _springAnimation.InitialVelocity = new System.Numerics.Vector3(_springMultiplier * 3);
            _springAnimation.DampingRatio = 0.4f;
            _springAnimation.Period = TimeSpan.FromMilliseconds(50);
        }

        if (_springAnimation != null)
            _springAnimation.FinalValue = new System.Numerics.Vector3(finalValue);
    }

    void CreateOrUpdateScalarAnimation(bool fromZeroToOne)
    {
        if (_scalarAnimation == null && _compositor != null)
        {
            _scalarAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _scalarAnimation.Target = "Opacity";
            _scalarAnimation.Direction = Microsoft.UI.Composition.AnimationDirection.Normal;
            //_scalarAnimation.IterationBehavior = Microsoft.UI.Composition.AnimationIterationBehavior.Forever;
            _scalarAnimation.Duration = TimeSpan.FromMilliseconds(1500);
        }

        if (fromZeroToOne)
        {
            _scalarAnimation?.InsertKeyFrame(0f, 0.4f);
            _scalarAnimation?.InsertKeyFrame(1f, 1f);
        }
        else
        {
            _scalarAnimation?.InsertKeyFrame(0f, 1f);
            _scalarAnimation?.InsertKeyFrame(1f, 0.4f);
        }
    }
    #endregion

    public static string LocalMethodSample(int flag)
    {
        if (flag == 1)
            return $"{App.DatabaseTasks}";
        else if (flag == 2)
            return $"{App.DatabaseNotes}";
        else
            return $"No logic match for '{flag}'";
    }

    /// <summary>
    /// Used to create a tab-style interface.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ControlsNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var current = App.GetCurrentNamespace();

        // https://learn.microsoft.com/en-us/windows/apps/design/controls/navigationview#pane-backgrounds

        // Top display mode
        //sender.Resources["NavigationViewTopPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,50,0));
        // Left display mode
        //sender.Resources["NavigationViewExpandedPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 50, 0));
        // LeftCompact/LeftMinimal
        //sender.Resources["NavigationViewDefaultPaneBackground"] = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,50,0));

        /* [Other NavigationView Resource Keys]
           x:Key="NavigationViewBackButtonBackground"                 
           x:Key="NavigationViewButtonBackgroundDisabled"             
           x:Key="NavigationViewButtonBackgroundPointerOver"          
           x:Key="NavigationViewButtonBackgroundPressed"              
           x:Key="NavigationViewButtonForegroundDisabled"             
           x:Key="NavigationViewButtonForegroundPointerOver"          
           x:Key="NavigationViewButtonForegroundPressed"              
           x:Key="NavigationViewContentBackground"                    
           x:Key="NavigationViewContentGridBorderBrush"               
           x:Key="NavigationViewDefaultPaneBackground"                
           x:Key="NavigationViewExpandedPaneBackground"               
           x:Key="NavigationViewItemBackground"                       
           x:Key="NavigationViewItemBackgroundChecked"                
           x:Key="NavigationViewItemBackgroundCheckedDisabled"        
           x:Key="NavigationViewItemBackgroundCheckedPointerOver"     
           x:Key="NavigationViewItemBackgroundCheckedPressed"         
           x:Key="NavigationViewItemBackgroundDisabled"               
           x:Key="NavigationViewItemBackgroundPointerOver"            
           x:Key="NavigationViewItemBackgroundPressed"                
           x:Key="NavigationViewItemBackgroundSelected"               
           x:Key="NavigationViewItemBackgroundSelectedDisabled"       
           x:Key="NavigationViewItemBackgroundSelectedPointerOver"    
           x:Key="NavigationViewItemBackgroundSelectedPressed"        
           x:Key="NavigationViewItemBorderBrush"                      
           x:Key="NavigationViewItemBorderBrushChecked"               
           x:Key="NavigationViewItemBorderBrushCheckedDisabled"       
           x:Key="NavigationViewItemBorderBrushCheckedPointerOver"    
           x:Key="NavigationViewItemBorderBrushCheckedPressed"        
           x:Key="NavigationViewItemBorderBrushDisabled"              
           x:Key="NavigationViewItemBorderBrushPointerOver"           
           x:Key="NavigationViewItemBorderBrushPressed"               
           x:Key="NavigationViewItemBorderBrushSelected"              
           x:Key="NavigationViewItemBorderBrushSelectedDisabled"      
           x:Key="NavigationViewItemBorderBrushSelectedPointerOver"   
           x:Key="NavigationViewItemBorderBrushSelectedPressed"       
           x:Key="NavigationViewItemForeground"                       
           x:Key="NavigationViewItemForegroundChecked"                
           x:Key="NavigationViewItemForegroundCheckedDisabled"        
           x:Key="NavigationViewItemForegroundCheckedPointerOver"     
           x:Key="NavigationViewItemForegroundCheckedPressed"         
           x:Key="NavigationViewItemForegroundDisabled"               
           x:Key="NavigationViewItemForegroundPointerOver"            
           x:Key="NavigationViewItemForegroundPressed"                
           x:Key="NavigationViewItemForegroundSelected"               
           x:Key="NavigationViewItemForegroundSelectedDisabled"       
           x:Key="NavigationViewItemForegroundSelectedPointerOver"    
           x:Key="NavigationViewItemForegroundSelectedPressed"        
           x:Key="NavigationViewItemHeaderForeground"                 
           x:Key="NavigationViewItemSeparatorForeground"              
           x:Key="NavigationViewSelectionIndicatorForeground"         
           x:Key="NavigationViewTopPaneBackground"                    
           x:Key="TopNavigationViewItemBackgroundPointerOver"         
           x:Key="TopNavigationViewItemBackgroundPressed"             
           x:Key="TopNavigationViewItemBackgroundSelected"            
           x:Key="TopNavigationViewItemBackgroundSelectedPointerOver" 
           x:Key="TopNavigationViewItemBackgroundSelectedPressed"     
           x:Key="TopNavigationViewItemForeground"                    
           x:Key="TopNavigationViewItemForegroundDisabled"            
           x:Key="TopNavigationViewItemForegroundPointerOver"         
           x:Key="TopNavigationViewItemForegroundPressed"             
           x:Key="TopNavigationViewItemForegroundSelected"            
           x:Key="TopNavigationViewItemForegroundSelectedPointerOver" 
           x:Key="TopNavigationViewItemForegroundSelectedPressed"     
           x:Key="TopNavigationViewItemRevealBackgroundFocused"       
           x:Key="TopNavigationViewItemRevealContentForegroundFocused"
           x:Key="TopNavigationViewItemRevealIconForegroundFocused"   
           x:Key="TopNavigationViewItemSeparatorForeground" 
           <x:Double x:Key="PaneToggleButtonSize">40</x:Double>
           <x:Double x:Key="PaneToggleButtonHeight">36</x:Double>
           <x:Double x:Key="PaneToggleButtonWidth">40</x:Double>
           <x:Double x:Key="NavigationViewCompactPaneLength">48</x:Double>
           <x:Double x:Key="NavigationViewTopPaneHeight">48</x:Double>
           <Thickness x:Key="NavigationViewItemInnerHeaderMargin">16,0</Thickness>
           <Thickness x:Key="NavigationViewAutoSuggestBoxMargin">16,0</Thickness>
           <Style x:Key="PaneToggleButtonStyle" TargetType="Button">
           <Setter Target="PaneToggleButtonGrid">
        */
        if (args.IsSettingsSelected is true)
        {
            // Do nothing
        }
        else if (args.SelectedItem is NavigationViewItem item && item.Content is string content && Type.GetType($"{current}.Views.{content}Page") is Type pageType)
        {
            _ = this.ContentFrame.Navigate(pageType);
        }
    }
}
