---
id: 0006
title: Добавить CI (GitHub Actions) для Shared и Windows
status: todo
type: chore
platform: all
milestone: M1
priority: medium
depends_on: [0001]
spec: []
adr: [0003]
---

## Контекст

CI нет. Агенты часто работают в Linux-контейнерах и не могут проверить Windows-код — CI должен
делать это за них.

## Что сделать

1. `.github/workflows/ci.yml`:
   - job `shared` на `ubuntu-latest` с сервисом `postgres` (service container):
     `dotnet test FocusManager.Shared.slnf` с заданной `FOCUSMANAGER_TEST_POSTGRES`;
   - job `windows` на `windows-latest`: сборка и тесты `FocusManager.Windows.slnf`
     (для WinUI при необходимости — `microsoft/setup-msbuild`).
2. Тесты, требующие реального окружения (реестр, COM Проводника, права администратора), пометить
   трейтом (например, `[Trait("Category", "Integration")]`) и при необходимости исключить в CI фильтром.
   Не отключать и не удалять их.
3. Добавить бейдж/упоминание в `README.md`, в `AGENTS.md` — что CI — финальная проверка Windows-кода.

## Критерии приёмки

- [ ] Оба job зелёные на `main`.
- [ ] Тесты PostgreSQL в job `shared` выполняются, а не пропускаются (Skipped = 0).
- [ ] Версия SDK берётся из `global.json`.

## Вне рамок

- Публикация релизов, подпись, Android-сборка (добавится в 0009).

## Результат

