# Architecture Decision Records (ADR)

An ADR records a significant architectural decision: context, decision, and consequences.
Decisions are not rewritten retroactively. When a decision changes, write a new ADR
and mark the old one `Superseded by ADR-NNNN` (or identify the superseded part for a partial replacement).

Write an ADR for a new project or layer, a new external dependency/framework, a change in
storage/IPC/UI approach, or a change in project dependency rules.

File name: `NNNN-kebab-case-title.md`, using the next sequential number.

| ADR | Decision | Status |
|-----|---------|--------|
| [0001](0001-docs-spec-plans-layout.md) | Keep specifications, plans, and tasks in the repository under `docs/` | Accepted; task layout and workflow superseded by ADR-0008 |
| [0002](0002-shared-dotnet-code-for-android.md) | Build Android with .NET to reuse shared C# code | Superseded by ADR-0005 |
| [0003](0003-source-tree-by-platform.md) | Split sources into `Shared` / `Windows` / `Android` | Accepted, extended by ADR-0007 |
| [0004](0004-postgresql-storage.md) | PostgreSQL instead of SQLite | Superseded by ADR-0006 |
| [0005](0005-native-stack-per-platform.md) | A separate stack per platform; share the specification and test vectors | Accepted |
| [0006](0006-local-storage-and-firebase.md) | Local storage (SQLite/Room) + Firebase for the cloud | Accepted |
| [0007](0007-desktop-agent-and-tauri-ui.md) | Desktop: background C# agent + Tauri 2 / React UI | Accepted |
| [0008](0008-adopt-ai-development-infrastructure.md) | Adopt the transferred AI development infrastructure, English documentation, and root-level work tracking | Accepted |

## Template

```markdown
# ADR-NNNN: Title

- Status: Proposed | Accepted | Superseded by ADR-NNNN
- Date: YYYY-MM-DD

## Context
The problem and constraints.

## Decision
What was decided.

## Alternatives
What was considered and why it was rejected.

## Consequences
What becomes easier or harder, and what tasks arise.
```
