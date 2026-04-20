using System.Diagnostics;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Enforcement;

public sealed class AppEnforcer
{
    private static readonly int CurrentSessionId = Process.GetCurrentProcess().SessionId;
    private static readonly string WindowsDirectoryPath = NormalizePath(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

    private static readonly HashSet<string> IgnoredSystemProcessNames =
    [
        "audiodg.exe",
        "backgroundtaskhost.exe",
        "conhost.exe",
        "ctfmon.exe",
        "dllhost.exe",
        "explorer.exe",
        "fontdrvhost.exe",
        "runtimebroker.exe",
        "searchhost.exe",
        "shellexperiencehost.exe",
        "smartscreen.exe",
        "sppsvc.exe",
        "startmenuexperiencehost.exe",
        "svchost.exe",
        "taskhostw.exe",
        "wmiprvse.exe",
        "focusmanager.agent.exe",
        "focusmanager.app.exe"
    ];

    private readonly INotifier _notifier;
    private readonly RuleEvaluator _ruleEvaluator;
    private readonly ILogger<AppEnforcer> _logger;

    public AppEnforcer(RuleEvaluator ruleEvaluator, INotifier notifier, ILogger<AppEnforcer> logger)
    {
        _ruleEvaluator = ruleEvaluator;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task EnforceAsync(
        ProcessStartedEventArgs args,
        WhitelistConfig config,
        ISet<int>? exemptProcessIds = null,
        CancellationToken cancellationToken = default)
    {
        if (exemptProcessIds is not null && exemptProcessIds.Contains(args.ProcessId))
        {
            return;
        }

        if (ShouldIgnoreProcess(args))
        {
            return;
        }

        var decision = _ruleEvaluator.EvaluateApplicationStart(args.ExecutablePath, config);
        if (decision.IsAllowed)
        {
            return;
        }

        if (!TryTerminateProcess(args.ProcessId))
        {
            return;
        }

        var blockedTarget = string.IsNullOrWhiteSpace(args.ExecutablePath)
            ? $"PID {args.ProcessId}"
            : args.ExecutablePath;

        _logger.LogInformation("Blocked application launch: {Target}. Reason: {Reason}", blockedTarget, decision.Reason);

        await _notifier.ShowBlockedAsync(
            "Blocked Application",
            $"{blockedTarget}. {decision.Reason}",
            cancellationToken);
    }

    private static bool ShouldIgnoreProcess(ProcessStartedEventArgs args)
    {
        if (args.ProcessId <= 0)
        {
            return true;
        }

        if (!TryGetSessionId(args.ProcessId, out var processSessionId))
        {
            return true;
        }

        if (processSessionId != CurrentSessionId)
        {
            return true;
        }

        var executablePath = (args.ExecutablePath ?? string.Empty).Trim().Trim('"');
        var processName = Path.GetFileName(executablePath);

        if (string.IsNullOrWhiteSpace(processName))
        {
            processName = executablePath;
        }

        if (IgnoredSystemProcessNames.Contains(processName.ToLowerInvariant()))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return true;
        }

        if (!Path.IsPathRooted(executablePath))
        {
            return true;
        }

        if (IsWindowsDirectoryPath(executablePath))
        {
            return true;
        }

        return false;
    }

    private static bool IsWindowsDirectoryPath(string executablePath)
    {
        var normalizedExecutablePath = NormalizePath(executablePath);
        if (string.IsNullOrWhiteSpace(normalizedExecutablePath) || string.IsNullOrWhiteSpace(WindowsDirectoryPath))
        {
            return false;
        }

        if (string.Equals(normalizedExecutablePath, WindowsDirectoryPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var windowsPrefix = WindowsDirectoryPath + Path.DirectorySeparatorChar;
        return normalizedExecutablePath.StartsWith(windowsPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');

        try
        {
            cleaned = Path.GetFullPath(cleaned);
        }
        catch
        {
            // Keep raw value when normalization fails.
        }

        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool TryGetSessionId(int processId, out int sessionId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            sessionId = process.SessionId;
            return true;
        }
        catch
        {
            sessionId = -1;
            return false;
        }
    }

    private static bool TryTerminateProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);

            if (process.HasExited)
            {
                return false;
            }

            process.Kill(entireProcessTree: true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
