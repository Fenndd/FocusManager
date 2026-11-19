using FocusManager.Core.Models;

namespace FocusManager.Persistence.Sqlite;

/// <summary>
/// Platform-supplied settings for <see cref="SqliteWhitelistStore"/>. Shared code has no platform
/// defaults: each agent host chooses the database location and the folders that are always allowed.
/// </summary>
public sealed record SqliteWhitelistStoreOptions
{
    /// <summary>
    /// Path of the SQLite database file. A relative path is resolved against the current directory
    /// when the store is created. A missing directory is created on first use.
    /// </summary>
    public required string DatabasePath { get; init; }

    /// <summary>
    /// Folders that are always present in the allow-list, for example the system drive root on
    /// Windows (FLD-004). On load and save, each default folder that the list does not already
    /// contain is inserted at the start of the list, in this order. Empty by default.
    /// </summary>
    public IReadOnlyList<AllowedFolder> DefaultFolders { get; init; } = [];
}
