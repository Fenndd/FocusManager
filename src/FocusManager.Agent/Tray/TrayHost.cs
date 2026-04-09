using System.Windows.Forms;

namespace FocusManager.Agent.Tray;

public sealed class TrayHost : IDisposable
{
    private NotifyIcon? _notifyIcon;

    public void Start()
    {
        // TODO: Initialize tray icon, context menu, and user actions.
    }

    public void Stop()
    {
        // TODO: Hide tray icon and dispose resources.
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}
