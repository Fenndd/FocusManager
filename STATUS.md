# Current Status

Updated: 2026-11-19.

## Current Review Remediation — 2026-11-19

The owner requested completion of the existing review follow-up without starting new development stages.
[T063](tasks/T063-resolve-uncommitted-rereview-findings.md) is `in_progress` to resolve the two P2
findings. T056 is reopened in `in_progress` to correct two issues found in the independent re-review of
its applied technical clarifications. T055 remains blocked on an isolated Windows environment; the Linux
hold and acceptance gates remain.

## Latest Review Recording — 2026-11-19 (historical)

[T062](tasks/T062-record-uncommitted-rereview-and-handoff.md) is `review`: it records the completed independent
[2026-11-19 rereview](docs/reviews/2026-11-19-uncommitted.md) and its
[retained evidence](docs/reviews/evidence/2026-11-19/README.md). The verdict is **Changes required**:
two P2 findings (extra-leading-separator UNC compatibility and contradictory operative handoff), no confirmed
P0/P1. [T063](tasks/T063-resolve-uncommitted-rereview-findings.md) is `ready`, prepared but not started;
the owner reserves the actual fixes for another session. Do not start them or advance the feature backlog
from this recording request. Older Product Baseline/Next Agent instructions below are the reviewed stale
text and must not be treated as authority to restart T054/T056; this notice is navigation mitigation only,
not closure of the handoff finding. T055 remains blocked; Linux hold and acceptance gates remain.

The completed review verified Shared 297 tests, Windows 411 tests in each of PowerShell/Git Bash, and the
full Windows/WinUI MSBuild build in an isolated copy. Real agent/UI startup/restarts, Linux-host execution
and service runtime assumptions remain unverified. T062's recording-only checks are in its Result; these
earlier test runs were not rerun merely to save the report. The next action is the owner's separate assignment
of T063, preserving T055's environment blocker and T058's design/acceptance gates.

The external automatic continuation watcher for this chat has stopped; its STOP marker is present.

## Confirmed Owner Answers — 2026-11-18

