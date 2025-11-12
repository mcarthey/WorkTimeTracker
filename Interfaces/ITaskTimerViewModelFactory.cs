using WorkTimeTracker.Models;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Factory for creating TaskTimerViewModel instances with proper dependency injection.
    /// Follows Factory Pattern and enables creation of ViewModels with injected dependencies.
    /// </summary>
    public interface ITaskTimerViewModelFactory
    {
        /// <summary>
        /// Creates a new TaskTimerViewModel with the given task model
        /// </summary>
        /// <param name="task">The task model to wrap</param>
        /// <returns>A new TaskTimerViewModel instance</returns>
        TaskTimerViewModel Create(TaskTimer task);
    }
}
