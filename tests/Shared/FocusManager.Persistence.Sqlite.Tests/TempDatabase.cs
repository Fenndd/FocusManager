using System.Security.Cryptography;
using FocusManager.Core.Models;
using Microsoft.Data.Sqlite;

namespace FocusManager.Persistence.Sqlite.Tests;

/// <summary>
/// A database path inside a new temporary directory, which is deleted on dispose.
/// Raw connections bypass the store and do not pool, so they never keep the file open.
/// </summary>
internal sealed class TempDatabase : IDisposable
{
    private static readonly string[] Tables = ["settings", "allowed_apps", "allowed_folders", "allowed_sites"];

    public TempDatabase()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), "FocusManager.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
        DatabasePath = Path.Combine(DirectoryPath, "focusmanager.db");
    }

    public string DirectoryPath { get; }

    public string DatabasePath { get; }

    public SqliteWhitelistStore CreateStore(params AllowedFolder[] defaultFolders)
    {
        return new SqliteWhitelistStore(new SqliteWhitelistStoreOptions
        {
            DatabasePath = DatabasePath,
            DefaultFolders = defaultFolders
        });
    }

    public SqliteConnection OpenRawConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString());

        connection.Open();
        return connection;
    }

    public void Execute(string sql)
    {
        using var connection = OpenRawConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public long QueryLong(string sql)
    {
        using var connection = OpenRawConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar());
    }

    public string ComputeFileHash()
    {
        return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(DatabasePath)));
    }

    /// <summary>
    /// Columns and unique indexes (with collations) of the store's tables, one line each.
    /// </summary>
    public IReadOnlyList<string> DescribeSchema()
    {
        using var connection = OpenRawConnection();
        var lines = new List<string>();

        foreach (var table in Tables)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
SELECT name, type, ""notnull"", dflt_value, pk
FROM pragma_table_info('{table}')
ORDER BY cid;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var defaultValue = reader.IsDBNull(3) ? "none" : reader.GetString(3);
                    lines.Add($"{table} column {reader.GetString(0)} {reader.GetString(1)} " +
                        $"notnull={reader.GetInt64(2)} default={defaultValue} pk={reader.GetInt64(4)}");
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
SELECT il.""unique"", il.origin, ii.name, ii.coll
FROM pragma_index_list('{table}') AS il, pragma_index_xinfo(il.name) AS ii
WHERE ii.""key"" = 1
ORDER BY il.name, ii.seqno;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lines.Add($"{table} index unique={reader.GetInt64(0)} origin={reader.GetString(1)} " +
                        $"column={reader.GetString(2)} collation={reader.GetString(3)}");
                }
            }
        }

        return lines;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup only.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup only.
        }
    }
}
