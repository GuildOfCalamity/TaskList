using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Windows.Storage;

using Task_List_App.Models;
using Task_List_App.Helpers;
using Task_List_App.Core.Services;
using Task_List_App.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Services.Maps;
using Task_List_App.Contracts.Services;

namespace Task_List_App.ViewModels
{
    public partial class NotesViewModel : ObservableRecipient
    {
        #region [Properties]
        static SemaphoreSlim semaSlim = new SemaphoreSlim(1, 1);
        static bool _copying = false;
        static bool _loaded = false;

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

        [ObservableProperty]
        bool editRequest = false;

        bool _canThrowError = false;
        public bool CanThrowError
        {
            get => _canThrowError;
            set
            {
                SetProperty(ref _canThrowError, value);
                ThrowExCommand?.NotifyCanExecuteChanged();
            }
        }

        public List<string> SortBy { get; set; } = new() { "Updated", "Created", "Natural" };
        Core.Contracts.Services.IFileService? fileService { get; set; }
        public INavigationService? NavService { get; private set; }
        public ShellViewModel ShellModel { get; private set; }
        public ICommand? TestCommand1 { get; }
        public ICommand? TestCommand2 { get; }
        public ICommand? EditRequestCommand { get; }
        public RelayCommand? ThrowExCommand { get; }
        #endregion

        /// <summary>
        /// Main constructor.
        /// </summary>
        public NotesViewModel()
        {
            Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}__{System.Reflection.MethodBase.GetCurrentMethod()?.Name} [{DateTime.Now.ToString("hh:mm:ss.fff tt")}]");
            try
            {
                // For future use.
                NavService = App.GetService<INavigationService>();

                // For testing home-brew relay commands
                TestCommand1 = new RelayCommand<NoteItem>(async (item) => await UpdateNote(item));
                TestCommand2 = new RelayCommand(async () => await UpdateNote(null));
                EditRequestCommand = new RelayCommand(() => { EditRequest = true; }, () => !EditRequest);
                ThrowExCommand = new RelayCommand(async () => await ThrowError(), () => CanThrowError);

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

            #region [Deferred Page Events]
            NotesPage.FinishedLoadingEvent += (s, e) => { _loaded = e; };
            NotesPage.EditRequestEvent += (s, e) => { EditRequest = e; };
            NotesPage.TextChangedEvent += (s, e) => // Make sure we save on note updated.
            {
                if (CurrentNote == null || e == false || !_loaded)
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
                            // Check for reentry attempts (similar to lock technique)
                            if (_copying)
                                return;

                            #region [Update our compare/undo set]
                            _copying = true;
                            CompareItems.Clear();
                            // Manual DTO method:
                            //foreach (var item in NoteItems)
                            //{
                            //    CompareItems.Add(new NoteItem
                            //    {
                            //        Title = item.Title,
                            //        Data = item.Data,
                            //        Changed = item.Changed,
                            //        Created = item.Created,
                            //        Updated = item.Updated,
                            //    });
                            //}
                            CompareItems = NoteItems.DeepCopy();
                            #endregion
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
            #endregion

            LoadNoteItemsJson();
            if (NoteItems.Count > 0)
                CurrentNote = NoteItems[CurrentIndex];
            else
                CurrentNote = new NoteItem { Title = "No data available", Data = "Enter some data to create a note.", Updated = DateTime.Now, Created = DateTime.Now, Changed = false };

            // Update the count on app startup.
            ShellModel = App.GetService<ShellViewModel>();
            UpdateNoteBadgeIcon(NoteItems.Count);

            // Listen for app-wide keypress events.
            ShellPage.ShellKeyboardEvent += ShellPage_ShellKeyboardEvent;
            // Listen for app-wide window events.
            ShellPage.MainWindowActivatedEvent += MainWindow_ActivatedEvent;
        }

        #region [Shell Window Events]
        /// <summary>
        /// After adding the NotesPage, this event will be shared now, so we'll 
        /// need to add an additional check to the CurrentRoute logic for avoiding 
        /// uneccessary saving when on a non-focused page.
        /// </summary>
        async void ShellPage_ShellKeyboardEvent(object? sender, Windows.System.VirtualKey e)
        {
            // Don't trigger action shortcuts if we're at the login page.
            if (!string.IsNullOrEmpty(NavService?.CurrentRoute) && 
               (NavService.CurrentRoute.Contains(nameof(LoginViewModel)) || 
                NavService.CurrentRoute.Contains(nameof(TasksViewModel))))
                return;

            if (e == Windows.System.VirtualKey.S)
            {
                Debug.WriteLine($"[INFO] Received Keyboard Save/Update Event ({NavService?.CurrentRoute})");
                if (NoteItems.Count > 0)
                    SaveNoteItemsJson();
            }
        }

        /// <summary>
        /// We'll save the user's data when the main window is deactivated.
        /// </summary>
        void MainWindow_ActivatedEvent(object? sender, WindowActivatedEventArgs e)
        {
            Debug.WriteLine($"[INFO] MainWindowActivatedEvent {e.WindowActivationState}");

            if (e.WindowActivationState == WindowActivationState.Deactivated && _loaded && !App.IsClosing)
            {
                // Don't trigger actions if we're at the login page or not on the Notes page.
                if (!string.IsNullOrEmpty(NavService?.CurrentRoute) &&
                   (NavService.CurrentRoute.Contains(nameof(LoginViewModel)) ||
                    NavService.CurrentRoute.Contains(nameof(TasksViewModel))))
                    return;

                Debug.WriteLine($"[INFO] Saving current notes.");
                if (NoteItems.Count > 0)
                    SaveNoteItemsJson();
            }
        }
        #endregion

        #region [Bound Events]
        /// <summary>
        /// Make sure we save on exit.
        /// </summary>
        public void NotesPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (NoteItems.Count > 0)
                SaveNoteItemsJson();
        }

