using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Monitoring;

public sealed class ProcessStartMonitor
{
    private readonly WmiProcessWatcher _watcher;

    public ProcessStartMonitor(WmiProcessWatcher watcher)
    {
        _watcher = watcher;
        _watcher.ProcessStarted += OnWatcherProcessStarted;
    }

    public event EventHandler<ProcessStartedEventArgs>? ProcessStarted;

    public void Start()
    {
        _watcher.Start();
    }

    public void Stop()
    {
        _watcher.Stop();
    }

    private void OnWatcherProcessStarted(object? sender, ProcessStartedEventArgs args)
    {
        ProcessStarted?.Invoke(this, args);
    }
}
