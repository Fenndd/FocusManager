---
id: T008
title: Research Android blocking mechanisms
milestone: M3
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/platforms/android.md", "tasks/**", "ROADMAP.md"]
---

# T008 — Research Android blocking mechanisms

## Goal

Validate Android blocking hypotheses with prototypes and record implementable mechanisms, limitations, and the Android MVP tasks.

## Sources

Legacy classification: type `research`; platform `android`; priority `high`.

Requirement identifiers: `AND-001`, `AND-002`, `AND-003`, `AND-004`, `AND-005`, `AND-006`, `AND-007`, `APP-001`, `SITE-001`, `SITE-005`, `SITE-006`, `CNT-001`, `CNT-002`, `CNT-003`, `PROT-007`, `PROT-008`.

Legacy ADR associations: ADR-0005.

[Android requirements](../docs/spec/platforms/android.md), [application rules](../docs/spec/apps.md), [site rules](../docs/spec/sites.md), [content](../docs/spec/content.md), [protection](../docs/spec/protection.md), [triggers](../docs/spec/triggers.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

The stack is already Kotlin + Jetpack Compose, Room, and Firebase. Mechanisms in `docs/spec/platforms/android.md` are hypotheses that need prototype validation before main development.

[Owner decision K2 (2026-11-18)](../docs/spec/decisions.md#k2-android-protection-and-validation) governs the research direction and the real-device plus emulator evidence gate. It does not accept a mechanism or ADR.

## Scope

In scope:
1. Prototype application launch detection, blocking-screen behavior, essential system exceptions, service restart and power-management constraints on a documented API-26/current-version sample.
2. Check official store requirements for Accessibility and foreground/background operation relevant to this foundation.
3. Keep prototypes outside src/ and uncommitted, or in a separately authorized research branch. Record actual commands and evidence.
4. Record the foundational Android mechanisms ADR and identify remaining version/device gaps. T044 owns website methods; T030 owns advanced content/triggers/protection; T031 owns enforcement task decomposition.

Out of scope: production Android code (T009 and tasks created by T031), website/advanced prototype matrices moved to T044/T030.

Migration note: ADR-0008 is taken by infrastructure; allocate the next available ADR number.

Planning change (T026, 2026-11-17): Split the oversized research matrix into application/service foundation T008, website methods T044 and advanced capabilities T030.

## Acceptance Criteria

1. The foundational Android mechanisms ADR records evidence, permissions, restrictions and owner acceptance explicitly.
2. Only tested hypotheses are promoted; remaining research stays in T044/T030 or named child tasks.
3. T009/T031 prerequisites match the evidence; no research outcome silently removes a required feature.

## Checks

- Execute the application/service prototype sample on a real device and emulators (decision K2), and record actual commands, Android/device versions, permissions, lifecycle results and limits.
- Verify relevant official Android/Google Play requirements at research time.
- Inspect ADR acceptance and T009/T030/T031/T044 boundaries; no prototype command exists yet.

## Open Questions

No unanswered owner decision blocks foundational research under decision K2. Concrete essential/default package mappings and any special foreground-application exception remain unresolved in the owning specifications; do not assume their outcome in prototypes. Select and document the device/emulator coverage needed to validate the candidate mechanism; the research matrix below is historical guidance, not an accepted device list.

## Result

### Owner-decision addendum (2026-11-18, T057)

The owner accepted the direction in [decision K2](../docs/spec/decisions.md#k2-android-protection-and-validation), after the desk research below:

1. Keep a Google Play compliant self-control model and state the actual limits of Android strict protection. Do not promise prevention of disabling or uninstalling without validated, policy-compliant evidence.
2. Assess Accessibility for each concrete feature purpose, subject to policy, disclosure, consent and approval. Acceptance does not authorize blanket anti-disable or anti-uninstall behaviour.
3. A persistent foreground-service notification is acceptable.
4. Do not automatically copy desktop running-app snapshot semantics to Android. Use explicitly allowed applications and essential exceptions; a special foreground-application exception remains decision/prototype dependent.
5. Preserve functional phone operation and emergency access. Do not automatically exempt all Settings, Play Store or installer functionality; no exact default package list was accepted, and the existing AND-003 essential set remains.
6. Require real-device and emulator prototype evidence before accepting a mechanism or mechanisms ADR.

This answers the product direction behind the historical owner questions below. It does not accept the proposed UsageStats/Accessibility architecture, foreground-service type, SDK/API choices, exact device matrix, default package list, or any ADR. Mechanism-specific Google Play approval is still unverified. [Google Play's Accessibility policy](https://support.google.com/googleplay/android-developer/answer/16558241?hl=en) is the primary reference for the anti-disable/uninstall restriction; parental-control and enterprise exceptions do not establish permission for FocusManager's self-control use case.

Documentation checks for this addendum: **verified** preservation of T008's lifecycle state, acceptance criteria and prototype requirements; the AND-003 essential set; and the distinction between accepted owner direction and historical proposals. **Not verified:** device/emulator enforcement, lifecycle or store approval; no SDK installation, prototype, production implementation or .NET test was performed as part of this documentation update. The T057 delegated update used tier 2, GPT-6.1 Sol, medium effort.

### Historical desk-research record (before the owner answers, 2026-11-18)

**Status at the time of desk research:** read-only investigation. Android, Google Play and vendor documentation, secondary reports and repository files were read. **No prototype was run and no device or emulator was used.** The task stayed `ready` (lifecycle is owned by the main agent). The recommendations below were proposals, and no ADR or specification change had been made. The historical questions and unverified items are preserved as research evidence; the dated owner-decision addendum above supersedes their unanswered product-direction wording. Researcher tier/model/effort was not recorded in the journal result.

### Host environment note (2026-11-18)

`ANDROID_HOME` is set to `%LOCALAPPDATA%\Android\Sdk`, which does not exist; `adb` is not on PATH; Android Studio is installed at `C:\Program Files\Android\Android Studio`; JDK 21 (Eclipse Adoptium) is on PATH. No Android SDK, emulator or device was available, so no prototype command was executed and no latency, reboot, OEM or lifecycle behaviour was observed.

### Question researched

Which Android mechanisms (UsageStats, AccessibilityService, overlay, Device Admin/Owner, foreground service, foreground-app detection) can enforce app blocking on API 26 to current (Android 16 and 17), which Google Play Accessibility and usage-access policy risks apply, and which Compose/architecture choices fit. Product decisions are listed separately.

### Recommendation (PROPOSAL, not accepted)

A tiered, policy-aware design, with the owner deciding the Play-risk questions before [T009](T009-bootstrap-android-projects.md) and [T031](T031-plan-android-enforcement.md) depend on it:

1. Baseline (Play-safe): a foreground service plus `UsageStatsManager.queryEvents` polling (`ACTIVITY_RESUMED/PAUSED` on API 29+, `MOVE_TO_FOREGROUND/BACKGROUND` on API 26-28). On a match, show a blocking screen (a Compose Activity, or an overlay if `SYSTEM_ALERT_WINDOW` is granted) or send the user home. Google says to prefer narrower APIs than Accessibility.
2. Optional layer: an AccessibilityService for low-latency detection and later features. It is not an accessibility tool, so it needs the Play declaration, a separate in-app prominent disclosure with consent, a video, and deterministic rule-based behaviour only; treat approval as a release gate. Detect Android 17 Advanced Protection Mode (reportedly blocks non-tool accessibility services; secondhand) via `AdvancedProtectionManager` and fall back to the baseline.
3. Do not build PROT-007/PROT-008 on Accessibility or Device Admin as currently specified (see option D and the owner decisions). Keep them at research status under [T030](T030-research-android-advanced-capabilities.md).
4. Service: foreground service with `specialUse` type plus the Play Console declaration; manifest `<queries>` (MAIN/LAUNCHER and HOME) instead of `QUERY_ALL_PACKAGES`; `systemExempted` is not a workaround. For power management use `ACTION_IGNORE_BATTERY_OPTIMIZATION_SETTINGS` with OEM guidance; `ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS` only if Play accepts the use case (a focus blocker is not on Google's acceptable list).
5. Architecture: single Gradle application module (AND-001), minSdk 26, compile/target API 36 or higher (Play requires 36 from 2026-08-31), Compose with a current BOM, Room (ADR-0006), layering as packages. A new dependency set needs an ADR.

### Options compared

| Option | Key pros | Key cons |
| --- | --- | --- |
| A. UsageStats polling + foreground service + blocking Activity/overlay (baseline) | Narrowest permissions (user-granted Usage access); no Accessibility declaration; not affected by Advanced Protection Mode per secondary reports; documented events on API 26+ | Polling latency and a possible flash of the blocked app (latency undocumented); `queryEvents` returns null before first unlock (Android 11+), so no enforcement before first unlock after boot; events kept only "a few days"; cannot read screen content, URLs or sections; FGS and battery exemption depend on OEM behaviour; overlay permission is a second special grant; PROT-008 settings blocking not possible without Accessibility |
| B. AccessibilityService as primary detector (`TYPE_WINDOW_STATE_CHANGED`) + HOME/blocking screen | Lowest latency; only route to screen content for address-bar, keyword and section features (CNT-001/002, SITE-001); can detect the Settings screens PROT-008 wants | Highest policy and distribution risk (Play is the only channel, H1); non-tool apps need declaration, disclosure, consent, video; "monitoring" and "automation" apps named as non-qualifying for `isAccessibilityTool`; autonomous action prohibited; use against disable/uninstall prohibited unless parent/guardian/enterprise authorizes; Boomerang precedent of repeated Play removal; manual user enablement; Android 13+ "Restricted setting" gate for sideloaded copies; Android 17 Advanced Protection Mode reportedly blocks it; OEM battery managers often switch it off |
| C. Tiered hybrid (recommended for evaluation): A as baseline, B optional with graceful degradation | Follows Play's narrower-API guidance; stays functional if Accessibility is refused, OEM-disabled or blocked; no requirement silently removed | Two detection paths to build and test; Accessibility disclosure and declaration work still needed for those features; behaviour differs between paths and must be documented in AND-004's missing-permission UI |
| D. Device Admin / Device Owner for uninstall and settings protection (PROT-007/008) | Device Owner is the only mechanism that reliably prevents uninstall and settings tampering without Accessibility screen blocking; exempt from some FGS and BAL limits | Device Admin: consumer apps keep only force-lock and wipe, policies deprecated or throwing on newer target SDKs, removal protection dropped in Android 7 so the user can deactivate it; Device Owner needs a factory reset or unprovisioned device (adb path needs no accounts), not a realistic consumer Play flow; Play treats enterprise management and parental control as the authorized contexts; Play policy text on device admin itself not found |

All four are untested hypotheses (no prototype).

### Owner decisions needed (5, verbatim from the research)

1. Is a weaker Android strict level acceptable (PROT-004/007/008)? Options: (a) strict level only blocks ending the session and is advisory for uninstall and settings; (b) gate real protection behind an advanced, non-Play, Device Owner mode (which conflicts with H1 'Google Play only' and needs a new distribution decision); (c) pursue Accessibility-based settings blocking and accept the risk of Play rejection or removal. Research cannot make this call.
2. Accept the Play Accessibility declaration burden (disclosure, consent, video, ongoing resubmission) and the risk of rejection for a non-accessibility-tool app, or limit M3 to the UsageStats baseline and defer Accessibility features (address bar, sections, keywords, settings blocking) until the declaration outcome is known?
3. Is a persistent foreground-service notification (cannot be hidden from the shade on all versions) acceptable as the Android product behaviour, together with the Usage access, overlay (optional), notification and battery-exemption permission flow listed in AND-004?
4. Accept ADR-0010 only after the device prototype evidence exists, and state which devices/OEMs must be tested to satisfy the T008 acceptance (minimum matrix: API 26, API 29, API 36/37, one aggressive-OEM phone).
5. SM-003 Android snapshot semantics and APP-007 essential-app defaults (already flagged in T031) remain open; this research does not decide the default essential-app list, including whether Settings, Play Store and the installer are included.

(Item 4 says "ADR-0010" as the researcher wrote it; the ADR number is still to be allocated by the main agent.)

### Unverified (15, none softened)

1. Everything marked "prototype": no SDK or adb on the host, no device checked. No claim about latency, flash of the blocked app, reboot recovery or OEM service-kill behaviour is verified.
2. Exact wording and effective date of the Play bullet forbidding Accessibility use to prevent users disabling or uninstalling apps unless authorized by a parent/guardian (and enterprise administrators). It came from page fetches of answer/9888170 and answer/13986130 summarized by a tool; a human must re-read it before the owner decision. The fetched Accessibility page answer/10964491 did not contain it.
3. Whether Google would approve a self-imposed focus blocker (not parent/enterprise) for the Accessibility declaration; no listing of an existing focus app was found. Evidence is only the Boomerang precedent (TechCrunch 2020) and general policy text.
4. Android 17 Advanced Protection Mode blocking non-tool accessibility services: only secondary sources (Security Affairs, The Hacker News, Android Authority snippets, Beta 2 reports from March 2026). developer.android.com confirms AAPM and `AdvancedProtectionManager` but not the accessibility restriction, and shipping in the June 2026 stable build is not confirmed.
5. Restricted settings for sideloaded apps (Android 13+) and Enhanced Confirmation Mode (Android 15+) are from secondary sources; whether Play-installed apps are exempt and whether Usage access or overlay are also gated is unverified. GEN-006 says Play only, so this mainly affects test/CI installs.
6. Whether the system can launch a blocking Activity from an AccessibilityService without `SYSTEM_ALERT_WINDOW` (the BAL page lists "bound by a service that has been granted permission" but does not name accessibility services). Test on Android 14, 15, 16, 17.
7. Overlay (`TYPE_APPLICATION_OVERLAY`) behaviour over system screens: whether Settings and permission dialogs hide overlays (`HIDE_OVERLAY_WINDOWS` exists) and how that affects blocking; the `SYSTEM_ALERT_WINDOW` reference page was read only partly.
8. UsageStats event latency and the right polling interval; battery cost; whether events are delivered with the screen off. Docs give no latency numbers.
9. Whether FGS notifications can be swiped away on Android 13+ and what the FGS Task Manager "Stop" button does to the app and to a bound AccessibilityService (force-stop behaviour); those pages were not fetched. OEM reports (appblock.app, community threads) say Accessibility switches itself off on Xiaomi/Huawei/Oppo/Samsung under battery restrictions; anecdotal.
10. Whether Play reviews `specialUse` favorably for a focus app (no outcome examples), and whether a `specialUse` FGS can be restarted from `BOOT_COMPLETED` on Android 15+ (the service-types page lists restrictions only for dataSync, mediaPlayback, mediaProjection and phoneCall).
11. `DevicePolicyManager` method roles and API levels (`setUninstallBlocked`, `setUserControlDisabledPackages`, `setLockTaskPackages`, `setPackagesSuspended`, `DISALLOW_UNINSTALL_APPS`) were not read in the docs (page too large); the DPM facts come from model memory in a tool summary. Play's own policy on Device Admin apps was not found. The `adb dpm set-device-owner` conditions come from the AOSP testing guide as relayed by a search result.
12. Shares of API 26-29 devices (spec says "vast majority of devices"): no distribution data fetched.
13. The exact set of system exceptions (AND-003) and the APIs to resolve them (home, dialer, IME, systemui, emergency dialer): unverified API choices that need prototypes.
14. Whether Compose BOM 2026.09.00 is the latest stable and which AGP/Kotlin/Room versions pair with it (the BOM mapping page was truncated). Hilt versus Koin or manual DI was not researched; left to T009 under the ADR dependency rule.
15. Compose in an overlay or window created from a Service needs a `LifecycleOwner` and `SavedStateRegistryOwner` on the view root; sources are community threads, not official docs. Prefer an Activity for the blocking screen.

### Acceptance criteria and checks: NOT met

- Criterion 1 (foundational mechanisms ADR records evidence, permissions, restrictions and owner acceptance): **not met**. No ADR was written; no owner acceptance exists.
- Criterion 2 (only tested hypotheses promoted): **not met / not applicable yet**. Nothing was tested, so nothing may be promoted; every mechanism stays a hypothesis.
- Criterion 3 (T009/T031 prerequisites match the evidence; no outcome silently removes a required feature): **not met**. T009/T031 were not changed; PROT-007/008 are flagged but still required.
- Check "execute the application/service prototype sample, record commands, Android/device versions, permissions, lifecycle results": **not verified** (no SDK/adb; nothing run).
- Check "verify official Android/Google Play requirements at research time": **partly verified** by documentation fetches on 2026-11-18; several Play policy statements need a human re-read (unverified item 2).
- Check "inspect ADR acceptance and T009/T030/T031/T044 boundaries": **not verified**; no ADR exists.

### Follow-up work needed (prototype matrix)

Minimum matrix: API 26, API 29, API 36/37 (emulator or device) plus one aggressive-OEM device (Xiaomi, Samsung or OnePlus) if available. Prototypes stay outside `src/`, uncommitted or on a separately authorized research branch.

1. Install the Android SDK, create AVDs (API 26, 29, 36, 37) or attach devices, fix `ANDROID_HOME`; record exact adb/Gradle commands and device versions.
2. Matrix 1, UsageStats detection: poll interval versus latency, flash of the blocked app, screen off, after reboot before first unlock.
3. Matrix 2, blocking screen: Activity vs overlay vs `performGlobalAction(HOME)`; BAL results with and without `SYSTEM_ALERT_WINDOW`; Compose Activity cold start; essential-app exceptions (launcher, dialer, IME, systemui, emergency).
4. Matrix 3, `specialUse` FGS lifecycle: swipe from recents, FGS Task Manager stop, Doze (`adb shell dumpsys deviceidle force-idle`), App Standby (`am set-inactive`), reboot via `BOOT_COMPLETED`, battery-exemption flows.
5. Matrix 4 (optional layer): AccessibilityService on API 26-37, event delivery and survival after force-stop, restricted-settings gate on a sideloaded build, Advanced Protection Mode on an Android 17 device, `AdvancedProtectionManager` detection.
6. Human re-read of the Play policy pages (Accessibility API bullets, `QUERY_ALL_PACKAGES`, foreground service declaration, device administrator) and a Play Console dry run of the declarations before the owner decides on Accessibility and PROT-007/008.
7. After the owner decisions: write the ADR with tested/untested separation, update android.md and protection.md, adjust T009 and T031 prerequisites, and keep the T030/T044 boundaries.

### Deferred record edits (proposed only; not made, pending owner decisions and prototypes)

- New foundation ADR (status Proposed): UsageStats-in-FGS baseline, optional Accessibility layer with fallback, Compose blocking Activity with optional overlay, PROT-007/008 not promoted, `<queries>` instead of `QUERY_ALL_PACKAGES`, battery settings flow, API 36+ target / minSdk 26; evidence split into "documented (URL)" and "tested on device (command, version, device)". The researcher suggested 0010; the Linux and Opera researchers also suggested 0010, so the main agent must allocate the number.
- [`docs/spec/platforms/android.md`](../docs/spec/platforms/android.md): "Detecting application launches" limitations (Play declaration, disclosure, narrower APIs, path under research); "Protection against uninstalling and disabling" limitations plus a `> ❓ Question:` on a weaker Android strict level; AND-003 exception-set hypothesis note; AND-004/AND-007 notes (restricted settings gate, Advanced Protection Mode, battery-optimization action).
- [`docs/spec/protection.md`](../docs/spec/protection.md) PROT-007 platforms line: "planned" to "research" for Android (requirement not removed).
- [T009](T009-bootstrap-android-projects.md) scope (target SDK 36+, `specialUse` FGS with subtype property, Play Console declaration in the release checklist, `<queries>` placeholder), [T031](T031-plan-android-enforcement.md) scope (child tasks: UsageStats detector and blocking screen; optional Accessibility layer with disclosure/fallback; permission and battery onboarding; Play Console submissions; PROT-007/008 stays a gated research item), [T044](T044-research-android-site-filtering.md) and [T030](T030-research-android-advanced-capabilities.md) source notes (Play anti-disable rule, Advanced Protection Mode, VPN/`systemExempted` FGS; T044 owns `VpnService` details).
- This task: the host note and prototype matrix above (already recorded here).

### Sources (all consulted by the researcher)

Android and Play documentation:
- https://developer.android.com/guide/topics/ui/accessibility/service; https://developer.android.com/reference/android/accessibilityservice/AccessibilityServiceInfo; https://support.google.com/googleplay/android-developer/answer/10964491?hl=en; https://support.google.com/googleplay/android-developer/answer/16550159; https://support.google.com/googleplay/android-developer/answer/9888170; https://support.google.com/googleplay/android-developer/answer/13986130; https://support.google.com/googleplay/android-developer/answer/11926878; https://support.google.com/googleplay/android-developer/answer/13392821; https://developer.android.com/develop/background-work/services/fgs/service-types; https://developer.android.com/develop/background-work/services/fgs/restrictions-bg-start; https://developer.android.com/develop/background-work/services/fgs/changes; https://developer.android.com/guide/components/activities/background-starts; https://developer.android.com/reference/android/app/usage/UsageStatsManager; https://developer.android.com/reference/android/app/usage/UsageEvents.Event; https://developer.android.com/training/package-visibility/declaring; https://developer.android.com/training/monitoring-device-state/doze-standby; https://developer.android.com/topic/performance/power/power-details; https://developer.android.com/work/device-admin; https://developers.google.com/android/work/device-admin-deprecation; https://source.android.com/docs/devices/admin/provision; https://source.android.com/docs/devices/admin/testing-setup; https://developer.android.com/about/versions/17/features; https://developer.android.com/about/versions/17/behavior-changes-all; https://developer.android.com/about/versions/17/behavior-changes-17; https://developer.android.com/develop/ui/compose/bom/bom-mapping

Secondary and anecdotal sources:
- https://securityaffairs.com/189497/security/advanced-protection-mode-in-android-17-prevents-apps-from-misusing-accessibility-services.html; https://9to5google.com/2022/05/06/android-13-sideloaded-accessibility/; https://techcrunch.com/2020/08/04/parental-control-app-boomerang-repeatedly-blocked-from-play-store-losing-business; https://dontkillmyapp.com/; https://appblock.app/my-accessibility-keeps-turning-off-what-can-i-do-2/; https://www.techrepublic.com/article/news-android-17-pixel-rollout/

Cited in the researcher's evidence but not in its source list (read only partly):
- https://developer.android.com/reference/android/Manifest.permission (`HIDE_OVERLAY_WINDOWS`; the `SYSTEM_ALERT_WINDOW` reference was read only partly)

Repository files read: this task; [T009](T009-bootstrap-android-projects.md); [T030](T030-research-android-advanced-capabilities.md); [T031](T031-plan-android-enforcement.md); [T044](T044-research-android-site-filtering.md); [android.md](../docs/spec/platforms/android.md); [protection.md](../docs/spec/protection.md); [decisions.md](../docs/spec/decisions.md) (H1: Google Play only); [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md); [ADR README](../docs/architecture/decisions/README.md); [AI_ORCHESTRATION.md](../docs/engineering/AI_ORCHESTRATION.md).
