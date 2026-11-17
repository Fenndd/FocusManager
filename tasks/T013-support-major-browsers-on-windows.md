---
id: T013
title: Support major Windows browsers and block unsupported browsers
milestone: M2
status: planned
depends_on: ["T012"]
area: ["src/Windows/FocusManager.Platform.Windows/**", "src/Windows/FocusManager.Agent/**", "tests/Windows/FocusManager.Platform.Windows.Tests/**", "tests/Windows/FocusManager.Agent.Tests/**", "docs/spec/sites.md", "docs/spec/platforms/windows.md"]
---

# T013 — Support major Windows browsers and block unsupported browsers

## Goal

Apply site policies across supported installed Windows browsers and block known unsupported browsers during sessions that restrict sites.

## Sources

Legacy classification: type `change`; platform `windows`; priority `high`.

Requirement identifiers: `SITE-001`, `SITE-005`, `SITE-006`, `WIN-005`.

Legacy ADR associations: ADR-0007.

[Site requirements](../docs/spec/sites.md), [Windows requirements](../docs/spec/platforms/windows.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Site restrictions currently affect only Chrome through `src/Windows/FocusManager.Platform.Windows/ChromePolicyRegistry.cs`, at `HKCU\Software\Policies\Google\Chrome`. SITE-005 / decision I4 requires major browsers: Chromium-based browsers use their own policy registry locations; Firefox uses `WebsiteFilter`. Unsupported browsers are blocked completely during relevant sessions (SITE-006).

## Scope

In scope:
1. Define an internal supported-browser list with policy locations: Chrome, Edge, Brave, Vivaldi, Chromium (verify each registry path), and Firefox (`Software\Policies\Mozilla\Firefox`, `WebsiteFilter`). Store the list as data for easy extension.
2. Apply and clear policies for all installed supported browsers.
3. Maintain known browser executables; when a profile restricts sites, block unsupported browsers as applications, including Opera until T011 supplies a mechanism.
4. Test against a test registry section, following `ChromePolicyRegistryTests`.

Out of scope:
- `HKLM` policy for strict protection belongs to M7 protection levels.
- Opera filtering belongs to T011.

## Acceptance Criteria

1. Manual Windows checks confirm site restrictions in Chrome, Edge, Firefox, and Brave.
2. An unsupported browser cannot start during a session that restricts sites.
3. Windows statuses for SITE-005 / SITE-006 and WIN-005 are updated.

## Checks

- Run `dotnet test FocusManager.Windows.slnf` on Windows, including test-registry and unsupported-browser cases.
- Start a site-restricted session on Windows; verify Chrome, Edge, Firefox, and Brave restrictions, policy cleanup, and unsupported-browser blocking.
- Inspect browser data and requirement statuses.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