        /// <summary>
        /// <see cref="ComboBox"/> event for sorting.
        /// </summary>
        public void SortingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loaded && CurrentNote != null)
            {
                if (EditRequest)
                    EditRequest = false;

                try
                {
                    var selection = e.AddedItems[0] as string;
                    if (!string.IsNullOrEmpty(selection))
                    {
                        // Since there may be unsaved changes, we'll want to save before reload.
                        if (NoteItems.Count > 0)
                            SaveNoteItemsJson();

                        LoadNoteItemsJson(selection);
                        if (NoteItems.Count > 0)
                        {
                            CurrentIndex = 0;
                            CurrentNote = NoteItems[CurrentIndex];
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = App.ShowMessageBox("Exception", $"SortingSelectionChanged: {ex.Message}", "OK", string.Empty, null, null);
                }
            }
        }
        #endregion

        #region [Misc Routines]
        void UpdateNoteBadgeIcon(int count)
        {
            if (ShellModel != null && ShellModel.NoteTotal != count)
                ShellModel.NoteTotal = count;
        }

        /// <summary>
        /// Creates a new <see cref="List{T}"/> object with example data.
        /// </summary>
        /// <returns><see cref="List{T}"/></returns>
        List<NoteItem> GenerateDefaultNoteItems()
        {
            return new List<NoteItem>
            {
                new NoteItem { Title = "Note title #1", Data = $"📌 Here is a sample note with data for cycle 1.", Created = DateTime.Now.AddDays(-1), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Note title #2", Data = $"📔 Here is a sample note with data for cycle 2.", Created = DateTime.Now.AddDays(-2), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Note title #3", Data = $"🔔 Here is a sample note with data for cycle 3.", Created = DateTime.Now.AddDays(-3), Updated = DateTime.Now, Changed = false },
                new NoteItem { Title = "Note title #4", Data = $"✔️ Here is a sample note with data for cycle 4.", Created = DateTime.Now.AddDays(-3), Updated = DateTime.Now, Changed = false },
            };
        }

        /// <summary>
        /// Command testing method.
        /// </summary>
        async Task ThrowError()
        {
            IsBusy = true;
            Func<int> testFunc = () => {
                int test = Random.Shared.Next(1, 11);
                if (test < 8) { throw new Exception("I don't like this number."); }
                return test;
            };
            try
            {
                int result = testFunc.Retry(3);
                Debug.WriteLine($"Passed: {result}");
            }
            catch (Exception) 
            { 
                Debug.WriteLine($"Retry attempts were exhausted!"); 
            }
            await Task.Delay(500);
            IsBusy = false;
        }
        #endregion

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
            // Hide edit textbox
            EditRequest = false;
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
                // Hide edit textbox
                EditRequest = false;
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
                // Hide markdown textblock
                EditRequest = true;
            }
        }

