# Дорожная карта

Состояние задач — в их файлах (`docs/plans/tasks/`). Здесь — только этапы и порядок.

## M0 — Реструктуризация репозитория ✅

Разделение кода на `Shared`/`Windows`/`Android`, спецификация, архитектура, система задач.
См. ADR-0001, ADR-0003. Хранилище переведено на PostgreSQL — ADR-0004.

## M1 — Подготовка общего кода к Android

Цель: всё, что по смыслу платформенно-независимо, лежит в `src/Shared`, не зависит от ОС и покрыто
тестами, которые проходят на Linux.

| Задача | Кратко |
|--------|--------|
| [0001](tasks/0001-make-core-path-rules-os-independent.md) | Правила путей в Core не зависят от ОС; тесты зелёные на Linux |
| [0002](tasks/0002-remove-windows-defaults-from-store.md) | Убрать Windows-умолчание `C:\` из общего хранилища |
| [0003](tasks/0003-extract-study-mode-orchestration-to-shared.md) | Вынести оркестрацию режима учёбы из Windows-агента в Shared |
| [0004](tasks/0004-extract-whitelist-editing-logic-to-shared.md) | Вынести валидацию/нормализацию ввода из WinUI-страниц в Shared |
| [0005](tasks/0005-deduplicate-contracts-mapping.md) | Убрать дублирование маппинга DTO ↔ модели |
| [0006](tasks/0006-add-ci-workflow.md) | CI: Shared на Linux, Windows на Windows |
| [0007](tasks/0007-migrate-to-dotnet-10.md) | Переход на .NET 10 LTS |
| [0010](tasks/0010-migrate-data-from-sqlite.md) | Перенос данных из старой SQLite-базы в PostgreSQL |

## M2 — Android: исследование и каркас

| Задача | Кратко |
|--------|--------|
| [0008](tasks/0008-research-android-enforcement-and-ui.md) | Исследование механизмов блокировки и выбор UI-подхода, ADR |
| [0009](tasks/0009-bootstrap-android-projects.md) | Каркас Android-проектов, подключение Shared, сборка |

## M3 — Android: MVP

Режим учёбы + белый список приложений на Android. Задачи создаются по итогам 0008.

## Бэклог (без этапа)

Открытые вопросы из спецификации (`> ❓ Вопрос:` в `docs/spec/`) — после решения владельцем
превращаются в задачи типа `change`.
