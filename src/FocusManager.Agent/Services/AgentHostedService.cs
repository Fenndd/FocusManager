using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Monitoring;
using FocusManager.Agent.Tray;
using FocusManager.Contracts;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Services;

public sealed class AgentHostedService : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly object _stateLock = new();

    private readonly ILogger<AgentHostedService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly TrayHost _trayHost;
    private readonly IWhitelistStore _whitelistStore;
    private readonly ProcessStartMonitor _processStartMonitor;
    private readonly ExplorerMonitor _explorerMonitor;
    private readonly AppEnforcer _appEnforcer;
    private readonly FolderEnforcer _folderEnforcer;
    private readonly SiteEnforcer _siteEnforcer;
    private readonly SessionSnapshotService _sessionSnapshotService;

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;

    private bool _isStudyModeEnabled;
    private HashSet<int> _snapshotProcessIds = [];

    public AgentHostedService(
        ILogger<AgentHostedService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        TrayHost trayHost,
        IWhitelistStore whitelistStore,
        ProcessStartMonitor processStartMonitor,
        ExplorerMonitor explorerMonitor,
        AppEnforcer appEnforcer,
        FolderEnforcer folderEnforcer,
        SiteEnforcer siteEnforcer,
        SessionSnapshotService sessionSnapshotService)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _trayHost = trayHost;
        _whitelistStore = whitelistStore;
        _processStartMonitor = processStartMonitor;
        _explorerMonitor = explorerMonitor;
        _appEnforcer = appEnforcer;
        _folderEnforcer = folderEnforcer;
        _siteEnforcer = siteEnforcer;
        _sessionSnapshotService = sessionSnapshotService;

        _processStartMonitor.ProcessStarted += OnProcessStarted;
        _explorerMonitor.FolderOpened += OnFolderOpened;
        _trayHost.StudyModeToggleRequested += OnTrayStudyModeToggleRequested;
        _trayHost.ExitRequested += OnTrayExitRequested;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent starting. IPC server booting on pipe {PipeName}.", IpcProtocol.PipeName);

        var persistedStudyModeEnabled = await _whitelistStore.IsStudyModeEnabledAsync(cancellationToken);
        _trayHost.Start(initialStudyModeEnabled: persistedStudyModeEnabled);

        if (persistedStudyModeEnabled)
        {
            await EnableStudyModeAsync(cancellationToken);
        }
        else
        {
            DisableStudyModeInMemory();
            _processStartMonitor.Stop();
            _explorerMonitor.Stop();
            await _siteEnforcer.ClearAsync(cancellationToken);
        }

        _trayHost.SetStudyModeState(GetStudyModeEnabled());

        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = RunIpcLoopAsync(_loopCts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FocusManager.Agent stopping.");

        _loopCts?.Cancel();

        _processStartMonitor.ProcessStarted -= OnProcessStarted;
        _explorerMonitor.FolderOpened -= OnFolderOpened;
        _trayHost.StudyModeToggleRequested -= OnTrayStudyModeToggleRequested;
        _trayHost.ExitRequested -= OnTrayExitRequested;

        _processStartMonitor.Stop();
        _explorerMonitor.Stop();

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
                using var server = IpcPipeFactory.CreateServerStream(IpcProtocol.PipeName);

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

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
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
                return AgentResponseEnvelope.Ok(studyModeStatus: new StudyModeStatusResponse(GetStudyModeEnabled()));
            }
            case AgentCommandType.SetStudyMode:
            {
                var payload = request.SetStudyMode
                    ?? throw new InvalidOperationException("SetStudyMode payload is missing.");

                await SetStudyModeAsync(payload.Enabled, cancellationToken);
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

                var config = MapToCore(payload.Config);
                await _whitelistStore.SaveAsync(config, cancellationToken);

                if (GetStudyModeEnabled())
                {
                    await _siteEnforcer.ApplyAsync(config, cancellationToken);
                }

                return AgentResponseEnvelope.Ok();
            }
            default:
                return AgentResponseEnvelope.Fail("Unknown command.");
        }
    }

    private async Task SetStudyModeAsync(bool enabled, CancellationToken cancellationToken)
    {
        await _whitelistStore.SetStudyModeEnabledAsync(enabled, cancellationToken);

        if (enabled)
        {
            await EnableStudyModeAsync(cancellationToken);
        }
        else
        {
            await DisableStudyModeAsync(cancellationToken);
        }

        _trayHost.SetStudyModeState(GetStudyModeEnabled());
    }

    private async Task EnableStudyModeAsync(CancellationToken cancellationToken)
    {
        if (GetStudyModeEnabled())
        {
            return;
        }

        var snapshot = await _sessionSnapshotService.CaptureAsync(cancellationToken);
        var config = await _whitelistStore.LoadAsync(cancellationToken);

        lock (_stateLock)
        {
            _snapshotProcessIds = snapshot.ExistingProcessIds;
            _isStudyModeEnabled = true;
        }

        var processMonitorStarted = false;
        var explorerMonitorStarted = false;

        try
        {
            _processStartMonitor.Start();
            processMonitorStarted = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Process start monitoring is unavailable: {Error}. Study mode will continue without app-launch blocking.",
                ex.Message);
        }

        try
        {
            _explorerMonitor.Start();
            explorerMonitorStarted = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Explorer monitoring is unavailable: {Error}. Study mode will continue without folder blocking.",
                ex.Message);
        }

        await _siteEnforcer.ApplyAsync(config, cancellationToken);

        _logger.LogInformation(
            "Study mode enabled. Snapshot captured at {CapturedAtUtc} with {ExistingProcessCount} active processes. ProcessMonitorStarted={ProcessMonitorStarted}, ExplorerMonitorStarted={ExplorerMonitorStarted}.",
            snapshot.CapturedAtUtc,
            snapshot.ExistingProcessIds.Count,
            processMonitorStarted,
            explorerMonitorStarted);
    }

    private async Task DisableStudyModeAsync(CancellationToken cancellationToken)
    {
        _processStartMonitor.Stop();
        _explorerMonitor.Stop();

        await _siteEnforcer.ClearAsync(cancellationToken);

        DisableStudyModeInMemory();
        _logger.LogInformation("Study mode disabled.");
    }

    private void DisableStudyModeInMemory()
    {
        lock (_stateLock)
        {
            _isStudyModeEnabled = false;
            _snapshotProcessIds = [];
        }
    }

    private bool GetStudyModeEnabled()
    {
        lock (_stateLock)
        {
            return _isStudyModeEnabled;
        }
    }

    private HashSet<int> GetSnapshotProcessIds()
    {
        lock (_stateLock)
        {
            return _snapshotProcessIds;
        }
    }

    private async void OnProcessStarted(object? sender, ProcessStartedEventArgs args)
    {
        try
        {
            if (!GetStudyModeEnabled())
            {
                return;
            }

            var snapshotProcessIds = GetSnapshotProcessIds();

            if (snapshotProcessIds.Contains(args.ProcessId))
            {
                return;
            }

            var config = await _whitelistStore.LoadAsync();
            await _appEnforcer.EnforceAsync(args, config, snapshotProcessIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enforce application rules for PID {ProcessId}.", args.ProcessId);
        }
    }

    private async void OnFolderOpened(object? sender, FolderOpenedEventArgs args)
    {
        try
        {
            if (!GetStudyModeEnabled())
            {
                return;
            }

            var config = await _whitelistStore.LoadAsync();
            await _folderEnforcer.EnforceAsync(args, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enforce folder rules for path {FolderPath}.", args.FolderPath);
        }
    }

    private async void OnTrayStudyModeToggleRequested(bool enabled)
    {
        try
        {
            await SetStudyModeAsync(enabled, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change study mode from tray.");
            _trayHost.SetStudyModeState(GetStudyModeEnabled());
        }
    }

    private void OnTrayExitRequested()
    {
        _hostApplicationLifetime.StopApplication();
    }

    private static WhitelistConfigDto MapToDto(WhitelistConfig config)
    {
        return new WhitelistConfigDto(
            config.AllowedApps
                .Select(x => new AllowedAppDto(x.DisplayName, x.ExecutablePath))
                .ToList(),
            config.AllowedFolders
                .Select(x => new AllowedFolderDto(x.DisplayName, x.FolderPath, x.AllowSubfolders))
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
                .Select(x => new AllowedFolder(x.DisplayName, x.FolderPath, x.AllowSubfolders))
                .ToList(),
            AllowedSites = dto.AllowedSites
                .Select(x => new AllowedSite(x.DisplayName, x.HostPattern))
                .ToList()
        };
    }
}
