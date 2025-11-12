using System.Windows;
using System.Windows.Controls;
using WorkTimeTracker.ViewModels;

namespace WorkTimeTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // DataContext is now set via dependency injection in App.xaml.cs
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Auto-save application state on exit
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SaveApplicationStateAsync();
            viewModel.Cleanup();
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Intentionally empty - can be used for future features
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Commit description changes when TextBox loses focus
        if (sender is TextBox textBox && textBox.DataContext is TaskTimerViewModel viewModel)
        {
            viewModel.CommitDescription();
        }
    }
}