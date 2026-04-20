using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Enforcement;

public sealed class FolderEnforcer
{
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _recentBlocks = new(StringComparer.OrdinalIgnoreCase);

    private readonly RuleEvaluator _ruleEvaluator;
    private readonly ExplorerInterop _explorerInterop;
    private readonly INotifier _notifier;
    private readonly ILogger<FolderEnforcer> _logger;

    public FolderEnforcer(
        RuleEvaluator ruleEvaluator,
        ExplorerInterop explorerInterop,
        INotifier notifier,
        ILogger<FolderEnforcer> logger)
    {
        _ruleEvaluator = ruleEvaluator;
        _explorerInterop = explorerInterop;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task EnforceAsync(FolderOpenedEventArgs args, WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        if (!IsFileSystemPath(args.FolderPath))
        {
            return;
        }

        var decision = _ruleEvaluator.EvaluateFolderOpen(args.FolderPath, config);
        if (decision.IsAllowed)
        {
            return;
        }

        var blockedPath = NormalizePath(args.FolderPath);
        var fallbackFolder = config.AllowedFolders.FirstOrDefault()?.FolderPath;

        var corrected = await ApplyCorrectiveActionAsync(args.WindowHandle, fallbackFolder, cancellationToken);

        var shouldSuppressLog = ShouldSuppressBlock(blockedPath);
        if (shouldSuppressLog)
        {
            return;
        }

        _logger.LogInformation(
            "Blocked folder open: {FolderPath}. Reason: {Reason}. CorrectiveActionApplied: {Corrected}",
            blockedPath,
            decision.Reason,
            corrected);

        await _notifier.ShowBlockedAsync(
            "Blocked Folder",
            $"{blockedPath}. {decision.Reason}",
            cancellationToken);
    }

    private async Task<bool> ApplyCorrectiveActionAsync(
        int windowHandle,
        string? fallbackFolder,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(fallbackFolder))
        {
            var redirected = await _explorerInterop.RedirectExplorerWindowToAllowedFolderAsync(windowHandle, fallbackFolder, cancellationToken);
            if (redirected)
            {
                return true;
            }

            // Hard fallback: close blocked window and open allowed folder separately.
            await _explorerInterop.CloseExplorerWindowAsync(windowHandle, cancellationToken);
            return await _explorerInterop.RedirectToAllowedFolderAsync(fallbackFolder, cancellationToken);
        }

        return await _explorerInterop.CloseExplorerWindowAsync(windowHandle, cancellationToken);
    }

    private bool ShouldSuppressBlock(string folderPath)
    {
        var now = DateTimeOffset.UtcNow;
        var minInterval = TimeSpan.FromSeconds(2);

        lock (_sync)
        {
            if (_recentBlocks.TryGetValue(folderPath, out var lastBlockedAt) && now - lastBlockedAt < minInterval)
            {
                return true;
            }

            _recentBlocks[folderPath] = now;

            foreach (var key in _recentBlocks.Keys.ToList())
            {
                if (now - _recentBlocks[key] > TimeSpan.FromMinutes(5))
                {
                    _recentBlocks.Remove(key);
                }
            }

            return false;
        }
    }

    private static bool IsFileSystemPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmed = path.Trim();

        if (trimmed.StartsWith("::", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (trimmed.StartsWith("shell:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (trimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trimmed.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Path.IsPathRooted(trimmed);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');

        if (Uri.TryCreate(cleaned, UriKind.Absolute, out Uri? uri) && uri is { IsFile: true })
        {
            cleaned = uri.LocalPath;
        }

        try
        {
            cleaned = Path.GetFullPath(cleaned);
        }
        catch
        {
            // Keep raw path if normalization fails.
        }

        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
