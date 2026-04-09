using FocusManager.Agent.Tray;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Services;

public sealed class AgentHostedService : IHostedService
{
    private readonly ILogger<AgentHostedService> _logger;
    private readonly TrayHost _trayHost;

    public AgentHostedService(ILogger<AgentHostedService> logger, TrayHost trayHost)
    {
        _logger = logger;
        _trayHost = trayHost;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent starting (stub). Monitoring is not wired yet.");
        _trayHost.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent stopping.");
        _trayHost.Stop();
        return Task.CompletedTask;
    }
}
