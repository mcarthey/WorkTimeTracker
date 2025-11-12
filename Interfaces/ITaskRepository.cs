using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTimeTracker.Entities;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Repository interface for task data access.
    /// Follows Repository Pattern and Dependency Inversion Principle.
    /// Storage mechanism is abstracted - can be JSON, XML, database, etc.
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// Saves the application state
        /// </summary>
        /// <param name="state">The application state to save</param>
        Task SaveStateAsync(ApplicationStateEntity state);

        /// <summary>
        /// Loads the application state
        /// </summary>
        /// <returns>The loaded application state, or null if no saved state exists</returns>
        Task<ApplicationStateEntity?> LoadStateAsync();

        /// <summary>
        /// Exports tasks to a human-readable format (for user export feature)
        /// </summary>
        /// <param name="tasks">The tasks to export</param>
        /// <param name="fileName">The file name to export to</param>
        Task ExportToTextAsync(IEnumerable<TaskTimerEntity> tasks, string fileName);

        /// <summary>
        /// Checks if a saved state exists
        /// </summary>
        bool HasSavedState();
    }
}
