---
id: T025
title: Adopt the shared AI development infrastructure
milestone: M0
status: review
depends_on: []
area: ["AGENTS.md", "CLAUDE.md", "GEMINI.md", ".agents/**", ".claude/**", ".editorconfig", ".gitattributes", ".gitignore", "docs/**", "tasks/**", "ROADMAP.md", "STATUS.md", "README.md", "src/**/README.md"]
---

# T025 — Adopt the shared AI development infrastructure

## Goal

Install the complete local AgentsInfrastructure template and adapt FocusManager to its canonical processes and task/roadmap/status format. Translate all project documentation into English without changing product requirements.

## Sources

Legacy classification: type `chore`; platform `all`; priority `medium`.

Legacy ADR associations: ADR-0001.

The owner requested transfer of all infrastructure from `C:\File Explorer\Work\AgentsInfrastructure`; its `README.md`, `INFRASTRUCTURE_TRANSFER.md`, and `docs/engineering/TASK_FORMAT.md` define the source inventory and format. FocusManager already has agent rules, specifications, ADRs, and task records established by [ADR-0001](../docs/architecture/decisions/0001-docs-spec-plans-layout.md).

The owner's explicit correction supersedes the initial Russian-documentation adaptation: all documentation must be in English, and task/state files must follow the source template exactly. [Task format](../docs/engineering/TASK_FORMAT.md), [workflow](../docs/engineering/DEVELOPMENT_WORKFLOW.md), and [orchestration policy](../docs/engineering/AI_ORCHESTRATION.md) govern the adaptation. ADR-0008 records the infrastructure/task-format change.

## Scope

In scope:
1. Transfer canonical skills, their supporting files/licenses, thin Claude Code adapters, and four model presets.
2. Merge shared instructions with FocusManager `AGENTS.md`; update Claude and Gemini entry points.
3. Retain the three canonical process documents unchanged and adapt FocusManager's documentation and work records to them. Translate all project documentation into English, preserving product requirements and recorded uncertainty; English documentation does not remove the specified English/Russian product localization.
4. Migrate all 25 existing task records from `docs/plans/tasks/NNNN-*.md` to root `tasks/TNNN-*.md`, preserving numeric correspondence, slugs, dependencies, scope, acceptance criteria, cancellations, results, and legacy classification/requirement/ADR context. Use exactly the canonical fields and sections; derive `planned` / `ready` states from dependencies and unresolved questions.
5. Move the canonical roadmap to root `ROADMAP.md` with milestone goals/exit criteria and task title/dependency/status tables. Add root `STATUS.md` for current work only; remove superseded planning copies after verifying the migrated records. Update references and document the change in ADR-0008.
6. Merge text/Git settings while preserving existing uncommitted changes.
7. Check inventory, translations, links, adapters, model routing, task schema/dependencies, and final diff.

Out of scope:
- Product behavior, source-code implementation, product stack, and application architecture changes. Requirement meaning stays unchanged; documentation layout/process decisions are in scope.
- Installing clients or configuring accounts, permissions, MCP, or global settings.
- Commit, push, or changes to the source template.

Planning change: the initial task asked to adapt process documents to Russian and keep legacy paths/statuses. The owner's explicit correction requires English throughout and the template's root task/roadmap/status format; that correction is the reason for the revised scope and criteria.

## Acceptance Criteria

1. The complete transfer inventory is installed; the source template is unchanged.
2. FocusManager requirements and platform/code rules are preserved; project documentation is English.
3. All 25 task records and the task template use exactly the canonical front matter, heading order, identifiers, status semantics, and root locations; task dependencies and substantive content are preserved.
4. Root `ROADMAP.md`, root `STATUS.md`, task records, links, and ADR-0008 agree; superseded planning/state copies are removed after migration verification.
5. Skills, supporting files, licenses, and adapters match the template or carry an explicit project adaptation.
6. The four presets agree with the routing tables.
7. Local links, client entry points, ignore rules, English-language content, and the diff have been checked.

## Checks

