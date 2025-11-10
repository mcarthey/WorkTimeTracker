using System;

namespace WorkTimeTracker.Models
{
    public class TaskTimer
    {
        private DateTime? _startTime;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Elapsed { get; internal set; } = TimeSpan.Zero;

        public void Start() => _startTime ??= DateTime.Now;

        public void Stop()
        {
            if (_startTime != null)
            {
                Elapsed += DateTime.Now - _startTime.Value;
                _startTime = null;
            }
        }

        public void UpdateElapsed()
        {
            if (_startTime != null)
            {
                Elapsed += DateTime.Now - _startTime.Value;
                _startTime = DateTime.Now;
            }
        }

        public void Reset()
        {
            _startTime = null;
            Elapsed = TimeSpan.Zero;
        }
    }
}
