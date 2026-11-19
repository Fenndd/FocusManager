using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using Microsoft.Data.Sqlite;

namespace FocusManager.Persistence.Sqlite;

/// <summary>
/// Stores the allow-lists and the study mode flag in a local SQLite database (ADR-0006).
/// </summary>
/// <remarks>
/// The schema is the one written by the agent before 2026-09-30, so existing databases open without
/// manual steps. The only migration adds <c>allowed_folders.allow_subfolders</c>, which databases
/// created before 2026-04-20 lack. Only the agent process opens the database (the UI talks to the
/// agent over IPC), so the default rollback journal is kept and the legacy file format is unchanged.
/// Operations on one instance are serialized; other connections to the same file are serialized by
/// SQLite locks, waiting up to 30 seconds for a lock.
/// </remarks>
public sealed class SqliteWhitelistStore : IWhitelistStore, IAsyncDisposable, IDisposable
{
    private const string StudyModeKey = "study_mode_enabled";

    private const int BusyTimeoutSeconds = 30;

    // Tables of the pre-2026-09-30 store, with the same column definitions.
    private const string CreateTablesSql = @"
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
    folder_path TEXT NOT NULL UNIQUE COLLATE NOCASE,
    allow_subfolders INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS allowed_sites (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL,
    host_pattern TEXT NOT NULL UNIQUE COLLATE NOCASE
);";

    private const string CountAllowSubfoldersColumnSql = @"
SELECT COUNT(*)
FROM pragma_table_info('allowed_folders')
WHERE name = 'allow_subfolders' COLLATE NOCASE;";

    private const string AddAllowSubfoldersColumnSql =
        "ALTER TABLE allowed_folders ADD COLUMN allow_subfolders INTEGER NOT NULL DEFAULT 0;";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly AllowedFolder[] _defaultFolders;

    // Guarded by _gate.
    private bool _schemaInitialized;
    private bool _disposed;

    public SqliteWhitelistStore(SqliteWhitelistStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DatabasePath);
        ArgumentNullException.ThrowIfNull(options.DefaultFolders);

        foreach (var folder in options.DefaultFolders)
        {
            if (folder?.DisplayName is null || string.IsNullOrWhiteSpace(folder.FolderPath))
            {
                throw new ArgumentException(
                    "Every default folder must have a display name and a folder path.",
                    nameof(options));
            }
        }

