using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Task_List_App.Models;

/// <summary>
/// This is a replacement for the <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> currently being used.
/// </summary>
public class NoteItem : INotifyPropertyChanged, ICloneable
{
	string _title = "";
	string _data = "";
    bool _changed = false;
	DateTime _created = DateTime.Now;
	DateTime _updated = DateTime.Now;

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
	public string Data
	{
		get { return _data; }
		set
		{
			if (_data != value)
			{
				_data = value;
				OnPropertyChanged();
			}
		}
	}
    public bool Changed
    {
        get { return _changed; }
        set
        {
			if (_changed != value)
			{
				_changed = value;
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
    public DateTime Updated
    {
        get { return _updated; }
        set
        {
            if (_updated != value)
            {
                _updated = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

	/// <summary>
	/// Support for deep-copy routines.
	/// </summary>
    public object Clone()
    {
        return new NoteItem
        {
			Title = this.Title,
			Data = this.Data,
            Created = this.Created,
			Updated = this.Updated,
			Changed = this.Changed
        };
    }

    public override string ToString()
	{
		return $"Title={Title}, Changed={Changed}, Created={Created}, Updated={Updated}";
	}

	#region [Support Mechanics]
	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
        try
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName.Equals("Changed"))
				Debug.WriteLine($"[INFO] OnPropertyChanged detected for {nameof(Changed)} => {_changed}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARNING] OnPropertyChanged: {ex.Message}");
        }
	}
    #endregion
}
