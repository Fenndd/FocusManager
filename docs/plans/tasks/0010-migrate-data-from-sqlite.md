---
id: 0010
title: Перенести данные из старой SQLite-базы в PostgreSQL
status: todo
type: feature
platform: windows
milestone: M1
priority: low
depends_on: []
spec: [WIN-006]
adr: [0004]
---

## Контекст

До перехода на PostgreSQL (ADR-0004) белые списки и состояние режима хранились в
`%LOCALAPPDATA%\FocusManager\focusmanager.db` (SQLite; таблицы `settings`, `allowed_apps`,
`allowed_folders`, `allowed_sites` — те же имена и колонки, `allow_subfolders` был `INTEGER 0/1`).
После обновления агент эти данные не видит.

## Что сделать

1. Одноразовый перенос: скрипт в `scripts/windows/` или консольная команда агента
   (например, `FocusManager.Agent.exe --import-sqlite <путь>`). Выбор — на исполнителе.
2. Импорт идёт через `IWhitelistStore.SaveAsync`/`SetStudyModeEnabledAsync`, а не прямым SQL, чтобы
   сохранить правила хранилища (папка по умолчанию, уникальность без учёта регистра).
3. Описать процедуру в `README.md`.

## Критерии приёмки

- [ ] На тестовой SQLite-базе старой схемы (с `allow_subfolders` и без) все записи и флаг режима
      оказываются в PostgreSQL.
- [ ] Повторный запуск не создаёт дубликатов.

## Вне рамок

- Автоматический перенос при каждом старте агента.
- Возврат к SQLite.

## Результат

