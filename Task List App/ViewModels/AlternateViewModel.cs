using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Task_List_App.Helpers;
using Task_List_App.Models;

namespace Task_List_App.ViewModels
{
    public class AlternateViewModel : ObservableRecipient
    {
        private TaskItem _sample = new();
        public TaskItem Sample
        {
            get => _sample;
            set => SetProperty(ref _sample, value);
        }

        private List<TaskItem> _samples = new();
        public List<TaskItem> Samples
        {
            get => _samples;
            set => SetProperty(ref _samples, value);
        }

        public AlternateViewModel()
        {
            // For template testing
            Samples = GenerateSampleItems();
            Sample = Samples[0];

        }

        #region [Template Test]
        /// <summary>
        /// Creates a new <see cref="List{TaskData}"/> object with example data.
        /// </summary>
        /// <returns><see cref="List{TaskData}"/></returns>
        List<TaskItem> GenerateSampleItems()
        {
            return new List<TaskItem>
            {
                new TaskItem { Title = "Task #1", Time = $"Soon", Created = DateTime.Now.AddDays(-1), Status   = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #2", Time = $"Tomorrow", Created = DateTime.Now.AddDays(-4), Status   = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #3", Time = $"A few days", Created = DateTime.Now.AddDays(-8), Status   = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #4", Time = $"A week from now", Created = DateTime.Now.AddDays(-12), Status  = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #5", Time = $"Two weeks from now", Created = DateTime.Now.AddDays(-16), Status  = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #6", Time = $"A month from now", Created = DateTime.Now.AddDays(-30), Status  = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #7", Time = $"Two months from now", Created = DateTime.Now.AddDays(-60), Status  = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #8", Time = $"Six months from now", Created = DateTime.Now.AddDays(-90), Status  = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
                new TaskItem { Title = "Task #9", Time = $"A year from now", Created = DateTime.Now.AddDays(-180), Status = $"Waiting", Completed = GeneralExtensions.RandomBoolean() },
            };
        }
        #endregion
    }
}
