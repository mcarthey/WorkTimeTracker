namespace WorkTimeTracker.Models;

public class TaskTimer
{
    public string Description { get; set; } = string.Empty;
    public TimeSpan Elapsed { get; private set; }

    private DateTime? _startTime;

    public void Start() => _startTime = DateTime.Now;
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
            var now = DateTime.Now;
            Elapsed = Elapsed + (now - _startTime.Value);
            _startTime = now;
        }
    }
}