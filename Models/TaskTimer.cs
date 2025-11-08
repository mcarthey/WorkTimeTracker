using System;
using System.Diagnostics;

namespace WorkTimeTracker.Models
{
    public class TaskTimer
    {
        private readonly Stopwatch _stopwatch = new();

        public string Description { get; set; } = string.Empty;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public bool IsRunning => _stopwatch.IsRunning;

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();
    }
}
