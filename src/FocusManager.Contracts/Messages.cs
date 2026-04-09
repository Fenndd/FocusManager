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
    IReadOnlyList<AllowedSiteDto> AllowedSites);

public sealed record AllowedAppDto(string DisplayName, string ExecutablePath);
public sealed record AllowedFolderDto(string DisplayName, string FolderPath);
public sealed record AllowedSiteDto(string DisplayName, string HostPattern);
