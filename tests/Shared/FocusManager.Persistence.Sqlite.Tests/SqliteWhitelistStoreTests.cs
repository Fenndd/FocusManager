using System.Collections.Concurrent;
using FocusManager.Core.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FocusManager.Persistence.Sqlite.Tests;

public sealed class SqliteWhitelistStoreTests
{
    // A host-supplied default such as FLD-004; the file-system root keeps the tests OS-independent.
    private static readonly AllowedFolder RootFolder =
        new("System Root", Path.GetPathRoot(Path.GetTempPath())!, AllowSubfolders: false);

    [Fact]
    public async Task LoadAsync_ReturnsDefaultFolders_AndStudyModeDisabled_ByDefault()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore(RootFolder);

        var config = await sut.LoadAsync();
        var studyMode = await sut.IsStudyModeEnabledAsync();

        Assert.Empty(config.AllowedApps);
        Assert.Equal([RootFolder], config.AllowedFolders);
        Assert.Empty(config.AllowedSites);
        Assert.False(studyMode);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsAllLists()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore(RootFolder);

        var saved = new WhitelistConfig
        {
            AllowedApps =
            [
                new AllowedApp("Code", @"C:\Apps\Code.exe"),
                new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe")
            ],
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true),
                new AllowedFolder("Notes", @"C:\Notes", AllowSubfolders: false)
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
        Assert.Equal([RootFolder, .. saved.AllowedFolders], loaded.AllowedFolders);
        Assert.Equal(saved.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public async Task SaveAsync_ReplacesPreviousValues()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore(RootFolder);

        var first = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Code", @"C:\Apps\Code.exe")],
            AllowedFolders = [new AllowedFolder("Study", @"C:\Study")],
            AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")]
        };

