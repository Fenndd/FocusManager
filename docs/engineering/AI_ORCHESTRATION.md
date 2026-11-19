# Working with Subagents

## FocusManager Execution Protocol

This project adaptation was added in T026. [AGENTS.md](../../AGENTS.md) defines authorization and code boundaries;
[TASK_FORMAT.md](TASK_FORMAT.md) defines the lifecycle. This policy coordinates work and does not grant permission to implement unready product work.

### Start and Select Work

1. Inspect Git status and relevant diffs before editing. Preserve unfinished work; read STATUS.md and ROADMAP.md, then the selected task and its linked specification/ADRs. Do not load all project documents by default.
2. For a chat request, create its task first. Select a `ready` task with every dependency accepted (`done`) or explicitly authorized for continuation under TASK_FORMAT.md; use roadmap order among eligible tasks. Research may collect evidence before its outcome is known, but may not implement an undecided outcome.
3. Check task size and environment. Split work before execution if it cannot produce one independently verifiable result in one session. Planning gates create child implementation tasks; their completion never marks a feature implemented.
4. Set the task, roadmap and STATUS.md to `in_progress` together. The main agent owns these shared coordination files. Worker results do not change lifecycle state independently.
5. Update the owning specification before behavior changes. Product questions stay next to the requirement as `> ❓ Question:`; ask the owner when the answer is needed. A proposal, research result or model preference is not an accepted requirement/ADR.

### Parallelism and Ownership

- Compare declared `area` patterns, real files and dependency ancestry before dispatch. Empty/unclear or overlapping areas mean sequential edits. Distinct task IDs do not imply independent files.
- In FocusManager, Core/rule changes (T001/T012), storage migration, session extraction, contracts, service lifecycle and UI integration often share files. Recheck actual conflicts instead of launching a worker per roadmap row.
- Android, Linux and payment research can gather evidence concurrently with PC work when useful, but often share ADR/specification/task indexes. Use read-only findings for those shared areas and let the main agent apply coordinated edits sequentially.
- Give a writer exclusive ownership of named paths for the delegated interval. If unexpected overlap appears, stop the conflicting edit and report it; never overwrite another worker's changes. Do not reset or clean a dirty checkout to obtain isolation.
- Keep dependent operations sequential. Contracts and accepted decisions precede their consumers; migration and service/IPC work require proportionate checks; choose the assignment tier under Choosing a Tier below, rather than escalating from the topic alone. Batch only independent reads or checks.

### Brief and Return Contract

A brief must name the task ID, selected tier/model/effort, policy-read requirement, goal, source paths and accepted decisions, exact editing boundaries or read-only mode, dependencies, acceptance checks and requested return format. Include relevant dirty-tree context. Never pass secrets or unrelated history.

Return: changed paths or findings with verified source locations; actual commands/manual checks with `verified`, `not verified` or `failed`; unresolved questions and blockers. Request compact evidence, not copied files. The main agent inspects original sources and the integrated diff before accepting findings.

### Verify, Review and Close

- Choose checks from the task and actual tooling. Documentation: content, links, task schema/dependencies/readiness, roadmap/status agreement and diff. Shared C#: `dotnet test FocusManager.Shared.slnf`. Windows changes: `dotnet test FocusManager.Windows.slnf` on Windows plus relevant manual checks. Follow new platform READMEs only after those commands exist.
- Report unavailable Linux/Windows/device/manual checks explicitly. A build is not proof of enforcement, session-0 behavior, reboot recovery, access isolation or live store lifecycle behavior. Do not silently dismiss a failure using a dated baseline.
- Use independent read-only review when it adds value for significant integration, persisted data, privileged IPC or protection changes. This does not mandate a worker for every small edit. The main agent resolves findings and reruns affected checks.
- Put the actual result/checks in the task and set it to `review`; only established owner acceptance or explicit prior acceptance authority with its conditions met permits `done`. Update roadmap and STATUS.md together. Start dependents while a prerequisite is in `review` only under explicit scoped owner continuation authority recorded under TASK_FORMAT.md; keep the prerequisite in `review`.
- Missing decisions/access/data go to the owner or owning task, not a stronger model. For reasoning failures use the tier gates below; stop repeated ineffective attempts. Record a started task as `blocked` only with a concrete reason and the next action.
- Before ending, leave STATUS.md with the next eligible task, pending acceptance, unanswered decisions and unverified checks. Do not infer T025 acceptance from a new preparation request. Commits, pushes, account changes, publication and deployment follow explicit authorization.

