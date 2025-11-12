using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using WorkTimeTracker.Entities;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.Services;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Tests.Services
{
    /// <summary>
    /// Unit tests for DataPersistenceService
    /// </summary>
    public class DataPersistenceServiceTests
    {
        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();

            // Act
            var act = () => new DataPersistenceService(null!, mockFactory.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("repository");
        }

        [Fact]
        public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();

            // Act
            var act = () => new DataPersistenceService(mockRepository.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("viewModelFactory");
        }

        [Fact]
        public async Task SaveApplicationStateAsync_ShouldCallRepositorySaveState()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            var tasks = new ObservableCollection<TaskTimerViewModel>
            {
                CreateMockTaskTimerViewModel("Task 1", 100),
                CreateMockTaskTimerViewModel("Task 2", 200)
            };

            // Act
            await service.SaveApplicationStateAsync(tasks);

            // Assert
            mockRepository.Verify(
                r => r.SaveStateAsync(It.Is<ApplicationStateEntity>(
                    state => state.Tasks.Count == 2 &&
                             state.Tasks[0].Description == "Task 1" &&
                             state.Tasks[1].Description == "Task 2")),
                Times.Once);
        }

        [Fact]
        public async Task LoadApplicationStateAsync_WhenNoSavedState_ShouldReturnNull()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            mockRepository.Setup(r => r.LoadStateAsync())
                .ReturnsAsync((ApplicationStateEntity?)null);

            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            // Act
            var result = await service.LoadApplicationStateAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoadApplicationStateAsync_WhenEmptyTasks_ShouldReturnNull()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            mockRepository.Setup(r => r.LoadStateAsync())
                .ReturnsAsync(new ApplicationStateEntity { Tasks = new List<TaskTimerEntity>() });

            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            // Act
            var result = await service.LoadApplicationStateAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoadApplicationStateAsync_WithSavedTasks_ShouldReturnViewModels()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            var savedState = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>
                {
                    new TaskTimerEntity { Description = "Task 1", ElapsedSeconds = 100 },
                    new TaskTimerEntity { Description = "Task 2", ElapsedSeconds = 200 }
                }
            };
            mockRepository.Setup(r => r.LoadStateAsync()).ReturnsAsync(savedState);

            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns((TaskTimer task) =>
                {
                    var mockVm = CreateMockTaskTimerViewModel(task.Description, task.Elapsed.TotalSeconds);
                    return mockVm;
                });

            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            // Act
            var result = await service.LoadApplicationStateAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Count.Should().Be(2);
            result[0].Description.Should().Be("Task 1");
            result[1].Description.Should().Be("Task 2");

            // Verify factory was called for each task
            mockFactory.Verify(f => f.Create(It.IsAny<TaskTimer>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExportToTextFileAsync_ShouldCallRepositoryExportToText()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            var tasks = new ObservableCollection<TaskTimerViewModel>
            {
                CreateMockTaskTimerViewModel("Task 1", 100)
            };

            // Act
            await service.ExportToTextFileAsync(tasks, "test.txt");

            // Assert
            mockRepository.Verify(
                r => r.ExportToTextAsync(
                    It.IsAny<IEnumerable<TaskTimerEntity>>(),
                    "test.txt"),
                Times.Once);
        }

        [Fact]
        public async Task ExportToTextFileAsync_WithNullFileName_ShouldGenerateFileName()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            var tasks = new ObservableCollection<TaskTimerViewModel>();

            // Act
            await service.ExportToTextFileAsync(tasks, null);

            // Assert
            mockRepository.Verify(
                r => r.ExportToTextAsync(
                    It.IsAny<IEnumerable<TaskTimerEntity>>(),
                    It.Is<string>(s => s.StartsWith("time_tracking_"))),
                Times.Once);
        }

        [Fact]
        public void CanRestoreState_ShouldReturnRepositoryHasSavedState()
        {
            // Arrange
            var mockRepository = new Mock<ITaskRepository>();
            mockRepository.Setup(r => r.HasSavedState()).Returns(true);

            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var service = new DataPersistenceService(mockRepository.Object, mockFactory.Object);

            // Act
            var result = service.CanRestoreState();

            // Assert
            result.Should().BeTrue();
            mockRepository.Verify(r => r.HasSavedState(), Times.Once);
        }

        /// <summary>
        /// Helper method to create a mocked TaskTimerViewModel
        /// </summary>
        private TaskTimerViewModel CreateMockTaskTimerViewModel(string description, double elapsedSeconds)
        {
            var mockVm = new Mock<TaskTimerViewModel>(
                new TaskTimer { Description = description, Elapsed = TimeSpan.FromSeconds(elapsedSeconds) },
                Mock.Of<ITimerCoordinationService>(),
                Mock.Of<IDirtyTrackingService>());

            mockVm.Setup(vm => vm.Description).Returns(description);
            mockVm.Setup(vm => vm.TaskElapsedSeconds).Returns(elapsedSeconds);
            mockVm.Setup(vm => vm.IsRunning).Returns(false);

            return mockVm.Object;
        }
    }
}
