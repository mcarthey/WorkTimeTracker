using System;
using WorkTimeTracker.Interfaces;

namespace WorkTimeTracker.Services
{
    /// <summary>
    /// Tracks meaningful state changes in the application.
    /// Only tracks COMMITTED changes (not every keystroke), enabling efficient persistence.
    /// Follows Single Responsibility Principle.
    /// </summary>
    public class DirtyTrackingService : IDirtyTrackingService
    {
        private bool _hasUnsavedChanges;

        /// <inheritdoc/>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    DirtyStateChanged?.Invoke(this, value);
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<bool>? DirtyStateChanged;

        /// <inheritdoc/>
        public void MarkTaskCreated()
        {
            HasUnsavedChanges = true;
        }

        /// <inheritdoc/>
        public void MarkTaskUpdated()
        {
            HasUnsavedChanges = true;
        }

        /// <inheritdoc/>
        public void MarkTaskDeleted()
        {
            HasUnsavedChanges = true;
        }

        /// <inheritdoc/>
        public void MarkTimerChanged()
        {
            HasUnsavedChanges = true;
        }

        /// <inheritdoc/>
        public void MarkAsSaved()
        {
            HasUnsavedChanges = false;
        }
    }
}
