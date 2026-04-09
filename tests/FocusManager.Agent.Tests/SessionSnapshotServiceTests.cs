using FocusManager.Agent.Services;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class SessionSnapshotServiceTests
{
    [Fact]
    public async Task CaptureAsync_ReturnsSnapshotContainingCurrentProcessId()
    {
        var sut = new SessionSnapshotService();

        var snapshot = await sut.CaptureAsync();

        Assert.Contains(Environment.ProcessId, snapshot.ExistingProcessIds);
    }

    [Fact]
    public async Task CaptureAsync_SetsCapturedAtUtc_CloseToNow()
    {
        var sut = new SessionSnapshotService();
        var before = DateTimeOffset.UtcNow;

        var snapshot = await sut.CaptureAsync();

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(snapshot.CapturedAtUtc, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public async Task CaptureAsync_Throws_WhenCancellationRequested()
    {
        var sut = new SessionSnapshotService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.CaptureAsync(cts.Token));
    }
}
