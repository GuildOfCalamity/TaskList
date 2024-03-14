using System.Windows.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App.Controls;

/// <summary>
/// TODO: We could create a version of this by inheriting from <see cref="ButtonBase"/> instead of <see cref="UserControl"/>.
/// Checkout the "\WindowsCommunityToolkit-rel-winui-7.1.2\CommunityToolkit.WinUI.UI.Controls.Media\Eyedropper" control
/// for more information on creating a <see cref="ButtonBase"/> control (it's more involved than this example).
/// </summary>
public sealed partial class ProgressButton : UserControl
{
    /// <summary>
    /// This control offers the flexibility for an <see cref="Action"/> 
    /// to be bound to the control or an <see cref="ICommand"/>.
    /// The <see cref="ButtonBusy"/> property determines if the
    /// <see cref="ProgressBar"/> is shown while the command or
    /// action is executing.
    /// </summary>
    public ProgressButton()
    {
        this.InitializeComponent();
        this.Loaded += ProgressButton_Loaded;
        this.SizeChanged += ProgressButton_SizeChanged;
    }

    #region [Dependency Properties]
    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The button's click event property.
    /// </summary>
    public Action? ButtonEvent
    {
        get => (Action?)GetValue(ButtonEventProperty);
        set => SetValue(ButtonEventProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonEvent
    /// </summary>
    public static readonly DependencyProperty ButtonEventProperty = DependencyProperty.Register(
        nameof(ButtonEvent),
        typeof(Action),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonEventPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="Action"/> object contained within.
    /// </summary>
    static void OnButtonEventPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is Action act)
        {
            Debug.WriteLine($"[OnButtonEventPropertyChanged] => {act}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The button's <see cref="ICommand"/>.
    /// </summary>
    public ICommand? ButtonCommand
    {
        get => (ICommand?)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonCommand
    /// </summary>
    public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
        nameof(ButtonCommand),
        typeof(ICommand),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonCommandPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="ICommand"/> object contained within.
    /// </summary>
    static void OnButtonCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is ICommand cmd)
        {
            Debug.WriteLine($"[OnButtonCommandPropertyChanged] => {cmd}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The button's command parameter.
    /// </summary>
    public object? ButtonParameter
    {
        get => (object?)GetValue(ButtonParameterProperty);
        set => SetValue(ButtonParameterProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonCommand
    /// </summary>
    public static readonly DependencyProperty ButtonParameterProperty = DependencyProperty.Register(
        nameof(ButtonParameter),
        typeof(object),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonParameterPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="object"/> object contained within.
    /// </summary>
    static void OnButtonParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is object obj)
        {
            Debug.WriteLine($"[OnButtonParameterPropertyChanged] => {obj}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The text content to display.
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonText
    /// </summary>
    public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
        nameof(ButtonText),
        typeof(string),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonTextPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="string"/> object contained within.
    /// </summary>
    static void OnButtonTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is string str)
        {
            Debug.WriteLine($"[OnButtonTextPropertyChanged] => {str}");
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// If true, the progress bar is shown.
    /// </summary>
    public bool ButtonBusy
    {
        get => (bool)GetValue(ButtonBusyProperty);
        set => SetValue(ButtonBusyProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBusy
    /// </summary>
    public static readonly DependencyProperty ButtonBusyProperty = DependencyProperty.Register(
        nameof(ButtonBusy),
        typeof(bool),
        typeof(ProgressButton),
        new PropertyMetadata(null, OnButtonBusyPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="bool"/> object contained within.
    /// </summary>
    static void OnButtonBusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is bool bl)
        {
            Debug.WriteLine($"[OnButtonBusyPropertyChanged] => {bl}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnBusyChanged((bool)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }
    void OnBusyChanged(bool newValue)
    {
        if (newValue)
            ThisProgress.Visibility = Visibility.Visible;
        else
            ThisProgress.Visibility = Visibility.Collapsed;
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The progress bar button's value.
    /// If the value property is used then the min/max will be defaulted to 0/100 respectively.
    /// </summary>
    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }
    /// <summary>
    /// Backing property for ProgressValue
    /// </summary>
    public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(
        nameof(ProgressValue),
        typeof(double),
        typeof(ProgressButton),
        new PropertyMetadata(0d, OnProgressValuePropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="double"/> object contained within.
    /// </summary>
    static void OnProgressValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is double dbl)
        {
            Debug.WriteLine($"[OnProgressValuePropertyChanged] => {dbl}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnValueChanged((double)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }
    }
    /// <summary>
    /// Trying to add some automated smarts to the control; if the value property is set 
    /// then the <see cref="ProgressBar"/> will assume it is not in an indeterminate mode.
    /// </summary>
    void OnValueChanged(double newValue)
    {
        if (ThisProgress.IsIndeterminate)
        {
            ThisProgress.IsIndeterminate = false;
            ThisProgress.Minimum = 0d;
            ThisProgress.Maximum = 100d;
        }
        ThisProgress.Value = newValue;
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The color of the progress bar.
    /// </summary>
    public SolidColorBrush ProgressBrush
    {
        get => (SolidColorBrush)GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ProgressBrush
    /// </summary>
    public static readonly DependencyProperty ProgressBrushProperty = DependencyProperty.Register(
        nameof(ProgressBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.SpringGreen), OnProgressBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnProgressBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            Debug.WriteLine($"[OnProgressBrushPropertyChanged] => {scb}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnProgressBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnProgressBrushChanged(SolidColorBrush newValue)
    {
        ThisProgress.Foreground = newValue;
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The color of the button's background.
    /// </summary>
    public SolidColorBrush ButtonBackgroundBrush
    {
        get => (SolidColorBrush)GetValue(ButtonBackgroundBrushProperty);
        set => SetValue(ButtonBackgroundBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBackgroundBrush
    /// </summary>
    public static readonly DependencyProperty ButtonBackgroundBrushProperty = DependencyProperty.Register(
        nameof(ButtonBackgroundBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.Transparent), OnButtonBackgroundBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnButtonBackgroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            Debug.WriteLine($"[OnButtonBackgroundBrushPropertyChanged] => {scb}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonBackgroundBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonBackgroundBrushChanged(SolidColorBrush newValue)
    {
        ThisButton.Background = newValue;
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The color of the button's foreground.
    /// </summary>
    public SolidColorBrush ButtonForegroundBrush
    {
        get => (SolidColorBrush)GetValue(ButtonForegroundBrushProperty);
        set => SetValue(ButtonForegroundBrushProperty, value);
    }
    /// <summary>
    /// Backing property for ButtonBackgroundBrush
    /// </summary>
    public static readonly DependencyProperty ButtonForegroundBrushProperty = DependencyProperty.Register(
        nameof(ButtonForegroundBrush),
        typeof(SolidColorBrush),
        typeof(ProgressButton),
        new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.White), OnButtonForegroundBrushPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="SolidColorBrush"/> object contained within.
    /// </summary>
    static void OnButtonForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is SolidColorBrush scb)
        {
            Debug.WriteLine($"[OnButtonForegroundBrushPropertyChanged] => {scb}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonForegroundBrushChanged((SolidColorBrush)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WARNING] Wrong type => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonForegroundBrushChanged(SolidColorBrush newValue)
    {
        ThisButton.Foreground = newValue;
    }

    // ========================================================================================================================
    // ========================================================================================================================
    /// <summary>
    /// The CornerRadius of the control.
    /// </summary>
    public CornerRadius ButtonCornerRadius
    {
        get => (CornerRadius)GetValue(ButtonCornerRadiusProperty);
        set => SetValue(ButtonCornerRadiusProperty, value);
    }
    /// <summary>
    /// CornerRadius property for ProgressBrush
    /// </summary>
    public static readonly DependencyProperty ButtonCornerRadiusProperty = DependencyProperty.Register(
        nameof(ButtonCornerRadius),
        typeof(CornerRadius),
        typeof(ProgressButton),
        new PropertyMetadata(new CornerRadius(4), OnButtonCornerRadiusPropertyChanged));
    /// <summary>
    /// Upon trigger, the <see cref="DependencyObject"/> will be the control itself (<see cref="ProgressButton"/>)
    /// and the <see cref="DependencyPropertyChangedEventArgs"/> will be the <see cref="CornerRadius"/> object contained within.
    /// </summary>
    static void OnButtonCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is CornerRadius cr)
        {
            Debug.WriteLine($"[OnButtonCornerRadiusPropertyChanged] => {cr}");

            // A "cheat" so we can use non-static local control variables.
            ((ProgressButton)d).OnButtonCornerRadiusChanged((CornerRadius)e.NewValue);
        }
        else if (e.NewValue != null)
        {
            Debug.WriteLine($"[WRONG_TYPE] => {e.NewValue?.GetType()}");
            Debugger.Break();
        }

    }
    void OnButtonCornerRadiusChanged(CornerRadius newValue)
    {
        // Make sure they match each other.
        ThisProgress.CornerRadius = newValue;
        ThisButton.CornerRadius = newValue;
    }
    #endregion

    #region [Control Events]
    void ProgressButton_Loaded(object sender, RoutedEventArgs e)
    {
        if (ThisButton.ActualHeight != double.NaN && ThisButton.ActualHeight > 0)
        {
            ThisProgress.MinHeight = ThisButton.ActualHeight;
            // We're setting this in the DP now.
            //ThisProgress.CornerRadius = ThisButton.CornerRadius;
        }
        else
        {
            ThisProgress.MinHeight = ThisButton.MinHeight = 30;
            ThisProgress.CornerRadius = ThisButton.CornerRadius = new CornerRadius(4);
        }
        // Get rid of the single line that appears when using the non-indeterminate mode.
        ThisProgress.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    void ProgressButton_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ThisButton.ActualHeight != double.NaN && ThisButton.ActualHeight > 0)
        {
            ThisProgress.MinHeight = ThisButton.ActualHeight;
            // We're setting this in the DP now.
            //ThisProgress.CornerRadius = ThisButton.CornerRadius;
        }
        else
        {
            ThisProgress.MinHeight = ThisButton.MinHeight = 30;
            ThisProgress.CornerRadius = ThisButton.CornerRadius = new CornerRadius(4);
        }
    }

    /// <summary>
    /// Handles the <see cref="ICommand"/> or <see cref="Action"/>
    /// that is bound to the <see cref="ProgressButton"/>.
    /// </summary>
    void ThisButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"** I've been clicked **");
        try
        {
            // Has the user bound an Action to the click event?
            if (ButtonEvent != null)
                ButtonEvent.Invoke();

            // Has the user bound an ICommand to the click event?
            if (ButtonParameter != null)
                ButtonCommand?.Execute(ButtonParameter);
            else
                ButtonCommand?.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CLICK_ERROR] => {ex.Message}");
            //Debugger.Break();
        }
    }
    #endregion
}
