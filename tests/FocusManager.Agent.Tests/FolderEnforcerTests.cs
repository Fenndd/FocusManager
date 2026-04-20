using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Tests.TestDoubles;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class FolderEnforcerTests
{
    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenFolderIsAllowed()
    {
        var notifier = new RecordingNotifier();
        var sut = CreateSut(notifier);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Study"), config);

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_DoesNothing_WhenChildFolderIsAllowedByFlag()
    {
        var notifier = new RecordingNotifier();
        var sut = CreateSut(notifier);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study", AllowSubfolders: true)]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Study\\Math"), config);

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Ignores_VirtualShellPaths()
    {
        var notifier = new RecordingNotifier();
        var sut = CreateSut(notifier);

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"::\\{F874310E-B6B7-47DC-BC84-B9E6B38F5903}"), new WhitelistConfig());

        Assert.Empty(notifier.Blocked);
    }

    [Fact]
    public async Task EnforceAsync_Notifies_WhenFolderIsDenied()
    {
        var notifier = new RecordingNotifier();
        var sut = CreateSut(notifier);

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Blocked"), new WhitelistConfig());

        Assert.Single(notifier.Blocked);
        Assert.Contains("Blocked Folder", notifier.Blocked[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnforceAsync_SuppressesImmediateDuplicateNotifications_ForSameFolder()
    {
        var notifier = new RecordingNotifier();
        var sut = CreateSut(notifier);

        var args = new FolderOpenedEventArgs(@"C:\\Blocked");

        await sut.EnforceAsync(args, new WhitelistConfig());
        await sut.EnforceAsync(args, new WhitelistConfig());

        Assert.Single(notifier.Blocked);
    }

    private static FolderEnforcer CreateSut(RecordingNotifier notifier)
    {
        return new FolderEnforcer(
            new RuleEvaluator(),
            new ExplorerInterop(),
            notifier,
            NullLogger<FolderEnforcer>.Instance);
    }
}
