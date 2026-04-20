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
        var allowedHosts = config.AllowedSites
            .Select(x => x.HostPattern)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (allowedHosts.Count == 0)
        {
            // Empty whitelist means "do not enforce website restrictions".
            return _chromePolicyRegistry.ClearWhitelistAsync(cancellationToken);
        }

        return _chromePolicyRegistry.ApplyWhitelistAsync(allowedHosts, cancellationToken);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        return _chromePolicyRegistry.ClearWhitelistAsync(cancellationToken);
    }
}