        var second = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe")],
            AllowedFolders = [new AllowedFolder("Projects", @"C:\Projects")],
            AllowedSites = [new AllowedSite("Docs", "learn.microsoft.com")]
        };

        await sut.SaveAsync(first);
        await sut.SaveAsync(second);

        var loaded = await sut.LoadAsync();

        Assert.Equal(second.AllowedApps, loaded.AllowedApps);
        Assert.Equal([RootFolder, .. second.AllowedFolders], loaded.AllowedFolders);
        Assert.Equal(second.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public async Task SaveAsync_RejectsDuplicatesDifferingOnlyByCase_AndKeepsPreviousValues()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore(RootFolder);

        var original = new WhitelistConfig
        {
            AllowedApps = [new AllowedApp("Code", @"C:\Apps\Code.exe")],
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

        var exception = await Assert.ThrowsAsync<SqliteException>(() => sut.SaveAsync(duplicate));
        Assert.Equal(19, exception.SqliteErrorCode); // SQLITE_CONSTRAINT
        Assert.Equal(2067, exception.SqliteExtendedErrorCode); // SQLITE_CONSTRAINT_UNIQUE

        var loaded = await sut.LoadAsync();
        Assert.Equal(original.AllowedApps, loaded.AllowedApps);
        Assert.Equal([RootFolder], loaded.AllowedFolders);
        Assert.Equal(original.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public async Task SetStudyModeEnabledAsync_PersistsAcrossInstances()
    {
        using var database = new TempDatabase();

        using (var writer = database.CreateStore())
        {
            await writer.SetStudyModeEnabledAsync(true);
        }

        using (var reader = database.CreateStore())
        {
            Assert.True(await reader.IsStudyModeEnabledAsync());
            await reader.SetStudyModeEnabledAsync(false);
        }

        using var verifier = database.CreateStore();
        Assert.False(await verifier.IsStudyModeEnabledAsync());
    }

    [Fact]
    public async Task SaveAsync_Throws_WhenConfigIsNull()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore();

#pragma warning disable CS8625
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SaveAsync(null));
#pragma warning restore CS8625
    }

    [Fact]
    public async Task SaveAsync_WithCanceledToken_Throws_AndKeepsPreviousValues()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore();

        var original = new WhitelistConfig { AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")] };
        await sut.SaveAsync(original);

        using var canceled = new CancellationTokenSource();
        canceled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.SaveAsync(new WhitelistConfig(), canceled.Token));

        var loaded = await sut.LoadAsync();
        Assert.Equal(original.AllowedSites, loaded.AllowedSites);
    }

    [Fact]
    public void Constructor_Throws_WhenOptionsAreNull()
    {
#pragma warning disable CS8625
        Assert.Throws<ArgumentNullException>(() => new SqliteWhitelistStore(null));
#pragma warning restore CS8625
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Throws_WhenDatabasePathIsEmpty(string? databasePath)
    {
        Assert.ThrowsAny<ArgumentException>(() => new SqliteWhitelistStore(new SqliteWhitelistStoreOptions
        {
            DatabasePath = databasePath!
        }));
    }

    [Fact]
    public void Constructor_Throws_WhenDefaultFoldersAreNull()
    {
        using var database = new TempDatabase();

        Assert.Throws<ArgumentNullException>(() => new SqliteWhitelistStore(new SqliteWhitelistStoreOptions
        {
            DatabasePath = database.DatabasePath,
            DefaultFolders = null!
        }));
    }

    [Fact]
    public void Constructor_Throws_WhenDefaultFolderIsInvalid()
    {
        using var database = new TempDatabase();

        Assert.Throws<ArgumentException>(() => database.CreateStore(new AllowedFolder[] { null! }));
        Assert.Throws<ArgumentException>(() => database.CreateStore(new AllowedFolder("Empty", " ")));
        Assert.Throws<ArgumentException>(() => database.CreateStore(new AllowedFolder(null!, RootFolder.FolderPath)));
    }

    [Fact]
    public void Constructor_DoesNotCreateDatabase()
    {
        using var database = new TempDatabase();
        var databasePath = Path.Combine(database.DirectoryPath, "nested", "focusmanager.db");

        using var sut = new SqliteWhitelistStore(new SqliteWhitelistStoreOptions { DatabasePath = databasePath });

        Assert.False(Directory.Exists(Path.GetDirectoryName(databasePath)));
    }

    [Fact]
    public async Task LoadAsync_AddsNoFolders_WhenNoDefaultFoldersAreConfigured()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore();

        Assert.Empty((await sut.LoadAsync()).AllowedFolders);

        var study = new AllowedFolder("Study", Path.Combine(database.DirectoryPath, "Study"));
        await sut.SaveAsync(new WhitelistConfig { AllowedFolders = [study] });

        Assert.Equal([study], (await sut.LoadAsync()).AllowedFolders);
        Assert.Equal(1, database.QueryLong("SELECT COUNT(*) FROM allowed_folders;"));
    }

    [Fact]
    public async Task LoadAsync_InsertsMissingDefaultFolders_InConfiguredOrder_BeforeOtherFolders()
    {
        using var database = new TempDatabase();
        var firstDefault = new AllowedFolder("First", Path.Combine(database.DirectoryPath, "First"));
        var secondDefault = new AllowedFolder("Second", Path.Combine(database.DirectoryPath, "Second"));
        var study = new AllowedFolder("Study", Path.Combine(database.DirectoryPath, "Study"), AllowSubfolders: true);

        using (var withoutDefaults = database.CreateStore())
        {
            await withoutDefaults.SaveAsync(new WhitelistConfig { AllowedFolders = [study] });
        }

        using var sut = database.CreateStore(firstDefault, secondDefault);

        Assert.Equal([firstDefault, secondDefault, study], (await sut.LoadAsync()).AllowedFolders);
    }

    [Fact]
    public async Task SaveAsync_PersistsDefaultFolders()
    {
        using var database = new TempDatabase();
        var study = new AllowedFolder("Study", Path.Combine(database.DirectoryPath, "Study"));

        using (var sut = database.CreateStore(RootFolder))
        {
            await sut.SaveAsync(new WhitelistConfig { AllowedFolders = [study] });
        }

        using var withoutDefaults = database.CreateStore();
        Assert.Equal([RootFolder, study], (await withoutDefaults.LoadAsync()).AllowedFolders);
    }

    [Fact]
    public async Task SaveAsync_KeepsUserFolder_WhenItMatchesDefaultFolderIgnoringCaseAndTrailingSeparator()
    {
        // folder_path is unique ignoring case (COLLATE NOCASE), so the default must not be added again.
        using var database = new TempDatabase();
        var defaultFolder = new AllowedFolder("Default", Path.Combine(database.DirectoryPath, "Default"));
        var userFolder = new AllowedFolder(
            "Mine",
            Path.Combine(database.DirectoryPath, "DEFAULT") + Path.DirectorySeparatorChar,
            AllowSubfolders: true);

        using var sut = database.CreateStore(defaultFolder);
        await sut.SaveAsync(new WhitelistConfig { AllowedFolders = [userFolder] });

        Assert.Equal([userFolder], (await sut.LoadAsync()).AllowedFolders);
    }

    [Fact]
    public async Task DefaultFolders_AreCopied_WhenStoreIsCreated()
    {
        using var database = new TempDatabase();
        var defaultFolders = new List<AllowedFolder> { RootFolder };

        using var sut = new SqliteWhitelistStore(new SqliteWhitelistStoreOptions
        {
            DatabasePath = database.DatabasePath,
            DefaultFolders = defaultFolders
        });
        defaultFolders.Clear();

        Assert.Equal([RootFolder], (await sut.LoadAsync()).AllowedFolders);
    }

    [Fact]
    public async Task ConcurrentOperations_OnOneInstance_Succeed_AndNeverMixSavedConfigs()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore();

        var configs = Enumerable.Range(0, 8)
            .Select(i => new WhitelistConfig
            {
                AllowedApps = [new AllowedApp($"App {i}", $"app-{i}.exe")],
                AllowedFolders = [new AllowedFolder($"Folder {i}", Path.Combine(database.DirectoryPath, $"Folder{i}"))],
                AllowedSites = [new AllowedSite($"Site {i}", $"site-{i}.example")]
            })
            .ToList();

        var loads = new ConcurrentBag<WhitelistConfig>();
        var operations = configs.SelectMany((config, i) => new[]
        {
            Task.Run(() => sut.SaveAsync(config)),
            Task.Run(async () => loads.Add(await sut.LoadAsync())),
            Task.Run(() => sut.SetStudyModeEnabledAsync(i % 2 == 0)),
            Task.Run(() => sut.IsStudyModeEnabledAsync())
        });

        await Task.WhenAll(operations);
        loads.Add(await sut.LoadAsync());

        foreach (var loaded in loads)
        {
            var isEmpty = loaded.AllowedApps.Count == 0 && loaded.AllowedFolders.Count == 0 && loaded.AllowedSites.Count == 0;
            Assert.True(isEmpty || configs.Any(x => IsSameConfig(x, loaded)), "A load returned lists from different saves.");
        }
    }

    [Fact]
    public async Task ConcurrentFirstUse_FromTwoInstances_Succeeds_AndInitializesSchemaOnce()
    {
        using var database = new TempDatabase();
        using var first = database.CreateStore();
        using var second = database.CreateStore();

        var a = new WhitelistConfig { AllowedSites = [new AllowedSite("A", "a.example")] };
        var b = new WhitelistConfig { AllowedSites = [new AllowedSite("B", "b.example")] };

        await Task.WhenAll(Task.Run(() => first.SaveAsync(a)), Task.Run(() => second.SaveAsync(b)));

        var loaded = await first.LoadAsync();
        Assert.True(IsSameConfig(a, loaded) || IsSameConfig(b, loaded));
        Assert.Equal(1, database.QueryLong("SELECT COUNT(*) FROM settings;"));
        Assert.Equal(1, database.QueryLong(
            "SELECT COUNT(*) FROM pragma_table_info('allowed_folders') WHERE name = 'allow_subfolders';"));
    }

    [Fact]
    public async Task SaveAsync_WaitsForWriteLockHeldByAnotherConnection()
    {
        using var database = new TempDatabase();
        using var sut = database.CreateStore();
        await sut.LoadAsync();

        var saved = new WhitelistConfig { AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")] };
        Task save;

        using (var other = database.OpenRawConnection())
        using (var writeLock = other.BeginTransaction(deferred: false))
        {
            save = Task.Run(() => sut.SaveAsync(saved));

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.False(save.IsCompleted, "The save must wait for the lock instead of failing or bypassing it.");

            writeLock.Rollback();
        }

        await save.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(saved.AllowedSites, (await sut.LoadAsync()).AllowedSites);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispose_ReleasesDatabaseFile(bool disposeAsynchronously)
    {
        using var database = new TempDatabase();
        var sut = database.CreateStore(RootFolder);
        await sut.SaveAsync(new WhitelistConfig { AllowedSites = [new AllowedSite("Wiki", "wikipedia.org")] });
        await sut.LoadAsync();

        if (disposeAsynchronously)
        {
            await sut.DisposeAsync();
        }
        else
        {
            sut.Dispose();
        }

        // On Windows this fails while a pooled connection still holds the file open.
        File.Delete(database.DatabasePath);
        Assert.False(File.Exists(database.DatabasePath));
    }

    [Fact]
    public async Task Operations_Throw_AfterDispose()
    {
        using var database = new TempDatabase();
        var sut = database.CreateStore();
        await sut.LoadAsync();

        sut.Dispose();
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.LoadAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.SaveAsync(new WhitelistConfig()));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.IsStudyModeEnabledAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.SetStudyModeEnabledAsync(true));
    }

    private static bool IsSameConfig(WhitelistConfig expected, WhitelistConfig actual)
    {
        return expected.AllowedApps.SequenceEqual(actual.AllowedApps)
            && expected.AllowedFolders.SequenceEqual(actual.AllowedFolders)
            && expected.AllowedSites.SequenceEqual(actual.AllowedSites);
    }
}
