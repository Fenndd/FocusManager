using FocusManager.Core.Models;

namespace FocusManager.Core.Abstractions;

public interface IWhitelistStore
{
    Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(WhitelistConfig config, CancellationToken cancellationToken = default);
    Task<bool> IsStudyModeEnabledAsync(CancellationToken cancellationToken = default);
    Task SetStudyModeEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
}
