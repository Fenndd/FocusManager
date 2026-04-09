using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Monitoring;

public sealed class ExplorerMonitor
{
    private readonly ExplorerInterop _explorerInterop;

    public ExplorerMonitor(ExplorerInterop explorerInterop)
    {
        _explorerInterop = explorerInterop;
        _explorerInterop.FolderOpened += OnFolderOpened;
    }

    public event EventHandler<FolderOpenedEventArgs>? FolderOpened;

    public void Start()
    {
        _explorerInterop.StartMonitoring();
    }

    public void Stop()
    {
        _explorerInterop.StopMonitoring();
    }

    private void OnFolderOpened(object? sender, FolderOpenedEventArgs args)
    {
        FolderOpened?.Invoke(this, args);
    }
}
