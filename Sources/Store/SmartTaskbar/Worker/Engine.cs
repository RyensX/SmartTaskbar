﻿using System.Timers;
using Timer = System.Timers.Timer;

namespace SmartTaskbar;

public sealed class Engine : IDisposable
{
    private static Timer? _timer;

    private static bool _enabled;

    public Engine()
    {
        // 125 milliseconds is a balance between user-acceptable perception and system call time.
        _timer = new Timer(125);
        _timer.Elapsed += Timer_Elapsed;
    }

    public void Dispose()
    {
        if (_timer is null) return;

        _timer?.Dispose();
        _timer = null;
    }

    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        _timer?.Stop();
        // Make sure the taskbar has been automatically hidden, otherwise it will not work
        Fun.SetAutoHide();
        var taskbar = TaskbarHelper.InitTaskbar();

        // Some users will kill the explorer.exe under certain situation.
        // In this case, the taskbar cannot be found, just return and wait for the user to reopen the file explorer.
        if (!taskbar.HasValue)
            return;

        var behavior = taskbar.Value.ShouldMouseOverWindowShowTheTaskbar();

        if (behavior == TaskbarBehavior.Pending)
        {
            behavior = taskbar.Value.ShouldForegroundWindowShowTheTaskbar();
            if (behavior == TaskbarBehavior.Pending)
                behavior = taskbar.Value.ShouldDesktopShowTheTaskbar();
        }

        switch (behavior)
        {
            case TaskbarBehavior.Show:
                taskbar.Value.ShowTaskar();
                break;
            case TaskbarBehavior.Hide:
                taskbar.Value.HideTaskbar();
                break;
        }

        if (_enabled)
            _timer?.Start();
    }

    /// <summary>
    ///     Turn off the timer, Pause auto mode
    /// </summary>
    public static void Stop()
    {
        _enabled = false;
        _timer?.Stop();
    }

    /// <summary>
    ///     Start the timer, start the auto mode
    /// </summary>
    public static void Start()
    {
        _enabled = true;
        _timer?.Start();
    }
}