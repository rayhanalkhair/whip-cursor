using WhipCursor.Core;

namespace WhipCursor;

/// <summary>Tray icon + menu. Owns settings and pushes every change to the engine and to disk.</summary>
internal sealed class TrayApp : ApplicationContext
{
    private readonly Logger _log;
    private readonly SettingsStore _store;
    private readonly SoundPlayer _sound;
    private readonly WhipEngine _engine;
    private readonly HotkeyWindow _hotkey;
    private readonly Icon _icon;
    private readonly NotifyIcon _tray;
    private AppSettings _settings;

    public TrayApp(Logger log)
    {
        _log = log;
        _store = new SettingsStore(AppPaths.SettingsFile);
        _settings = _store.Load();
        _sound = new SoundPlayer(SoundSources.Load(log), log);
        _engine = new WhipEngine(_settings, _sound, log);

        _icon = TrayIconFactory.Create();
        _tray = new NotifyIcon
        {
            Icon = _icon,
            Text = "Whip Cursor",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip(),
        };
        _tray.ContextMenuStrip.Opening += (_, _) => RebuildMenu();
        RebuildMenu();

        _hotkey = new HotkeyWindow();
        _hotkey.Pressed += (_, _) => Update(_settings with { Enabled = !_settings.Enabled });
        if (!_hotkey.Registered)
        {
            _log.Info("Ctrl+Alt+W is taken by another app; hotkey disabled.");
            _tray.ShowBalloonTip(4000, "Whip Cursor",
                "Ctrl+Alt+W is used by another app, so the toggle hotkey is off. Use the tray menu instead.",
                ToolTipIcon.Info);
        }

        if (_settings.StartWithWindows) TryApplyStartup(true); // refresh the path if the exe moved

        _engine.Start();
    }

    private void RebuildMenu()
    {
        var items = _tray.ContextMenuStrip!.Items;
        items.Clear();
        items.Add(Toggle("Enabled\tCtrl+Alt+W", _settings.Enabled, () => Update(_settings with { Enabled = !_settings.Enabled })));
        items.Add(new ToolStripSeparator());
        items.Add(Choice("Volume", _settings.Volume, v => _settings with { Volume = v }));
        items.Add(Choice("Whip length", _settings.Length, v => _settings with { Length = v }));
        items.Add(Choice("Crack sensitivity", _settings.Sensitivity, v => _settings with { Sensitivity = v }));
        items.Add(Toggle("Spark effect", _settings.SparksEnabled, () => Update(_settings with { SparksEnabled = !_settings.SparksEnabled })));
        items.Add(new ToolStripSeparator());
        items.Add(Toggle("Start with Windows", _settings.StartWithWindows, ToggleStartup));
        items.Add(new ToolStripSeparator());
        items.Add(new ToolStripMenuItem("Quit", null, (_, _) => ExitThread()));
    }

    private static ToolStripMenuItem Toggle(string text, bool isChecked, Action onClick) =>
        new(text, null, (_, _) => onClick()) { Checked = isChecked };

    private ToolStripMenuItem Choice<T>(string text, T current, Func<T, AppSettings> apply) where T : struct, Enum
    {
        var parent = new ToolStripMenuItem(text);
        foreach (var value in Enum.GetValues<T>())
        {
            var item = new ToolStripMenuItem(value.ToString(), null, (_, _) => Update(apply(value)))
            {
                Checked = EqualityComparer<T>.Default.Equals(value, current),
            };
            parent.DropDownItems.Add(item);
        }
        return parent;
    }

    private void Update(AppSettings next)
    {
        _settings = next;
        _engine.UpdateSettings(next);
        try
        {
            _store.Save(next);
        }
        catch (Exception ex)
        {
            _log.Error("Could not save settings.", ex);
        }
    }

    private void ToggleStartup()
    {
        bool enable = !_settings.StartWithWindows;
        if (TryApplyStartup(enable)) Update(_settings with { StartWithWindows = enable });
    }

    private bool TryApplyStartup(bool enabled)
    {
        try
        {
            StartupRegistration.Apply(enabled);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Could not update Start with Windows.", ex);
            return false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tray.Visible = false;
            _tray.Dispose();
            _engine.Dispose();
            _sound.Dispose();
            _hotkey.Dispose();
            _icon.Dispose();
        }
        base.Dispose(disposing);
    }
}
