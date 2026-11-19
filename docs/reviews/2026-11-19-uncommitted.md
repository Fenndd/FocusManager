# Uncommitted working-tree rereview — 2026-11-19

**Verdict: Changes required.** Two P2 findings remain: malformed UNC root normalization changes a folder authorization result, and the active status handoff repeats completed remediation. No P0/P1 issue or introduced storage defect was established. Passing automated checks do not remove these findings or establish real application/service lifecycle acceptance.

Recorded by [T062](../../tasks/T062-record-uncommitted-rereview-and-handoff.md) under the owner's instruction to preserve the completed review. Corrections are reserved for a later owner-assigned session in [T063](../../tasks/T063-resolve-uncommitted-rereview-findings.md); recording this report does not start them, accept other task results, or accept ADR-0009. The original [2026-11-18 report](2026-11-18-uncommitted.md) remains historical evidence. This report supersedes its verdict about the current reviewed snapshot, without implying that every earlier defect remains current.

## Review target and method

Baseline: `3035e03de6b3c25545bd6b252bfd1f5ab622aed9` (`HEAD`). The input before recording contained **76 tracked changed/deleted paths and 57 non-ignored untracked files**, with no staged changes. Review covered the uncommitted source/test/storage/path changes, planning/specification and owner-decision propagation, service-design gates, infrastructure, and retained evidence. Relevant owning sources and surrounding behavior were inspected; historical/proposed text was distinguished from active requirements.

Executions used an external Windows copy of that input, .NET SDK 8.0.419 on Windows, rather than launching the owner's real application. Independent bounded tier-2 GPT-6.1 Sol / medium reviews supplied source and documentation findings; the main agent verified/integrated the findings and execution evidence. Requested routing settings do not establish client configuration loading or other clients' effective models.

Finding line numbers below refer to the reviewed snapshot **before T062 recording insertions**. Source behavior was not changed by recording. The dated STATUS notice added by T062 is a navigation mitigation, not a rewrite of the stale sections or resolution of F2.

## F1 — [P2] Preserve the empty UNC server slot during normalization

**Location:** [PathRules.cs](../../src/Shared/FocusManager.Core/Rules/PathRules.cs), line 57 (`rest.Split(_separator, StringSplitOptions.RemoveEmptyEntries)`).

For a Windows path with three leading separators, removing empty entries collapses the empty server slot and makes a malformed root equivalent to an ordinary UNC root. The executable comparison used the HEAD evaluator and the current production composition (`RuleEvaluator(new WindowsHostPathRules())`) on Windows:

| Candidate folder | Stored allowed folder, `AllowSubfolders: true` | HEAD | Current |
| --- | --- | --- | --- |
| `\\\srv\share\x` | `\\srv\share` | deny | allow |
| `\\srv\share\x` | `\\\srv\share` | deny | allow |
| `\\srv\share\x` | `\\srv\share` | allow | allow |

**Trigger and impact:** a malformed candidate or malformed stored folder reaches the evaluator and the other path has the ordinary UNC spelling. The result becomes more permissive than HEAD, contrary to this refactor/compatibility repair's behavior-preservation scope. The case concerns malformed inputs; the review did **not** demonstrate a normal Explorer/real UNC-server bypass, so severity is P2 rather than a demonstrated broad enforcement bypass. T054's existing documented triple-separator divergence does not supply an owner-approved behavior change or a discriminating regression check for these authorization rows.

**Smallest correction:** preserve the UNC root's server/share slots before generic empty-segment removal, keeping the HEAD distinction for these two rows without disturbing the normal UNC control. Add discriminating cases for candidate and stored-path directions through the actual Windows composition and retain an appropriate shared compatibility vector. Re-run applicable shared/Windows checks in the later correction session. Do not expand this into the separate full-path/empty-list/site behavior work owned by T012.

**Evidence:** [executed comparison](evidence/2026-11-19/evaluator-probes.log), [probe source and staging instructions](evidence/2026-11-19/README.md). The probe asserts the observed before-fix behavior and exits if it differs; it is evidence, not a post-fix acceptance test.

