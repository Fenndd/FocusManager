using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;

namespace FocusManager.Infrastructure.Persistence;

public sealed class SqliteWhitelistStore : IWhitelistStore
{
    public Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SQLite load is not implemented yet.");
    }

    public Task SaveAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SQLite save is not implemented yet.");
    }

    public Task<bool> IsStudyModeEnabledAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Study mode status persistence is not implemented yet.");
    }

    public Task SetStudyModeEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Study mode status persistence is not implemented yet.");
    }
}
