---
id: 0004
title: Вынести валидацию ввода белых списков из WinUI в Shared
status: cancelled
type: refactor
platform: shared
milestone: M1
priority: medium
depends_on: [0001]
spec: [APP-006, SITE-003]
adr: [0007]
---

## Контекст

Логика нормализации и проверки ввода живёт в code-behind страниц WinUI
(`src/Windows/FocusManager.App/Views/*WhitelistPage.xaml.cs`).

## Результат

**Отменена (2026-11-16).** Интерфейс WinUI заменяется на Tauri + React (ADR-0007). Проверка ввода
переносится вместе с экранами в задаче 0018 и покрывается общими тестовыми примерами (задача 0022).
