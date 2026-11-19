---
id: T063
title: Resolve the uncommitted rereview findings
milestone: M1
status: in_progress
depends_on: []
area: ["src/Shared/FocusManager.Core/Rules/PathRules.cs", "src/Windows/FocusManager.Platform.Windows/WindowsHostPathRules.cs", "tests/Shared/FocusManager.Core.Tests/**", "tests/Windows/FocusManager.Platform.Windows.Tests/**", "test-vectors/windows-path-compat.json", "docs/spec/folders.md", "docs/reviews/2026-11-19-uncommitted.md", "docs/reviews/evidence/T063/**", "tasks/T001-make-core-path-rules-os-independent.md", "tasks/T054-fix-windows-path-compatibility-after-review.md", "tasks/T063-resolve-uncommitted-rereview-findings.md"]
---

# T063 — Resolve the uncommitted rereview findings

## Goal

Close the two P2 findings from the 2026-11-19 independent rereview: the extra-leading-separator UNC compatibility regression and the contradictory current handoff. Return a bounded, verified correction package without advancing the feature backlog.

## Sources

- [Current review and finding identifiers F1/F2](../docs/reviews/2026-11-19-uncommitted.md), [retained evidence](../docs/reviews/evidence/2026-11-19/README.md).
- [T001 behavior-preservation criteria](T001-make-core-path-rules-os-independent.md), [T054 remediation and known divergences](T054-fix-windows-path-compatibility-after-review.md).
- [Folder specification](../docs/spec/folders.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), [architecture](../docs/architecture/README.md).
- [T055 isolation/lifecycle boundary](T055-verify-windows-sqlite-startup-and-restarts.md), [T058 service reconciliation gate](T058-reconcile-service-design-with-owner-decisions.md), [owner decisions](../docs/spec/decisions.md).
- Owner instruction on 2026-11-19: record the review now; a different session will receive the correction instruction. This task is prepared only and must not be started by the recording session.

## Scope

In scope:
1. Reproduce F1 against the exact pre-T001 evaluator and current Windows composition, then preserve the empty UNC server slot (or use a verified equivalent compatibility treatment) for paths with three or more leading separators. Keep ordinary UNC server/share semantics, literal root slots, device/verbatim behavior and host resolution covered by existing tests.
2. Add discriminating regression tests and minimal shared Windows vectors for both the candidate and stored-allowlist forms; remove the corresponding parity-generator exclusion only when the compared input domain actually matches.
3. Have the main agent reconcile the operative `STATUS.md` Product Baseline and Next Agent instructions with T054/T056's submitted results, T055's blocker, the current review and acceptance gates. Update the roadmap only where current facts change. Preserve dated review history and clearly label historical results.
4. Record actual checks, limitations and finding disposition in a dated follow-up to the report and the affected task Results. Update a specification clarification before any behavior change without rewriting approved product requirements.

Out of scope: T012 rule changes; fixing all malformed-input categories F3-F5 from T054 without a demonstrated need; new path/product semantics; the T055 real-agent lifecycle; service/IPC implementation or ADR acceptance; Linux work; unrelated changes, commits or pushes.

This reworks unaccepted T001/T054 work and current documentation, rather than consuming accepted new feature output. `depends_on: []` follows the existing closeout convention; it does not accept T001/T054/T062. Readiness records the absence of a technical dependency, not permission to ignore the owner's instruction to leave execution to another session. Shared STATUS/ROADMAP edits belong to the main agent, not a concurrently assigned worker.

## Acceptance Criteria

1. F1's two reported cases return the same decision as `HEAD` `3035e03de6b3c25545bd6b252bfd1f5ab622aed9` (deny), while the ordinary `\\srv\share` child control remains allowed. The tests fail against the reviewed rules and pass after correction; record both commands/outcomes and the reviewed/fixed source states.
2. The regression coverage exercises both pure Windows rules and the production Windows adapter where applicable, includes minimal identifiable Windows JSON vectors, and preserves the Shared/platform boundary. Existing device/verbatim, root-slot, drive-root and 8.3 regression checks remain green.
3. F2's active Product Baseline and Next Agent instructions agree with task front matter and roadmap state. They do not direct a new agent to restart submitted T054/T056 work wholesale, repeat old defects as current, bypass T055 isolation, accept the ADR, resume Linux or advance gated features.
4. Shared and Windows suites pass on Windows; documentation links/dependencies/statuses and the diff pass checks. Any failure is diagnosed, with a comparable baseline run when its cause is unclear. Tests with side effects run in an isolated copy, and the owner's real agent/UI/data/registry/pipe remain untouched.
5. The report records both finding dispositions and the actual verification. T001's Linux criterion stays deferred/unverified, T055 remains blocked until its environment exists, ADR-0009 stays Proposed, and owner acceptance stays separate. Completion enters `review`, not `done`.

## Checks

- Stage the retained evaluator probes outside the working tree as described in the [evidence README](../docs/reviews/evidence/2026-11-19/README.md), or add equivalent focused regression tests. Use `git show 3035e03de6b3c25545bd6b252bfd1f5ab622aed9:src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs` for the baseline. Record the exact project/filter commands after the test names exist; do not invent a successful red run.
- In an isolated copy on Windows, run `dotnet test FocusManager.Shared.slnf` and `dotnet test FocusManager.Windows.slnf`. Inspect the JSON examples against actual evaluator decisions and the adapter tests. No Linux command is authorized.
- Reconcile STATUS against the current T001/T002/T027/T054/T055/T056/T058/T062/T063 records and ROADMAP; preserve the distinction between a submitted implementation, an independently reviewed result and owner acceptance.
- Run `git diff --check` and a whitespace/link check for new files; inspect the integrated diff. Record the actual source state and any unavailable manual/host checks.

## Open Questions

None for the two bounded corrections. If a real product interpretation is needed beyond preserving the baseline, record it in the owning specification and pause only that affected change for the owner.

## Result

2026-11-19: started in the owner-directed follow-up session to resolve the two existing review findings. This does not authorize the feature backlog or accept earlier submissions. T055's missing isolated environment and the Linux hold are unchanged.
