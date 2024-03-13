using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;

namespace Task_List_App.Controls;

public class AutomaticExpander : Expander
{
    /// <summary>
    /// For unregistering, if needed.
    /// </summary>
    private long ContentPropertyChangedToken { get; set; }

    public AutomaticExpander()
    {
        this.Loaded += OnExpanderLoaded;
        this.Unloaded += OnExpanderUnloaded;
        this.PointerEntered += OnExpanderPointerEntered;
    }

    void OnExpanderLoaded(object sender, RoutedEventArgs e)
    {
        ContentPropertyChangedToken = RegisterPropertyChangedCallback(Expander.ContentProperty, OnContentPropertyChanged);
    }

    void OnContentPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        this.IsExpanded = true;
    }

    void OnExpanderUnloaded(object sender, RoutedEventArgs e)
    {
        UnregisterPropertyChangedCallback(Expander.ContentProperty, ContentPropertyChangedToken);
    }

    void OnExpanderPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!this.IsExpanded)
            this.IsExpanded = true;
    }
}
