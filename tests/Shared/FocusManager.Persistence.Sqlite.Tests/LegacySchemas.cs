namespace FocusManager.Persistence.Sqlite.Tests;

/// <summary>
/// Schema scripts of earlier SQLite stores, copied verbatim from their sources. Windows agents
/// wrote these databases to <c>%LOCALAPPDATA%\FocusManager\focusmanager.db</c>.
/// </summary>
internal static class LegacySchemas
{
    /// <summary>
    /// Stores from 2026-04-09 to 2026-04-20 (up to commit 5f2e7e7): <c>allowed_folders</c> has no
    /// <c>allow_subfolders</c> column.
    /// </summary>
    public const string WithoutAllowSubfolders = @"
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
);

INSERT INTO settings(key, value)
VALUES ('study_mode_enabled', '0')
ON CONFLICT(key) DO NOTHING;
";

    /// <summary>
    /// Stores from 2026-04-20 to 2026-09-30 (commits f7abfaf to 31e6e31), the schema the new store
    /// also creates.
    /// </summary>
    public const string WithAllowSubfolders = @"
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
}
