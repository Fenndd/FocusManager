namespace FocusManager.Infrastructure.Windows;

public sealed class WmiProcessWatcher
{
    public event EventHandler<ProcessStartedEventArgs>? ProcessStarted;

    public void Start()
    {
        // TODO: Wire Win32_ProcessStartTrace subscription.
    }

    public void Stop()
    {
        // TODO: Dispose event subscription.
    }

    internal void RaiseProcessStarted(ProcessStartedEventArgs args)
    {
        ProcessStarted?.Invoke(this, args);
    }
}

public sealed record ProcessStartedEventArgs(int ProcessId, string ExecutablePath);
