using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Monitoring;

public sealed class ExplorerMonitor
{
    private readonly ExplorerInterop _explorerInterop;

    public ExplorerMonitor(ExplorerInterop explorerInterop)
    {
        _explorerInterop = explorerInterop;
    }

    public void Start()
    {
        _explorerInterop.StartMonitoring();
    }

    public void Stop()
    {
        _explorerInterop.StopMonitoring();
    }
}
