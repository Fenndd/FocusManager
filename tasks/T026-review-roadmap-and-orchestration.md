---
id: T026
title: Prepare the roadmap and agent orchestration for development
milestone: M0
status: review
depends_on: []
area: ["tasks/**", "ROADMAP.md", "STATUS.md", "docs/engineering/**", "docs/architecture/decisions/0008-adopt-ai-development-infrastructure.md", "AGENTS.md", "docs/spec/sync.md"]
---

# T026 — Prepare the roadmap and agent orchestration for development

## Goal

Audit and improve the development backlog and project-specific agent coordination so the next session can select and verify a bounded implementation task.

## Sources

Owner requests on 2026-11-17: prepare the foundation in this session, check and refine the roadmap/task decomposition, establish an AI orchestration policy, and divide development into milestones.
[Task format](../docs/engineering/TASK_FORMAT.md), [workflow](../docs/engineering/DEVELOPMENT_WORKFLOW.md), [orchestration](../docs/engineering/AI_ORCHESTRATION.md), [specification](../docs/spec/README.md), [architecture](../docs/architecture/README.md), and [roadmap](../ROADMAP.md).

## Scope

In scope: task coverage, sizing, dependencies, readiness, milestone exit criteria, project-specific orchestration and next-session handoff. Preserve existing uncommitted infrastructure work and requirement meaning.
Out of scope: application implementation, new product decisions, live platform feasibility research, deployment, commits and pushes. T025 retains its own acceptance state.

## Acceptance Criteria

1. Confirmed functional areas have explicit task ownership or an explicit research/decision/decomposition gate; a foundation task does not imply feature completion.
2. Immediate implementation tasks have verifiable boundaries and consistent dependencies/readiness; oversized work is split or explicitly gated before execution.
3. Orchestration covers task selection, safe parallelism, model routing, integration, review, escalation and handoff with FocusManager paths and checks.
4. Task schema, links, roadmap consistency and dependency cycles are checked; STATUS identifies the concrete next step and unresolved owner decisions.

## Checks

- Validate task metadata, sections, roadmap rows, dependencies, readiness and graph cycles.
- Check local Markdown links in changed documentation and inspect requirement coverage against docs/spec/.
- Review agent entrypoints, routing/presets and workflow for consistency; inspect the final diff and run git diff --check.
- No .NET tests are required for documentation-only changes.

## Open Questions

None for this preparation task. Future product questions stay in the owning specification and gate their implementation tasks.

## Result

Prepared on 2026-11-17; awaiting owner acceptance (`review`).

Reorganized ROADMAP.md into 13 acceptance milestones M0–M12 at the owner's explicit request. Added T027–T044
and refined existing scopes/dependencies without renumbering task IDs. The complete index now has 44 tasks.
Separated service/session design from implementation, profile composition/schema from migration, Android
application/service research from website and advanced research, and Linux host/IPC from enforcement and
folder/notification/packaging research. Corrected the missing T020/T017 dependency, CI/SDK and UI-removal
prerequisites, unconditional shared-vector obligation and persistence-only storage criteria.

Added explicit planning owners for profiles, triggers/timers, protection, content, statistics, accounts/sync,
privacy/telemetry, billing/entitlements and release packaging/parity. These are gates that create bounded
implementation children; their completion does not mean production features are implemented. Milestone
exit criteria require the corresponding implementation children and observable platform checks to be accepted.

REQUIREMENT_COVERAGE.md links all 106 actual requirement definitions in 18 specification files to task
owners/gates. Removed/deferred requirements are explicit. Added only a question to docs/spec/sync.md about
the unresolved first-version/no-cloud versus first-public-paid-release chronology. No product choice was made.
T023 no longer assumes email/Google sign-in or authorizes telemetry collection through project setup.

AI_ORCHESTRATION.md now contains the FocusManager execution protocol: task/readiness selection, shared-file
ownership, bounded briefs, sequential integration, meaningful checks, independent review, escalation and
acceptance/handoff. Model/effort tier tables, canonical skills and client presets remain unchanged. Corrected
DEVELOPMENT_WORKFLOW.md specification/ADR paths; recorded the project adaptation in ADR-0008. Added the
coverage pointer to AGENTS.md and a concrete next-session handoff in STATUS.md. T025 remains in review.

Verified:

- All 44 task records: exact six metadata fields, required seven sections in order, matching filename/ID/title,
  valid statuses, roadmap title/status/dependency/milestone agreement, unique complete task index.
- Dependency references, acyclic graph and readiness against predecessor state; no ready task has unfinished
  dependencies. No implementation task is promoted from research or an unresolved owner choice.
- All 106 actual requirement IDs are represented in the coverage map; specification definition IDs/statuses
  remain unchanged by T026 (the SYNC addition is an explicitly unresolved question).
- Local Markdown links in task/process/roadmap/status and changed specification/ADR/instructions were checked.
- Source inspection confirmed current project layout and relevant session-0/contract/storage boundaries.
  Final content review and git diff --check passed, including explicit checks of newly created/untracked text.

Independent read-only review: tier 3, requested gpt-6.1-sol / high, fork_turns none. Cross-document planning
contracts and partial mechanical verifiability justified this tier. Review checked specification coverage,
missing dependencies, task size and false completion/decision risks. Findings were checked in original
sources and incorporated, including the chronology question and stricter T037 decision/contract criteria.
The collaboration tool did not expose actual execution settings, elapsed-time or token totals; requested
settings are not presented as confirmed execution parameters.

Not verified: application tests/manual behavior (not applicable to this documentation-only task), platform
prototype feasibility, changing external store/payment/legal requirements, live account configuration,
client loading or execution of every preset. No application source/dependency/build changes, external
resource changes, commits or pushes were made. Existing uncommitted T025 work was preserved. Later feature
implementation still requires the bounded child tasks identified by the planning gates and owner decisions.

