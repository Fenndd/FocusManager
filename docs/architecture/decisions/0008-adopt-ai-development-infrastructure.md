# ADR-0008: Adopt the shared AI development infrastructure

- Status: Accepted
- Date: 2026-11-16
- Supersedes: the task-layout and development-process conventions in ADR-0001

## Context

The owner requested the complete AI development infrastructure from the AgentsInfrastructure
template, then clarified that FocusManager must adopt that infrastructure rather than retain
its previous process. All project documentation must be in English. Product requirements and
previously accepted product architecture remain unchanged.

The previous layout used `docs/plans/roadmap.md`, `docs/plans/tasks/NNNN-*.md`, and the task
statuses `todo` and `in-progress`. It did not have a root current-state document. Keeping that
layout alongside the template would create conflicting instructions and duplicate state.

## Decision

- Use the template's shared rules in root `AGENTS.md`, followed by FocusManager context,
  specification pointers, code conventions and verified build commands.
- Keep canonical skills in `.agents/skills/`, thin Claude Code adapters in `.claude/skills/`,
  and the four roleless model/effort presets in `.claude/agents/`.
- Use `CLAUDE.md` and `GEMINI.md` as entrypoints to the shared rules.
- Adopt the template's `docs/engineering/AI_ORCHESTRATION.md`, `DEVELOPMENT_WORKFLOW.md`,
  and `TASK_FORMAT.md` as the canonical processes. T026 subsequently adds the FocusManager execution protocol and corrects workflow paths; the shared tier/model tables and task lifecycle remain unchanged.
- Move the roadmap to root `ROADMAP.md`, tasks to root `tasks/T<NNN>-<slug>.md`, and keep current
  state and handoff information in root `STATUS.md`.
- Map task numbers mechanically: `0001` becomes `T001`, through `0025` becoming `T025`.
  This is a one-time migration, not a new ordering or replacement of task history.
  Existing ADR numbers and requirement IDs do not change.
- Use the exact task format and lifecycle defined in `TASK_FORMAT.md`: `planned`, `ready`,
  `in_progress`, `blocked`, `review`, `done`, `cancelled`. A previous `todo` becomes `ready`
  only when its dependencies are done and it has no unresolved prerequisite questions;
  otherwise it becomes `planned`. Cancelled tasks retain their cancellation and rationale.
- Preserve task scope, dependencies, acceptance criteria, results and useful legacy metadata
  while translating and restructuring them. Preserve the relative order and goals of milestones.
- Translate all existing project documentation into English without changing requirement meaning,
  decision status, historical dates or implementation claims.
- Preserve product specification in `docs/spec/` and architecture in `docs/architecture/`.
  These remain the owning sources; no parallel specification or ADR tree is introduced.
- Merge text-formatting and ignore rules while retaining unrelated local changes. Track shared
  AI infrastructure; continue ignoring personal overrides, caches, credentials and worktrees.

## Alternatives

- Adapt the infrastructure to the previous Russian-language task process. Rejected by the owner's
  explicit clarification: the project must adopt the introduced infrastructure.
- Keep both planning systems. Rejected because their paths, task statuses and state ownership
  would conflict and drift.

## Consequences

- Task selection, delegation and handoffs follow one shared process across supported clients.
- Existing task links and current process references must use the migrated locations and IDs.
  ADR-0001 preserves the original decision as history and identifies the superseding decision.
- Infrastructure files alone do not install tools, change client permissions, configure accounts,
  or prove that clients have loaded the instructions or executed a preset.
- This adoption changes development documentation and workflow only. Application source code,
  runtime behavior, dependencies and build configuration are outside this decision's scope.

## Project Adaptation (2026-11-17, T026)

The owner requested a roadmap/milestone and orchestration review before development. The workflow now points to the existing docs/spec/ and docs/architecture/decisions/ sources. The orchestration policy adds task selection, shared-file ownership, verification, acceptance and handoff rules for FocusManager. The model-routing skill and client presets still point to the single canonical policy; no new runtime, dependency, routing service or product architecture is introduced. T025 Result describes the original transfer checks at that time; it is not a claim that subsequent project adaptations remain byte-identical to the source template.
