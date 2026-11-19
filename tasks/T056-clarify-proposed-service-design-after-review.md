---
id: T056
title: Clarify the proposed service design after review
milestone: M2
status: in_progress
depends_on: []
area: ["docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md", "tasks/T027-*", "tasks/T050-*", "tasks/T052-*", "tasks/T056-*"]
---

# T056 — Clarify the proposed service design after review

## Goal

Make the technical safety and tray-staging details in the existing proposed Windows service design reviewable before asking the owner to accept it. Leave product choices and implementation to their owning decisions/tasks.

## Sources

- [Independent review and closeout handoff](../docs/reviews/2026-11-18-uncommitted.md).
- [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md), D6, H3 and Q1–Q16.
- [T027](T027-design-windows-service-session-boundary.md), [T050](T050-move-windows-storage-to-service-owned-partitions.md), [T052](T052-host-windows-agent-as-service.md).
- Owner request on 2026-11-18 to record clear follow-up work for another agent.

## Scope

In scope:
1. Clarify safe creation/opening of the service-owned data tree. Resetting only the root directory DACL is not sufficient evidence that pre-existing descendants, junctions, reparse points or database files are safe for LocalSystem. Define a bounded strategy and corresponding adversarial checks, using primary Windows documentation where required; do not claim a hypothetical implementation vulnerability is already present.
2. State explicitly how T052 prevents the existing WinForms tray/UI loop from starting in service session 0 while retaining any required console-mode code until T016. Keep tray migration and single-icon acceptance in T016.
3. Align T050/T052 scopes and criteria with those technical clarifications, and remove stale lifecycle wording in T027 if any remains.
4. Keep ADR-0009 Proposed. List any new material product choice for the owner instead of selecting it silently.

Out of scope: implementing or installing a service/helper, changing owner answers Q1–Q16, adopting a new storage schema, changing permissions on this machine, Linux work, or treating the proposal as accepted.

This is rework of an unaccepted proposal, not implementation depending on accepted T027/T050/T052 outputs; it has no acceptance dependency on those tasks. It grants no continuation authority to their implementation tasks.

## Acceptance Criteria

1. ADR D6 and T050 define safe handling of a user-precreated root and descendants, and tests that can distinguish that strategy from merely resetting the root ACL.
2. ADR/T052 explicitly state the console/service tray boundary and checks; T016 still owns the final tray handoff.
3. ADR status and all owner questions remain accurate; no runtime, security-isolation or owner-acceptance success is claimed from documentation alone.
4. Changed links, task dependencies, source references and the diff pass documentation review; all three owning task records link to the clarification result where applicable.

## Checks

- Trace existing `AgentHostedService.StartAsync` and `TrayHost` startup against the proposed service path.
- Review filesystem/ACL/reparse claims using the primary Windows sources needed for the chosen strategy and record the sources and limitations.
- Inspect the resulting ADR/task diff, relative links and dependency/status agreement. No .NET execution or access changes are required by this documentation-only task.
- Obtain independent read-only review of the proposed safety strategy; leave untested assumptions explicit.

## Open Questions

None blocked the preparation of these technical clarifications. The task added two questions for the owner (Q17 was extended by the review fixes with the install-time creation and the reboot consequence next to K1 Q5, for T058), ADR-0009 Q17 (what happens after the service refuses an unsafe data-tree object) and Q18 (whether Administrators keep access to the data tree). ADR Q1–Q16 are neither edited nor answered by this task; the owner's answers of 2026-11-18 ([decisions K1](../docs/spec/decisions.md)) are reconciled with the proposal by [T058](T058-reconcile-service-design-with-owner-decisions.md).

## Result

2026-11-19 follow-up: reopened for two bounded inconsistencies found in independent re-review of the applied technical clarifications. This does not start T058 or accept ADR-0009.

Status: documentation drafted and the independent review's fixes applied; the task is still `in_progress` for the main agent to move to `review`. Nothing is accepted: ADR-0009 stays Proposed, Q1–Q16 are untouched, no code was written or run, no .NET command was executed, and no machine setting, owner data, process, registry key, pipe or service was touched. This task continued an attempt that a rate limit interrupted; its partial ADR edits were kept and corrected.

