using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Task_List_App.Behaviors;

/// <summary>
/// <see cref="Microsoft.Xaml.Interactivity.Behavior"/> helper for <see cref="Microsoft.UI.Xaml.Controls.TextBox"/> controls.
/// Automatically closes a <see cref="FlyoutPresenter"/> when the <see cref="Windows.System.VirtualKey.Enter"/> key is pressed.
/// </summary>
/// <remarks>
/// https://stackoverflow.com/questions/28400964/close-flyout-which-contains-a-usercontrol
/// </remarks>
[Microsoft.Xaml.Interactivity.TypeConstraint(typeof(TextBox))]
public class CloseFlyoutOnEnterBehavior : DependencyObject, IBehavior
{
    /* --- How To Use ---
    <Button HorizontalAlignment="Center" VerticalAlignment="Center" Content="Click Me">
        <Button.Flyout>
            <Flyout Placement="Bottom">
                <TextBox Width="200" Header="Name" PlaceholderText="Some Name">
                    <interact:Interaction.Behaviors>
                        <extras:CloseFlyoutOnEnterBehavior />
                    </interact:Interaction.Behaviors>
                </TextBox>
            </Flyout>
        </Button.Flyout>
    </Button>
    */

    public DependencyObject? AssociatedObject { get; set; }

    public void Attach(DependencyObject obj)
    {
        if (obj is not null)
        {
            this.AssociatedObject = obj;
            (obj as TextBox).KeyUp += TextBox_KeyUp;
        }
    }

    void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        // Only perform when [Enter] key is pressed.
        if (!e.Key.Equals(Windows.System.VirtualKey.Enter))
            return;

        var parent = this.AssociatedObject;
        while (parent != null)
        {
            if (parent is FlyoutPresenter)
            {
                ((parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                return;
            }
            else
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }

    public void Detach()
    {
        var obj = this.AssociatedObject as TextBox;
        if (obj != null)
            obj.KeyUp -= TextBox_KeyUp;
    }
}
