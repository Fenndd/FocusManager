---
id: 0006
title: Добавить CI (GitHub Actions)
status: todo
type: chore
platform: all
milestone: M1
priority: medium
depends_on: [0001, 0002]
spec: []
adr: [0003, 0007]
---

## Контекст

CI нет. Агенты часто работают в Linux-контейнерах и не могут проверить Windows-код — CI должен
делать это за них.

## Что сделать

1. `.github/workflows/ci.yml`:
   - job `shared` на `ubuntu-latest`: `dotnet test FocusManager.Shared.slnf`;
   - job `windows` на `windows-latest`: сборка и тесты `FocusManager.Windows.slnf`
     (для WinUI при необходимости — `microsoft/setup-msbuild`, пока WinUI не удалён).
2. Тесты, требующие реального окружения (реестр, COM Проводника, права администратора), пометить
   трейтом (например, `[Trait("Category", "Integration")]`) и при необходимости исключить в CI фильтром.
   Не отключать и не удалять их.
3. В `AGENTS.md` — что CI является финальной проверкой Windows-кода.
4. Подготовить место для следующих job: интерфейс Tauri (задача 0017), Linux-агент (0020), Android (0009).
   Каждая из этих задач добавляет свой job.

## Критерии приёмки

- [ ] Оба job зелёные на основной ветке.
- [ ] Версия SDK берётся из `global.json`.

## Вне рамок

- Публикация релизов, подпись.

## Результат
