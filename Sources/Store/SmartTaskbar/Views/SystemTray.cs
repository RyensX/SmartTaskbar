﻿using System.ComponentModel;
using System.Diagnostics;
using Windows.System;
using Windows.UI.ViewManagement;

namespace SmartTaskbar;

internal class SystemTray : ApplicationContext
{
    private readonly ToolStripMenuItem _animationInBar;
    private readonly ToolStripMenuItem _autoMode;
    private readonly ContextMenuStrip _contextMenuStrip;
    private readonly ToolStripMenuItem _showBarOnExit;
    private readonly NotifyIcon _notifyIcon;

    private readonly Container _container = new();

    private readonly Engine _engine = new();
    private readonly UserSettings _userSettings = new();

    public SystemTray()
    {
        #region Initialization

        var font = new Font("Segoe UI", 10.5F);

        var resource = new ResourceCulture();

        var about = new ToolStripMenuItem(text: $"{resource.GetString("tray_about")} v1.3.0")
        {
            Font = font,
        };
        _animationInBar = new ToolStripMenuItem(resource.GetString("tray_animation"))
        {
            Font = font,
        };
        _showBarOnExit = new ToolStripMenuItem(resource.GetString("tray_showBarOnExit"))
        {
            Font = font,
        };
        _autoMode = new ToolStripMenuItem(resource.GetString("tray_auto"))
        {
            Font = font,
        };
        var exit = new ToolStripMenuItem(resource.GetString("tray_exit"))
        {
            Font = font,
        };

        _contextMenuStrip = new ContextMenuStrip(_container)
        {
            Renderer = new Win11Renderer()
        };

        _contextMenuStrip.Items.AddRange(new ToolStripItem[]
        {
            about,
            new ToolStripSeparator(),
            _animationInBar,
            _showBarOnExit,
            new ToolStripSeparator(),
            _autoMode,
            new ToolStripSeparator(),
            exit
        });

        _notifyIcon = new NotifyIcon(_container)
        {
            Text = Application.ProductName,
            Icon = Fun.IsLightTheme() ? IconResource.Logo_Black : IconResource.Logo_White,
            Visible = true,
        };

        #endregion

        #region Load Event

        about.Click += AboutOnClick;

        _animationInBar.Click += OnAnimationInBarOnClick;

        _showBarOnExit.Click += OnShowBarOnExitOnClick;

        _autoMode.Click += OnAutoModeOnClick;

        exit.Click += OnExitOnClick;

        _notifyIcon.MouseClick += OnNotifyIconOnMouseClick;

        _notifyIcon.MouseDoubleClick += OnNotifyIconOnMouseDoubleClick;

        Fun.UISettings.ColorValuesChanged += OnUiSettingsOnColorValuesChanged;

        Application.ApplicationExit += Application_ApplicationExit;

        _userSettings.OnAutoModeTypePropertyChanged += OnAutoModeTypePropertyChanged;

        #endregion

        #region Load Settings

        switch (_userSettings.AutoModeType)
        {
            case AutoModeType.Auto:
                _autoMode.Checked = true;
                Engine.Start();
                break;
            case AutoModeType.None:
                _autoMode.Checked = false;
                break;
        }

        #endregion
    }

    private void AboutOnClick(object? sender, EventArgs e)
        => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/ChanpleCai/SmartTaskbar"));

    private void OnUiSettingsOnColorValuesChanged(UISettings s, object e)
        => _notifyIcon.Icon = Fun.IsLightTheme() ? IconResource.Logo_Black : IconResource.Logo_White;

    private void OnNotifyIconOnMouseDoubleClick(object? s, MouseEventArgs e)
    {
        _userSettings.AutoModeType = AutoModeType.None;
        Fun.ChangeAutoHide();
    }

    private void OnNotifyIconOnMouseClick(object? s, MouseEventArgs e)
    {
        Fun.SetForegroundWindow(_contextMenuStrip.Handle);

        if (_userSettings.AutoModeType == AutoModeType.Auto)
            Engine.Start();

        if (e.Button != MouseButtons.Right) return;

        _animationInBar.Checked = Fun.GetTaskbarAnimation();
        _showBarOnExit.Checked = UserSettings.ShowTaskbarWhenExit;

        var taskbar = TaskbarHelper.InitTaskbar();

        var y = taskbar?.Rect.top ?? Cursor.Position.Y;

        _contextMenuStrip.Show(Cursor.Position.X - 30,
                                          y - _contextMenuStrip.Height - 20);
    }

    private void OnExitOnClick(object? s, EventArgs e)
    {
        _container.Dispose();
        _engine.Dispose();
        TaskbarHelper.InitTaskbar()?.HideTaskbar();
        if (UserSettings.ShowTaskbarWhenExit) Fun.CancelAutoHide();
        Application.Exit();
    }

    private void OnShowBarOnExitOnClick(object? s, EventArgs e)
        => UserSettings.ShowTaskbarWhenExit = !_showBarOnExit.Checked;

    private void OnAutoModeOnClick(object? s, EventArgs e)
        => _userSettings.AutoModeType = _autoMode.Checked ? AutoModeType.None : AutoModeType.Auto;

    private void OnAnimationInBarOnClick(object? s, EventArgs e)
        => _animationInBar.Checked = Fun.ChangeTaskbarAnimation();

    private void OnAutoModeTypePropertyChanged(object? s, AutoModeType e)
    {
        switch (e)
        {
            case AutoModeType.Auto:
                _autoMode.Checked = true;
                Engine.Start();
                break;
            case AutoModeType.None:
                _autoMode.Checked = false;
                Engine.Stop();
                break;
        }
    }

    private static async void Application_ApplicationExit(object? sender, EventArgs e)
    {
        // Weird bug.
        await Task.Delay(500);
        Process.GetCurrentProcess().Kill();
    }
}