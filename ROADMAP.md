# Roadmap

Task files own scope, dependencies, status and results. This roadmap owns milestone order, goals,
exit criteria and the task index. [STATUS.md](STATUS.md) owns the current handoff.
[Specification](docs/spec/README.md), [owner decisions](docs/spec/decisions.md) and
[architecture](docs/architecture/README.md) remain the owning product sources.
Follow [TASK_FORMAT.md](docs/engineering/TASK_FORMAT.md) and
[AI_ORCHESTRATION.md](docs/engineering/AI_ORCHESTRATION.md).

## How to Execute This Roadmap

Milestones are acceptance checkpoints, not claims of implemented functionality or release dates.
Their numeric order is the default development sequence; actual task dependencies govern what may start.
Windows/shared and Android work follow the task and orchestration gates. Linux development, research,
setup and Linux-host validation are on owner hold from 2026-11-18 until explicit owner resumption;
preserve existing materials and defer Linux-only portions of mixed tasks. See
[decision J1](docs/spec/decisions.md#j1-linux-development-hold). Infrastructure adoption T025 and
roadmap preparation T026 await acceptance.

T001–T025 retain their legacy numeric identities. T026 audits planning; T027–T044 split oversized work
and add missing decision/research/decomposition gates. T047–T052 were added on 2026-11-18 by T027 as proposed children of T016 (blocked on acceptance of ADR-0009). Milestones were refined at the owner's request
on 2026-11-17; task IDs and requirement meanings are unchanged.

Near-term M1/M2 tasks describe implementation. Later planning tasks explicitly create bounded child
implementation tasks from approved decisions/research. Add those children to the appropriate milestone
and coverage map before coding. Completing a planning/research gate never completes the feature or
milestone: all resulting implementation children and the observable exit criteria must be accepted.
Do not invent platform mechanisms to make the distant backlog appear ready.

See [requirement coverage](docs/engineering/REQUIREMENT_COVERAGE.md) for every functional area's
implementation or preparation owner. K3 now permits cloud from the first version when needed, subject to
zero/minimal expense priority; T037 retains detailed account/synchronization/release semantics and T060
estimates costs before any paid activation. The previous mandatory no-cloud boundary is superseded.

Current review closeout: read the [2026-11-19 rereview](docs/reviews/2026-11-19-uncommitted.md), retained by
T062, before the historical [2026-11-18 report](docs/reviews/2026-11-18-uncommitted.md). T054 and T056 are
submitted in `review`; T055 (isolated Windows SQLite startup/restart verification) is `blocked`.
T063 is in progress in the owner-directed follow-up to correct the two P2 findings. This is rework/verification
of unaccepted work, not permission to advance the feature
backlog or accept ADR-0009. Completing the package does not complete M1: T007/T012/T022/T006 and owner
acceptance remain separate gates. Linux criteria remain deferred and unverified; the hold does not
silently waive them or change milestone exit criteria.

## M0 — Development foundation

Goal: Agents can select, implement, verify and hand off bounded tasks using one roadmap and orchestration policy.

Exit criteria:

1. T025 infrastructure and T026 planning/orchestration are reviewed and accepted.
2. Task indexes, dependency graph, source coverage and next-session handoff agree.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T025](tasks/T025-install-ai-development-infrastructure.md) | Adopt the shared AI development infrastructure | — | review |
| [T026](tasks/T026-review-roadmap-and-orchestration.md) | Prepare the roadmap and agent orchestration for development | — | review |
| [T046](tasks/T046-update-shared-ai-agent-infrastructure.md) | Update the shared AI agent infrastructure | — | review |
| [T053](tasks/T053-record-review-and-conserve-linux-work.md) | Record the review handoff and conserve Linux work | — | review |
| [T057](tasks/T057-record-owner-service-android-and-cloud-decisions.md) | Record owner service Android and cloud decisions | — | review |
| [T061](tasks/T061-record-profile-offline-billing-and-default-decisions.md) | Record profile offline billing and default decisions | — | review |
| [T062](tasks/T062-record-uncommitted-rereview-and-handoff.md) | Record the uncommitted rereview and correction handoff | — | review |

## M1 — PC storage rules and repeatable checks

Goal: The existing Windows application has local storage, explicit path/rule semantics, a migrated SDK and repeatable checks.

Exit criteria:

