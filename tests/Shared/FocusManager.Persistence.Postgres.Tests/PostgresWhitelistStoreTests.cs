using FocusManager.Core.Models;
using FocusManager.Persistence.Postgres;
using Npgsql;
using Xunit;

namespace FocusManager.Persistence.Postgres.Tests;

public sealed class PostgresWhitelistStoreTests
{
    private static readonly AllowedFolder DefaultRootFolder = new("System Root (C:)", @"C:\", false);

    [PostgresFact]
    public async Task LoadAsync_ReturnsDefaultRootFolder_AndStudyModeDisabled_ByDefault()
    {
        await using var scope = await TempSchemaScope.CreateAsync();
        await using var sut = new PostgresWhitelistStore(scope.ConnectionString);

        var config = await sut.LoadAsync();
        var studyMode = await sut.IsStudyModeEnabledAsync();

        Assert.Empty(config.AllowedApps);
        Assert.Equal([DefaultRootFolder], config.AllowedFolders);
        Assert.Empty(config.AllowedSites);
        Assert.False(studyMode);
    }

    [PostgresFact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsAllLists()
    {
        await using var scope = await TempSchemaScope.CreateAsync();
        await using var sut = new PostgresWhitelistStore(scope.ConnectionString);

        var saved = new WhitelistConfig
        {
            AllowedApps =
            [
                new AllowedApp("Code", @"C:\\Apps\\Code.exe"),
                new AllowedApp("Notepad", @"C:\\Windows\\System32\\notepad.exe")
            ],
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\\Study", AllowSubfolders: true),
                new AllowedFolder("Notes", @"C:\\Notes", AllowSubfolders: false)
            ],
            AllowedSites =
            [
                new AllowedSite("Wiki", "wikipedia.org"),
                new AllowedSite("Docs", "*.microsoft.com")
            ]
        };

        await sut.SaveAsync(saved);
        var loaded = await sut.LoadAsync();

        Assert.Equal(saved.AllowedApps, loaded.AllowedApps);
        Assert.Equal(WithDefaultFolder(saved.AllowedFolders), loaded.AllowedFolders);
        Assert.Equal(saved.AllowedSites, loaded.AllowedSites);
    }

    [PostgresFact]
    public async Task SaveAsync_ReplacesPreviousValues()
    {
        await using var scope = await TempSchemaScope.CreateAsync();
        await using var sut = new PostgresWhitelistStore(scope.ConnectionString);

        var first = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Code", @"C:\\Apps\\Code.exe")],
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study")],
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        };

        var second = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Notepad", @"C:\\Windows\\System32\\notepad.exe")],
            AllowedFolders = [new AllowedFolder("Projects", @"C:\\Projects")],
            AllowedSites = [new AllowedSite("Docs", "learn.microsoft.com")]
        };

        await sut.SaveAsync(first);
        await sut.SaveAsync(second);

        var loaded = await sut.LoadAsync();

        Assert.Equal(second.AllowedApps, loaded.AllowedApps);
        Assert.Equal(WithDefaultFolder(second.AllowedFolders), loaded.AllowedFolders);
        Assert.Equal(second.AllowedSites, loaded.AllowedSites);
    }

    [PostgresFact]
    public async Task SaveAsync_RejectsDuplicatesDifferingOnlyByCase_AndKeepsPreviousValues()
    {
        await using var scope = await TempSchemaScope.CreateAsync();
        await using var sut = new PostgresWhitelistStore(scope.ConnectionString);

        var original = new WhitelistConfig
        {
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        };
        await sut.SaveAsync(original);

        var duplicate = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Docs", "learn.microsoft.com"),
                new AllowedSite("Docs upper", "LEARN.microsoft.com")
            ]
        };

        await Assert.ThrowsAsync<PostgresException>(() => sut.SaveAsync(duplicate));

        var loaded = await sut.LoadAsync();
        Assert.Equal(original.AllowedSites, loaded.AllowedSites);
    }

    [PostgresFact]
    public async Task SetStudyModeEnabledAsync_PersistsAcrossInstances()
    {
        await using var scope = await TempSchemaScope.CreateAsync();

        await using (var writer = new PostgresWhitelistStore(scope.ConnectionString))
        {
            await writer.SetStudyModeEnabledAsync(true);
        }

        await using (var reader = new PostgresWhitelistStore(scope.ConnectionString))
        {
            Assert.True(await reader.IsStudyModeEnabledAsync());
            await reader.SetStudyModeEnabledAsync(false);
        }

        await using var verifier = new PostgresWhitelistStore(scope.ConnectionString);
        Assert.False(await verifier.IsStudyModeEnabledAsync());
    }

    [PostgresFact]
    public async Task SaveAsync_Throws_WhenConfigIsNull()
    {
        await using var scope = await TempSchemaScope.CreateAsync();
        await using var sut = new PostgresWhitelistStore(scope.ConnectionString);

#pragma warning disable CS8625
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SaveAsync(null));
#pragma warning restore CS8625
    }

    [Fact]
    public void Constructor_Throws_WhenConnectionStringIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new PostgresWhitelistStore(" "));
    }

    private static List<AllowedFolder> WithDefaultFolder(IEnumerable<AllowedFolder> folders)
    {
        var list = folders.ToList();
        if (list.All(x => !string.Equals(x.FolderPath, DefaultRootFolder.FolderPath, StringComparison.OrdinalIgnoreCase)))
        {
            list.Insert(0, DefaultRootFolder);
        }

        return list;
    }
}
