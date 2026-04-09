using FocusManager.Core.Models;

namespace FocusManager.Agent.Services;

public sealed class SessionSnapshotService
{
    public Task<SessionSnapshot> CaptureAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Session snapshot capture is not implemented yet.");
    }
}
