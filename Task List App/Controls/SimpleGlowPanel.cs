using System.Numerics;

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

using Windows.ApplicationModel;

using Task_List_App.Helpers;

namespace Task_List_App.Controls;

/// <summary>
/// NOTICE: You must setup a styler for this <see cref="ContentControl"/> in the resource dictionary which 
/// will nest the <see cref="ContentPresenter"/>, otherwise you will get a null exception for the ShadowHost.
/// This is a "simple" version. You could enhance this with <see cref="DependencyProperty"/>s for offsets, 
/// margins/padding, multiple colors, et al.
/// </summary>
public class SimpleGlowPanel : ContentControl
{
    private Compositor _Compositor;
    private DropShadow _Shadow;
    private SpriteVisual _ShadowVisual;
    private Visual HostVisual;
    private Rectangle ShadowHost;

    #region [Non-Native Dependency Properties]
    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
            nameof(BlurRadius),
            typeof(double),
            typeof(SimpleGlowPanel), new PropertyMetadata(35.0));

    public double BlurRadius
    {
        get { return (double)GetValue(BlurRadiusProperty); }
        set { SetValue(BlurRadiusProperty, value); }
    }
    #endregion

    public SimpleGlowPanel()
    {
        this.DefaultStyleKey = typeof(SimpleGlowPanel);
        RegisterPropertyChangedCallback(ContentProperty, ContentPropertyChanged);
        RegisterPropertyChangedCallback(BackgroundProperty, BackgroundPropertyChanged);
        RegisterPropertyChangedCallback(BlurRadiusProperty, OnBlurRadiusPropertyChanged);
        this.SizeChanged += SimpleShadowPanelOnSizeChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ShadowHost = GetTemplateChild("ShadowHost") as Rectangle;
        SetupComposition();
        UpdateShadow();
    }

    /// <summary>
    /// Configure defaults during template application.
    /// Upon 1st invoke the ShadowHost will be null until the base template is rendered.
    /// </summary>
    void SetupComposition()
    {
        if (DesignMode.DesignModeEnabled)
            return;

        if (ShadowHost is null)
        {
            Debug.WriteLine($"WARNING: '{nameof(ShadowHost)}' is null, cannot continue with composition.");
            Debug.WriteLine($"Make sure you have a style setup for this control in your resource dictionary.");
            return;
        }

        HostVisual = ElementCompositionPreview.GetElementVisual(ShadowHost);
        _Compositor = HostVisual.Compositor;

        _ShadowVisual = _Compositor.CreateSpriteVisual();
        _ShadowVisual.BindSize(HostVisual);

        _Shadow = _Compositor.CreateDropShadow();
        _Shadow.Offset = Vector3.Zero;
        _Shadow.BlurRadius = 35f;

        _ShadowVisual.Shadow = _Shadow;

        ElementCompositionPreview.SetElementChildVisual(ShadowHost, _ShadowVisual);
    }

    Windows.UI.Color GetBackgroundColor()
    {
        if (Background is SolidColorBrush brush)
            return brush.Color;

        return Colors.Transparent;
    }
    
    void UpdateShadow()
    {
        if (_Shadow != null)
        {
            _Shadow.Color = GetBackgroundColor();
            _Shadow.BlurRadius = (float)BlurRadius;
            if (Content is Rectangle rect)
                _Shadow.Mask = rect.GetAlphaMask();
        }
    }

    #region [Dependency Property Events]
    void SimpleShadowPanelOnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateShadow();
    void BackgroundPropertyChanged(DependencyObject sender, DependencyProperty dp) => UpdateShadow();
    void ContentPropertyChanged(DependencyObject sender, DependencyProperty dp) => UpdateShadow();
    void OnBlurRadiusPropertyChanged(DependencyObject sender, DependencyProperty dp) => UpdateShadow();
    #endregion
}
