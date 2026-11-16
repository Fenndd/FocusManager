namespace FocusManager.Core.Abstractions;

public interface INotifier
{
    Task ShowBlockedAsync(string title, string details, CancellationToken cancellationToken = default);
    Task ShowInfoAsync(string title, string details, CancellationToken cancellationToken = default);
}
