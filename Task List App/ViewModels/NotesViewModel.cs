using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Windows.Storage;

using Task_List_App.Models;
using Task_List_App.Helpers;
using Task_List_App.Core.Services;
using Task_List_App.Views;
using System.Globalization;

namespace Task_List_App.ViewModels
{
    public partial class NotesViewModel : ObservableRecipient
    {
        #region [Properties]
        static bool _copying = false;

        public event EventHandler<bool>? NotesLoadedEvent;

        public List<NoteItem> NoteItems = new List<NoteItem>(); //public ObservableList<NoteItem> NoteItems = new();
        public List<NoteItem> CompareItems = new List<NoteItem>();

        [ObservableProperty]
        NoteItem? currentNote = null;

        [ObservableProperty]
        int currentIndex = 0;

        [ObservableProperty]
        int currentCount = 0;

        [ObservableProperty]
        bool isBusy = false;

        [ObservableProperty]
        bool canGoForward = true;

        [ObservableProperty]
        bool canGoBack = true;

        Core.Contracts.Services.IFileService? fileService { get; set; }
        #endregion

        /// <summary>
        /// Main constructor.
        /// </summary>
        public NotesViewModel()
        {
            Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");
            try
            {
                // IoC method...
                //fileService = Ioc.Default.GetService<Core.Contracts.Services.IFileService>();
                //fileService = App.Current.IoCServices.GetService<Core.Contracts.Services.IFileService>();

                // DI method...
                fileService = App.GetService<Core.Contracts.Services.IFileService>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NotesViewModel: {ex.Message}");
                fileService = new Core.Services.FileService();
            }

            // Make sure we save on exit.
            NotesPage.PageUnloadedEvent += (s, e) =>
            {
                if (NoteItems.Count > 0)
                    SaveNoteItemsJson();
            };

            // Make sure we save on note updated.
            NotesPage.TextChangedEvent += (s, e) =>
            {
                if (CurrentNote == null || e == false)
                    return;

                for (int i = 0; i < NoteItems.Count; i++)
                {
                    if (i < CompareItems.Count)
                    {
                        if (NoteItems[i].Title != CompareItems[i].Title ||
                            NoteItems[i].Data != CompareItems[i].Data)
                        {
                            NoteItems[i].Changed = true;
                            NoteItems[i].Updated = DateTime.Now;
                        }
                    }
                    else // boundary check failed, so rebuild the copy
                    {
                        try
                        {
                            // Check for reentry attempts.
                            if (_copying)
                                return;

                            _copying = true;
                            CompareItems.Clear();
                            foreach (var item in NoteItems)
                            {
                                CompareItems.Add(new NoteItem
                                {
                                    Title = item.Title,
                                    Data = item.Data,
                                    Changed = item.Changed,
                                    Created = item.Created,
                                    Updated = item.Updated,
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[WARNING] TextChangeEvent: {ex.Message}");
                        }
                        finally
                        {
                            _copying = false;
                        }
                    }
                }
            };

            LoadNoteItemsJson();

            if (NoteItems.Count > 0)
                CurrentNote = NoteItems[CurrentIndex];
            else
                CurrentNote = new NoteItem { Title = "No data available", Data = "Enter some data to create a note.", Updated = DateTime.Now, Created = DateTime.Now, Changed = false };
        }

        #region [ICommands]
        [RelayCommand]
        void PreviousNote()
        {
            if (CurrentIndex > 0)
            {
                // Update database if a change was detected.
                if (CurrentNote != null && CurrentNote.Changed)
                {
                    CurrentNote.Changed = false;
                    SaveNoteItemsJson();
                }
                CanGoForward = true;
                CurrentNote = NoteItems[--CurrentIndex];
            }
            else if (CurrentIndex == 0)
            {
                CanGoBack = false;
                CanGoForward = true;
                CurrentNote = NoteItems[CurrentIndex];
            }
            else
            {
                CanGoBack = false;
            }
        }

        [RelayCommand]
        void NextNote()
        {
            if (CurrentIndex < (NoteItems.Count - 1))
            {
                // Update database if a change was detected.
                if (CurrentNote != null && CurrentNote.Changed)
                {
                    CurrentNote.Changed = false;
                    SaveNoteItemsJson();
                }
                CanGoBack = true;
                CurrentNote = NoteItems[++CurrentIndex];
            }
            else
            {
                // If we've reached the end of the list
                // then create a new NoteItem and add it.
                var newNote = new NoteItem()
                {
                    Title = $"Note #{NoteItems.Count + 1}",
                    Data = "",
                    Changed = true,
                    Created = DateTime.Now, 
                    Updated = DateTime.Now
                };
                CurrentNote = newNote;
                NoteItems.Add(newNote);
                SaveNoteItemsJson();
                CanGoForward = false;
            }
        }

        /// <summary>
        /// We don't use this currently.
        /// </summary>
        /// <param name="item"><see cref="NoteItem"/></param>
        [RelayCommand]
        void UpdateNote(NoteItem item)
        {
            if (NoteItems.Count > 0 && item == null)
            {
                SaveNoteItemsJson();
                LoadNoteItemsJson();
            }
            else if (NoteItems.Count > 0 && item != null)
            {
                var idx = NoteItems.IndexOf(item);
                if (idx != -1)
                {
                    NoteItems.RemoveAt(idx);
                    NoteItems.Add(item);
                    SaveNoteItemsJson();
                }
            }
        }
        #endregion

        #region [JSON Serializer Routines]
        /// <summary>
        /// Loads the <see cref="TaskItem"/> collection.
        /// Requires <see cref="Core.Services.FileService"/>.
        /// </summary>
        public void LoadNoteItemsJson()
        {
            string baseFolder = "";

            if (App.IsClosing)
                return;

            try
            {
                IsBusy = true;

                if (App.IsPackaged)
                    baseFolder = ApplicationData.Current.LocalFolder.Path;
                else
                    baseFolder = Directory.GetCurrentDirectory();

                if (File.Exists(Path.Combine(baseFolder, App.DatabaseNotes)))
                {
                    Debug.WriteLine($"DaysUntilBackupReplaced is currently set to {fileService?.DaysUntilBackupReplaced}");

                    // FileService testing.
                    var jdata = fileService?.Read<List<NoteItem>>(baseFolder, App.DatabaseNotes);
                    if (jdata != null)
                    {
                        // Look out for duplication bugs.
                        NoteItems.Clear();
                        CompareItems.Clear();

                        // Sort and then validate each item.
                        var sorted = jdata.Select(t => t).OrderByDescending(t => t.Updated);
                        foreach (var item in sorted)
                        {
                            NoteItems.Add(item);
                            CompareItems.Add(new NoteItem
                            {
                                Title = item.Title,
                                Data = item.Data,
                                Changed = item.Changed,
                                Created = item.Created,
                                Updated = item.Updated,
                            });
                        }
                        CurrentCount = NoteItems.Count;
                    }
                    else
                        Debug.WriteLine($"Json data was null ({App.DatabaseNotes})");
                }
                else
                {   
                    // Inject some dummy data if file was not found.
                    NoteItems = CompareItems = GenerateDefaultNoteItems();
                    SaveNoteItemsJson();
                }
                // Signal any listeners.
                NotesLoadedEvent?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                // Signal any listeners.
                NotesLoadedEvent?.Invoke(this, false);
                Debug.WriteLine($"LoadNoteItemsJson: {ex.Message}");
                App.DebugLog($"LoadNoteItemsJson: {ex.Message}");
                Debugger.Break();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Saves the <see cref="TaskItem"/> collection.
        /// Requires <see cref="Core.Services.FileService"/>.
        /// </summary>
        public void SaveNoteItemsJson()
        {
            string baseFolder = "";

            if (App.IsClosing)
                return;

            try
            {
                IsBusy = true;

                if (App.IsPackaged)
                    baseFolder = ApplicationData.Current.LocalFolder.Path;
                else
                    baseFolder = Directory.GetCurrentDirectory();

                if (NoteItems.Count > 0)
                {
                    List<NoteItem> toSave = new();
                    foreach (var item in NoteItems)
                    {
                        item.Changed = false; // reset the "is modified" flag
                        if (!string.IsNullOrEmpty(item.Data))
                            toSave.Add(item); 
                    }
                    // Use the FileService
                    fileService?.Save(baseFolder, App.DatabaseNotes, toSave);
                    CurrentCount = toSave.Count;
                }
                else
                {
                    Debug.WriteLine($"No {nameof(NoteItem)}s to save.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveNoteItemsJson: {ex.Message}");
                App.DebugLog($"SaveNotesItemsJson: {ex.Message}");
                Debugger.Break();
            }
            finally
            { 
                IsBusy = false;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new <see cref="List{T}"/> object with example data.
        /// </summary>
        /// <returns><see cref="List{T}"/></returns>
        List<NoteItem> GenerateDefaultNoteItems()
        {
            return new List<NoteItem>
            {
                new NoteItem { Title = "Title number 1", Data = $"📌 Here is a sample note with data for day 1.", Created = DateTime.Now.AddDays(-1), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Title number 2", Data = $"📔 Here is a sample note with data for day 2.", Created = DateTime.Now.AddDays(-2), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Title number 3", Data = $"🔔 Here is a sample note with data for day 3.", Created = DateTime.Now.AddDays(-3), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Title number 4", Data = $"✔️ Here is a sample note with data for day 4.", Created = DateTime.Now.AddDays(-3), Updated = DateTime.Now, Changed = false },
            };
        }
    }
}