What was done (all documentation):
- [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) D6, [safe creation and opening of the data tree](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#safe-creation-and-opening-of-the-data-tree-t056-proposal): rules R1–R8 (create with an explicit protected DACL through `CreateDirectoryW` and never adopt a pre-existing object by resetting its DACL; validate every pre-existing object by handle without following reparse points, with final path, owner, exact DACL, link count and allowed entries, and refuse instead of repairing; parents before children; partition names only from the canonical authenticated SID; held directory handles; a post-open identity recheck as detection; no backup or restore privileges; optional SQLite defenses), an ancestors paragraph, the adversarial checks DT1–DT14 (V13) with a "reset the root ACL only" double and a control-by-control sensitivity check, and a [source status table](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#source-status-for-d6).
- ADR-0009 [D10](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#d10-tray-ownership-in-console-and-service-mode-t056-proposal): the trace of how the console agent starts its tray, the host-mode proposal and its checks V14, V15, M15 and M16. Assumptions A21–A29 and questions Q17–Q18 were added; the references list was completed.
- [T050](T050-move-windows-storage-to-service-owned-partitions.md) and [T052](T052-host-windows-agent-as-service.md): scope, acceptance criteria, checks and Result pointers aligned with D6 and D10. T050 no longer says that a user-created directory is "corrected" by resetting its DACL (the earlier ADR wording, withdrawn). [T027](T027-design-windows-service-session-boundary.md): counts of decisions, questions, assumptions and checks refreshed, and a pointer to this result added.

Trace finding for criterion 2. The agent registers `TrayHost` unconditionally (`Program.cs:53`) and `AgentHostedService.StartAsync` calls `_trayHost.Start` unconditionally (`AgentHostedService.cs:76`). `TrayHost.Start` creates its own STA thread that builds the `NotifyIcon` and runs `Application.Run` (`TrayHost.cs:34-41`, `:148-154`, `:166`), so the WinForms message loop is not on the host's main thread and any host that calls `StartAsync` starts it. T052 therefore needs an explicit host mode and a registration that never constructs `TrayHost` in service mode (D10). Console mode keeps its registrations and order of calls; T016 still owns the tray removal, the Tauri handoff and the single-icon acceptance.

Corrections made to the interrupted draft: the OWNER RIGHTS claim cited a Win32 page that does not list `S-1-3-4`; it now cites the Windows Server page and is marked as an inference for the default. The ACE rule distinguishes the protected directories and main database file from the inherited-only ACEs of SQLite sidecars. A hard link test must use a stand-in on the same volume. The WAL page does not describe `-shm` as persistent state.

Checks actually run:
- Source lines cited in D10 and H3 (`Program.cs`, `AgentHostedService.cs`, `TrayHost.cs`, `FocusManager.Agent.csproj:4`, `AppEnforcer.cs:12`) re-read in the working tree on 2026-11-19: verified. The other source lines of the ADR hazard table were not re-read in this pass; the earlier pass is recorded in T027.
- A throwaway script resolved every relative link and anchor in the ADR and in T027, T050, T052 and T056, and every reference-style link label in the ADR: verified (no missing target, anchor or label; the script's only unresolved output was front-matter arrays, which it mis-parsed as labels). The `#result` and `#d10-...` anchors exist.
- Front matter, `depends_on`, `area` and `status` of T027, T050, T052 and T056 were not changed by this task (T027 already differs from `HEAD` in front matter because of earlier work), and the ADR status line is still `Proposed`: verified by inspection of the diff. `ROADMAP.md`, `STATUS.md` and `REQUIREMENT_COVERAGE.md` were not edited.
- Primary sources read on 2026-11-19 through the documentation site: CreateDirectoryW, CreateFileW, GetFinalPathNameByHandle, Reparse Points, Hard Links and Junctions, BY_HANDLE_FILE_INFORMATION, Symbolic Link Effects, Security Identifiers (Windows Server), Null DACLs and Empty DACLs, SECURITY_DESCRIPTOR_CONTROL, FileSystemAclExtensions.Create, KNOWNFOLDERID, Interactive Services, IsWindowsService, AddWindowsService, Environment.UserInteractive, and the SQLite pages on corruption, WAL and security (summarized fetches). Verified claims and the claims that stay "not verified" are listed row by row in the source status table of D6.
- Independent read-only review of the safety strategy (the task's own check): obtained on 2026-11-19, verdict accept-with-fixes (3 major, 10 minor, 3 nit findings and two spot-checks of the primary sources). Its fixes were applied by a later tier-2 pass, listed below. The reviewer ran nothing, so the strategy itself is still untested; the fixes themselves have not had a second independent review.
- Every strategy rule and every DT, V13, V14 and V15 check, and M15 and M16: not verified. Nothing was built, run or tested; M15 and M16 need a VM.

Review fixes applied on 2026-11-19 (documentation only; the review's findings, each checked against the code or the text before it was applied):
- DT14 sensitivity unsatisfiable, DT1 lacked the load-bearing case (major): added DT1b (untrusted owner, exact expected DACL), DT11b (trusted owner, drifted DACL) and DT3b (validator called on a descendant under a junction ancestor while the final component has no reparse attribute, isolating R2b). DT14 is restated as a per-control table of isolating rows with redundant pairs recorded, instead of "disabling each control makes at least one row fail". DT1 is marked as a pair row.
- V14 and V15 not buildable (major): checked in the source. `AgentHostedService.cs:42-52` takes sealed concrete classes, the non-persisted branch calls `SiteEnforcer.ClearAsync` (`:87`) which deletes keys under `Registry.CurrentUser` (`ChromePolicyRegistry.cs:71`), and `RunIpcLoopAsync` (`:93`) always uses `IpcProtocol.PipeName` (`IpcProtocol.cs:6`). D10 gained item 7 (test seams: interfaces or injectable composition for monitors and enforcers, injectable pipe name or IPC-loop factory, injectable registry root and path, and the rule that no V14 or V15 test starts the real IPC loop, WMI watcher or touches `HKCU`); T052 got scope item 2a. T048 states an injected registry root for V5, so the registry seam may already exist; the text says reuse it if so. V15 is a characterization test, but it cannot literally be written against the code as it stands because the constructor takes concrete classes, so the order is: mechanical interface extraction, V15 green on unchanged behavior, then the rest. AC10 now says V15 and M16 carry the console-mode claim (no source file in `tests/` references `AgentHostedService`, `TrayHost`, `AddHostedService` or `Host.Create`; searched).
- Integration rows need elevation (major): D6 "Elevation" paragraph and T050: the I rows run elevated or in the VM and are "not verified" otherwise, with the alternative of constructor-parameterised trusted principals and a unit-pinned production descriptor; the Result of T050 records the elevation state of each run.
- `FS_PERSISTENT_ACLS`: re-read on the CreateDirectoryW page on 2026-11-19 (the statement is there); R1 volume precondition, DT13b and a source-status row added. The `GetVolumeInformation` page was not read.
- Directory object type: R2g and DT8b.
- Realistic attacker path: DT3, DT3b, DT4, DT5 and DT7 are labelled control-isolation tests, R3 and a paragraph before the table say that nothing under a validated root is user-reachable.
- ProgramData: R5 is conditional on A12, the VM records the actual ACL of `ProgramData` and `C:\`, the known-folder APIs are named (`SHGetKnownFolderPath(FOLDERID_ProgramData)` or `Environment.GetFolderPath(CommonApplicationData)`; the second page was not read) and the `%ProgramData%` environment variable is forbidden, in D6 and T050. R2d is stated to compare the DACL only.
- Q17: extended with the install-time creation by the elevated install step (R1, T052 script item) and the reboot-unavailable consequence next to owner answer K1 Q5, noted for T058 and not answered. Q1–Q16 and Q18 are untouched.
- Session-0 refusal in `TrayHost.Start` is required T052 scope with a unit test; V14 asserts `TrayNotifier` is not registered. Console-only registrations go in a separate `NoInlining` method, and M15 (module list of the service process), not V14, is the evidence for "no `System.Windows.Forms` loaded" (A29 reworded).
- Pipe first-instance failure and squatting: a service-side sentence in D10 item 5 and D4 (no serving on a non-first instance, enforcement continues, retry with backoff; non-zero exit versus degraded start left as a T051/T052 detail), and a T051 scope note and test.
- M14 records `whoami /priv` (new A30); R8 names `temp_store = MEMORY` or a service-only validated temp directory. D10 says console mode is "unchanged in sessions other than 0".
- T016: scope, acceptance and checks now cover removal of the tray interface, `NullAgentTray` and the host-mode seam, and the M15 and M16 re-run; its area gained `src/Windows/FocusManager.Agent/Hosting/**` for the registration helper (the interface lives in `Tray/`, already covered). T016 scope item 3 now says M1–M14 plus M15 and M16. T027 counts updated to A1–A30.
- The review also contained an unrelated finding about `PathRules.cs` device paths; it belongs to T054 and was not touched.
- Not applied or only partly: nothing was rejected. The review's suggestion for a literal pre-refactor V15 was adapted as described above; whether a persistent pipe failure should end the process is left to T051/T052 and is not a product choice. The interrupted earlier fix wave had already named DT3b, DT11b, DT13b, R2g and A30 in the ADR header before their bodies existed; the header is now consistent with the body (DT8b added, DT13b defined as the volume row).
- Checks of this pass (2026-11-19, documentation only, no .NET run, no machine setting, process, registry key or pipe touched):
  - Review finding claims about the code re-read in the working tree: `AgentHostedService.cs:42-52`, `:76`, `:87`, `:93`, `SiteEnforcer.cs:31-34`, `ChromePolicyRegistry.cs:31` and `:71`, `IpcProtocol.cs:6`, `Program.cs:53`, `TrayHost.cs:22-43`: verified. A search of `tests/` source files for `AgentHostedService`, `TrayHost`, `AddHostedService`, `Host.Create` found nothing: verified.
  - `FS_PERSISTENT_ACLS` statement on the CreateDirectoryW page: fetched and read on 2026-11-19: verified. No other primary page was re-read in this pass.
  - A throwaway script over the ADR and T016, T027, T050, T051, T052, T056: every relative link and anchor resolves, every reference-style label used in the ADR is defined, status line is `Proposed`, Q1 to Q18, A1 to A30, V1 to V15, M1 to M16 and the rows DT1, DT1b, DT3b, DT8b, DT11b, DT13b, DT14 exist: verified. Its only prose-like unresolved bracket was a code attribute, not a link.
  - Front matter fields and `depends_on` of all 61 task files parse, every `depends_on` ID exists, and the dependency graph has no cycle: verified, except that the script could not parse the multi-line `area` field of T046, which this pass did not touch. T016 gained one `area` pattern; no `status` or `depends_on` value was changed; `ROADMAP.md`, `STATUS.md` and `REQUIREMENT_COVERAGE.md` were not edited.
  - `git diff --check` on the tracked changed files (T016, T027) and a trailing-whitespace search of all changed files: no whitespace error. Git warns that the CRLF working-copy line endings are normalized to LF on commit (`.gitattributes`: `* text=auto eol=lf`); the files already used CRLF and were kept consistent: verified.
  - Rule and row content (R1 to R8, R2g, DT1 to DT14, V13 to V15, M14, M15, M16, D10 item 7): not verified. Nothing was built, run or tested.
  - The fixes were not independently re-reviewed.

Limitations and items for the main agent and T058:
- The default ACL of `ProgramData` (A12), the owner of objects created by a LocalSystem service (A23), whether the reparse flag affects only the last component (A21), directory sharing semantics (A22), the effect of planted SQLite sidecars (A25) and the behavior of WinForms in session 0 (A27) are unverified assumptions, as is the session-0 behavior of everything in D6 and D10. The strategy does not cover alternate data streams or an administrator.
- Owner answers K1 and L (2026-11-18) conflict with unedited proposal text, to be reconciled by T058: the Q1 premise and Context (one service for all levels), D8 snapshot recapture and service stop (K1 Q5, Q16(b)), the `HKLM` remarks in D7 and the Q15 question, the Q11 import wording in D6 and T050 item 5, and the Q4 and Q8 premises. D6 and D10 were written not to contradict them (Q18 cites K1 Q8; D10 cites K1 Q4 and Q6).
- Sequencing observation, not a decision: T052 does not depend on T017, so a service-mode agent has no tray between T052 and T016 (D10, item 6). The main agent may want T017 ordered before the service is used for anything but development.
- New owner questions: Q17 and Q18 (ADR-0009). No other new product choice was found.
- Completion enters `review` for owner acceptance; this record does not claim it.


> Pause note 2026-11-19 (superseded): the interrupted fix wave was completed by a later pass; the review fixes are listed in the Result. The task stays `in_progress` for the main agent.

Main-agent verification 2026-11-19 (after all edits, Windows 11, SDK 8.0.419): dotnet test FocusManager.Shared.slnf passed (Core 261, SQLite 36); dotnet test FocusManager.Windows.slnf passed (Core 261, SQLite 36, Agent 29, Platform.Windows 85); 0 failures. Status set to review; owner acceptance is pending.
