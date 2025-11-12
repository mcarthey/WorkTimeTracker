using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Repositories;
using WorkTimeTracker.Services;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the service provider for dependency injection
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider
        ?? throw new InvalidOperationException("ServiceProvider not initialized");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure dependency injection container
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Show main window with injected ViewModel
        var mainWindow = new MainWindow
        {
            DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
        };
        mainWindow.Show();
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// Follows SOLID principles: all dependencies are registered here.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Register services as singletons (one instance throughout app lifetime)
        services.AddSingleton<ITimerCoordinationService, TimerCoordinationService>();
        services.AddSingleton<IDirtyTrackingService, DirtyTrackingService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ITaskRepository, JsonTaskRepository>();
        services.AddSingleton<IDataPersistenceService, DataPersistenceService>();
        services.AddSingleton<ITaskTimerViewModelFactory, TaskTimerViewModelFactory>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanup
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

