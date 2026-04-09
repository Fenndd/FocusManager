namespace FocusManager.Infrastructure.Windows;

public sealed class ExplorerInterop
{
    public event EventHandler<FolderOpenedEventArgs>? FolderOpened;

    public void StartMonitoring()
    {
        // TODO: Subscribe to Explorer window/tab events.
    }

    public void StopMonitoring()
    {
        // TODO: Unsubscribe from Explorer window/tab events.
    }

    public Task RedirectToAllowedFolderAsync(string targetFolderPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Explorer redirection is not implemented yet.");
    }

    internal void RaiseFolderOpened(FolderOpenedEventArgs args)
    {
        FolderOpened?.Invoke(this, args);
    }
}

public sealed record FolderOpenedEventArgs(string FolderPath);
