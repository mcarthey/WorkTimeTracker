using System.Collections.Generic;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Services
{
    /// <summary>
    /// Service responsible for coordinating timer operations across multiple task timers.
    /// Ensures only one timer runs at a time by managing timer state.
    /// Follows Single Responsibility Principle and removes tight coupling from ViewModels.
    /// </summary>
    public class TimerCoordinationService : ITimerCoordinationService
    {
        private readonly List<TaskTimerViewModel> _registeredTimers = new();

        /// <inheritdoc/>
        public void StopAllTimersExcept(TaskTimerViewModel? exceptViewModel)
        {
            foreach (var timer in _registeredTimers)
            {
                if (timer != exceptViewModel && timer.IsRunning)
                {
                    timer.StopTimer();
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterTimer(TaskTimerViewModel viewModel)
        {
            if (!_registeredTimers.Contains(viewModel))
            {
                _registeredTimers.Add(viewModel);
            }
        }

        /// <inheritdoc/>
        public void UnregisterTimer(TaskTimerViewModel viewModel)
        {
            _registeredTimers.Remove(viewModel);
        }
    }
}