        /// <summary>
        /// We don't use this currently.
        /// </summary>
        /// <param name="item"><see cref="NoteItem"/></param>
        [RelayCommand]
        async Task<bool> UpdateNote(NoteItem? item)
        {
            semaSlim.Wait();

            TaskCompletionSource<bool> tcs = new();

            if (NoteItems.Count > 0 && item == null)
            {
                SaveNoteItemsJson();
                LoadNoteItemsJson();
                tcs.SetResult(false);
            }
            else if (NoteItems.Count > 0 && item != null)
            {
                var idx = NoteItems.IndexOf(item);
                if (idx != -1)
                {
                    NoteItems.RemoveAt(idx);
                    NoteItems.Add(item);
                    SaveNoteItemsJson();
                    tcs.SetResult(true);
                }
                else
                {
                    tcs.SetResult(false);
                }
            }

            semaSlim.Release();

            return await tcs.Task;
        }
        #endregion

        #region [JSON Serializer Routines]
        /// <summary>
        /// Loads the <see cref="NoteItem"/> collection.
        /// Requires <see cref="Core.Services.FileService"/>.
        /// </summary>
        public void LoadNoteItemsJson(string sortBy = "updated")
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
                    Debug.WriteLine($"[INFO] DaysUntilBackupReplaced is currently set to {fileService?.DaysUntilBackupReplaced}");

                    // Use our FileService for reading/writing.
                    var jdata = fileService?.Read<List<NoteItem>>(baseFolder, App.DatabaseNotes);
                    if (jdata != null)
                    {
                        // Look out for duplication bugs.
                        NoteItems.Clear();
                        CompareItems.Clear();

                        IOrderedEnumerable<NoteItem>? sorted = Enumerable.Empty<NoteItem>().OrderBy(x => 1);
                        
                        // Sort and then validate each item.
                        if (sortBy.StartsWith("updated", StringComparison.OrdinalIgnoreCase))
                            sorted = jdata.Select(t => t).OrderByDescending(t => t.Updated);
                        else if (sortBy.StartsWith("created", StringComparison.OrdinalIgnoreCase))
                            sorted = jdata.Select(t => t).OrderByDescending(t => t.Created);
                        else
                            sorted = jdata.Select(t => t).OrderBy(x => 1); // file order

                        foreach (var item in sorted)
                        {
                            NoteItems.Add(item);
                            // Manual DTO method:
                            //CompareItems.Add(new NoteItem
                            //{
                            //    Title = item.Title,
                            //    Data = item.Data,
                            //    Changed = item.Changed,
                            //    Created = item.Created,
                            //    Updated = item.Updated,
                            //});
                        }

                        // Update our compare/undo set (we don't want a reference copy)
                        CompareItems = NoteItems.DeepCopy();
                        CurrentCount = NoteItems.Count;
                    }
                    else
                        Debug.WriteLine($"Json data was null ({App.DatabaseNotes})");
                }
                else
                {   
                    // Inject some dummy data if file was not found.
                    NoteItems = GenerateDefaultNoteItems();
                    CompareItems = GenerateDefaultNoteItems();
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
        /// Saves the <see cref="NoteItem"/> collection.
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
                    // We could use our DeepCopy() helper, but we'll
                    // want to filter the current notes to see if there
                    // are any that should be automatically removed.
                    List<NoteItem> toSave = new();
                    foreach (var item in NoteItems)
                    {
                        // Reset the "is modified" flag.
                        item.Changed = false;
                        // Don't commit notes that do not have body data.
                        if (!string.IsNullOrEmpty(item.Data))
                            toSave.Add(item);
                    }

                    // Use our FileService for reading/writing.
                    fileService?.Save(baseFolder, App.DatabaseNotes, toSave);
                    CurrentCount = toSave.Count;

                    // Update our compare/undo set.
                    CompareItems.Clear();
                    CompareItems = toSave.DeepCopy();

                    // Update nav menu badge icon
                    UpdateNoteBadgeIcon(CurrentCount);
                }
                else
                {
                    Debug.WriteLine($"No {nameof(NoteItem)}s to save.");
                    _ = App.ShowMessageBox("SaveNoteItems", $"There are no {nameof(NoteItem)}s to save.", "OK", string.Empty, null, null);
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
    }
}
