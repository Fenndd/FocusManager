using Microsoft.Win32;
using System.Runtime.Versioning;

namespace FocusManager.Infrastructure.Windows;

[SupportedOSPlatform("windows")]
public sealed class ChromePolicyRegistry
{
    private const string DefaultChromePolicyPath = "Software\\Policies\\Google\\Chrome";
    private const string UrlAllowListKeyName = "URLAllowlist";
    private const string UrlBlockListKeyName = "URLBlocklist";

    private readonly string _chromePolicyPath;

    public ChromePolicyRegistry()
        : this(policyPath: null)
    {
    }

    public ChromePolicyRegistry(string? policyPath)
    {
        _chromePolicyPath = string.IsNullOrWhiteSpace(policyPath)
            ? DefaultChromePolicyPath
            : policyPath.Trim();
    }

    public Task ApplyWhitelistAsync(IReadOnlyCollection<string> allowedHosts, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var chromePolicyKey = Registry.CurrentUser.CreateSubKey(_chromePolicyPath, writable: true);
        if (chromePolicyKey is null)
        {
            return Task.CompletedTask;
        }

        using var blockListKey = chromePolicyKey.CreateSubKey(UrlBlockListKeyName, writable: true);
        using var allowListKey = chromePolicyKey.CreateSubKey(UrlAllowListKeyName, writable: true);

        if (blockListKey is null || allowListKey is null)
        {
            return Task.CompletedTask;
        }

        ClearValues(blockListKey);
        ClearValues(allowListKey);

        blockListKey.SetValue("1", "*", RegistryValueKind.String);

        var normalizedRules = allowedHosts
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeAllowRule)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var i = 0; i < normalizedRules.Count; i++)
        {
            var name = (i + 1).ToString();
            allowListKey.SetValue(name, normalizedRules[i], RegistryValueKind.String);
        }

        return Task.CompletedTask;
    }

    public Task ClearWhitelistAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var chromePolicyKey = Registry.CurrentUser.OpenSubKey(_chromePolicyPath, writable: true);
        if (chromePolicyKey is null)
        {
            return Task.CompletedTask;
        }

        TryDeleteSubKeyTree(chromePolicyKey, UrlAllowListKeyName);
        TryDeleteSubKeyTree(chromePolicyKey, UrlBlockListKeyName);

        return Task.CompletedTask;
    }

    private static string NormalizeAllowRule(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Contains("://", StringComparison.Ordinal) || trimmed.StartsWith("chrome://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (trimmed.Contains("/*", StringComparison.Ordinal))
        {
            return trimmed;
        }

        return $"*://{trimmed}/*";
    }

    private static void ClearValues(RegistryKey key)
    {
        foreach (var valueName in key.GetValueNames())
        {
            key.DeleteValue(valueName, throwOnMissingValue: false);
        }
    }

    private static void TryDeleteSubKeyTree(RegistryKey parent, string subKeyName)
    {
        try
        {
            parent.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
        }
        catch
        {
            // Best effort cleanup only.
        }
    }
}
