using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using WorkTimeTracker.Models;

// alias to avoid confusion
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels
{
    public class TaskTimerViewModel : INotifyPropertyChanged
    {
        private readonly TaskTimer _task;
        private readonly Timer _uiTimer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TaskTimerViewModel(TaskTimer task)
        {
            _task = task;
            _uiTimer = new Timer(1000);
            _uiTimer.Elapsed += (_, _) =>
            {
                OnPropertyChanged(nameof(ElapsedFormatted));
                OnPropertyChanged(nameof(TaskElapsedSeconds));
            };
        }

        public string Description
        {
            get => _task.Description;
            set
            {
                if (_task.Description != value)
                {
                    _task.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ElapsedFormatted => _task.Elapsed.ToString(@"hh\:mm\:ss");

        // NEW: numeric value the main VM can sum
        public double TaskElapsedSeconds => _task.Elapsed.TotalSeconds;

        public bool IsRunning => _task.IsRunning;

        public ICommand StartCommand => new RelayCommand(Start);
        public ICommand StopCommand => new RelayCommand(Stop);

        private void Start()
        {
            _task.Start();
            _uiTimer.Start();
            OnPropertyChanged(nameof(IsRunning));
        }

        private void Stop()
        {
            _task.Stop();
            _uiTimer.Stop();
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
