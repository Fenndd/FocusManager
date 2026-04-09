using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;

namespace FocusManager.Infrastructure.Persistence;

public sealed class SqliteWhitelistStore : IWhitelistStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    // Temporary in-memory state to keep IPC flow testable before SQLite is implemented.
    private WhitelistConfig _config = new();
    private bool _isStudyModeEnabled;

    public async Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return Clone(_config);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            _config = Clone(config);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> IsStudyModeEnabledAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return _isStudyModeEnabled;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetStudyModeEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _isStudyModeEnabled = enabled;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static WhitelistConfig Clone(WhitelistConfig source)
    {
        return new WhitelistConfig
        {
            AllowedApps = source.AllowedApps
                .Select(x => new AllowedApp(x.DisplayName, x.ExecutablePath))
                .ToList(),
            AllowedFolders = source.AllowedFolders
                .Select(x => new AllowedFolder(x.DisplayName, x.FolderPath))
                .ToList(),
            AllowedSites = source.AllowedSites
                .Select(x => new AllowedSite(x.DisplayName, x.HostPattern))
                .ToList()
        };
    }
}
