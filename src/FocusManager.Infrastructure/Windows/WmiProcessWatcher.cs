using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;

namespace FocusManager.Infrastructure.Windows;

[SupportedOSPlatform("windows")]
public sealed class WmiProcessWatcher
{
    private readonly object _sync = new();

    private ManagementEventWatcher? _watcher;

    public event EventHandler<ProcessStartedEventArgs>? ProcessStarted;

    public void Start()
    {
        lock (_sync)
        {
            if (_watcher is not null)
            {
                return;
            }

            var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += OnEventArrived;
            _watcher.Start();
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (_watcher is null)
            {
                return;
            }

            _watcher.EventArrived -= OnEventArrived;

            try
            {
                _watcher.Stop();
            }
            catch
            {
                // No-op: watcher can already be stopped/disposed during shutdown.
            }

            _watcher.Dispose();
            _watcher = null;
        }
    }

    private void OnEventArrived(object sender, EventArrivedEventArgs args)
    {
        try
        {
            var rawProcessId = args.NewEvent.Properties["ProcessID"]?.Value;
            if (rawProcessId is null)
            {
                return;
            }

            var processId = Convert.ToInt32(rawProcessId);
            if (processId <= 0)
            {
                return;
            }

            var processName = Convert.ToString(args.NewEvent.Properties["ProcessName"]?.Value) ?? string.Empty;
            var executablePath = ResolveExecutablePath(processId, processName);

            RaiseProcessStarted(new ProcessStartedEventArgs(processId, executablePath));
        }
        catch
        {
            // Ignore malformed/ephemeral WMI events.
        }
    }

    private static string ResolveExecutablePath(int processId, string processName)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.MainModule?.FileName ?? processName;
        }
        catch
        {
            return processName;
        }
    }

    private void RaiseProcessStarted(ProcessStartedEventArgs args)
    {
        ProcessStarted?.Invoke(this, args);
    }
}

public sealed record ProcessStartedEventArgs(int ProcessId, string ExecutablePath);