## F2 — [P2] Refresh the operative handoff after remediation reaches review

**Location:** [STATUS.md](../../STATUS.md), reviewed lines 143 and 145 in “Next Agent — Current Package Exit”; also Product Baseline line 103. The earlier closeout summary at reviewed lines 53–58 already records T054/T056 as `review`.

**Trigger and impact:** the next agent follows the active instructions to “Complete T054” and “Prepare T056 and independent technical review,” even though both tasks have submitted results and matching `review` roadmap/task states. Product Baseline still says the submitted normalization has confirmed regressions and warns against claiming it fixed without distinguishing the original submission from T054's subsequent repairs. This can restart completed remediation/review and report historical findings as current, rather than preserve the pending acceptance gates, T055 environment blocker and T058 design reconciliation.

**Smallest correction:** update those operative steps and the baseline to distinguish repaired/submitted work from remaining F1, pending acceptance and unverified checks. Retain historical findings in the dated review sections. Do not mark T001/T054/T056 accepted or rewrite their acceptance criteria. T062's dated recording notice points readers here but intentionally leaves this correction for T063.

## Checks actually performed during the completed review

| Check | Result and scope |
| --- | --- |
| `dotnet test FocusManager.Shared.slnf --nologo` in PowerShell | **Verified:** Core 261 + SQLite 36 = 297 passed; zero failures/skips |
| `dotnet test FocusManager.Windows.slnf --nologo` in PowerShell | **Verified:** Core 261 + SQLite 36 + Agent 29 + Platform.Windows 85 = 411 passed; zero failures/skips |
| `bash.exe --noprofile --norc -c 'dotnet test FocusManager.Windows.slnf --nologo'` on Windows | **Verified:** 411 passed; zero failures/skips; Git Bash execution is not Linux validation |
| Visual Studio MSBuild 18.3, command below | **Verified:** successful full build including WinUI `FocusManager.App`; not an application launch |
| Exact HEAD/current executable evaluator probe | **Verified:** the two F1 differences and ordinary UNC control above |
| Static task/roadmap/dependency checks | **Verified:** all 61 pre-recording task statuses match roadmap rows, dependency targets exist, and the graph is acyclic |
| `git diff --check` | **Verified:** exit 0; no whitespace errors in the tracked diff |
| Preservation at the end of read-only review | **Verified:** 249 existing non-ignored input files unchanged, Git index unchanged and Git status unchanged; scope counts refer to the input before T062 |
| Owning decision/task/proposal/infrastructure consistency | No additional verified defect found in the bounded inspection; this is not runtime proof or blanket acceptance |

Retained [sanitized evidence and provenance](evidence/2026-11-19/README.md) include the test/build output, inert probe sources and compact [integrity summary](evidence/2026-11-19/final-integrity.json). They do not include a private absolute-path inventory. Static checks used read-only Python parsing of task front matter and roadmap statuses, verified dependency targets, and traversed the dependency graph with active/visited sets; they did not execute tasks or change their states.

