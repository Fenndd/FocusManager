---
id: T028
title: Resolve simultaneous-profile rule composition
milestone: M5
status: planned
depends_on: []
area: ["docs/spec/profiles.md", "docs/spec/study-mode.md", "test-vectors/**"]
---

# T028 — Resolve simultaneous-profile rule composition

## Goal

Obtain and record the owner decision needed to define the profile model and simultaneous-session semantics.

## Sources

[Profiles](../docs/spec/profiles.md) (PRF-005), [sessions](../docs/spec/study-mode.md) (SM-004, SM-005).

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

PRF-005 is unresolved. Ask the owner for the composition rule; do not default to the proposed strictest-rule policy.

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
