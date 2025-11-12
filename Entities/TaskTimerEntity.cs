using System;

namespace WorkTimeTracker.Entities
{
    /// <summary>
    /// Plain data object for persisting TaskTimer state.
    /// This entity is storage-agnostic and can be serialized to JSON, XML, or database.
    /// </summary>
    public class TaskTimerEntity
    {
        /// <summary>
        /// Task description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Total elapsed time in seconds
        /// </summary>
        public double ElapsedSeconds { get; set; }

        /// <summary>
        /// Whether the timer was running when saved
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Start time if timer was running (for restoration)
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Timestamp when this entity was created/updated
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
