using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Windows.Storage;

using Task_List_App.Models;
using System.Collections.Generic;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace Task_List_App.ViewModels;

/// <summary>
/// TODO: Add deferred database file backup system.
/// </summary>
public partial class TasksViewModel : ObservableRecipient
{
	/// <summary>
	/// An event that the task page can subscribe to.
	/// </summary>
	public event EventHandler<bool>? TasksLoadedEvent;

	public List<TaskItem> TaskItems = new(); //public ObservableList<TaskItem> TaskItems = new();

	[ObservableProperty]
    List<string> status = new()
    {
		"Completed", 
		"Not Started", 
		"Active", 
		"Almost Done", 
		"Waiting", 
		"Need Info", 
		"Unforeseen Issue", 
		"Canceled",
    };

    [ObservableProperty]
    List<string> times = new()
    {
        "Soon",
        "Tomorrow",
        "A few days",
        "A week from now",
        "Two weeks from now",
        "A month from now",
        "Six months from now",
        "A year from now"
    };

	/// <summary>
	/// A simple string sort will not work in our case, so we 
	/// will provide a sorting order to be used with the LINQ.
	/// </summary>
    string[] sortMatrix = new[] 
	{ 
		"Soon", 
		"Tomorrow",
		"A few days", 
		"A week from now", 
		"Two weeks from now", 
		"A month from now", 
        "Six months from now", 
		"A year from now" 
	};

    /// <summary>
    /// This would not display correctly in the ComboBox control.
	/// {ComboBox x:Name="cbTime" SelectedValuePath="Value"}
    /// </summary>
    [ObservableProperty]
	Dictionary<int, string> timesDict = new()
	{
		{ 1, "soon" },
		{ 2, "a few days" },
		{ 3, "a week from now" },
		{ 4, "two weeks from now" },
		{ 5, "a month from now" },
		{ 6, "six months from now" },
		{ 7, "a year from now" },
    };

    [ObservableProperty]
    string message = "";

	[ObservableProperty]
	bool refreshNeeded = false;

	[ObservableProperty]
	bool isBusy = false;

    public ObservableCollection<string> Messages = new ObservableCollection<string>();

    public Core.Services.FileService? fileService { get; private set; }

    public TasksViewModel()
    {
		Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");

        try
        {
            // Why does this not work as expected?
			// We are injecting via App.xaml.cs with "services.AddSingleton<IFileService, FileService>".
            fileService = App.GetService<Core.Services.FileService>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
            fileService = new Core.Services.FileService();
        }

        LoadTaskItemsJson();
    }

    #region [Methods supporting our INotifyPropertyChanged]
    /// <summary>
    /// Passing null to this command indicates to update all and re-load data.
    /// </summary>
    /// <param name="item"><see cref="TaskItem"/></param>
    [RelayCommand]
	void UpdateTaskItem(TaskItem item)
	{
		if (TaskItems.Count > 0 && item == null)
		{
			SaveTaskItemsJson();
            LoadTaskItemsJson();
            //RefreshNeeded = true;
		}
		else if (TaskItems.Count > 0 && item != null)
		{
			var idx = TaskItems.IndexOf(item);
			if (idx != -1)
			{
				TaskItems.RemoveAt(idx);
				TaskItems.Add(item);
				SaveTaskItemsJson();
				RefreshNeeded = true;
			}
		}
	}

    /// <summary>
    /// Removes a <see cref="TaskItem"/> from the database.
    /// </summary>
    /// <param name="item"><see cref="TaskItem"/></param>
    [RelayCommand]
	void DeleteTaskItem(TaskItem? item)
	{
		if (item is null)
			return;

		var idx = TaskItems.IndexOf(item);
		if (idx != -1)
		{
			TaskItems.RemoveAt(idx);
			SaveTaskItemsJson();
			LoadTaskItemsJson();
		}
	}

    /// <summary>
    /// Adds a <see cref="TaskItem"/> to the database.
    /// </summary>
    /// <param name="item"><see cref="TaskItem"/></param>
	[RelayCommand]
	void AddTaskItem(TaskItem? item)
	{
		if (item is null)
			return;

		var idx = TaskItems.IndexOf(item);
		if (idx == -1)
		{
			TaskItems.Add(item);
			SaveTaskItemsJson();
			LoadTaskItemsJson();
		}
	}

    /// <summary>
    /// Toggle the completed status of a <see cref="TaskItem"/>.
    /// </summary>
    /// <param name="item"><see cref="TaskItem"/></param>
	[RelayCommand]
	void ToggleCompletedItem(TaskItem? item)
	{
		if (item is null)
			return;

		Debug.WriteLine($"ToggleCompletedItem: {item}");
		// This will be inverted since the Completed flag is bound to
		// the checkbox and will already be updated once we get here.
		if (item.Completed) 
		{ 
			item.Status = status[0];
			item.Completion = DateTime.Now;
		}
		else 
		{ 
			item.Status = status[1];
			item.Completion = null;
		}

        RefreshNeeded = true;
	}

