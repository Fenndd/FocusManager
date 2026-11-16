using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using Npgsql;

namespace FocusManager.Persistence.Postgres;

public sealed class PostgresWhitelistStore : IWhitelistStore, IAsyncDisposable, IDisposable
{
    private const string StudyModeKey = "study_mode_enabled";

    // Serializes concurrent schema creation from several store instances/processes.
    private const long SchemaLockKey = 0x466F6375734D6772; // "FocusMgr"

    private static readonly AllowedFolder DefaultRootFolder = new("System Root (C:)", @"C:\", false);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly NpgsqlDataSource _dataSource;

    private bool _schemaInitialized;

    public PostgresWhitelistStore(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);

            var apps = await ReadAllowedAppsAsync(connection, cancellationToken);
            var folders = EnsureDefaultFolders(await ReadAllowedFoldersAsync(connection, cancellationToken));
            var sites = await ReadAllowedSitesAsync(connection, cancellationToken);

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
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await ExecuteNonQueryAsync(
                connection,
                transaction,
                "DELETE FROM allowed_apps; DELETE FROM allowed_folders; DELETE FROM allowed_sites;",
                cancellationToken);

            foreach (var app in config.AllowedApps)
            {
                await using var command = new NpgsqlCommand(@"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES (@display_name, @executable_path);", connection, transaction);
                command.Parameters.AddWithValue("display_name", app.DisplayName);
                command.Parameters.AddWithValue("executable_path", app.ExecutablePath);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            var foldersToSave = EnsureDefaultFolders(config.AllowedFolders);
            foreach (var folder in foldersToSave)
            {
                await using var command = new NpgsqlCommand(@"
INSERT INTO allowed_folders(display_name, folder_path, allow_subfolders)
VALUES (@display_name, @folder_path, @allow_subfolders);", connection, transaction);
                command.Parameters.AddWithValue("display_name", folder.DisplayName);
                command.Parameters.AddWithValue("folder_path", folder.FolderPath);
                command.Parameters.AddWithValue("allow_subfolders", folder.AllowSubfolders);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var site in config.AllowedSites)
            {
                await using var command = new NpgsqlCommand(@"
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES (@display_name, @host_pattern);", connection, transaction);
                command.Parameters.AddWithValue("display_name", site.DisplayName);
                command.Parameters.AddWithValue("host_pattern", site.HostPattern);
                await command.ExecuteNonQueryAsync(cancellationToken);
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
            await using var command = new NpgsqlCommand(
                "SELECT value FROM settings WHERE key = @key LIMIT 1;",
                connection);
            command.Parameters.AddWithValue("key", StudyModeKey);

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
            await using var command = new NpgsqlCommand(@"
INSERT INTO settings(key, value)
VALUES (@key, @value)
ON CONFLICT (key) DO UPDATE SET value = excluded.value;", connection);
            command.Parameters.AddWithValue("key", StudyModeKey);
            command.Parameters.AddWithValue("value", enabled ? "1" : "0");

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _gate.Dispose();
        return _dataSource.DisposeAsync();
    }

    public void Dispose()
    {
        _gate.Dispose();
        _dataSource.Dispose();
    }

    // Must be called while holding _gate.
    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        if (_schemaInitialized)
        {
            return connection;
        }

        try
        {
            await InitializeSchemaAsync(connection, cancellationToken);
            _schemaInitialized = true;
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static async Task InitializeSchemaAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var lockCommand = new NpgsqlCommand("SELECT pg_advisory_xact_lock(@key);", connection, transaction))
        {
            lockCommand.Parameters.AddWithValue("key", SchemaLockKey);
            await lockCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        // Uniqueness is case-insensitive, matching Windows path and host semantics.
        await ExecuteNonQueryAsync(connection, transaction, @"
CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS allowed_apps (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name TEXT NOT NULL,
    executable_path TEXT NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_allowed_apps_executable_path ON allowed_apps (lower(executable_path));

CREATE TABLE IF NOT EXISTS allowed_folders (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name TEXT NOT NULL,
    folder_path TEXT NOT NULL,
    allow_subfolders BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_allowed_folders_folder_path ON allowed_folders (lower(folder_path));

CREATE TABLE IF NOT EXISTS allowed_sites (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name TEXT NOT NULL,
    host_pattern TEXT NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_allowed_sites_host_pattern ON allowed_sites (lower(host_pattern));

INSERT INTO settings(key, value)
VALUES ('study_mode_enabled', '0')
ON CONFLICT (key) DO NOTHING;", cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<List<AllowedApp>> ReadAllowedAppsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(@"
SELECT display_name, executable_path
FROM allowed_apps
ORDER BY id;", connection);

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
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(@"
SELECT display_name, folder_path, allow_subfolders
FROM allowed_folders
ORDER BY id;", connection);

        var result = new List<AllowedFolder>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AllowedFolder(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetBoolean(2)));
        }

        return result;
    }

    private static async Task<List<AllowedSite>> ReadAllowedSitesAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(@"
SELECT display_name, host_pattern
FROM allowed_sites
ORDER BY id;", connection);

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

    private static List<AllowedFolder> EnsureDefaultFolders(IEnumerable<AllowedFolder> folders)
    {
        var result = folders.ToList();

        var hasRootFolder = result.Any(x =>
            string.Equals(NormalizeFolderPath(x.FolderPath), NormalizeFolderPath(DefaultRootFolder.FolderPath), StringComparison.OrdinalIgnoreCase));

        if (!hasRootFolder)
        {
            result.Insert(0, DefaultRootFolder);
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
