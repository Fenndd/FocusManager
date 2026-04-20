using FocusManager.Core.Models;
using FocusManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FocusManager.Infrastructure.Tests;

public sealed class SqliteWhitelistStoreTests
{
    [Fact]
    public async Task LoadAsync_ReturnsEmptyLists_AndStudyModeDisabled_ByDefault()
    {
        using var scope = new TempDatabaseScope();
        var sut = new SqliteWhitelistStore(scope.DatabasePath);

        var config = await sut.LoadAsync();
        var studyMode = await sut.IsStudyModeEnabledAsync();

        Assert.Empty(config.AllowedApps);
        Assert.Empty(config.AllowedFolders);
        Assert.Empty(config.AllowedSites);
        Assert.False(studyMode);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsAllLists()
    {
        using var scope = new TempDatabaseScope();
        var sut = new SqliteWhitelistStore(scope.DatabasePath);

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
        Assert.Equal(saved.AllowedFolders, loaded.AllowedFolders);
        Assert.Equal(saved.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public async Task SaveAsync_ReplacesPreviousValues()
    {
        using var scope = new TempDatabaseScope();
        var sut = new SqliteWhitelistStore(scope.DatabasePath);

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
        Assert.Equal(second.AllowedFolders, loaded.AllowedFolders);
        Assert.Equal(second.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public async Task SetStudyModeEnabledAsync_PersistsAcrossInstances()
    {
        using var scope = new TempDatabaseScope();

        var writer = new SqliteWhitelistStore(scope.DatabasePath);
        await writer.SetStudyModeEnabledAsync(true);

        var reader = new SqliteWhitelistStore(scope.DatabasePath);
        var loadedEnabled = await reader.IsStudyModeEnabledAsync();

        Assert.True(loadedEnabled);

        await reader.SetStudyModeEnabledAsync(false);

        var verifier = new SqliteWhitelistStore(scope.DatabasePath);
        var loadedDisabled = await verifier.IsStudyModeEnabledAsync();

        Assert.False(loadedDisabled);
    }

    [Fact]
    public async Task InitializeSchema_MigratesExistingAllowedFoldersTable_AddingAllowSubfoldersColumn()
    {
        using var scope = new TempDatabaseScope();
        CreateLegacySchema(scope.DatabasePath);

        var sut = new SqliteWhitelistStore(scope.DatabasePath);

        await sut.SaveAsync(new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\\Study", AllowSubfolders: true)]
        });

        var loaded = await sut.LoadAsync();

        var folder = Assert.Single(loaded.AllowedFolders);
        Assert.True(folder.AllowSubfolders);
    }

    [Fact]
    public async Task SaveAsync_Throws_WhenConfigIsNull()
    {
        using var scope = new TempDatabaseScope();
        var sut = new SqliteWhitelistStore(scope.DatabasePath);

#pragma warning disable CS8625
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SaveAsync(null));
#pragma warning restore CS8625
    }

    private sealed class TempDatabaseScope : IDisposable
    {
        public TempDatabaseScope()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "FocusManager.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            DatabasePath = Path.Combine(DirectoryPath, "focusmanager.db");
        }

        public string DirectoryPath { get; }

        public string DatabasePath { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }

    private static void CreateLegacySchema(string databasePath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString());

        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS allowed_apps (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL,
    executable_path TEXT NOT NULL UNIQUE COLLATE NOCASE
);

CREATE TABLE IF NOT EXISTS allowed_folders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL,
    folder_path TEXT NOT NULL UNIQUE COLLATE NOCASE
);

CREATE TABLE IF NOT EXISTS allowed_sites (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL,
    host_pattern TEXT NOT NULL UNIQUE COLLATE NOCASE
);";

        command.ExecuteNonQuery();
    }
}
