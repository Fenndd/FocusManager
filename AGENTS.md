# AGENTS.md — инструкции для ИИ-агентов

Этот файл — точка входа для любого ИИ-агента (Claude Code, Codex и т. п.), который работает с репозиторием.
Прочитай его целиком перед началом работы.

## Что это за проект

FocusManager — блокировщик для сосредоточения (прежде всего на учёбе): профили с белым или чёрным
списком приложений, сайтов и папок, способы запуска, уровни защиты от обхода. Основные платформы —
**Android, Windows, Linux** (одинаковый функционал); iOS — позже. Сейчас есть только Windows-версия.

| Платформа | Стек (ADR-0005, ADR-0007) |
|-----------|---------------------------|
| Windows, Linux | Агент — C#/.NET (служба); интерфейс — Tauri 2 + TypeScript + React; хранилище — SQLite |
| Android | Kotlin + Jetpack Compose; хранилище — Room |
| Облако | Firebase (ADR-0006) |

Решения владельца продукта — `docs/spec/decisions.md`. Не принимай за владельца продуктовых решений:
если спецификация молчит — спроси или оформи вопрос `> ❓` в спецификации.

## Где что лежит

| Что | Где |
|-----|-----|
| Спецификация (что должна делать система) | `docs/spec/` |
| Архитектура и правила размещения кода | `docs/architecture/README.md` |
| Архитектурные решения (ADR) | `docs/architecture/decisions/` |
| Дорожная карта | `docs/plans/roadmap.md` |
| Задачи для агентов | `docs/plans/tasks/` |
| Общий C#-код агентов ПК (Windows + Linux) | `src/Shared/` |
| Windows-код (C#) | `src/Windows/` |
| Linux-код (C#, планируется) | `src/Linux/` |
| Интерфейс ПК (Tauri, планируется) | `src/Desktop/` |
| Android (Kotlin, планируется) | `src/Android/` |
| Контракты и форматы данных (JSON Schema, планируется) | `contracts/` |
| Общие тестовые примеры правил (планируется) | `test-vectors/` |
| C#-тесты | `tests/Shared/`, `tests/Windows/`, `tests/Linux/` |
| Скрипты разработчика | `scripts/<platform>/` |

## Порядок работы над задачей

1. Возьми задачу из `docs/plans/tasks/` (статус `todo`, все `depends_on` в статусе `done`).
   Если пользователь дал задачу словами — сначала оформи её файлом по шаблону `docs/plans/tasks/_template.md`.
2. Поставь задаче `status: in-progress` в том же коммите/PR, где начинаешь работу.
3. Прочитай разделы спецификации, на которые ссылается задача (`spec:`).
4. **Если задача меняет поведение** — сначала обнови спецификацию (`docs/spec/`), потом код.
   Спецификация и код меняются в одном PR; расхождение спеки и кода считается багом.
5. Размещай код по правилам из `docs/architecture/README.md` (раздел «Куда класть новый код»).
6. Добавь/обнови тесты. Прогони проверки (см. ниже).
7. Заполни в задаче раздел «Результат», поставь `status: done`.
8. Если по ходу нашлась новая работа вне рамок задачи — не расширяй задачу, создай новый файл задачи со статусом `todo`.
9. Если принято архитектурное решение (новый проект, новая зависимость, смена подхода) — добавь ADR.

## Сборка и тесты

```bash
# Общий код — собирается и тестируется на любой ОС (Linux/macOS/Windows)
dotnet test FocusManager.Shared.slnf

# Windows-версия целиком — только на Windows (WinUI 3, WinForms)
dotnet test FocusManager.Windows.slnf

# Запуск Windows-версии для ручной проверки — только на Windows
Start-FocusManager.cmd
```

На Linux `FocusManager.Platform.Windows` и его тесты собираются (включён `EnableWindowsTargeting`),
но Windows-тесты там не запускаются. `FocusManager.Agent`/`FocusManager.App` на Linux собираются только
официальным SDK от Microsoft (нужен WindowsDesktop SDK), WinUI-приложение — только на Windows.
Если ты работаешь не на Windows и не можешь проверить Windows-код — честно напиши об этом в «Результате» задачи.

Известное состояние: 3 теста `FocusManager.Core.Tests` падают не на Windows (Windows-пути в тестах) —
см. задачу `0001`.

### PostgreSQL (временно, до задачи 0002)

Сейчас хранилище агента — PostgreSQL; по ADR-0006 оно возвращается на SQLite (задача 0002), после чего
этот раздел удаляется. Пока: тесты `FocusManager.Persistence.Postgres.Tests` **пропускаются (Skipped),
если не задана** переменная `FOCUSMANAGER_TEST_POSTGRES`.

```bash
# Linux-контейнер (Ubuntu): локальный PostgreSQL для тестов
apt-get install -y postgresql && service postgresql start
su postgres -c "psql -c \"CREATE ROLE focusmanager LOGIN PASSWORD 'focusmanager';\" -c \"CREATE DATABASE focusmanager_test OWNER focusmanager;\""
export FOCUSMANAGER_TEST_POSTGRES="Host=localhost;Port=5432;Database=focusmanager_test;Username=focusmanager;Password=focusmanager"
dotnet test FocusManager.Shared.slnf
```

Пароли и строки подключения с паролями в репозиторий не коммитить.

## Правила кода

- Язык кода, идентификаторов и комментариев в коде — английский. Документация в `docs/` — русский.
- `src/Shared/*` (общий C#-код агентов ПК) **не должен** ссылаться на `src/Windows`/`src/Linux` и использовать
  API ОС (реестр, WMI, COM, `/proc`, D-Bus). Платформенные вещи — через интерфейсы в `FocusManager.Core`.
- Правило, которое есть на нескольких платформах (совпадение сайтов, приложений, путей), меняется вместе
  с общими тестовыми примерами в `test-vectors/` (ADR-0005).
- Стиль C#: file-scoped namespaces, `sealed` классы по умолчанию, `record` для моделей, async-методы с `CancellationToken`.
- Общие настройки проектов — в `Directory.Build.props` (корень) и `tests/Directory.Build.props`; не дублируй их в `.csproj`.
- Новый проект обязательно добавляй в `FocusManager.sln` (в папку своей платформы) и в соответствующий `*.slnf`.
- Не меняй поведение «попутно» в задачах на рефакторинг: рефакторинг — без изменения поведения, изменения поведения — через спецификацию.
