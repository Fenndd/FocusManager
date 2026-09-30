using FocusManager.Agent.Tray;
using FocusManager.Core.Abstractions;
using System.Windows.Forms;

namespace FocusManager.Agent.Notifications;

public sealed class TrayNotifier : INotifier
{
    private readonly TrayHost _trayHost;

    public TrayNotifier(TrayHost trayHost)
    {
        _trayHost = trayHost;
    }

    public Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _trayHost.ShowNotification(title, details, ToolTipIcon.Warning);
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _trayHost.ShowNotification(title, details, ToolTipIcon.Info);
        return Task.CompletedTask;
    }
}
