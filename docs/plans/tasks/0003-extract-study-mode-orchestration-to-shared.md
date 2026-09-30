---
id: 0003
title: Вынести оркестрацию режима учёбы из Windows-агента в Shared
status: todo
type: refactor
platform: shared
milestone: M1
priority: high
depends_on: [0001]
spec: [SM-001, SM-002, SM-003, SM-004, SM-005]
adr: [0002]
---

## Контекст

`src/Windows/FocusManager.Agent/Services/AgentHostedService.cs` смешивает три вещи:
1. IPC-сервер (Windows, named pipe);
2. трей (Windows, WinForms);
3. **логику режима учёбы**: состояние вкл/выкл, снимок сессии, запуск/остановка мониторов,
   применение политики сайтов, фильтрация событий по снимку, вызов правил.

Пункт 3 одинаков для Windows и Android и должен жить в общем коде, иначе его придётся переписать для Android.

## Что сделать

1. Объявить в `FocusManager.Core/Abstractions` порты, нужные оркестрации, например:
   `IAppLaunchMonitor` (событие запуска), `IAppBlocker`, `ISiteFilter` (apply/clear),
   `ISessionSnapshotProvider`, при необходимости `IFolderMonitor`. Названия — на усмотрение исполнителя,
   но интерфейсы не должны содержать Windows-понятий (PID, HWND, реестр).
2. Реализовать `StudyModeController` (или аналог) в `src/Shared` — состояние, Enable/Disable,
   восстановление после перезапуска, маршрутизация событий в правила и блокировщики.
   Если кода становится много — создать проект `FocusManager.Application` (с ADR).
3. Windows-агент: адаптеры над существующими `ProcessStartMonitor`/`AppEnforcer`/`SiteEnforcer`/
   `ExplorerMonitor`/`FolderEnforcer`; `AgentHostedService` оставляет только IPC, трей и делегирование.
4. Unit-тесты контроллера в `tests/Shared` на фейках портов.

## Критерии приёмки

- [ ] `AgentHostedService` не содержит логики режима (состояние, снимок, порядок включения/выключения).
- [ ] Контроллер покрыт тестами на SM-002, SM-003, SM-004, SM-005; тесты проходят на Linux.
- [ ] Поведение Windows-версии не изменилось (ручная проверка через `Start-FocusManager.cmd` на Windows).

## Вне рамок

- Изменение поведения режима. Любые найденные спорные места — отдельными задачами.
- Изменение IPC-протокола.

## Результат

