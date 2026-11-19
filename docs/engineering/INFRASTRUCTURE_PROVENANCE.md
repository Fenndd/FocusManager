# Infrastructure Provenance

- Preparation date: 2026-11-18
- State: applied; post-application static checks passed. Client loading and execution remain unverified.
- Source: canonical AI Development Infrastructure source template (AgentsInfrastructure).
- Source baseline: `5ab76bf2f5b726998b690b362a15c6e164f4ae85`; source worktree clean when inspected.
- Prior installed baseline: unavailable; no prior receipt found. Current destination was compared directly with the requested source; no historical baseline reconstructed.
- Destination HEAD before application: `3035e03de6b3c25545bd6b252bfd1f5ab622aed9`.
- Destination state: dirty, as verified by the parent orchestrator; existing instructions and product changes must be preserved.

## Installed or Merged Paths

This inventory records installed, merged, and verified unchanged files.

| Source path | Destination path | Outcome |
| --- | --- | --- |
| `templates/AGENTS.md` | `AGENTS.md` | Merged / updated |
| `CLAUDE.md` | `CLAUDE.md` | Unchanged; verified against destination |
| `GEMINI.md` | `GEMINI.md` | Unchanged; verified against destination |
| `.editorconfig` | `.editorconfig` | Unchanged; verified against destination |
| `.gitattributes` | `.gitattributes` | Unchanged; verified against destination |
| `.gitignore` | `.gitignore` | Unchanged; verified against destination |
| `.agents/skills/model-routing/SKILL.md` | `.agents/skills/model-routing/SKILL.md` | Merged / updated |
| `.agents/skills/security-best-practices/LICENSE.txt` | `.agents/skills/security-best-practices/LICENSE.txt` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/SKILL.md` | `.agents/skills/security-best-practices/SKILL.md` | Unchanged; verified against destination |
| `.agents/skills/security-threat-model/LICENSE.txt` | `.agents/skills/security-threat-model/LICENSE.txt` | Unchanged; verified against destination |
| `.agents/skills/security-threat-model/SKILL.md` | `.agents/skills/security-threat-model/SKILL.md` | Unchanged; verified against destination |
| `.agents/skills/session-teacher/SKILL.md` | `.agents/skills/session-teacher/SKILL.md` | Unchanged; verified against destination |
| `.agents/skills/session-teacher/references/original-gist.md` | `.agents/skills/session-teacher/references/original-gist.md` | Unchanged; verified against destination |
| `.agents/skills/security-threat-model/agents/openai.yaml` | `.agents/skills/security-threat-model/agents/openai.yaml` | Unchanged; verified against destination |
| `.agents/skills/security-threat-model/references/prompt-template.md` | `.agents/skills/security-threat-model/references/prompt-template.md` | Unchanged; verified against destination |
| `.agents/skills/security-threat-model/references/security-controls-and-assets.md` | `.agents/skills/security-threat-model/references/security-controls-and-assets.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/agents/openai.yaml` | `.agents/skills/security-best-practices/agents/openai.yaml` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/golang-general-backend-security.md` | `.agents/skills/security-best-practices/references/golang-general-backend-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-express-web-server-security.md` | `.agents/skills/security-best-practices/references/javascript-express-web-server-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-general-web-frontend-security.md` | `.agents/skills/security-best-practices/references/javascript-general-web-frontend-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-jquery-web-frontend-security.md` | `.agents/skills/security-best-practices/references/javascript-jquery-web-frontend-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-typescript-nextjs-web-server-security.md` | `.agents/skills/security-best-practices/references/javascript-typescript-nextjs-web-server-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-typescript-react-web-frontend-security.md` | `.agents/skills/security-best-practices/references/javascript-typescript-react-web-frontend-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/javascript-typescript-vue-web-frontend-security.md` | `.agents/skills/security-best-practices/references/javascript-typescript-vue-web-frontend-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/python-django-web-server-security.md` | `.agents/skills/security-best-practices/references/python-django-web-server-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/python-fastapi-web-server-security.md` | `.agents/skills/security-best-practices/references/python-fastapi-web-server-security.md` | Unchanged; verified against destination |
| `.agents/skills/security-best-practices/references/python-flask-web-server-security.md` | `.agents/skills/security-best-practices/references/python-flask-web-server-security.md` | Unchanged; verified against destination |
| `.claude/agents/opus-high.md` | `.claude/agents/opus-high.md` | Merged / updated |
| `.claude/agents/opus-max.md` | `.claude/agents/opus-max.md` | Merged / updated |
| `.claude/agents/opus-medium.md` | `.claude/agents/opus-medium.md` | Merged / updated |
| `.claude/agents/sonnet-high.md` | `.claude/agents/sonnet-high.md` | Merged / updated |
| `.claude/skills/model-routing/SKILL.md` | `.claude/skills/model-routing/SKILL.md` | Merged / updated |
| `.claude/skills/security-best-practices/SKILL.md` | `.claude/skills/security-best-practices/SKILL.md` | Unchanged; verified against destination |
| `.claude/skills/security-threat-model/SKILL.md` | `.claude/skills/security-threat-model/SKILL.md` | Unchanged; verified against destination |
| `.claude/skills/session-teacher/SKILL.md` | `.claude/skills/session-teacher/SKILL.md` | Unchanged; verified against destination |
| `docs/engineering/AI_ORCHESTRATION.md` | `docs/engineering/AI_ORCHESTRATION.md` | Merged / updated |
| `docs/engineering/DEVELOPMENT_WORKFLOW.md` | `docs/engineering/DEVELOPMENT_WORKFLOW.md` | Merged / updated |
| `docs/engineering/TASK_FORMAT.md` | `docs/engineering/TASK_FORMAT.md` | Merged / updated |
| `docs/engineering/AI_INFRASTRUCTURE_EVALUATION.md` | `docs/engineering/AI_INFRASTRUCTURE_EVALUATION.md` | Added |
| `presets/codex-config.toml` | `.codex/config.toml` | Added |
| Transfer-generated receipt | `docs/engineering/INFRASTRUCTURE_PROVENANCE.md` | Created provenance receipt |

