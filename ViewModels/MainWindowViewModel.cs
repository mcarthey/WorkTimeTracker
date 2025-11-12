using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels
{
    /// <summary>
    /// Main window ViewModel with proper dependency injection and SOLID principles.
    /// Delegates responsibilities to injected services instead of doing everything itself.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Timer _clock;
        private readonly IDataPersistenceService _dataPersistenceService;
        private readonly INotificationService _notificationService;
        private readonly IDirtyTrackingService _dirtyTrackingService;
        private readonly ITaskTimerViewModelFactory _viewModelFactory;

        private string _currentTime = DateTime.Now.ToString("T");
        private string _currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TaskTimerViewModel> Tasks { get; } = new();

        // Notification properties bound to service
        public string NotificationMessage => _notificationService.CurrentNotificationMessage ?? string.Empty;
        public string ConfirmationMessage => _notificationService.CurrentConfirmationMessage ?? string.Empty;

        private bool _notificationVisible;
        public bool NotificationVisible
        {
            get => _notificationVisible;
            set { _notificationVisible = value; OnPropertyChanged(); }
        }

        private bool _isConfirming;
        public bool IsConfirming
        {
            get => _isConfirming;
            set { _isConfirming = value; OnPropertyChanged(); }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); }
        }

        public string TotalTimeFormatted =>
            "Total Time: " + TimeSpan.FromSeconds(Tasks.Sum(t => t.TaskElapsedSeconds)).ToString(@"hh\:mm\:ss");

        public ICommand AddTaskCommand => new RelayCommand(AddTask);
        public ICommand RemoveTaskCommand => new RelayCommand<TaskTimerViewModel>(RemoveTask);
        public ICommand ClearTimersCommand => new RelayCommand(ClearAllTimers);
        public ICommand SaveDataCommand => new RelayCommand(async () => await SaveData());
        public ICommand LoadDataCommand => new RelayCommand(async () => await LoadData());
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand => new RelayCommand(CancelConfirmation);

        public MainWindowViewModel(
            IDataPersistenceService dataPersistenceService,
            INotificationService notificationService,
            IDirtyTrackingService dirtyTrackingService,
            ITaskTimerViewModelFactory viewModelFactory)
        {
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dirtyTrackingService = dirtyTrackingService ?? throw new ArgumentNullException(nameof(dirtyTrackingService));
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));

            // Setup clock timer
            _clock = new Timer(1000);
            _clock.Elapsed += (_, _) =>
            {
                CurrentTime = DateTime.Now.ToString("T");
                CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
                OnPropertyChanged(nameof(TotalTimeFormatted));
            };
            _clock.Start();

            // Subscribe to notification service events
            _notificationService.NotificationVisibilityChanged += (_, visible) =>
            {
                NotificationVisible = visible;
                IsConfirming = false;
                OnPropertyChanged(nameof(NotificationMessage));
            };

            _notificationService.ConfirmationVisibilityChanged += (_, visible) =>
            {
                NotificationVisible = visible;
                IsConfirming = visible;
                OnPropertyChanged(nameof(ConfirmationMessage));
            };

            // Initialize with default command
            ConfirmCommand = new RelayCommand(() => { });

            // Design-time sample data
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                Tasks.Add(_viewModelFactory.Create(new TaskTimer { Description = "Sample Task 1", Elapsed = TimeSpan.FromMinutes(45) }));
                Tasks.Add(_viewModelFactory.Create(new TaskTimer { Description = "Sample Task 2", Elapsed = TimeSpan.FromMinutes(120) }));
            }
            else
            {
                // Auto-load saved state on startup
                _ = LoadDataIfAvailable();
            }
        }

        /// <summary>
        /// Adds a new task to the collection
        /// </summary>
        private void AddTask()
        {
            var newTask = _viewModelFactory.Create(new TaskTimer());
            Tasks.Add(newTask);
            OnPropertyChanged(nameof(TotalTimeFormatted));

            _dirtyTrackingService.MarkTaskCreated();
            _notificationService.ShowNotification("Task added.");
        }

        private TaskTimerViewModel? _pendingDeleteTask;

        /// <summary>
        /// Initiates task removal with confirmation
        /// </summary>
        private void RemoveTask(TaskTimerViewModel? vm)
        {
            if (vm == null) return;

            _pendingDeleteTask = vm;
            var shortDesc = Truncate(vm.Description, 15);

            _notificationService.ShowConfirmation(
                $"Delete? {shortDesc}",
                ConfirmDeleteTask,
                CancelConfirmation);

            ConfirmCommand = new RelayCommand(ConfirmDeleteTask);
            OnPropertyChanged(nameof(ConfirmCommand));
        }

        /// <summary>
        /// Confirms and executes task deletion
        /// </summary>
        private void ConfirmDeleteTask()
        {
            if (_pendingDeleteTask != null)
            {
                _pendingDeleteTask.Cleanup();
                Tasks.Remove(_pendingDeleteTask);
                OnPropertyChanged(nameof(TotalTimeFormatted));

                _dirtyTrackingService.MarkTaskDeleted();
                _notificationService.ShowNotification("Task deleted.");
                _pendingDeleteTask = null;
            }
        }

        /// <summary>
        /// Cancels the current confirmation dialog
        /// </summary>
        private void CancelConfirmation()
        {
            _pendingDeleteTask = null;
            _notificationService.ClearConfirmation();
        }

        /// <summary>
        /// Initiates clearing all timers with confirmation
        /// </summary>
        private void ClearAllTimers()
        {
            _notificationService.ShowConfirmation(
                "Confirm reset all timers?",
                ConfirmClearAllTimers);

            ConfirmCommand = new RelayCommand(ConfirmClearAllTimers);
            OnPropertyChanged(nameof(ConfirmCommand));
        }

        /// <summary>
        /// Confirms and executes clearing all timers
        /// </summary>
        private void ConfirmClearAllTimers()
        {
            foreach (var t in Tasks)
            {
                t.StopTimer();
                t.Reset();
            }
            OnPropertyChanged(nameof(TotalTimeFormatted));
            _notificationService.ShowNotification("All timers reset.");
        }

        /// <summary>
        /// Saves application state and exports to text file (user-facing export feature)
        /// </summary>
        private async System.Threading.Tasks.Task SaveData()
        {
            try
            {
                // Export to text file (existing user feature)
                await _dataPersistenceService.ExportToTextFileAsync(Tasks);
                _notificationService.ShowNotification("Data exported successfully.");
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads application state from persistence
        /// </summary>
        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var loadedTasks = await _dataPersistenceService.LoadApplicationStateAsync();

                if (loadedTasks != null && loadedTasks.Any())
                {
                    // Clear existing tasks
                    foreach (var task in Tasks.ToList())
                    {
                        task.Cleanup();
                    }
                    Tasks.Clear();

                    // Add loaded tasks
                    foreach (var task in loadedTasks)
                    {
                        Tasks.Add(task);
                    }

                    OnPropertyChanged(nameof(TotalTimeFormatted));
                    _dirtyTrackingService.MarkAsSaved(); // Just loaded, so no unsaved changes
                    _notificationService.ShowNotification("Data loaded successfully.");
                }
                else
                {
                    _notificationService.ShowNotification("No saved data found.");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"Load failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Automatically loads data if available on startup
        /// </summary>
        private async System.Threading.Tasks.Task LoadDataIfAvailable()
        {
            if (_dataPersistenceService.CanRestoreState())
            {
                await LoadData();
            }
        }

        /// <summary>
        /// Saves application state automatically (called on exit)
        /// </summary>
        public async System.Threading.Tasks.Task SaveApplicationStateAsync()
        {
            try
            {
                await _dataPersistenceService.SaveApplicationStateAsync(Tasks);
                _dirtyTrackingService.MarkAsSaved();
            }
            catch (Exception ex)
            {
                // Log error - in production would use ILogger
                System.Diagnostics.Debug.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Truncates a string to specified length
        /// </summary>
        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
        }

        /// <summary>
        /// Cleanup method called on application exit
        /// </summary>
        public void Cleanup()
        {
            _clock?.Stop();
            _clock?.Dispose();

            foreach (var task in Tasks)
            {
                task.Cleanup();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
