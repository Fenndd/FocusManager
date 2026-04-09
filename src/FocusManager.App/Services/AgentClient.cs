using FocusManager.Core.Models;

namespace FocusManager.App.Services;

public interface IAgentClient
{
    Task<bool> GetStudyModeStatusAsync(CancellationToken cancellationToken = default);
    Task SetStudyModeAsync(bool enabled, CancellationToken cancellationToken = default);
    Task<WhitelistConfig> GetWhitelistAsync(CancellationToken cancellationToken = default);
    Task SaveWhitelistAsync(WhitelistConfig config, CancellationToken cancellationToken = default);
}

public sealed class AgentClient : IAgentClient
{
    public Task<bool> GetStudyModeStatusAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("IPC channel to FocusManager.Agent is not wired yet.");
    }

    public Task SetStudyModeAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("IPC channel to FocusManager.Agent is not wired yet.");
    }

    public Task<WhitelistConfig> GetWhitelistAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("IPC channel to FocusManager.Agent is not wired yet.");
    }

    public Task SaveWhitelistAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("IPC channel to FocusManager.Agent is not wired yet.");
    }
}