- Compare source `README.md` / `INFRASTRUCTURE_TRANSFER.md` inventory to installed files, including skill resources/licenses/adapters and four presets; confirm the source template was not changed.
- Validate every task's exact front matter fields, required headings/order, ID/title/file correspondence, statuses, dependency mapping, and `area`; compare the migrated content with each original before removing the old copy.
- Check root roadmap/state/task consistency and local Markdown links; search project documentation for remaining Cyrillic text and legacy planning references, examining any legitimate exceptions rather than deleting product requirements.
- Inspect `git diff --check`, final diff, client instructions, routing tables, and preserved pre-existing changes.
- .NET checks are not required for documentation/instruction-only changes: application behavior, implementation, and build configuration do not change.

## Open Questions

None

## Result

Implementation and verification completed on 2026-11-17; awaiting owner acceptance (`review`).

Installed the complete transferable infrastructure: four canonical skills with references, metadata
and licenses, four thin Claude Code skill adapters, four roleless model/effort presets, Claude and
Gemini entry points, and the three unchanged engineering process documents. Root `AGENTS.md`
retains the complete shared template baseline and adds FocusManager context and build/code rules.
The source template was used read-only.

Translated all 20 specification files and the existing architecture, documentation index and Android
README into English. Preserved the 106 actual requirement definitions, requirement/decision IDs,
platform statuses, dates, owner decisions, unresolved questions and historical ADR rationale.
Product UI language requirements still specify English and Russian.

Migrated all 25 task records and the template to root `tasks/`, with the canonical fields/sections
and lifecycle. Preserved numeric correspondence, slugs, dependencies, milestones, legacy metadata,
technical identifiers, substantive scope/criteria, and cancellation records. Removed verified legacy
copies and the temporary migration helper. Root `ROADMAP.md` now owns milestone goals, exit criteria
and task tables; root `STATUS.md` owns current state and handoff. ADR-0008 records the process
migration and partially supersedes ADR-0001. T008 retains its Android ADR deliverable but uses the
next available ADR number because ADR-0008 now records infrastructure adoption.

Merged text/Git settings. Pre-existing `.gitignore` entries were retained; explicit exceptions allow
shared infrastructure to be tracked while personal overrides, caches and secrets remain ignored.

Verified:

- All 32 skill/adapter/preset/process payload files match the source byte-for-byte; the complete
  shared AGENTS baseline and both client entry points match their source. Adapter metadata and
  canonical targets, included resources/licenses, and preset/routing values are consistent.
- All 92 project/infrastructure Markdown files passed the English-language scan and required
  local-link checks. Legacy planning paths remain only in explicit historical/migration descriptions.
- All 25 tasks have the exact six front-matter fields, required section order, matching IDs/titles,
  mapped dependencies, valid statuses, and matching roadmap links/titles/statuses. The dependency
  graph is acyclic; no `ready` task has an unfinished dependency or unresolved question.
- Original task classifications, milestones, specification/ADR associations and dependencies were
  compared before removing legacy files. Technical code spans were retained except the intentional
  task/roadmap path migration. Specification requirement/decision IDs, platform-status sequences,
  dates and external URLs were compared with Git HEAD; all checks passed.
- All 15 Git-ignore cases passed, covering shared files and personal/secret paths. Final diff review
  and `git diff --check` passed. Application implementation and build configuration are unchanged.

Delegation: tier 3, requested `gpt-6.1-sol` / `high`, for specification translation, architecture
translation and task migration. This tier was selected because the work preserves relationships
across documents and translation meaning cannot be fully checked mechanically. Independent
read-only reviews of the infrastructure and T001–T010 reported no actionable findings. Service
usage limits interrupted the task worker's final reporting and the remaining-task review follow-up;
the main agent completed integration, reviewed T011–T025 against their sources, removed temporary
files and ran the final checks. Actual execution parameters, elapsed-time and token totals were
not exposed by the collaboration tool; the values above are the requested routing settings.

Not verified: .NET tests (not applicable to this documentation-only change), loading by every AI
client, or actual execution of every copied preset. No clients were installed, permissions changed,
external resources modified, or commits/pushes made. No remaining implementation blocker is known.
