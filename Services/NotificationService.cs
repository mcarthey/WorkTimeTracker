using System;
using System.Threading.Tasks;
using WorkTimeTracker.Interfaces;

namespace WorkTimeTracker.Services
{
    /// <summary>
    /// Service responsible for managing user notifications and confirmations.
    /// Decouples notification logic from ViewModels following Dependency Inversion Principle.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private string? _currentNotificationMessage;
        private string? _currentConfirmationMessage;
        private Action? _pendingConfirmAction;
        private Action? _pendingCancelAction;

        /// <inheritdoc/>
        public event EventHandler<bool>? NotificationVisibilityChanged;

        /// <inheritdoc/>
        public event EventHandler<bool>? ConfirmationVisibilityChanged;

        /// <inheritdoc/>
        public string? CurrentNotificationMessage => _currentNotificationMessage;

        /// <inheritdoc/>
        public string? CurrentConfirmationMessage => _currentConfirmationMessage;

        /// <inheritdoc/>
        public void ShowNotification(string message)
        {
            _currentNotificationMessage = message;
            NotificationVisibilityChanged?.Invoke(this, true);

            // Auto-hide after 3 seconds
            Task.Delay(3000).ContinueWith(_ =>
            {
                _currentNotificationMessage = null;
                NotificationVisibilityChanged?.Invoke(this, false);
            });
        }

        /// <inheritdoc/>
        public void ShowConfirmation(string message, Action onConfirm, Action? onCancel = null)
        {
            _currentConfirmationMessage = message;
            _pendingConfirmAction = onConfirm;
            _pendingCancelAction = onCancel;
            ConfirmationVisibilityChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Confirms the pending confirmation dialog
        /// </summary>
        public void Confirm()
        {
            _pendingConfirmAction?.Invoke();
            ClearConfirmation();
        }

        /// <summary>
        /// Cancels the pending confirmation dialog
        /// </summary>
        public void Cancel()
        {
            _pendingCancelAction?.Invoke();
            ClearConfirmation();
        }

        /// <inheritdoc/>
        public void ClearConfirmation()
        {
            _currentConfirmationMessage = null;
            _pendingConfirmAction = null;
            _pendingCancelAction = null;
            ConfirmationVisibilityChanged?.Invoke(this, false);
        }
    }
}
