using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Enforcement;

public sealed class FolderEnforcer
{
    private const int MaxBackNavigationAttempts = 8;

    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _recentBlocks = new(StringComparer.OrdinalIgnoreCase);

    private readonly RuleEvaluator _ruleEvaluator;
    private readonly IExplorerNavigator _explorerNavigator;
    private readonly INotifier _notifier;
    private readonly ILogger<FolderEnforcer> _logger;

    public FolderEnforcer(
        RuleEvaluator ruleEvaluator,
        IExplorerNavigator explorerNavigator,
        INotifier notifier,
        ILogger<FolderEnforcer> logger)
    {
        _ruleEvaluator = ruleEvaluator;
        _explorerNavigator = explorerNavigator;
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

        var corrected = await ApplyCorrectiveActionAsync(
            args.ToExplorerWindowTarget(),
            blockedPath,
            config,
            fallbackFolder,
            cancellationToken);

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
        ExplorerWindowTarget target,
        string blockedPath,
        WhitelistConfig config,
        string? fallbackFolder,
        CancellationToken cancellationToken)
    {
        var currentPath = await _explorerNavigator.GetCurrentFolderPathAsync(target, cancellationToken);
        if (IsAllowedFolder(currentPath, config))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(currentPath))
        {
            return await RedirectToFallbackAsync(target, fallbackFolder, cancellationToken);
        }

        if (!PathsEqual(currentPath, blockedPath))
        {
            return false;
        }

        for (var attempt = 0; attempt < MaxBackNavigationAttempts; attempt++)
        {
            var previousPath = currentPath;
            if (!await _explorerNavigator.GoBackExplorerWindowAsync(target, cancellationToken))
            {
                break;
            }

            currentPath = await _explorerNavigator.GetCurrentFolderPathAsync(target, cancellationToken);
            if (IsAllowedFolder(currentPath, config))
            {
                return true;
            }

            if (!PathsEqual(currentPath, blockedPath) && !PathsEqual(currentPath, previousPath))
            {
                continue;
            }

            break;
        }

        if (await _explorerNavigator.CloseExplorerWindowAsync(target, cancellationToken))
        {
            return true;
        }

        return await RedirectToFallbackAsync(target, fallbackFolder, cancellationToken);
    }

    private async Task<bool> RedirectToFallbackAsync(
        ExplorerWindowTarget target,
        string? fallbackFolder,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(fallbackFolder))
        {
            if (target.HasTarget &&
                await _explorerNavigator.RedirectExplorerWindowToAllowedFolderAsync(target, fallbackFolder, cancellationToken))
            {
                return true;
            }

            return await _explorerNavigator.RedirectToAllowedFolderAsync(fallbackFolder, cancellationToken);
        }

        return false;
    }

    private bool IsAllowedFolder(string? folderPath, WhitelistConfig config)
    {
        return !string.IsNullOrWhiteSpace(folderPath) &&
            IsFileSystemPath(folderPath) &&
            _ruleEvaluator.EvaluateFolderOpen(folderPath, config).IsAllowed;
    }

    private static bool PathsEqual(string? first, string? second)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
        {
            return false;
        }

        return string.Equals(NormalizePath(first), NormalizePath(second), StringComparison.OrdinalIgnoreCase);
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
