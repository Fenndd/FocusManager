using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FocusManager.Infrastructure.Windows;

[SupportedOSPlatform("windows")]
public sealed class ExplorerInterop : IExplorerNavigator
{
    private readonly object _sync = new();
    private readonly ExplorerWindowPathCache _pathCache = new();

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
            _pathCache.Clear();
        }
    }

    public Task<bool> RedirectToAllowedFolderAsync(string targetFolderPath, CancellationToken cancellationToken = default)
    {
        return RedirectExplorerWindowToAllowedFolderAsync(windowHandle: 0, targetFolderPath, cancellationToken);
    }

    public Task<bool> RedirectExplorerWindowToAllowedFolderAsync(
        int windowHandle,
        string targetFolderPath,
        CancellationToken cancellationToken = default)
    {
        return RedirectExplorerWindowToAllowedFolderAsync(
            new ExplorerWindowTarget(windowHandle),
            targetFolderPath,
            cancellationToken);
    }

    public Task<bool> RedirectExplorerWindowToAllowedFolderAsync(
        ExplorerWindowTarget target,
        string targetFolderPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetFolderPath))
        {
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var normalizedTarget = NormalizeTargetPath(targetFolderPath);
        var redirected = target.HasTarget && TryNavigateExplorerWindow(target, normalizedTarget);

        if (!redirected)
        {
            OpenExplorerWindow(normalizedTarget);
            return Task.FromResult(true);
        }

        _pathCache.UpdateLastKnownPath(target, normalizedTarget);
        return Task.FromResult(true);
    }

    public Task<bool> CloseExplorerWindowAsync(int windowHandle, CancellationToken cancellationToken = default)
    {
        return CloseExplorerWindowAsync(new ExplorerWindowTarget(windowHandle), cancellationToken);
    }

    public Task<bool> CloseExplorerWindowAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default)
    {
        if (!target.HasTarget)
        {
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var closed = TryCloseExplorerWindow(target);
        if (closed)
        {
            _pathCache.Remove(target);
        }

        return Task.FromResult(closed);
    }

    public Task<bool> GoBackExplorerWindowAsync(int windowHandle, CancellationToken cancellationToken = default)
    {
        return GoBackExplorerWindowAsync(new ExplorerWindowTarget(windowHandle), cancellationToken);
    }

    public Task<bool> GoBackExplorerWindowAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default)
    {
        if (!target.HasTarget)
        {
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(TryGoBackExplorerWindow(target));
    }

    public Task<string?> GetCurrentFolderPathAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default)
    {
        if (!target.HasTarget)
        {
            return Task.FromResult<string?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string? currentPath = null;
        var found = ExecuteForExplorerWindow(
            target,
            window =>
            {
                currentPath = TryGetFolderPath(window);
                return !string.IsNullOrWhiteSpace(currentPath);
            });

        if (found && !string.IsNullOrWhiteSpace(currentPath))
        {
            _pathCache.UpdateLastKnownPath(target, currentPath);
        }

        return Task.FromResult(found ? currentPath : null);
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
                var seenKeys = new HashSet<ExplorerWindowKey>();

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

                        var key = new ExplorerWindowKey(hwnd, i);
                        seenKeys.Add(key);

                        if (_pathCache.HasFolderChanged(key.WindowHandle, key.ShellWindowIndex, folderPath))
                        {
                            RaiseFolderOpened(new FolderOpenedEventArgs(folderPath, hwnd, i));
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

                _pathCache.CleanupClosedWindows(seenKeys);
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

    private static bool TryNavigateExplorerWindow(ExplorerWindowTarget target, string targetFolderPath)
    {
        return ExecuteForExplorerWindow(
            target,
            window =>
            {
                try
                {
                    window.Navigate2(targetFolderPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
    }

    private static bool TryCloseExplorerWindow(ExplorerWindowTarget target)
    {
        return ExecuteForExplorerWindow(
            target,
            window =>
            {
                try
                {
                    window.Quit();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
    }

    private static bool TryGoBackExplorerWindow(ExplorerWindowTarget target)
    {
        return ExecuteForExplorerWindow(
            target,
            window =>
            {
                try
                {
                    window.GoBack();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
    }

    private static bool ExecuteForExplorerWindow(ExplorerWindowTarget target, Func<dynamic, bool> action)
    {
        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null)
            {
                return false;
            }

            object? shell = null;
            object? windows = null;

            try
            {
                shell = Activator.CreateInstance(shellType);
                if (shell is null)
                {
                    return false;
                }

                dynamic shellDynamic = shell;
                windows = shellDynamic.Windows();

                dynamic windowsDynamic = windows;
                var count = Convert.ToInt32(windowsDynamic.Count);

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
                        if (!IsMatchingExplorerWindow(windowDynamic, target, i))
                        {
                            continue;
                        }

                        return action(windowDynamic);
                    }
                    catch
                    {
                        // Ignore broken window entry.
                    }
                    finally
                    {
                        ReleaseComObject(window);
                    }
                }
            }
            finally
            {
                ReleaseComObject(windows);
                ReleaseComObject(shell);
            }
        }
        catch
        {
            // Best-effort COM access only.
        }

        return false;
    }

    private static bool IsMatchingExplorerWindow(dynamic window, ExplorerWindowTarget target, int shellWindowIndex)
    {
        try
        {
            var hwnd = Convert.ToInt32(window.HWND);
            if (target.WindowHandle > 0 && hwnd != target.WindowHandle)
            {
                return false;
            }

            var fullName = Convert.ToString(window.FullName) ?? string.Empty;
            if (!fullName.EndsWith("explorer.exe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (target.ShellWindowIndex >= 0)
            {
                return shellWindowIndex == target.ShellWindowIndex;
            }

            if (!string.IsNullOrWhiteSpace(target.FolderPath))
            {
                var currentPath = TryGetFolderPath(window);
                return string.Equals(
                    NormalizeTargetPath(currentPath),
                    NormalizeTargetPath(target.FolderPath),
                    StringComparison.OrdinalIgnoreCase);
            }

            return target.WindowHandle > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void OpenExplorerWindow(string targetFolderPath)
    {
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
    }

    private static string TryGetFolderPath(dynamic window)
    {
        var fromDocument = TryGetFolderPathFromDocument(window);
        if (!string.IsNullOrWhiteSpace(fromDocument))
        {
            return fromDocument;
        }

        return TryGetFolderPathFromLocationUrl(window);
    }

    private static string TryGetFolderPathFromDocument(dynamic window)
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

    private static string TryGetFolderPathFromLocationUrl(dynamic window)
    {
        try
        {
            var locationUrl = Convert.ToString(window.LocationURL) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(locationUrl))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(locationUrl, UriKind.Absolute, out Uri uri))
            {
                return string.Empty;
            }

            if (uri.IsFile)
            {
                return uri.LocalPath;
            }

            // Keep shell-like URL as-is so caller can decide whether to ignore.
            return locationUrl;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeTargetPath(string targetFolderPath)
    {
        var cleaned = targetFolderPath.Trim().Trim('"');

        try
        {
            return Path.GetFullPath(cleaned);
        }
        catch
        {
            return cleaned;
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

public interface IExplorerNavigator
{
    Task<string?> GetCurrentFolderPathAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default);

    Task<bool> GoBackExplorerWindowAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default);

    Task<bool> CloseExplorerWindowAsync(ExplorerWindowTarget target, CancellationToken cancellationToken = default);

    Task<bool> RedirectExplorerWindowToAllowedFolderAsync(
        ExplorerWindowTarget target,
        string targetFolderPath,
        CancellationToken cancellationToken = default);

    Task<bool> RedirectToAllowedFolderAsync(string targetFolderPath, CancellationToken cancellationToken = default);
}

public sealed class ExplorerWindowPathCache
{
    private readonly object _sync = new();
    private readonly Dictionary<ExplorerWindowKey, string> _lastWindowPaths = [];

    public bool HasFolderChanged(int windowHandle, int shellWindowIndex, string folderPath)
    {
        var key = new ExplorerWindowKey(windowHandle, shellWindowIndex);

        lock (_sync)
        {
            if (_lastWindowPaths.TryGetValue(key, out var previousPath) &&
                string.Equals(previousPath, folderPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _lastWindowPaths[key] = folderPath;
            return true;
        }
    }

    public void UpdateLastKnownPath(ExplorerWindowTarget target, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !TryCreateKey(target, out var key))
        {
            return;
        }

        lock (_sync)
        {
            _lastWindowPaths[key] = folderPath;
        }
    }

    public void Remove(ExplorerWindowTarget target)
    {
        if (!TryCreateKey(target, out var key))
        {
            return;
        }

        lock (_sync)
        {
            _lastWindowPaths.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _lastWindowPaths.Clear();
        }
    }

    internal void CleanupClosedWindows(HashSet<ExplorerWindowKey> seenKeys)
    {
        lock (_sync)
        {
            foreach (var key in _lastWindowPaths.Keys.Where(x => !seenKeys.Contains(x)).ToList())
            {
                _lastWindowPaths.Remove(key);
            }
        }
    }

    private static bool TryCreateKey(ExplorerWindowTarget target, out ExplorerWindowKey key)
    {
        if (target.WindowHandle > 0 && target.ShellWindowIndex >= 0)
        {
            key = new ExplorerWindowKey(target.WindowHandle, target.ShellWindowIndex);
            return true;
        }

        key = default!;
        return false;
    }
}

public sealed record ExplorerWindowTarget(int WindowHandle = 0, int ShellWindowIndex = -1, string FolderPath = "")
{
    public bool HasTarget => WindowHandle > 0 || ShellWindowIndex >= 0;
}

public sealed record FolderOpenedEventArgs(string FolderPath, int WindowHandle = 0, int ShellWindowIndex = -1)
{
    public ExplorerWindowTarget ToExplorerWindowTarget()
    {
        return new ExplorerWindowTarget(WindowHandle, ShellWindowIndex, FolderPath);
    }
}

internal sealed record ExplorerWindowKey(int WindowHandle, int ShellWindowIndex);
