---
id: T028
title: Resolve simultaneous-profile rule composition
milestone: M5
status: ready
depends_on: []
area: ["docs/spec/profiles.md", "docs/spec/study-mode.md", "test-vectors/**"]
---

# T028 — Resolve simultaneous-profile rule composition

## Goal

Obtain and record the owner decision needed to define the profile model and simultaneous-session semantics.

## Sources

[Profiles](../docs/spec/profiles.md) (PRF-005), [sessions](../docs/spec/study-mode.md) (SM-004, SM-005),
[L1 owner answers](../docs/spec/decisions.md#l1-profiles-default-exceptions-and-protection).

## Scope

In scope: present concrete allowlist/blocklist conflicts, empty categories, ending one of two sessions and live edits; record the owner answer next to PRF-005 and encode approved examples.
Out of scope: choosing a composition rule for the owner, implementation or unrelated product questions.

## Acceptance Criteria

1. PRF-005 contains the explicit owner-approved rule with decision provenance.
2. Approved examples cover overlapping modes, empty lists, ending one session and live edits; T021 links to that rule.

## Checks

- Review the owner answer against the updated specification and examples.
- Check that no proposal is represented as an accepted decision.

## Open Questions

None. The owner confirmed composition in L1 on 2026-11-18; apply PRF-005 and preserve PRF-004 empty-category behavior.

## Result

T061 handoff (2026-11-18): the owner composition answer is recorded in PRF-005 with L1 provenance.
The decision gate is resolved and this task is ready; approved shared examples and the T021 handoff
are still required. No vectors or runtime code were produced by recording the decision; this task
is not complete or accepted.