Read [K1–K4](docs/spec/decisions.md#k-owner-answers-on-service-behavior-android-and-cloud--2026-11-18)
before relying on older unanswered-question or proposal text. One service serves all levels; Windows
users keep separate data and enforcement is confined to the associated OS session. Startup settings
are independent from starting new blocking sessions; active-session recovery rechecks forbidden running
applications. Restrictions must also be cleared after service crash/forced termination by an independent
mechanism. Android follows accepted compliant/verified limits. Cloud is permitted from the first version
when needed, prioritizing zero/minimal expenses, without paid-service activation authority. Opera's simple
Chrome-style policy mechanism must be tested before selecting a more complex approach.

[T058](tasks/T058-reconcile-service-design-with-owner-decisions.md) is planned after T056 to reconcile
these product decisions with the still-Proposed ADR and task design. Do not overwrite the concurrent
T054/T055/T056 work or infer architecture/implementation acceptance. [T059](tasks/T059-research-optional-windows-account-switch-restriction.md)
is planned under T034 for the optional switching restriction (new PROT-012). [T060](tasks/T060-estimate-cloud-costs-before-enabling-paid-services.md)
is ready for cost research only. No source implementation, service/database/billing change, or Linux work
is performed by T057.

## Latest Owner Answers — Profiles and Independent Device Sessions

Read [L1–L6](docs/spec/decisions.md#l-owner-profile-offline-billing-and-privacy-answers--2026-11-18)
before any earlier open-question/proposal text. Profile restrictions combine; no seeded profiles exist.
T061 selects the removable Android/Windows ordinary defaults in APP-007 and the Default Exceptions view.
Emergency defaults are once/day with a five-minute wait; friction remains pause/phrase with screen-based
pausing. The owner confirmed separately displayed protected technical OS components and a countdown that
progresses only with the FocusManager waiting screen visible. Both T061 clarification blockers are resolved.
Startup uses last-used or explicitly selected
profiles when the separate new-blocking toggle is enabled.

A shared account is not a shared live session: SYNC-004 is removed, and device-local blocking sessions are
independent. Google/email sign-in, latest-change-wins and offline local functions are decided. Firebase/Blaze
is permitted if needed with EUR 0 target and EUR 5/month accepted operating-budget ceiling; billing is not
changed by these documents. Mobile payments use Google Play/App Store and desktop Stripe, subject to actual
Store distribution compatibility checks. Exact prices/features/limits/remaining trial choices are held
until the owner explicitly requests them. Popular blocked-item collection and general visible-text keyword
analysis are deferred. Consent is requested at registration, with no inferred guest consent. CNT-003 requires
a local Blocked notifications tab available during active sessions; retention/sensitive-display details
remain to be defined before storage implementation. T059's separate account-switch setting is removed by
emergency exit with the relevant controls.

T028 is now ready for the examples/model handoff; no vectors or implementation are claimed by recording
its resolved product rule. T021/T032/T034/T035/T037/T038/T039/T040/T060 carry the corresponding next work.
Existing Windows service reconciliation and J1 Linux hold remain in force; no old platform/control feature
is reactivated by account synchronization or this documentation change.

## Active Work

### Closeout package state — 2026-11-19 (after the session pause and automatic continuation)

Main-agent verification on Windows 11 (SDK 8.0.419), run after the last edit: `dotnet test FocusManager.Shared.slnf` — Core 261, SQLite 36, 0 failed; `dotnet test FocusManager.Windows.slnf` — Core 261, SQLite 36, Agent 29, Platform.Windows 85, 0 failed. Not verified: Linux (owner hold J1, deferred), any real agent/UI launch, the WinUI page, a UNC server, non-root per-drive current directory. Nothing is committed or pushed.

- [T054](tasks/T054-fix-windows-path-compatibility-after-review.md): `review`. The four reproduced differences, 8.3 names, device/verbatim roots, UNC root slots and the production composition are fixed with test-first evidence and 17 vectors in `test-vectors/windows-path-compat.json`. Two independent reviews (both accept-with-fixes) found no blocker; every finding was applied except documented, deliberately kept divergences (see "Known remaining divergences from HEAD" in its Result: three leading separators, legacy NUL device name, degenerate `\?`/`\??`, blank allowed folder no longer allowing every backslash-prefixed candidate, non-idempotent Normalize). The UI persistence step is mirrored in a test, not executed through WinUI. T001 stays `review` and is not accepted by this.
- [T055](tasks/T055-verify-windows-sqlite-startup-and-restarts.md): `blocked`. No safe isolated Windows environment (Windows 11 Home, no Hyper-V/Sandbox). Owner action: provide a disposable Windows VM or a spare test PC, then run `docs/reviews/evidence/T055/runbook.md`. T002 AC1 stays partly verified.
- [T056](tasks/T056-clarify-proposed-service-design-after-review.md): `review`. ADR-0009 stays **Proposed**; Q1–Q16 untouched, Q17/Q18 added. The independent review (accept-with-fixes, 3 major) was applied to D6/D10, T050, T051, T052, T016 and T027. These fixes were **not independently re-reviewed**, and nothing in D6/D10 has run (assumptions A12, A21–A23, A27, A29, A30 untested). T058 must reconcile the ADR with owner decisions K1/L (Q1 premise, D6 refusal/reboot consequence vs K1 Q5, D7 HKLM remarks, D8 snapshot and service stop wording).
- Owner decision raised for T058: whether the UI should persist full paths (stored `D:`, `\Study` and relative names depend on the process's current drive/directory and would resolve differently in a session-0 service).
- Resume order after acceptance or explicit continuation authority: T058, then the T047–T052 children only after ADR-0009 acceptance; T007/T012/T022/T006 as before. The Linux hold J1 and K/L decisions remain in force.

- [T061](tasks/T061-record-profile-offline-billing-and-default-decisions.md) is `review`: both owner clarifications are integrated, the separate protected-system group and visible FocusManager waiting-screen condition are recorded, and documentation checks passed. The former block is resolved; implementation/ADR acceptance and the Linux hold remain separate.
- [T057](tasks/T057-record-owner-service-android-and-cloud-decisions.md) is `review`: the owner's Windows/Android/cloud answers and Opera clarification are recorded in K1–K4 and owning specifications (subsequently clarified by L1–L6); documentation checks and independent review passed. T054/T055/T056 are concurrently owned elsewhere; this task did not overwrite their source, task results or proposed ADR.
- Phase: close out the independent review of uncommitted work before advancing feature development. M1 development started at the owner's request on 2026-11-17 while M0 awaits acceptance; no milestone is accepted.
- [T053](tasks/T053-record-review-and-conserve-linux-work.md) is `review`: the report, retained evidence, executable follow-up tasks and the owner's Linux hold are recorded. Documentation checks and independent verification passed; no implementation fix or acceptance is performed by T053.
- Start the next handoff at [the review and closeout instructions](docs/reviews/2026-11-18-uncommitted.md).
- Closeout tasks (T054 and T056 are now `review`, T055 is `blocked`; see the closeout package state above):
  - [T054 — Windows path compatibility](tasks/T054-fix-windows-path-compatibility-after-review.md): fix the P1 normalization and P2 colon/filename regressions, cover UI/persisted inputs, and add minimal shared regression examples (done, in review).
  - [T055 — Windows SQLite startup/restarts](tasks/T055-verify-windows-sqlite-startup-and-restarts.md): verify the real application/agent flow using synthetic current/legacy data in an isolated Windows environment. A checkout or working-directory change alone does not isolate LocalApplicationData, named pipes, registry policies or the launcher's process-by-name cleanup.
  - [T056 — Proposed service design clarifications](tasks/T056-clarify-proposed-service-design-after-review.md): clarify pre-created data-tree safety and the console/service tray boundary. Keep ADR-0009 Proposed; K1 now records the owner answers and T058 owns design reconciliation after T056.
- T054/T055/T056 rework or verify unaccepted submissions. They do not consume accepted outputs or authorize T007/T012/T047–T052 or other feature implementation. Completion enters `review`; owner acceptance remains separate.

## Owner Hold — Linux

The owner explicitly stopped Linux development on 2026-11-18 until the owner personally decides to resume.
See [decision J1](docs/spec/decisions.md#j1-linux-development-hold).

Preserve existing code, research, tasks and architecture. Do not continue Linux research, implementation,
prototypes, setup/provisioning, Linux-host tests, Linux CI or packaging, including Linux-only parts of mixed
tasks. T019 changed from `ready` to `planned`; T020/T042/T043 remain `planned` with an explicit hold.
Dependency completion or a generic instruction to continue the backlog does not lift it.

Windows and shared-code work verified on Windows may continue under normal task gates; retain existing
pure Unix-style data cases without adding Linux-platform work or claiming Linux runtime support.
Linux criteria in T001/T006/T022 and other tasks are deferred and unverified, not waived or passed.
The current closeout does not require a Linux environment. Any later acceptance that changes an original
criterion needs the owner's explicit scope decision; the hold alone does not establish that acceptance.

## Submitted Work and Acceptance

- [T001](tasks/T001-make-core-path-rules-os-independent.md): `review`, unaccepted. T054 repaired the previously reproduced practical regressions; the later F1 compatibility finding is being addressed by T063. Other documented edge divergences remain; T001's original Linux criterion is deferred under the hold.
- [T002](tasks/T002-restore-sqlite-storage.md): `review`; no confirmed introduced storage defect in the independent review. Store tests pass, but application startup/restart AC1 remains partly verified and is assigned to T055.
- [T045](tasks/T045-make-app-enforcer-kill-test-path-independent.md): `review`; PowerShell and Git Bash Windows suites passed, and a fresh isolated mutation check confirmed the termination test detects a missing notification. Owner acceptance is pending.
- [T027](tasks/T027-design-windows-service-session-boundary.md): `review`; ADR-0009 is Proposed; K1 supplies the owner answers, while T058 must reconcile recovery, independent crash cleanup and session scope after T056. Newer/residual questions still need evidence or owner answers. T056 can clarify technical details without accepting the proposal. Children T047–T052 stay `planned` and may not start from this handoff.
- [T025](tasks/T025-install-ai-development-infrastructure.md), [T026](tasks/T026-review-roadmap-and-orchestration.md), [T046](tasks/T046-update-shared-ai-agent-infrastructure.md): remain `review`, awaiting owner acceptance. This review does not establish every historical criterion of T025/T026 anew.
- [T011 Opera](tasks/T011-research-opera-site-filtering.md) and [T008 Android](tasks/T008-research-android-enforcement-and-ui.md): desk research recorded, still `ready`; prototypes and owner decisions are needed. They are outside this closeout package. Linux desk research T019 is conserved on hold.

## Product Baseline

Only the Windows implementation exists. The uncommitted agent stores settings and allow-lists in SQLite at
`%LOCALAPPDATA%\FocusManager\focusmanager.db`, the historical location/schema; PostgreSQL is removed.
`RuleEvaluator` takes explicit path semantics (`IPathRules`, Windows registration in the agent).
T054 repaired the previously reproduced practical compatibility regressions; the later UNC edge case F1 is
under T063 correction. Other recorded edge divergences and T001 acceptance remain open.
The UI remains WinUI 3. Windows service and Tauri production implementations are planned; Android is planned;
Linux work is conserved. Existing Windows/Linux sharing boundaries remain in effect.

All implementation, infrastructure and planning work remains uncommitted. No commit, push, service installation
or external change has been made by the review/documentation work. Generated Python caches are ignored.

## Review Evidence and Verification

The independent review compared the final working tree to `3035e03de6b3c25545bd6b252bfd1f5ab622aed9`:
48 changed/deleted tracked paths, 23 non-ignored untracked files, no staged changes. This describes the reviewed
input before T053 adds its report/tasks/hold documentation. The report preserves the original verdict and
distinguishes subsequent owner instructions. Retained evidence is linked there.

Executed in isolated Windows copies before T053:

- `dotnet test FocusManager.Windows.slnf` from PowerShell and Git Bash: Core 88, SQLite 35, Platform.Windows 6 and Agent 26 passed on each run (155 total, no failures).
- Old/new evaluator differential probes on .NET 8: 4 differences in 5 cases, including UI-compatible drive roots and colon filenames. These expose missing regression coverage despite the passing submitted suite.
- Mutation of the notifier call in an isolated copy: T045's target test failed as expected; bytes were restored and the full Windows suite passed.
- Visual Studio MSBuild full Windows build passed, including WinUI. `dotnet build` failed with MSB4062/ExpandPriContent on both the reviewed tree and an isolated HEAD copy; it is a baseline tooling limitation, not a new regression.
- Static task/link/dependency/preset review passed; source and index preservation were checked by hashes at the end of the read-only review.

Not verified: real application startup/persistence across agent restarts (T055); service session-0, reboot,
access isolation and assumptions A1–A20; Android/Opera prototypes; CI (T006 not started); live Firebase/store
requirements. Linux runtime verification is now explicitly deferred by the owner, not an execution target.
The earlier submitted test runs in individual tasks remain historical evidence; the report owns this fresh
independent execution record. Documentation checks for T053 are recorded in its Result when completed.

## Prepared Foundation

ROADMAP has 13 milestones M0–M12 and 63 indexed task records T001–T063. T047–T052 are the proposed service
implementation children; T053 records the review handoff and T054–T056 define its bounded follow-up work; T057 records owner answers and T058–T060 own their design/research follow-ups. T062 retains the rereview; T063 is addressing its two corrections.
[Requirement coverage](docs/engineering/REQUIREMENT_COVERAGE.md) maps all 108 requirement definitions/retained removed identifiers to owners/gates;
coverage is not implementation. Earlier implementation/research used delegated tier-2 workers and independent
review as recorded in its tasks. The independent review and T053 documentation use tier-2 GPT-6.1 Sol/medium
subagents with main-agent integration; requested settings are not a claim about other clients' runtime loading.

## Next Agent — Current Package Exit

1. Preserve all unfinished work and use the latest T063 result and dated review follow-up for the two rereview findings. T054 and T056 are submitted in `review`; do not restart them wholesale or infer owner acceptance.
2. T055 remains `blocked`: its real application startup/restart checks require a disposable isolated Windows VM or spare test PC and the existing runbook. Do not substitute store tests or touch the owner's live database, registry, pipes or processes.
3. ADR-0009 remains Proposed; T058 reconciliation is a separate gated future task. Do not install a service, change machine ACLs, accept the ADR, or infer acceptance from product answers.
4. Keep Linux work on hold and its original criteria deferred, not passed or waived. Stop before the feature backlog; submitted results and milestone acceptance remain separate owner decisions.

After acceptance or explicit scoped continuation authority, later work includes T007 (.NET 10), T012,
T022 and T006; the Linux hold still applies. Other ready planning gates exist (T005/T035/T039/T024/T041),
but they are not part of this package. T005 and the service-contract work share `contracts/`; keep them sequential.

## Owner Decisions and Future Gates

The owner answered ADR-0009 product questions in K1, including all-level service and independent crash
cleanup in follow-up answers. T058 owns design reconciliation; newer/residual questions and acceptance of
submitted results remain separate gates. The owner has not accepted tasks merely
by requesting this report or follow-up preparation. Technical findings can be repaired without inventing
new product choices. Opera and Android decisions remain in T011/T008; their research needs prototypes.
Linux decisions, environment selection and prototypes are frozen until explicit resumption.

Later gates remain: the approved PRF-005 examples/model handoff (T028), Android implementation/prototypes and exception mapping
(T031/T032), protection timer/feasibility design (T034), content/privacy/storage details (T035/T039),
account/conflict-contract details (T037), and selected-provider feasibility (T024). Commercial choices in
T040 stay on the explicit owner hold rather than being chosen automatically. The earlier no-cloud-first-version restriction is superseded by K3; cloud may be used when needed.
Detailed release/feature/account semantics remain with T037, and T060 estimates costs before paid activation.

Duplicate case-variant list entries still throw and roll back (legacy behavior). The agent returns the error
and UI handlers display it; the earlier “no user-facing message” claim is corrected. ADR-0009's proposed
T013 → T048 order remains a proposal, not an accepted product decision.

## Infrastructure Maintenance — 2026-11-18

[T046](tasks/T046-update-shared-ai-agent-infrastructure.md) remains `review`. Shared routing/process updates,
fixed presets, evaluation guidance, optional Codex executor defaults and provenance were applied from source
revision `5ab76bf2f5b726998b690b362a15c6e164f4ae85`; application/preservation evidence is in the
[receipt](docs/engineering/INFRASTRUCTURE_PROVENANCE.md). This does not establish T025/T026/T046 acceptance,
effective client loading/model execution, usage savings or product completion.
