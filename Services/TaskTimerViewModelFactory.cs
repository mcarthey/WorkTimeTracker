using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Services
{
    /// <summary>
    /// Factory for creating TaskTimerViewModel instances with dependency injection.
    /// Follows Factory Pattern and enables proper service injection into ViewModels.
    /// </summary>
    public class TaskTimerViewModelFactory : ITaskTimerViewModelFactory
    {
        private readonly ITimerCoordinationService _timerCoordinationService;
        private readonly IDirtyTrackingService _dirtyTrackingService;

        public TaskTimerViewModelFactory(
            ITimerCoordinationService timerCoordinationService,
            IDirtyTrackingService dirtyTrackingService)
        {
            _timerCoordinationService = timerCoordinationService;
            _dirtyTrackingService = dirtyTrackingService;
        }

        /// <inheritdoc/>
        public TaskTimerViewModel Create(TaskTimer task)
        {
            return new TaskTimerViewModel(
                task,
                _timerCoordinationService,
                _dirtyTrackingService);
        }
    }
}
