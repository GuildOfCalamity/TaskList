using System;
using System.Numerics;

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

using Windows.UI;

namespace Task_List_App.Controls;

/// <summary>
/// The <see cref="CompositionShadow"/> control allows the creation of a DropShadow for any 
/// XAML FrameworkElement in markup making it much easier to add shadows to XAML without having 
/// to directly drop down to Windows.UI.Composition APIs. This also fixes the issue for the hated
/// shadowed border effect around controls and images; typically the behavior would outline the 
/// rounded container and would not adhere to the nearest edges of the control or image.
/// Supplying zero for the offset values has the added benefit of creating a glow effect.
/// I have raised the default blur radius from 8 to 16.
/// The default shadow color is <see cref="Microsoft.UI.Colors.Black"/>.
/// </summary>
/// <remarks>
/// The <see cref="Windows.UI.Composition.DropShadow"/> only supports <see cref="Microsoft.UI.Colors"/>
/// but I've added a converter method so that passing a <see cref="SolidColorBrush"/> does not crash
/// the control.
/// The CommunityToolkit DropShadowPanel is kind of weak and does not give you the desired effect (like it did in WPF).
/// It is also scheduled to be removed soon (maybe because of the afformentioned issues?). For more info on that control:
/// https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.ui.controls.dropshadowpanel?view=win-comm-toolkit-dotnet-7.0
/// </remarks>
[ContentProperty(Name = nameof(CastingElement))]
public sealed partial class CompositionShadow : UserControl
{
    FrameworkElement? _castingElement;
    readonly DropShadow _dropShadow;
    readonly SpriteVisual _shadowVisual;

    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
            nameof(BlurRadius),
            typeof(double),
            typeof(CompositionShadow),
            new PropertyMetadata(16.0, OnBlurRadiusChanged));

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(CompositionShadow),
            new PropertyMetadata(Microsoft.UI.Colors.Black, OnColorChanged));

    /// <summary>
    /// I've added brush support so that theme resources may be used in conjunction with shadowed controls.
    /// e.g. Your shadow would be black in a light theme and then switch to white in a dark theme.
    /// </summary>
    public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(CompositionShadow),
            new PropertyMetadata(new SolidColorBrush(Microsoft.UI.Colors.Black), OnBrushChanged));

    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(
            nameof(OffsetX),
            typeof(double),
            typeof(CompositionShadow),
            new PropertyMetadata(0.0, OnOffsetXChanged));

    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(CompositionShadow),
            new PropertyMetadata(0.0, OnOffsetYChanged));

    public static readonly DependencyProperty OffsetZProperty = DependencyProperty.Register(
            nameof(OffsetZ),
            typeof(double),
            typeof(CompositionShadow),
            new PropertyMetadata(0.0, OnOffsetZChanged));

    public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(
            nameof(ShadowOpacity),
            typeof(double),
            typeof(CompositionShadow),
            new PropertyMetadata(1.0, OnShadowOpacityChanged));

    public CompositionShadow()
    {
        InitializeComponent();
        DefaultStyleKey = typeof(CompositionShadow);
        SizeChanged += CompositionShadow_SizeChanged;
        Loaded += (object sender, RoutedEventArgs e) =>
        {
            ConfigureShadowVisualForCastingElement();
        };

        Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        _shadowVisual = compositor.CreateSpriteVisual();
        _dropShadow = compositor.CreateDropShadow();
        _shadowVisual.Shadow = _dropShadow;

        // SetElementChildVisual on the control itself ("this") would result in the shadow
        // rendering on top of the content. To avoid this, CompositionShadow contains a Border
        // (to host the shadow) and a ContentPresenter (to host the actual content, "CastingElement").
        ElementCompositionPreview.SetElementChildVisual(ShadowElement, _shadowVisual);
    }

    /// <summary>
    /// The blur radius of the drop shadow.
    /// </summary>
    public double BlurRadius
    {
        get { return (double)GetValue(BlurRadiusProperty); }
        set { SetValue(BlurRadiusProperty, value); }
    }

    /// <summary>
    /// The FrameworkElement that this <see cref="CompositionShadow"/> uses to create 
    /// the mask for the underlying <see cref="Windows.UI.Composition.DropShadow"/>.
    /// </summary>
    public FrameworkElement CastingElement
    {
        get { return _castingElement; }
        set
        {
            if (_castingElement != null)
                _castingElement.SizeChanged -= CompositionShadow_SizeChanged;

            _castingElement = value;
            _castingElement.SizeChanged += CompositionShadow_SizeChanged;

            ConfigureShadowVisualForCastingElement();
        }
    }

    /// <summary>
    /// The color of the drop shadow.
    /// </summary>
    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    /// <summary>
    /// The color of the drop shadow.
    /// </summary>
    public SolidColorBrush Brush
    {
        get { return (SolidColorBrush)GetValue(BrushProperty); }
        set { SetValue(BrushProperty, value); }
    }

    /// <summary>
    /// Exposes the underlying composition object to allow custom Windows.UI.Composition animations.
    /// </summary>
    public DropShadow DropShadow
    {
        get { return _dropShadow; }
    }

    /// <summary>
    /// Exposes the underlying SpriteVisual to allow custom animations and transforms.
    /// </summary>
    public SpriteVisual Visual
    {
        get { return _shadowVisual; }
    }

    /// <summary>
    /// The mask of the underlying <see cref="Windows.UI.Composition.DropShadow"/>.
    /// Allows for a custom <see cref="Windows.UI.Composition.CompositionBrush"/> to be set.
    /// </summary>
    public CompositionBrush Mask
    {
        get { return _dropShadow.Mask; }
        set { _dropShadow.Mask = value; }
    }

    /// <summary>
    /// The x offset of the drop shadow.
    /// </summary>
    public double OffsetX
    {
        get { return (double)GetValue(OffsetXProperty); }
        set { SetValue(OffsetXProperty, value); }
    }

    /// <summary>
    /// The y offset of the drop shadow.
    /// </summary>
    public double OffsetY
    {
        get { return (double)GetValue(OffsetYProperty); }
        set { SetValue(OffsetYProperty, value); }
    }

    /// <summary>
    /// The z offset of the drop shadow.
    /// </summary>
    public double OffsetZ
    {
        get { return (double)GetValue(OffsetZProperty); }
        set { SetValue(OffsetZProperty, value); }
    }

    /// <summary>
    /// The opacity of the drop shadow.
    /// </summary>
    public double ShadowOpacity
    {
        get { return (double)GetValue(ShadowOpacityProperty); }
        set { SetValue(ShadowOpacityProperty, value); }
    }

    static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnBlurRadiusChanged((double)e.NewValue);
    }

    static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnColorChanged((Color)e.NewValue);
    }

    static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnBrushChanged((SolidColorBrush)e.NewValue);
    }

    static void OnOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnOffsetXChanged((double)e.NewValue);
    }

    static void OnOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnOffsetYChanged((double)e.NewValue);
    }

    static void OnOffsetZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnOffsetZChanged((double)e.NewValue);
    }

    static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CompositionShadow)d).OnShadowOpacityChanged((double)e.NewValue);
    }

    void CompositionShadow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateShadowSize();
    }

    void ConfigureShadowVisualForCastingElement()
    {
        UpdateShadowMask();
        UpdateShadowSize();
    }

    void OnBlurRadiusChanged(double newValue)
    {
        _dropShadow.BlurRadius = (float)newValue;
    }

    void OnColorChanged(Color newValue)
    {
        _dropShadow.Color = newValue;
    }

    void OnBrushChanged(SolidColorBrush newValue)
    {
        if (newValue is not null)
            _dropShadow.Color = newValue.Color;
    }

    void OnOffsetXChanged(double newValue)
    {
        UpdateShadowOffset((float)newValue, _dropShadow.Offset.Y, _dropShadow.Offset.Z);
    }

    private void OnOffsetYChanged(double newValue)
    {
        UpdateShadowOffset(_dropShadow.Offset.X, (float)newValue, _dropShadow.Offset.Z);
    }

    void OnOffsetZChanged(double newValue)
    {
        UpdateShadowOffset(_dropShadow.Offset.X, _dropShadow.Offset.Y, (float)newValue);
    }

    void OnShadowOpacityChanged(double newValue)
    {
        _dropShadow.Opacity = (float)newValue;
    }

    void UpdateShadowMask()
    {
        if (_castingElement != null)
        {
            CompositionBrush? mask = null;
            if (_castingElement is Image)
            {
                mask = ((Image)_castingElement).GetAlphaMask();
            }
            else if (_castingElement is Shape)
            {
                mask = ((Shape)_castingElement).GetAlphaMask();
            }
            else if (_castingElement is TextBlock)
            {
                mask = ((TextBlock)_castingElement).GetAlphaMask();
            }

			_dropShadow.Mask = mask;
        }
        else
        {
            // Disable the mask
            _dropShadow.Mask = null;
        }
    }

    void UpdateShadowOffset(float x, float y, float z)
    {
        _dropShadow.Offset = new Vector3(x, y, z);
    }

    void UpdateShadowSize()
    {
        Vector2 newSize = new Vector2((float)ActualWidth, (float)ActualHeight);

        if (_castingElement != null)
            newSize = new Vector2((float)_castingElement.ActualWidth, (float)_castingElement.ActualHeight);

        _shadowVisual.Size = newSize;
    }
}
