using System.Globalization;
using FocusManager.Core.Models;

namespace FocusManager.Core.Rules;

public sealed class RuleEvaluator
{
    public RuleDecision EvaluateApplicationStart(string executablePath, WhitelistConfig config)
    {
        if (config.AllowedApps.Count == 0)
        {
            return RuleDecision.Deny("Applications whitelist is empty.");
        }

        var candidatePath = NormalizePath(executablePath);
        var candidateFileName = Path.GetFileName(candidatePath);

        foreach (var app in config.AllowedApps)
        {
            var allowedPath = NormalizePath(app.ExecutablePath);

            if (string.Equals(candidatePath, allowedPath, StringComparison.OrdinalIgnoreCase))
            {
                return RuleDecision.Allow();
            }

            // Fallback for events where only process name is available.
            if (!string.IsNullOrWhiteSpace(candidateFileName) &&
                string.Equals(candidateFileName, Path.GetFileName(allowedPath), StringComparison.OrdinalIgnoreCase))
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

        var candidatePath = NormalizePath(folderPath);

        foreach (var folder in config.AllowedFolders)
        {
            var allowedPath = NormalizePath(folder.FolderPath);
            if (IsChildOrSamePath(candidatePath, allowedPath))
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
            // Keep best-effort raw path when full-path resolution fails.
        }

        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool IsChildOrSamePath(string candidatePath, string allowedPath)
    {
        if (string.Equals(candidatePath, allowedPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var allowedPrefix = allowedPath + Path.DirectorySeparatorChar;
        return candidatePath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase);
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
