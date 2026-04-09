using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FocusManager.Infrastructure.Windows;

[SupportedOSPlatform("windows")]
public sealed class ExplorerInterop
{
    private readonly object _sync = new();
    private readonly Dictionary<int, string> _lastWindowPaths = [];

    private Timer? _pollTimer;

    public event EventHandler<FolderOpenedEventArgs>? FolderOpened;

    public void StartMonitoring()
    {
        lock (_sync)
        {
            if (_pollTimer is not null)
            {
                return;
            }

            _pollTimer = new Timer(PollExplorerWindows, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
    }

    public void StopMonitoring()
    {
        lock (_sync)
        {
            _pollTimer?.Dispose();
            _pollTimer = null;
            _lastWindowPaths.Clear();
        }
    }

    public Task RedirectToAllowedFolderAsync(string targetFolderPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetFolderPath))
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{targetFolderPath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch
        {
            // Best effort redirect only.
        }

        return Task.CompletedTask;
    }

    private void PollExplorerWindows(object? state)
    {
        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null)
            {
                return;
            }

            object? shell = null;
            object? windows = null;

            try
            {
                shell = Activator.CreateInstance(shellType);
                if (shell is null)
                {
                    return;
                }

                dynamic shellDynamic = shell;
                windows = shellDynamic.Windows();

                dynamic windowsDynamic = windows;
                var count = Convert.ToInt32(windowsDynamic.Count);
                var seenHandles = new HashSet<int>();

                for (var i = 0; i < count; i++)
                {
                    object? window = null;

                    try
                    {
                        window = windowsDynamic.Item(i);
                        if (window is null)
                        {
                            continue;
                        }

                        dynamic windowDynamic = window;

                        var fullName = Convert.ToString(windowDynamic.FullName) ?? string.Empty;
                        if (!fullName.EndsWith("explorer.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var hwnd = Convert.ToInt32(windowDynamic.HWND);
                        var folderPath = TryGetFolderPath(windowDynamic);

                        if (string.IsNullOrWhiteSpace(folderPath))
                        {
                            continue;
                        }

                        seenHandles.Add(hwnd);

                        if (HasFolderChanged(hwnd, folderPath))
                        {
                            RaiseFolderOpened(new FolderOpenedEventArgs(folderPath));
                        }
                    }
                    catch
                    {
                        // Ignore one broken Explorer window.
                    }
                    finally
                    {
                        ReleaseComObject(window);
                    }
                }

                CleanupClosedWindows(seenHandles);
            }
            finally
            {
                ReleaseComObject(windows);
                ReleaseComObject(shell);
            }
        }
        catch
        {
            // Ignore polling errors and try again on next tick.
        }
    }

    private bool HasFolderChanged(int windowHandle, string folderPath)
    {
        lock (_sync)
        {
            if (_lastWindowPaths.TryGetValue(windowHandle, out var previousPath) &&
                string.Equals(previousPath, folderPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _lastWindowPaths[windowHandle] = folderPath;
            return true;
        }
    }

    private void CleanupClosedWindows(HashSet<int> seenHandles)
    {
        lock (_sync)
        {
            foreach (var handle in _lastWindowPaths.Keys.Where(x => !seenHandles.Contains(x)).ToList())
            {
                _lastWindowPaths.Remove(handle);
            }
        }
    }

    private static string TryGetFolderPath(dynamic window)
    {
        try
        {
            var document = window.Document;
            if (document is null)
            {
                return string.Empty;
            }

            var folder = document.Folder;
            if (folder is null)
            {
                return string.Empty;
            }

            var self = folder.Self;
            return Convert.ToString(self.Path) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void ReleaseComObject(object? instance)
    {
        if (instance is null || !Marshal.IsComObject(instance))
        {
            return;
        }

        try
        {
            Marshal.FinalReleaseComObject(instance);
        }
        catch
        {
            // No-op.
        }
    }

    private void RaiseFolderOpened(FolderOpenedEventArgs args)
    {
        FolderOpened?.Invoke(this, args);
    }
}

public sealed record FolderOpenedEventArgs(string FolderPath);