The full-build command was run from the isolated snapshot root:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' FocusManager.Windows.slnf /t:Build /p:Configuration=Debug /v:minimal
```

## Acceptance state and explicit limits

| Material criterion | Status | Evidence or remaining gap |
| --- | --- | --- |
| T001 explicit path semantics, host-independent evaluator and explicit Windows registration (AC3-5) | Met | Core, platform adapter, registration and consumers inspected; composition tests pass; obsolete known-failures note removed |
| T001 Windows behavior preservation (AC2) | Partially met | T054 repairs the earlier practical cases; F1 and recorded edge differences prevent a claim of equivalence for every input |
| T001 Linux-host suite (AC1) | Not verified | Deferred by J1; Windows-host Unix-style fixtures are not Linux-host evidence |
| T054 targeted root/relative/trailing-dot/colon repairs, adapter and minimal vectors | Met in automated checks | Passing Core/platform/adapter/composition/vector tests; no real WinUI page round trip or physical D: drive was exercised |
| T002 legacy schema/migration, Shared defaults boundary, removed PostgreSQL references and autonomous persistence tests (AC2-4) | Met at storage/configuration level | Historical SQLite comparison, transactions inspected and 36 SQLite tests pass; real-agent legacy-file lifecycle remains T055 |
| T002 real startup/restart (AC1), T055 application scenarios | Partially met / not verified in the real application | Store-level restart coverage passes; real elevated agent/UI lifecycle requires the isolated Windows environment |
| T045 shell independence and retained notification assertion | Met | Windows suite passes in PowerShell and Git Bash; assertion retained and inspected. No new notifier mutation was executed by this rereview |
| K/L decision propagation and future implementation gates | Met in inspected documentation | No confirmed propagation or acceptance-gate defect; recording decisions is not implementing the features |
| T027/T056 proposal and service/session/storage safety acceptance | Partially met | Boundaries and future checks documented; ADR Proposed, T058 reconciliation and runtime assumptions remain open |
| Task/roadmap metadata and dependency consistency | Met | 61 pre-recording records match, dependency targets exist, graph acyclic; F2 concerns operative prose rather than metadata |
| Infrastructure consistency and effective client loading | Partially met | Static entrypoints/presets agree; actual loading across clients and quota/savings effects not verified |

- [T055](../../tasks/T055-verify-windows-sqlite-startup-and-restarts.md) remains **blocked**. Real Windows agent/UI startup, save and restart behavior is **not verified**; T002 application-lifecycle AC1 remains partly verified. It requires an isolated disposable Windows VM or test machine and the existing [runbook](evidence/T055/runbook.md). Passing SQLite unit tests and the WinUI build do not replace that check. The owner's live database/processes were not exercised.
- [T001](../../tasks/T001-make-core-path-rules-os-independent.md) remains unaccepted. Its Linux-host criterion is deferred under the explicit [J1 hold](../spec/decisions.md#j1-linux-development-hold), not passed or waived. Pure Unix-style fixture checks executed on Windows do not establish Linux platform support. No Linux research, provisioning or execution was resumed.
- [ADR-0009](../architecture/decisions/0009-windows-service-and-user-session-boundary.md) remains **Proposed**. [T056](../../tasks/T056-clarify-proposed-service-design-after-review.md) technical clarifications and their fixes are documentation, with no D6/D10 execution and no second independent review of the fixes established here. Service session-0 behavior, data-tree ACL strategy and other runtime assumptions remain unverified.
- [T058](../../tasks/T058-reconcile-service-design-with-owner-decisions.md) remains gated after T056 acceptance or explicit scoped continuation. K/L decisions supersede conflicting earlier proposed passages; retaining those explicitly deferred passages was not reported as a newly accepted contradiction. T047–T052 do not start from this report or from product answers alone; ADR acceptance remains a separate gate.
- Owner decisions retain independent local blocking sessions, no seeded profiles, removable ordinary defaults and a separate protected technical group, visible waiting-screen timing, held commercial choices, deferred general visible-text analysis and the Linux hold. Account synchronization is not remote session control.
- Infrastructure static consistency does not verify Codex/Claude loading, actual model execution, quotas or savings. Android/Opera prototypes, live Firebase/store/billing behavior and other task-specific runtime checks were not performed by this review.
- No commit, push, deployment, service installation or owner-resource change was performed by the completed review. The review found no introduced storage defect, which does not establish lifecycle acceptance or absence of all possible storage defects.

## Recording and next action

T062 records these conclusions and sanitized evidence only. Its documentation checks are separate from the earlier executions above; no .NET or Linux run was performed for recording. The main agent verified local links/anchors, task/roadmap/dependency agreement, whitespace and preservation outside the documentation boundary; the actual recording result is in [T062 Result](../../tasks/T062-record-uncommitted-rereview-and-handoff.md#result). T062 is `review`, not accepted.

Leave both findings open for the owner's later assignment of T063. Do not start repair, advance the feature backlog, infer acceptance of other submitted work, or lift the Linux hold from this handoff.
