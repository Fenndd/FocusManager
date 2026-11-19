using FocusManager.Core.Models;
using Xunit;

namespace FocusManager.Persistence.Sqlite.Tests;

/// <summary>
/// The agent's restart lifecycle at storage level (T055): a database written by an earlier store is
/// opened by one agent run, edited through the same load/save calls the IPC handler makes, closed, and
/// reopened by later runs. Each <c>using</c> scope stands for one agent process; disposing the store
/// releases the database file as process exit does. This does not replace the real-agent lifecycle
/// evidence that T055 still needs from an isolated Windows environment.
/// </summary>
public sealed class SqliteWhitelistStoreRestartLifecycleTests
{
    // The default folder the Windows agent passes in Program.cs (FLD-004).
    private static readonly AllowedFolder WindowsRootFolder = new("System Root (C:)", @"C:\", AllowSubfolders: false);

    [Fact]
    public async Task MigratedLegacyDatabase_KeepsRowsAndFlags_AcrossSaveAndLaterRestarts()
    {
        using var database = new TempDatabase();
        database.Execute(LegacySchemas.WithoutAllowSubfolders);
        database.Execute(@"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES ('Code', 'C:\Apps\Code.exe');
INSERT INTO allowed_folders(display_name, folder_path)
VALUES ('System Root (C:)', 'C:\'), ('Study', 'C:\Study'), ('Notes', 'C:\Notes');
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES ('Wiki', 'wikipedia.org');");

        // Run 1: open the legacy file, read it as the UI does, then add values and save them.
        using (var firstRun = database.CreateStore(WindowsRootFolder))
        {
            var config = await firstRun.LoadAsync();

            // Rows of the legacy file read with the column default (false); nothing is dropped.
            Assert.Equal(
                [
                    WindowsRootFolder,
                    new AllowedFolder("Study", @"C:\Study"),
                    new AllowedFolder("Notes", @"C:\Notes")
                ],
                config.AllowedFolders);

            config.AllowedFolders.Add(new AllowedFolder("Lectures", @"C:\Lectures", AllowSubfolders: true));
            config.AllowedApps.Add(new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe"));
            config.AllowedSites.Add(new AllowedSite("Docs", "*.microsoft.com"));
            await firstRun.SaveAsync(config);
        }

        AllowedFolder[] expectedFolders =
        [
            WindowsRootFolder,
            new AllowedFolder("Study", @"C:\Study"),
            new AllowedFolder("Notes", @"C:\Notes"),
            new AllowedFolder("Lectures", @"C:\Lectures", AllowSubfolders: true)
        ];
        AllowedApp[] expectedApps =
        [
            new AllowedApp("Code", @"C:\Apps\Code.exe"),
            new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe")
        ];
        AllowedSite[] expectedSites =
        [
            new AllowedSite("Wiki", "wikipedia.org"),
            new AllowedSite("Docs", "*.microsoft.com")
        ];
        var hashAfterSave = database.ComputeFileHash();

        // Runs 2 and 3: later restarts read the saved values and leave the file byte-identical.
        for (var run = 2; run <= 3; run++)
        {
            using (var laterRun = database.CreateStore(WindowsRootFolder))
            {
                var loaded = await laterRun.LoadAsync();

                Assert.Equal(expectedApps, loaded.AllowedApps);
                Assert.Equal(expectedFolders, loaded.AllowedFolders);
                Assert.Equal(expectedSites, loaded.AllowedSites);
                Assert.False(await laterRun.IsStudyModeEnabledAsync());
            }

            // Hashed after the store released the file, as the existing migration tests do.
            Assert.Equal(hashAfterSave, database.ComputeFileHash());
        }

        using var currentSchema = new TempDatabase();
        currentSchema.Execute(LegacySchemas.WithAllowSubfolders);
        Assert.Equal(currentSchema.DescribeSchema(), database.DescribeSchema());
    }
}
