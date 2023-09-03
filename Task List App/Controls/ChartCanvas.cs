using System;
using System.Diagnostics;
using Microcharts;
using Microsoft.UI.Xaml;
using SkiaSharp.Views.Windows;

namespace Task_List_App.Controls;

/// <summary>
/// A SkiaSharp WinUI Canvas that draws a Microcharts Chart.
/// The ChartCanvas is a small custom control that we built on top of
/// the SKXamlCanvas from SkiaSharp.Views.WinUI. It’s a Canvas that takes 
/// a Microchart chart as (Dependency) Property. It renders on load and 
/// rerenders when the Canvas resizes and when the Chart changes.
/// https://github.com/XamlBrewer/XamlBrewer.WinUI3.QuestPDF.Sample/blob/master/XamlBrewer.WinUI3.QuestPdf.Sample/Controls/ChartCanvas.cs
/// </summary>
/// <remarks>
/// This is a wrapper for Microcharts until a WinUI3 version is released.
/// https://xamlbrewer.wordpress.com/category/microcharts/
/// </remarks>
public class ChartCanvas : SKXamlCanvas
{
    public static readonly DependencyProperty ChartProperty = DependencyProperty.Register(
        nameof(Chart), 
        typeof(Chart), 
        typeof(ChartCanvas), 
        new PropertyMetadata(null));

    public Chart Chart
    {
        get 
        { 
            return (Chart)GetValue(ChartProperty); 
        }
        set
        {
            if (Chart != null)
                Chart.PropertyChanged -= (o, e) => { Invalidate(); };

            SetValue(ChartProperty, value);
            Invalidate();

            if (Chart != null)
                Chart.PropertyChanged += (o, e) => { Invalidate(); };
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        try
        {
            e.Surface.Canvas.Clear();
            Chart?.DrawContent(e.Surface.Canvas, e.Info.Width, e.Info.Height);
            base.OnPaintSurface(e);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnPaintSurface: {ex.Message}");
        }
    }
}
