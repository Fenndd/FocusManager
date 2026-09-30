---
id: 0009
title: Создать каркас Android-проектов
status: todo
type: feature
platform: android
milestone: M2
priority: medium
depends_on: [0002, 0007, 0008]
spec: [AND-001]
adr: [0002, 0003]
---

## Контекст

Нужна пустая, но собирающаяся Android-версия, подключённая к общему коду, чтобы следующие задачи
добавляли функциональность, а не инфраструктуру.

## Что сделать

1. Создать проекты (UI-подход — по ADR из задачи 0008):
   - `src/Android/FocusManager.Platform.Android` — интеграции с ОС (пока пустые реализации портов из Core);
   - `src/Android/FocusManager.Android` — приложение (ссылается на Core, Persistence.Sqlite, Platform.Android).
2. Добавить их в `FocusManager.sln` (папка `src/Android`) и создать `FocusManager.Android.slnf`
   (Shared + Android). В `FocusManager.Windows.slnf` Android-проекты не добавлять.
3. Приложение: один экран с переключателем режима учёбы, состояние сохраняется через
   `SqliteWhitelistStore` в каталоге данных приложения (проверка, что общий код работает на устройстве).
4. `scripts/android/` — скрипт сборки/установки на эмулятор (если нужен).
5. Обновить `AGENTS.md` (команды сборки Android), `src/Android/README.md`, `docs/architecture/README.md`.
6. Добавить Android-сборку в CI (если задача 0006 выполнена).

## Критерии приёмки

- [ ] `dotnet build FocusManager.Android.slnf` проходит на машине с workload `android`.
- [ ] Приложение запускается на эмуляторе, переключатель сохраняет состояние после перезапуска.
- [ ] `dotnet build FocusManager.Windows.slnf` не требует Android workload.

## Вне рамок

- Реальная блокировка приложений/сайтов — задачи M3.

## Результат

