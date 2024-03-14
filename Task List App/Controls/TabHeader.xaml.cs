using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Task_List_App.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Task_List_App.Controls;

public sealed partial class TabHeader : UserControl
{
    bool _showOutline = false;
    SolidColorBrush _hoverBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 90));

    #region [Properties]
    public static readonly DependencyProperty HoverEffectProperty = DependencyProperty.Register(
        nameof(HoverEffect),
        typeof(string),
        typeof(TabHeader),
        new PropertyMetadata("False"));
    public string HoverEffect
    {
        get { return GetValue(HoverEffectProperty) as string; }
        set { SetValue(HoverEffectProperty, value); }
    }

    public static readonly DependencyProperty HoverStrengthProperty = DependencyProperty.Register(
    nameof(HoverStrengthProperty),
    typeof(double),
    typeof(TabHeader),
    new PropertyMetadata(1d));

    public double HoverStrength
    {
        get { return (double)GetValue(HoverStrengthProperty); }
        set { SetValue(HoverStrengthProperty, value); }
    }

    public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register(
        nameof(SelectedImage),
        typeof(string),
        typeof(TabHeader),
        null);
    public string SelectedImage
    {
        get { return GetValue(SelectedImageProperty) as string; }
        set { SetValue(SelectedImageProperty, value); }
    }

    public static readonly DependencyProperty UnselectedImageProperty = DependencyProperty.Register(
        nameof(UnselectedImage),
        typeof(string),
        typeof(TabHeader),
        null);

    public string UnselectedImage
    {
        get { return GetValue(UnselectedImageProperty) as string; }
        set { SetValue(UnselectedImageProperty, value); }
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(TabHeader),
        null);

    public string Label
    {
        get { return GetValue(LabelProperty) as string; }
        set { SetValue(LabelProperty, value); }
    }

    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
        nameof(ImageWidth),
        typeof(double),
        typeof(TabHeader),
        new PropertyMetadata(40d));

    public double ImageWidth
    {
        get { return (double)GetValue(ImageWidthProperty); }
        set { SetValue(ImageWidthProperty, value); }
    }

    public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
        nameof(ImageHeight),
        typeof(double),
        typeof(TabHeader),
        new PropertyMetadata(40d));

    public double ImageHeight
    {
        get { return (double)GetValue(ImageHeightProperty); }
        set { SetValue(ImageHeightProperty, value); }
    }
    #endregion

    public TabHeader()
    {
        this.InitializeComponent();
        this.Loaded += TabHeaderOnLoaded;
        this.PointerEntered += TabHeaderOnPointerEntered;
        this.PointerExited += TabHeaderOnPointerExited;
        DataContext = this;
    }

    #region [Events]
    /// <summary>
    /// Demonstrate fetching brush resource using our <see cref="GeneralExtensions.GetResource{T}"/> method.
    /// </summary>
    void TabHeaderOnLoaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] Loaded {sender.GetType().Name} of base type {sender.GetType().BaseType?.Name}");
        _hoverBrush = GeneralExtensions.GetResource<SolidColorBrush>("SecondaryBrush") ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30,30,90));
    }

    /// <summary>
    /// Change the style if mouse enters the control bounds.
    /// There are many visual options here to experiment with.
    /// </summary>
    void TabHeaderOnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        if (!string.IsNullOrEmpty(HoverEffect) && HoverEffect.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            //stackPanel.BorderBrush = _hoverBrush;
            stackPanel.BorderThickness = new Thickness(HoverStrength);
        }
    }

    /// <summary>
    /// Change the style if mouse leaves the control bounds.
    /// There are many visual options here to experiment with.
    /// </summary>
    void TabHeaderOnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        this.ProtectedCursor = null;
        if (!string.IsNullOrEmpty(HoverEffect) && HoverEffect.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            //stackPanel.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            stackPanel.BorderThickness = new Thickness(0);
        }
    }
    #endregion

    /// <summary>
    /// Changes the image visible state during swap.
    /// </summary>
    /// <param name="isSelected"></param>
    public void SetSelectedItem(bool isSelected)
    {
        if (isSelected)
        {
            selectedImage.Visibility = Visibility.Visible;
            unselectedImage.Visibility = Visibility.Collapsed;
        }
        else
        {
            selectedImage.Visibility = Visibility.Collapsed;
            unselectedImage.Visibility = Visibility.Visible;
        }
    }
}
