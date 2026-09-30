# ADR-0003: Исходники разделены на Shared / Windows / Android

- Статус: Принято (хранилище SQLite позже заменено на PostgreSQL — см. ADR-0004)
- Дата: 2026-09-30

## Контекст

Раньше все проекты лежали плоско в `src/`, а `FocusManager.Infrastructure` смешивал кроссплатформенное
хранилище (SQLite) и Windows-интеграции (WMI, COM Проводника, реестр Chrome). С появлением Android
нужно явно видеть, какой код общий, а какой платформенный, и собирать платформы независимо.

## Решение

- `src/Shared`, `src/Windows`, `src/Android`; `tests/` зеркалит ту же структуру.
- `FocusManager.Infrastructure` разделён:
  - `src/Shared/FocusManager.Persistence.Sqlite` (namespace `FocusManager.Persistence.Sqlite`);
  - `src/Windows/FocusManager.Platform.Windows` (namespace `FocusManager.Platform.Windows`, TFM `net8.0-windows`).
- `FocusManager.Contracts` перенесён в `src/Windows`: это IPC между Windows-UI и Windows-агентом,
  на Android IPC не нужен (UI и сервис в одном приложении). Если DTO понадобятся на Android
  (например, импорт/экспорт настроек) — перенести в `Shared` отдельным решением.
- Имена `FocusManager.App`/`FocusManager.Agent` сохранены: от них зависят имена exe и скрипты.
- Решения-фильтры `FocusManager.Shared.slnf`, `FocusManager.Windows.slnf` (и позже `FocusManager.Android.slnf`)
  позволяют собирать платформу без чужих SDK/workload.
- Общие настройки проектов вынесены в `Directory.Build.props`, общие пакеты тестов — в `tests/Directory.Build.props`.
- Скрипты разработчика — `scripts/<platform>/`.

## Последствия

- Перенос выполнен без изменения поведения; семантические рефакторинги (вынос Windows-умолчаний
  из хранилища, вынос оркестрации в Shared) оформлены задачами `0001`–`0005`.
- Пути сборки изменились: `src/Windows/FocusManager.Agent/bin/...` — скрипты обновлены.
