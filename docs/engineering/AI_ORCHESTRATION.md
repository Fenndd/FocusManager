# Working with Subagents

## FocusManager Execution Protocol

This project adaptation was added in T026. [AGENTS.md](../../AGENTS.md) defines authorization and code boundaries;
[TASK_FORMAT.md](TASK_FORMAT.md) defines the lifecycle. This policy coordinates work and does not grant permission to implement unready product work.

### Start and Select Work

1. Inspect Git status and relevant diffs before editing. Preserve unfinished work; read STATUS.md and ROADMAP.md, then the selected task and its linked specification/ADRs. Do not load all project documents by default.
2. For a chat request, create its task first. Select a `ready` task with every dependency `done`; use roadmap order among eligible tasks. Research may collect evidence before its outcome is known, but may not implement an undecided outcome.
3. Check task size and environment. Split work before execution if it cannot produce one independently verifiable result in one session. Planning gates create child implementation tasks; their completion never marks a feature implemented.
4. Set the task, roadmap and STATUS.md to `in_progress` together. The main agent owns these shared coordination files. Worker results do not change lifecycle state independently.
5. Update the owning specification before behavior changes. Product questions stay next to the requirement as `> ❓ Question:`; ask the owner when the answer is needed. A proposal, research result or model preference is not an accepted requirement/ADR.

### Parallelism and Ownership

- Compare declared `area` patterns, real files and dependency ancestry before dispatch. Empty/unclear or overlapping areas mean sequential edits. Distinct task IDs do not imply independent files.
- In FocusManager, Core/rule changes (T001/T012), storage migration, session extraction, contracts, service lifecycle and UI integration often share files. Recheck actual conflicts instead of launching a worker per roadmap row.
- Android, Linux and payment research can gather evidence concurrently with PC work when useful, but often share ADR/specification/task indexes. Use read-only findings for those shared areas and let the main agent apply coordinated edits sequentially.
- Give a writer exclusive ownership of named paths for the delegated interval. If unexpected overlap appears, stop the conflicting edit and report it; never overwrite another worker's changes. Do not reset or clean a dirty checkout to obtain isolation.
- Keep dependent operations sequential. Contracts and accepted decisions precede their consumers; migration and service/IPC work require the risk tier selected below. Batch only independent reads or checks.

### Brief and Return Contract

A brief must name the task ID, selected tier/model/effort, policy-read requirement, goal, source paths and accepted decisions, exact editing boundaries or read-only mode, dependencies, acceptance checks and requested return format. Include relevant dirty-tree context. Never pass secrets or unrelated history.

Return: changed paths or findings with verified source locations; actual commands/manual checks with `verified`, `not verified` or `failed`; unresolved questions and blockers. Request compact evidence, not copied files. The main agent inspects original sources and the integrated diff before accepting findings.

### Verify, Review and Close

- Choose checks from the task and actual tooling. Documentation: content, links, task schema/dependencies/readiness, roadmap/status agreement and diff. Shared C#: `dotnet test FocusManager.Shared.slnf`. Windows changes: `dotnet test FocusManager.Windows.slnf` on Windows plus relevant manual checks. Follow new platform READMEs only after those commands exist.
- Report unavailable Linux/Windows/device/manual checks explicitly. A build is not proof of enforcement, session-0 behavior, reboot recovery, access isolation or live store lifecycle behavior. Do not silently dismiss a failure using a dated baseline.
- Use independent read-only review when it adds value for significant integration, persisted data, privileged IPC or protection changes. This does not mandate a worker for every small edit. The main agent resolves findings and reruns affected checks.
- Put the actual result/checks in the task and set it to `review`; only established owner acceptance permits `done`. Update roadmap and STATUS.md together. Do not start dependents while a prerequisite is in `review`.
- Missing decisions/access/data go to the owner or owning task, not a stronger model. For reasoning failures use the escalation rule below; stop repeated ineffective attempts. Record a started task as `blocked` only with a concrete reason and the next action.
- Before ending, leave STATUS.md with the next eligible task, pending acceptance, unanswered decisions and unverified checks. Do not infer T025 acceptance from a new preparation request. Commits, pushes, account changes, publication and deployment follow explicit authorization.

