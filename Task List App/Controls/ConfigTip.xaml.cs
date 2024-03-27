using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Task_List_App.ViewModels;

namespace Task_List_App.Controls;

/// <summary>
/// <see cref="ConfigTip"/> inherits from <see cref="TeachingTip"/>,
/// which inherits from <see cref="ContentControl"/>,
/// which inherits from <see cref="Control"/>, 
/// which inherits from <see cref="FrameworkElement"/>,
/// which inherits from <see cref="UIElement"/>, 
/// which inherits from <see cref="DependencyObject"/>.
/// </summary>
public sealed partial class ConfigTip : TeachingTip
{
    public ControlsViewModel ViewModel { get; private set; }
    public Core.Contracts.Services.IMessageService MsgService { get; private set; }

    public ConfigTip()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        ViewModel = App.GetService<ControlsViewModel>();
        MsgService = App.GetService<Core.Contracts.Services.IMessageService>();

        this.InitializeComponent();

        this.Loaded += ConfigTip_Loaded;
        this.Closing += ConfigTip_Closing;
    }

    void ConfigTip_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"[INFO] Loaded {nameof(ConfigTip)}");
    }

    void ConfigTip_Closing(TeachingTip sender, TeachingTipClosingEventArgs args)
    {
        Debug.WriteLine($"[INFO] Closing {nameof(ConfigTip)} because {args.Reason}");
        // [Possible Close Reasons]
        //    CloseButton  - tip was closed by the user clicking the close button
        //    LightDismiss - tip was closed by light-dismissal (clicking outside of the control)
        //    Programmatic - tip was programmatically closed

        ViewModel.ConfigOpen = false;
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SampleMenuCommand.Execute(this);
    }
}
