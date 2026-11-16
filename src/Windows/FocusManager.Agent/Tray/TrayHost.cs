using System.Drawing;
using System.Windows.Forms;

namespace FocusManager.Agent.Tray;

public sealed class TrayHost : IDisposable
{
    private readonly object _sync = new();

    private ApplicationContext? _applicationContext;
    private NotifyIcon? _notifyIcon;
    private ToolStripMenuItem? _studyModeItem;
    private Thread? _uiThread;
    private SynchronizationContext? _uiContext;

    private bool _isRunning;
    private bool _studyModeEnabled;

    public event Action<bool>? StudyModeToggleRequested;
    public event Action? ExitRequested;

    public void Start(bool initialStudyModeEnabled = false)
    {
        lock (_sync)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _studyModeEnabled = initialStudyModeEnabled;

            _uiThread = new Thread(RunTrayUi)
            {
                IsBackground = true,
                Name = "FocusManager.TrayHost"
            };

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();
        }
    }

    public void Stop()
    {
        Thread? uiThread;

        lock (_sync)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            uiThread = _uiThread;
        }

        PostToUi(() => _applicationContext?.ExitThread());

        if (uiThread is { IsAlive: true })
        {
            uiThread.Join(TimeSpan.FromSeconds(2));
        }

        lock (_sync)
        {
            _uiThread = null;
            _uiContext = null;
        }
    }

    public void SetStudyModeState(bool enabled)
    {
        lock (_sync)
        {
            _studyModeEnabled = enabled;
        }

        PostToUi(() => UpdateStudyModeMenu(enabled));
    }

    public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "FocusManager" : title.Trim();
        var safeText = string.IsNullOrWhiteSpace(text) ? "Action completed." : text.Trim();

        if (safeTitle.Length > 63)
        {
            safeTitle = safeTitle[..63];
        }

        if (safeText.Length > 255)
        {
            safeText = safeText[..255];
        }

        PostToUi(() =>
        {
            if (_notifyIcon is null || !_notifyIcon.Visible)
            {
                return;
            }

            _notifyIcon.ShowBalloonTip(
                timeout: 3000,
                tipTitle: safeTitle,
                tipText: safeText,
                tipIcon: icon);
        });
    }

    public void Dispose()
    {
        Stop();
    }

    private void RunTrayUi()
    {
        _uiContext = SynchronizationContext.Current;

        if (_uiContext is null)
        {
            _uiContext = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_uiContext);
        }

        _applicationContext = new ApplicationContext();

        var menu = new ContextMenuStrip();

        _studyModeItem = new ToolStripMenuItem();
        _studyModeItem.Click += OnStudyModeClicked;

        var exitItem = new ToolStripMenuItem("Exit FocusManager Agent");
        exitItem.Click += OnExitClicked;

        menu.Items.Add(_studyModeItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Visible = true,
            Text = "FocusManager Agent",
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => OnStudyModeClicked(this, EventArgs.Empty);

        bool studyModeEnabled;
        lock (_sync)
        {
            studyModeEnabled = _studyModeEnabled;
        }

        UpdateStudyModeMenu(studyModeEnabled);

        Application.Run(_applicationContext);

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        menu.Dispose();

        _notifyIcon = null;
        _studyModeItem = null;
        _applicationContext = null;
    }

    private void OnStudyModeClicked(object? sender, EventArgs e)
    {
        bool newState;

        lock (_sync)
        {
            newState = !_studyModeEnabled;
            _studyModeEnabled = newState;
        }

        UpdateStudyModeMenu(newState);
        StudyModeToggleRequested?.Invoke(newState);
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        ExitRequested?.Invoke();
    }

    private void UpdateStudyModeMenu(bool enabled)
    {
        if (_studyModeItem is null)
        {
            return;
        }

        _studyModeItem.Checked = enabled;
        _studyModeItem.Text = enabled
            ? "Study Mode: ON"
            : "Study Mode: OFF";
    }

    private void PostToUi(Action action)
    {
        var context = _uiContext;

        if (context is null)
        {
            return;
        }

        context.Post(
            _ =>
            {
                try
                {
                    action();
                }
                catch
                {
                    // No-op: tray teardown/update best effort.
                }
            },
            null);
    }
}
