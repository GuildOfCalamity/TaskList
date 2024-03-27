using System.Numerics;

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App.Controls;

/// <summary>
/// https://learn.microsoft.com/en-us/windows/uwp/composition/xaml-lighting
/// The Microsoft example above seems outdated due to the Window.Current 
/// reference. I've added improvements to prevent null references concerning 
/// the <see cref="UIElement"/> during the OnConnected event.
/// </summary>
public sealed class ColoredSpotLight : XamlLight
{
    SpotLight? spotLight = null;
    
    #region [Dependency Properties]
    // Register an attached property that lets you set a UIElement
    // or Brush as a target for this light type in markup.
    public static readonly DependencyProperty IsTargetProperty = DependencyProperty.RegisterAttached(
        "IsTarget",
        typeof(bool),
        typeof(ColoredSpotLight),
        new PropertyMetadata(null, OnIsTargetChanged)
    );
    /// <summary>
    /// Handle attached property changed to automatically target and untarget UIElements and Brushes.
    /// </summary>
    static void OnIsTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var isAdding = (bool)e.NewValue;

        if (isAdding)
        {
            if (obj is UIElement)
                XamlLight.AddTargetElement(GetIdStatic(), obj as UIElement);
            else if (obj is Brush)
                XamlLight.AddTargetBrush(GetIdStatic(), obj as Brush);
        }
        else
        {
            if (obj is UIElement)
                XamlLight.RemoveTargetElement(GetIdStatic(), obj as UIElement);
            else if (obj is Brush)
                XamlLight.RemoveTargetBrush(GetIdStatic(), obj as Brush);
        }
    }

    public static readonly DependencyProperty InnerColorProperty = DependencyProperty.RegisterAttached(
        nameof(InnerColor),
        typeof(Windows.UI.Color),
        typeof(ColoredSpotLight),
        new PropertyMetadata(Microsoft.UI.Colors.MediumOrchid, (d, e) => (d as ColoredSpotLight)?.UpdateInnerColor(e))
    );
    public Windows.UI.Color InnerColor
    {
        get { return (Windows.UI.Color)GetValue(InnerColorProperty); }
        set { SetValue(InnerColorProperty, value); }
    }
    /// <summary>
    /// These fire before <see cref="Microsoft.UI.Xaml.Media.XamlLight.OnConnected(UIElement)"/>
    /// so we'll need to check if the <see cref="spotLight"/> in null before apply any changes.
    /// </summary>
    void UpdateInnerColor(DependencyPropertyChangedEventArgs args)
    {
        Debug.WriteLine($"[INFO] UpdateInnerColor => {args.NewValue}");
        if (spotLight != null && args != null)
            spotLight.InnerConeColor = (Windows.UI.Color)args.NewValue;
    }

    public static readonly DependencyProperty OuterColorProperty = DependencyProperty.RegisterAttached(
        nameof(OuterColor),
        typeof(Windows.UI.Color),
        typeof(ColoredSpotLight),
        new PropertyMetadata(Microsoft.UI.Colors.DodgerBlue, (d, e) => (d as ColoredSpotLight)?.UpdateOuterColor(e))
    );
    public Windows.UI.Color OuterColor
    {
        get { return (Windows.UI.Color)GetValue(OuterColorProperty); }
        set { SetValue(OuterColorProperty, value); }
    }
    /// <summary>
    /// These fire before <see cref="Microsoft.UI.Xaml.Media.XamlLight.OnConnected(UIElement)"/>
    /// so we'll need to check if the <see cref="spotLight"/> in null before apply any changes.
    /// </summary>
    void UpdateOuterColor(DependencyPropertyChangedEventArgs args)
    {
        Debug.WriteLine($"[INFO] UpdateOuterColor => {args.NewValue}");
        if (spotLight != null && args != null)
            spotLight.OuterConeColor = (Windows.UI.Color)args.NewValue;
    }

    public static readonly DependencyProperty InnerAngleProperty = DependencyProperty.RegisterAttached(
        nameof(InnerAngle),
        typeof(float),
        typeof(ColoredSpotLight),
        new PropertyMetadata((float)(Math.PI / 8), (d, e) => (d as ColoredSpotLight)?.UpdateInnerAngle(e))
    );
    public float InnerAngle
    {
        get { return (float)GetValue(InnerAngleProperty); }
        set { SetValue(InnerAngleProperty, value); }
    }
    /// <summary>
    /// These fire before <see cref="Microsoft.UI.Xaml.Media.XamlLight.OnConnected(UIElement)"/>
    /// so we'll need to check if the <see cref="spotLight"/> in null before apply any changes.
    /// </summary>
    void UpdateInnerAngle(DependencyPropertyChangedEventArgs args)
    {
        Debug.WriteLine($"[INFO] UpdateInnerAngle => {args.NewValue}");
        if (spotLight != null && args != null)
            spotLight.InnerConeAngle = (float)args.NewValue;
    }

    public static readonly DependencyProperty OuterAngleProperty = DependencyProperty.RegisterAttached(
        nameof(OuterAngle),
        typeof(float),
        typeof(ColoredSpotLight),
        new PropertyMetadata((float)(Math.PI / 2.5), (d, e) => (d as ColoredSpotLight)?.UpdateOuterAngle(e))
    );
    public float OuterAngle
    {
        get { return (float)GetValue(OuterAngleProperty); }
        set { SetValue(OuterAngleProperty, value); }
    }
    /// <summary>
    /// These fire before <see cref="Microsoft.UI.Xaml.Media.XamlLight.OnConnected(UIElement)"/>
    /// so we'll need to check if the <see cref="spotLight"/> in null before apply any changes.
    /// </summary>
    void UpdateOuterAngle(DependencyPropertyChangedEventArgs args)
    {
        Debug.WriteLine($"[INFO] UpdateOuterAngle => {args.NewValue}");
        if (spotLight != null && args != null)
            spotLight.OuterConeAngle = (float)args.NewValue;
    }
    #endregion

    public static void SetIsTarget(DependencyObject target, bool value) => target.SetValue(IsTargetProperty, value);
    public static Boolean GetIsTarget(DependencyObject target) => (bool)target.GetValue(IsTargetProperty);

    /// <summary>
    /// Similar to an OnLoad event.
    /// </summary>
    /// <param name="newElement"><see cref="UIElement"/></param>
    protected override void OnConnected(UIElement newElement)
    {
        if (CompositionLight == null)
        {
            if (Window.Current == null)
            {
                Debug.WriteLine($"[WARNING] Window.Current is null, compositor cannot be accessed.");
                if (newElement != null)
                {
                    Debug.WriteLine($"[INFO] Attempt to access compositor via UIElement.");
                    App.DebugLog($"{nameof(ColoredSpotLight)}: Attempt to access compositor via UIElement.");
                    var HostVisual = ElementCompositionPreview.GetElementVisual(newElement);
                    spotLight = HostVisual.Compositor.CreateSpotLight();
                }
                else
                {
                    // This happens in normal runtime without the debugger attached.
                    // When running via Visual Studio, the UIElement is not null?
                    if (MainWindow.Instance != null)
                    {
                        Debug.WriteLine($"[WARNING] UIElement is null, attempt to access compositor via MainWindow's compositor.");
                        App.DebugLog($"{nameof(ColoredSpotLight)}: UIElement is null, attempt to access compositor via MainWindow's compositor.");
                        spotLight = MainWindow.Instance.Compositor.CreateSpotLight();
                    }
                    else
                    {
                        Debug.WriteLine($"[WARNING] UIElement is null, unable to get the element visual's compositor.");
                        App.DebugLog($"{nameof(ColoredSpotLight)}: UIElement is null, unable to get the element visual's compositor.");
                        return;
                    }
                }
            }
            else
            {
                // OnConnected is called when the first target UIElement is shown on the screen.
                // This lets you delay creation of the composition object until it's actually needed.
                spotLight = Window.Current.Compositor.CreateSpotLight();
            }

            // Set the light coordinate space and add the target
            //Visual lightRoot = ElementCompositionPreview.GetElementVisual(newElement);
            //spotLight.CoordinateSpace = lightRoot;
            //spotLight.Targets.Add(lightRoot);

            spotLight.InnerConeColor = InnerColor;
            spotLight.OuterConeColor = OuterColor;
            spotLight.InnerConeAngle = 0.1f; // was 10
            spotLight.OuterConeAngle = 1.2f; // was 50
            
            //spotLight.Direction = new Vector3(0, -1, -1); // top-left
            spotLight.Direction = new Vector3(0, 0, -1); // left to right

            CompositionLight = spotLight;
        }
    }

    /// <summary>
    /// OnDisconnected is called when there are no more target UIElements on the screen.
    /// The CompositionLight should be disposed when no longer required, for SDK 15063 see remarks in the XamlLight documentation.
    /// https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.xamllight?view=winrt-22621#managing-resources
    /// </summary>
    /// <param name="oldElement"><see cref="UIElement"/></param>
    protected override void OnDisconnected(UIElement oldElement)
    {
        if (CompositionLight != null)
        {
            CompositionLight.Dispose();
            CompositionLight = null;

            // On Windows 10 Creators Update (SDK 15063), CompositionLight can't be accessed after Dispose is called,
            // so setting it to null after calling Dispose causes an error. To work around this issue, you can save
            // the CompositionLight to a temporary variable and call Dispose on that after you set CompositionLight to null.
            //var temp = CompositionLight;
            //CompositionLight = null;
            //temp.Dispose();
        }
    }

    /// <summary>
    /// This specifies the unique name of the light.
    /// In most cases you should use the type's FullName.
    /// </summary>
    static string? GetIdStatic() => typeof(ColoredSpotLight).FullName;
    protected override string? GetId() => GetIdStatic();
}
