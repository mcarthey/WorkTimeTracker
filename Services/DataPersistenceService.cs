using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkTimeTracker.Entities;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker.Services
{
    /// <summary>
    /// Service responsible for orchestrating data persistence operations.
    /// Maps between ViewModels and Entities, coordinating with the repository layer.
    /// Follows Single Responsibility Principle and acts as a facade for persistence.
    /// </summary>
    public class DataPersistenceService : IDataPersistenceService
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskTimerViewModelFactory _viewModelFactory;

        public DataPersistenceService(
            ITaskRepository repository,
            ITaskTimerViewModelFactory viewModelFactory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
        }

        /// <inheritdoc/>
        public async Task SaveApplicationStateAsync(ObservableCollection<TaskTimerViewModel> tasks)
        {
            var state = new ApplicationStateEntity
            {
                SavedAt = DateTime.Now,
                Tasks = tasks.Select(MapToEntity).ToList()
            };

            await _repository.SaveStateAsync(state);
        }

        /// <inheritdoc/>
        public async Task<ObservableCollection<TaskTimerViewModel>?> LoadApplicationStateAsync()
        {
            var state = await _repository.LoadStateAsync();

            if (state == null || state.Tasks == null || !state.Tasks.Any())
            {
                return null;
            }

            var viewModels = state.Tasks.Select(MapToViewModel).ToList();
            return new ObservableCollection<TaskTimerViewModel>(viewModels);
        }

        /// <inheritdoc/>
        public async Task ExportToTextFileAsync(
            ObservableCollection<TaskTimerViewModel> tasks,
            string? fileName = null)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var exportFileName = fileName ?? $"time_tracking_{timestamp}.txt";

            var entities = tasks.Select(MapToEntity).ToList();
            await _repository.ExportToTextAsync(entities, exportFileName);
        }

        /// <inheritdoc/>
        public bool CanRestoreState()
        {
            return _repository.HasSavedState();
        }

        /// <summary>
        /// Maps a TaskTimerViewModel to a TaskTimerEntity for persistence
        /// </summary>
        private TaskTimerEntity MapToEntity(TaskTimerViewModel viewModel)
        {
            return new TaskTimerEntity
            {
                Description = viewModel.Description,
                ElapsedSeconds = viewModel.TaskElapsedSeconds,
                IsRunning = viewModel.IsRunning,
                // Note: StartTime is not persisted for simplicity
                // Timers are stopped on save and can be restarted manually
                StartTime = null,
                LastModified = DateTime.Now
            };
        }

        /// <summary>
        /// Maps a TaskTimerEntity to a TaskTimerViewModel for restoration
        /// </summary>
        private TaskTimerViewModel MapToViewModel(TaskTimerEntity entity)
        {
            var task = new TaskTimer
            {
                Description = entity.Description,
                Elapsed = TimeSpan.FromSeconds(entity.ElapsedSeconds)
            };

            var viewModel = _viewModelFactory.Create(task);

            // Note: We don't restore running state to avoid confusion
            // User can manually restart timers after restoration

            return viewModel;
        }
    }
}
