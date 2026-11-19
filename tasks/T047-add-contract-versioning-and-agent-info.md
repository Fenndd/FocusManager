---
id: T047
title: Add protocol versioning, GetAgentInfo and error codes to the UI-agent contract
milestone: M2
status: planned
depends_on: ["T005", "T027"]
area: ["contracts/**", "src/Windows/FocusManager.Contracts/**", "tests/Windows/FocusManager.Contracts.Tests/**", "tasks/T047-*"]
---

# T047 — Add protocol versioning, GetAgentInfo and error codes to the UI-agent contract

## Goal

Extend the T005 JSON Schema contract with the compatibility mechanisms that the proposed service model needs, before any consumer (T017, T051) depends on them.

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed; D9 and hazard H12; check V9), [study mode / agent availability](../docs/spec/study-mode.md) (SM-007), [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md). Output of [T005](T005-define-contracts-schema.md): the schema layout, the type-generation command and the validation test.

This task implements only what ADR-0009 proposes. If the ADR is not accepted, or an owner answer changes D9, revise this task first.

## Scope

In scope:
1. Envelope field `protocolVersion` (absent means 1) and the compatibility rules in `contracts/README.md`: additive optional fields are compatible; removals and semantic changes raise the major version; unknown fields are ignored in both directions; an unknown command yields `UnknownCommand`.
2. `GetAgentInfo` request and response schema: agent version, supported protocol range, platform, capabilities.
3. `errorCode` values (`InvalidRequest`, `UnknownCommand`, `UnsupportedProtocolVersion`, `Unauthorized`, `Unavailable`, `Internal`) and the rule that the `error` text carries no exception text.
4. Reserved event framing fields (`kind` response or event, `requestId`) as schema only.
5. Transport limits documented in `contracts/README.md` (maximum message size, deadlines, connection model).
6. Enforcement-status fields (whether the caller's session is enforced; per-category state) only with the vocabulary the owner chooses in ADR-0009 Q16. Until Q16 is answered, this item is not started and is recorded as a follow-up.
7. Regenerated C# and TypeScript types and extended schema tests.

Out of scope: server-side behavior (answering `GetAgentInfo`, authorization, sanitizing real errors, enforcing limits) belongs to T051. Subscription and event delivery belong to T014. The helper protocol belongs to T049. The generation mechanism itself belongs to T005. No message that T005 describes changes incompatibly.

## Acceptance Criteria

1. The schema contains `protocolVersion` (optional), `GetAgentInfo`, the `errorCode` set and the event framing fields, and `contracts/README.md` documents the compatibility rules and transport limits.
2. Regenerating C# and TypeScript types with the T005 command produces no diff against the committed files.
3. Schema tests pass for: a document with unknown fields; a request without a version (the WinUI client); an error response for each `errorCode`; a document with an unsupported major version, which the validation helper rejects.
4. Existing T005 test documents still validate unchanged.
5. If Q16 is answered, the status fields validate with the chosen vocabulary; otherwise the Result records that item 6 was not done.

## Checks

- Run the generation command established by T005 (it does not exist until T005 is complete) and confirm a clean diff.
- Run `dotnet test FocusManager.Windows.slnf` on Windows, including the schema compatibility tests.
- Inspect `contracts/README.md` for the rules and limits; compare the schema with ADR-0009 D9.

## Open Questions

ADR-0009 must be accepted first (T027). ADR-0009 Q9 and Q16 (which states the UI may show and the status vocabulary) gate item 6. Everything else follows the proposal.

## Result

Not started. No implementation or checks have been completed for this task.
