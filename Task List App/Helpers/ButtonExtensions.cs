using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace Task_List_App.Helpers;

/// <summary>
/// Portions of this code are from https://github.com/AndrewKeepCoding/AK.Toolkit/blob/main/WinUI3/AK.Toolkit.WinUI3.ButtonExtensions/ButtonExtensions.cs
/// I have improved upon this base by adding foreground attached properties to supplement the background attached properties.
/// </summary>
public static class ButtonExtensions
{
    public static readonly DependencyProperty PointerOverBackgroundLightnessFactorProperty =
        DependencyProperty.RegisterAttached(
            "PointerOverBackgroundLightnessFactor",
            typeof(double),
            typeof(ButtonExtensions),
            new PropertyMetadata(1.0, OnPointerOverBackgroundLightnessFactorPropertyChanged));

    public static readonly DependencyProperty PointerOverForegroundLightnessFactorProperty =
    DependencyProperty.RegisterAttached(
        "PointerOverForegroundLightnessFactor",
        typeof(double),
        typeof(ButtonExtensions),
        new PropertyMetadata(1.0, OnPointerOverForegroundLightnessFactorPropertyChanged));

    public static readonly DependencyProperty PressedBackgroundLightnessFactorProperty =
        DependencyProperty.RegisterAttached(
            "PressedBackgroundLightnessFactor",
            typeof(double),
            typeof(ButtonExtensions),
            new PropertyMetadata(1.0, OnPressedBackgroundLightnessFactorPropertyChanged));

    public static readonly DependencyProperty PressedForegroundLightnessFactorProperty =
    DependencyProperty.RegisterAttached(
        "PressedForegroundLightnessFactor",
        typeof(double),
        typeof(ButtonExtensions),
        new PropertyMetadata(1.0, OnPressedForegroundLightnessFactorPropertyChanged));

    public static double GetPointerOverBackgroundLightnessFactor(DependencyObject obj) => (double)obj.GetValue(PointerOverBackgroundLightnessFactorProperty);
    public static double GetPointerOverForegroundLightnessFactor(DependencyObject obj) => (double)obj.GetValue(PointerOverForegroundLightnessFactorProperty);
    public static double GetPressedBackgroundLightnessFactor(DependencyObject obj) => (double)obj.GetValue(PressedBackgroundLightnessFactorProperty);
    public static double GetPressedForegroundLightnessFactor(DependencyObject obj) => (double)obj.GetValue(PressedForegroundLightnessFactorProperty);
    public static void SetPointerOverBackgroundLightnessFactor(DependencyObject obj, double value) => obj.SetValue(PointerOverBackgroundLightnessFactorProperty, value);
    public static void SetPointerOverForegroundLightnessFactor(DependencyObject obj, double value) => obj.SetValue(PointerOverForegroundLightnessFactorProperty, value);
    public static void SetPressedBackgroundLightnessFactor(DependencyObject obj, double value) => obj.SetValue(PressedBackgroundLightnessFactorProperty, value);
    public static void SetPressedForegroundLightnessFactor(DependencyObject obj, double value) => obj.SetValue(PressedForegroundLightnessFactorProperty, value);

    static void OnPointerOverBackgroundLightnessFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button button || e.NewValue is not double backgroundLightnessFactor)
            return;

        _ = button.RegisterPropertyChangedCallback(Button.BackgroundProperty, (_, _) =>
        {
            UpdatePointerOverBackgroundLightness(button, backgroundLightnessFactor);
        });

        UpdatePointerOverBackgroundLightness(button, backgroundLightnessFactor);
    }

    static void OnPointerOverForegroundLightnessFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button button || e.NewValue is not double foregroundLightnessFactor)
            return;

        _ = button.RegisterPropertyChangedCallback(Button.ForegroundProperty, (_, _) =>
        {
            UpdatePointerOverForegroundLightness(button, foregroundLightnessFactor);
        });

        UpdatePointerOverForegroundLightness(button, foregroundLightnessFactor);
    }

    static void OnPressedBackgroundLightnessFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button button || e.NewValue is not double backgroundLightnessFactor)
            return;

        _ = button.RegisterPropertyChangedCallback(Button.BackgroundProperty, (_, _) =>
        {
            UpdatePressedBackgroundLightness(button, backgroundLightnessFactor);
        });

        UpdatePressedBackgroundLightness(button, backgroundLightnessFactor);
    }

    static void OnPressedForegroundLightnessFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button button || e.NewValue is not double foregroundLightnessFactor)
            return;

        _ = button.RegisterPropertyChangedCallback(Button.ForegroundProperty, (_, _) =>
        {
            UpdatePressedForegroundLightness(button, foregroundLightnessFactor);
        });

        UpdatePressedForegroundLightness(button, foregroundLightnessFactor);
    }


    static void UpdatePointerOverBackgroundLightness(Button button, double backgroundLightnessFactor)
    {
        if (button.Background is not SolidColorBrush backgroundBrush)
            return;

        HslColor hsl = backgroundBrush.Color.ToHsl();
        hsl.L = Math.Max(Math.Min(hsl.L * backgroundLightnessFactor, 1.0), 0.0);
        Color newColor = ColorHelper.FromHsl(hue: hsl.H, saturation: hsl.S, lightness: hsl.L);

        button.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(newColor);
    }

    static void UpdatePointerOverForegroundLightness(Button button, double foregroundLightnessFactor)
    {
        if (button.Foreground is not SolidColorBrush foregroundBrush)
            return;

        HslColor hsl = foregroundBrush.Color.ToHsl();
        hsl.L = Math.Max(Math.Min(hsl.L * foregroundLightnessFactor, 1.0), 0.0);
        Color newColor = ColorHelper.FromHsl(hue: hsl.H, saturation: hsl.S, lightness: hsl.L);

        button.Resources["ButtonForegroundPointerOver"] = new SolidColorBrush(newColor);
    }

    static void UpdatePressedBackgroundLightness(Button button, double backgroundLightnessFactor)
    {
        if (button.Background is not SolidColorBrush backgroundBrush)
            return;

        HslColor hsl = backgroundBrush.Color.ToHsl();
        hsl.L = Math.Max(Math.Min(hsl.L * backgroundLightnessFactor, 1.0), 0.0);
        Color newColor = ColorHelper.FromHsl(hue: hsl.H, saturation: hsl.S, lightness: hsl.L);

        button.Resources["ButtonBackgroundPressed"] = new SolidColorBrush(newColor);
    }

    static void UpdatePressedForegroundLightness(Button button, double foregroundLightnessFactor)
    {
        if (button.Foreground is not SolidColorBrush foregroundBrush)
            return;

        HslColor hsl = foregroundBrush.Color.ToHsl();
        hsl.L = Math.Max(Math.Min(hsl.L * foregroundLightnessFactor, 1.0), 0.0);
        Color newColor = ColorHelper.FromHsl(hue: hsl.H, saturation: hsl.S, lightness: hsl.L);

        button.Resources["ButtonForegroundPressed"] = new SolidColorBrush(newColor);
    }
}
