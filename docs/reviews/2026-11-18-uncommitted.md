# Independent review of uncommitted changes — 2026-11-18

Verdict: **Changes required**. This verdict concerns the original reviewed implementation; subsequent owner scheduling instructions are recorded separately below.

The reviewed Windows path refactor has two verified compatibility defects. Passing automated suites do not establish T001's behavior-preservation criterion or T002's real agent startup/restart criterion. The handoff is [T054](../../tasks/T054-fix-windows-path-compatibility-after-review.md) for remediation, followed by [T055](../../tasks/T055-verify-windows-sqlite-startup-and-restarts.md) for isolated Windows lifecycle evidence. Neither task accepts T001/T002 or authorizes the remaining roadmap.

## Reviewed state and evidence provenance

- Baseline HEAD: `3035e03de6b3c25545bd6b252bfd1f5ab622aed9`; no staged changes at review time (index matched HEAD).
- Scope: 48 tracked modified/deleted files plus 23 nonignored new files from the working-tree inventory. This is the original review snapshot, before this documentation handoff. Scope includes Core paths, SQLite replacement, Windows wiring/tests, solution files, specifications, tasks, service design and agent-infrastructure documentation/presets. Ignored/generated files are excluded from the change target.
- The original reviewer verified that 215 hashed source files were unchanged by review; Git index SHA-256: `0E7A79E472FAC5D14D33A4A16D084ACDDE7E2DE5F325E60C501230CB67AC2BF3`. This digest identifies the earlier preservation check, rather than a fresh check of the expanded handoff tree.
- Tests/builds and diagnostic probes ran in temporary Windows copies, with a separate HEAD baseline build. No production startup was run and no implementation fix is included here.
- [Retained evidence](evidence/2026-11-18/README.md) contains compact log excerpts and both diagnostic source files. Probe files use `.cs.review` so they are documentation, not tests automatically compiled into the project. Full machine-path-heavy logs remain outside the repository; their retained excerpts are identified individually.

## Findings

### P1 — Windows folder normalization breaks previously allowed paths

Smallest viable repair: make Windows boundary normalization and the evaluator compatible with existing UI/persisted values, then add regression coverage of that round trip. Keep OS state out of shared Core; do not silently interpret every drive-relative path as a drive root without checking the required compatibility context.

Location in the reviewed source: `src/Shared/FocusManager.Core/Rules/PathRules.cs`, `SplitRoot` (line 123 drive-relative branch), together with `Normalize` (line 29). T001 promises unchanged Windows behavior.

The existing Windows UI's `src/Windows/FocusManager.App/Views/FoldersWhitelistPage.xaml.cs`, `NormalizeFolderPath` (line 205; trim at line 223), strips trailing separators. A selected `D:\` can therefore be stored as `D:`. The new evaluator preserves `D:` and `D:\` as distinct values; the HEAD evaluator on the same Windows/.NET 8 process allowed the root. This can deny a folder the owner explicitly selected. This is an actual UI-to-evaluator compatibility case, not merely a hypothetical spelling.

The probes also confirm changes for a current-drive-root-relative value and a trailing-dot value. These require explicit compatibility treatment; do not silently move T001's regressions into a later feature task.

