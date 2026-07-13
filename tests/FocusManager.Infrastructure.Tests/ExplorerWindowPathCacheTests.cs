using FocusManager.Infrastructure.Windows;
using Xunit;

namespace FocusManager.Infrastructure.Tests;

public sealed class ExplorerWindowPathCacheTests
{
    [Fact]
    public void HasFolderChanged_TreatsForwardToPreviouslyBlockedPathAsChange_AfterCorrectiveSync()
    {
        var sut = new ExplorerWindowPathCache();
        var target = new ExplorerWindowTarget(100, 1);

        Assert.True(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
        sut.UpdateLastKnownPath(target, @"C:\Study");

        Assert.True(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
    }

    [Fact]
    public void HasFolderChanged_SuppressesSamePath_WhenNoCorrectiveSyncOccurred()
    {
        var sut = new ExplorerWindowPathCache();

        Assert.True(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
        Assert.False(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
    }

    [Fact]
    public void Remove_AllowsSamePathToBeReportedAgain_ForClosedAndReusedTarget()
    {
        var sut = new ExplorerWindowPathCache();
        var target = new ExplorerWindowTarget(100, 1);

        Assert.True(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
        sut.Remove(target);

        Assert.True(sut.HasFolderChanged(100, 1, @"C:\Blocked"));
    }
}
