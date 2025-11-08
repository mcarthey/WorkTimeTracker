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
        private bool _isRunning;
        private bool _justStopped;


        public event PropertyChangedEventHandler? PropertyChanged;

        public TaskTimerViewModel(TaskTimer task)
        {
            _task = task;
            _uiTimer = new Timer(1000);
            _uiTimer.Elapsed += (_, _) =>
            {
                // Update elapsed time continuously while running
                if (IsRunning)
                {
                    _task.UpdateElapsed();
                    OnPropertyChanged(nameof(ElapsedFormatted));
                    OnPropertyChanged(nameof(TaskElapsedSeconds));
                }
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

        // numeric value the main VM can sum
        public double TaskElapsedSeconds => _task.Elapsed.TotalSeconds;

        public ICommand StartCommand => new RelayCommand(Start);
        public ICommand StopCommand => new RelayCommand(Stop);

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool JustStopped
        {
            get => _justStopped;
            set
            {
                if (_justStopped != value)
                {
                    _justStopped = value;
                    OnPropertyChanged();
                }
            }
        }

        private void Start()
        {
            if (!IsRunning)
            {
                _task.Start();
                _uiTimer.Start();
                IsRunning = true; // 🔔 Triggers parent reaction via PropertyChanged
            }
        }

        public async void Stop()
        {
            if (IsRunning)
            {
                _task.Stop();
                _uiTimer.Stop();
                IsRunning = false;
                OnPropertyChanged(nameof(ElapsedFormatted));
                OnPropertyChanged(nameof(TaskElapsedSeconds));

                // 🔴 Flash indicator
                JustStopped = true;
                await Task.Delay(800); // 0.8 sec pulse
                JustStopped = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
