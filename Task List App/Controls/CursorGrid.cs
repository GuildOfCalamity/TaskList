using System.Collections.Immutable;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;

namespace Task_List_App.Controls;

/// <summary>
/// For details on the <see cref="Microsoft.UI.Xaml.UIElement.ProtectedCursor"/> property, see...
/// https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.protectedcursor?view=windows-app-sdk-1.3
/// </summary>
public class CursorGrid : Grid
{
    public static readonly DependencyProperty EnableCursorOverrideProperty =
        DependencyProperty.Register(
            nameof(EnableCursorOverride),
            typeof(bool),
            typeof(CursorGrid),
            new PropertyMetadata(default, (d, _) => (d as CursorGrid)?.UpdateCursor()));

    public static readonly DependencyProperty InputSystemCursorShapeProperty =
        DependencyProperty.Register(
            nameof(InputSystemCursorShape),
            typeof(InputSystemCursorShape),
            typeof(CursorGrid),
            new PropertyMetadata(default, (d, _) => (d as CursorGrid)?.UpdateCursor()));
    
    public static ImmutableArray<InputSystemCursorShape> CursorOptions { get; set; }

    static CursorGrid()
    {
        CursorOptions = ImmutableArray.Create(Enum.GetValues<InputSystemCursorShape>());
    }

    public CursorGrid()
    {
        Background = new SolidColorBrush(Colors.Transparent);
    }

    public InputSystemCursorShape InputSystemCursorShape
    {
        get => (InputSystemCursorShape)GetValue(InputSystemCursorShapeProperty);
        set => SetValue(InputSystemCursorShapeProperty, value);
    }

    public bool EnableCursorOverride
    {
        get => (bool)GetValue(EnableCursorOverrideProperty);
        set => SetValue(EnableCursorOverrideProperty, value);
    }

    void UpdateCursor()
    {
        ProtectedCursor = EnableCursorOverride && InputSystemCursor.Create(InputSystemCursorShape) is InputCursor inputCursor ? inputCursor : null;
    }
}
