using FocusManager.Core.Abstractions;

namespace FocusManager.Agent.Tests.TestDoubles;

internal sealed class RecordingNotifier : INotifier
{
    public List<(string Title, string Details)> Blocked { get; } = [];

    public List<(string Title, string Details)> Info { get; } = [];

    public Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Blocked.Add((title, details));
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Info.Add((title, details));
        return Task.CompletedTask;
    }
}
