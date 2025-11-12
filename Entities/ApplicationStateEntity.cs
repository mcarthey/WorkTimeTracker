using System;
using System.Collections.Generic;

namespace WorkTimeTracker.Entities
{
    /// <summary>
    /// Represents the complete application state for persistence.
    /// This entity encapsulates all data needed to restore the application to its previous state.
    /// </summary>
    public class ApplicationStateEntity
    {
        /// <summary>
        /// All task timers in the application
        /// </summary>
        public List<TaskTimerEntity> Tasks { get; set; } = new();

        /// <summary>
        /// When this state was saved
        /// </summary>
        public DateTime SavedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Version for future compatibility
        /// </summary>
        public string Version { get; set; } = "2.0.0";
    }
}
