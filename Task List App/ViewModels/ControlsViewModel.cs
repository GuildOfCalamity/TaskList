using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Task_List_App.Contracts.Services;
using Task_List_App.Helpers;
using Task_List_App.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Task_List_App.ViewModels;

/// <summary>
/// This view model is used for custom controls demonstration.
/// </summary>
public class ControlsViewModel : ObservableRecipient
{
    #region [Properties]
    private bool _isBusy = false;
    private bool _configOpen = false;
    private string _lastMessage = "Empty";
    private SelectorBarItem? _barItem;
    private SelectorBarItem? _barItemCustom;
    PivotItem? _pivotItem;
    private Core.Contracts.Services.IFileService? fileService { get; set; }
    public event EventHandler<string>? ControlChangedEvent;
    public Action? ProgressButtonClickEvent { get; set; }

    public bool IsBusy
    {
        get => _isBusy;
        set 
        { 
            SetProperty(ref _isBusy, value);
            ControlChangedEvent?.Invoke(this, nameof(IsBusy) + $" changed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        }
    }

    public bool ConfigOpen
    {
        get => _configOpen;
        set
        {
            SetProperty(ref _configOpen, value);
            ControlChangedEvent?.Invoke(this, nameof(ConfigOpen) + $" changed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        }
    }

    public PivotItem? PivotSelected
    {
        get => _pivotItem;
        set
        {
            SetProperty(ref _pivotItem, value);
            ControlChangedEvent?.Invoke(this, nameof(PivotSelected) + $" changed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        }
    }

    public SelectorBarItem? BarItem
    {
        get => _barItem;
        set
        {
            SetProperty(ref _barItem, value);
            ControlChangedEvent?.Invoke(this, nameof(BarItem) + $" changed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        }
    }

    public SelectorBarItem? BarItemCustom
    {
        get => _barItemCustom;
        set
        {
            SetProperty(ref _barItemCustom, value);
            ControlChangedEvent?.Invoke(this, nameof(BarItemCustom) + $" changed at {DateTime.Now.ToString("hh:mm:ss.fff tt")}");
        }
    }

    public string LastMessage
    {
        get => _lastMessage;
        set => SetProperty(ref _lastMessage, value);
    }
    #endregion

    #region [Commands]
    public ICommand SampleMenuCommand { get; }
    public ICommand OpenConfigCommand { get; }
    #endregion

    #region [Events]
    /// <summary>
    /// <see cref="Task_List_App.Controls.EditBox"/> data changed event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnEditBoxDataChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ControlChangedEvent?.Invoke(this, $"[DataChanged]   OldValue ⇨ {e.OldValue}   NewValue ⇨ {e.NewValue}");
    }
    #endregion

    public ControlsViewModel()
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        // Sample command for flyouts.
        SampleMenuCommand = new RelayCommand<object>((obj) => 
        {
            if (obj is MenuFlyoutItem mfi)
            {
                if (mfi != null)
                    ControlChangedEvent?.Invoke(this, $"Invoked MenuFlyoutItem \"{mfi.Text}\"");
            }
            else if (obj is AppBarButton abb)
            {
                if (abb != null)
                    ControlChangedEvent?.Invoke(this, $"Invoked AppBarButton \"{abb.Label}\"");
            }
            else
            {
                Debug.WriteLine($"[WARNING] No action defined for type '{obj?.GetType()}'");
            }
        });

        // Sample command for custom TeachingTip.
        OpenConfigCommand = new RelayCommand(() => { ConfigOpen ^= true; });

        // Action example for our ProgressButton.
        ProgressButtonClickEvent += async () => 
        {
            IsBusy = true;
            if (!ConfigOpen) { ConfigOpen = true; }
            await Task.Delay(4000);
            IsBusy = false;
        };

        try
        {
            fileService = App.GetService<Core.Contracts.Services.IFileService>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARNING] ControlsViewModel: {ex.Message}");
            fileService = new Core.Services.FileService();
        }
    }
}
