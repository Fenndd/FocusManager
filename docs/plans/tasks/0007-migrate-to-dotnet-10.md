---
id: 0007
title: Перейти на .NET 10 (LTS)
status: todo
type: chore
platform: shared
milestone: M1
priority: high
depends_on: [0002]
spec: []
adr: [0005]
---

## Контекст

Проект на .NET 8 (`global.json` → 8.0.419). **Поддержка .NET 8 закончилась 10 ноября 2026** —
обновления безопасности больше не выходят. Агент работает с правами администратора, поэтому это срочно.
Зависит от 0002, чтобы не обновлять Npgsql, который удаляется.

## Что сделать

1. `global.json` → SDK 10.0.x.
2. TFM: `net10.0`, `net10.0-windows`, `net10.0-windows10.0.19041.0` (App, пока он есть).
3. Обновить пакеты: `Microsoft.Data.Sqlite`, `Microsoft.Extensions.*`, `System.Management`,
   тестовые пакеты, `Microsoft.WindowsAppSDK`.
4. Проверить сборку и запуск на Windows (`Start-FocusManager.cmd`), обновить пути TFM в `scripts/windows/Start-FocusManager.ps1`.
5. Обновить упоминания `net8.0` в `docs/architecture/README.md` и `AGENTS.md`.

## Критерии приёмки

- [ ] `dotnet test FocusManager.Shared.slnf` и `FocusManager.Windows.slnf` проходят.
- [ ] Windows-версия запускается скриптом и блокирует приложения/папки/сайты как раньше.

## Вне рамок

- Использование новых возможностей языка/платформы.

## Результат