1. SQLite persistence, old-data compatibility and offline startup meet T002 criteria.
2. The SDK migration and Windows launch are verified; shared path/rule tests and vectors pass on the applicable hosts.
3. Shared and Windows CI jobs are green after an authorized push; unavailable CI evidence prevents milestone acceptance.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T002](tasks/T002-restore-sqlite-storage.md) | Restore local SQLite storage without Windows defaults | — | review |
| [T054](tasks/T054-fix-windows-path-compatibility-after-review.md) | Fix Windows path compatibility after review | — | review |
| [T063](tasks/T063-resolve-uncommitted-rereview-findings.md) | Resolve the uncommitted rereview findings | — | in_progress |
| [T055](tasks/T055-verify-windows-sqlite-startup-and-restarts.md) | Verify Windows SQLite startup and restarts | — | blocked |
| [T007](tasks/T007-migrate-to-dotnet-10.md) | Migrate to .NET 10 LTS | T002 | planned |
| [T001](tasks/T001-make-core-path-rules-os-independent.md) | Make Core path rules independent of the host OS | — | review |
| [T012](tasks/T012-align-core-rules-with-spec.md) | Align Core rules with the new specification | T001 | planned |
| [T022](tasks/T022-add-shared-test-vectors.md) | Create shared rule test vectors for all platforms | T012 | planned |
| [T006](tasks/T006-add-ci-workflow.md) | Add GitHub Actions CI | T001, T002, T007 | planned |
| [T045](tasks/T045-make-app-enforcer-kill-test-path-independent.md) | Make the AppEnforcer termination test independent of PATH | — | review |
| [T004](tasks/T004-extract-whitelist-editing-logic-to-shared.md) | Extract whitelist input validation from WinUI into Shared | T001 | cancelled |
| [T010](tasks/T010-migrate-data-from-sqlite.md) | Migrate legacy SQLite data to PostgreSQL | — | cancelled |

## M2 — Desktop UI and background agent

Goal: The Tauri UI controls the Windows background service, with migrated screens, browser enforcement and configurable notifications.

Exit criteria:

1. Accepted service/user-session design is implemented; IPC identity, session-0 handling, logoff/reboot recovery and cleanup are verified.
2. Existing screens and tray behavior pass migration checks before WinUI removal; localization and Linux WebKitGTK shell checks are recorded.
3. Supported-browser behavior includes Opera through implementation tasks produced by T011; research alone does not satisfy support. Notification settings and repeat suppression are verified.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T005](tasks/T005-define-contracts-schema.md) | Define UI-agent JSON Schema contracts and generate types | — | ready |
| [T027](tasks/T027-design-windows-service-session-boundary.md) | Design the Windows service and user-session boundary | — | review |
| [T056](tasks/T056-clarify-proposed-service-design-after-review.md) | Clarify the proposed service design after review | — | in_progress |
| [T058](tasks/T058-reconcile-service-design-with-owner-decisions.md) | Reconcile the service design with confirmed owner decisions | T056 | planned |
| [T003](tasks/T003-extract-study-mode-orchestration-to-shared.md) | Extract session orchestration from the Windows agent into Shared | T001 | planned |
| [T017](tasks/T017-bootstrap-tauri-desktop-ui.md) | Bootstrap the Tauri 2 and React desktop UI | T005, T047 | planned |
| [T016](tasks/T016-run-agent-as-windows-service.md) | Run the Windows agent as a service without its own UI | T027, T003, T017, T047–T052 | planned |
| [T047](tasks/T047-add-contract-versioning-and-agent-info.md) | Add protocol versioning, GetAgentInfo and error codes to the UI-agent contract | T005, T027 | planned |
| [T048](tasks/T048-make-windows-enforcement-session-aware.md) | Make Windows enforcement session-aware with a policy ledger | T003, T013, T027 | planned |
| [T049](tasks/T049-add-windows-session-helper.md) | Add the Windows session helper for Explorer interaction | T048, T027 | planned |
| [T050](tasks/T050-move-windows-storage-to-service-owned-partitions.md) | Move Windows storage to service-owned per-user partitions | T002, T048, T027 | planned |
| [T051](tasks/T051-harden-agent-ipc-authorization.md) | Harden the agent IPC with token-based caller authorization | T047, T048, T050, T027 | planned |
| [T052](tasks/T052-host-windows-agent-as-service.md) | Host the Windows agent as a LocalSystem service with session supervision | T049, T050, T051, T027 | planned |
| [T018](tasks/T018-migrate-screens-and-remove-winui.md) | Migrate screens to Tauri and remove the WinUI application | T017, T016, T012, T022 | planned |
| [T013](tasks/T013-support-major-browsers-on-windows.md) | Support major Windows browsers and block unsupported browsers | T012 | planned |
| [T011](tasks/T011-research-opera-site-filtering.md) | Find a site-filtering mechanism for Opera | — | ready |
| [T014](tasks/T014-enable-block-notifications-with-setting.md) | Enable configurable blocking notifications | T016, T017 | planned |

## M3 — Android blocking foundation