	/// <summary>
	/// Traverses all items looking for task.Completed==True and sets the task.Status="Completed".
	/// </summary>
	/// <returns>true if successful, false otherwise</returns>
	public bool CompleteSelectedTaskItems()
	{
		if (TaskItems.Count == 0 || App.IsClosing)
			return false;

		foreach (var task in TaskItems)
		{
			if (task.Completed)
			{
				Debug.WriteLine($"> Marking task '{task.Title}' as '{status[0]}'.");
				task.Status = status[0];
			}
		}
		SaveTaskItemsJson();
		LoadTaskItemsJson();

		return true;
	}

    /// <summary>
    /// Traverses all items looking for task.Completed==True and deletes them from the list.
    /// </summary>
    /// <returns>true if successful, false otherwise</returns>
    public bool RemoveCompletedTaskItems()
	{
		if (TaskItems.Count == 0 || App.IsClosing)
			return false;

		List<TaskItem> toRemove = new();
		foreach (var task in TaskItems)
		{
			if (task.Completed)
				toRemove.Add(task);
		}

		foreach (var task in toRemove)
		{
			TaskItems.Remove(task);
		}

		SaveTaskItemsJson();
		RefreshNeeded = true;
		return true;
	}

	public bool ResortAllTasks()
	{
        if (TaskItems.Count == 0)
            return false;

        TaskItems = TaskItems.Select(t => t).OrderBy(t => t.Completed).ThenBy(t => Array.IndexOf(sortMatrix, t.Time)).ToList();

        RefreshNeeded = true;

        return true;
    }

    public int TallyCompletedTaskItems()
	{
		if (TaskItems.Count == 0)
			return 0;

		var count = TaskItems.Select(o => o).Where(m => m.Completed == true).Count();

		return count;
	}

	public int TallyUncompletedTaskItems()
	{
		if (TaskItems.Count == 0)
			return 0;

		var count = TaskItems.Select(o => o).Where(m => m.Completed == false).Count();

		return count;
	}

