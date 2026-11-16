---
id: 0002
title: Вернуть локальное хранилище SQLite без Windows-умолчаний
status: todo
type: change
platform: shared
milestone: M1
priority: high
depends_on: []
spec: [WIN-006, GEN-005, FLD-004, SYNC-001]
adr: [0006]
---

## Контекст

ADR-0006 (решение C6): на ПК данные хранятся локально в SQLite, PostgreSQL убирается. Сейчас
агент использует `src/Shared/FocusManager.Persistence.Postgres` (`PostgresWhitelistStore`) и требует
строку подключения; до 2026-09-30 был `SqliteWhitelistStore` — его код есть в истории git
(`git show 31e6e31:src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs`).

Кроме того, в хранилище зашито Windows-правило «`C:\` всегда в списке» (FLD-004), а хранилище — общий
код агентов Windows и Linux.

## Что сделать

1. Создать `src/Shared/FocusManager.Persistence.Sqlite` (`Microsoft.Data.Sqlite`) с `SqliteWhitelistStore`,
   совместимым по схеме и расположению файла со старой базой (`%LOCALAPPDATA%\FocusManager\focusmanager.db`
   на Windows; путь передаётся параметром, чтобы Linux-агент мог задать свой).
2. Папки по умолчанию передаются через параметры (например, `SqliteWhitelistStoreOptions.DefaultFolders`).
   Windows-агент передаёт `C:\` (FLD-004), общий код Windows-путей не содержит.
3. Удалить `FocusManager.Persistence.Postgres`, его тесты, `appsettings.json` агента (если не нужен),
   ссылки в `FocusManager.sln` и `*.slnf`.
4. Перенести тесты хранилища (в т. ч. миграцию старой схемы `allow_subfolders`) в
   `tests/Shared/FocusManager.Persistence.Sqlite.Tests`; тесты не требуют внешних сервисов.
5. Убрать разделы про PostgreSQL из `README.md` и `AGENTS.md`.

## Критерии приёмки

- [ ] Агент запускается без какой-либо настройки БД; данные сохраняются между перезапусками.
- [ ] Существующая база SQLite старого формата открывается и читается.
- [ ] В `src/Shared` нет строк `C:\` и упоминаний PostgreSQL/Npgsql.
- [ ] `dotnet test FocusManager.Shared.slnf` проходит без переменных окружения.

## Вне рамок

- Новая модель данных (профили, триггеры) — задача 0021.

## Результат