        _databasePath = Path.GetFullPath(options.DatabasePath.Trim());
        _defaultFolders = [.. options.DefaultFolders];
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = BusyTimeoutSeconds
        }.ToString();
    }

    public async Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);

            // A deferred transaction reads the three tables as one snapshot without taking a write lock.
            await using var transaction = connection.BeginTransaction(deferred: true);

            var apps = await ReadAllowedAppsAsync(connection, transaction, cancellationToken);
            var folders = EnsureDefaultFolders(await ReadAllowedFoldersAsync(connection, transaction, cancellationToken));
            var sites = await ReadAllowedSitesAsync(connection, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new WhitelistConfig
            {
                AllowedApps = apps,
                AllowedFolders = folders,
                AllowedSites = sites
            };
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);

            // All lists are replaced in one write transaction: a failed save keeps the previous values.
            await using var transaction = connection.BeginTransaction(deferred: false);

            await ExecuteNonQueryAsync(
                connection,
                transaction,
                "DELETE FROM allowed_apps; DELETE FROM allowed_folders; DELETE FROM allowed_sites;",
                cancellationToken);

            await using (var command = CreateCommand(connection, transaction, @"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES ($display_name, $executable_path);"))
            {
                var displayName = command.Parameters.Add("$display_name", SqliteType.Text);
                var executablePath = command.Parameters.Add("$executable_path", SqliteType.Text);

                foreach (var app in config.AllowedApps)
                {
                    displayName.Value = app.DisplayName;
                    executablePath.Value = app.ExecutablePath;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await using (var command = CreateCommand(connection, transaction, @"
INSERT INTO allowed_folders(display_name, folder_path, allow_subfolders)
VALUES ($display_name, $folder_path, $allow_subfolders);"))
            {
                var displayName = command.Parameters.Add("$display_name", SqliteType.Text);
                var folderPath = command.Parameters.Add("$folder_path", SqliteType.Text);
                var allowSubfolders = command.Parameters.Add("$allow_subfolders", SqliteType.Integer);

                foreach (var folder in EnsureDefaultFolders(config.AllowedFolders))
                {
                    displayName.Value = folder.DisplayName;
                    folderPath.Value = folder.FolderPath;
                    allowSubfolders.Value = folder.AllowSubfolders ? 1 : 0;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await using (var command = CreateCommand(connection, transaction, @"
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES ($display_name, $host_pattern);"))
            {
                var displayName = command.Parameters.Add("$display_name", SqliteType.Text);
                var hostPattern = command.Parameters.Add("$host_pattern", SqliteType.Text);

                foreach (var site in config.AllowedSites)
                {
                    displayName.Value = site.DisplayName;
                    hostPattern.Value = site.HostPattern;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> IsStudyModeEnabledAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = CreateCommand(
                connection,
                transaction: null,
                "SELECT value FROM settings WHERE key = $key LIMIT 1;");
            command.Parameters.AddWithValue("$key", StudyModeKey);

            var value = await command.ExecuteScalarAsync(cancellationToken);
            if (value is null)
            {
                return false;
            }

            var text = Convert.ToString(value) ?? "0";
            return text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetStudyModeEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = CreateCommand(connection, transaction: null, @"
INSERT INTO settings(key, value)
VALUES ($key, $value)
ON CONFLICT(key) DO UPDATE SET value = excluded.value;");
            command.Parameters.AddWithValue("$key", StudyModeKey);
            command.Parameters.AddWithValue("$value", enabled ? "1" : "0");

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _gate.Wait();
        try
        {
            ReleaseDatabaseFile();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            ReleaseDatabaseFile();
        }
        finally
        {
            _gate.Release();
        }
    }

    // Must be called while holding _gate.
    private void ReleaseDatabaseFile()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Pooled connections keep the file open; clearing the pool lets it be moved, deleted or backed up.
        using var connection = new SqliteConnection(_connectionString);
        SqliteConnection.ClearPool(connection);
    }

    // Must be called while holding _gate.
    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_schemaInitialized)
        {
            // SQLite creates a missing database file, but not its directory.
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var connection = new SqliteConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);

            if (!_schemaInitialized)
            {
                await InitializeSchemaAsync(connection, cancellationToken);
                _schemaInitialized = true;
            }

            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static async Task InitializeSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        // A write (IMMEDIATE) transaction serializes concurrent first use from several connections
        // or processes, and a failure leaves the database exactly as it was.
        await using var transaction = connection.BeginTransaction(deferred: false);

        await ExecuteNonQueryAsync(connection, transaction, CreateTablesSql, cancellationToken);

        // Existing rows of a database created before 2026-04-20 get the column default 0 (false).
        await using (var command = CreateCommand(connection, transaction, CountAllowSubfoldersColumnSql))
        {
            var columnCount = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
            if (columnCount == 0)
            {
                await ExecuteNonQueryAsync(connection, transaction, AddAllowSubfoldersColumnSql, cancellationToken);
            }
        }

        await using (var command = CreateCommand(connection, transaction, @"
INSERT INTO settings(key, value)
VALUES ($key, '0')
ON CONFLICT(key) DO NOTHING;"))
        {
            command.Parameters.AddWithValue("$key", StudyModeKey);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static SqliteCommand CreateCommand(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, sql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<List<AllowedApp>> ReadAllowedAppsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
SELECT display_name, executable_path
FROM allowed_apps
ORDER BY id;");

        var result = new List<AllowedApp>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AllowedApp(
                reader.GetString(0),
                reader.GetString(1)));
        }

        return result;
    }

    private static async Task<List<AllowedFolder>> ReadAllowedFoldersAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
SELECT display_name, folder_path, allow_subfolders
FROM allowed_folders
ORDER BY id;");

        var result = new List<AllowedFolder>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AllowedFolder(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2) != 0));
        }

        return result;
    }

    private static async Task<List<AllowedSite>> ReadAllowedSitesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
SELECT display_name, host_pattern
FROM allowed_sites
ORDER BY id;");

        var result = new List<AllowedSite>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AllowedSite(
                reader.GetString(0),
                reader.GetString(1)));
        }

        return result;
    }

    // Each configured default folder that the list does not contain is inserted at the start, in order.
    private List<AllowedFolder> EnsureDefaultFolders(IEnumerable<AllowedFolder> folders)
    {
        var result = folders.ToList();
        var insertIndex = 0;

        foreach (var defaultFolder in _defaultFolders)
        {
            var defaultPath = NormalizeFolderPath(defaultFolder.FolderPath);
            var isPresent = result.Any(x =>
                string.Equals(NormalizeFolderPath(x.FolderPath), defaultPath, StringComparison.OrdinalIgnoreCase));

            if (!isPresent)
            {
                result.Insert(insertIndex++, defaultFolder);
            }
        }

        return result;
    }

    private static string NormalizeFolderPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');

        try
        {
            cleaned = Path.GetFullPath(cleaned);
        }
        catch
        {
            // Keep raw path if normalization fails.
        }

        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
