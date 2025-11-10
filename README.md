# Work Time Tracker

A modern, open-source WPF application for tracking time spent on multiple tasks.  
Built with .NET 9 and C# 13, Work Time Tracker provides a clean, intuitive interface for managing your work sessions, logging time, and boosting productivity.

---

## Features

- **Task Management:** Add, remove, and describe multiple tasks, each with its own timer.
- **Individual Timers:** Start and stop timers for each task independently. Only one timer runs at a time.
- **Elapsed Time Adjustment:** Instantly add or subtract 15 minutes from any task with a single click.
- **Total Time Calculation:** See the running total of all task times at a glance.
- **Data Export:** Save a summary of your tasks and times to a timestamped text file.
- **Clear All Timers:** Reset all task timers with confirmation to prevent accidental resets.
- **User Notifications:** In-app notifications for actions like adding, deleting, saving, and resetting tasks.
- **Modern UI:** Clean, responsive interface with styled controls and real-time date/time display.
- **Design-Time Data:** Preview the DataGrid layout in Visual Studio designer with sample data.

---

## Screenshots

![Work Time Tracker Screenshot](https://github.com/mcarthey/WorkTimeTracker/raw/main/.github/screenshot.png)

---

## Getting Started

### Prerequisites

- Windows 10 or 11
- No .NET installation required (self-contained build)

### Installation

1. [Download the latest release](https://github.com/mcarthey/WorkTimeTracker/releases) (`.zip` or installer).
2. Extract the `.zip` (if applicable) to a folder of your choice.
3. Run `WorkTimeTracker.exe`.

---

## Usage

- **Add Task:** Click "➕ Add Task" to create a new task row.
- **Start/Stop Timer:** Use the "Start" and "Stop" buttons for each task.
- **Adjust Time:** Use "+15" or "-15" buttons to quickly modify elapsed time.
- **Remove Task:** Click "X" to delete a task (confirmation required).
- **Clear Timers:** Click "🧹 Clear Timers" to reset all timers (confirmation required).
- **Save Data:** Click "💾 Save" to export your current session to a text file.

---

## Building from Source

1. Clone the repository:

```bash
git clone https://github.com/mcarthey/WorkTimeTracker.git
```

2. Open the solution in Visual Studio 2022 or later.
3. Build and run the project.

Or, publish a self-contained build via CLI:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## Contributing

Contributions, bug reports, and feature requests are welcome!  
Please open an issue or submit a pull request.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Acknowledgments

- Built with WPF and .NET 9
- Inspired by the need for simple, effective time tracking at work

---

**Happy tracking!**
