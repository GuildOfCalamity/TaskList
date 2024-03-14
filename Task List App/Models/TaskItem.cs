using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Task_List_App.Models;

/// <summary>
/// This is a replacement for the <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> currently being used.
/// </summary>
public class TaskItem : INotifyPropertyChanged, ICloneable
{
	string _title = "";
	string _time = "";
	string _status = "";
	bool _completed = false;
	DateTime _created = DateTime.Now;
	DateTime? _completion = null;

    #region [Properties]
    public string Title
	{
		get { return _title; }
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged();
			}
		}
	}
	public string Status
	{
		get { return _status; }
		set
		{
			if (_status != value)
			{
				_status = value;
				OnPropertyChanged();
			}
		}
	}
	public string Time
	{
		get { return _time; }
		set
		{
			if (_time != value)
			{
				_time = value;
				OnPropertyChanged();
			}
		}
	}
	public bool Completed
	{
		get { return _completed; }
		set
		{
			if (_completed != value)
			{
				_completed = value;
				OnPropertyChanged();
			}
		}
	}
	public DateTime Created
	{
		get { return _created; }
		set
		{
			if (_created != value)
			{
				_created = value;
				OnPropertyChanged();
			}
		}
	}
    public DateTime? Completion
    {
        get { return _completion; }
        set
        {
            if (_completion != value)
            {
                _completion = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    public override string ToString()
	{
		return $"Title={Title}, Status={Status}, Time={Time}, Completed={Completed}";
	}

	#region [Support Mechanics]
	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
        try
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARNING] OnPropertyChanged: {ex.Message}");
        }
	}

    /// <summary>
    /// Support for deep-copy routines.
    /// </summary>
    public object Clone()
    {
        return new TaskItem
        {
            Title = this.Title,
            Status = this.Status,
            Time = this.Time,
            Created = this.Created,
            Completed = this.Completed,
            Completion = this.Completion
        };
    }
    #endregion
}

/* [How to Use]
    
    using System.Collections.Generic;
    using System.Windows;
    
    public partial class MainWindow : Window
    {
        private List<TaskItem> items;
		public List<TaskItem> Items => items;
    
        public MainWindow()
        {
            InitializeComponent();
            
            items = new List<TaskItem>
            {
                new Item { Title = "Task #1", Completed = false },
                new Item { Title = "Task #2", Completed = false },
                new Item { Title = "Task #3", Completed = false }
            };
    
            DataContext = this;
        }
    
        void UpdateItem()
        {
            // Assuming you have a mechanism to identify the specific item you want
            // to update. In this example, let's update the first item's values.
            items[0].Title = "Updated Item 1";
            items[0].Completed = true;
        }
    
        void UpdateUIButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateItem();
        }
    }
*/
