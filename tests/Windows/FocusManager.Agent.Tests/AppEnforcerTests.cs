using System.Diagnostics;
using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Tests.TestDoubles;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Platform.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class AppEnforcerTests
{
    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenProcessIdIsExempt()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(123456, @"C:\\Apps\\blocked.exe"),
            new WhitelistConfig(),
            exemptProcessIds: new HashSet<int> { 123456 });

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_KnownSystemProcessName()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, @"C:\\Windows\\explorer.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_NonRootedExecutablePath()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, "svchost.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_AnyProcessUnderWindowsDirectory()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, @"C:\\Windows\\System32\\calc.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_AnyProcessUnderWindowsAppsDirectory()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, @"C:\\Program Files\\WindowsApps\\SomeApp\\App.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenApplicationIsAllowed()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        var config = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Notepad", @"C:\\Windows\\System32\\notepad.exe")]
        };

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, @"C:\\Windows\\System32\\notepad.exe"),
            config);

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_DoesNotNotify_WhenProcessCannotBeTerminated()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(-1, @"C:\\Apps\\blocked.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Notifies_WhenDeniedProcessWasKilled()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(PathRules.Windows), notifier, NullLogger<AppEnforcer>.Instance);

        // Use a fully qualified system executable that neither depends on PATH resolution
        // (Git Bash puts GNU timeout first) nor on stdin/console (timeout.exe fails when input is redirected).
        // ping.exe -n 30 against loopback stays alive for roughly 29 seconds.
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "ping.exe"),
            Arguments = "-n 30 127.0.0.1",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        });

        Assert.NotNull(process);

        try
        {
            await Task.Delay(200);

            await sut.EnforceAsync(
                new ProcessStartedEventArgs(process!.Id, @"C:\\Apps\\blocked.exe"),
                new WhitelistConfig());

            process.WaitForExit(3000);

            Assert.Single(notifier.Blocked);
            Assert.Contains("Blocked Application", notifier.Blocked[0].Title, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try
            {
                if (process is { HasExited: false })
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }
}
