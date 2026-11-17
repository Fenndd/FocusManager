# Current Status

Updated: 2026-11-17.

## Active Work

- Phase: M0 — development foundation; no product milestone is accepted by this preparation.
- [T026 — Prepare the roadmap and agent orchestration for development](tasks/T026-review-roadmap-and-orchestration.md): `review`, prepared and verified, awaiting owner acceptance.
- [T025 — Adopt the shared AI development infrastructure](tasks/T025-install-ai-development-infrastructure.md): remains `review`; prior work and its acceptance state are preserved.

## Product Baseline

Only the Windows implementation currently exists. The agent still uses PostgreSQL and the UI is WinUI 3.
SQLite, Windows service/Tauri migrations, Linux and Android production implementations are planned,
not completed by this session. Application source, dependencies and build configuration were not changed.

The working tree already contained extensive uncommitted T025 infrastructure/documentation migration
when T026 began. The owner subsequently requested one commit containing both preparation tasks. Generated Python
caches are ignored. No push or external account/service/publication change is part of this request.

## Prepared Foundation

- ROADMAP.md has 13 milestones M0–M12 with observable outcomes and exit criteria; 44 indexed task records.
- T027–T044 split service design, profile decision/schema/migration, platform feasibility/bootstrap/enforcement,
  and make missing feature/account/payment/release preparation explicit. Existing task IDs are retained.
- Later feature gates create one-session implementation children; those children must be added and accepted
  before the corresponding milestone closes. The distant backlog is not yet fully decomposed into production tasks.
- [Requirement coverage](docs/engineering/REQUIREMENT_COVERAGE.md) maps all 106 actual requirement definitions
  to implementation or decision/research/decomposition owners; deferred iOS/partner requirements stay deferred.
- [Orchestration](docs/engineering/AI_ORCHESTRATION.md) retains the tier/model tables and adds FocusManager task
  selection, area ownership, integration, checks, review and handoff. Workflow paths use the actual specification/ADR tree.

## Checks and Limitations

T026 records the actual checks and independent review. Task schema, IDs/titles, all milestone/roadmap
rows, dependency references/readiness, acyclic graph and local links were checked. Specification edits
only add the unresolved release-sequence question; requirement text/statuses are preserved.

.NET, desktop, Linux and Android execution checks were not run for this documentation-only task.
Research mechanisms, store/payment requirements, live Firebase access, every AI client's instruction loading
and actual execution of every copied preset remain unverified. Routing requests are preferences, not an installed scheduler.

## Next Session

1. Inspect Git status/relevant diffs and read this status, roadmap and applicable rules; preserve the combined
   preparation changes. Record acceptance for T025/T026 only if the owner establishes it.
2. Start M1 with [T002](tasks/T002-restore-sqlite-storage.md), currently `ready`: inspect current PostgreSQL
   storage/tests and the historical SQLite source identified in the task. Set task/roadmap/status `in_progress`
   together. Implement only SQLite restoration/platform defaults and its migration/persistence checks.
3. After accepted T002, do T007 SDK migration; T001 path semantics follows the default M1 order. T006 CI
   now requires accepted T001/T002/T007. Do not start dependents while prerequisites remain in `review`.
4. Independent platform/payment research can proceed when its task is ready and ownership does not conflict.
   Research often shares ADR/spec/task-index files: prefer read-only parallel findings and sequential integration.

## Decisions and Obstacles

No technical blocker for the T002 storage task is known. Future decision gates include PRF-005 composition
(T028), Android snapshots and essential-app defaults (T031/T032), protection defaults/feasibility (T034),
content/privacy choices (T035/T039), account/conflict semantics and release chronology (T037), pricing/free tier/
trial/provider choices (T024/T040), and Linux supported environments/packages (T019/T043/T041).

In particular, SYNC retains the first-version/no-cloud statement while PAY requires account-backed access
and paid features from the first public release. The question is recorded in docs/spec/sync.md; T037 must
obtain the owner's clarification before downstream cloud/release work treats any interpretation as approved.
