namespace FocusManager.Core.Models;

public sealed class WhitelistConfig
{
    public List<AllowedApp> AllowedApps { get; init; } = [];
    public List<AllowedFolder> AllowedFolders { get; init; } = [];
    public List<AllowedSite> AllowedSites { get; init; } = [];
}
