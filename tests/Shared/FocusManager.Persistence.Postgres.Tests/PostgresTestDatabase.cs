using Npgsql;
using Xunit;

namespace FocusManager.Persistence.Postgres.Tests;

internal static class PostgresTestDatabase
{
    // Connection string to a PostgreSQL database the tests may create schemas in.
    public const string ConnectionStringVariable = "FOCUSMANAGER_TEST_POSTGRES";

    public static string? ConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringVariable) is { Length: > 0 } value ? value : null;
}

/// <summary>
/// Runs the test only when <see cref="PostgresTestDatabase.ConnectionStringVariable"/> is set;
/// otherwise the test is reported as skipped.
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (PostgresTestDatabase.ConnectionString is null)
        {
            Skip = $"Set {PostgresTestDatabase.ConnectionStringVariable} to run PostgreSQL tests.";
        }
    }
}

/// <summary>
/// Creates an isolated schema per test and drops it on dispose.
/// </summary>
internal sealed class TempSchemaScope : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _schemaName;

    private TempSchemaScope(string adminConnectionString, string schemaName)
    {
        _adminConnectionString = adminConnectionString;
        _schemaName = schemaName;

        ConnectionString = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            SearchPath = schemaName,
            Pooling = false
        }.ToString();
    }

    public string ConnectionString { get; }

    public static async Task<TempSchemaScope> CreateAsync()
    {
        var adminConnectionString = PostgresTestDatabase.ConnectionString
            ?? throw new InvalidOperationException($"{PostgresTestDatabase.ConnectionStringVariable} is not set.");

        var schemaName = "fm_test_" + Guid.NewGuid().ToString("N");

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE SCHEMA \"{schemaName}\";", connection);
        await command.ExecuteNonQueryAsync();

        return new TempSchemaScope(adminConnectionString, schemaName);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_adminConnectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{_schemaName}\" CASCADE;", connection);
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
