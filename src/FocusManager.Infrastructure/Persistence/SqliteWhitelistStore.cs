using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using Microsoft.Data.Sqlite;

namespace FocusManager.Infrastructure.Persistence;

public sealed class SqliteWhitelistStore : IWhitelistStore
{
    private const string StudyModeKey = "study_mode_enabled";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _connectionString;

    public SqliteWhitelistStore()
        : this(databasePath: null)
    {
    }

    public SqliteWhitelistStore(string? databasePath)
    {
        var resolvedDatabasePath = ResolveDatabasePath(databasePath);
        var databaseDirectory = Path.GetDirectoryName(resolvedDatabasePath);

        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = resolvedDatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        InitializeSchema();
    }

    public async Task<WhitelistConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await CreateOpenConnectionAsync(cancellationToken);

            var apps = await ReadAllowedAppsAsync(connection, cancellationToken);
            var folders = await ReadAllowedFoldersAsync(connection, cancellationToken);
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
            await using var connection = await CreateOpenConnectionAsync(cancellationToken);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, "DELETE FROM allowed_apps;", cancellationToken);
            await ExecuteNonQueryAsync(connection, transaction, "DELETE FROM allowed_folders;", cancellationToken);
            await ExecuteNonQueryAsync(connection, transaction, "DELETE FROM allowed_sites;", cancellationToken);

            foreach (var app in config.AllowedApps)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
INSERT INTO allowed_apps(display_name, executable_path)
VALUES ($display_name, $executable_path);";
                command.Parameters.AddWithValue("$display_name", app.DisplayName);
                command.Parameters.AddWithValue("$executable_path", app.ExecutablePath);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var folder in config.AllowedFolders)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
INSERT INTO allowed_folders(display_name, folder_path, allow_subfolders)
VALUES ($display_name, $folder_path, $allow_subfolders);";
                command.Parameters.AddWithValue("$display_name", folder.DisplayName);
                command.Parameters.AddWithValue("$folder_path", folder.FolderPath);
                command.Parameters.AddWithValue("$allow_subfolders", folder.AllowSubfolders ? 1 : 0);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var site in config.AllowedSites)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
INSERT INTO allowed_sites(display_name, host_pattern)
VALUES ($display_name, $host_pattern);";
                command.Parameters.AddWithValue("$display_name", site.DisplayName);
                command.Parameters.AddWithValue("$host_pattern", site.HostPattern);
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
            await using var connection = await CreateOpenConnectionAsync(cancellationToken);
            var command = connection.CreateCommand();
            command.CommandText = "SELECT value FROM settings WHERE key = $key LIMIT 1;";
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
            await using var connection = await CreateOpenConnectionAsync(cancellationToken);
            var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO settings(key, value)
VALUES ($key, $value)
ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
            command.Parameters.AddWithValue("$key", StudyModeKey);
            command.Parameters.AddWithValue("$value", enabled ? "1" : "0");

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private void InitializeSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
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
    folder_path TEXT NOT NULL UNIQUE COLLATE NOCASE,
    allow_subfolders INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS allowed_sites (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL,
    host_pattern TEXT NOT NULL UNIQUE COLLATE NOCASE
);

INSERT INTO settings(key, value)
VALUES ('study_mode_enabled', '0')
ON CONFLICT(key) DO NOTHING;
";

        command.ExecuteNonQuery();

        EnsureColumnExists(connection, "allowed_folders", "allow_subfolders", "INTEGER NOT NULL DEFAULT 0");
    }

    private async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<List<AllowedApp>> ReadAllowedAppsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT display_name, executable_path
FROM allowed_apps
ORDER BY id;";

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
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT display_name, folder_path, allow_subfolders
FROM allowed_folders
ORDER BY id;";

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

    private static void EnsureColumnExists(
        SqliteConnection connection,
        string tableName,
        string columnName,
        string columnSqlDefinition)
    {
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = checkCommand.ExecuteReader();
        while (reader.Read())
        {
            var existingColumnName = reader.GetString(1);
            if (string.Equals(existingColumnName, columnName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnSqlDefinition};";
        alterCommand.ExecuteNonQuery();
    }

    private static async Task<List<AllowedSite>> ReadAllowedSitesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT display_name, host_pattern
FROM allowed_sites
ORDER BY id;";

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

    private static string ResolveDatabasePath(string? databasePath)
    {
        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            return Path.GetFullPath(databasePath.Trim());
        }

        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FocusManager");

        return Path.Combine(appDataDir, "focusmanager.db");
    }
}
