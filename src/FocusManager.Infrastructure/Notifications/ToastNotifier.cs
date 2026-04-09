using System.Diagnostics;
using FocusManager.Core.Abstractions;

namespace FocusManager.Infrastructure.Notifications;

public sealed class ToastNotifier : INotifier
{
    public Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[BLOCKED] {title}: {details}");
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[INFO] {title}: {details}");
        return Task.CompletedTask;
    }
}
