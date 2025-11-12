using System;
using System.Threading.Tasks;

namespace WorkTimeTracker.Interfaces
{
    /// <summary>
    /// Service responsible for displaying notifications to the user.
    /// Decouples notification logic from ViewModels following Dependency Inversion Principle.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Shows an informational notification to the user
        /// </summary>
        /// <param name="message">The message to display</param>
        void ShowNotification(string message);

        /// <summary>
        /// Shows a confirmation dialog to the user
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="onConfirm">Action to execute if user confirms</param>
        /// <param name="onCancel">Action to execute if user cancels</param>
        void ShowConfirmation(string message, Action onConfirm, Action? onCancel = null);

        /// <summary>
        /// Clears any active confirmation dialog
        /// </summary>
        void ClearConfirmation();

        /// <summary>
        /// Event raised when notification visibility changes
        /// </summary>
        event EventHandler<bool>? NotificationVisibilityChanged;

        /// <summary>
        /// Event raised when confirmation visibility changes
        /// </summary>
        event EventHandler<bool>? ConfirmationVisibilityChanged;

        /// <summary>
        /// Gets the current notification message
        /// </summary>
        string? CurrentNotificationMessage { get; }

        /// <summary>
        /// Gets the current confirmation message
        /// </summary>
        string? CurrentConfirmationMessage { get; }
    }
}
