using FluentAssertions;
using Moq;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Tests.Services
{
    /// <summary>
    /// Unit tests for TimerCoordinationService
    /// </summary>
    public class TimerCoordinationServiceTests
    {
        [Fact]
        public void RegisterTimer_ShouldAddTimerToCollection()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var mockTimer = CreateMockTaskTimerViewModel();

            // Act
            service.RegisterTimer(mockTimer);

            // Assert - no exception, timer is registered
            // We verify by testing StopAllTimersExcept behavior
            service.StopAllTimersExcept(null);
            Mock.Get(mockTimer).Verify(t => t.StopTimer(), Times.Once);
        }

        [Fact]
        public void RegisterTimer_ShouldNotAddSameTimerTwice()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var mockTimer = CreateMockTaskTimerViewModel();

            // Act
            service.RegisterTimer(mockTimer);
            service.RegisterTimer(mockTimer); // Register same timer again

            // Assert - should only be registered once
            service.StopAllTimersExcept(null);
            Mock.Get(mockTimer).Verify(t => t.StopTimer(), Times.Once);
        }

        [Fact]
        public void UnregisterTimer_ShouldRemoveTimerFromCollection()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var mockTimer = CreateMockTaskTimerViewModel();
            service.RegisterTimer(mockTimer);

            // Act
            service.UnregisterTimer(mockTimer);

            // Assert - timer should not be stopped when calling StopAllTimersExcept
            service.StopAllTimersExcept(null);
            Mock.Get(mockTimer).Verify(t => t.StopTimer(), Times.Never);
        }

        [Fact]
        public void StopAllTimersExcept_ShouldStopAllTimersWhenExceptIsNull()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var timer1 = CreateMockTaskTimerViewModel(isRunning: true);
            var timer2 = CreateMockTaskTimerViewModel(isRunning: true);
            var timer3 = CreateMockTaskTimerViewModel(isRunning: true);

            service.RegisterTimer(timer1);
            service.RegisterTimer(timer2);
            service.RegisterTimer(timer3);

            // Act
            service.StopAllTimersExcept(null);

            // Assert
            Mock.Get(timer1).Verify(t => t.StopTimer(), Times.Once);
            Mock.Get(timer2).Verify(t => t.StopTimer(), Times.Once);
            Mock.Get(timer3).Verify(t => t.StopTimer(), Times.Once);
        }

        [Fact]
        public void StopAllTimersExcept_ShouldNotStopExceptedTimer()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var timer1 = CreateMockTaskTimerViewModel(isRunning: true);
            var timer2 = CreateMockTaskTimerViewModel(isRunning: true);
            var timer3 = CreateMockTaskTimerViewModel(isRunning: true);

            service.RegisterTimer(timer1);
            service.RegisterTimer(timer2);
            service.RegisterTimer(timer3);

            // Act - except timer2
            service.StopAllTimersExcept(timer2);

            // Assert
            Mock.Get(timer1).Verify(t => t.StopTimer(), Times.Once);
            Mock.Get(timer2).Verify(t => t.StopTimer(), Times.Never); // Should NOT be stopped
            Mock.Get(timer3).Verify(t => t.StopTimer(), Times.Once);
        }

        [Fact]
        public void StopAllTimersExcept_ShouldOnlyStopRunningTimers()
        {
            // Arrange
            var service = new TimerCoordinationService();
            var runningTimer = CreateMockTaskTimerViewModel(isRunning: true);
            var stoppedTimer = CreateMockTaskTimerViewModel(isRunning: false);

            service.RegisterTimer(runningTimer);
            service.RegisterTimer(stoppedTimer);

            // Act
            service.StopAllTimersExcept(null);

            // Assert
            Mock.Get(runningTimer).Verify(t => t.StopTimer(), Times.Once);
            Mock.Get(stoppedTimer).Verify(t => t.StopTimer(), Times.Never);
        }

        /// <summary>
        /// Helper method to create a mocked TaskTimerViewModel
        /// </summary>
        private TaskTimerViewModel CreateMockTaskTimerViewModel(bool isRunning = false)
        {
            var mock = new Mock<TaskTimerViewModel>(
                new TaskTimer(),
                Mock.Of<ITimerCoordinationService>(),
                Mock.Of<IDirtyTrackingService>());

            mock.Setup(t => t.IsRunning).Returns(isRunning);
            mock.Setup(t => t.StopTimer()).Verifiable();

            return mock.Object;
        }
    }
}