Goal: Android has a buildable localized local-first application with verified core blocking and permission/lifecycle flows.

Exit criteria:

1. T008/T044 evidence supports the selected mechanisms and the T009 skeleton passes emulator/build checks.
2. T031 produces bounded production tasks for app blocking, both website methods, permissions, battery guidance and session recovery; those tasks are accepted before this milestone closes.
3. T030 advanced feasibility is recorded or split into explicit follow-up tasks assigned to the corresponding feature milestones; no required feature is silently dropped.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T008](tasks/T008-research-android-enforcement-and-ui.md) | Research Android blocking mechanisms | — | ready |
| [T044](tasks/T044-research-android-site-filtering.md) | Research Android website filtering methods | T008 | planned |
| [T009](tasks/T009-bootstrap-android-projects.md) | Bootstrap the Kotlin and Compose Android application | T008 | planned |
| [T031](tasks/T031-plan-android-enforcement.md) | Plan Android application and website enforcement | T008, T009, T044 | planned |
| [T030](tasks/T030-research-android-advanced-capabilities.md) | Research Android content triggers and protection | T008 | planned |

## M4 — Linux service and enforcement foundation

Execution state: **on owner hold** from 2026-11-18. T019/T020/T042/T043 are conserved in `planned`;
do not start them, their children, Linux-only mixed-task work or Linux-host checks until the owner
explicitly resumes Linux development. Existing requirements, sources and research are preserved.

Goal: The desktop UI controls a Linux service with verified core blocking on the owner-approved environment matrix.

Exit criteria:

1. Accepted Linux mechanisms and supported environments gate host and enforcement work.
2. Host/Unix-socket access, reconnect, storage and application/site enforcement meet T020/T042 criteria.
3. T043 produces bounded folder/notification/package tasks; folder parity tasks are implemented and accepted before milestone closure, with remaining notification/packaging work assigned to M8/M12.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T019](tasks/T019-research-linux-mechanisms.md) | Research Linux blocking mechanisms | — | planned |
| [T020](tasks/T020-bootstrap-linux-agent.md) | Bootstrap the Linux agent | T019, T002, T003, T017 | planned |
| [T042](tasks/T042-implement-linux-enforcement.md) | Implement Linux application and website enforcement | T019, T020, T012, T022 | planned |
| [T043](tasks/T043-research-linux-folders-notifications-packaging.md) | Research Linux folders notifications and packaging | T019 | planned |

## M5 — Profiles and rule modes

Goal: Users manage profiles and apply allowlist/blocklist rules consistently on the three primary platforms.

Exit criteria:

1. Owner-approved composition semantics, versioned schemas and lossless desktop data migration are verified.
2. T032 child tasks deliver profile editors, multi-profile runtime, essential-app behavior, list validation and platform integrations; all are accepted.
3. Shared examples and platform checks cover empty lists, simultaneous profiles, live edits, session cleanup and relevant folder behavior.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T028](tasks/T028-resolve-profile-composition.md) | Resolve simultaneous-profile rule composition | — | ready |
| [T021](tasks/T021-define-profile-data-model.md) | Define the profile data model and synchronization format | T005, T028 | planned |
| [T029](tasks/T029-migrate-whitelists-to-profiles.md) | Migrate desktop whitelist data to the profile model | T002, T021 | planned |
| [T032](tasks/T032-plan-profile-runtime-and-editors.md) | Plan profile runtime and platform editors | T021, T029 | planned |

## M6 — Launch methods and session timers

Goal: Profiles run manually, on schedules, by usage/launch limits and through supported location/network triggers.

Exit criteria:

1. T033 creates and links bounded shared/runtime and per-platform implementation tasks with resolved timing/reset semantics.
2. All child tasks are implemented and accepted: timer/manual, schedule, time/launch limits, Wi-Fi, Android geofence and trigger combination.
3. Offline/restart and clock-change checks demonstrate the approved session behavior; unverified cases remain acceptance blockers.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T033](tasks/T033-plan-triggers-and-timers.md) | Plan launch methods and resilient session timers | T021 | planned |

## M7 — Protection levels and emergency exit

Goal: The four specified protection levels and preconfigured emergency exit apply with truthful platform limits.

Exit criteria:

1. T034 decisions/research and child tasks cover friction, code, strict restrictions, weakening rules, emergency defaults and resistant timers.
2. Shared policy and all platform/UI child tasks are accepted; uninstall/settings capabilities have evidence and owner-accepted limitations.
3. Bypass warnings are verified; partner functionality remains deferred.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T034](tasks/T034-plan-protection-levels.md) | Plan protection levels and bypass handling | T021 | planned |
| [T059](tasks/T059-research-optional-windows-account-switch-restriction.md) | Research an optional Windows account switching restriction | T034 | planned |

