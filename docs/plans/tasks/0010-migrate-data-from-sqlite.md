---
id: 0010
title: Перенести данные из старой SQLite-базы в PostgreSQL
status: cancelled
type: feature
platform: windows
milestone: M1
priority: low
depends_on: []
spec: [WIN-006]
adr: [0004]
---

## Контекст

После перехода на PostgreSQL (ADR-0004) старые данные из SQLite не были видны агенту.

## Результат

**Отменена (2026-11-16).** PostgreSQL убирается, хранилище снова SQLite (ADR-0006, задача 0002),
совместимое со старой базой — переносить нечего.
