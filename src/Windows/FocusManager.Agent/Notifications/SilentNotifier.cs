using FocusManager.Core.Abstractions;

namespace FocusManager.Agent.Notifications;

public sealed class SilentNotifier : INotifier
{
    public Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
