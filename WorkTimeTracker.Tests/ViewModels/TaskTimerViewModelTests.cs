using FluentAssertions;
using Moq;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for TaskTimerViewModel
    /// </summary>
    public class TaskTimerViewModelTests
    {
        [Fact]
        public void Constructor_WithNullTask_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();

            // Act
            var act = () => new TaskTimerViewModel(null!, mockTimerCoordination.Object, mockDirtyTracking.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("task");
        }

        [Fact]
        public void Constructor_WithNullTimerCoordinationService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();

            // Act
            var act = () => new TaskTimerViewModel(task, null!, mockDirtyTracking.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("timerCoordinationService");
        }

        [Fact]
        public void Constructor_WithNullDirtyTrackingService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var task = new TaskTimer();
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();

            // Act
            var act = () => new TaskTimerViewModel(task, mockTimerCoordination.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("dirtyTrackingService");
        }

        [Fact]
        public void Constructor_ShouldRegisterWithTimerCoordinationService()
        {
            // Arrange
            var task = new TaskTimer();
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();

            // Act
            var viewModel = new TaskTimerViewModel(task, mockTimerCoordination.Object, mockDirtyTracking.Object);

            // Assert
            mockTimerCoordination.Verify(s => s.RegisterTimer(viewModel), Times.Once);
        }

        [Fact]
        public void Description_Set_ShouldUpdateTask()
        {
            // Arrange
            var task = new TaskTimer();
            var viewModel = CreateViewModel(task);

            // Act
            viewModel.Description = "New Description";

            // Assert
            viewModel.Description.Should().Be("New Description");
            task.Description.Should().Be("New Description");
        }

        [Fact]
        public void Description_Set_ShouldNotMarkAsDirtyUntilCommit()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.Description = "New Description";

            // Assert - dirty tracking should NOT be called yet
            mockDirtyTracking.Verify(s => s.MarkTaskUpdated(), Times.Never);
        }

        [Fact]
        public void CommitDescription_WhenDescriptionChanged_ShouldMarkAsDirty()
        {
            // Arrange
            var task = new TaskTimer { Description = "Original" };
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            viewModel.Description = "Modified";

            // Act
            viewModel.CommitDescription();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTaskUpdated(), Times.Once);
        }

        [Fact]
        public void CommitDescription_WhenDescriptionUnchanged_ShouldNotMarkAsDirty()
        {
            // Arrange
            var task = new TaskTimer { Description = "Same" };
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act - commit without changing
            viewModel.CommitDescription();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTaskUpdated(), Times.Never);
        }

        [Fact]
        public void IsRunning_Initially_ShouldBeFalse()
        {
            // Arrange
            var task = new TaskTimer();
            var viewModel = CreateViewModel(task);

            // Assert
            viewModel.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void StartCommand_Execute_ShouldCallTimerCoordinationService()
        {
            // Arrange
            var task = new TaskTimer();
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var viewModel = CreateViewModel(task, timerCoordinationService: mockTimerCoordination.Object);

            // Act
            viewModel.StartCommand.Execute(null);

            // Assert
            mockTimerCoordination.Verify(s => s.StopAllTimersExcept(viewModel), Times.Once);
        }

        [Fact]
        public void StartCommand_Execute_ShouldMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.StartCommand.Execute(null);

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Once);
        }

        [Fact]
        public void StopTimer_WhenRunning_ShouldMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            viewModel.StartCommand.Execute(null); // Start first
            mockDirtyTracking.Reset(); // Reset to verify only the stop call

            // Act
            viewModel.StopTimer();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Once);
        }

        [Fact]
        public void StopTimer_WhenNotRunning_ShouldNotMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act - stop when not running
            viewModel.StopTimer();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Never);
        }

        [Fact]
        public void Add15MinCommand_Execute_ShouldIncreaseElapsedTime()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromMinutes(30) };
            var viewModel = CreateViewModel(task);

            // Act
            viewModel.Add15MinCommand.Execute(null);

            // Assert
            viewModel.TaskElapsedSeconds.Should().Be(2700); // 45 minutes
        }

        [Fact]
        public void Add15MinCommand_Execute_ShouldMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer();
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.Add15MinCommand.Execute(null);

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Once);
        }

        [Fact]
        public void Subtract15MinCommand_Execute_ShouldDecreaseElapsedTime()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromMinutes(30) };
            var viewModel = CreateViewModel(task);

            // Act
            viewModel.Subtract15MinCommand.Execute(null);

            // Assert
            viewModel.TaskElapsedSeconds.Should().Be(900); // 15 minutes
        }

        [Fact]
        public void Subtract15MinCommand_Execute_WhenResultWouldBeNegative_ShouldSetToZero()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromMinutes(5) };
            var viewModel = CreateViewModel(task);

            // Act
            viewModel.Subtract15MinCommand.Execute(null);

            // Assert
            viewModel.TaskElapsedSeconds.Should().Be(0);
        }

        [Fact]
        public void Subtract15MinCommand_Execute_ShouldMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromMinutes(30) };
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.Subtract15MinCommand.Execute(null);

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Once);
        }

        [Fact]
        public void Reset_ShouldResetElapsedTimeToZero()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromHours(2) };
            var viewModel = CreateViewModel(task);

            // Act
            viewModel.Reset();

            // Assert
            viewModel.TaskElapsedSeconds.Should().Be(0);
            viewModel.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void Reset_ShouldMarkTimerAsChanged()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromHours(2) };
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(task, dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.Reset();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTimerChanged(), Times.Once);
        }

        [Fact]
        public void Cleanup_ShouldUnregisterFromTimerCoordinationService()
        {
            // Arrange
            var task = new TaskTimer();
            var mockTimerCoordination = new Mock<ITimerCoordinationService>();
            var viewModel = CreateViewModel(task, timerCoordinationService: mockTimerCoordination.Object);

            // Act
            viewModel.Cleanup();

            // Assert
            mockTimerCoordination.Verify(s => s.UnregisterTimer(viewModel), Times.Once);
        }

        [Fact]
        public void ElapsedFormatted_ShouldReturnFormattedTimeString()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(45))) };
            var viewModel = CreateViewModel(task);

            // Act
            var formatted = viewModel.ElapsedFormatted;

            // Assert
            formatted.Should().Be("02:30:45");
        }

        [Fact]
        public void TaskElapsedSeconds_ShouldReturnTotalSeconds()
        {
            // Arrange
            var task = new TaskTimer { Elapsed = TimeSpan.FromMinutes(5) };
            var viewModel = CreateViewModel(task);

            // Act
            var seconds = viewModel.TaskElapsedSeconds;

            // Assert
            seconds.Should().Be(300);
        }

        /// <summary>
        /// Helper method to create a TaskTimerViewModel with optional mocked dependencies
        /// </summary>
        private TaskTimerViewModel CreateViewModel(
            TaskTimer task,
            ITimerCoordinationService? timerCoordinationService = null,
            IDirtyTrackingService? dirtyTrackingService = null)
        {
            return new TaskTimerViewModel(
                task,
                timerCoordinationService ?? Mock.Of<ITimerCoordinationService>(),
                dirtyTrackingService ?? Mock.Of<IDirtyTrackingService>());
        }
    }
}
