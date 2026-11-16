---
id: 0020
title: Создать Linux-агент
status: todo
type: feature
platform: linux
milestone: M4
priority: medium
depends_on: [0019, 0002, 0003]
spec: [LNX-002, LNX-003, APP-001, SITE-001]
adr: [0007]
---

## Контекст

ADR-0007: агент Linux — C#/.NET, общий код с Windows-агентом в `src/Shared`, платформенный код —
в `src/Linux`. Механизмы — по итогам задачи 0019.

## Что сделать

1. `src/Linux/FocusManager.Platform.Linux` и хост агента для Linux; `tests/Linux/...`;
   `FocusManager.Linux.slnf`; добавить в `FocusManager.sln` (папка `src/Linux`).
2. Служба systemd (unit-файл, скрипты разработчика в `scripts/linux/`).
3. IPC через Unix socket; Tauri-интерфейс подключается к нему (транспорт заложен в 0017).
4. Блокировка приложений и сайтов (политики Chromium/Firefox) по общей логике сессий из `src/Shared`.
5. Хранилище — `FocusManager.Persistence.Sqlite` с путём для Linux.
6. Job Linux в CI.

## Критерии приёмки

- [ ] На Ubuntu LTS агент работает как служба, интерфейс управляет сессией, приложения и сайты блокируются.
- [ ] Статусы Linux в спецификации обновлены.

## Вне рамок

- Блокировка папок и уведомлений — отдельные задачи по итогам 0019.

## Результат
