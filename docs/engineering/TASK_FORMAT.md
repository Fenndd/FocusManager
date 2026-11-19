# Tasks and Roadmap

This document defines how planned work is recorded so that a person or an orchestrating agent can choose the next task, see what can run in parallel, and verify a result without rereading the whole history. It sets the format only; it does not create a roadmap or tasks.

## Files

| File | Answers | Owner of the facts |
| --- | --- | --- |
| `ROADMAP.md` | What is ahead: milestones in order and the tasks in each | Order, milestone goals and exit criteria, task index |
| `tasks/T<NNN>-<slug>.md` | What exactly one task requires and what came out of it | Everything about that task |
| `STATUS.md` | What is happening now: active milestone and task, obstacles, next step | Current state only |

Requirements, architecture, and decisions live in their own documents. Tasks and the roadmap link to them and do not restate or change them.

## Milestones

A milestone is a group of tasks that ends in a state that can be run or checked as a whole, not merely a count of finished tasks. Each milestone is a natural acceptance and review point.

Each milestone in `ROADMAP.md` has:

- an identifier `M<k>` and a short name;
- a goal: what becomes possible when it is done;
- exit criteria: observable conditions that are checked before the milestone is accepted;
- its tasks.

## Task Identifiers and Size

- Identifiers are `T001`, `T002`, … in creation order. They are never reused or renumbered, even if a task is cancelled or moved to another milestone.
- The file name is the identifier plus a short kebab-case slug, for example `T007-session-storage.md`. The slug may change; the identifier may not.
- A task is small enough to be completed and verified in one working session, and large enough to produce a result that can be checked on its own. If it cannot be verified without another task, merge them or split differently.

## Task File

Every task file starts with this front matter and contains these sections in this order.

```markdown
---
id: T000
title: Short imperative title
milestone: M0
status: planned
depends_on: []
area: []
---

# T000 — Short imperative title

## Goal

The result this task produces, in one or two sentences.

## Sources

Links to the requirements, specification sections, and decisions this task implements. Do not restate them here.

## Scope

In scope: what this task changes.
Out of scope: what it deliberately leaves to other tasks.

## Acceptance Criteria

1. Observable conditions only: behavior, outputs, files, or check results that anyone can confirm.

## Checks

The exact commands or manual steps that confirm each criterion. If a required command does not exist yet, say so; do not invent one.

## Open Questions

Decisions this task needs from the user or from another document. Write "None" if there are none.

## Result

Filled in when implementation is complete: what was done, changed files, checks run and their outcome (verified / not verified / failed), limitations, and, if delegated, the tier, model, and effort used. Record acceptance separately: who accepted the result and when, or the explicit prior authorization and the evidence that its conditions were met. Record any explicit instruction allowing dependent work before acceptance, with its scope and the prerequisite's actual state.
```

### Front Matter Fields

| Field | Meaning |
| --- | --- |
| `id` | The task identifier; equals the file-name prefix. |
| `title` | Short imperative title; matches the heading and the roadmap entry. |
| `milestone` | The milestone the task belongs to. |
| `status` | One of the statuses below. |
| `depends_on` | Identifiers of tasks that must be accepted (`done`) before this one can start, unless an explicit user instruction permits continuation before acceptance as described below. List only real dependencies: an output this task needs, or a change it builds on. |
| `area` | Implementation paths or glob patterns assigned to the task's worker, for example `src/auth/**`, plus its task file when that worker is assigned to edit it. State them precisely enough to check parallel editing boundaries. Shared `ROADMAP.md` and `STATUS.md` are owned by the main agent and are not automatically part of every worker's area. |

### Statuses

| Status | Meaning |
| --- | --- |
| `planned` | Defined, but a dependency is not accepted and has no explicit continuation instruction, or an open question is unresolved. |
| `ready` | All dependencies are accepted (`done`), or explicitly authorized for continuation before acceptance, and there are no open questions. |
| `in_progress` | Work has started. |
| `blocked` | Work started but cannot continue; the reason is recorded in the task and in `STATUS.md`. |
| `review` | Implementation is complete, evidence and limitations are recorded, and acceptance is pending. User acceptance is the default. |
| `done` | Acceptance criteria met, checks recorded in Result, and accepted by the user or under explicit prior acceptance authority whose conditions are met and recorded. |
| `cancelled` | Will not be done; the reason is recorded in the task. The identifier stays reserved. |

When a status changes, the main agent keeps the task's front matter and its roadmap entry consistent in the integration change. A worker may update its assigned task file or return a proposed status, but must not concurrently edit the shared roadmap. Each task file has one writer at a time; the main agent integrates final facts after the writer hands it back.

Implementation completion moves a task to `review`, not automatically to `done`. A general request to implement work is not acceptance authority. Independent review is evidence, not acceptance. If acceptance was explicitly authorized in advance, record the user instruction, its scope and conditions, and how those conditions were met before marking `done`.

## Order and Parallelism

- A task can start only when it is `ready`.
- Starting dependent work before acceptance requires an explicit user continuation instruction. Record that instruction and its scope in the prerequisite and dependent task records; keep the prerequisite's actual status, such as `review`. This exception permits the stated continuation, not acceptance or blanket continuation of the backlog.
- Two `ready` tasks may run in parallel only if neither depends on the other, directly or through other tasks, and their `area` patterns do not overlap. If either `area` is empty or unclear, run them sequentially.
- Workers edit their assigned areas only. The main agent serializes shared roadmap and current-status updates, so those bookkeeping files do not create an artificial overlap between independent worker areas. Any other shared file still needs a single writer or sequential integration.
- The order of tasks in `ROADMAP.md` is the default order among `ready` tasks. Dependencies, not list position, decide what is allowed.

## ROADMAP.md Layout

```markdown
# Roadmap

## M1 — Short name

Goal: what becomes possible.

Exit criteria:
1. Observable condition.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T001](tasks/T001-slug.md) | Short imperative title | — | planned |
```

## Rules

- Acceptance criteria must be observable. Words such as "works well", "clean", or "robust" are not criteria unless they are tied to a check.
- A task does not introduce new requirements or decisions. If planning or implementation reveals one, record it as an open question and resolve it in the owning document first.
- Changing a task's goal, scope, or acceptance criteria after it is `ready` is a planning change: record the reason in the task and update dependents if they are affected.
- Finished tasks keep their Result section; do not delete task files to tidy the folder.
