---
id: 0005
title: Описать контракты интерфейс ↔ агент в JSON Schema и генерировать типы
status: todo
type: refactor
platform: windows
milestone: M2
priority: high
depends_on: []
spec: [SM-007]
adr: [0007]
---

## Контекст

Интерфейс ПК будет на TypeScript (Tauri), агент — на C# (ADR-0007). Сейчас сообщения описаны только
в C# (`src/Windows/FocusManager.Contracts/Messages.cs`), а маппинг DTO ↔ модели продублирован
в `src/Windows/FocusManager.App/Services/AgentClient.cs` и `AgentHostedService.cs`.

## Что сделать

1. Создать `contracts/` в корне репозитория: JSON Schema для текущих сообщений (`AgentRequestEnvelope`,
   `AgentResponseEnvelope`, DTO белых списков) и `contracts/README.md` с правилами версионирования.
2. Настроить генерацию типов: C# (для агента) и TypeScript (для интерфейса). Генерированный код
   не правится руками; команда генерации описана в `contracts/README.md` и `AGENTS.md`.
3. Заменить ручные DTO в `FocusManager.Contracts` на сгенерированные (или проверять их соответствие схеме тестом).
4. Маппинг DTO ↔ модели Core — в одном месте (Contracts или агент); копию в `AgentClient` удалить.
5. Тест: JSON, сериализованный агентом, проходит валидацию схемой.

## Критерии приёмки

- [ ] Схема — единственный источник формата сообщений.
- [ ] `dotnet test FocusManager.Windows.slnf` проходит на Windows.

## Вне рамок

- Изменение формата сообщений и транспорта.
- Документы синхронизации Firebase — задача 0021.

## Результат
