using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WorkTimeTracker.Entities;
using WorkTimeTracker.Interfaces;

namespace WorkTimeTracker.Repositories
{
    /// <summary>
    /// JSON-based implementation of the task repository.
    /// Provides storage-agnostic persistence using JSON serialization.
    /// Can be easily replaced with database implementation without affecting business logic.
    /// Follows Repository Pattern and Open/Closed Principle.
    /// </summary>
    public class JsonTaskRepository : ITaskRepository
    {
        private const string StateFileName = "worktimetracker_state.json";
        private readonly string _stateFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonTaskRepository()
        {
            // Store in user's local app data folder for proper application data separation
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "WorkTimeTracker");

            // Ensure directory exists
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _stateFilePath = Path.Combine(appFolder, StateFileName);

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <inheritdoc/>
        public async Task SaveStateAsync(ApplicationStateEntity state)
        {
            try
            {
                var json = JsonSerializer.Serialize(state, _jsonOptions);
                await File.WriteAllTextAsync(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error - for now just wrap and rethrow
                // In future, inject ILogger for proper logging
                throw new InvalidOperationException($"Failed to save application state: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ApplicationStateEntity?> LoadStateAsync()
        {
            try
            {
                if (!File.Exists(_stateFilePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(_stateFilePath);
                return JsonSerializer.Deserialize<ApplicationStateEntity>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                // Log error - for now just wrap and rethrow
                throw new InvalidOperationException($"Failed to load application state: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ExportToTextAsync(IEnumerable<TaskTimerEntity> tasks, string fileName)
        {
            try
            {
                var tasksList = tasks.ToList();
                var lines = new[]
                {
                    $"Work Time Tracker - {DateTime.Now}",
                    new string('=', 40)
                }
                .Concat(tasksList.Select((t, i) =>
                {
                    var elapsed = TimeSpan.FromSeconds(t.ElapsedSeconds);
                    return $"{i + 1}. {t.Description,-25} {elapsed:hh\\:mm\\:ss}";
                }))
                .Concat(new[]
                {
                    new string('-', 40),
                    $"Total: {TimeSpan.FromSeconds(tasksList.Sum(t => t.ElapsedSeconds)):hh\\:mm\\:ss}"
                });

                await File.WriteAllLinesAsync(fileName, lines);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export tasks to text: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public bool HasSavedState()
        {
            return File.Exists(_stateFilePath);
        }
    }
}
