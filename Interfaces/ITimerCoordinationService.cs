using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Service responsible for coordinating timer operations across multiple tasks.
    /// Implements Single Responsibility Principle by isolating timer coordination logic.
    /// </summary>
    public interface ITimerCoordinationService
    {
        /// <summary>
        /// Stops all timers except the specified one.
        /// This ensures only one timer can run at a time.
        /// </summary>
        /// <param name="exceptViewModel">The ViewModel to exclude from stopping</param>
        void StopAllTimersExcept(TaskTimerViewModel? exceptViewModel);

        /// <summary>
        /// Registers a task timer ViewModel for coordination
        /// </summary>
        /// <param name="viewModel">The ViewModel to register</param>
        void RegisterTimer(TaskTimerViewModel viewModel);

        /// <summary>
        /// Unregisters a task timer ViewModel from coordination
        /// </summary>
        /// <param name="viewModel">The ViewModel to unregister</param>
        void UnregisterTimer(TaskTimerViewModel viewModel);
    }
}
