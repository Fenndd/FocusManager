---
id: 0004
title: Вынести валидацию и нормализацию ввода белых списков из WinUI в Shared
status: todo
type: refactor
platform: shared
milestone: M1
priority: medium
depends_on: [0001]
spec: [APP-006, SITE-003]
adr: [0002]
---

## Контекст

Логика редактирования белых списков живёт в code-behind страниц WinUI
(`src/Windows/FocusManager.App/Views/*WhitelistPage.xaml.cs`): `NormalizeHostPattern`, `NormalizeHost`,
`NormalizeExecutablePath`, `NormalizeFolderPath`, проверки дубликатов, `.exe`, существования файла.
Android-UI понадобятся те же правила (как минимум для сайтов), а сейчас они не тестируются вовсе.

## Что сделать

1. В `FocusManager.Core` добавить валидаторы/нормализаторы записей белого списка с результатом вида
   `ValidationResult(IsValid, NormalizedValue, Error)`.
2. Платформенные проверки (существует ли файл, расширение `.exe`) — через интерфейс
   или отдельный Windows-валидатор; общая часть (пустое значение, дубликат, формат хоста) — в Shared.
3. Страницы WinUI вызывают эти валидаторы; тексты сообщений в UI не меняются.
4. Тесты в `tests/Shared/FocusManager.Core.Tests`.

## Критерии приёмки

- [ ] В code-behind страниц не осталось собственной логики нормализации.
- [ ] Для `NormalizeHostPattern` есть тесты на все ветви (`*`, `*.x`, пробелы, `/`, пустое).
- [ ] UI ведёт себя как раньше (ручная проверка на Windows).

## Вне рамок

- Перевод страниц на MVVM (ViewModel сейчас пустые) — можно сделать отдельной задачей.

## Результат

