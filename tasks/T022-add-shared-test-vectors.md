---
id: T022
title: Create shared rule test vectors for all platforms
milestone: M1
status: planned
depends_on: ["T012"]
area: ["test-vectors/**", "tests/Shared/FocusManager.Core.Tests/**", "AGENTS.md"]
---

# T022 — Create shared rule test vectors for all platforms

## Goal

Provide common rule examples that verify equivalent C#, TypeScript, and Kotlin behavior across platforms.

## Sources

Legacy classification: type `chore`; platform `all`; priority `medium`.

Requirement identifiers: `APP-002`, `SITE-002`, `SITE-003`, `FLD-002`, `FLD-003`, `PRF-004`.

Legacy ADR associations: ADR-0005.

[Application rules](../docs/spec/apps.md), [site rules/input](../docs/spec/sites.md), [folder rules](../docs/spec/folders.md), [profiles](../docs/spec/profiles.md), and [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md).

There is no production code shared by every platform. Equivalent rule behavior is established through common examples; C# desktop rules, TypeScript desktop input validation, and Kotlin Android implementations consume them.

## Scope

In scope:
1. Create root `test-vectors/` with JSON input/expected-result examples for site matching (SITE-002), site input normalization (SITE-003), application matching (APP-002), folders with/without subfolders (FLD-002, FLD-003), and empty lists (PRF-004). Document the format in a README.
2. Make `tests/Shared/FocusManager.Core.Tests` read these files and test `RuleEvaluator`.
3. State in `AGENTS.md`: rule changes require vector changes; new platforms must run the examples.

Out of scope: TypeScript and Kotlin consumers belong to T018 and Android tasks.

Review handoff (2026-11-18): [T054](T054-fix-windows-path-compatibility-after-review.md) creates the
minimal regression examples required for its Windows compatibility fixes. Preserve/extend those examples
when delivering this complete suite and loader; T054 does not complete T022. Linux-host execution in
criterion 2 is deferred under the owner's explicit Linux hold, not waived or verified. Windows/shared
examples may still be prepared and checked on Windows when this task's dependencies permit it.

## Acceptance Criteria

1. The examples cover all rule branches in the referenced requirements.
2. C# vector-based tests pass on Linux and Windows.

## Checks

- Review each referenced requirement branch against a corresponding input/expected-result case.
- Run `dotnet test FocusManager.Shared.slnf` on Linux and Windows, including the file-driven cases.
- Inspect format instructions and `AGENTS.md` obligations.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
