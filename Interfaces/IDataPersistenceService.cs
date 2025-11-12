using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Service responsible for orchestrating data persistence operations.
    /// Acts as a facade over the repository layer, coordinating save/load operations
    /// with ViewModels and entity mappings.
    /// </summary>
    public interface IDataPersistenceService
    {
        /// <summary>
        /// Saves the current application state
        /// </summary>
        /// <param name="tasks">The collection of task ViewModels to save</param>
        Task SaveApplicationStateAsync(ObservableCollection<TaskTimerViewModel> tasks);

        /// <summary>
        /// Loads the application state and returns task ViewModels
        /// </summary>
        /// <returns>Collection of restored task ViewModels</returns>
        Task<ObservableCollection<TaskTimerViewModel>?> LoadApplicationStateAsync();

        /// <summary>
        /// Exports tasks to a user-readable text file (existing export feature)
        /// </summary>
        /// <param name="tasks">The tasks to export</param>
        /// <param name="fileName">Optional file name (auto-generated if not provided)</param>
        Task ExportToTextFileAsync(ObservableCollection<TaskTimerViewModel> tasks, string? fileName = null);

        /// <summary>
        /// Checks if there is a saved state to restore
        /// </summary>
        bool CanRestoreState();
    }
}
