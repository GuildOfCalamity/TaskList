using System;
using System.Numerics;

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

namespace Task_List_App.Helpers;

/// <summary>
/// WARNING: Use at your own risk, this may cause undesired behavior.
/// </summary>
public static class ShadowHelper
{
    private static Dictionary<WeakReference<UIElement>, (LayerVisual, long)> visuals = new Dictionary<WeakReference<UIElement>, (LayerVisual, long)>();

    public static bool GetIsEnabled(FrameworkElement obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(FrameworkElement obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ShadowHelper), new PropertyMetadata(false, OnIsEnabledPropertyChanged));

    private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue != (bool)e.OldValue)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.Unloaded += Element_Unloaded;

                    var parent = element.Parent as UIElement;
                    if (parent == null)
                    {
                        parent = VisualTreeHelper.GetParent(element) as UIElement;
                    }
                    if (parent == null && element != Window.Current.Content)
                    {
                        var weakListener = new WeakEventListener<FrameworkElement, object, RoutedEventArgs>(element);

                        weakListener.OnEventAction = (i, s, a) => Element_Loaded(s, a);
                        weakListener.OnDetachAction = c =>
                        {
                            element.Loaded -= weakListener.OnEvent;
                        };

                        element.Loaded += weakListener.OnEvent;
                    }

                    if (parent != null)
                    {
                        SetShadow(element, parent);
                    }
                }
                else
                {
                    element.Unloaded -= Element_Unloaded;
                    RemoveShadow(element);
                }
            }
        }
    }

    private static void Element_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveShadow(sender as UIElement);
    }

    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        var element = sender as FrameworkElement;
        element.Loaded -= Element_Loaded;

        var parent = element.Parent as UIElement;
        if (parent == null)
        {
            parent = VisualTreeHelper.GetParent(element) as UIElement;
        }

        if (parent != null)
        {
            SetShadow(element, parent);
        }
    }

    private static void SetShadow(UIElement element, UIElement parent)
    {
        if (element == null || parent == null) return;

        RemoveShadow(element);

        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.Opacity = 0;
        var compositor = visual.Compositor;

        var sizeBind = compositor.CreateExpressionAnimation("visual.Size");
        sizeBind.SetReferenceParameter("visual", visual);

        var offsetBind = compositor.CreateExpressionAnimation("visual.Offset");
        offsetBind.SetReferenceParameter("visual", visual);

        var rVisual = compositor.CreateRedirectVisual(visual);
        rVisual.StartAnimation("Size", sizeBind);

        var lVisual = compositor.CreateLayerVisual();
        lVisual.StartAnimation("Size", sizeBind);
        lVisual.StartAnimation("Offset", offsetBind);

        lVisual.Children.InsertAtTop(rVisual);

        var shadow = compositor.CreateDropShadow();
        shadow.BlurRadius = 24;
        shadow.Color = Windows.UI.Color.FromArgb(180, 0, 0, 0);
        shadow.SourcePolicy = Microsoft.UI.Composition.CompositionDropShadowSourcePolicy.InheritFromVisualContent;

        lVisual.Shadow = shadow;
        lVisual.Opacity = (float)element.Opacity;

        var parentContainerVisual = ElementCompositionPreview.GetElementChildVisual(parent) as ContainerVisual;

        if (parentContainerVisual == null)
        {
            parentContainerVisual = compositor.CreateContainerVisual();
            parentContainerVisual.RelativeSizeAdjustment = Vector2.One;
            ElementCompositionPreview.SetElementChildVisual(parent, parentContainerVisual);
        }

        parentContainerVisual.Children.InsertAtTop(lVisual);

        var token = element.RegisterPropertyChangedCallback(UIElement.OpacityProperty, OnOpacityPropertyChanged);

        visuals[new WeakReference<UIElement>(element)] = (lVisual, token);
    }

    private static void RemoveShadow(UIElement element)
    {
        if (element == null) return;

        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.Opacity = (float)element.Opacity;

        WeakReference<UIElement> key = null;

        foreach (var (weak, (v, token)) in visuals)
        {
            if (weak.TryGetTarget(out var ele) && ele == element)
            {
                key = weak;

                element.UnregisterPropertyChangedCallback(UIElement.OpacityProperty, token);

                var parentContainerVisual = v.Parent;

                if (parentContainerVisual != null)
                    parentContainerVisual.Children.Remove(v);

                break;
            }
        }

        if (key != null)
            visuals.Remove(key);
    }

    private static void OnOpacityPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (sender is FrameworkElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Opacity = 0;

            foreach (var (weak, (v, token)) in visuals)
            {
                if (weak.TryGetTarget(out var ele) && ele == element)
                    v.Opacity = (float)element.Opacity;
            }
        }
    }

}
