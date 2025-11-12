using FluentAssertions;
using WorkTimeTracker.Services;

namespace WorkTimeTracker.Tests.Services
{
    /// <summary>
    /// Unit tests for DirtyTrackingService
    /// </summary>
    public class DirtyTrackingServiceTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithNoUnsavedChanges()
        {
            // Arrange & Act
            var service = new DirtyTrackingService();

            // Assert
            service.HasUnsavedChanges.Should().BeFalse();
        }

        [Fact]
        public void MarkTaskCreated_ShouldSetHasUnsavedChangesTrue()
        {
            // Arrange
            var service = new DirtyTrackingService();

            // Act
            service.MarkTaskCreated();

            // Assert
            service.HasUnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void MarkTaskUpdated_ShouldSetHasUnsavedChangesTrue()
        {
            // Arrange
            var service = new DirtyTrackingService();

            // Act
            service.MarkTaskUpdated();

            // Assert
            service.HasUnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void MarkTaskDeleted_ShouldSetHasUnsavedChangesTrue()
        {
            // Arrange
            var service = new DirtyTrackingService();

            // Act
            service.MarkTaskDeleted();

            // Assert
            service.HasUnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void MarkTimerChanged_ShouldSetHasUnsavedChangesTrue()
        {
            // Arrange
            var service = new DirtyTrackingService();

            // Act
            service.MarkTimerChanged();

            // Assert
            service.HasUnsavedChanges.Should().BeTrue();
        }

        [Fact]
        public void MarkAsSaved_ShouldSetHasUnsavedChangesFalse()
        {
            // Arrange
            var service = new DirtyTrackingService();
            service.MarkTaskCreated(); // Make it dirty first

            // Act
            service.MarkAsSaved();

            // Assert
            service.HasUnsavedChanges.Should().BeFalse();
        }

        [Fact]
        public void DirtyStateChanged_ShouldRaiseEventWhenStateChanges()
        {
            // Arrange
            var service = new DirtyTrackingService();
            var eventRaised = false;
            var eventValue = false;

            service.DirtyStateChanged += (sender, hasUnsaved) =>
            {
                eventRaised = true;
                eventValue = hasUnsaved;
            };

            // Act
            service.MarkTaskCreated();

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeTrue();
        }

        [Fact]
        public void DirtyStateChanged_ShouldRaiseEventWithFalseWhenMarkedAsSaved()
        {
            // Arrange
            var service = new DirtyTrackingService();
            service.MarkTaskCreated(); // Make it dirty first

            var eventRaised = false;
            var eventValue = true;

            service.DirtyStateChanged += (sender, hasUnsaved) =>
            {
                eventRaised = true;
                eventValue = hasUnsaved;
            };

            // Act
            service.MarkAsSaved();

            // Assert
            eventRaised.Should().BeTrue();
            eventValue.Should().BeFalse();
        }

        [Fact]
        public void DirtyStateChanged_ShouldNotRaiseEventWhenStateDoesNotChange()
        {
            // Arrange
            var service = new DirtyTrackingService();
            service.MarkTaskCreated(); // Make it dirty

            var eventCount = 0;
            service.DirtyStateChanged += (sender, hasUnsaved) => eventCount++;

            // Act - mark it dirty again (no state change)
            service.MarkTaskCreated();

            // Assert - event should not fire again since state didn't change
            eventCount.Should().Be(0);
        }
    }
}
