using System.Diagnostics;
using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Tests.TestDoubles;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class AppEnforcerTests
{
    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenProcessIdIsExempt()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

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
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, @"C:\\Windows\\explorer.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_NonRootedExecutablePath()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(Environment.ProcessId, "svchost.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenApplicationIsAllowed()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

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
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

        await sut.EnforceAsync(
            new ProcessStartedEventArgs(-1, @"C:\\Apps\\blocked.exe"),
            new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Notifies_WhenDeniedProcessWasKilled()
    {
        var notifier = new RecordingNotifier();
        var sut = new AppEnforcer(new RuleEvaluator(), notifier, NullLogger<AppEnforcer>.Instance);

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c timeout /t 30 >nul",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        Assert.NotNull(process);

        try
        {
            await Task.Delay(200);

            await sut.EnforceAsync(
                new ProcessStartedEventArgs(process!.Id, process.MainModule?.FileName ?? "cmd.exe"),
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
