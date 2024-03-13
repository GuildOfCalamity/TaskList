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

public sealed partial class SeparatorLine : UserControl
{
    static Windows.UI.Color _color1 = Windows.UI.Color.FromArgb(150, 80, 80, 90);
    static Windows.UI.Color _color2 = Windows.UI.Color.FromArgb(150, 10, 10, 20);

    #region [Properties]
    public static readonly DependencyProperty Line1BrushProperty = DependencyProperty.Register(
        nameof(Line1Brush),
        typeof(Brush),
        typeof(SeparatorLine),
     new PropertyMetadata(new SolidColorBrush(_color1)));

    public Brush Line1Brush
    {
        get { return (Brush)GetValue(Line1BrushProperty); }
        set { SetValue(Line1BrushProperty, value); }
    }

    public static readonly DependencyProperty Line2BrushProperty = DependencyProperty.Register(
        nameof(Line2Brush),
        typeof(Brush),
        typeof(SeparatorLine),
     new PropertyMetadata(new SolidColorBrush(_color2)));

    public Brush Line2Brush
    {
        get { return (Brush)GetValue(Line2BrushProperty); }
        set { SetValue(Line2BrushProperty, value); }
    }
    #endregion

    public SeparatorLine()
    {
        DataContext = this;
        this.InitializeComponent();
        this.Loaded += OnLoaded;

        // This is just for effect, it serves no practical purpose.
        this.PointerEntered += OnPointerEntered;
        this.PointerExited += OnPointerExited;
    }

    #region [Events]
    void OnLoaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] Loaded {sender.GetType().Name} of base type {sender.GetType().BaseType?.Name}");
    }

    void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var clr1 = (Line1Brush as SolidColorBrush)?.Color ?? _color1;
        var clr2 = (Line2Brush as SolidColorBrush)?.Color ?? _color2;
        Line1Brush = new SolidColorBrush(clr2);
        Line2Brush = new SolidColorBrush(clr1);
    }

    void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        var clr1 = (Line1Brush as SolidColorBrush)?.Color ?? _color2;
        var clr2 = (Line2Brush as SolidColorBrush)?.Color ?? _color1;
        Line1Brush = new SolidColorBrush(clr2);
        Line2Brush = new SolidColorBrush(clr1);
    }
    #endregion
}
