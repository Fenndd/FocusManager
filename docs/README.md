# FocusManager Documentation

```
docs/
├── spec/                     ← WHAT the system does (source of truth for behavior)
│   ├── README.md             ← specification rules, IDs, statuses, platforms
│   ├── decisions.md          ← product-owner decisions (questions → answers)
│   ├── product.md            ← purpose, audience, glossary, general requirements (GEN-*)
│   ├── profiles.md           ← profiles and allowlist/blocklist modes (PRF-*)
│   ├── triggers.md           ← activation: manual, schedule, limits, location (TRG-*)
│   ├── study-mode.md         ← sessions (SM-*)
│   ├── apps.md               ← applications (APP-*)
│   ├── sites.md              ← sites (SITE-*)
│   ├── folders.md            ← folders, desktop only (FLD-*)
│   ├── content.md            ← keywords, application sections, other apps' notifications (CNT-*)
│   ├── notifications.md      ← FocusManager notifications (NTF-*)
│   ├── stats.md              ← user statistics (STAT-*)
│   ├── sync.md               ← account and synchronization (SYNC-*)
│   ├── telemetry.md          ← technical analytics and privacy (TEL-*)
│   ├── protection.md         ← levels of protection against circumvention (PROT-*)
│   ├── monetization.md       ← monetization (PAY-*)
│   └── platforms/
│       ├── windows.md        ← Windows (WIN-*)
│       ├── linux.md          ← Linux, draft (LNX-*)
│       ├── android.md        ← Android (AND-*)
│       └── ios.md            ← iOS, deferred (IOS-*)
├── architecture/             ← HOW the code is structured
│   ├── README.md             ← layers, projects, dependency rules, code placement
│   └── decisions/            ← ADRs: recorded architectural decisions
└── engineering/              ← development processes and task format

ROADMAP.md                    ← milestones in order, goals, exit criteria, task index
STATUS.md                     ← current state, obstacles, next step
tasks/T<NNN>-<slug>.md         ← one task per file
```

`ROADMAP.md`, `STATUS.md`, and `tasks/` are at the repository root.
[ADR-0008](architecture/decisions/0008-adopt-ai-development-infrastructure.md) supersedes the original
`docs/plans/` layout recorded in ADR-0001.

## How the Specification, Architecture, and Plans Relate

- The **specification** describes the product's *target* behavior independently of the code.
  Each requirement has an ID (`APP-003`) and an implementation status for each platform.
- The **architecture** describes how code is organized into projects and why (ADRs).
- A **task** is a unit of work. Its Sources section links to specification requirements
  and, where needed, ADRs. A task does not replace the specification: every behavior change it makes
  must be recorded in `docs/spec/`.
- [ROADMAP.md](../ROADMAP.md) owns milestone order and the task index;
  [STATUS.md](../STATUS.md) records the current state.
  [TASK_FORMAT.md](engineering/TASK_FORMAT.md) defines task fields, sections, statuses, and lifecycle.

## Changing Behavior (Including Existing Behavior)

1. Create a task in `tasks/T<NNN>-<slug>.md` using [TASK_FORMAT.md](engineering/TASK_FORMAT.md).
   Resolve its dependencies and open questions before marking it `ready`; mark it `in_progress` when work starts.
2. In the same branch, update requirements in `docs/spec/`:
   - a new requirement gets a new ID;
   - an existing requirement keeps its ID when edited; reset affected platform statuses to `planned` / `partial`;
   - a removed requirement keeps its ID and is marked `Removed (YYYY-MM-DD, task TNNN)` so that the ID is never reused.
3. Implement the behavior, update tests, and set the specification's platform statuses to `done` when verified.
4. Record the result, changed files, checks, and limitations in the task's Result section.
   Use `review` while awaiting acceptance and `done` when accepted; update the roadmap status in the same change.

Requirement history is available through `git log -p docs/spec/<file>.md`.
