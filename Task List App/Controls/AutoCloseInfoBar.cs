using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Task_List_App.Controls;

/// <summary>
/// An InfoBar that closes itself after an interval.
/// </summary>
public class AutoCloseInfoBar : InfoBar
{
    long? _iopToken;
    long? _mpToken;
    DispatcherTimer? _timer;

    /// <summary>
    /// Gets or sets the auto-close interval, in milliseconds.
    /// </summary>
    public int AutoCloseInterval { get; set; } = 6000;

    public AutoCloseInfoBar() : base()
    {
        this.Loaded += AutoCloseInfoBar_Loaded;
        this.Unloaded += AutoCloseInfoBar_Unloaded;
    }

    #region [Control Events]
    void AutoCloseInfoBar_Loaded(object sender, RoutedEventArgs e)
    {
        _iopToken = this.RegisterPropertyChangedCallback(IsOpenProperty, IsOpenChanged);
        _mpToken = this.RegisterPropertyChangedCallback(MessageProperty, MessageChanged);

        if (IsOpen)
            Open();
    }

    /// <summary>
    /// Clean-up procedure
    /// </summary>
    void AutoCloseInfoBar_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_iopToken != null)
            this.UnregisterPropertyChangedCallback(IsOpenProperty, (long)_iopToken);

        if (_mpToken != null)
            this.UnregisterPropertyChangedCallback(MessageProperty, (long)_mpToken);
    }

    /// <summary>
    /// Triggered once the <see cref="AutoCloseInterval"/> elapses.
    /// </summary>
    void Timer_Tick(object? sender, object e)
    {
        this.IsOpen = false;
    }

    /// <summary>
    /// Callback for our control's property change.
    /// </summary>
    void MessageChanged(DependencyObject o, DependencyProperty p)
    {
        var obj = o as AutoCloseInfoBar;
        if (obj == null)
            return;

        if (p != MessageProperty)
            return;

        if (obj.IsOpen)
        {
            // If the message has changed we should reset the timer.
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromMilliseconds(AutoCloseInterval);
                _timer.Start();
            }
        }
        else
        {
            Debug.WriteLine($"'{obj.GetType()}' is not open, skipping timer reset.");
        }
    }

    /// <summary>
    /// Callback for our control's property change.
    /// </summary>
    void IsOpenChanged(DependencyObject o, DependencyProperty p)
    {
        var obj = o as AutoCloseInfoBar;
        if (obj == null)
            return;

        if (p != IsOpenProperty)
            return;

        if (obj.IsOpen)
            obj.Open();
        else
            obj.Close();
    }
    #endregion

    #region [Control Methods]
    void Open()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += Timer_Tick;
        _timer.Interval = TimeSpan.FromMilliseconds(AutoCloseInterval);
        _timer.Start();
    }

    void Close()
    {
        if (_timer == null)
            return;

        _timer.Stop();
        _timer.Tick -= Timer_Tick;
    }
    #endregion
}
