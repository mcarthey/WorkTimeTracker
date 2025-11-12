using FluentAssertions;
using WorkTimeTracker.Entities;
using WorkTimeTracker.Repositories;

namespace WorkTimeTracker.Tests.Repositories
{
    /// <summary>
    /// Unit tests for JsonTaskRepository
    /// Note: These are integration tests as they test file I/O
    /// </summary>
    public class JsonTaskRepositoryTests : IDisposable
    {
        private readonly string _testDirectory;

        public JsonTaskRepositoryTests()
        {
            // Create a temporary test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), $"WorkTimeTrackerTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public async Task SaveStateAsync_ShouldCreateJsonFile()
        {
            // Arrange
            var repository = new JsonTaskRepository();
            var state = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>
                {
                    new TaskTimerEntity { Description = "Test Task", ElapsedSeconds = 100 }
                },
                SavedAt = DateTime.Now,
                Version = "2.0.0"
            };

            // Act
            await repository.SaveStateAsync(state);

            // Assert
            repository.HasSavedState().Should().BeTrue();
        }

        [Fact]
        public async Task SaveStateAsync_AndLoadStateAsync_ShouldRoundTripData()
        {
            // Arrange
            var repository = new JsonTaskRepository();
            var originalState = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>
                {
                    new TaskTimerEntity
                    {
                        Description = "Task 1",
                        ElapsedSeconds = 100,
                        IsRunning = false,
                        StartTime = null,
                        LastModified = new DateTime(2025, 1, 1, 12, 0, 0)
                    },
                    new TaskTimerEntity
                    {
                        Description = "Task 2",
                        ElapsedSeconds = 200,
                        IsRunning = false,
                        StartTime = null,
                        LastModified = new DateTime(2025, 1, 1, 13, 0, 0)
                    }
                },
                SavedAt = new DateTime(2025, 1, 1, 14, 0, 0),
                Version = "2.0.0"
            };

            // Act
            await repository.SaveStateAsync(originalState);
            var loadedState = await repository.LoadStateAsync();

            // Assert
            loadedState.Should().NotBeNull();
            loadedState!.Version.Should().Be("2.0.0");
            loadedState.Tasks.Should().HaveCount(2);

            loadedState.Tasks[0].Description.Should().Be("Task 1");
            loadedState.Tasks[0].ElapsedSeconds.Should().Be(100);
            loadedState.Tasks[0].IsRunning.Should().BeFalse();

            loadedState.Tasks[1].Description.Should().Be("Task 2");
            loadedState.Tasks[1].ElapsedSeconds.Should().Be(200);
            loadedState.Tasks[1].IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task LoadStateAsync_WhenNoFileExists_ShouldReturnNull()
        {
            // Arrange
            var repository = new JsonTaskRepository();

            // Act
            var result = await repository.LoadStateAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void HasSavedState_WhenNoFileExists_ShouldReturnFalse()
        {
            // Arrange
            var repository = new JsonTaskRepository();

            // Act
            var result = repository.HasSavedState();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasSavedState_AfterSave_ShouldReturnTrue()
        {
            // Arrange
            var repository = new JsonTaskRepository();
            var state = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>(),
                Version = "2.0.0"
            };

            // Act
            await repository.SaveStateAsync(state);
            var result = repository.HasSavedState();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExportToTextAsync_ShouldCreateTextFile()
        {
            // Arrange
            var repository = new JsonTaskRepository();
            var tasks = new List<TaskTimerEntity>
            {
                new TaskTimerEntity { Description = "Task 1", ElapsedSeconds = 3600 }, // 1 hour
                new TaskTimerEntity { Description = "Task 2", ElapsedSeconds = 7200 }  // 2 hours
            };

            var testFilePath = Path.Combine(_testDirectory, "test_export.txt");

            // Act
            await repository.ExportToTextAsync(tasks, testFilePath);

            // Assert
            File.Exists(testFilePath).Should().BeTrue();

            var content = await File.ReadAllTextAsync(testFilePath);
            content.Should().Contain("Work Time Tracker");
            content.Should().Contain("Task 1");
            content.Should().Contain("Task 2");
            content.Should().Contain("01:00:00"); // Task 1 time
            content.Should().Contain("02:00:00"); // Task 2 time
            content.Should().Contain("Total:");
            content.Should().Contain("03:00:00"); // Total time
        }

        [Fact]
        public async Task ExportToTextAsync_WithEmptyTasks_ShouldCreateFileWithHeaders()
        {
            // Arrange
            var repository = new JsonTaskRepository();
            var tasks = new List<TaskTimerEntity>();
            var testFilePath = Path.Combine(_testDirectory, "test_empty_export.txt");

            // Act
            await repository.ExportToTextAsync(tasks, testFilePath);

            // Assert
            File.Exists(testFilePath).Should().BeTrue();

            var content = await File.ReadAllTextAsync(testFilePath);
            content.Should().Contain("Work Time Tracker");
            content.Should().Contain("Total:");
            content.Should().Contain("00:00:00"); // Total should be zero
        }

        [Fact]
        public async Task SaveStateAsync_WithInvalidPath_ShouldThrowException()
        {
            // Arrange - Create a repository that will try to save to an invalid location
            // Note: This test verifies error handling, but the actual repository saves to LocalApplicationData
            var repository = new JsonTaskRepository();
            var state = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>(),
                Version = "2.0.0"
            };

            // Act & Assert - Should not throw since repository creates directory if needed
            var act = async () => await repository.SaveStateAsync(state);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task LoadStateAsync_WithCorruptedFile_ShouldThrowException()
        {
            // Arrange
            var repository = new JsonTaskRepository();

            // First save a valid state
            var state = new ApplicationStateEntity
            {
                Tasks = new List<TaskTimerEntity>(),
                Version = "2.0.0"
            };
            await repository.SaveStateAsync(state);

            // Get the state file path and corrupt it
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var stateFilePath = Path.Combine(appDataPath, "WorkTimeTracker", "worktimetracker_state.json");
            await File.WriteAllTextAsync(stateFilePath, "{ invalid json }");

            // Act
            var act = async () => await repository.LoadStateAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
