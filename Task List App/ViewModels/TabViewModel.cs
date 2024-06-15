using CommunityToolkit.Mvvm.ComponentModel;

namespace Task_List_App.ViewModels;

public class TabViewModel : ObservableRecipient
{
    private bool _option1 = false;
    private bool _option2 = true;

    public bool Option1
    {
        get => _option1;
        set => SetProperty(ref _option1, value);
    }

    public bool Option2
    {
        get => _option2;
        set => SetProperty(ref _option2, value);
    }

    public TabViewModel()
    {
        Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");
    }
}
