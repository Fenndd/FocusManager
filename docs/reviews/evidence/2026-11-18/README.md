# Retained review evidence — 2026-11-18

These files preserve compact evidence from the original independent review, not a new execution during T053. See the [review report](../../2026-11-18-uncommitted.md) for scope, findings, authority and limitations. The original temporary review copy and HEAD baseline are not needed to read these results.

| File | Original evidence retained |
| --- | --- |
| [windows-tests.excerpt.txt](windows-tests.excerpt.txt) | `_review-windows-tests.log`: PowerShell Windows suite test assemblies, runner version, success/totals |
| [gitbash-tests.excerpt.txt](gitbash-tests.excerpt.txt) | `_review-gitbash-tests.log`: Windows suite in Git Bash, all 155 tests passing |
| [msbuild.excerpt.txt](msbuild.excerpt.txt) | `_review-msbuild.log`: Visual Studio MSBuild 18.3.0 and successful build outputs including WinUI App |
| [windows-build.excerpt.txt](windows-build.excerpt.txt) | `_review-windows-build.log`: CLI MSB4062 `ExpandPriContent` failure |
| [baseline-build.excerpt.txt](baseline-build.excerpt.txt) | `_review-baseline-build.log`: same CLI failure in separate HEAD baseline copy |
| [regression-probes.excerpt.txt](regression-probes.excerpt.txt) | `_review-regression-probes.log`: exact diagnostic inputs/results and 1 passed / 4 failed equality probes |
| [mutation.excerpt.txt](mutation.excerpt.txt) | `_review-mutation.log`: targeted test failure after suppressing one notifier invocation in disposable review copy; source bytes restored afterward |
| [BaselineRuleEvaluator.cs.review](BaselineRuleEvaluator.cs.review) | HEAD `RuleEvaluator.cs` renamed `BaselineRuleEvaluator` for a same-process comparison |
| [ReviewRegressionProbeTests.cs.review](ReviewRegressionProbeTests.cs.review) | Exact five-case diagnostic source used for the differential comparison |

Local copy roots are replaced by `<REVIEW_COPY>` / `<HEAD_BASELINE_COPY>` and the user's NuGet cache prefix by `<USER_NUGET>`. Selected test/probe/mutation lines omit repetitive stack traces and individual passing tests. Other short build/run outputs are retained without truncating the diagnostic. These are outcome excerpts, not a complete console transcript or a source manifest. The original 215-file preservation index digest is recorded in the report.

## Environment and commands

Original execution: Windows, .NET SDK 8.0.419, runtime 8.0.25, VSTest 17.11.1; Visual Studio MSBuild 18.3.0-release-26070-10+3972042b7. Git Bash was a Windows shell for the second suite; it supplies no Linux-host evidence.

The original reviewer supplied these exact historical commands; excerpts preserve their outcomes:

```powershell
# PowerShell, isolated reviewed copy
dotnet test FocusManager.Windows.slnf --logger 'console;verbosity=normal'
dotnet build FocusManager.Windows.slnf --no-restore -v minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' FocusManager.Windows.slnf /t:Build /p:Configuration=Debug /v:minimal

# Separate baseline copy from git archive of HEAD, including restore
dotnet build FocusManager.Windows.slnf -v minimal

# Diagnostic comparison in reviewed copy
dotnet test tests/Shared/FocusManager.Core.Tests/FocusManager.Core.Tests.csproj --filter FullyQualifiedName~ReviewRegressionProbeTests --logger 'console;verbosity=detailed'

# Diagnostic mutation in reviewed copy: one notifier invocation replaced with
# await Task.CompletedTask, then source bytes restored in finally
dotnet test tests/Windows/FocusManager.Agent.Tests/FocusManager.Agent.Tests.csproj --filter FullyQualifiedName~EnforceAsync_Notifies_WhenDeniedProcessWasKilled --logger 'console;verbosity=normal'
```

Git Bash invocation: `Bash -c 'dotnet test FocusManager.Windows.slnf'`. Use the current launcher to resolve supported MSBuild for future runs, and record the executable/flags actually used.

For a future Windows-only reproduction of the diagnostic comparison, create an isolated review copy, copy the two `.cs.review` files into that copy's Core test project as `.cs` files, then use:

```powershell
dotnet test tests/Shared/FocusManager.Core.Tests/FocusManager.Core.Tests.csproj --filter FullyQualifiedName~ReviewRegressionProbeTests --logger "console;verbosity=detailed"
```

The probe calls the OS-dependent baseline's `Path.GetFullPath`: its `\Study` case resolved on drive C: and its `D:` case resolved to `D:\` in the recorded run. Record the actual current directory and drive-relative resolution before comparing; different Windows per-drive current directories can change the baseline result. The new and HEAD evaluators run in the same .NET 8 process; do not substitute historical .NET Framework behavior. Expected historical diagnostic outcome is four failed equality comparisons, not a passing production test suite. After a fix, add proper expected-result regression tests under T054 instead of shipping the temporary baseline class.

No Linux run, live agent/UI launch, owner database access or real alternate-data-stream process execution is evidenced here. The owner has subsequently placed all Linux development, including provisioning/runs for Shared verification, on hold until explicit resumption.
