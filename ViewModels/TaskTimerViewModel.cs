using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels
{
    public class TaskTimerViewModel : INotifyPropertyChanged
    {
        private readonly TaskTimer _task;
        private readonly Timer _uiTimer;
        private readonly ITimerCoordinationService _timerCoordinationService;
        private readonly IDirtyTrackingService _dirtyTrackingService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isRunning;
        private string _committedDescription; // Tracks last committed description value
        public SolidColorBrush RowBrush { get; } = new SolidColorBrush(Colors.White);

        public TaskTimerViewModel(
            TaskTimer task,
            ITimerCoordinationService timerCoordinationService,
            IDirtyTrackingService dirtyTrackingService)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _timerCoordinationService = timerCoordinationService ?? throw new ArgumentNullException(nameof(timerCoordinationService));
            _dirtyTrackingService = dirtyTrackingService ?? throw new ArgumentNullException(nameof(dirtyTrackingService));

            _committedDescription = task.Description;

            _uiTimer = new Timer(1000);
            _uiTimer.Elapsed += (_, _) =>
            {
                _task.UpdateElapsed();
                OnPropertyChanged(nameof(ElapsedFormatted));
                OnPropertyChanged(nameof(TaskElapsedSeconds));
            };

            // Register with coordination service
            _timerCoordinationService.RegisterTimer(this);
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
                    // Note: This still fires on every keystroke for UI binding
                    // But we DON'T mark as dirty here - only on commit
                }
            }
        }

        /// <summary>
        /// Commits the current description value, marking the task as updated.
        /// Call this on LostFocus or Enter key press, NOT on every keystroke.
        /// </summary>
        public void CommitDescription()
        {
            if (_committedDescription != _task.Description)
            {
                _committedDescription = _task.Description;
                _dirtyTrackingService.MarkTaskUpdated();
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

            // Mark as changed since time was manually adjusted
            _dirtyTrackingService.MarkTimerChanged();
        }

        private void Subtract15Minutes()
        {
            _task.Elapsed = _task.Elapsed - TimeSpan.FromMinutes(15);
            if (_task.Elapsed < TimeSpan.Zero)
                _task.Elapsed = TimeSpan.Zero;
            OnPropertyChanged(nameof(ElapsedFormatted));
            OnPropertyChanged(nameof(TaskElapsedSeconds));

            // Mark as changed since time was manually adjusted
            _dirtyTrackingService.MarkTimerChanged();
        }
        // -------------------------------------------

        private void Start()
        {
            // Use injected service instead of tight coupling to MainWindowViewModel
            _timerCoordinationService.StopAllTimersExcept(this);

            _task.Start();
            _uiTimer.Start();
            IsRunning = true;

            // Mark timer as changed for persistence tracking
            _dirtyTrackingService.MarkTimerChanged();

            RowBrush.Dispatcher.Invoke(() =>
                RowBrush.Color = Color.FromRgb(170, 255, 204)); // soft green
        }

        private void Stop()
        {
            StopTimer();
        }

        /// <summary>
        /// Stops the timer. Can be called by command or by TimerCoordinationService.
        /// </summary>
        public void StopTimer()
        {
            if (!IsRunning)
                return;

            _task.Stop();
            _uiTimer.Stop();
            IsRunning = false;

            // Mark timer as changed for persistence tracking
            _dirtyTrackingService.MarkTimerChanged();

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

            // Mark as changed since timer was reset
            _dirtyTrackingService.MarkTimerChanged();
        }

        /// <summary>
        /// Cleanup method to unregister from services
        /// </summary>
        public void Cleanup()
        {
            _timerCoordinationService.UnregisterTimer(this);
            _uiTimer?.Stop();
            _uiTimer?.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
