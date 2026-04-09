using FocusManager.Core.Abstractions;

namespace FocusManager.Infrastructure.Notifications;

public sealed class ToastNotifier : INotifier
{
    public Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Windows toast notifications are not implemented yet.");
    }

    public Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Windows toast notifications are not implemented yet.");
    }
}