The model tables below are routing preferences, not enforced infrastructure. No scheduler, hook or external service is installed by this protocol.

## Operating Sequence

For substantial work, use this sequence. Choose useful assignments, not an agent count; there are no policy quotas, target proportions, or artificial concurrency limits. Respect actual tool and session limits.

1. Establish the requested result, confirmed requirements, dependencies, and observable checks. Use bounded investigation to discover facts; ask the user about material product decisions outside existing authorization.
2. Identify coherent results to delegate: investigation, implementation, diagnosis, documentation, or verification. Assign substantive bounded results to working-default executors. Before retaining substantial execution in the main session, identify a concrete reason, such as an unavailable tool or inseparable context that makes handoff ineffective. Keep short actions and coordination in the main session; do not create an agent for every command or fragment a result into uncheckable pieces.
3. Decide scheduling separately. Run independent assignments in parallel where supported; delegate dependent stages sequentially after checking the preceding output. A shared contract or sequential dependency does not require the orchestrator to implement everything itself.
4. Start each assignment at tier 2. Use tier 1 for determined work, tier 3 for a documented reasoning gap, and tier 4 only through its gate. Choose for the assignment, not the parent task.
5. Give the executor the brief below and explicitly apply the client's model and effort settings where supported. Do not inherit the orchestrator's settings accidentally.
6. Inspect the result and evidence. Resolve missing checks or conflicting claims before accepting a significant conclusion, without repeating the executor's entire investigation.
7. After a stronger agent resolves its question, return ordinary implementation, documentation, and checks to the working default. Integrate results and update the owning task/state documents.

An orchestrator reads the common sections through Data Handling and its client's section before delegation. An executor reads the common sections before work; it needs launch details only if it will delegate. Do not reread unchanged policy during every step of the same assignment. This is advisory routing through existing tools, not an enforced router.

## Orchestrator and Executors

The selected main-session orchestrators are GPT-6.1 Terra at ultra effort in Codex and Sonnet 5.5 at high effort in ultracode mode in Claude. These are user-selected session settings; this repository does not switch the main session or establish client support for the ultracode label. The working executors are GPT-6.1 Sol medium and Sonnet 5.5 high respectively. Keep the model and effort set fixed unless the user changes it.

The orchestrator interprets the request, coordinates dependencies, assigns work, checks evidence, and owns integration and the final report. It may delegate a bounded integration stage, contract analysis, planning question, or independent review when that requires reasoning it cannot reliably supply. Responsibility for concurrency, security, or cross-component correctness does not oblige it to personally implement those changes. Product behavior remains governed by confirmed requirements and the user's decision authority.

Executors perform most substantive engineering work at the working default, including normal implementation judgment, multi-file changes, debugging, design comparisons, and review. They may choose implementation details within the brief and existing requirements. Report new product requirements or significant unapproved decisions as proposals. Define roles for the current task; do not pre-create permanent role profiles or bundle unrelated work into one stronger-model assignment.

## Choosing a Tier

Every assignment has one tier. Tier 2 is the default, including ordinary uncertainty that bounded investigation can settle.

| Tier | Use for the delegated task |
| --- | --- |
| 1 — determined | Locate facts in named sources or apply an existing pattern with no meaningful choice between approaches. Direct comparison, search, or an existing check verifies the result. |
| 2 — working default | Implement, diagnose, review, test, or document a bounded result. Expected behavior and meaningful checks are known or can be established through bounded investigation. Normal design choices and competing initial hypotheses belong here. |
| 3 — targeted difficulty | Resolve one identified reasoning gap that tier 2 cannot adequately settle through ordinary bounded investigation. Before launching, record the question, specific difficulty and evidence, and limited output/check expected from the stronger agent. |
| 4 — exceptional | Resolve a focused question only when every condition of the tier-4 gate holds. Do not assign an entire feature or broad audit. |

The topic's label (architecture, security, concurrency, persistence, migration), file count, context volume, consequences, parent model, or lack of confidence does not set the tier. High consequences require stronger checks and independent verification, not automatic escalation. Missing requirements, access, data, or a usable brief are not reasoning gaps. Diagnose a failed check first: improve the brief, environment, implementation, or verification as appropriate. Escalate only for remaining reasoning difficulty; do not repeat ineffective attempts indefinitely.

