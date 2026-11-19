"""T055 helper: create synthetic FocusManager SQLite fixtures and print a read-only report.

Standard library only (Python 3.9+; uses the sqlite3 module bundled with Python). Run with
isolated mode so nothing is imported from the current directory:

    python -I fixture_tools.py create --kind legacy  --out <path>\\legacy.db
    python -I fixture_tools.py create --kind current --out <path>\\current.db
    python -I fixture_tools.py report <path>\\focusmanager.db

Why Python and not PowerShell: the SQLite assemblies shipped in the agent output target .NET 8
and do not load into Windows PowerShell 5.1 (verified: System.Runtime 6.0.0.0 load failure), so
this helper deliberately does not depend on them. Python is only needed on the machine that
creates fixtures and reads reports (the host); fixtures are plain files that are copied into the
disposable VM, and the database is copied back out (agent stopped) for the report.

Schema text is copied verbatim from tests/Shared/FocusManager.Persistence.Sqlite.Tests/
LegacySchemas.cs (WithoutAllowSubfolders / WithAllowSubfolders). WithAllowSubfolders is also the
schema created by `git show 31e6e31:src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs`
(InitializeSchema) and by the current store.

All values are synthetic and carry the marker FMT055. study_mode_enabled is always 0 so that the
agent does not start enforcement (process termination, browser policy) while it is under test.
`create` refuses to overwrite an existing file; `report` opens the file read-only.
"""

from __future__ import annotations

import argparse
import hashlib
import pathlib
import sqlite3
import sys

SCHEMA_WITHOUT_ALLOW_SUBFOLDERS = """
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
"""

SCHEMA_WITH_ALLOW_SUBFOLDERS = """
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
"""

# Rows include the "System Root (C:)" folder that the real agent persisted as its first folder.
ROWS_LEGACY = r"""
INSERT INTO allowed_apps(display_name, executable_path) VALUES
  ('FMT055 Notepad', 'C:\Windows\System32\notepad.exe'),
  ('FMT055 Legacy Tool', 'C:\FMT055\Apps\LegacyTool.exe');
INSERT INTO allowed_folders(display_name, folder_path) VALUES
  ('System Root (C:)', 'C:\'),
  ('FMT055 Legacy Study', 'C:\FMT055\Study'),
  ('FMT055 Legacy Notes', 'C:\FMT055\Notes');
INSERT INTO allowed_sites(display_name, host_pattern) VALUES
  ('FMT055 Wiki', 'fmt055-wiki.example.org'),
  ('FMT055 Docs', '*.fmt055-docs.example.org');
"""

ROWS_CURRENT = r"""
INSERT INTO allowed_apps(display_name, executable_path) VALUES
  ('FMT055 Notepad', 'C:\Windows\System32\notepad.exe'),
  ('FMT055 Current Tool', 'C:\FMT055\Apps\CurrentTool.exe');
INSERT INTO allowed_folders(display_name, folder_path, allow_subfolders) VALUES
  ('System Root (C:)', 'C:\', 0),
  ('FMT055 Current Study', 'C:\FMT055\Study', 1),
  ('FMT055 Current Notes', 'C:\FMT055\Notes', 0);
INSERT INTO allowed_sites(display_name, host_pattern) VALUES
  ('FMT055 Wiki', 'fmt055-wiki.example.org'),
  ('FMT055 Docs', '*.fmt055-docs.example.org');
"""

TABLES = ("settings", "allowed_apps", "allowed_folders", "allowed_sites")


def sha256_of(path: pathlib.Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def create(kind: str, out: pathlib.Path) -> None:
    if out.exists():
        sys.exit(f"Refusing to overwrite existing file: {out}")
    out.parent.mkdir(parents=True, exist_ok=True)
    schema = SCHEMA_WITHOUT_ALLOW_SUBFOLDERS if kind == "legacy" else SCHEMA_WITH_ALLOW_SUBFOLDERS
    rows = ROWS_LEGACY if kind == "legacy" else ROWS_CURRENT
    connection = sqlite3.connect(str(out), isolation_level=None)  # default rollback journal, no WAL
    try:
        connection.executescript(schema + rows)
    finally:
        connection.close()
    print(f"created {kind} fixture: {out}")
    print(f"length={out.stat().st_size} sha256={sha256_of(out)}")


def report(path: pathlib.Path) -> None:
    if not path.is_file():
        sys.exit(f"Not a file: {path}")
    print(f"file.name        : {path.name}")
    print(f"file.length      : {path.stat().st_size}")
    print(f"file.sha256      : {sha256_of(path)}")
    side = [suffix for suffix in ("-journal", "-wal", "-shm") if pathlib.Path(str(path) + suffix).exists()]
    print(f"side files       : {side or 'none'}  (none expected when the agent is stopped)")

    connection = sqlite3.connect(f"{path.resolve().as_uri()}?mode=ro", uri=True)
    try:
        print("--- sqlite_master (normalized sql) ---")
        for kind, name, sql in connection.execute(
            "SELECT type, name, sql FROM sqlite_master WHERE name NOT LIKE 'sqlite_%' ORDER BY type, name"
        ):
            print(f"{kind} {name}: {' '.join((sql or '').split())}")

        print("--- columns ---")
        for table in TABLES:
            for _cid, name, ctype, notnull, default, pk in connection.execute(f"PRAGMA table_info('{table}')"):
                shown = "none" if default is None else default
                print(f"{table} column {name} {ctype} notnull={notnull} default={shown} pk={pk}")

        print("--- rows (id order) ---")
        for table in TABLES:
            order = "key" if table == "settings" else "id"
            cursor = connection.execute(f"SELECT * FROM {table} ORDER BY {order}")
            names = [d[0] for d in cursor.description]
            rows = cursor.fetchall()
            print(f"[{table}] count={len(rows)}")
            for row in rows:
                print("  " + " | ".join(f"{n}={v}" for n, v in zip(names, row)))
    finally:
        connection.close()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)
    p_create = sub.add_parser("create", help="create a new synthetic fixture file")
    p_create.add_argument("--kind", choices=("legacy", "current"), required=True)
    p_create.add_argument("--out", type=pathlib.Path, required=True)
    p_report = sub.add_parser("report", help="print a read-only report of a database file")
    p_report.add_argument("path", type=pathlib.Path)
    args = parser.parse_args()
    if args.command == "create":
        create(args.kind, args.out)
    else:
        report(args.path)


if __name__ == "__main__":
    main()
