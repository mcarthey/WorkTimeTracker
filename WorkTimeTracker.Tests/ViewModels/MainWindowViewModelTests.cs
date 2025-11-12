using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using WorkTimeTracker.Entities;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for MainWindowViewModel
    /// </summary>
    public class MainWindowViewModelTests
    {
        [Fact]
        public void Constructor_WithNullDataPersistenceService_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var act = () => new MainWindowViewModel(
                null!,
                Mock.Of<INotificationService>(),
                Mock.Of<IDirtyTrackingService>(),
                Mock.Of<ITaskTimerViewModelFactory>());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("dataPersistenceService");
        }

        [Fact]
        public void Constructor_WithNullNotificationService_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var act = () => new MainWindowViewModel(
                Mock.Of<IDataPersistenceService>(),
                null!,
                Mock.Of<IDirtyTrackingService>(),
                Mock.Of<ITaskTimerViewModelFactory>());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("notificationService");
        }

        [Fact]
        public void Constructor_WithNullDirtyTrackingService_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var act = () => new MainWindowViewModel(
                Mock.Of<IDataPersistenceService>(),
                Mock.Of<INotificationService>(),
                null!,
                Mock.Of<ITaskTimerViewModelFactory>());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("dirtyTrackingService");
        }

        [Fact]
        public void Constructor_WithNullViewModelFactory_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var act = () => new MainWindowViewModel(
                Mock.Of<IDataPersistenceService>(),
                Mock.Of<INotificationService>(),
                Mock.Of<IDirtyTrackingService>(),
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("viewModelFactory");
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptyTasksCollection()
        {
            // Arrange & Act
            var viewModel = CreateViewModel();

            // Assert
            viewModel.Tasks.Should().NotBeNull();
            viewModel.Tasks.Should().BeEmpty();
        }

        [Fact]
        public void AddTaskCommand_Execute_ShouldAddNewTaskToCollection()
        {
            // Arrange
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns((TaskTimer task) => CreateMockTaskTimerViewModel(task));

            var viewModel = CreateViewModel(viewModelFactory: mockFactory.Object);

            // Act
            viewModel.AddTaskCommand.Execute(null);

            // Assert
            viewModel.Tasks.Should().HaveCount(1);
        }

        [Fact]
        public void AddTaskCommand_Execute_ShouldMarkTaskAsCreated()
        {
            // Arrange
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns((TaskTimer task) => CreateMockTaskTimerViewModel(task));

            var viewModel = CreateViewModel(
                dirtyTrackingService: mockDirtyTracking.Object,
                viewModelFactory: mockFactory.Object);

            // Act
            viewModel.AddTaskCommand.Execute(null);

            // Assert
            mockDirtyTracking.Verify(s => s.MarkTaskCreated(), Times.Once);
        }

        [Fact]
        public void AddTaskCommand_Execute_ShouldShowNotification()
        {
            // Arrange
            var mockNotification = new Mock<INotificationService>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns((TaskTimer task) => CreateMockTaskTimerViewModel(task));

            var viewModel = CreateViewModel(
                notificationService: mockNotification.Object,
                viewModelFactory: mockFactory.Object);

            // Act
            viewModel.AddTaskCommand.Execute(null);

            // Assert
            mockNotification.Verify(s => s.ShowNotification("Task added."), Times.Once);
        }

        [Fact]
        public void RemoveTaskCommand_Execute_ShouldShowConfirmation()
        {
            // Arrange
            var mockNotification = new Mock<INotificationService>();
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns((TaskTimer task) => CreateMockTaskTimerViewModel(task));

            var viewModel = CreateViewModel(
                notificationService: mockNotification.Object,
                viewModelFactory: mockFactory.Object);

            viewModel.AddTaskCommand.Execute(null);
            var taskToRemove = viewModel.Tasks[0];

            // Act
            viewModel.RemoveTaskCommand.Execute(taskToRemove);

            // Assert
            mockNotification.Verify(
                s => s.ShowConfirmation(
                    It.IsAny<string>(),
                    It.IsAny<Action>(),
                    It.IsAny<Action>()),
                Times.Once);
        }

        [Fact]
        public void ClearTimersCommand_Execute_ShouldShowConfirmation()
        {
            // Arrange
            var mockNotification = new Mock<INotificationService>();
            var viewModel = CreateViewModel(notificationService: mockNotification.Object);

            // Act
            viewModel.ClearTimersCommand.Execute(null);

            // Assert
            mockNotification.Verify(
                s => s.ShowConfirmation(
                    "Confirm reset all timers?",
                    It.IsAny<Action>(),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task SaveDataCommand_Execute_ShouldExportToTextFile()
        {
            // Arrange
            var mockDataPersistence = new Mock<IDataPersistenceService>();
            var viewModel = CreateViewModel(dataPersistenceService: mockDataPersistence.Object);

            // Act
            viewModel.SaveDataCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            mockDataPersistence.Verify(
                s => s.ExportToTextFileAsync(
                    It.IsAny<ObservableCollection<TaskTimerViewModel>>(),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task SaveDataCommand_Execute_ShouldShowNotification()
        {
            // Arrange
            var mockNotification = new Mock<INotificationService>();
            var mockDataPersistence = new Mock<IDataPersistenceService>();

            var viewModel = CreateViewModel(
                dataPersistenceService: mockDataPersistence.Object,
                notificationService: mockNotification.Object);

            // Act
            viewModel.SaveDataCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            mockNotification.Verify(
                s => s.ShowNotification("Data exported successfully."),
                Times.Once);
        }

        [Fact]
        public async Task LoadDataCommand_Execute_WhenDataExists_ShouldLoadTasks()
        {
            // Arrange
            var mockDataPersistence = new Mock<IDataPersistenceService>();
            var loadedTasks = new ObservableCollection<TaskTimerViewModel>
            {
                CreateMockTaskTimerViewModel(new TaskTimer { Description = "Loaded Task 1" }),
                CreateMockTaskTimerViewModel(new TaskTimer { Description = "Loaded Task 2" })
            };
            mockDataPersistence.Setup(s => s.LoadApplicationStateAsync())
                .ReturnsAsync(loadedTasks);

            var viewModel = CreateViewModel(dataPersistenceService: mockDataPersistence.Object);

            // Act
            viewModel.LoadDataCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            viewModel.Tasks.Should().HaveCount(2);
            viewModel.Tasks[0].Description.Should().Be("Loaded Task 1");
            viewModel.Tasks[1].Description.Should().Be("Loaded Task 2");
        }

        [Fact]
        public async Task LoadDataCommand_Execute_WhenNoData_ShouldShowNotification()
        {
            // Arrange
            var mockDataPersistence = new Mock<IDataPersistenceService>();
            mockDataPersistence.Setup(s => s.LoadApplicationStateAsync())
                .ReturnsAsync((ObservableCollection<TaskTimerViewModel>?)null);

            var mockNotification = new Mock<INotificationService>();

            var viewModel = CreateViewModel(
                dataPersistenceService: mockDataPersistence.Object,
                notificationService: mockNotification.Object);

            // Act
            viewModel.LoadDataCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            mockNotification.Verify(
                s => s.ShowNotification("No saved data found."),
                Times.Once);
        }

        [Fact]
        public async Task LoadDataCommand_Execute_WhenDataExists_ShouldMarkAsSaved()
        {
            // Arrange
            var mockDataPersistence = new Mock<IDataPersistenceService>();
            var loadedTasks = new ObservableCollection<TaskTimerViewModel>
            {
                CreateMockTaskTimerViewModel(new TaskTimer())
            };
            mockDataPersistence.Setup(s => s.LoadApplicationStateAsync())
                .ReturnsAsync(loadedTasks);

            var mockDirtyTracking = new Mock<IDirtyTrackingService>();

            var viewModel = CreateViewModel(
                dataPersistenceService: mockDataPersistence.Object,
                dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            viewModel.LoadDataCommand.Execute(null);
            await Task.Delay(100); // Allow async operation to complete

            // Assert
            mockDirtyTracking.Verify(s => s.MarkAsSaved(), Times.Once);
        }

        [Fact]
        public async Task SaveApplicationStateAsync_ShouldSaveState()
        {
            // Arrange
            var mockDataPersistence = new Mock<IDataPersistenceService>();
            var viewModel = CreateViewModel(dataPersistenceService: mockDataPersistence.Object);

            // Act
            await viewModel.SaveApplicationStateAsync();

            // Assert
            mockDataPersistence.Verify(
                s => s.SaveApplicationStateAsync(It.IsAny<ObservableCollection<TaskTimerViewModel>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveApplicationStateAsync_ShouldMarkAsSaved()
        {
            // Arrange
            var mockDirtyTracking = new Mock<IDirtyTrackingService>();
            var viewModel = CreateViewModel(dirtyTrackingService: mockDirtyTracking.Object);

            // Act
            await viewModel.SaveApplicationStateAsync();

            // Assert
            mockDirtyTracking.Verify(s => s.MarkAsSaved(), Times.Once);
        }

        [Fact]
        public void TotalTimeFormatted_WithNoTasks_ShouldShowZero()
        {
            // Arrange
            var viewModel = CreateViewModel();

            // Act
            var total = viewModel.TotalTimeFormatted;

            // Assert
            total.Should().Contain("00:00:00");
        }

        [Fact]
        public void TotalTimeFormatted_WithMultipleTasks_ShouldShowCombinedTime()
        {
            // Arrange
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();

            var task1 = new TaskTimer { Elapsed = TimeSpan.FromHours(1) };
            var task2 = new TaskTimer { Elapsed = TimeSpan.FromHours(2) };

            var vm1 = CreateMockTaskTimerViewModel(task1);
            var vm2 = CreateMockTaskTimerViewModel(task2);

            mockFactory.SetupSequence(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns(vm1)
                .Returns(vm2);

            var viewModel = CreateViewModel(viewModelFactory: mockFactory.Object);
            viewModel.AddTaskCommand.Execute(null);
            viewModel.AddTaskCommand.Execute(null);

            // Act
            var total = viewModel.TotalTimeFormatted;

            // Assert
            total.Should().Contain("03:00:00");
        }

        [Fact]
        public void Cleanup_ShouldCleanupAllTasks()
        {
            // Arrange
            var mockFactory = new Mock<ITaskTimerViewModelFactory>();
            var mockVm = new Mock<TaskTimerViewModel>(
                new TaskTimer(),
                Mock.Of<ITimerCoordinationService>(),
                Mock.Of<IDirtyTrackingService>());

            mockFactory.Setup(f => f.Create(It.IsAny<TaskTimer>()))
                .Returns(mockVm.Object);

            var viewModel = CreateViewModel(viewModelFactory: mockFactory.Object);
            viewModel.AddTaskCommand.Execute(null);

            // Act
            viewModel.Cleanup();

            // Assert
            mockVm.Verify(vm => vm.Cleanup(), Times.Once);
        }

        /// <summary>
        /// Helper method to create MainWindowViewModel with optional mocked dependencies
        /// </summary>
        private MainWindowViewModel CreateViewModel(
            IDataPersistenceService? dataPersistenceService = null,
            INotificationService? notificationService = null,
            IDirtyTrackingService? dirtyTrackingService = null,
            ITaskTimerViewModelFactory? viewModelFactory = null)
        {
            return new MainWindowViewModel(
                dataPersistenceService ?? Mock.Of<IDataPersistenceService>(),
                notificationService ?? Mock.Of<INotificationService>(),
                dirtyTrackingService ?? Mock.Of<IDirtyTrackingService>(),
                viewModelFactory ?? Mock.Of<ITaskTimerViewModelFactory>());
        }

        /// <summary>
        /// Helper method to create a mocked TaskTimerViewModel
        /// </summary>
        private TaskTimerViewModel CreateMockTaskTimerViewModel(TaskTimer task)
        {
            var mockVm = new Mock<TaskTimerViewModel>(
                task,
                Mock.Of<ITimerCoordinationService>(),
                Mock.Of<IDirtyTrackingService>());

            mockVm.Setup(vm => vm.Description).Returns(task.Description);
            mockVm.Setup(vm => vm.TaskElapsedSeconds).Returns(task.Elapsed.TotalSeconds);
            mockVm.Setup(vm => vm.IsRunning).Returns(false);
            mockVm.Setup(vm => vm.Cleanup()).Verifiable();

            return mockVm.Object;
        }
    }
}
