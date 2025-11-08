using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using WorkTimeTracker.Models;
using Timer = System.Timers.Timer;

namespace WorkTimeTracker.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Timer _clock;
    private string _currentTime = DateTime.Now.ToString("T");

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TaskTimerViewModel> Tasks { get; } = new();

    public string CurrentTime
    {
        get => _currentTime;
        set { _currentTime = value; OnPropertyChanged(); }
    }

    public string TotalTimeFormatted =>
        "Total Time: " + TimeSpan.FromSeconds(Tasks.Sum(t => t.TaskElapsedSeconds)).ToString(@"hh\:mm\:ss");

    public ICommand AddTaskCommand => new RelayCommand(AddTask);
    public ICommand RemoveTaskCommand => new RelayCommand<TaskTimerViewModel>(RemoveTask);

    public MainWindowViewModel()
    {
        _clock = new Timer(1000);
        _clock.Elapsed += (_, _) =>
        {
            CurrentTime = DateTime.Now.ToString("T");
            OnPropertyChanged(nameof(TotalTimeFormatted));
        };
        _clock.Start();
    }

    private void AddTask()
    {
        var vm = new TaskTimerViewModel(new TaskTimer());
        Tasks.Add(vm);
        OnPropertyChanged(nameof(TotalTimeFormatted));
    }

    private void RemoveTask(TaskTimerViewModel? vm)
    {
        if (vm != null) Tasks.Remove(vm);
        OnPropertyChanged(nameof(TotalTimeFormatted));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
