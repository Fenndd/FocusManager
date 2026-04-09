namespace FocusManager.Contracts;

public enum AgentCommandType
{
    GetStudyModeStatus,
    SetStudyMode,
    GetWhitelist,
    SaveWhitelist
}

public sealed record SetStudyModeRequest(bool Enabled);
public sealed record StudyModeStatusResponse(bool IsEnabled);

public sealed record SaveWhitelistRequest(WhitelistConfigDto Config);
public sealed record GetWhitelistResponse(WhitelistConfigDto Config);

public sealed record WhitelistConfigDto(
    IReadOnlyList<AllowedAppDto> AllowedApps,
    IReadOnlyList<AllowedFolderDto> AllowedFolders,
    IReadOnlyList<AllowedSiteDto> AllowedSites)
{
    public static WhitelistConfigDto Empty { get; } = new(
        Array.Empty<AllowedAppDto>(),
        Array.Empty<AllowedFolderDto>(),
        Array.Empty<AllowedSiteDto>());
}

public sealed record AllowedAppDto(string DisplayName, string ExecutablePath);
public sealed record AllowedFolderDto(string DisplayName, string FolderPath);
public sealed record AllowedSiteDto(string DisplayName, string HostPattern);

public sealed record AgentRequestEnvelope(
    AgentCommandType Command,
    SetStudyModeRequest? SetStudyMode = null,
    SaveWhitelistRequest? SaveWhitelist = null);

public sealed record AgentResponseEnvelope(
    bool Success,
    string? Error = null,
    StudyModeStatusResponse? StudyModeStatus = null,
    GetWhitelistResponse? Whitelist = null)
{
    public static AgentResponseEnvelope Ok(
        StudyModeStatusResponse? studyModeStatus = null,
        GetWhitelistResponse? whitelist = null)
    {
        return new AgentResponseEnvelope(true, null, studyModeStatus, whitelist);
    }

    public static AgentResponseEnvelope Fail(string error)
    {
        return new AgentResponseEnvelope(false, error);
    }
}