	public List<TaskItem> GetPendingTaskItems()
	{
		List<TaskItem> results = new();

		if (TaskItems.Count == 0 || App.IsClosing)
			return results;

        var samples = TaskItems.Select(o => o).Where(m => m.Completed == false && m.Created < DateTime.Now);

		foreach (var item in samples)
		{
			var dt = DateTime.Now - item.Created;
			switch (item.Time) 
			{
                case string str when str.Contains(times[0]): // "soon"
                    if (dt.TotalDays >= 1)
						results.Add(item);
					break;
                case string str when str.Contains(times[1]): // "tomorrow"
                    if (dt.TotalDays >= 2)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[2]): // "a few days"
                    if (dt.TotalDays >= 4)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[3]): // "a week from now"
                    if (dt.TotalDays >= 8)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[4]): // "two weeks from now"
                    if (dt.TotalDays >= 14)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[5]): // "a month from now"
                    if (dt.TotalDays >= 29)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[6]): // "six months from now"
                    if (dt.TotalDays >= 171)
                        results.Add(item);
                    break;
                case string str when str.Contains(times[7]): // "a year from now"
                    if (dt.TotalDays >= 353)
                        results.Add(item);
                    break;
                default:
					Debug.WriteLine($"WARNING: Item's time '{item.Time}' is not defined.");
					break;
            }
		}

		return results;
	}

	public IEnumerable<TaskItem> GetCompletionTimes()
	{
		if (TaskItems.Count == 0 || App.IsClosing)
			return Enumerable.Empty<TaskItem>();

		return TaskItems.Select(o => o).Where(m => m.Completed == true && m.Completion != null);
	}

	public TaskItem? FindTaskItem(string? title)
	{
		if (string.IsNullOrEmpty(title))
			return null;

		var existing = TaskItems.Select(o => o).Where(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
		if (existing != null)
			return existing;

		return null;
	}

	public bool UpdateTaskStatus(TaskItem item, bool completed)
	{
		if (TaskItems.Count == 0 || item is null)
			return false;

		if (TaskItems.Count > 0 && item != null)
		{
			if (completed) // mark as complete
			{
				item.Status = status[0];
				SaveTaskItemsJson();
				RefreshNeeded = true;
			}
			else // mark as not started
			{
				item.Status = status[1];
				SaveTaskItemsJson();
				RefreshNeeded = true;
			}
			return true;
		}

		return false;
	}
    #endregion

    /// <summary>
    /// All of the ViewModel methods are fast, so this method can be used to 
	/// trigger the busy flag in certain scenarios where you might want the 
	/// user to see that some activity is occurring. As the database grows 
	/// large this might not be neccessary in the future.
    /// </summary>
    /// <param name="ts"><see cref="TimeSpan?"/></param>
    /// <returns><see cref="Task.CompletedTask"/></returns>
    public async Task SignalBusyCycle(TimeSpan? ts)
    {
        if (App.IsClosing)
            return;

        IsBusy = true;
		await Task.Delay(ts ?? TimeSpan.FromSeconds(2));
		IsBusy = false;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new <see cref="List{TaskData}"/> object with example data.
    /// </summary>
    /// <returns><see cref="List{TaskData}"/></returns>
    List<TaskItem> GenerateDefaultTaskItems()
    {
        return new List<TaskItem>
        {
            new TaskItem { Title = "Task #1", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-1), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #2", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-4), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = true },
            new TaskItem { Title = "Task #3", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-8), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #4", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-12), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #5", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-16), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #6", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-30), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #7", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-90), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
            new TaskItem { Title = "Task #8", Time = $"{times[Random.Shared.Next(0, times.Count)]}", Created = DateTime.Now.AddDays(-180), Status = $"{status[Random.Shared.Next(0, status.Count)]}", Completed = false },
        };
    }

    #region [JSON Serializer Routines]
    /// <summary>
    /// Loads the <see cref="TaskItem"/> collection.
 	/// Requires <see cref="Core.Services.FileService"/>.
    /// </summary>
    public void LoadTaskItemsJson()
    {
        string baseFolder = "";

        if (App.IsClosing)
            return;

        try
        {
            if (App.IsPackaged)
                baseFolder = ApplicationData.Current.LocalFolder.Path;
            else
                baseFolder = Directory.GetCurrentDirectory();

            if (File.Exists(Path.Combine(baseFolder, @"TaskItems.json")))
            {
				Debug.WriteLine($"DaysUntilBackupReplaced is currently set to {fileService?.DaysUntilBackupReplaced}");

                // FileService testing.
                var jdata = fileService?.Read<List<TaskItem>>(baseFolder, "TaskItems.json");
                if (jdata != null)
                {
                    // Look out for duplication bugs.
                    TaskItems.Clear();

                    // Sort and then validate each item.
                    var sorted = jdata.Select(t => t).OrderBy(t => t.Completed).ThenBy(t => Array.IndexOf(sortMatrix, t.Time));
                    foreach (var item in sorted)
                    {
                        // During load check for mis-matched status vs completed.
                        if (item.Completed && item.Status != status[0])
                            item.Status = status[0];
                        else if (item.Status == status[0] && !item.Completed)
                            item.Completed = true;

                        TaskItems.Add(item);
                    }
                }
                else
                    Debug.WriteLine($"Json data was null.");
            }
            else
            {   // Inject some dummy data if file was not found.
                TaskItems = GenerateDefaultTaskItems();
                SaveTaskItemsJson();
            }
            // Signal any listeners.
            TasksLoadedEvent?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            // Signal any listeners.
            TasksLoadedEvent?.Invoke(this, false);
            Debug.WriteLine($"LoadTaskItemsJson: {ex.Message}");
            App.DebugLog($"LoadTaskItemsJson: {ex.Message}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// Saves the <see cref="TaskItem"/> collection.
 	/// Requires <see cref="Core.Services.FileService"/>.
    /// </summary>
    public void SaveTaskItemsJson()
    {
        string baseFolder = "";

        if (App.IsClosing)
            return;

        try
        {
            if (App.IsPackaged)
                baseFolder = ApplicationData.Current.LocalFolder.Path;
            else
                baseFolder = Directory.GetCurrentDirectory();

            if (TaskItems.Count > 0)
            {
                // There is a two-step process here in cases where the collection does not serialize well.
                List<TaskItem> toSave = new();
                foreach (var item in TaskItems) { toSave.Add(item); }

                // Use the FileService
                fileService?.Save(baseFolder, "TaskItems.json", toSave);
            }
            else
            {
                Debug.WriteLine($"No task items to save.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveTaskItemsJson: {ex.Message}");
            App.DebugLog($"SaveTaskItemsJson: {ex.Message}");
            Debugger.Break();
        }
    }
    #endregion

    #region [XML Serializer Routines]
    /// <summary>
    /// Loads the <see cref="TaskItem"/> collection.
	/// Does not require <see cref="Core.Services.FileService"/>.
    /// </summary>
    public void LoadTaskItemsXml()
	{
		string baseFolder = "";

		if (App.IsClosing)
			return;

		try
		{
            if (App.IsPackaged)
				baseFolder = ApplicationData.Current.LocalFolder.Path;
			else
				baseFolder = Directory.GetCurrentDirectory();

			if (File.Exists(Path.Combine(baseFolder, @"TaskItems.xml")))
			{
				var data = File.ReadAllText(Path.Combine(baseFolder, @"TaskItems.xml"));
				var serializer = new XmlSerializer(typeof(List<TaskItem>));
				if (serializer != null)
				{
					// Look out for duplication bugs.
					TaskItems.Clear();

					#region [Default Sorting]
					var tempList = serializer.Deserialize(new StringReader(data)) as List<TaskItem> ?? GenerateDefaultTaskItems();
					var sorted = tempList.Select(t => t).OrderBy(t => t.Completed);
					foreach (var item in sorted) 
					{ 
						// During load check for mis-matched status vs completed.
						if (item.Completed && item.Status != status[0])
							item.Status = status[0];
                        else if (item.Status == status[0] && !item.Completed)
                            item.Completed = true;

                        TaskItems.Add(item); 
					}
					#endregion
				}
				else
					Debug.WriteLine($"XmlSerializer was null.");
            }
            else
			{   // Inject some dummy data if file was not found.
				TaskItems = GenerateDefaultTaskItems();
				SaveTaskItemsXml();
			}
			// Signal any listeners.
			TasksLoadedEvent?.Invoke(this, true);
		}
		catch (Exception ex)
		{
			// Signal any listeners.
			TasksLoadedEvent?.Invoke(this, false);
			Debug.WriteLine($"LoadTaskItemsXml: {ex.Message}");
			App.DebugLog($"LoadTaskItemsXml: {ex.Message}");
			Debugger.Break();
		}
	}

    /// <summary>
    /// Saves the <see cref="TaskItem"/> collection.
	/// Does not require <see cref="Core.Services.FileService"/>.
    /// </summary>
    public void SaveTaskItemsXml()
	{
		string baseFolder = "";

		if (App.IsClosing)
			return;

		try
		{
			if (App.IsPackaged)
				baseFolder = ApplicationData.Current.LocalFolder.Path;
			else
				baseFolder = Directory.GetCurrentDirectory();

			var serializer = new XmlSerializer(typeof(List<TaskItem>));
			if (TaskItems.Count > 0 && serializer != null)
			{
				// There is a two-step process here in cases where the collection does not serialize well.
				List<TaskItem> toSave = new();
				foreach (var item in TaskItems) { toSave.Add(item); }
                var stringWriter = new StringWriter();
				serializer.Serialize(stringWriter, toSave);
				var applicationData = stringWriter.ToString();
				
                File.WriteAllText(Path.Combine(baseFolder, @"TaskItems.xml"), applicationData);

				if (!File.Exists(Path.Combine(baseFolder, @"TaskItems.xml.bak")))
					File.WriteAllText(Path.Combine(baseFolder, @"TaskItems.xml.bak"), applicationData);
				else
				{
                    var efi = new FileInfo(Path.Combine(baseFolder, @"TaskItems.xml.bak")).LastWriteTime;
					// If backup is older than 24 hours
					if (efi.TimeOfDay.TotalDays >= 1)
					{
                        File.Delete(Path.Combine(baseFolder, @"TaskItems.xml.bak"));
                        File.WriteAllText(Path.Combine(baseFolder, @"TaskItems.xml.bak"), applicationData);
                    }
                }
			}
			else
			{
				Debug.WriteLine($"No task items to save.");
			}
        }
        catch (Exception ex)
		{
			Debug.WriteLine($"SaveTaskItemsXml: {ex.Message}");
			App.DebugLog($"SaveTaskItemsXml: {ex.Message}");
			Debugger.Break();
		}
	}

    /// <summary>
    /// Updates the <see cref="TaskItem"/>'s title, if there is a match.
	/// Does not require <see cref="Core.Services.FileService"/>.
    /// </summary>
    public void UpdateTaskTitle(string title, string newTitle)
    {
		if (App.IsClosing)
			return;
		
        var existing = TaskItems.Select(o => o).Where(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (existing != null)
        {
            // Remove existing one and replace with updated one.
            var idx = TaskItems.IndexOf(existing);
            if (idx != -1)
            {
				TaskItems.RemoveAt(idx);
				TaskItems.Insert(idx, new TaskItem { Title = newTitle, Created = existing.Created, Status = existing.Status });
            }
        }
        else
        {
			TaskItems.Add(new TaskItem { Title = newTitle, Time = times[1], Created = DateTime.Now, Status = status[1] });
        }
		SaveTaskItemsXml();
		LoadTaskItemsXml();
    }
    #endregion

    #region [Message Log Test]
    void InitMessageControl()
    {
        //MessageItemsControl.ItemsSource = Messages;
    }

    async void AddMessage(string msg)
    {
        Messages.Add(msg);
        await Task.Yield(); // wait for layout update.

        //MessageScrollViewer.ChangeView(null, MessageScrollViewer.ScrollableHeight, null);
    }
    #endregion

}
