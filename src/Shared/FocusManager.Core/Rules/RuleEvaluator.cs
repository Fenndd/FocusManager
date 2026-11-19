using System.Globalization;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;

namespace FocusManager.Core.Rules;

public sealed class RuleEvaluator
{
    private readonly IPathRules _pathRules;

    /// <summary>Creates an evaluator with explicit path semantics for the platform that hosts it.</summary>
    public RuleEvaluator(IPathRules pathRules)
    {
        _pathRules = pathRules ?? throw new ArgumentNullException(nameof(pathRules));
    }

    public RuleDecision EvaluateApplicationStart(string executablePath, WhitelistConfig config)
    {
        if (config.AllowedApps.Count == 0)
        {
            return RuleDecision.Deny("Applications whitelist is empty.");
        }

        var candidatePath = _pathRules.Normalize(executablePath);
        var candidateFileName = _pathRules.GetFileName(candidatePath);

        foreach (var app in config.AllowedApps)
        {
            var allowedPath = _pathRules.Normalize(app.ExecutablePath);

            if (_pathRules.AreEqual(candidatePath, allowedPath))
            {
                return RuleDecision.Allow();
            }

            // Fallback for events where only process name is available.
            if (!string.IsNullOrWhiteSpace(candidateFileName) &&
                _pathRules.AreEqual(candidateFileName, _pathRules.GetFileName(allowedPath)))
            {
                return RuleDecision.Allow();
            }
        }

        return RuleDecision.Deny("Application is not present in whitelist.");
    }

    public RuleDecision EvaluateFolderOpen(string folderPath, WhitelistConfig config)
    {
        if (config.AllowedFolders.Count == 0)
        {
            return RuleDecision.Deny("Folders whitelist is empty.");
        }

        var candidatePath = _pathRules.Normalize(folderPath);

        foreach (var folder in config.AllowedFolders)
        {
            var allowedPath = _pathRules.Normalize(folder.FolderPath);
            if (_pathRules.AreEqual(candidatePath, allowedPath))
            {
                return RuleDecision.Allow();
            }

            if (folder.AllowSubfolders && _pathRules.IsChild(candidatePath, allowedPath))
            {
                return RuleDecision.Allow();
            }
        }

        return RuleDecision.Deny("Folder is not present in whitelist.");
    }

    public RuleDecision EvaluateSiteOpen(string urlOrHost, WhitelistConfig config)
    {
        if (config.AllowedSites.Count == 0)
        {
            return RuleDecision.Deny("Sites whitelist is empty.");
        }

        var host = ExtractHost(urlOrHost);
        if (string.IsNullOrWhiteSpace(host))
        {
            return RuleDecision.Deny("Host could not be parsed.");
        }

        foreach (var site in config.AllowedSites)
        {
            if (HostMatchesPattern(host, site.HostPattern))
            {
                return RuleDecision.Allow();
            }
        }

        return RuleDecision.Deny("Site is not present in whitelist.");
    }

    private static string ExtractHost(string urlOrHost)
    {
        var raw = (urlOrHost ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        if (!raw.Contains("://", StringComparison.Ordinal))
        {
            raw = "https://" + raw;
        }

        return Uri.TryCreate(raw, UriKind.Absolute, out var uri)
            ? uri.Host
            : string.Empty;
    }

    private static bool HostMatchesPattern(string host, string pattern)
    {
        var normalizedHost = host.Trim().ToLowerInvariant();
        var normalizedPattern = (pattern ?? string.Empty).Trim().ToLower(CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(normalizedPattern))
        {
            return false;
        }

        if (normalizedPattern == "*")
        {
            return true;
        }

        if (normalizedPattern.StartsWith("*.", StringComparison.Ordinal))
        {
            var suffix = normalizedPattern[2..];
            return normalizedHost == suffix || normalizedHost.EndsWith('.' + suffix, StringComparison.Ordinal);
        }

        return string.Equals(normalizedHost, normalizedPattern, StringComparison.Ordinal);
    }
}
