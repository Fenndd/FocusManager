using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Monitoring;

public sealed class ProcessStartMonitor
{
    private readonly WmiProcessWatcher _watcher;

    public ProcessStartMonitor(WmiProcessWatcher watcher)
    {
        _watcher = watcher;
    }

    public void Start()
    {
        _watcher.Start();
    }

    public void Stop()
    {
        _watcher.Stop();
    }
}