The model tables below are routing preferences, not enforced infrastructure. No scheduler, hook or external service is installed by this protocol.

## When to Delegate

Use a subagent for an independent part of a task, isolation of a large context, checking a separate hypothesis, or independent review if the benefit justifies the startup and coordination costs. Simple edits and tightly dependent sequential steps should usually be handled in the main session.

Define roles and tasks for the current task. Do not create fixed profiles such as `explorer`, `worker`, `reviewer`, or similar in advance. Choose the number of agents based on useful independent tasks and environment constraints, rather than trying to use every available slot.

## Choosing a Tier

Every delegated task gets exactly one tier, 1–4. The client tables below map each tier to a model and effort; this rule is the same for every client. Rate only the delegated task itself, not the overall project.

Check the tiers from 4 down to 2. The first tier with any matching condition is the task's tier; if none matches, the tier is 1.

| Tier | Choose it if any of these holds |
| --- | --- |
| 4 | An error would be costly, hard to detect, or hard to reverse: security, concurrency or ordering, data integrity or loss, changes to persisted data or its schema, irreversible operations. Or a tier-3 attempt at this task already failed and the cause was reasoning difficulty. |
| 3 | The work spans several components whose interaction must be reasoned about together, or a contract between them changes or has to be inferred. Or the cause or the right approach is unknown and has to be found by comparing hypotheses or options. Or sources contradict each other. Or the planned checks can confirm only part of the result. |
| 2 | The worker must make its own decisions — how to implement, where a defect is, what to test, whether code is correct — but within one bounded unit: the contract and expected behavior are given by the brief or existing code, the changes stay inside the assigned files without changing interfaces others use, and the checks named in the brief would catch a wrong result. |
| 1 | None of the above: the result is fully determined by the brief. Either a factual question answered by locating and reading sources, or a change that applies an existing reference pattern to new places. No choice between reasonable approaches is left, and correctness is checked mechanically against the reference, a search, or existing checks. |

Review and diagnosis are rated by applying the same conditions to the code or problem under examination.

Resolving doubt:

- These do not change the tier: the number of files, the size of input or output, the parent session's model, or the importance of the project as a whole. Large mechanical work stays tier 1.
- If you cannot tell whether a condition holds, the brief is usually missing something: a reference, a contract, or a check. Add it first. If doubt remains after that, choose the higher of the two tiers.
- A condition about contracts or interfaces applies only when the new contract or its effects must be worked out. A change that the brief fully specifies and that is applied everywhere by an existing reference, such as a mechanical rename including all callers, stays tier 1.
- Tier 1 requires an existing reference pattern; tier 2 requires a defined contract. If these are missing, define them in the main session before delegating instead of raising the tier.
- Raise the tier by one only after a failure caused by reasoning difficulty, not by missing requirements, data, or access. If tier 4 fails, return the problem to the main agent and the user instead of retrying.

## Choosing a Model in Codex

Assess reasoning difficulty, context volume, risk/reversibility, and expected output volume separately. A large input or output does not by itself require a stronger reasoning model. Delegate only when the expected benefit exceeds startup, context transfer, and verification costs; first consider targeted search and reading in the main session.

