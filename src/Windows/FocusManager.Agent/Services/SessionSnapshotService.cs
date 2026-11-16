using System.Diagnostics;
using FocusManager.Core.Models;

namespace FocusManager.Agent.Services;

public sealed class SessionSnapshotService
{
    public Task<SessionSnapshot> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingProcessIds = new HashSet<int>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                existingProcessIds.Add(process.Id);
            }
            finally
            {
                process.Dispose();
            }
        }

        return Task.FromResult(new SessionSnapshot
        {
            ExistingProcessIds = existingProcessIds,
            CapturedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
