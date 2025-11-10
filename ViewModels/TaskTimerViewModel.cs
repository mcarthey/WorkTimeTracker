using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using WorkTimeTracker.Models;
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels
{
    public class TaskTimerViewModel : INotifyPropertyChanged
    {
        private readonly TaskTimer _task;
        private readonly Timer _uiTimer;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isRunning;
        public SolidColorBrush RowBrush { get; } = new SolidColorBrush(Colors.White);

        public TaskTimerViewModel(TaskTimer task)
        {
            _task = task;

            _uiTimer = new Timer(1000);
            _uiTimer.Elapsed += (_, _) =>
            {
                _task.UpdateElapsed();
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
        public double TaskElapsedSeconds => _task.Elapsed.TotalSeconds;

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

        public ICommand StartCommand => new RelayCommand(Start);
        public ICommand StopCommand => new RelayCommand(Stop);

        // --- Added for +15/-15 min functionality ---
        public ICommand Add15MinCommand => new RelayCommand(Add15Minutes);
        public ICommand Subtract15MinCommand => new RelayCommand(Subtract15Minutes);

        private void Add15Minutes()
        {
            _task.Elapsed = _task.Elapsed.Add(TimeSpan.FromMinutes(15));
            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));
        }

        private void Subtract15Minutes()
        {
            _task.Elapsed = _task.Elapsed - TimeSpan.FromMinutes(15);
            if (_task.Elapsed < TimeSpan.Zero)
                _task.Elapsed = TimeSpan.Zero;
            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));
        }
        // -------------------------------------------

        private void Start()
        {
            // Stop all others first
            (App.Current?.MainWindow?.DataContext as MainWindowViewModel)?.StopAllExcept(this);

            _task.Start();
            _uiTimer.Start();
            IsRunning = true;

            RowBrush.Dispatcher.Invoke(() =>
                RowBrush.Color = Color.FromRgb(170, 255, 204)); // soft green
        }

        public void Stop()
        {
            _task.Stop();
            _uiTimer.Stop();
            IsRunning = false;

            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));

            // briefly red, then fade back to white
            RowBrush.Dispatcher.Invoke(() =>
                RowBrush.Color = Color.FromRgb(255, 179, 179));

            var revertTimer = new Timer(700);
            revertTimer.Elapsed += (_, _) =>
            {
                revertTimer.Stop();
                RowBrush.Dispatcher.Invoke(() => RowBrush.Color = Colors.White);
            };
            revertTimer.Start();
        }

        public void Reset()
        {
            _task.Reset();
            IsRunning = false;
            RowBrush.Color = Colors.White;
            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
