using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Task_List_App.Models;

/// <summary>
/// Home-brew observable collection type.
/// Handles the <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> events.
/// </summary>
/// <typeparam name="T">The type of object in the list.</typeparam>
public class ObservableList<T> : List<T>, INotifyCollectionChanged
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	public new void Add(T item)
	{
		base.Add(item);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
	}

	public new void Remove(T item)
	{
		base.Remove(item);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
	}

	public new void Clear()
	{
		base.Clear();
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		try
		{
			CollectionChanged?.Invoke(this, e);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"OnCollectionChanged: {ex.Message}");
		}
	}
}
