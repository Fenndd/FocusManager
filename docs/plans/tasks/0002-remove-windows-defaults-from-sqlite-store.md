---
id: 0002
title: Убрать Windows-умолчания из общего SQLite-хранилища
status: todo
type: refactor
platform: shared
milestone: M1
priority: high
depends_on: []
spec: [FLD-004, WIN-006]
adr: [0003]
---

## Контекст

`src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs` содержит
`DefaultRootFolder = new("System Root (C:)", @"C:\", false)` и всегда подмешивает его в список папок
(`EnsureDefaultFolders`). Это Windows-правило (FLD-004) внутри общего проекта: на Android появится
папка `C:\`.

## Что сделать

1. Добавить в хранилище параметр «папки по умолчанию» (например, `SqliteWhitelistStoreOptions`
   с `DatabasePath` и `DefaultFolders`). По умолчанию — пустой список.
2. В `src/Windows/FocusManager.Agent/Program.cs` зарегистрировать хранилище через фабрику
   с `C:\` в `DefaultFolders` (поведение Windows не меняется).
3. Обновить тесты `tests/Shared/FocusManager.Persistence.Sqlite.Tests`: отдельно — без умолчаний,
   отдельно — с умолчаниями.

## Критерии приёмки

- [ ] В `src/Shared` нет строк `C:\`.
- [ ] Windows-агент по-прежнему всегда отдаёт `C:\` в списке папок (FLD-004).
- [ ] `dotnet test FocusManager.Shared.slnf` проходит.

## Вне рамок

- Изменение схемы БД и миграций.

## Результат

