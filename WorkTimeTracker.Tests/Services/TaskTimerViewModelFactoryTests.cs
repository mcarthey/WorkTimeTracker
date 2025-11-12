using FluentAssertions;
using Moq;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;

namespace WorkTimeTracker.Tests.Services
{
    /// <summary>
    /// Unit tests for TaskTimerViewModelFactory
    /// </summary>
    public class TaskTimerViewModelFactoryTests
    {
        [Fact]
        public void Constructor_WithNullTimerCoordinationService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();

            // Act
            var act = () => new TaskTimerViewModelFactory(null!, mockDirtyTracking.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("timerCoordinationService");
        }

        [Fact]
        public void Constructor_WithNullDirtyTrackingService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();

            // Act
            var act = () => new TaskTimerViewModelFactory(mockTimerCoordination.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("dirtyTrackingService");
        }

        [Fact]
        public void Create_WithValidTask_ShouldReturnTaskTimerViewModel()
        {
            // Arrange
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var factory = new TaskTimerViewModelFactory(
                mockTimerCoordination.Object,
                mockDirtyTracking.Object);

            var task = new TaskTimer { Description = "Test Task" };

            // Act
            var result = factory.Create(task);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Be("Test Task");
        }

        [Fact]
        public void Create_ShouldRegisterViewModelWithTimerCoordinationService()
        {
            // Arrange
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var factory = new TaskTimerViewModelFactory(
                mockTimerCoordination.Object,
                mockDirtyTracking.Object);

            var task = new TaskTimer();

            // Act
            var result = factory.Create(task);

            // Assert - verify that RegisterTimer was called with the created ViewModel
            mockTimerCoordination.Verify(
                s => s.RegisterTimer(It.IsAny<ViewModels.TaskTimerViewModel>()),
                Times.Once);
        }

        [Fact]
        public void Create_WithMultipleCalls_ShouldReturnDifferentInstances()
        {
            // Arrange
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var factory = new TaskTimerViewModelFactory(
                mockTimerCoordination.Object,
                mockDirtyTracking.Object);

            var task1 = new TaskTimer { Description = "Task 1" };
            var task2 = new TaskTimer { Description = "Task 2" };

            // Act
            var result1 = factory.Create(task1);
            var result2 = factory.Create(task2);

            // Assert
            result1.Should().NotBeSameAs(result2);
            result1.Description.Should().Be("Task 1");
            result2.Description.Should().Be("Task 2");
        }
    }
}
