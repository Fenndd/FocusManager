using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using FocusManager.Agent.Tray;
using FocusManager.Contracts;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Services;

public sealed class AgentHostedService : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<AgentHostedService> _logger;
    private readonly TrayHost _trayHost;
    private readonly IWhitelistStore _whitelistStore;

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;

    public AgentHostedService(
        ILogger<AgentHostedService> logger,
        TrayHost trayHost,
        IWhitelistStore whitelistStore)
    {
        _logger = logger;
        _trayHost = trayHost;
        _whitelistStore = whitelistStore;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent starting. IPC server booting on pipe {PipeName}.", IpcProtocol.PipeName);
        _trayHost.Start();

        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = RunIpcLoopAsync(_loopCts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent stopping.");

        _loopCts?.Cancel();
        _trayHost.Stop();

        if (_loopTask is null)
        {
            return;
        }

        try
        {
            await _loopTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown path.
        }
    }

    private async Task RunIpcLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    IpcProtocol.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(cancellationToken);
                await HandleClientAsync(server, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IPC server loop error.");
            }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true
        };

        var requestJson = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(requestJson))
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(AgentResponseEnvelope.Fail("Empty IPC request."), JsonOptions));
            return;
        }

        AgentResponseEnvelope response;

        try
        {
            var request = JsonSerializer.Deserialize<AgentRequestEnvelope>(requestJson, JsonOptions)
                ?? throw new InvalidOperationException("Request could not be parsed.");

            response = await HandleRequestAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process IPC request.");
            response = AgentResponseEnvelope.Fail(ex.Message);
        }

        var responseJson = JsonSerializer.Serialize(response, JsonOptions);
        await writer.WriteLineAsync(responseJson);
    }

    private async Task<AgentResponseEnvelope> HandleRequestAsync(
        AgentRequestEnvelope request,
        CancellationToken cancellationToken)
    {
        switch (request.Command)
        {
            case AgentCommandType.GetStudyModeStatus:
            {
                var isEnabled = await _whitelistStore.IsStudyModeEnabledAsync(cancellationToken);
                return AgentResponseEnvelope.Ok(studyModeStatus: new StudyModeStatusResponse(isEnabled));
            }
            case AgentCommandType.SetStudyMode:
            {
                var payload = request.SetStudyMode
                    ?? throw new InvalidOperationException("SetStudyMode payload is missing.");

                await _whitelistStore.SetStudyModeEnabledAsync(payload.Enabled, cancellationToken);
                return AgentResponseEnvelope.Ok();
            }
            case AgentCommandType.GetWhitelist:
            {
                var config = await _whitelistStore.LoadAsync(cancellationToken);
                return AgentResponseEnvelope.Ok(whitelist: new GetWhitelistResponse(MapToDto(config)));
            }
            case AgentCommandType.SaveWhitelist:
            {
                var payload = request.SaveWhitelist
                    ?? throw new InvalidOperationException("SaveWhitelist payload is missing.");

                await _whitelistStore.SaveAsync(MapToCore(payload.Config), cancellationToken);
                return AgentResponseEnvelope.Ok();
            }
            default:
                return AgentResponseEnvelope.Fail("Unknown command.");
        }
    }

    private static WhitelistConfigDto MapToDto(WhitelistConfig config)
    {
        return new WhitelistConfigDto(
            config.AllowedApps
                .Select(x => new AllowedAppDto(x.DisplayName, x.ExecutablePath))
                .ToList(),
            config.AllowedFolders
                .Select(x => new AllowedFolderDto(x.DisplayName, x.FolderPath))
                .ToList(),
            config.AllowedSites
                .Select(x => new AllowedSiteDto(x.DisplayName, x.HostPattern))
                .ToList());
    }

    private static WhitelistConfig MapToCore(WhitelistConfigDto dto)
    {
        return new WhitelistConfig
        {
            AllowedApps = dto.AllowedApps
                .Select(x => new AllowedApp(x.DisplayName, x.ExecutablePath))
                .ToList(),
            AllowedFolders = dto.AllowedFolders
                .Select(x => new AllowedFolder(x.DisplayName, x.FolderPath))
                .ToList(),
            AllowedSites = dto.AllowedSites
                .Select(x => new AllowedSite(x.DisplayName, x.HostPattern))
                .ToList()
        };
    }
}
