using FluentAssertions;
using WorkTimeTracker.Services;

namespace WorkTimeTracker.Tests.Services
{
    /// <summary>
    /// Unit tests for NotificationService
    /// </summary>
    public class NotificationServiceTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithNullMessages()
        {
            // Arrange & Act
            var service = new NotificationService();

            // Assert
            service.CurrentNotificationMessage.Should().BeNull();
            service.CurrentConfirmationMessage.Should().BeNull();
        }

        [Fact]
        public void ShowNotification_ShouldSetCurrentNotificationMessage()
        {
            // Arrange
            var service = new NotificationService();
            const string message = "Test notification";

            // Act
            service.ShowNotification(message);

            // Assert
            service.CurrentNotificationMessage.Should().Be(message);
        }

        [Fact]
        public void ShowNotification_ShouldRaiseNotificationVisibilityChangedEvent()
        {
            // Arrange
            var service = new NotificationService();
            var eventRaised = false;
            var eventValue = false;

            service.NotificationVisibilityChanged += (sender, visible) =>
            {
                eventRaised = true;
                eventValue = visible;
            };

            // Act
            service.ShowNotification("Test");

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeTrue();
        }

        [Fact]
        public async Task ShowNotification_ShouldAutoHideAfterDelay()
        {
            // Arrange
            var service = new NotificationService();
            var hiddenEventRaised = false;

            service.NotificationVisibilityChanged += (sender, visible) =>
            {
                if (!visible)
                    hiddenEventRaised = true;
            };

            // Act
            service.ShowNotification("Test");
            await Task.Delay(3500); // Wait for auto-hide (3 seconds + buffer)

            // Assert
            hiddenEventRaised.Should().BeTrue();
            service.CurrentNotificationMessage.Should().BeNull();
        }

        [Fact]
        public void ShowConfirmation_ShouldSetCurrentConfirmationMessage()
        {
            // Arrange
            var service = new NotificationService();
            const string message = "Confirm action?";

            // Act
            service.ShowConfirmation(message, () => { });

            // Assert
            service.CurrentConfirmationMessage.Should().Be(message);
        }

        [Fact]
        public void ShowConfirmation_ShouldRaiseConfirmationVisibilityChangedEvent()
        {
            // Arrange
            var service = new NotificationService();
            var eventRaised = false;
            var eventValue = false;

            service.ConfirmationVisibilityChanged += (sender, visible) =>
            {
                eventRaised = true;
                eventValue = visible;
            };

            // Act
            service.ShowConfirmation("Test", () => { });

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeTrue();
        }

        [Fact]
        public void Confirm_ShouldExecuteConfirmAction()
        {
            // Arrange
            var service = new NotificationService();
            var actionExecuted = false;
            service.ShowConfirmation("Test", () => actionExecuted = true);

            // Act
            service.Confirm();

            // Assert
            actionExecuted.Should().BeTrue();
        }

        [Fact]
        public void Confirm_ShouldClearConfirmation()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { });

            // Act
            service.Confirm();

            // Assert
            service.CurrentConfirmationMessage.Should().BeNull();
        }

        [Fact]
        public void Confirm_ShouldRaiseConfirmationVisibilityChangedEventWithFalse()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { });

            var eventRaised = false;
            var eventValue = true;

            service.ConfirmationVisibilityChanged += (sender, visible) =>
            {
                eventRaised = true;
                eventValue = visible;
            };

            // Act
            service.Confirm();

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeFalse();
        }

        [Fact]
        public void Cancel_ShouldExecuteCancelAction()
        {
            // Arrange
            var service = new NotificationService();
            var cancelExecuted = false;
            service.ShowConfirmation("Test", () => { }, () => cancelExecuted = true);

            // Act
            service.Cancel();

            // Assert
            cancelExecuted.Should().BeTrue();
        }

        [Fact]
        public void Cancel_ShouldClearConfirmation()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { });

            // Act
            service.Cancel();

            // Assert
            service.CurrentConfirmationMessage.Should().BeNull();
        }

        [Fact]
        public void ClearConfirmation_ShouldClearMessageAndActions()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { });

            // Act
            service.ClearConfirmation();

            // Assert
            service.CurrentConfirmationMessage.Should().BeNull();
        }

        [Fact]
        public void ClearConfirmation_ShouldRaiseConfirmationVisibilityChangedEventWithFalse()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { });

            var eventRaised = false;
            var eventValue = true;

            service.ConfirmationVisibilityChanged += (sender, visible) =>
            {
                eventRaised = true;
                eventValue = visible;
            };

            // Act
            service.ClearConfirmation();

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeFalse();
        }

        [Fact]
        public void Cancel_WithNoCancelAction_ShouldNotThrow()
        {
            // Arrange
            var service = new NotificationService();
            service.ShowConfirmation("Test", () => { }, null);

            // Act
            var act = () => service.Cancel();

            // Assert
            act.Should().NotThrow();
        }
    }
}
