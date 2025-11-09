using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using System.Threading.Tasks;
using WorkTimeTracker.Models;
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Timer _clock;
        private string _currentTime = DateTime.Now.ToString("T");
        private string _currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TaskTimerViewModel> Tasks { get; } = new();

        public enum NotificationMode
        {
            Info,
            Confirm
        }

        private NotificationMode _notificationMode;
        public NotificationMode CurrentNotificationMode
        {
            get => _notificationMode;
            set { _notificationMode = value; OnPropertyChanged(); }
        }


        private bool _isConfirming;
        public bool IsConfirming
        {
            get => _isConfirming;
            set { _isConfirming = value; OnPropertyChanged(); }
        }

        private ICommand _confirmCommand;
        public ICommand ConfirmCommand
        {
            get => _confirmCommand;
            set { _confirmCommand = value; OnPropertyChanged(); }
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

        private string _notificationMessage;
        public string NotificationMessage
        {
            get => _notificationMessage;
            set { _notificationMessage = value; OnPropertyChanged(); }
        }

        private bool _notificationVisible;
        public bool NotificationVisible
        {
            get => _notificationVisible;
            set { _notificationVisible = value; OnPropertyChanged(); }
        }

        public string TotalTimeFormatted =>
            "Total Time: " + TimeSpan.FromSeconds(Tasks.Sum(t => t.TaskElapsedSeconds)).ToString(@"hh\:mm\:ss");

        public ICommand AddTaskCommand => new RelayCommand(AddTask);
        public ICommand RemoveTaskCommand => new RelayCommand<TaskTimerViewModel>(RemoveTask);
        public ICommand ClearTimersCommand => new RelayCommand(ClearAllTimers);
        public ICommand SaveDataCommand => new RelayCommand(SaveData);

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get => _cancelCommand ??= new RelayCommand(CancelConfirmation);
        }

        public MainWindowViewModel()
        {
            _clock = new Timer(1000);
            _clock.Elapsed += (_, _) =>
            {
                CurrentTime = DateTime.Now.ToString("T");
                CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
                OnPropertyChanged(nameof(TotalTimeFormatted));
            };
            _clock.Start();
        }

        public void StopAllExcept(TaskTimerViewModel current)
        {
            foreach (var t in Tasks)
            {
                if (t != current && t.IsRunning)
                    t.Stop();
            }
        }

        private void AddTask()
        {
            Tasks.Add(new TaskTimerViewModel(new TaskTimer()));
            OnPropertyChanged(nameof(TotalTimeFormatted));
            ShowNotification("Task added.");
        }

        private TaskTimerViewModel? _pendingDeleteTask;

        private void RemoveTask(TaskTimerViewModel? vm)
        {
            if (vm == null) return;

            _pendingDeleteTask = vm;
            // Truncate to 40 characters (adjust as needed)
            var shortDesc = Truncate(vm.Description, 15);
            NotificationMessage = $"Delete? {shortDesc}";
            IsConfirming = true;
            NotificationVisible = true;
            CurrentNotificationMode = NotificationMode.Confirm;
            ConfirmCommand = new RelayCommand(ConfirmDeleteTask);
        }


        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
        }

        private void ConfirmDeleteTask()
        {
            if (_pendingDeleteTask != null)
            {
                Tasks.Remove(_pendingDeleteTask);
                OnPropertyChanged(nameof(TotalTimeFormatted));
                ShowNotification("Task deleted.");
                _pendingDeleteTask = null;
            }
            IsConfirming = false;
            CurrentNotificationMode = NotificationMode.Info;
        }

        private void CancelConfirmation()
        {
            IsConfirming = false;
            NotificationVisible = false;
            NotificationMessage = string.Empty;
            _pendingDeleteTask = null;
            CurrentNotificationMode = NotificationMode.Info;
        }


        private void ClearAllTimers()
        {
            NotificationMessage = "Confirm reset all timers?";
            IsConfirming = true;
            NotificationVisible = true;
            ConfirmCommand = new RelayCommand(ConfirmClearAllTimers);
            // Do NOT call ShowNotification here!
        }

        private void ConfirmClearAllTimers()
        {
            foreach (var t in Tasks)
            {
                t.Stop();
                t.Reset();
            }
            OnPropertyChanged(nameof(TotalTimeFormatted));
            ShowNotification("All timers reset.");
            IsConfirming = false;
        }

        private void SaveData()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"time_tracking_{timestamp}.txt";
            var lines = new[] {
                $"Work Time Tracker - {DateTime.Now}",
                new string('=', 40)
            }.Concat(Tasks.Select((t, i) =>
                $"{i + 1}. {t.Description,-25} {t.ElapsedFormatted}"))
             .Concat(new[] {
                 new string('-', 40),
                 $"Total: {TimeSpan.FromSeconds(Tasks.Sum(t => t.TaskElapsedSeconds)):hh\\:mm\\:ss}"
             });

            File.WriteAllLines(fileName, lines);
            ShowNotification($"Saved data to {fileName}");
        }

        private async void ShowNotification(string message, int durationMs = 3000)
        {
            NotificationMessage = message;
            NotificationVisible = true;
            IsConfirming = false;
            CurrentNotificationMode = NotificationMode.Info;
            await Task.Delay(durationMs);
            // Only clear if still in Info mode
            if (CurrentNotificationMode == NotificationMode.Info)
            {
                NotificationVisible = false;
                NotificationMessage = string.Empty;
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
