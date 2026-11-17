---
id: T005
title: Define UI-agent JSON Schema contracts and generate types
milestone: M2
status: ready
depends_on: []
area: ["contracts/**", "src/Windows/FocusManager.Contracts/**", "src/Windows/FocusManager.App/Services/AgentClient.cs", "src/Windows/FocusManager.Agent/Services/AgentHostedService.cs", "tests/Windows/**", "AGENTS.md"]
---

# T005 — Define UI-agent JSON Schema contracts and generate types

## Goal

Make JSON Schema the source of truth for current desktop UI-agent messages and share DTO mapping instead of maintaining duplicate definitions.

## Sources

Legacy classification: type `refactor`; platform `windows`; priority `high`.

Requirement identifiers: `SM-007`.

Legacy ADR associations: ADR-0007.

[Study mode / agent availability](../docs/spec/study-mode.md) and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

The desktop UI will use TypeScript / Tauri and the agent C#. Messages currently exist only in C# (`src/Windows/FocusManager.Contracts/Messages.cs`), with duplicated DTO-to-model mapping in `src/Windows/FocusManager.App/Services/AgentClient.cs` and `AgentHostedService.cs`.

## Scope

In scope:
1. Create root `contracts/` with JSON Schema for current messages (`AgentRequestEnvelope`, `AgentResponseEnvelope`, whitelist DTOs), plus versioning rules in `contracts/README.md`.
2. Set up C# and TypeScript type generation. Do not edit generated code manually; document the generation command in `contracts/README.md` and `AGENTS.md`.
3. Replace handwritten DTOs in `FocusManager.Contracts` with generated types, or test that the handwritten types match the schema.
4. Keep DTO-to-Core mapping in one place (Contracts or agent) and remove the copy in `AgentClient`.
5. Add a test that validates agent-serialized JSON against the schema.

Out of scope:
- Changes to the message format or transport.
- Firebase synchronization documents (T021).

## Acceptance Criteria

1. JSON Schema is the single source of message format truth.
2. `dotnet test FocusManager.Windows.slnf` passes on Windows.

## Checks

- Run the generation command established by this task; it does not exist yet and must be documented in `contracts/README.md`.
- Run `dotnet test FocusManager.Windows.slnf` on Windows, including schema validation.
- Inspect mapping ownership, generated-file instructions, and documented compatibility/versioning rules.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