## M8 — Content and notification controls

Goal: Profiles can restrict keywords/sections and suppress blocked-app notifications where validated on the main platforms.

Exit criteria:

1. T035 creates desktop feasibility and per-platform implementation tasks using T030/T043 evidence and owner answers.
2. Keyword/section filtering and selective notification suppression child tasks are accepted, including permissions and rule-update reliability.
3. FocusManager notifications remain distinct from suppressed application notifications; NTF repeat-suppression and settings behavior have platform checks.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T035](tasks/T035-plan-content-controls.md) | Plan content filtering and notification controls | — | ready |

## M9 — Statistics and progress

Goal: Users view local usage charts, session history, blocked attempts and progress on the primary platforms.

Exit criteria:

1. T036 produces and completes collection, local storage/aggregation and platform UI tasks for STAT-001–STAT-005.
2. Usage/history/progress checks pass without an account or internet; telemetry contains no user list contents.
3. Undefined achievement/exam-mode details have owner decisions and separate tasks before claiming those product directions complete; cloud merging is verified in M10.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T036](tasks/T036-plan-statistics-and-progress.md) | Plan local statistics and progress views | T021 | planned |

## M10 — Accounts synchronization and privacy

Goal: Optional accounts provide free basic synchronization and backup while local blocking remains fully functional offline.

Exit criteria:

1. The first-version/public-release boundary and account/conflict/merge semantics are explicitly approved.
2. Firebase access isolation and T038 child tasks for sign-in, profiles/settings, platform-specific lists, statistics and backup are accepted; live blocking sessions remain independent per device (L2; SYNC-004 removed).
3. T039 child tasks implement reviewed consent/withdrawal, crash reporting, technical analytics and data exclusions; cost assumptions and privacy-review blockers are recorded.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T037](tasks/T037-resolve-account-and-sync-contract.md) | Resolve account and synchronization contracts | T021 | planned |
| [T060](tasks/T060-estimate-cloud-costs-before-enabling-paid-services.md) | Estimate cloud costs before enabling paid services | — | ready |
| [T039](tasks/T039-plan-privacy-and-telemetry.md) | Plan consent privacy and technical telemetry | — | ready |
| [T023](tasks/T023-set-up-firebase-project.md) | Set up the Firebase project | T021, T037 | planned |
| [T038](tasks/T038-plan-sync-implementation.md) | Plan account and synchronization implementation | T023, T037 | planned |

## M11 — Trial subscriptions and cross-platform access

Goal: The approved seven-day auto-renewing trial and 1/3/6/12-month subscriptions grant the correct access across platforms.

Exit criteria:

1. Owner-approved payment architecture, free/paid scope, prices and trial eligibility are recorded; no lifetime purchase or advertising is introduced.
2. T040 child tasks implement selected purchase channels, entitlement/account binding, paywalls and recovery; renewal/refund/expiry/offline/account-switch checks pass.
3. Required legal/business decisions are resolved for a paid public release; research alone is insufficient.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T024](tasks/T024-research-subscriptions-and-payments.md) | Research cross-platform subscription payments | — | ready |
| [T040](tasks/T040-plan-paid-entitlements.md) | Plan trials billing and shared entitlements | T024, T037 | planned |

## M12 — Packaging distribution and public release

Goal: Verified packages, store submissions and a bilingual website/privacy policy support the first public paid release.

Exit criteria:

1. T041 produces and completes platform packaging/update/uninstall, distribution-channel and parity-acceptance tasks with owner-approved environments/regions.
2. Current official store/testing/signing requirements and Microsoft Store service compatibility are verified at release time; publication has explicit authorization.
3. M1–M11 feature/platform exit criteria and all generated release children are accepted; PAY-006, privacy/legal and rollback gates are met.
4. T015 website/downloads/contact/privacy content reflects actual shipped behavior and is verified after authorized publication.

| Task | Title | Depends on | Status |
| --- | --- | --- | --- |
| [T041](tasks/T041-plan-release-and-platform-parity.md) | Plan release packaging and platform acceptance | — | ready |
| [T015](tasks/T015-create-product-site.md) | Create a product website with a privacy policy | T039, T041 | planned |

## Deferred and Cancelled Work

- iOS follows finalization of the primary platforms; IOS-001–IOS-002 remain deferred. macOS is not planned.
- Accountability partner functionality remains deferred by PROT-011; do not implement it through protection or synchronization tasks.
- T004 remains cancelled because WinUI is replaced by T018. T010 remains cancelled because PostgreSQL is removed by ADR-0006. Their IDs and historical records are retained.
