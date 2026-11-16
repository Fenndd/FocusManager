namespace FocusManager.Core.Models;

public sealed class WhitelistConfig
{
    public List<AllowedApp> AllowedApps { get; set; } = [];
    public List<AllowedFolder> AllowedFolders { get; set; } = [];
    public List<AllowedSite> AllowedSites { get; set; } = [];
}