## Deliberate Adaptations and Omissions

- Preserved FocusManager Context and all product rules from the existing modified AGENTS.md; aligned dependency wording with explicit scoped continuation authority.
- Preserved the T026 FocusManager Execution Protocol, project checks, ownership boundaries, product decision authority and task lifecycle. Updated only conflicting routing and acceptance/continuation references.
- Retained docs/spec/ and docs/architecture/decisions/ in DEVELOPMENT_WORKFLOW.md.
- Preserved .gitignore byte-for-byte: existing rules cover every shared ignore while retaining local rules and exceptions, including ignored .codex/.
- No prior provenance receipt or installed source baseline available; compared source and destination directly. No deletion or directory replacement; unrelated files remain untouched.
- Staged optional .codex/config.toml because no destination config exists; parent verified current-client support. Executor defaults only, no main-model, permission, limit or global changes.
- No optional payload omitted. The existing `.gitignore` ignores `.codex/`; this local policy is preserved, so config presence does not imply Git tracking.
- No retirement candidates identified among inspected infrastructure files. No existing file was deleted.
- Product source, tasks, roadmap, status, requirements, architecture decisions, data, credentials and unrelated configuration are excluded from this payload. Root AGENTS.md retains its existing project guidance.

## Verification

- Static staged overlay checked: four canonical skills and four adapters have matching metadata and resolved targets; four fixed Claude presets and Codex/Claude tables match the source checker invariants.
- Local Markdown links checked from staged destination-relative positions with unchanged destination fallback: 21 links resolved; no broken targets.
- Optional destination TOML parsed successfully and contains only `[agents]` defaults `gpt-6.1-sol` / `medium`; parent confirmed current-client schema support. No destination config existed to conflict.
- All 38 existing payload baseline hashes rechecked unchanged during preparation; `.gitignore` retained byte-for-byte. Changed-file hashes and payload boundaries checked; reviewable staged diff generated.
- Git status is unavailable to the worker sandbox. Parent supplied HEAD and dirty-state evidence; exact status must be audited before application.
- Product build/tests: not run; instructions/process/configuration preparation only.
- Client loading, effective model execution, routing behavior and usage savings: not verified. Static checks and staged files do not establish these.


## Related Project Bookkeeping

- Explicitly authorized FocusManager bookkeeping exception to the generic payload: added real maintenance task T046, one M0 roadmap row and a separate dated STATUS note to honor the existing task-first rule. Existing product records and acceptance states remain preserved.
- `tasks/T046-update-shared-ai-agent-infrastructure.md`: prepared maintenance task in `review`; T046 was unclaimed when checked.
- `ROADMAP.md`: exactly one M0 row added; prior content retained.
- `STATUS.md`: one dated maintenance note appended; prior bytes retained.
- Staged bookkeeping checks: ID uniqueness, task schema, one M0 row, unchanged product roadmap rows and original status prefix verified. Final application/whitespace evidence remains with the integrator.

## Applied Verification

- Applied on 2026-11-18 from source revision `5ab76bf2f5b726998b690b362a15c6e164f4ae85`.
- Prepared content hashes and original destination hashes matched before writing.
- Installed file bytes, local payload links/client includes, fixed model/effort tables, adapters, presets, and optional TOML defaults verified after writing.
- Git diff whitespace check for affected tracked infrastructure files passed.
- Existing unrelated modified/untracked file contents preserved (29 non-secret paths checked by hash).
- Client instruction loading, effective model execution, and quota savings: not verified.
