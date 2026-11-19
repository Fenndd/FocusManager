---
id: T045
title: Make the AppEnforcer termination test independent of PATH
milestone: M1
status: review
depends_on: []
area: ["tests/Windows/FocusManager.Agent.Tests/AppEnforcerTests.cs"]
---

# T045 — Make the AppEnforcer termination test independent of PATH

## Goal

Make `AppEnforcerTests.EnforceAsync_Notifies_WhenDeniedProcessWasKilled` pass from any shell on Windows, so the Windows suite is a repeatable check.

## Sources

[Roadmap M1](../ROADMAP.md) (repeatable checks) and [Windows requirements](../docs/spec/platforms/windows.md) (WIN-003). No requirement changes.

Found during T002 verification on 2026-11-17. The test starts `cmd.exe /c timeout /t 30 >nul` and expects the process to be alive when `AppEnforcer` terminates it (`tests/Windows/FocusManager.Agent.Tests/AppEnforcerTests.cs:117-137`). `cmd.exe` resolves `timeout` through `PATH`. When `dotnet test FocusManager.Windows.slnf` is started from Git Bash, the GNU `timeout` in Git's `usr/bin` is found first, rejects `/t`, and the process exits before enforcement; the test then fails with `Assert.Single() Failure: The collection was empty`. Started from PowerShell, the same build passes (26/26 in `FocusManager.Agent.Tests`). `AppEnforcer` and the test were not changed by T002.

## Scope

In scope: start a child process whose lifetime does not depend on `PATH` resolution or an attached console (for example a fully qualified system executable, or the test host itself), and keep the existing assertions and cleanup.
Out of scope: changes to `AppEnforcer`, other tests, or enforcement behavior.

## Acceptance Criteria

1. `dotnet test FocusManager.Windows.slnf` reports no failure in `FocusManager.Agent.Tests` when started from PowerShell and when started from Git Bash on the same Windows host.
2. The test still fails if `AppEnforcer` does not notify after terminating a denied process.
3. `src/**` is unchanged.

## Checks

- Run `dotnet test FocusManager.Windows.slnf` on Windows from PowerShell and from Git Bash.
- Temporarily break the notification in a local, uncommitted edit (or reason from the assertion) to confirm criterion 2; record which was done.
- Inspect the diff for criterion 3.

## Open Questions

None

## Result

Implemented in `tests/Windows/FocusManager.Agent.Tests/AppEnforcerTests.cs` (`EnforceAsync_Notifies_WhenDeniedProcessWasKilled`). The child process is now `Path.Combine(Environment.SystemDirectory, "ping.exe") -n 30 127.0.0.1` instead of `cmd.exe /c timeout /t 30 >nul`: the executable is fully qualified (no `PATH` lookup, so Git Bash's GNU `timeout` is never picked up), does not read stdin (so it does not exit when the test host's input is redirected, which `timeout.exe` rejects), and stays alive about 29 seconds. Stdin, stdout and stderr are redirected so it does not depend on an attached console. Assertions, the 200 ms delay, `WaitForExit(3000)` and the best-effort kill cleanup are unchanged. Only that test file and this Result section were edited by this task.

Checks:

- `dotnet test FocusManager.Windows.slnf` from PowerShell: verified. Core.Tests 14/14, Platform.Windows.Tests 6/6, Persistence.Sqlite.Tests 35/35, Agent.Tests 26/26 passed, 0 failed.
- The same command from Git Bash (Bash tool; `which timeout` resolves to `/usr/bin/timeout`, the GNU binary that caused the original failure): verified. Same counts, Agent.Tests 26/26, 0 failed.
- Acceptance criterion 2: verified by a mutation check in the review follow-up. The `_notifier.ShowBlockedAsync(...)` call in `src/Windows/FocusManager.Agent/Enforcement/AppEnforcer.cs` was temporarily replaced by a no-op, and `dotnet test tests/Windows/FocusManager.Agent.Tests --filter AppEnforcerTests.EnforceAsync_Notifies_WhenDeniedProcessWasKilled` then failed with `Assert.Single() Failure: The collection was empty` (`AppEnforcerTests.cs:143`). The file was restored byte for byte from a backup, and `git diff -- src/Windows/FocusManager.Agent/Enforcement` is empty. The test passes only after a real kill-then-notify path, since the fake executable path `C:\Apps\blocked.exe` is denied by an empty `WhitelistConfig`.
- Acceptance criterion 3: this task made no edits under `src/**`. `git status` shows `src/**` modifications that pre-date this task (T002 SQLite work); none touch `AppEnforcer`.

Limitations: the test still relies on `ping.exe` being present in the Windows system directory (standard on Windows). Status is left for the main agent to move to `review`.
