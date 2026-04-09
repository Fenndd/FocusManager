namespace FocusManager.Infrastructure.Windows;

public sealed class ChromePolicyRegistry
{
    public Task ApplyWhitelistAsync(IReadOnlyCollection<string> allowedHosts, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Chrome policy update is not implemented yet.");
    }

    public Task ClearWhitelistAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Chrome policy cleanup is not implemented yet.");
    }
}
