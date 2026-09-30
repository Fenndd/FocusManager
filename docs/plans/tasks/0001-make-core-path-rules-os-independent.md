---
id: 0001
title: Сделать правила путей в Core независимыми от ОС
status: todo
type: refactor
platform: shared
milestone: M1
priority: high
depends_on: []
spec: [APP-002, APP-003, FLD-002, FLD-003]
adr: [0002]
---

## Контекст

`src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs` сравнивает пути через `Path.GetFullPath`,
`Path.DirectorySeparatorChar` и `StringComparison.OrdinalIgnoreCase`, т. е. семантика зависит от ОС,
на которой код **выполняется**. Поэтому на Linux падают 3 теста с Windows-путями:

- `EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable`
- `EvaluateFolderOpen_Allows_SameFolderPath_IgnoringTrailingSeparator`
- `EvaluateFolderOpen_Allows_ChildFolder_WhenSubfoldersAllowed`

Для Android это значит, что общий код нельзя проверить на CI вне Windows, а на самом Android
(case-sensitive ФС, `/`) правила будут вести себя иначе, чем описано в спецификации.

## Что сделать

1. Ввести в Core явное описание семантики путей платформы (например, `PathStyle` / `IPathRules`:
   разделители, чувствительность к регистру, нормализация) — без обращения к `System.IO.Path` текущей ОС.
2. `RuleEvaluator` получает эту семантику через конструктор. Windows-агент регистрирует Windows-вариант
   (поведение на Windows не меняется).
3. Тесты `RuleEvaluatorTests` параметризовать по стилю путей (Windows и Unix), чтобы оба набора
   проходили на любой ОС.

## Критерии приёмки

- [ ] `dotnet test FocusManager.Shared.slnf` — 0 падений на Linux.
- [ ] Поведение на Windows не изменилось (все существующие кейсы проходят для Windows-стиля).
- [ ] В `RuleEvaluator` нет `Path.GetFullPath`/`Path.DirectorySeparatorChar`.
- [ ] Регистрация в `src/Windows/FocusManager.Agent/Program.cs` обновлена.
- [ ] Строка про известные падения в `AGENTS.md` удалена.

## Вне рамок

- Изменение семантики правил (например, отказ от APP-003) — отдельные задачи `change`.
- Аналогичные `NormalizePath` в `AppEnforcer`/`FolderEnforcer` (Windows-код) — можно не трогать.

## Результат

