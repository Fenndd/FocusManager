# ADR-0001: Keep Specifications, Plans, and Tasks in the Repository

- Status: Accepted; task layout and workflow superseded by [ADR-0008](0008-adopt-ai-development-infrastructure.md) (2026-11-16)
- Date: 2026-09-30

## Context

Development is carried out mainly by AI agents. An agent needs to understand from the repository
what the system should do, how the code is structured, and what to do next. The existing implementation
will be both extended and changed, so a place to record the *target* behavior and a clear process for changing it are needed.

## Decision

The following records the original decision. Its `docs/plans/` layout and task workflow are historical;
ADR-0008 replaces them with root-level `ROADMAP.md`, `STATUS.md`, and `tasks/T<NNN>-<slug>.md`,
following [TASK_FORMAT.md](../../engineering/TASK_FORMAT.md). The specification and architecture locations remain current.

- `docs/spec/` — behavior specification, requirements with immutable IDs and per-platform statuses.
- `docs/architecture/` — architecture description and ADRs.
- `docs/plans/roadmap.md` — milestones; `docs/plans/tasks/NNNN-*.md` — tasks, one per file,
  with a status in YAML front matter.
- Root `AGENTS.md` — entry point for agents; `CLAUDE.md` imports it.
- A behavior change = a task + specification changes + code in one PR.

## Alternatives

- **GitHub Issues/Projects for tasks.** Convenient for people, but an agent in an isolated environment
  may not have access, and tasks are not versioned alongside the code. They can be used additionally
  (an issue link in a task field).
- **One large task table (backlog.md).** Frequent merge conflicts when agents work in parallel.
- **Folders by status (`todo/`, `done/`).** Moving files breaks links and also causes conflicts;
  a front-matter status can be found with `grep`.

## Consequences

- Every PR that changes behavior must change `docs/spec/`.
- In the original workflow, task numbers could conflict when created in parallel; the resolution rule
  was in `docs/plans/README.md`. The current task format and lifecycle are defined by ADR-0008 and
  [TASK_FORMAT.md](../../engineering/TASK_FORMAT.md).
