using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Monitoring;
using FocusManager.Core.Models;
using FocusManager.Infrastructure.Windows;
using Microsoft.Win32;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class SitePolicyIntegrationTests
{
    [Fact]
    public async Task SiteEnforcer_ApplyAsync_WritesOnlyConfiguredHosts()
    {
        using var scope = new RegistryPolicyScope();
        var registry = new ChromePolicyRegistry(scope.PolicyPath);
        var sut = new SiteEnforcer(registry);

        var config = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Wiki", "wikipedia.org"),
                new AllowedSite("Empty", " "),
                new AllowedSite("Docs", "*.microsoft.com")
            ]
        };

        await sut.ApplyAsync(config);

        var values = ReadAllowListValues(scope.PolicyPath);

        Assert.Equal(2, values.Count);
        Assert.Contains("wikipedia.org", values);
        Assert.Contains("[*.]microsoft.com", values);
    }

    [Fact]
    public async Task SiteEnforcer_ClearAsync_RemovesPolicySubkeys()
    {
        using var scope = new RegistryPolicyScope();
        var registry = new ChromePolicyRegistry(scope.PolicyPath);
        var sut = new SiteEnforcer(registry);

        await sut.ApplyAsync(new WhitelistConfig
        {
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        });

        await sut.ClearAsync();

        using var policyKey = Registry.CurrentUser.OpenSubKey(scope.PolicyPath);
        Assert.NotNull(policyKey);
        Assert.Null(policyKey!.OpenSubKey("URLAllowlist"));
        Assert.Null(policyKey.OpenSubKey("URLBlocklist"));
    }

    [Fact]
    public async Task SiteEnforcer_ApplyAsync_WithEmptyWhitelist_RemovesPolicySubkeys()
    {
        using var scope = new RegistryPolicyScope();
        var registry = new ChromePolicyRegistry(scope.PolicyPath);
        var sut = new SiteEnforcer(registry);

        await sut.ApplyAsync(new WhitelistConfig
        {
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        });

        await sut.ApplyAsync(new WhitelistConfig
        {
            AllowedSites = []
        });

        using var policyKey = Registry.CurrentUser.OpenSubKey(scope.PolicyPath);
        Assert.NotNull(policyKey);
        Assert.Null(policyKey!.OpenSubKey("URLAllowlist"));
        Assert.Null(policyKey.OpenSubKey("URLBlocklist"));
    }

    [Fact]
    public async Task ChromeMonitor_ApplyAndClear_ManagePolicyKeys()
    {
        using var scope = new RegistryPolicyScope();
        var monitor = new ChromeMonitor(new ChromePolicyRegistry(scope.PolicyPath));

        await monitor.ApplyAsync(["example.com", "https://news.ycombinator.com"]);

        var valuesAfterApply = ReadAllowListValues(scope.PolicyPath);
        Assert.Equal(2, valuesAfterApply.Count);
        Assert.Contains("example.com", valuesAfterApply);
        Assert.Contains("https://news.ycombinator.com", valuesAfterApply);

        await monitor.ClearAsync();

        using var policyKey = Registry.CurrentUser.OpenSubKey(scope.PolicyPath);
        Assert.NotNull(policyKey);
        Assert.Null(policyKey!.OpenSubKey("URLAllowlist"));
        Assert.Null(policyKey.OpenSubKey("URLBlocklist"));
    }

    private static List<string> ReadAllowListValues(string policyPath)
    {
        using var policyKey = Registry.CurrentUser.OpenSubKey(policyPath);
        Assert.NotNull(policyKey);

        using var allowList = policyKey!.OpenSubKey("URLAllowlist");
        Assert.NotNull(allowList);

        return allowList!
            .GetValueNames()
            .Select(x => allowList.GetValue(x)?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();
    }

    private sealed class RegistryPolicyScope : IDisposable
    {
        private const string RootPrefix = "Software\\FocusManager.Tests";

        public RegistryPolicyScope()
        {
            RootPath = $"{RootPrefix}\\{Guid.NewGuid():N}";
            PolicyPath = $"{RootPath}\\ChromePolicy";
        }

        public string RootPath { get; }

        public string PolicyPath { get; }

        public void Dispose()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RootPath, throwOnMissingSubKey: false);
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }
}
