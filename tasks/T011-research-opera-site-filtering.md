---
id: T011
title: Find a site-filtering mechanism for Opera
milestone: M2
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/sites.md", "docs/spec/platforms/*.md", "tasks/**", "ROADMAP.md"]
---

# T011 — Find a site-filtering mechanism for Opera

## Goal

Determine an Opera site-filtering mechanism for each main platform and plan implementation without implementing it in this research task.

## Sources

Legacy classification: type `research`; platform `all`; priority `medium`.

Requirement identifiers: `SITE-001`, `SITE-005`, `SITE-006`.

Legacy ADR associations: ADR-0007.

[Site requirements](../docs/spec/sites.md), [platform requirements](../docs/spec/platforms/), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Opera support is required by the owner decision of 2026-11-16 (SITE-005). The original context cites StatCounter October 2026 at approximately 3% desktop / 2% mobile share. Opera uses Chromium, but user/administrator reports say it ignores the enterprise `URLBlocklist` / `URLAllowlist` policies used for other desktop browsers. Until a mechanism exists, it is blocked as unsupported (SITE-006). Treat those reports as a research hypothesis, not a verified mechanism limitation.

## Scope

Planning clarification (T057, owner discussion on 2026-11-18): the owner asks whether a simple
Chrome-style mechanism can work in Opera. This is a question to verify, not acceptance of an extension
or a more expensive filtering system. Begin with a minimal Windows prototype of the existing
`URLBlocklist` / `URLAllowlist` policy approach on current Opera / Opera One and Opera GX; record
browser and Chromium versions, candidate policy locations, test rules, actual blocking results and
cleanup. Chromium ancestry alone proves neither policy support nor its absence. Forum claims do not
replace testing. If the simple approach fails on recorded versions, compare the extension and other
alternatives with measured evidence and owner review before selecting a mechanism. No publication is
authorized. The research task permits bounded prototypes, not production implementation.

Linux-specific research, prototypes and validation in the original scope below are deferred under
[decision J1](../docs/spec/decisions.md#j1-linux-development-hold); keep that historical scope and its
acceptance criteria intact. Deferred checks are unverified, and Windows evidence does not complete
the per-platform acceptance criteria.

In scope:
1. Test on Windows and Linux whether current Opera and Opera GX ignore Chromium registry / JSON policies.
2. Compare desktop alternatives, including a browser extension (installation, removal protection, Opera extension-store rules), system filtering (local DNS / proxy, WFP on Windows) for Opera only or all browsers, and address reading through OS accessibility (Windows UI Automation, Linux AT-SPI).
3. Test Android Opera address-bar Accessibility and VPN filtering; T008 investigates the general Android case.
4. Assess reliability, bypasses, load, and required rights.

Out of scope: production implementation, extension publication, external account/resource changes, or selecting an unapproved costly mechanism.

## Acceptance Criteria

1. A chosen method is documented for each platform, with an ADR for a new mechanism such as an extension or DNS filter.
2. Implementation tasks exist; SITE-005 and `docs/spec/platforms/*.md` are updated.

## Checks

- Test Opera / Opera GX policies and each candidate mechanism on the platforms above; record versions, permissions, results, bypasses, and load.
- Inspect the selected methods, any ADR, linked implementation tasks, and updated requirements. No prototype command exists yet; record the actual commands/manual steps used during research.

## Open Questions

None

## Result

### Current clarification — 2026-11-18 (T057)

The owner has not selected the earlier extension proposal. The next evidence step is the minimal
Windows Chrome-style policy prototype described above. No Opera installation, browser version,
policy prototype or extension was tested by T057; the task remains `ready` and the original desk
research below is retained as unverified historical evidence. Its proposed alternatives, blanket
capability statements and Linux follow-ups are not accepted conclusions or current execution
authority. In particular, extension compatibility does not establish Windows registry policy support.
The Linux hold overrides Linux follow-up steps until explicitly lifted. No mechanism or ADR is accepted.

### Preserved desk research

**Status of this record (2026-11-18):** desk research only. Documentation, vendor pages, forum posts and repository files were read. No Opera or Opera GX install was tested, no prototype was built, and no registry value, extension or network rule was applied. The task stays `ready` (lifecycle is owned by the main agent). Nothing below is accepted: the recommendation is a proposal, and no ADR or specification change has been made. Researcher tier/model/effort is not recorded in the journal result; the main agent adds it if needed.

### Question researched

Can site filtering work in Opera and Opera GX on Windows (and by extension Linux), and which mechanism fits the existing FocusManager agent best in reliability, bypass resistance, install friction and architecture fit?

### Finding on the policy hypothesis

The hypothesis "Opera ignores Chromium registry/JSON policies" is only partly supported and remains **unverified**. No official Opera document says Opera supports or ignores Chromium policies. Opera volunteer moderators say it "doesn't support policies at all", and users report that `opera://policy` is disabled. Chromium forks choose their own policy key at build time, so Chrome's `Software\Policies\Google\Chrome` key is very unlikely to affect Opera. A manual test is still needed before any ADR.

### Recommendation (PROPOSAL, not accepted)

Evaluate an Opera-specific Manifest V3 extension that enforces the profile rules in the browser, plus agent-side verification that fails closed: the agent keeps blocking `opera.exe` and the Opera GX executables as unsupported applications (SITE-006) whenever the extension cannot be confirmed active. The extension is the only candidate that sees real URLs (paths, subdomain matching) and works the same on Windows and Linux. Its weak points are install friction and removal protection (Opera has no force-install policy; the user can disable the extension, drop incognito or all-sites access, or use a private window). The fail-closed heartbeat is the mitigation, not tamper-proofing. Option B is the stronger but much heavier alternative. Option D is not recommended as the primary method. Linux and Android were not researched in depth (Android stays with [T044](T044-research-android-site-filtering.md) and [T008](T008-research-android-enforcement-and-ui.md)).

### Options compared

| Option | Key pros | Key cons |
| --- | --- | --- |
| A. Opera MV3 extension (Opera add-ons, optionally unlisted) with agent heartbeat and fail-closed fallback | Sees full URLs incl. paths; matches SITE-002 semantics; same code on Windows and Linux; `opera.exe` already blockable as an application; precedent: Cold Turkey ships an Opera extension | User must install it and grant incognito and all-sites access; no force-install, `storage.managed` unsupported; user can disable it, so protection depends on fail-closed detection with up to one heartbeat interval of latency; store review or developer-mode sideload; native-messaging path for Opera unverified; `declarativeNetRequest` not listed on Opera's API page; ADR-0009 rejected localhost TCP for UI IPC (no caller identity), so the channel is undecided |
| B. Local filtering proxy plus user-mode WFP rule restricting Opera to the proxy | Hard to bypass inside Opera if the WFP rule holds; no Opera-side install; can cover all browsers; fits the ADR-0009 service | Host-level only (no paths, a gap against SITE-001 and CNT-002); large new component and attack surface; conflicts with user proxy or VPN; system proxy is a per-user setting; breaks Opera's built-in VPN and DoH; user-mode WFP by application ID not verified for this use |
| C. DNS/hosts filtering, or WFP SNI inspection | Transparent, covers all browsers | Bypassed by Opera's DoH setting, built-in VPN and ECH; SNI inspection needs a signed kernel callout driver; coarse. Not recommended |
| D. UI Automation (Windows) / AT-SPI (Linux) address-bar reading | No Opera-side install; fits the session-helper model (ADR-0009 D3) | Reactive (page loads first); localized, undocumented Opera element names; possibly shortened URL; Chromium UIA provider "under development"; closing a tab needs synthetic input (inference); fragile. Not recommended as primary |
| E. DevTools remote-debugging port | URL-level control from outside the browser | User can launch Opera another way; open debugging port is an attack and bypass surface; support unverified. Not recommended |
| F. Status quo: block Opera as unsupported (SITE-006) | Zero new risk; already specified ([T013](T013-support-major-browsers-on-windows.md)) | Contradicts the owner's 2026-11-16 decision that Opera is supported (SITE-005); Opera is about 3% of desktop share (StatCounter, October 2026, as cited in the task) |

### Owner decisions needed (7, verbatim from the research)

1. Is an extension-based solution acceptable for Opera, given that the user must install it and grant incognito and all-sites access, and that it cannot be force-installed or fully tamper-proofed? The alternative is keeping Opera blocked (SITE-006).
2. If the extension is chosen, may FocusManager publish it to the Opera add-ons store (a public or unlisted listing and an Opera developer account), or should it be sideloaded in developer mode? Publication is an external action that needs the owner's authorization.
3. What protection level is expected for Opera in strict mode? Fail-closed blocking of Opera when the extension is missing or has incomplete permissions could mean Opera becomes unusable if the user declines to install it.
4. May the Opera extension send visited hostnames to the agent (privacy), and should they be kept off disk and out of cloud sync?
5. Is the heavier proxy plus WFP approach (option B) wanted as a later 'strict' tier for all browsers? It would also disable Opera's built-in VPN and DoH while a session is active.
6. Should Opera GX be treated as in scope under the same SITE-005 decision? It has not been separately tested here.
7. Android Opera scope remains with T044/T008; no decision from this research.

### Unverified (13, none softened)

1. Whether current Opera and Opera GX ignore Chromium registry and JSON policies. No install was tested; the only evidence is volunteer forum posts, which show no dates and may be stale. No official Opera enterprise documentation was found.
2. Which registry key or Linux directory, if any, current Opera reads for policies.
3. That Opera GX behaves like Opera One for policies, extensions and API support.
4. `declarativeNetRequest` and `nativeMessaging` support in current Opera: not listed on Opera's API page; AdGuard's repository (third-party, not directly fetched) lists Opera MV3 from version 107.
5. The native messaging host registry path for Opera on Windows: only 2017-2019 forum anecdotes that Opera reads the Chrome path.
6. Whether `--remote-debugging-port` works in current Opera.
7. The Opera address-bar UI Automation element name, and whether Opera's Chromium build enables accessibility on demand like Chrome.
8. Whether Opera's built-in VPN and DoH bypass a system proxy on desktop (VPN scope is from a vendor blog; desktop DoH details are from a third-party page).
9. Whether a user-mode WFP filter can block `opera.exe` outbound traffic from a LocalSystem service without a driver, including any elevation or signing requirement.
10. Whether Opera supports `--force-renderer-accessibility` or the UIA provider flag.
11. Opera add-ons store review turnaround, and whether an unlisted extension suits this use (Cold Turkey's extension is described as unlisted only in a forum reply).
12. Opera GX extension install problems with the new Chrome Web Store domain (forum posts 2023-2024): current state unverified.
13. Linux and Android behaviour: not researched in depth. T044 owns the Android address-bar and VPN matrix. Opera and Opera GX Windows install paths come from forum guidance only.

### Acceptance criteria and checks: NOT met

- Criterion 1 (a chosen method documented per platform, ADR for a new mechanism): **not met**. Only a proposal exists; the owner has not chosen, and no ADR was written.
- Criterion 2 (implementation tasks exist; SITE-005 and `docs/spec/platforms/*.md` updated): **not met**. No tasks were created and no spec was edited (deferred, see below).
- Check "test Opera / Opera GX policies and each candidate mechanism, record versions, permissions, results, bypasses, load": **not verified** (nothing was run; no Opera version was recorded). No prototype command exists, and none was run.
- Check "inspect selected methods, any ADR, linked tasks and updated requirements": **not verified**, because none of these exist yet.

### Follow-up work needed

1. Windows manual verification on current Opera One, Opera GX and Opera Developer: record the Opera and Chromium versions; test whether Chrome, Chromium and any Opera-specific policy keys under HKCU and HKLM have any effect; check `opera://policy`. Use read-only inspection plus test registry values that are removed afterwards.
2. Repeat on Linux for Chrome-style policy directories and any Opera-specific path.
3. Throwaway (unpublished) MV3 test extension: verify `declarativeNetRequest` and `webNavigation` blocking in Opera and Opera GX, the incognito and all-sites permission flows, the user's ability to disable or remove it, and a heartbeat to a local test endpoint.
4. Test native messaging registration for Opera on Windows (candidate keys, including the Chrome path).
5. Draft the Opera ADR from the options above once the manual tests are recorded, then decide the implementation tasks and update SITE-005, WIN-005 and the Linux spec.
6. Coordinate with T044 (Android Opera address-bar and VPN matrix) and T013 (Opera executables in the unsupported-browser data).

### Deferred record edits (proposed only; not made, pending owner decisions and prototypes)

- [`docs/spec/sites.md`](../docs/spec/sites.md) SITE-005: replace the "user reports... needs a separate filtering method" sentence with the evidence-qualified wording (no official statement; volunteer reports; not confirmed on a current install; candidate under evaluation is an Opera extension with agent-side verification; blocked until implemented per SITE-006).
- [`docs/spec/platforms/windows.md`](../docs/spec/platforms/windows.md) WIN-005: add that Opera and Opera GX are not covered by the browser policy keys and are blocked as applications until the chosen mechanism exists (install paths unverified).
- [`docs/spec/platforms/linux.md`](../docs/spec/platforms/linux.md) "Blocking websites" row: Opera policy locations unverified; same extension mechanism if accepted, otherwise SITE-006.
- New ADR for the Opera mechanism (status Proposed; candidates A-F, decision pending owner). The researcher suggested number 0010, and the Android and Linux researchers suggested 0010 too, so the main agent must allocate numbers; [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) is taken.
- New tasks (IDs to be assigned by the main agent): manual Opera/Opera GX policy verification on Windows and Linux; write the Opera ADR; prototype the MV3 extension with heartbeat and fail-closed enforcement; add Opera executables to the unsupported-browser block list (extends T013) and lift it once the prototype is accepted.
- T013: no change now; "including Opera until T011 supplies a mechanism" stays correct.

### Sources (all consulted by the researcher)

Repository files read: this task; [sites.md](../docs/spec/sites.md) (SITE-005, SITE-006); [windows.md](../docs/spec/platforms/windows.md), [linux.md](../docs/spec/platforms/linux.md), [android.md](../docs/spec/platforms/android.md) (website-blocking rows); `src/Windows/FocusManager.Platform.Windows/ChromePolicyRegistry.cs`, `src/Windows/FocusManager.Agent/Enforcement/SiteEnforcer.cs`, `src/Windows/FocusManager.Agent/Monitoring/ChromeMonitor.cs` (only current site mechanism: Chrome policy values in `HKCU\Software\Policies\Google\Chrome`; no UI Automation, DevTools, extension, proxy or WFP code found); [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (D3, site policies in the owner's hive); [T013](T013-support-major-browsers-on-windows.md); [decisions.md](../docs/spec/decisions.md) (I4, I5).

Opera policy support:
- https://forums.opera.com/post/9759 (Group Policy/ADMX request, no staff reply visible); https://forums.opera.com/post/398247 (user states policy page is disabled; Intune/ADMX unsupported); https://forums.opera.com/post/354887 (Opera GX group-policy request, about 2024-07, no verified staff answer); https://forums.opera.com/post/282190 (registry policy attempt, no visible answer); https://forums.opera.com/topic/75035/installing-and-configuring-opera-via-gpo (volunteer: "Opera doesn't support policies at all"); https://forums.opera.com/topic/48492/opera-intune-admx-and-policy-support (volunteer: no registry policy support, no bundled ADMX); https://forums.opera.com/post/337778 (no Opera DoH policy answer); https://gitlab.e.foundation/e/os/cromite/-/commit/a8aed07597294c06e28744898aa61d696cea10db (a Chromium fork changing the policy registry key); https://chromium.googlesource.com/chromium/src.git/+/main/components/policy/core/common/policy_paths.cc (policy paths are branding-dependent); https://groups.google.com/a/chromium.org/g/chromium-dev/c/a1BQAwnXCGE (branded builds and policy key generation); https://support.google.com/chrome/a/answer/7532015?hl=en and https://chromium.org/administrators/complex-policies-on-windows (Chrome force-install policy; not available to Opera)

Opera extensions and switches:
- https://help.opera.com/en/latest/customization/ (install Chrome Web Store extensions via "Add to Opera"); https://help.opera.com/en/extensions/apis/ (supported APIs and differences); https://help.opera.com/en/extensions/acceptance-criteria/ and https://help.opera.com/en/extensions/publishing-guidelines/ (store review rules); https://help.opera.com/en/extensions/testing/ (developer mode, Load Extension); https://blogs.opera.com/news/2025/09/mv2-extensions-opera/ (MV3-only store; MV2 kept "as long as technically reasonable"; 2025-09-10); https://getcoldturkey.com/support/extensions/opera (precedent: incognito and all-sites requirements); https://forums.opera.com/topic/4090/private-windows-and-extensions (extensions off in private mode by default; dated forum evidence); https://forums.opera.com/post/299522 (command-line switches discussion); https://forums.opera.com/post/160329 (native messaging anecdotes, 2017-2019); https://forums.opera.com/topic/34209/opera-gx-not-opening/26?lang=en-US (Opera GX install path guidance); https://github.com/AdguardTeam/AdguardBrowserExtension (third-party: Opera MV3 minimum version 107; not directly fetched)

Network and system filtering:
- https://help.opera.com/latest/advanced/ (Opera uses the OS proxy settings); https://blogs.opera.com/security/2026/05/opera-vpn-is-safe/ (free VPN covers Opera browsing only; vendor source); https://winaero.com/enable-dns-over-https-in-opera-doh (Opera DoH setting, off by default); https://learn.microsoft.com/en-us/windows/win32/fwp/application-layer-enforcement--ale- (ALE app identity); https://learn.microsoft.com/en-us/windows-hardware/drivers/network/porting-packet-processing-drivers-and-apps-to-wfp (user-mode vs callout driver); https://cdt.org/insights/encrypted-client-hello-closing-the-sni-metadata-gap/ (ECH; secondary source)

Accessibility / address-bar reading (anecdotal):
- https://chromium.googlesource.com/chromium/src/+/HEAD/docs/accessibility/overview.md and https://chromium.googlesource.com/chromium/src/+/HEAD/docs/accessibility/browser/uiautomation.md; https://www.macscripter.net/t/what-appears-in-opera-address-field-is-different-than-actual-url/70014 (macOS displayed-URL quirk); https://kiwix.lema.org/stackoverflow.com_en_all_2023-11/questions/3579649/get-url-from-browser-to-c-application (Opera/Firefox DDE route; unverified for current Opera; cited in the researcher's option D evidence, not in its source list)
- The researcher's source list also named "kiwix.lim.org" as "not used"; no URL was given and none was consulted.
