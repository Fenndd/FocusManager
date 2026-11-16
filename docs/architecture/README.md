# Архитектура FocusManager

Решения: [ADR-0001](decisions/0001-docs-spec-plans-layout.md) (документация),
[ADR-0003](decisions/0003-source-tree-by-platform.md) (структура исходников),
[ADR-0005](decisions/0005-native-stack-per-platform.md) (стеки платформ),
[ADR-0006](decisions/0006-local-storage-and-firebase.md) (хранилище и облако),
[ADR-0007](decisions/0007-desktop-agent-and-tauri-ui.md) (архитектура ПК).

## Платформы и стеки

| Платформа | Логика | Интерфейс | Локальное хранилище | Облако |
|-----------|--------|-----------|---------------------|--------|
| Windows | C#/.NET — агент (служба) | Tauri 2 + TypeScript + React | SQLite | Firebase (через интерфейс) |
| Linux | C#/.NET — агент (systemd), общий код с Windows | то же Tauri-приложение | SQLite | Firebase (через интерфейс) |
| Android | Kotlin | Jetpack Compose | Room (SQLite) | Firebase Android SDK |
| iOS (позже) | Swift | SwiftUI | — | — |

Общие для всех платформ — **спецификация**, **тестовые примеры** (`test-vectors/`) и **форматы данных**
(`contracts/`), а не код (ADR-0005).

## Структура репозитория

Текущее состояние и цель (ADR-0007). ⏳ — ещё не создано.

```
FocusManager.sln                 ← C#-проекты (папки решения: src/Shared, src/Windows, src/Linux, tests/...)
FocusManager.Shared.slnf         ← общий C#-код + тесты (собирается на любой ОС)
FocusManager.Windows.slnf        ← Shared + Windows (собирается на Windows)
Directory.Build.props            ← общие настройки C#-проектов
contracts/                    ⏳ JSON Schema: сообщения интерфейс ↔ агент, документы синхронизации
test-vectors/                 ⏳ общие тестовые примеры правил (JSON) для всех платформ
src/
├── Shared/                      ← C#, общий код агентов ПК (Windows + Linux), net8.0, без API ОС
│   ├── FocusManager.Core/                  модели, правила, абстракции (порты)
│   └── FocusManager.Persistence.Postgres/  временно; заменяется на Persistence.Sqlite (задача 0002)
├── Windows/                     ← C#, только Windows
│   ├── FocusManager.Platform.Windows/      WMI, COM Проводника, реестр политик браузеров
│   ├── FocusManager.Contracts/             IPC-сообщения (станут генерироваться из contracts/, задача 0005)
│   ├── FocusManager.Agent/                 агент (станет службой без UI, задача 0016)
│   └── FocusManager.App/                   WinUI 3 — удаляется после перехода на Tauri (задача 0018)
├── Linux/                    ⏳ C#: Platform.Linux, хост агента (задача 0020)
├── Desktop/                  ⏳ Tauri 2 + TypeScript + React — интерфейс ПК (задача 0017)
├── Android/                  ⏳ Kotlin + Compose, Gradle (задача 0009)
└── iOS/                         позже
tests/
├── Directory.Build.props        ← общие пакеты C#-тестов (xUnit)
├── Shared/                      ← тесты общего C#-кода, на любой ОС
└── Windows/                     ← тесты Windows-проектов, на Windows
scripts/
└── windows/                     ← скрипты разработчика для Windows
```

Каждый не-.NET проект (Tauri, Android) держит свои тесты внутри своей папки по правилам своего
инструментария (`src/Desktop/...`, `src/Android/...`).

## Зависимости C#-проектов

```
Shared:   Persistence.*        ──► Core
Windows:  Platform.Windows     ──► Core
          Contracts            (без зависимостей; позже — сгенерирован из contracts/)
          Agent                ──► Core, Persistence.*, Platform.Windows, Contracts
          App (WinUI, временно) ──► Core, Contracts
Linux:    Platform.Linux       ──► Core                                      (план)
          Agent (Linux-хост)   ──► Core, Persistence.Sqlite, Platform.Linux  (план)
```

Правила (проверяются ревью):

1. `src/Shared/*` ссылается только на `src/Shared/*` и таргетит `net8.0` (без `-windows`). Никаких API ОС
   (реестр, WMI, COM, P/Invoke, `System.Windows.*`, `/proc`, D-Bus). Нужна возможность ОС — интерфейс
   в `Core.Abstractions`, реализация в `Platform.<OS>`.
2. `FocusManager.Core` не зависит ни от чего, кроме BCL.
3. Проекты платформы X не ссылаются на проекты платформы Y.
4. Интерфейс ПК общается с агентом только через контракты (`contracts/`); агент не зависит от интерфейса.
5. Блокировка не зависит от сети и от запущенного интерфейса: агент применяет последние сохранённые правила.
6. C#-тесты зеркалят `src/`: общий код — `tests/Shared`, Windows-код — `tests/Windows`, Linux — `tests/Linux`.

## Куда класть новый код

| Код | Куда |
|-----|------|
| Модель предметной области, правило «разрешено/запрещено» для ПК | `src/Shared/FocusManager.Core` |
| Логика сессий, профилей, триггеров агента ПК (одинакова на Windows и Linux) | `src/Shared` (задача 0003) |
| Локальное хранилище ПК | `src/Shared/FocusManager.Persistence.Sqlite` |
| Код, вызывающий API Windows / Linux | `src/Windows/FocusManager.Platform.Windows` / `src/Linux/...` |
| Экраны ПК, трей, вход в аккаунт, синхронизация с Firebase | `src/Desktop` (Tauri) |
| Сообщения интерфейс ↔ агент, документы синхронизации | `contracts/` (JSON Schema) |
| Правило, которое должно работать одинаково на нескольких платформах | реализация в каждой платформе + примеры в `test-vectors/` |
| Всё Android | `src/Android` |

## Конвенции

- Язык кода, идентификаторов и комментариев — английский; документация в `docs/` — русский.
- C#: file-scoped namespaces, `sealed` по умолчанию, `record` для моделей, async с `CancellationToken`.
- Проекты C#: `FocusManager.<Слой>` для общего кода, `FocusManager.Platform.<OS>` для интеграций с ОС.
- В namespace `FocusManager.Platform.Windows`/`...Linux` типы ОС при полном квалифицировании —
  через `global::` (иначе имя разрешится в наш namespace); `using` в начале файла работает без этого.
