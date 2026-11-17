---
id: T015
title: Create a product website with a privacy policy
milestone: M12
status: planned
depends_on: ["T039", "T041"]
area: []
---

# T015 — Create a product website with a privacy policy

## Goal

Publish a simple product website with download information and a stable privacy-policy URL after the hosting and publication decisions are resolved.

## Sources

Legacy classification: type `chore`; platform `all`; priority `low`.

Requirement identifiers: `GEN-007`, `TEL-005`, `GEN-006`.

[Product requirements](../docs/spec/product.md), [telemetry](../docs/spec/telemetry.md), [synchronization](../docs/spec/sync.md), and [owner decisions](../docs/spec/decisions.md).

Decision H3 requires a simple website. Stores require a privacy policy, particularly for Android Accessibility usage; the website is also a desktop download channel (decision H2).

## Scope

In scope:
1. Build a static website, for example on GitHub Pages, using the desktop UI web stack if convenient.
2. Provide product description, Windows/Linux downloads, privacy policy based on `telemetry.md` and `sync.md`, and contact information, in English and Russian.
3. Decide and record where the source belongs, for example a repository `site/` folder. The `area` remains empty until the location is agreed; run this task sequentially.
4. Publish to the agreed destination after the required owner decisions/authorization.

Out of scope: website payments (T024 research and T040 implementation children).

Planning change (T026, 2026-11-17): Privacy-policy and download publication must follow the actual reviewed data flows and release/channel plan; hosting/contact approvals remain unresolved.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M12 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. The website is published and the privacy policy has a stable public URL.

## Checks

- Use the build/check commands documented when the site is created; no site commands currently exist.
- Review both language versions, platform download links, contact information, and policy consistency with telemetry/synchronization requirements.
- After authorized publication, open the public website and permanent privacy-policy URL.

## Open Questions

Which source location and hosting/publication destination should be used, and who supplies/approves the public contact information and privacy policy? Resolve these owner decisions before publication.

## Result

Not started. No implementation or checks have been completed for this task.
