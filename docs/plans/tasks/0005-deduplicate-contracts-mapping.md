---
id: 0005
title: Убрать дублирование маппинга DTO ↔ модели в Windows-версии
status: todo
type: refactor
platform: windows
milestone: M1
priority: low
depends_on: []
spec: []
adr: []
---

## Контекст

`MapToDto`/`MapToCore` для `WhitelistConfig` ⇄ `WhitelistConfigDto` написаны дважды:
в `src/Windows/FocusManager.App/Services/AgentClient.cs` и
`src/Windows/FocusManager.Agent/Services/AgentHostedService.cs`. При добавлении поля в модель
легко забыть одну из копий.

## Что сделать

1. Перенести маппинг в `FocusManager.Contracts` (добавить ссылку Contracts → Core) как extension-методы.
2. Использовать его в App и Agent, удалить копии.
3. Тест на round-trip маппинга (можно в `tests/Windows/FocusManager.Agent.Tests` или новом `FocusManager.Contracts.Tests`).

## Критерии приёмки

- [ ] Маппинг существует в одном месте.
- [ ] Round-trip тест: модель → DTO → модель даёт равную модель.

## Вне рамок

- Изменение формата IPC-сообщений.

## Результат