For tier 3, "important architecture work" is insufficient. "Determine whether these two ordering guarantees can both hold under the documented interleaving; the available test cannot distinguish the candidate protocols" identifies a question and a verification limitation. A failed tier-2 attempt is useful evidence but is not mandatory when the difficulty is already concrete. If the justification is absent, use tier 2 for bounded investigation instead of selecting a stronger model as insurance.

### The Tier-4 Gate

Record all of the following in the brief before selecting tier 4:

1. The narrow question or artifact and its boundaries.
2. The concrete reasoning difficulty that makes tier 3 inadequate, supported by known interacting constraints or unresolved contradictory evidence. A failed tier-3 attempt is useful but not required when the difficulty is clear in advance.
3. Why an incorrect answer would be costly or hard to detect **and** why available checks or independent review at tier 3 cannot adequately resolve that difficulty.
4. How the orchestrator will obtain adequate verification before integration, including a bounded independent check if it cannot assess the reasoning itself.

If a condition is absent, use tier 3 or below and improve verification. A tier-4 failure returns the unresolved issue and evidence to the orchestrator; it does not authorize repeating a broad request.

### Return to the Working Default

A tier-3 or tier-4 assignment ends at its stated deliverable. Usually it returns a conclusion, invariant, diagnosis, or proposed contract with evidence and uncertainty. Direct edits are allowed when the narrow implementation itself needs that reasoning and the brief assigns those paths. Do not automatically extend the stronger agent's assignment into surrounding code, routine tests, documentation, or follow-up work; give those steps to the working default. Reusing a stronger agent for another difficult question requires a fresh, specific justification. Do not resume it for routine work merely because it has context.

## Brief and Coordination

Each brief states the result or question, tier, required sources, relevant confirmed decisions, read-only mode or exact edit boundaries, completion criteria and checks, and the instruction to read the common policy sections. Include the tier-3 justification or all tier-4 conditions when applicable. Provide minimal sufficient context instead of copying the entire history. Clarify who owns a shared contract before dependent implementation; bounded proposals and investigation can be delegated at tier 2.

Parallel editors own disjoint implementation areas. The orchestrator is the single writer/integrator of shared `ROADMAP.md` and `STATUS.md` facts; assign each task file to one writer. See [task areas and acceptance](TASK_FORMAT.md). Run dependent changes sequentially. Use an isolated checkout only when needed; do not create duplicate implementations without a concrete reason. Executors respect their boundary even when broader edits seem helpful.

### Subagent Duties

1. Read the common sections of this policy before starting, follow applicable repository instructions, and check that the brief supports its assigned tier.
2. Stay within the brief and authorized requirements. Establish missing facts/checks through bounded investigation where possible. If a material decision or genuine higher-tier difficulty blocks progress, report the exact gap and evidence before affected changes. Do not silently select a stronger model. If work is simpler than expected, finish without restarting merely to change tiers.
3. For reading work, return compact findings with source/file/symbol references, contradictions, and uncertainty. For writing work, edit assigned files directly and return paths and relevant behavior. Do not return whole files or a generic repository tour.
4. Return the result, evidence against completion criteria, actual checks and outcomes (verified / not verified / failed), and unresolved issues. A successful command does not establish every criterion.

## Verification and Integration

The orchestrator inspects relevant original source sections, significant assumptions, diff, and checks proportionally to risk. A summary or "done" message alone is insufficient. Verify important cited locations; do not reread every file to reproduce the worker's effort. Reports should allow checking results without repeating the investigation.

When the orchestrator cannot reliably assess a substantive claim, delegate a bounded independent review, normally at tier 2. Give the reviewer requirements, sources, result/diff and checks without treating the implementer's conclusion as established. Apply the same tier gates to reviews and implementation. Independence means a separate assessment, not a renamed self-review.

When agents disagree, identify the conflicting factual claim, contract, or invariant; inspect its owning source or obtain a discriminating check. Disagreement alone does not justify escalation. Record unresolved uncertainty instead of accepting consensus or an unsupported stronger-model answer. New product decisions still require user authority. Implementation completion and acceptance are distinct; follow [the workflow](DEVELOPMENT_WORKFLOW.md) and task status rules.

## Data Handling

