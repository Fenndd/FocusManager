# Архитектура FocusManager

Решения, на которых основан этот документ: [ADR-0001](decisions/0001-docs-spec-plans-layout.md),
[ADR-0002](decisions/0002-shared-dotnet-code-for-android.md), [ADR-0003](decisions/0003-source-tree-by-platform.md).

## Структура репозитория

```
FocusManager.sln                 ← все проекты (папки решения: src/Shared, src/Windows, src/Android, tests/...)
FocusManager.Shared.slnf         ← только общий код + его тесты (собирается на любой ОС)
FocusManager.Windows.slnf        ← Shared + Windows (собирается на Windows)
FocusManager.Android.slnf        ← Shared + Android (появится с задачей 0009)
Directory.Build.props            ← общие настройки всех проектов
src/
├── Shared/                      ← кроссплатформенный код, net8.0, без OS API
│   ├── FocusManager.Core/                  доменные модели, правила, абстракции (порты)
│   └── FocusManager.Persistence.Sqlite/    реализация IWhitelistStore на SQLite
├── Windows/                     ← только Windows
│   ├── FocusManager.Platform.Windows/      интеграции с ОС: WMI, COM Проводника, реестр Chrome
│   ├── FocusManager.Contracts/             IPC-сообщения App ↔ Agent (named pipe)
│   ├── FocusManager.Agent/                 фоновый агент: мониторинг, блокировки, трей, IPC-сервер
│   └── FocusManager.App/                   UI на WinUI 3
└── Android/                     ← только Android (планируется, см. src/Android/README.md)
tests/
├── Directory.Build.props        ← общие пакеты тестов (xUnit)
├── Shared/                      ← тесты общего кода, запускаются на любой ОС
└── Windows/                     ← тесты Windows-проектов, запускаются на Windows
scripts/
└── windows/                     ← скрипты разработчика для Windows
```

## Слои и зависимости

Кто на кого ссылается (стрелка = «зависит от»):

```
Shared:   Persistence.Sqlite ──► Core
Windows:  Platform.Windows   ──► Core
          Contracts          (без зависимостей)
          Agent              ──► Core, Persistence.Sqlite, Platform.Windows, Contracts
          App (WinUI)        ──► Core, Contracts            (с агентом — только через IPC)
Android:  Platform.Android   ──► Core                                  (план)
          Android-приложение ──► Core, Persistence.Sqlite, Platform.Android (план)
```

Правила (проверяются ревью):

1. `src/Shared/*` ссылается только на `src/Shared/*`. Никаких ссылок на `src/Windows`, `src/Android`.
2. `src/Shared/*` таргетит `net8.0` (без `-windows`/`-android`) и не использует OS-специфичные API:
   реестр, WMI, COM, P/Invoke, `System.Windows.*`, Android SDK. Если нужно — объяви интерфейс в `Core.Abstractions`
   и реализуй его в `Platform.<OS>`.
3. `FocusManager.Core` не зависит ни от чего, кроме BCL.
4. Проекты платформы X не ссылаются на проекты платформы Y.
5. UI-проекты не ссылаются на `Platform.<OS>` напрямую, если это не требуется платформой
   (на Windows UI общается с агентом только через `Contracts`).
6. Тесты зеркалят `src/`: тест общего кода — в `tests/Shared`, Windows-кода — в `tests/Windows`.

## Куда класть новый код

Задай себе вопрос: **«Будет ли этот код одинаковым на Windows и Android?»**

| Код | Куда |
|-----|------|
| Модель предметной области, правило «разрешено/запрещено», валидация и нормализация ввода пользователя | `Shared/FocusManager.Core` |
| Оркестрация режима учёбы (вкл/выкл, снимок, применение правил), не зависящая от ОС | `Shared/FocusManager.Core` (пока проект маленький; при росте — выделить `FocusManager.Application`, через ADR) |
| Хранение данных | `Shared/FocusManager.Persistence.Sqlite` |
| Интерфейс к возможности ОС (запуск процесса, открыть папку, политика браузера, уведомление) | интерфейс — `Core/Abstractions`, реализация — `Platform.<OS>` |
| Код, вызывающий API ОС (WMI, COM, реестр, AccessibilityService, VpnService) | `Platform.<OS>` |
| Хост фонового процесса, DI-композиция, трей | `Windows/FocusManager.Agent` / Android-приложение |
| Экраны, ViewModel, привязки к UI-фреймворку | UI-проект платформы |
| Логика ViewModel, не зависящая от UI-фреймворка | `Shared` (см. задачу 0004) |

Если код сейчас нужен только одной платформе, но **по смыслу** платформенно-независим (например, правило
сравнения хостов) — клади в `Shared`. Если код по смыслу платформенный, но пока есть только одна
платформа — клади в `Platform.<OS>`, не в `Shared`.

## Что уже общее и что нет (на 2026-09-30)

| Код | Сейчас | Цель |
|-----|--------|------|
| Модели (`AllowedApp`, `AllowedFolder`, `AllowedSite`, `WhitelistConfig`) | Shared | Shared |
| `RuleEvaluator` (правила сравнения) | Shared, но зависит от семантики путей текущей ОС | Shared, OS-независимый — задача 0001 |
| `SqliteWhitelistStore` | Shared, но содержит Windows-умолчание `C:\` | Shared без Windows-умолчаний — задача 0002 |
| Оркестрация режима учёбы | внутри `AgentHostedService` (Windows) | Shared — задача 0003 |
| Валидация/нормализация ввода белых списков | в code-behind страниц WinUI | Shared — задача 0004 |
| Маппинг DTO ↔ модели | продублирован в `AgentClient` и `AgentHostedService` | `Contracts` — задача 0005 |

## Конвенции именования

- Проекты: `FocusManager.<Слой>` для общего кода, `FocusManager.Platform.<OS>` для интеграций с ОС.
- Namespace = имя проекта + путь папки.
- В проектах, где есть namespace `FocusManager.Platform.Windows`, `FocusManager.Platform.Android`
  или `FocusManager.Android`, типы ОС при полном квалифицировании пиши через `global::`
  (`global::Android.App.Service`, `global::Windows.Storage.StorageFile`) — иначе `Android`/`Windows`
  разрешится в наш namespace. Директивы `using` в начале файла работают без `global::`.
