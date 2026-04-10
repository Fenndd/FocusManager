using FocusManager.Infrastructure.Windows;
using Microsoft.Win32;
using Xunit;

namespace FocusManager.Infrastructure.Tests;

public sealed class ChromePolicyRegistryTests
{
    [Fact]
    public async Task ApplyWhitelistAsync_WritesBlockListAndNormalizedAllowList()
    {
        using var scope = new RegistryPolicyScope();
        var sut = new ChromePolicyRegistry(scope.PolicyPath);

        await sut.ApplyWhitelistAsync(
        [
            " example.com ",
            "*.wikipedia.org",
            "https://news.ycombinator.com",
            "chrome://settings",
            "example.com"
        ]);

        using var policyKey = Registry.CurrentUser.OpenSubKey(scope.PolicyPath);
        Assert.NotNull(policyKey);

        using var blockList = policyKey!.OpenSubKey("URLBlocklist");
        Assert.NotNull(blockList);
        Assert.Equal("*", blockList!.GetValue("1")?.ToString());

        using var allowList = policyKey.OpenSubKey("URLAllowlist");
        Assert.NotNull(allowList);

        var values = allowList!
            .GetValueNames()
            .Select(x => allowList.GetValue(x)?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();

        Assert.Equal(4, values.Count);
        Assert.Contains("example.com", values);
        Assert.Contains("[*.]wikipedia.org", values);
        Assert.Contains("https://news.ycombinator.com", values);
        Assert.Contains("chrome://settings", values);
    }

    [Fact]
    public async Task ClearWhitelistAsync_RemovesAllowAndBlockSubkeys()
    {
        using var scope = new RegistryPolicyScope();
        var sut = new ChromePolicyRegistry(scope.PolicyPath);

        await sut.ApplyWhitelistAsync(["example.com"]);
        await sut.ClearWhitelistAsync();

        using var policyKey = Registry.CurrentUser.OpenSubKey(scope.PolicyPath);
        Assert.NotNull(policyKey);
        Assert.Null(policyKey!.OpenSubKey("URLAllowlist"));
        Assert.Null(policyKey.OpenSubKey("URLBlocklist"));
    }

    [Fact]
    public async Task ApplyAndClear_Throw_WhenCancellationAlreadyRequested()
    {
        using var scope = new RegistryPolicyScope();
        var sut = new ChromePolicyRegistry(scope.PolicyPath);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.ApplyWhitelistAsync(["example.com"], cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.ClearWhitelistAsync(cts.Token));
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
