using System;

namespace Task_List_App.Helpers;

/// <summary>
/// A memory effecient version of the System.Diagnostics.Stopwatch class.
/// Because this timer's function is passive, there's no need/way for a
/// stop method. A reset method would be equivalent to calling StartNew().
/// </summary>
internal struct ValueStopwatch
{
    // Set the ratio of timespan ticks to stopwatch ticks.
    static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)System.Diagnostics.Stopwatch.Frequency;
    long _startTimestamp;
    public bool IsActive => _startTimestamp != 0;
    private ValueStopwatch(long startTimestamp) => _startTimestamp = startTimestamp;
    public static ValueStopwatch StartNew() => new ValueStopwatch(System.Diagnostics.Stopwatch.GetTimestamp());
    public TimeSpan GetElapsedTime()
    {
        // _startTimestamp cannot be zero for an initialized ValueStopwatch.
        if (!IsActive)
            throw new InvalidOperationException($"{nameof(ValueStopwatch)} is uninitialized. Initialize the {nameof(ValueStopwatch)} before using.");

        long end = System.Diagnostics.Stopwatch.GetTimestamp();
        long timestampDelta = end - _startTimestamp;
        long ticks = (long)(TimestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }
}
