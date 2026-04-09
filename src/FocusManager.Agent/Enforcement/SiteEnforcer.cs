using FocusManager.Core.Models;
using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Enforcement;

public sealed class SiteEnforcer
{
    private readonly ChromePolicyRegistry _chromePolicyRegistry;

    public SiteEnforcer(ChromePolicyRegistry chromePolicyRegistry)
    {
        _chromePolicyRegistry = chromePolicyRegistry;
    }

    public Task ApplyAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Site enforcement is not implemented yet.");
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Site enforcement is not implemented yet.");
    }
}
