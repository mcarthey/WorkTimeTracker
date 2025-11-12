using System;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Service responsible for tracking meaningful state changes in the application.
    /// Distinguishes between "in-progress" changes (like typing) and "committed" changes
    /// that should trigger persistence operations.
    /// </summary>
    public interface IDirtyTrackingService
    {
        /// <summary>
        /// Gets whether there are unsaved changes in the application
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// Marks that a task was created (committed change)
        /// </summary>
        void MarkTaskCreated();

        /// <summary>
        /// Marks that a task was updated (committed change, not every keystroke)
        /// </summary>
        void MarkTaskUpdated();

        /// <summary>
        /// Marks that a task was deleted (committed change)
        /// </summary>
        void MarkTaskDeleted();

        /// <summary>
        /// Marks that timer state changed (started/stopped/reset)
        /// </summary>
        void MarkTimerChanged();

        /// <summary>
        /// Marks all changes as saved
        /// </summary>
        void MarkAsSaved();

        /// <summary>
        /// Event raised when dirty state changes
        /// </summary>
        event EventHandler<bool>? DirtyStateChanged;
    }
}
