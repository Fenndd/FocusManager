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
    public async Task EnforceAsync_GoesBack_TargetTab_WhenExplorerTabsShareWindowHandle()
    {
        var notifier = new RecordingNotifier();
        var navigator = new FakeExplorerNavigator();
        navigator.AddTab(new ExplorerWindowTarget(100, 0), @"C:\\Study");
        navigator.AddTab(new ExplorerWindowTarget(100, 1), @"C:\\Blocked", [@"C:\\Study"]);
        var sut = CreateSut(notifier, navigator);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Blocked", 100, 1), config);

        var goBackTarget = Assert.Single(navigator.GoBackTargets);
        Assert.Equal(1, goBackTarget.ShellWindowIndex);
        Assert.Equal(@"C:\\Study", navigator.GetTabPath(100, 0));
        Assert.Equal(@"C:\\Study", navigator.GetTabPath(100, 1));
        Assert.Empty(navigator.CloseTargets);
    }

    [Fact]
    public async Task EnforceAsync_RepeatsBackNavigation_UntilTargetTabReachesAllowedFolder()
    {
        var notifier = new RecordingNotifier();
        var navigator = new FakeExplorerNavigator();
        navigator.AddTab(
            new ExplorerWindowTarget(200, 0),
            @"C:\\Blocked",
            [@"C:\\OtherBlocked", @"C:\\Study"]);
        var sut = CreateSut(notifier, navigator);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Blocked", 200, 0), config);

        Assert.Equal(2, navigator.GoBackTargets.Count);
        Assert.All(navigator.GoBackTargets, target => Assert.Equal(0, target.ShellWindowIndex));
        Assert.Equal(@"C:\\Study", navigator.GetTabPath(200, 0));
        Assert.Empty(navigator.CloseTargets);
    }

    [Fact]
    public async Task EnforceAsync_ClosesTargetTab_WhenBackNavigationDoesNotLeaveBlockedFolder()
    {
        var notifier = new RecordingNotifier();
        var navigator = new FakeExplorerNavigator();
        navigator.AddTab(new ExplorerWindowTarget(300, 0), @"C:\\Blocked", [@"C:\\Blocked"]);
        var sut = CreateSut(notifier, navigator);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Blocked", 300, 0), config);

        var closeTarget = Assert.Single(navigator.CloseTargets);
        Assert.Equal(0, closeTarget.ShellWindowIndex);
    }

    [Fact]
    public async Task EnforceAsync_DoesNotTouchTarget_WhenCurrentPathNoLongerMatchesBlockedEvent()
    {
        var notifier = new RecordingNotifier();
        var navigator = new FakeExplorerNavigator();
        navigator.AddTab(new ExplorerWindowTarget(400, 0), @"C:\\DifferentBlocked", [@"C:\\Study"]);
        var sut = CreateSut(notifier, navigator);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        await sut.EnforceAsync(new FolderOpenedEventArgs(@"C:\\Blocked", 400, 0), config);

        Assert.Empty(navigator.GoBackTargets);
        Assert.Empty(navigator.CloseTargets);
        Assert.Equal(@"C:\\DifferentBlocked", navigator.GetTabPath(400, 0));
    }

    [Fact]
    public async Task EnforceAsync_AppliesCorrectiveAction_WhenForwardReopensSameBlockedFolderImmediately()
    {
        var notifier = new RecordingNotifier();
        var navigator = new FakeExplorerNavigator();
        navigator.AddTab(new ExplorerWindowTarget(500, 0), @"C:\\Blocked", [@"C:\\Study"]);
        var sut = CreateSut(notifier, navigator);

        var config = new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")]
        };

        var args = new FolderOpenedEventArgs(@"C:\\Blocked", 500, 0);
        await sut.EnforceAsync(args, config);

        navigator.SetTabPath(500, 0, @"C:\\Blocked", [@"C:\\Study"]);
        await sut.EnforceAsync(args, config);

        Assert.Equal(2, navigator.GoBackTargets.Count);
        Assert.Equal(@"C:\\Study", navigator.GetTabPath(500, 0));
        Assert.Single(notifier.Blocked);
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

    private static FolderEnforcer CreateSut(RecordingNotifier notifier, IExplorerNavigator? navigator = null)
    {
        return new FolderEnforcer(
            new RuleEvaluator(),
            navigator ?? new FakeExplorerNavigator(),
            notifier,
            NullLogger<FolderEnforcer>.Instance);
    }

    private sealed class FakeExplorerNavigator : IExplorerNavigator
    {
        private readonly List<FakeExplorerTab> _tabs = [];

        public List<ExplorerWindowTarget> GoBackTargets { get; } = [];

        public List<ExplorerWindowTarget> CloseTargets { get; } = [];

        public Task<bool> RedirectToAllowedFolderAsync(
            string targetFolderPath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RedirectExplorerWindowToAllowedFolderAsync(
            ExplorerWindowTarget target,
            string targetFolderPath,
            CancellationToken cancellationToken = default)
        {
            var tab = FindTab(target);
            if (tab is null)
            {
                return Task.FromResult(false);
            }

            tab.CurrentPath = targetFolderPath;
            return Task.FromResult(true);
        }

        public Task<bool> CloseExplorerWindowAsync(
            ExplorerWindowTarget target,
            CancellationToken cancellationToken = default)
        {
            var tab = FindTab(target);
            if (tab is null)
            {
                return Task.FromResult(false);
            }

            CloseTargets.Add(target);
            _tabs.Remove(tab);
            return Task.FromResult(true);
        }

        public Task<bool> GoBackExplorerWindowAsync(
            ExplorerWindowTarget target,
            CancellationToken cancellationToken = default)
        {
            var tab = FindTab(target);
            if (tab is null || tab.BackPaths.Count == 0)
            {
                return Task.FromResult(false);
            }

            GoBackTargets.Add(target);
            tab.CurrentPath = tab.BackPaths.Dequeue();
            return Task.FromResult(true);
        }

        public Task<string?> GetCurrentFolderPathAsync(
            ExplorerWindowTarget target,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FindTab(target)?.CurrentPath);
        }

        public void AddTab(ExplorerWindowTarget target, string currentPath, IEnumerable<string>? backPaths = null)
        {
            _tabs.Add(new FakeExplorerTab(target, currentPath, new Queue<string>(backPaths ?? [])));
        }

        public string? GetTabPath(int windowHandle, int shellWindowIndex)
        {
            return _tabs.FirstOrDefault(x =>
                x.Target.WindowHandle == windowHandle &&
                x.Target.ShellWindowIndex == shellWindowIndex)?.CurrentPath;
        }

        public void SetTabPath(
            int windowHandle,
            int shellWindowIndex,
            string currentPath,
            IEnumerable<string>? backPaths = null)
        {
            var tab = _tabs.First(x =>
                x.Target.WindowHandle == windowHandle &&
                x.Target.ShellWindowIndex == shellWindowIndex);

            tab.CurrentPath = currentPath;
            tab.BackPaths.Clear();
            foreach (var backPath in backPaths ?? [])
            {
                tab.BackPaths.Enqueue(backPath);
            }
        }

        private FakeExplorerTab? FindTab(ExplorerWindowTarget target)
        {
            return _tabs.FirstOrDefault(x =>
                x.Target.WindowHandle == target.WindowHandle &&
                x.Target.ShellWindowIndex == target.ShellWindowIndex);
        }
    }

    private sealed class FakeExplorerTab
    {
        public FakeExplorerTab(ExplorerWindowTarget target, string currentPath, Queue<string> backPaths)
        {
            Target = target;
            CurrentPath = currentPath;
            BackPaths = backPaths;
        }

        public ExplorerWindowTarget Target { get; }

        public string CurrentPath { get; set; }

        public Queue<string> BackPaths { get; }
    }
}
