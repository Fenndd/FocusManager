using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Monitoring;

public sealed class ChromeMonitor
{
    private readonly ChromePolicyRegistry _policyRegistry;

    public ChromeMonitor(ChromePolicyRegistry policyRegistry)
    {
        _policyRegistry = policyRegistry;
    }

    public Task ApplyAsync(IReadOnlyCollection<string> allowedHosts, CancellationToken cancellationToken = default)
    {
        return _policyRegistry.ApplyWhitelistAsync(allowedHosts, cancellationToken);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        return _policyRegistry.ClearWhitelistAsync(cancellationToken);
    }
}
