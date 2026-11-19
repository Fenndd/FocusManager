using FocusManager.Core.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FocusManager.Persistence.Sqlite.Tests;

/// <summary>
/// Databases written by earlier SQLite stores, and data surviving a restart of the agent.
/// </summary>
public sealed class SqliteWhitelistStoreMigrationTests
{
    // The Windows agent's FLD-004 default, which the earlier SQLite stores also persisted.
    private static readonly AllowedFolder WindowsRootFolder = new("System Root (C:)", @"C:\", AllowSubfolders: false);

    [Fact]
    public async Task OpensDatabaseWithoutAllowSubfoldersColumn_AddsColumn_AndKeepsAllRows()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithoutAllowSubfolders);
        database.Execute(@"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES ('Code', 'C:\Apps\Code.exe'), ('Notepad', 'C:\Windows\System32\notepad.exe');
INSERT INTO allowed_folders(display_name, folder_path)
VALUES ('Study', 'C:\Study'), ('Notes', 'D:\Notes');
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES ('Wiki', 'wikipedia.org'), ('Docs', '*.microsoft.com');
UPDATE settings SET value = '1' WHERE key = 'study_mode_enabled';");

        using (var sut = database.CreateStore(WindowsRootFolder))
        {
            var config = await sut.LoadAsync();

            Assert.Equal(
                [new AllowedApp("Code", @"C:\Apps\Code.exe"), new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe")],
                config.AllowedApps);
            Assert.Equal(
                [WindowsRootFolder, new AllowedFolder("Study", @"C:\Study"), new AllowedFolder("Notes", @"D:\Notes")],
                config.AllowedFolders);
            Assert.Equal(
                [new AllowedSite("Wiki", "wikipedia.org"), new AllowedSite("Docs", "*.microsoft.com")],
                config.AllowedSites);
            Assert.True(await sut.IsStudyModeEnabledAsync());
        }

        // Opening only adds the column: no rows are dropped or added.
        Assert.Equal(2, database.QueryLong("SELECT COUNT(*) FROM allowed_apps;"));
        Assert.Equal(2, database.QueryLong("SELECT COUNT(*) FROM allowed_folders;"));
        Assert.Equal(2, database.QueryLong("SELECT COUNT(*) FROM allowed_sites;"));
        Assert.Equal(0, database.QueryLong("SELECT COUNT(*) FROM allowed_folders WHERE allow_subfolders <> 0;"));
        Assert.Equal(DescribePreviousSchema(), database.DescribeSchema());
    }

    [Fact]
    public async Task MigratedDatabase_RoundTripsAllowSubfolders()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithoutAllowSubfolders);

