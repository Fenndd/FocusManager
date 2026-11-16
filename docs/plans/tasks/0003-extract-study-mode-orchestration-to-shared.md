---
id: 0003
title: Вынести логику сессий из Windows-агента в Shared
status: todo
type: refactor
platform: shared
milestone: M2
priority: high
depends_on: [0001]
spec: [SM-001, SM-002, SM-003, SM-004, SM-005]
adr: [0005, 0007]
---

## Контекст

`src/Windows/FocusManager.Agent/Services/AgentHostedService.cs` смешивает три вещи:
1. IPC-сервер (Windows, named pipe);
2. трей (Windows, WinForms — уходит в Tauri, задача 0016);
3. **логику сессий**: состояние вкл/выкл, снимок, запуск/остановка мониторов, применение политики сайтов,
   фильтрация событий по снимку, вызов правил.

Пункт 3 одинаков для агентов Windows и Linux (ADR-0007) и должен жить в `src/Shared`, иначе его
придётся писать дважды.

## Что сделать

1. Объявить в `FocusManager.Core/Abstractions` порты, нужные логике сессий, например:
   `IAppLaunchMonitor`, `IAppBlocker`, `ISiteFilter` (apply/clear), `ISessionSnapshotProvider`,
   `IFolderMonitor`. Интерфейсы без Windows-понятий (HWND, реестр); идентификатор процесса допустим.
2. Реализовать `SessionController` (или аналог) в `src/Shared`: состояние, начало/завершение сессии,
   восстановление после перезапуска, маршрутизация событий в правила и блокировщики.
   Если кода становится много — проект `FocusManager.Application` (с ADR).
3. Windows-агент: адаптеры над существующими `ProcessStartMonitor`/`AppEnforcer`/`SiteEnforcer`/
   `ExplorerMonitor`/`FolderEnforcer`; `AgentHostedService` оставляет IPC и делегирование.
4. Unit-тесты контроллера в `tests/Shared` на фейках портов.

## Критерии приёмки

- [ ] `AgentHostedService` не содержит логики сессий (состояние, снимок, порядок включения/выключения).
- [ ] Контроллер покрыт тестами на SM-002, SM-003, SM-004, SM-005; тесты проходят на Linux.
- [ ] Поведение Windows-версии не изменилось (ручная проверка на Windows).

## Вне рамок

- Профили, триггеры, уровни защиты (новая модель — задача 0021, функции — этап M5).
- Изменение IPC-протокола.

## Результат
