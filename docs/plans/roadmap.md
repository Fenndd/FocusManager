# Дорожная карта

Состояние задач — в их файлах (`docs/plans/tasks/`). Здесь — только этапы и порядок.
Решения владельца — `docs/spec/decisions.md`; архитектура — `docs/architecture/`.

**Параллельность (решения B1, B2):** этапы ПК (M1, M2) и Android (M3) идут параллельно. Linux (M4)
начинается после того, как логика сессий вынесена в общий код (задача 0003). iOS — после финализации
основных платформ.

## M0 — Реструктуризация и спецификация ✅

Разделение кода по платформам, спецификация по решениям владельца, архитектура (ADR-0001…0007),
система задач.

## M1 — Фундамент ПК

Цель: агент Windows на актуальной платформе, локальное хранилище, правила по новой спецификации,
проверки в CI.

| Задача | Кратко |
|--------|--------|
| [0002](tasks/0002-restore-sqlite-storage.md) | Вернуть SQLite, убрать PostgreSQL и Windows-умолчания из общего кода |
| [0007](tasks/0007-migrate-to-dotnet-10.md) | Переход на .NET 10 — **срочно**, поддержка .NET 8 закончилась |
| [0001](tasks/0001-make-core-path-rules-os-independent.md) | Правила путей в Core не зависят от ОС; тесты зелёные на Linux |
| [0012](tasks/0012-align-core-rules-with-spec.md) | Правила Core по новой спецификации (APP-003, APP-004, SITE-002) |
| [0022](tasks/0022-add-shared-test-vectors.md) | Общие тестовые примеры правил (`test-vectors/`) |
| [0006](tasks/0006-add-ci-workflow.md) | CI для общего кода и Windows |

## M2 — Новая архитектура ПК (ADR-0007)

Цель: агент — служба без UI, интерфейс — Tauri + React, браузеры по спецификации.

| Задача | Кратко |
|--------|--------|
| [0005](tasks/0005-define-contracts-schema.md) | Контракты интерфейс ↔ агент в JSON Schema, генерация типов C# и TypeScript |
| [0017](tasks/0017-bootstrap-tauri-desktop-ui.md) | Каркас интерфейса ПК на Tauri 2 + React, трей, локализация |
| [0016](tasks/0016-run-agent-as-windows-service.md) | Агент — служба Windows, трей уходит в Tauri |
| [0018](tasks/0018-migrate-screens-and-remove-winui.md) | Перенос экранов в Tauri, удаление WinUI |
| [0003](tasks/0003-extract-study-mode-orchestration-to-shared.md) | Логика сессий — в общий код агентов ПК |
| [0013](tasks/0013-support-major-browsers-on-windows.md) | Популярные браузеры на Windows (Chromium + Firefox), блокировка остальных |
| [0011](tasks/0011-research-opera-site-filtering.md) | Способ фильтрации сайтов в Opera |
| [0014](tasks/0014-enable-block-notifications-with-setting.md) | Уведомления о блокировке с настройкой |

## M3 — Android

| Задача | Кратко |
|--------|--------|
| [0008](tasks/0008-research-android-enforcement-and-ui.md) | Исследование механизмов блокировки, требования Google Play, ADR |
| [0009](tasks/0009-bootstrap-android-projects.md) | Каркас приложения на Kotlin + Compose |

Задачи MVP Android (блокировка приложений и сайтов, сессии) создаются по итогам 0008.

## M4 — Linux

| Задача | Кратко |
|--------|--------|
| [0019](tasks/0019-research-linux-mechanisms.md) | Исследование механизмов блокировки на Linux |
| [0020](tasks/0020-bootstrap-linux-agent.md) | Linux-агент: служба systemd, блокировка приложений и сайтов |

## M5 — Функции продукта на всех основных платформах

Профили и режимы, способы запуска, уровни защиты, контент, статистика (`docs/spec/`).

| Задача | Кратко |
|--------|--------|
| [0021](tasks/0021-define-profile-data-model.md) | Модель данных профилей и формат синхронизации |

Задачи на функции по платформам создаются после 0021.

## M6 — Облако, монетизация, релиз

| Задача | Кратко |
|--------|--------|
| [0023](tasks/0023-set-up-firebase-project.md) | Проект Firebase (нужны действия владельца) |
| [0024](tasks/0024-research-subscriptions-and-payments.md) | Оплата подписок на всех платформах, юридические шаги |
| [0015](tasks/0015-create-product-site.md) | Сайт продукта и политика конфиденциальности |

Перед релизом в Google Play: закрытое тестирование (12 тестировщиков, 14 дней — GEN-006).

## Позже

- iOS — после финализации основных платформ (`docs/spec/platforms/ios.md`, IOS-001).
- Партнёр по контролю — отложен владельцем (PROT-011).

## Отменено

- [0004](tasks/0004-extract-whitelist-editing-logic-to-shared.md) — WinUI заменяется на Tauri (см. 0018).
- [0010](tasks/0010-migrate-data-from-sqlite.md) — PostgreSQL убирается (ADR-0006).