Determine the tier with [Choosing a Tier](#choosing-a-tier), then use this table. The combinations below are project routing preferences, not launch settings, quality guarantees, or verified pricing rankings. Check the available models and effort values in the current session. They concern development assistants, not any model the product itself may use.

| Tier | Model | Effort |
| --- | --- | --- |
| 1 | GPT-6 Luna (`gpt-6-luna`) | max (always) |
| 2 | GPT-6.1 Sol (`gpt-6.1-sol`) | medium |
| 3 | GPT-6.1 Sol (`gpt-6.1-sol`) | high |
| 4 | GPT-6 Astra (`gpt-6-astra`) | high |

In Codex (desktop app or CLI), the spawn call is the single mechanism: pass both `model` and `reasoning_effort` from the tier's row, with `fork_turns: "none"`, and never leave either to be inherited. A full-history fork (`"all"`) inherits the parent's settings and ignores overrides. Codex has no preset files, so the brief must carry the tier and the duties in [Subagent Duties](#subagent-duties); the task brief defines the role. The table guides subagents and does not change the main session's model or effort.

The tier rule already selects the smallest adequate combination; do not go below the selected tier for economy, and do not go above it without a condition from the rule, subject to the user's explicit preferences. For Luna, always use `max`; do not lower it to `high`, `medium`, or `low`, even for very elementary tasks or under the economy rule. Use actual pricing/usage evidence when comparing monetary costs; the table alone does not establish which combination is cheapest. Consider uncertainty, the consequences of error, the context needed, and the ability to verify. The number of files and the parent session's settings do not by themselves determine the choice.

The table guides subagent requests; it does not switch the main session's model. For models other than Luna, do not select `max` solely because a task involves large reads or mechanical work. Keep product decisions, resolution of ambiguous requirements, ownership of tightly coupled concurrency/security changes, and final integration with the main agent. A bounded independent review of such work can still be delegated to a suitable reasoning model.

If the result is uncertain or a check fails, first determine the cause: requirements, data, or access may be missing. A stronger model will not remove such obstacles on its own. Increase the model level when the analysis complexity is actually the limiting factor; do not repeat ineffective attempts indefinitely.

## Choosing a Model in Claude Code

Determine the tier with [Choosing a Tier](#choosing-a-tier); only the model/effort set differs from Codex. Use Sonnet and Opus for subagents. Do not recommend Haiku. Do not use Fable. Delegate only as the session's tool rules allow.

| Tier | Preset | Model | Effort |
| --- | --- | --- | --- |
| 1 | `sonnet-high` | Sonnet | high |
| 2 | `opus-medium` | Opus | medium |
| 3 | `opus-high` | Opus | high |
| 4 | `opus-max` | Opus | max |

Claude Code's subagent tool accepts per-call `model` and `effort` overrides, but a per-call effort is allowed only when the user or instructions explicitly request it, and ad hoc values would drift between sessions. This project therefore uses one mechanism: each tier is a roleless preset in [.claude/agents/](../../.claude/agents/) that fixes its model and effort. Launch the preset by its agent type and pass neither `model` nor `effort`; either would replace the preset's value. These presets fix only model and effort; the task brief defines the role, so they are not the fixed role profiles prohibited above. Sonnet `max` is permitted by the user but has no preset because Opus `medium` covers its expected use; add one only when a task shows the need. The table guides subagents and does not change the main session's model or effort.

## Environment Capabilities

Before launching, check which models, effort values, and context handoff methods are actually available. Set the selected parameters explicitly when the interface supports it. Do not promise that an override will be applied if the tool only inherits the parent session's settings.

If a combination is unavailable, choose an appropriate available alternative within the session's permissions and report the difference. For Claude Code, use its own table above. For Gemini, use the same task-based selection principle, without inventing model equivalencies with Codex or carrying over unsupported parameters.

This document does not enable or configure the subagent mechanism. It does not expand tool permissions, authorize external data sharing, or override active session limitations.

## Task Definition and Coordination

Give the subagent the goal, required sources, editing boundaries or read-only mode, result criteria, expected checks, its tier, and the instruction to read this policy before starting. Add the last two in every client, even when a preset already states them. Provide enough context while avoiding unnecessary copying of the entire history.

Use a short task-specific brief instead of a full-history fork when history is not necessary. Check the interface before selecting a model: in the current collaboration interface a full-history fork inherits the parent's settings and does not accept overrides. This interface-specific limitation must be rechecked when the tools change.

For parallel edits, assign different file areas. Perform dependent changes sequentially; the main agent integrates shared files. Use a separate checkout only when isolation is genuinely needed. Do not create multiple workers for the same implementation without a reason.

The main agent checks findings, integrates the result, and is responsible for the overall verification. A subagent's “done” message alone does not confirm that the task is complete.

## Subagent Duties

These apply to every subagent in every client (Claude Code, Codex, or another), whatever the model or preset.

1. Before any other action, read this policy in full.
2. Check the task against [Choosing a Tier](#choosing-a-tier). If a condition of a higher tier than your assigned one applies, or the brief lacks what your tier requires (a reference pattern for tier 1, a defined contract for tier 2), stop before making changes and report which condition applies and why. Do not continue at the wrong tier and do not fill the gap by guessing. A task that turns out to be simpler than its tier is completed as assigned.
3. Do only the task in the brief, within its read-only mode or editing boundaries. Do not make product decisions.
4. Follow [Data Handling](#data-handling) and the reading or writing mode below, and report in the form the brief and this policy require.

## Reading and Writing with Bounded Context

For context gathering, assign a concrete question and relevant paths in read-only mode. Ask for findings with file/symbol references, verified line locations where useful, contradictions, and remaining uncertainty. Do not return entire files or a generic repository summary. The main agent reads the necessary original sections before editing or accepting a significant conclusion; a summary may omit important conditions and line numbers must be checked.

For repetitive generation, provide an existing reference, the target paths, behavior criteria, and meaningful checks. Have the subagent write directly to the assigned files and return the changed paths, a short explanation, actual check results, and unresolved issues. The main agent inspects the diff and relevant implementation without requiring the full generated text in the chat. If a pattern does not exist, define it first rather than asking a worker to invent architecture. Do not create boilerplate or tests merely to exercise delegation.

These modes describe task shapes, not permanent agent roles or separate required skills. No file-size threshold forces delegation or blocks a necessary targeted read.

## Data Handling

Do not send secrets, credentials, personal data, or other confidential content to development assistants or external services. Use permitted synthetic fixtures, source code without secrets, and content-free measurements. Development routing does not change the product's own data-processing requirements.

## Scope and Evidence

The project-local [model-routing skill](../../.agents/skills/model-routing/SKILL.md) is a Codex entrypoint to this shared policy; [.claude/skills/model-routing](../../.claude/skills/model-routing/SKILL.md) is the Claude Code entrypoint to the same skill. Claude Code uses its own model table and the subagent presets in `.claude/agents/`; there is no one-to-one mapping between the Codex and Claude tables. Other clients use this document through the existing repository instructions; no global configuration is changed.

This is advisory routing through existing subagent tools. Apart from the Claude Code model/effort presets, no Portal service, custom router plugin, hook, MCP server, API dependency, or telemetry system is configured. Reconsider enforced routing only after actual repeated inefficient operations and a tested integration justify it. Codex supports [plugin hooks](https://developers.openai.com/plugins/build/plugins), but that does not establish an installed or trusted hook in this checkout.

The source idea is discussed in [Spotify's Shunt article](https://engineering.atspotify.com/2026/9/portal-by-spotify-cut-my-claude-code-token-usage-by-90): bounded reading and direct file generation reduce the primary agent's context. Its reported savings are specific to its benchmark, not a measured reduction in this project's total tokens, latency, or cost. A single shared policy and a small entrypoint follow [OpenAI's guidance on focused skills](https://developers.openai.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra).

## Transparency

In the final report on substantial delegation, state the task, selected model and effort, a brief reason for the choice, the result, and how it was checked. If the tool does not expose the actual execution parameters, say so; do not present requested parameters as confirmed.

Include elapsed time and token usage only when the tool actually reports them. Mark unavailable metrics as unavailable; do not estimate savings from model names or present worker-only token counts as total workflow usage. Use the existing task report and `STATUS.md` for significant results rather than creating a parallel activity log.
