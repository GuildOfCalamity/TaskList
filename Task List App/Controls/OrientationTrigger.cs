using Microsoft.UI.Xaml;

namespace Task_List_App.Controls;

/// <summary>
/// Scrren Orientation Enum
/// </summary>
public enum LocalOrientation
{
    Landscape,
    Portrait
}

/// <summary>
/// OrientationTrigger is a state trigger that responds to changes of the size of the window,
/// and is set to active when the current orientation matches the requested orientation (property).
/// </summary>
public class OrientationTrigger : StateTriggerBase
{
    // Private members
    private LocalOrientation _orientation;
    private double _actualWidth;
    private double _actualHeight;

    /// <summary>
    /// Property that determines the orientation that this trigger will be active in.
    /// </summary>
    public LocalOrientation Orientation { get { return _orientation; } set { _orientation = value; EvaluateCurrentOrientation(GetCurrentOrientation()); } }

    /// <summary>
    /// Property that determines whether the orientation matches the size given.
    /// </summary>
    public double ActualWidth { get { return _actualWidth; } set { _actualWidth = value; EvaluateCurrentOrientation(GetCurrentOrientation()); } }

    /// <summary>
    /// Property that determines whether the orientation matches the size given.
    /// </summary>
    public double ActualHeight { get { return _actualHeight; } set { _actualHeight = value; EvaluateCurrentOrientation(GetCurrentOrientation()); } }

    /// <summary>
    /// Constructor
    /// </summary>
    public OrientationTrigger()
    {
        // Get the current orientation
        LocalOrientation currentOrientation = LocalOrientation.Landscape; //GetCurrentOrientation();

        // See if the current orientation matches the requested orientation.
        EvaluateCurrentOrientation(currentOrientation);
    }

    public void SizeChanged(Windows.Foundation.Size newSize)
    {
        //// Get the current orientation
        ActualWidth = newSize.Width;
        ActualHeight = newSize.Height;
        LocalOrientation currentOrientation = GetCurrentOrientation();

        // See if the current orientation matches the requested orientation.
        EvaluateCurrentOrientation(currentOrientation);
    }

    /// <summary>
    /// Determines the current orientation of the window/screen.
    /// </summary>
    /// <returns>Orientation</returns>
    private LocalOrientation GetCurrentOrientation()
    {
        var width = ActualWidth;
        var height = ActualHeight;

        // If our width is greater than our height, we are in landscape. Otherwise we are in
        // portrait.
        return width > height ? LocalOrientation.Landscape : LocalOrientation.Portrait;
    }

    /// <summary>
    /// Evaluates whether the current orientation matches the given/desired orientation.
    /// </summary>
    /// <param name="currentOrientation">The current orientation to be evaluated.</param>
    private void EvaluateCurrentOrientation(LocalOrientation currentOrientation)
    {
        SetActive(currentOrientation == Orientation);
    }
}