        using var sut = database.CreateStore(WindowsRootFolder);
        await sut.SaveAsync(new WhitelistConfig
        {
            AllowedFolders = [new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true)]
        });

        var loaded = await sut.LoadAsync();

        Assert.Equal(
            [WindowsRootFolder, new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true)],
            loaded.AllowedFolders);
    }

    [Fact]
    public async Task OpensPreviousSqliteDatabase_ReadsAllRows_WithoutModifyingTheFile()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithAllowSubfolders);

        // Rows as the last SQLite store saved them: the default root folder first, then user folders.
        database.Execute(@"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES ('Code', 'C:\Apps\Code.exe');
INSERT INTO allowed_folders(display_name, folder_path, allow_subfolders)
VALUES ('System Root (C:)', 'C:\', 0), ('Study', 'C:\Study', 1), ('Notes', 'D:\Notes', 0);
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES ('Wiki', 'wikipedia.org');
UPDATE settings SET value = '1' WHERE key = 'study_mode_enabled';");
        var hashBefore = database.ComputeFileHash();

        WhitelistConfig config;
        bool studyMode;
        using (var sut = database.CreateStore(WindowsRootFolder))
        {
            config = await sut.LoadAsync();
            studyMode = await sut.IsStudyModeEnabledAsync();
        }

        Assert.Equal([new AllowedApp("Code", @"C:\Apps\Code.exe")], config.AllowedApps);
        Assert.Equal(
            [
                WindowsRootFolder,
                new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true),
                new AllowedFolder("Notes", @"D:\Notes")
            ],
            config.AllowedFolders);
        Assert.Equal([new AllowedSite("Wiki", "wikipedia.org")], config.AllowedSites);
        Assert.True(studyMode);
        Assert.Equal(hashBefore, database.ComputeFileHash());
    }

    [Fact]
    public async Task SchemaInitialization_IsIdempotent_AcrossRepeatedOpens()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithoutAllowSubfolders);
        database.Execute(@"
INSERT INTO allowed_folders(display_name, folder_path)
VALUES ('Study', 'C:\Study');");

        for (var i = 0; i < 3; i++)
        {
            using var sut = database.CreateStore();
            Assert.Equal([new AllowedFolder("Study", @"C:\Study")], (await sut.LoadAsync()).AllowedFolders);
        }

        Assert.Equal(1, database.QueryLong("SELECT COUNT(*) FROM settings;"));
        Assert.Equal(1, database.QueryLong("SELECT COUNT(*) FROM allowed_folders;"));
        Assert.Equal(DescribePreviousSchema(), database.DescribeSchema());
    }

    [Fact]
    public async Task FailedMigration_LeavesDatabaseUnchanged_AndIsRetriedOnNextUse()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithoutAllowSubfolders);

        // The trigger makes the last step of schema initialization fail after the column was added.
        database.Execute(@"
DELETE FROM settings;
INSERT INTO allowed_folders(display_name, folder_path)
VALUES ('Study', 'C:\Study');
CREATE TRIGGER fail_settings_insert BEFORE INSERT ON settings
BEGIN
    SELECT RAISE(ABORT, 'simulated failure');
END;");
        var schemaBefore = database.DescribeSchema();

        using var sut = database.CreateStore();

        await Assert.ThrowsAsync<SqliteException>(() => sut.LoadAsync());
        Assert.Equal(schemaBefore, database.DescribeSchema());
        Assert.Equal(1, database.QueryLong("SELECT COUNT(*) FROM allowed_folders;"));

        database.Execute("DROP TRIGGER fail_settings_insert;");

        Assert.Equal([new AllowedFolder("Study", @"C:\Study")], (await sut.LoadAsync()).AllowedFolders);
        Assert.Equal(DescribePreviousSchema(), database.DescribeSchema());
    }

    [Fact]
    public async Task OpensDatabaseWithoutStudyModeSetting_ReturnsDisabled_AndAddsSetting()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithAllowSubfolders);
        database.Execute("DELETE FROM settings;");

        using var sut = database.CreateStore();

        Assert.False(await sut.IsStudyModeEnabledAsync());
        Assert.Equal(1, database.QueryLong(
            "SELECT COUNT(*) FROM settings WHERE key = 'study_mode_enabled' AND value = '0';"));
    }

    [Fact]
    public async Task OpensEmptyDatabaseFile_CreatesSchema()
    {
        using var database = new TempDatabase();
        File.WriteAllBytes(database.DatabasePath, []);

        using (var sut = database.CreateStore())
        {
            await sut.SetStudyModeEnabledAsync(true);
            Assert.True(await sut.IsStudyModeEnabledAsync());
        }

        Assert.Equal(DescribePreviousSchema(), database.DescribeSchema());
    }

    [Fact]
    public async Task NewDatabase_HasSchemaOfPreviousSqliteStore()
    {
        using var database = new TempDatabase();

        using (var sut = database.CreateStore())
        {
            await sut.LoadAsync();
        }

        Assert.Equal(DescribePreviousSchema(), database.DescribeSchema());
    }

    [Fact]
    public async Task Data_PersistsAcrossStoreInstances()
    {
        using var database = new TempDatabase();
        var saved = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Code", @"C:\Apps\Code.exe")],
            AllowedFolders = [new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true)],
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        };

        using (var beforeRestart = database.CreateStore(WindowsRootFolder))
        {
            await beforeRestart.SaveAsync(saved);
            await beforeRestart.SetStudyModeEnabledAsync(true);
        }

        using var afterRestart = database.CreateStore(WindowsRootFolder);
        var loaded = await afterRestart.LoadAsync();

        Assert.Equal(saved.AllowedApps, loaded.AllowedApps);
        Assert.Equal([WindowsRootFolder, .. saved.AllowedFolders], loaded.AllowedFolders);
        Assert.Equal(saved.AllowedSites, loaded.AllowedSites);
        Assert.True(await afterRestart.IsStudyModeEnabledAsync());
    }

    [Fact]
    public async Task FirstUse_CreatesMissingDatabaseDirectory()
    {
        using var database = new TempDatabase();
        var databasePath = Path.Combine(database.DirectoryPath, "nested", "FocusManager", "focusmanager.db");

        using (var sut = new SqliteWhitelistStore(new SqliteWhitelistStoreOptions { DatabasePath = databasePath }))
        {
            await sut.SetStudyModeEnabledAsync(true);
        }

        Assert.True(File.Exists(databasePath));

        using var reopened = new SqliteWhitelistStore(new SqliteWhitelistStoreOptions { DatabasePath = databasePath });
        Assert.True(await reopened.IsStudyModeEnabledAsync());
    }

    private static IReadOnlyList<string> DescribePreviousSchema()
    {
        using var previous = new TempDatabase();
        previous.Execute(LegacySchemas.WithAllowSubfolders);
        return previous.DescribeSchema();
    }
}
