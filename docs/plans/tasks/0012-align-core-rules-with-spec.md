---
id: 0012
title: Привести правила Core к новой спецификации
status: todo
type: change
platform: shared
milestone: M1
priority: high
depends_on: [0001]
spec: [APP-003, APP-004, PRF-004, SITE-002, SITE-004]
adr: []
---

## Контекст

Решения владельца меняют текущее поведение правил в `src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs`:

| Требование | Сейчас | Должно быть |
|------------|--------|-------------|
| APP-003 (удалено, решение I1) | приложение разрешается и по совпадению имени `.exe` | только по полному пути (APP-002) |
| APP-004, PRF-004 (решение E7) | пустой список приложений блокирует все новые приложения | пустой список — без ограничений |
| SITE-002 (решение I3) | `example.com` — только точный хост | `example.com` — домен и все поддомены; `*.example.com` — то же |

Политика Chrome уже трактует `example.com` с поддоменами, так что для сайтов меняется только `RuleEvaluator`.

## Что сделать

1. Изменить `RuleEvaluator` по таблице.
2. Обновить тесты `RuleEvaluatorTests` и тесты агента (`tests/Windows/FocusManager.Agent.Tests`), которые
   опираются на старое поведение (например, `EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable`).
3. Обновить статусы платформы Windows в `docs/spec/apps.md`, `profiles.md`, `sites.md` на `done`.

## Критерии приёмки

- [ ] Поведение соответствует таблице, покрыто тестами.
- [ ] `dotnet test FocusManager.Shared.slnf` проходит; Windows-тесты — на Windows.
- [ ] Спецификация отражает новое состояние.

## Вне рамок

- Тексты предупреждений в WinUI (SM-006) — экраны переносятся в Tauri (задача 0018).

## Результат
