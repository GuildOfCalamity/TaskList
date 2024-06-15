using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Task_List_App.Helpers;

/// <summary>
/// Helpers for executing code using the <see cref="Microsoft.UI.Dispatching.DispatcherQueueTimer"/>.
/// </summary>
public static class DispatcherQueueTimerExtensions
{
    /// <summary>
    /// Our collection which holds timers for the dispatcher.
    /// </summary>
    static ConcurrentDictionary<Microsoft.UI.Dispatching.DispatcherQueueTimer, Action> _debounceInstances = new ConcurrentDictionary<Microsoft.UI.Dispatching.DispatcherQueueTimer, Action>();

    /// <summary>
    /// Used to debounce (rate-limit) an event.  The action will be postponed and executed after the interval has elapsed.
    /// At the end of the interval, the function will be called with the arguments that were passed most recently to the debounced function.
    /// Use this method to control the timer instead of calling Start/Interval/Stop manually.
    /// A scheduled debounce can still be stopped by calling the stop method on the timer instance.
    /// Each timer can only have one debounced function limited at a time.
    /// <example>
    /// <code>
    /// private DispatcherQueueTimer _timer = DispatcherQueue.CreateTimer();
    /// _timer.Debounce(async () => {
    ///    Debug.WriteLine($"Debounce => {DateTime.Now.ToString("hh:mm:ss.fff tt")}"); // Only executes this code after half a second has elapsed since last trigger.
    /// }, TimeSpan.FromSeconds(0.5));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="timer">Timer instance, only one debounced function can be used per timer.</param>
    /// <param name="action">Action to execute at the end of the interval.</param>
    /// <param name="interval">Interval to wait before executing the action.</param>
    /// <param name="immediate">Determines if the action execute on the leading edge instead of trailing edge.</param>
    public static void Debounce(this Microsoft.UI.Dispatching.DispatcherQueueTimer timer, Action action, TimeSpan interval, bool immediate = false)
    {
        // Check and stop any existing timer
        var timeout = timer.IsRunning;
        if (timeout)
            timer.Stop();

        // Reset timer parameters
        timer.Tick -= DebounceTimerTick;
        timer.Interval = interval;

        if (immediate)
        {   // If we're in immediate mode then we only execute if the timer wasn't running beforehand
            if (!timeout)
                action.Invoke();
        }
        else
        {   // If we're not in immediate mode, then we'll execute when the current timer expires.
            timer.Tick += DebounceTimerTick;

            // Store/Update function
            _debounceInstances.AddOrUpdate(timer, action, (k, v) => v);
        }

        // Start the timer to keep track of the last call here.
        timer.Start();
    }
    /// <summary>
    /// This event is only registered/run if we weren't in immediate mode above.
    /// </summary>
    static void DebounceTimerTick(object sender, object e)
    {
        if (sender is Microsoft.UI.Dispatching.DispatcherQueueTimer timer)
        {
            timer.Tick -= DebounceTimerTick;
            timer.Stop();

            // Extract the code an execute it.
            if (_debounceInstances.TryRemove(timer, out Action? action))
                action?.Invoke();
        }
    }
}
