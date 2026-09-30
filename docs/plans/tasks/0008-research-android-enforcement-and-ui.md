---
id: 0008
title: Исследовать механизмы блокировки на Android и выбрать UI-подход
status: todo
type: research
platform: android
milestone: M2
priority: high
depends_on: []
spec: [AND-001, AND-002, AND-003, AND-004, SITE-001, APP-001]
adr: [0002]
---

## Контекст

`docs/spec/platforms/android.md` — черновик с гипотезами. Прежде чем писать код, нужно подтвердить,
какие механизмы реально работают на современных версиях Android и совместимы с .NET for Android.

## Что сделать

1. Для каждой функции (обнаружение запуска приложения, блокировка приложения, фильтрация сайтов,
   работа в фоне, защита от выключения) — сравнить варианты: надёжность, нужные разрешения,
   ограничения Google Play (политика использования Accessibility API), минимальная версия Android.
2. Проверить доступность нужных API из .NET for Android (`AccessibilityService`, `VpnService`,
   `UsageStatsManager`, foreground service).
3. Выбрать UI-подход: нативные Android Views/AndroidX из C# или .NET MAUI (только Android).
   Критерии: объём UI, переиспользование ViewModel из Shared, размер APK, опыт команды.
4. Решить открытые вопросы в `docs/spec/platforms/android.md` и `SM-003` (что такое «снимок» на Android).

## Критерии приёмки

- [ ] ADR-0004 «Механизмы блокировки и UI на Android» со статусом «Принято».
- [ ] `docs/spec/platforms/android.md` переписан из черновика в требования с ID и статусом `planned`.
- [ ] Созданы задачи этапа M3 (MVP Android) в `docs/plans/tasks/`, обновлён `roadmap.md`.

## Вне рамок

- Написание кода приложения (кроме одноразовых прототипов, которые не коммитятся в `src/`).

## Результат