| Folder candidate | Allowed folder | Allow subfolders | HEAD result | Reviewed result |
| --- | --- | --- | --- | --- |
| `D:\` | `D:` | false | allow | deny |
| `D:\Study` | `D:` | true | allow | allow |
| `C:\Study` | `\Study` (one leading backslash) | false | allow | deny |
| `C:\Study` | `C:\Study.` | false | allow | deny |

For the third case, the probe's Windows current drive was C: and `Path.GetFullPath` resolved `\Study` to `C:\Study`. This is not a UNC path. The results describe the actual .NET 8 run, rather than an assumption about historical .NET Framework path handling. Arbitrary relative-path resolution still needs an explicit strategy compatible with the pure shared-code boundary; the table alone does not establish every possible spelling.

### P2 — Colon suffix becomes an application filename match

Defensive fix: retain colons in the final path component; recognize a drive-prefix colon only at its valid prefix position. Add a discriminating deny regression instead of changing the fallback policy assigned to T012.

Location: `src/Shared/FocusManager.Core/Rules/PathRules.cs`, `GetFileName` (line 79). It treats the last colon anywhere in the path as a filename boundary. With allowed app `C:\Apps\Code.exe`, candidate `C:\Apps\evil.exe:Code.exe` was denied by HEAD but allowed by the reviewed evaluator. In this same .NET 8 run, `Path.GetFileName(candidate)` returned `evil.exe:Code.exe`.

This broadens the retained filename fallback and violates the refactor's compatibility objective. It is a demonstrated evaluator difference; a runnable alternate-data-stream process bypass has **not** been demonstrated. Fixing this must not implement T012's separate approved full-path-only application semantics or its other behavior changes.

## Checks actually performed during the original review

Windows environment: .NET SDK 8.0.419, .NET runtime 8.0.25; Visual Studio MSBuild 18.3.0. The documentation handoff inspected and retained earlier evidence; it did not rerun .NET commands.

| Check | Recorded outcome | Meaning and limits |
| --- | --- | --- |
| `dotnet test FocusManager.Windows.slnf` in PowerShell | verified: 155 passed, 0 failed | Core 88, SQLite 35, Platform.Windows 6, Agent 26; no live startup/restart proof |
| Same Windows suite in Git Bash | verified: 155 passed, 0 failed | Windows execution, not Linux execution |
| Visual Studio MSBuild full Windows build | verified: succeeds | Build evidence only; retained excerpt identifies executable/tool version |
| `dotnet build FocusManager.Windows.slnf` | failed: MSB4062, missing `ExpandPriContent` task assembly | Separate HEAD baseline build fails with the same error; no evidence this change introduced it; do not relabel the failed command as passed |
| HEAD evaluator versus new evaluator diagnostic probes | verified differences: 4 of 5 cases; comparison run reports 4 failed equality assertions | Three folder differences and one colon-filename difference; child-folder control is unchanged |
| Denied-process notification-test mutation diagnostic | mutation detected: the targeted test failed after one notifier invocation was replaced by `await Task.CompletedTask` in the disposable copy | Supports notification assertion sensitivity; bytes were restored afterward. This intentionally failing diagnostic is not a source-tree regression or a full mutation campaign |
| Source preservation check | verified in original review: 215 hashes unchanged | Digest above; not acceptance of all changes |

## Acceptance gaps and scope boundaries

- **T001 AC2 is unmet** because the verified Windows compatibility differences remain. T001 stays in `review` pending remediation and owner acceptance.
- **T001 AC1 was not verified on Linux.** The subsequent owner instruction halts all Linux development until explicit owner resumption, including provisioning or running Linux merely to verify Shared code. This check is now deferred by the owner hold, not passed and not a request to provision a Linux environment as a current-stage prerequisite. Shared tests may run on Windows. Preserve Linux code/research; the hold does not cancel the platform.
- **T002 AC1 remains partly unverified.** Store tests cover persistence across store instances and legacy schema migration, but no real agent/UI startup, restart, and legacy-database lifecycle has been demonstrated. T055 records that evidence safely using synthetic data in an isolated Windows user/session or VM.
- The agent hardcodes `Environment.SpecialFolder.LocalApplicationData` in `Program.cs`. The launcher elevates and stops agents by process name; registry and named-pipe effects also exist. Copying the repository, changing the working directory, or setting a `LOCALAPPDATA` environment variable alone does not isolate a production launch. Do not touch the owner's real database, processes, registry state, or IPC endpoint to complete T055.
- Matching changes require minimal JSON examples under `test-vectors/` (ADR-0005). The reviewed changes do not yet provide those examples. T054 must add Windows compatibility examples alongside regression tests; building T022's complete loader or resuming Linux is outside the package.
- T027's service/session proposal and Q1–Q16 remain proposed/open; no approval or service/IPC implementation follows from this review. T056 records the separate reparse/tray clarification follow-up. Existing accepted requirements remain owned by `docs/spec/` and accepted ADRs.

## Next-agent execution and authority

### Current package and original criteria

| Criteria | Recorded status | Follow-up |
| --- | --- | --- |
| T001 AC1, Linux execution | Not verified at review; now deferred by explicit owner hold | No Linux setup/run in this package; no waiver or acceptance inferred |
| T001 AC2, preserve Windows behavior | Not met | T054 fixes reproduced differences |
| T001 AC3–5, host-independent evaluator/explicit registration/removed obsolete note | Met by static inspection | Preserve these boundaries while repairing |
| Shared matching examples required by AGENTS/ADR-0005 | Not met | T054 supplies minimal regression JSON; T022 remains the full suite/loader |
| T002 AC1, real startup/restart | Partially met | T055 supplies isolated Windows application evidence |
| T002 AC2–4, legacy schema/defaults/references/autonomous tests | Met in reviewed source and synthetic fixtures on Windows | Preserve and rerun applicable checks after changes |
| T045 AC1–3, shell independence/assertion sensitivity/test-only scope | Met by Windows runs, mutation and scoped diff inspection | Owner acceptance pending |
| T027 AC1–2, proposal/boundaries/contract ordering | Met as a proposal | T056 clarifies technical risks |
| T027 AC3, explicit ADR acceptance | Not met | Owner Q1–Q16 and explicit ADR acceptance; no service code in this package |
| T046 static consistency/provenance/bookkeeping | Consistent in reviewed files; prior application preservation relies on the recorded receipt | Client loading/effective execution not verified; no renewed proof of all T025/T026 historical criteria |
| T008/T011/T019 prototype-backed research | Incomplete | Android/Opera outside this package; Linux conserved on owner hold |

Unresolved design risks are not demonstrated implementation vulnerabilities: the proposal needs an explicit strategy for user-precreated storage descendants/reparse points (T050), and explicit service-mode suppression of the retained console tray (T052). T056 must clarify both without changing product choices. Service/session-0, reboot, cross-user isolation and assumptions A1–A20 have no runtime proof. Static review checked 52 original task IDs/roadmap states, an acyclic dependency graph, relative documentation links and preset/table consistency; research proposals were not silently promoted to accepted requirements. No confirmed introduced storage defect was found.

### Execution sequence

1. Read AGENTS, STATUS, the current task files, and this report; inspect the current dirty tree. The source may have changed since the snapshot.
2. Start T054 by the task lifecycle. Restore Windows compatibility, add focused tests/minimal JSON examples, and run the Windows-host checks. Keep T012 behavior work separate.
3. Perform T055 with a verified isolated Windows environment. If isolation is unavailable, record the concrete blocker instead of launching against the owner's session. It can investigate current startup independently of T054; repeat affected evidence if T054 changes the evaluated paths used in its scenario.
4. Prepare [T056](../../tasks/T056-clarify-proposed-service-design-after-review.md), including independent technical review of its proposed safety strategy. Keep ADR-0009 Proposed and leave owner Q1–Q16 unanswered.
5. Record each completed result as `review`; present the bounded package for owner acceptance under the normal authority. T001/T002 remain pending acceptance and original deferred criteria remain explicit. Linux development remains on hold until an explicit owner instruction resumes it. T007, T012 and future service work are not authorized by this handoff package.

The remediation and verification tasks have `depends_on: []`: they address defects and evidence gaps in unaccepted existing work, rather than consuming an accepted new feature. This does not imply acceptance of T001/T002. Follow roadmap order for the default handoff and serialize overlapping test/evidence work.