Do not send secrets, credentials, personal data, or other confidential content to development assistants or external services. Use permitted synthetic fixtures, source code without secrets, and content-free measurements. Routing does not expand permissions or authorize external sharing, publication, deployment, or destructive operations.

## Choosing a Model in Codex

These fixed combinations concern development executors, not product models. They are routing preferences, not verified price rankings or quality guarantees.

| Tier | Model | Effort |
| --- | --- | --- |
| 1 | GPT-6 Luna (`gpt-6-luna`) | max (always) |
| 2 | GPT-6.1 Sol (`gpt-6.1-sol`) | medium |
| 3 | GPT-6.1 Sol (`gpt-6.1-sol`) | high |
| 4 | GPT-6 Astra (`gpt-6-astra`) | high |

When the collaboration tool supports per-agent overrides, pass both `model` and `reasoning_effort` from the selected row with `fork_turns: "none"`. In that interface a full-history fork inherits the parent and does not accept overrides; recheck the actual schema if it changes. Do not assume that fork behavior describes every Codex surface.

Supporting Codex clients also offer `[agents]` defaults and project custom-agent files under `.codex/agents/`. The optional source template `presets/codex-config.toml` sets only the working-default executor; merge it into a destination's `.codex/config.toml` through the transfer procedure after checking support. It does not set the orchestrator, grant permissions, enable tools, or cap agents. Custom-agent settings can take precedence over requested settings; inspect applicable configuration if a mismatch appears. See [official subagent configuration](https://learn.chatgpt.com/docs/agent-configuration/subagents). Settings supplement explicit routing and do not prove what executed.

Do not change tier to match the parent or silently substitute another model. Luna always uses max. Sol high is reserved for its tier-3 question; Astra requires the tier-4 gate.

## Choosing a Model in Claude Code

Tier 1 and the tier-2 working default both use Sonnet 5.5 high. Opus is reserved for a specific tier-3 or tier-4 question. Do not introduce other models or efforts.

| Tier | Preset | Model | Effort | Selection within tier |
| --- | --- | --- | --- | --- |
| 1 | `sonnet-high` | Sonnet 5.5 | high | Determined work. |
| 2 | `sonnet-high` | Sonnet 5.5 | high | Working default. |
| 3 | `opus-medium` | Opus 5.5 | medium | Targeted reasoning difficulty with meaningful checks. |
| 3 | `opus-high` | Opus 5.5 | high | The same focused difficulty when an error has serious consequences or checks cover only part of the answer. State why medium is inadequate; consequences alone do not establish tier 3. |
| 4 | `opus-max` | Opus 5.5 | max | Only after the tier-4 gate. |

The four roleless presets in [.claude/agents/](../../.claude/agents/) request fixed model versions and efforts; the brief defines the task. Launch the selected agent type without per-call `model` or `effort` overrides. `CLAUDE_CODE_SUBAGENT_MODEL_FORCE` can override preset models, and `CLAUDE_CODE_EFFORT_LEVEL` can override preset effort. Inspect only relevant non-secret settings when diagnosing a conflict; do not dump the environment. Where supported, `/tasks` shows a running agent's model and effort. See [official Claude subagent documentation](https://code.claude.com/docs/en/sub-agents). Presets request settings; client reporting is execution evidence.

## Environment and Reporting

Check models, efforts, launch methods and applicable overrides at the first launch, then recheck on capability changes or mismatches. Do not silently substitute unavailable combinations. Continue useful work that does not require the unavailable executor and report the limitation; do not move a large assignment into an unsuitable orchestrator to bypass it. Other clients, including Gemini, use common principles where supported, without invented model equivalencies.

The [model-routing skill](../../.agents/skills/model-routing/SKILL.md) and its [Claude adapter](../../.claude/skills/model-routing/SKILL.md) are entrypoints to this policy. This template configures no custom routing service, hook, MCP server, telemetry, or automatic global settings. Add enforcement only for demonstrated recurring failures and a tested integration.

For substantial delegation, briefly report delegated results, requested model/effort and reason, verification, and any mismatch or unavailable execution evidence. Use reported time/usage only; do not infer savings from model names or confuse worker usage with total usage. Use existing task results and state documents instead of a parallel activity log. Test routing with [the evaluation procedure](AI_INFRASTRUCTURE_EVALUATION.md); static consistency cannot establish execution quality or quota savings.
