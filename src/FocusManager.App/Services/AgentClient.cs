using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using FocusManager.Contracts;
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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<bool> GetStudyModeStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            new AgentRequestEnvelope(AgentCommandType.GetStudyModeStatus),
            cancellationToken);

        return response.StudyModeStatus?.IsEnabled
            ?? throw new InvalidOperationException("Agent returned no study mode status.");
    }

    public async Task SetStudyModeAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            new AgentRequestEnvelope(
                AgentCommandType.SetStudyMode,
                SetStudyMode: new SetStudyModeRequest(enabled)),
            cancellationToken);
    }

    public async Task<WhitelistConfig> GetWhitelistAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            new AgentRequestEnvelope(AgentCommandType.GetWhitelist),
            cancellationToken);

        var dto = response.Whitelist?.Config
            ?? throw new InvalidOperationException("Agent returned no whitelist config.");

        return MapToCore(dto);
    }

    public async Task SaveWhitelistAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            new AgentRequestEnvelope(
                AgentCommandType.SaveWhitelist,
                SaveWhitelist: new SaveWhitelistRequest(MapToDto(config))),
            cancellationToken);
    }

    private static async Task<AgentResponseEnvelope> SendAsync(
        AgentRequestEnvelope request,
        CancellationToken cancellationToken)
    {
        using var client = new NamedPipeClientStream(
            serverName: ".",
            pipeName: IpcProtocol.PipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.Asynchronous);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

        await client.ConnectAsync(timeoutCts.Token);

        using var writer = new StreamWriter(client, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true
        };
        using var reader = new StreamReader(client, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        await writer.WriteLineAsync(requestJson);

        var responseJson = await reader.ReadLineAsync(timeoutCts.Token)
            ?? throw new InvalidOperationException("Agent closed the IPC channel without response.");

        var response = JsonSerializer.Deserialize<AgentResponseEnvelope>(responseJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize agent response.");

        if (!response.Success)
        {
            throw new InvalidOperationException(response.Error ?? "Agent returned unknown IPC error.");
        }

        return response;
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
