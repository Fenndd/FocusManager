---
name: model-routing
description: Choose models and bounded context handoffs for substantial subagent work in this repository, including bulk reading and repetitive file generation. Use when delegation is justified; keep small sequential tasks in the main session.
---

# Project Model Routing (Claude Code entrypoint)

The canonical skill is shared with Codex. Read [.agents/skills/model-routing/SKILL.md](../../../.agents/skills/model-routing/SKILL.md) and the shared [orchestration policy](../../../docs/engineering/AI_ORCHESTRATION.md), then follow them.

Claude Code adaptation: determine the tier with the policy's "Choosing a Tier" rule, then use the "Choosing a Model in Claude Code" table, not the Codex table. Only Sonnet and Opus are recommended; never use Fable. Launch the matching roleless preset from `.claude/agents/` (`sonnet-high`, `opus-medium`, `opus-high`, `opus-max`) as the agent type, without passing `model` or `effort`: the presets are the project's single mechanism for fixing both, and a per-call value would replace the preset's. State the tier and the instruction to read the policy in every brief. Session tool rules, including when subagents may be spawned, take precedence.
