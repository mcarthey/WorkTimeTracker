using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
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
        public ICommand SaveDataCommand => new RelayCommand(SaveData);

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
        }

        private void RemoveTask(TaskTimerViewModel? vm)
        {
            if (vm == null) return;

            var result = MessageBox.Show(
                $"Delete task \"{vm.Description}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Tasks.Remove(vm);
                OnPropertyChanged(nameof(TotalTimeFormatted));
            }
        }

        private void ClearAllTimers()
        {
            var result = MessageBox.Show(
                "Reset all timers to 00:00:00?",
                "Confirm Clear",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            foreach (var t in Tasks)
            {
                t.Stop();
                t.Reset();
            }

            OnPropertyChanged(nameof(TotalTimeFormatted));
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
            MessageBox.Show($"Saved data to {fileName}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
