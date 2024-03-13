using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Task_List_App.Controls;

public class CheckBoxWithDescription : CheckBox
{
    CheckBoxWithDescription _checkBoxSubTextControl;

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header",
        typeof(string),
        typeof(CheckBoxWithDescription),
        new PropertyMetadata(default(string)));

    [Localizable(true)]
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        "Description",
        typeof(string),
        typeof(CheckBoxWithDescription),
        new PropertyMetadata(default(string)));

    [Localizable(true)]
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public CheckBoxWithDescription()
    {
        _checkBoxSubTextControl = (CheckBoxWithDescription)this;
        this.Loaded += CheckBoxSubTextControl_Loaded;
    }

    protected override void OnApplyTemplate()
    {
        Update();
        base.OnApplyTemplate();
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(Header))
        {
            AutomationProperties.SetName(this, Header);
        }
    }

    void CheckBoxSubTextControl_Loaded(object sender, RoutedEventArgs e)
    {
        StackPanel panel = new StackPanel() 
        { 
            Orientation = Microsoft.UI.Xaml.Controls.Orientation.Vertical 
        };

        panel.Children.Add(new TextBlock() 
        { 
            Text = Header, 
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.WrapWholeWords 
        });

        // Add text box only if the description is not empty. Required for additional plugin options.
        if (!string.IsNullOrWhiteSpace(Description))
        {
            panel.Children.Add(new IsEnabledTextBlock()
            {
                Text = Description,
                Style = (Style)App.Current.Resources["SecondaryIsEnabledTextBlockStyle"],
            });
        }

        _checkBoxSubTextControl.Content = panel;
    }
}

[TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
public class IsEnabledTextBlock : Control
{
    public IsEnabledTextBlock()
    {
        this.DefaultStyleKey = typeof(IsEnabledTextBlock);
    }

    protected override void OnApplyTemplate()
    {
        IsEnabledChanged -= IsEnabledTextBlock_IsEnabledChanged;
        SetEnabledState();
        IsEnabledChanged += IsEnabledTextBlock_IsEnabledChanged;
        base.OnApplyTemplate();
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
       "Text",
       typeof(string),
       typeof(IsEnabledTextBlock),
       null);

    [Localizable(true)]
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void IsEnabledTextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        SetEnabledState();
    }

    private void SetEnabledState()
    {
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
    }
}

