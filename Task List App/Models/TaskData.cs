using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Lisk_App.Models;

/// <summary>
/// This was my first iteration that used <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>.
/// I have deprecated this in favor of the home-brew <see cref="System.ComponentModel.INotifyPropertyChanged"/>
/// <see cref="Task_List_App.Models.TaskItem"/> model.
/// </summary>
public class TaskData
{
    public bool Selected { get; set; } = false;
    public string Title { get; set; } = "";
    public string Time { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.Now;
    public string Status { get; set; } = "";
    public override string ToString()
    {
        return $"{Title} - {Status} ({Time})";
    }
}
