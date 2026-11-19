---
id: T050
title: Move Windows storage to service-owned per-user partitions
milestone: M2
status: planned
depends_on: ["T002", "T048", "T027"]
area: ["src/Shared/FocusManager.Persistence.Sqlite/**", "src/Windows/FocusManager.Agent/**", "src/Windows/FocusManager.Platform.Windows/**", "tests/Shared/FocusManager.Persistence.Sqlite.Tests/**", "tests/Windows/**", "docs/spec/platforms/windows.md", "tasks/T050-*"]
---

# T050 — Move Windows storage to service-owned per-user partitions

## Goal

Store each OS user's data under a service-owned protected directory, outside every user profile, create and open that directory tree safely even when a standard user created objects there first, and provide the durable policy ledger (ADR-0009, step S3). The agent stays a console process; in console mode the partition belongs to the user running the agent.

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed): D6 and its [safe creation and opening of the data tree](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#safe-creation-and-opening-of-the-data-tree-t056-proposal) (rules R1–R8 and R2g, adversarial checks DT1–DT14 with the sub-rows DT1b, DT3b, DT8b, DT11b and DT13b), [source status for D6](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#source-status-for-d6), hazard H6, checks V10 and V13, assumptions A12, A14, A16 and A21–A26, owner questions Q17 and Q18. [T056 result](T056-clarify-proposed-service-design-after-review.md#result) (the documentation task that added the strategy; it is untested and does not accept the ADR). [T002](T002-restore-sqlite-storage.md) (the SQLite store whose path is an argument), [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md), [Windows requirements](../docs/spec/platforms/windows.md) (WIN-006).

## Scope

In scope:
1. Resolve `ProgramData` with `SHGetKnownFolderPath(FOLDERID_ProgramData)` or `Environment.GetFolderPath(CommonApplicationData)` (check on the VM that they agree); never read the `%ProgramData%` environment variable. Check that the volume reports `FS_PERSISTENT_ACLS` (`GetVolumeInformation`) and refuse otherwise (R1, DT13b). The tree is created by the same code from the elevated install step where one exists (R1; the T052 scripts, later T041), so that a refusal appears at install time. Create `FocusManager\`, `users\` and `users\<SID>\` under the D6 rules R1–R8. In short: create each directory with `CreateDirectoryW` and an explicit protected DACL (full access for LocalSystem and Administrators, none for anyone else), so that a directory is never adopted by resetting its DACL; treat `ERROR_ALREADY_EXISTS` as a pre-existing object and validate it by handle without following reparse points (not a reparse point, final path equal to the expected path, trusted owner, exact DACL, correct object type, one link, only expected entries) or refuse it; refuse rather than repair, delete or rename; hold the validated directories without `FILE_SHARE_DELETE`; accept partition names only from the canonical string of the authenticated SID; re-validate on events; no fallback path outside the tree. The Windows calls are P/Invoke in `FocusManager.Platform.Windows`; shared code receives only the validated path. A refusal reports the affected partition as `unavailable` and changes nothing (what happens next is Q17).
2. A partition factory: `users\<SID>\focusmanager.db` opened with the T002 store after the service itself created the main database file (D6 R4) and followed by the post-open identity recheck (R5); the whitelist schema does not change. In console mode the SID is that of the process user and the same code runs in the elevated agent, which refuses a user-precreated root in the same way; the SID comes from the authenticated caller once T051 exists. Evaluate the optional SQLite defenses of R8 without changing the T002 storage tests.
3. The durable implementation of the T048 policy-ledger port: a new table in the partition, with a migration test in the style of the T002 storage tests.
4. WIN-006 note updated in `docs/spec/platforms/windows.md` before the behavior changes.
5. Legacy import from `%LOCALAPPDATA%\FocusManager\focusmanager.db`, only if the owner answers ADR-0009 Q11 with "import": performed by a component in the user's own context through the existing `SaveWhitelist` request (the component is chosen in this task); never by LocalSystem opening a file in a profile. If Q11 is "recreate", this item is dropped.
6. The adversarial test suite V13 (D6, DT1–DT14). The data-tree code takes an injected root, tests use a unique temporary directory, and junction targets are harness-created stand-in directories outside the tree, never real protected locations. The suite must distinguish the strategy from merely resetting the root ACL: a test double `ResetRootAclOnlyOpener` fails DT1, DT1b, DT2, DT3, DT3b, DT4, DT5 and DT7, and each control disabled alone makes its isolating row fail (the per-control table of the DT14 rule: for example DT1b for R2c, DT11b for R2d, DT3b for R2b, DT6 for R2a, DT8b for R2g, DT13b for the volume precondition). Controls that overlap (R2a and R2b on a junction at the object, R2c and R2d on a permissive user-owned root) are checked through their isolating rows, and a row that still fails with its control disabled is recorded as a redundant pair that must fail when both are disabled. DT3, DT3b, DT4, DT5 and DT7 are control-isolation tests that simulate an administrator-modified or already-compromised tree; the realistic attacker path is the user-precreated root (DT1, DT1b), and the Result must not describe the isolation rows as attacks open to a standard user under a validated root. The integration rows (I) create trees that only LocalSystem and Administrators can access, so they run elevated or in the VM and are reported "not verified" when run non-elevated; alternatively the trusted-principal set and expected descriptor are constructor parameters, the production value is pinned by a unit test of the descriptor builder, and DT13 on the production descriptor still runs elevated or in the VM. The Result records the elevation state of each run and which alternative was used; the VM also records the actual ACL of `ProgramData` and `C:\` (A12) before the DT results are relied on. The SQLite temporary-file location is evaluated with R8 (`temp_store = MEMORY` or a service-only validated directory). Cases that need a second standard account (DT1, DT4, DT7) are manual on a virtual machine until automated; a case the test account cannot create (for example a symbolic link without the privilege) is recorded as not verified, not skipped silently.

Out of scope: caller identity and authorization (T051), the service host (T052), cloud synchronization, new profile schemas (T021), uninstall deletion of user data (ADR-0009 Q13, T041), protection against an administrator or SYSTEM (M7, T034), alternate data streams (not covered by D6).

## Acceptance Criteria

1. Partitions of two SIDs are isolated: neither store sees the other's rows, and the resolved path is outside every user profile (V10).
2. After setup, a standard user cannot read or write the data directory (ACL inspection), and the effective descriptor of every object the service created equals the D6 R1 descriptor with a trusted owner (DT13).
3. A pre-created root, `users` directory or partition directory with an untrusted owner or a drifted DACL; a junction or other reparse point at any level; a hard-linked or symbolically linked database; planted `-journal`, `-wal` or `-shm` entries; unexpected entries; and invalid partition names are refused with the objects untouched and nothing written to any stand-in directory (DT1–DT9, DT11, DT12 and the sub-rows DT1b, DT3b, DT8b, DT11b), including a user-owned root whose DACL is exactly right (DT1b) and a trusted-owner object with a drifted DACL (DT11b), and a volume without `FS_PERSISTENT_ACLS` creates nothing (DT13b). A pre-created directory is never adopted by resetting its DACL.
4. The suite is sensitive: it fails against the `ResetRootAclOnlyOpener` double, and each control disabled alone makes its isolating row fail, with redundant pairs recorded (DT14 rule). The swap-between-check-and-open cases (DT9, DT10) are recorded as a probe with the chosen iteration count, not as a proof.
5. Ledger entries persist across reopening the store; the T048 crash-reconciliation tests also pass against the durable ledger.
6. T002 storage tests pass unchanged.
7. The Result records A12, A21–A26 (and A14 only if the fallback is used) as verified, not verified or failed, lists the DT cases that could not be run, records the elevation state of every integration run, and records the VM's `ProgramData` ACL.

## Checks

- `dotnet test FocusManager.Shared.slnf` (storage tests).
- `dotnet test FocusManager.Windows.slnf` on Windows (path, ACL, partition and adversarial tests DT1–DT14 and sub-rows on a real NTFS temporary tree; run elevated, otherwise the integration rows are "not verified").
- Manual, Windows virtual machine with a second standard account: run the agent as an administrator, start a session, confirm `%ProgramData%\FocusManager\users\<SID>\focusmanager.db` exists and that `icacls` shows no access for the standard user; run the VM rows of V13 (DT1, DT1b, DT4, DT7) and record the ACL of `ProgramData` and `C:\`. Record the accounts and the Windows build. These checks never touch the owner's real `%ProgramData%\FocusManager`, `%LOCALAPPDATA%\FocusManager` or running agent.
- Independent review of the P/Invoke and validation code, because it runs as LocalSystem.

## Open Questions

ADR-0009 must be accepted (T027), and T002 must be accepted or explicitly authorized for continuation. Owner answers of 2026-11-18 ([decisions K1](../docs/spec/decisions.md)) supersede conflicting proposal text, including the legacy-import wording; [T058](T058-reconcile-service-design-with-owner-decisions.md) reconciles it. Owner questions in ADR-0009: Q2 (which OS users may use FocusManager), Q11 (legacy import), Q13 (user data on uninstall), and from T056 Q17 (what happens after the service refuses an unsafe object, including the install-time creation and the reboot-unavailable consequence next to K1 Q5, noted for T058) and Q18 (whether Administrators keep access to the data tree). Item 5 waits for Q11.

## Result

Not started. No implementation or checks have been completed for this task. The safe-creation strategy, its rules R1–R8 and R2g and the checks DT1–DT14 with their sub-rows were written by the documentation task [T056](T056-clarify-proposed-service-design-after-review.md#result); they are a proposal, untested, and do not accept ADR-0009.
