---
id: T046
title: Update the shared AI agent infrastructure
milestone: M0
status: review
depends_on: []
area:
  - AGENTS.md
  - .agents/skills/model-routing/SKILL.md
  - .claude/skills/model-routing/SKILL.md
  - .claude/agents/*.md
  - .codex/config.toml
  - docs/engineering/AI_ORCHESTRATION.md
  - docs/engineering/DEVELOPMENT_WORKFLOW.md
  - docs/engineering/TASK_FORMAT.md
  - docs/engineering/AI_INFRASTRUCTURE_EVALUATION.md
  - docs/engineering/INFRASTRUCTURE_PROVENANCE.md
  - tasks/T046-update-shared-ai-agent-infrastructure.md
---

# T046 — Update the shared AI agent infrastructure

## Goal

Update shared agent infrastructure from source revision `5ab76bf2f5b726998b690b362a15c6e164f4ae85`, preserving FocusManager rules and existing work, and record bounded verification and provenance.

## Sources

- Explicit owner request to update this project's shared infrastructure on 2026-11-18.
- [Repository working rules](../AGENTS.md), including Task Execution.
- [Orchestration policy](../docs/engineering/AI_ORCHESTRATION.md).
- [Infrastructure provenance](../docs/engineering/INFRASTRUCTURE_PROVENANCE.md).

## Scope

In scope: shared rules/routing, four fixed Claude presets, generic workflow/task guidance and evaluation procedure, optional supported Codex executor defaults, provenance, and this maintenance task. The main integrator adds this task's M0 roadmap row and a separate dated status note.

Out of scope: product implementation, specifications, requirements, architecture decisions, active task results, acceptance of T025/T026 or other tasks, dependency/platform changes, account/global settings, deletion, commit and push.

## Acceptance Criteria

1. Shared infrastructure is merged from the stated source while preserving FocusManager-specific context, T026 execution protocol, local paths, unrelated configuration and existing dirty work.
2. Routing tables, model/effort presets, canonical skills/adapters, local links and optional executor defaults are statically consistent.
3. Provenance lists actual installed/verified paths, source identity, unavailable prior baseline, adaptations and real checks with client/runtime limitations.
4. T046 appears once in M0; the status note preserves existing active work, product baseline and next steps. Acceptance remains with the owner.

## Checks

- Compare original file hashes, staged/applied bytes and the final scoped diff against the infrastructure manifest; review preservation of existing modifications.
- Check destination-relative Markdown links and client includes; compare four canonical skill/adapter metadata pairs and four Claude presets against the fixed routing tables.
- Parse optional `.codex/config.toml`; confirm only `gpt-6.1-sol` / `medium` executor defaults were added.
- Inspect this task schema and ID uniqueness, the single M0 row, and the appended status note; run `git diff --check` on affected tracked paths after application.
- Product build/tests and client execution are not required by this instructions-only update; report them as unverified.

## Open Questions

None for the authorized infrastructure scope. Owner acceptance remains pending.

## Result

Updated and statically verified on 2026-11-18; awaiting owner acceptance. The update was prepared by a tier-2 GPT-6.1 Sol / medium executor; the main orchestrator owns application and final integration evidence.

Verified in staging: four canonical skill/adapter metadata pairs; four Claude presets and fixed Codex/Claude tables; 21 infrastructure Markdown links using the staged overlay and unchanged destination fallback; optional TOML containing only the two executor defaults; 38 original infrastructure baseline hashes; unchanged `.gitignore`; manifest content hashes and payload boundaries. Existing FocusManager context, T026 protocol, `docs/spec/` and architecture-decision paths were retained.

A real T046 maintenance task, one M0 row and a dated status note were recorded to honor the project's task-first rule. Existing product records are preserved; no product feature completion or other task acceptance is claimed.

Destination application, final scoped Git diff/whitespace checks and preservation audit passed; evidence is recorded in the provenance receipt. Client loading, effective model execution, routing behavior, quota savings and product runtime tests are not verified. No acceptance or permission to start dependent product work is inferred.
