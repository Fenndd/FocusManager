# Evidence for the 2026-11-19 uncommitted rereview

These artifacts record the completed review before T062 recording changes. They are not results of executing T062. The baseline is `3035e03de6b3c25545bd6b252bfd1f5ab622aed9`; the reviewed input had 76 tracked changed/deleted paths and 57 non-ignored untracked files, with an empty index. Sources were copied to an external Windows snapshot before execution. SDK: 8.0.419. Original private temporary-directory prefixes are replaced with `<isolated-review>`; test counts, result lines, project-relative paths and probe values are preserved. The four small build/test logs are retained as sanitized excerpts; no original full file inventory or private paths are included.

## Artifacts and provenance

| File | What it establishes |
| --- | --- |
| [shared-tests.excerpt.txt](shared-tests.excerpt.txt) | PowerShell `dotnet test FocusManager.Shared.slnf`: Core 261 + SQLite 36, zero failures/skips |
| [windows-tests.excerpt.txt](windows-tests.excerpt.txt) | PowerShell `dotnet test FocusManager.Windows.slnf`: 411 passed, zero failures/skips |
| [gitbash-tests.excerpt.txt](gitbash-tests.excerpt.txt) | The same Windows solution tests under Git Bash on Windows: 411 passed |
| [windows-msbuild.excerpt.txt](windows-msbuild.excerpt.txt) | Visual Studio MSBuild 18.3 full Windows build output, including `FocusManager.App`/WinUI; reviewer recorded exit success |
| [evaluator-probes.log](evaluator-probes.log) | Executed HEAD/current evaluator comparison: two differences and the ordinary UNC control |
| [Program.cs.review](Program.cs.review), [BaselineRuleEvaluator.cs.review](BaselineRuleEvaluator.cs.review), [probes.csproj.review](probes.csproj.review) | Inert diagnostic sources from the executed probe; not repository production/test inputs |
| [final-integrity.json](final-integrity.json) | End-of-review static/preservation summary: 61 tasks, no metadata issues, 249 source files unchanged, index unchanged |

The review also checked unchanged Git status and an acyclic dependency graph. The compact integrity JSON is a result summary, not the full verifier or an independently reproducible 249-file hash manifest. Baseline evaluator source is the HEAD `RuleEvaluator.cs`, renamed only to `BaselineRuleEvaluator` so both implementations can run in one process.

## Reproducing the evaluator probe later

This procedure is for a separately authorized correction/review session on Windows. It is not a real agent/UI launch and does not establish browser/Explorer behavior.

1. Stage the exact reviewed tree outside this repository as `<isolated-review>/snapshot`. Include the Core and Platform.Windows project sources and their build settings. A future tree may give different results; record its revision/diff and do not describe it as this snapshot.
2. Create `<isolated-review>/evaluator-probes` beside `snapshot`. Copy the three `.review` files there and remove only their final `.review` suffix. Their project references then resolve to `../snapshot/src/Shared/FocusManager.Core/FocusManager.Core.csproj` and `../snapshot/src/Windows/FocusManager.Platform.Windows/FocusManager.Platform.Windows.csproj`.
3. To independently reconstruct the baseline, extract `git show 3035e03de6b3c25545bd6b252bfd1f5ab622aed9:src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs` from the repository. Rename its declared class from `RuleEvaluator` to `BaselineRuleEvaluator`; compare it with the retained baseline file. Do not replace the current evaluator.
4. On Windows with .NET SDK 8.x, run `dotnet run --project <isolated-review>/evaluator-probes/probes.csproj`. All three rows must print and the program must exit successfully; its expectations encode the reviewed differences, not a desired post-fix result. For a repair, change the expected current results only after recording the before-fix output and the intended compatibility behavior.
5. Compare literal path strings, not their rendered appearance: the two failure cases have three initial backslashes in the candidate or stored allowed path. The control has two. `AllowSubfolders` is true for every row.

Do not add these diagnostic files under production compile globs. No live UNC server, real Explorer path, Linux host or service behavior was tested by this probe.
