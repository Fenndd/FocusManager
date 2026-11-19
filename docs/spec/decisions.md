# Product decisions

A list of questions decided by the product owner. After an answer, the decision is recorded
in the **Decision** field (with a date), and the specification, ADRs, and roadmap are aligned with it.

**Priority:**
- 🔴 — needed now; further planning is blocked without it;
- 🟡 — before Android MVP development begins;
- 🟢 — can wait until later, before release.

**Status as of 2026-11-16:** all questions have been answered. New questions belong in specification requirements (`> ❓`).

**How to answer:** in chat, for example “A1: b, A2: as recommended, E1: a+b+c”, or “I agree with all recommendations
except …”. Answers can also be entered directly in this file.

“Recommendation” is the AI assistant's opinion. It is not a decision.

---

## A. Product and positioning

### A1. Target audience 🔴
- a) University and school students (self-control while studying)
- b) Anyone who wants to concentrate: studying, work, sleep
- c) Parents restricting children (parental control)

Recommendation: **a, expanding to b**. Studying is a clear niche and a differentiator from general blockers.
Parental control (c) is a different product: different store requirements and a different protection model (on iOS — `.child`).

Decision: **a + b** — university and school students, and anyone who wants to concentrate. (2026-10-02)

### A2. Primary concept 🔴
- a) Allowlist: only selected items are allowed during a session (as currently on Windows)
- b) Blocklist: selected items are blocked (as in Stay Focused)
- c) Both modes, with the allowlist as the primary mode

Recommendation: **c**. The allowlist is your differentiator; the blocklist is needed because users are accustomed to it.

Decision: **c** — both modes, with the allowlist as the primary mode. (2026-10-02)

### A3. Which differentiators to develop first (multiple choices allowed) 🟡
- a) Study sessions: subjects, rule sets per subject, exam mode
- b) Accountability partner: a friend or parent holds the unlock code and is notified on circumvention
- c) One session across all devices (phone + desktop)
- d) Different levels of difficulty for removing blocking
- e) Gentle friction instead of a hard prohibition (a pause, “why are you opening this?”)
- f) Gamification: daily streaks, achievements

Recommendation: **a + d for the MVP**, then b. Option c requires cloud — later.

Decision: **All options** (a–f) must eventually be in the application; their order follows the roadmap. (2026-10-02)

Clarification: direction b (accountability partner) is **deferred** — partner functionality is not currently planned. (2026-11-16)

### A4. Name and brand 🟢
“FocusManager” is a working name; the phrase is widely used.
Should a unique name be chosen before release (store and domain checks)?

Recommendation: **yes, before the first public release**. Use the working name for now.

Decision: Keep “FocusManager” as the working name for now. (2026-10-02)

### A5. UI languages at launch 🟡
Options: English; Russian; Slovak; Ukrainian; others.

Recommendation: **English + Russian** at launch. Build localization into the architecture from day one.

Decision: **English + Russian** at launch; in the longer term — all popular languages. (2026-10-02)

### A6. Publication regions 🟢
- a) Worldwide
- b) EU first, then worldwide

Recommendation: **a**. There is no reason to restrict regions for a free application.

Decision: **Worldwide except China.** (2026-10-02)

---

## B. Platforms and development order

### B1. Platform order 🔴
- a) Android → iOS → Windows (maintenance)
- b) Android and iOS in parallel
- c) Windows (finish) → Android → iOS

Recommendation: **a**. Android offers more capabilities, and you use it yourself. iOS second, after
checking its limitations with a prototype.

Decision: The primary platforms are **Android + Windows + Linux**. iOS follows after the primary platforms are finalized. (2026-10-02)

### B2. What to do with the Windows version now 🔴
- a) Freeze: bug fixes only until the mobile versions are released
- b) Develop in parallel

Recommendation: **a**.

Decision: **b** — Windows is developed in parallel. (2026-10-02)

### B3. macOS 🟢
- a) Not planned
- b) Eventually, after Windows

Recommendation: **a** for now.

Decision: **a** — macOS is not planned. (2026-10-02)

### B4. Prerequisites for iOS 🟡
Is a Mac available, and are you willing to pay Apple $99/year? Without a Mac — a cloud Mac (paid), or defer iOS.

Decision: **No** for now (neither a Mac nor an Apple account). (2026-10-02)

### B5. Identical functionality on all platforms? 🔴
- a) Identical: implement only what is possible everywhere (features limited by iOS capabilities)
- b) Shared core + platform-specific features (more on Android)

Recommendation: **b**. On iOS, no notifications, keywords, or strict mode — stated honestly in the description.

Decision: **Identical functionality on the primary platforms** (Android, Windows, Linux); desktop additionally has folder blocking. iOS comes later: implement what is possible and omit the rest. (2026-10-02)

### B6. Minimum OS versions 🟡
- Android: 8.0 (API 26) / 10 (API 29) / 12 (API 31)
- iOS: 16 (minimum for the Screen Time API for individual use) / 17

Recommendation: **Android 8.0, iOS 16**. They cover almost all devices.

Decision: Cover **the majority of current devices** (specific versions are in `platforms/*.md`). (2026-10-02)

---

## C. Technologies

### C1. Android version language 🔴
- a) Kotlin + Jetpack Compose
- b) C# (.NET for Android) — to share code with Windows

Recommendation: **a**. Blockers are written in Kotlin; there is little shared code with Windows (see ADR-0002 — to be revised).

Decision: **a** — Kotlin + Jetpack Compose. (2026-10-02)

### C2. iOS version language 🟡
Effectively no alternative: **Swift + SwiftUI** (Screen Time extensions are Swift only).

Decision: **Swift + SwiftUI.** (2026-10-02)

### C3. Desktop UI technology (Windows + Linux) 🔴
The agent's logic language remains C#/.NET. The UI is a separate process communicating with the agent through JSON,
so any UI stack is possible (owner: “the stack can be extended for the UI”).

Options (detailed discussion — in the 2026-11-16 chat):
- a) **Tauri 2 + web UI** (TypeScript + React): lightweight, official Firebase JS SDK, shared stack with the website
- b) **Electron + web UI**: the same, but heavier; identical Chromium on every OS
- c) **Compose Multiplatform** (Kotlin): shared UI with Android, but a JVM and no desktop Firebase SDK
- d) **Flutter** (Dart): attractive, but a new language and incomplete desktop Firebase support
- e) **Avalonia** (C#): the same language as the agent, but weaker for conversion-focused design and Firebase
- f) Keep WinUI 3 + a separate Linux UI — two UIs

Recommendation: **a**; fallback with the same UI code — **b**.

Decision: **a** — Tauri 2 + web UI. Framework — React + TypeScript (as recommended, see ADR-0007). (2026-11-16)

### C4. Shared code across platforms 🔴
- a) Do not share code; share the specification + test vectors (JSON), run by each platform
- b) Kotlin Multiplatform for Android + iOS
- c) Shared C# (current ADR-0002)

Recommendation: **a**. Revisit b only if duplication between Android and iOS starts causing real problems.

Decision: **a** — no shared code; share the specification and test vectors (JSON). (2026-10-02)

### C5. Repository 🟡
- a) One repository: `windows/`, `android/`, `ios/`, `docs/`
- b) A separate repository per platform

Recommendation: **a**. Keeping specifications and tasks together makes agents' work easier.

Decision: **a** — one repository. (2026-10-02)

### C6. Desktop storage 🔴
- a) Return to SQLite (local file, nothing to install)
- b) Keep PostgreSQL (the user must install a database server)

Recommendation: **a**. PostgreSQL on the user's computer is an insurmountable barrier for an ordinary person.

Decision: **Desktop — SQLite** (remove PostgreSQL), **Android — Room (SQLite)**, cloud — Firebase. (2026-11-16)

History: the owner proposed Firebase for mobile and asked about desktop. Clarification: Firebase is **cloud** (decision D2); local storage is needed on every device regardless. Recommendation: **desktop — SQLite** (remove PostgreSQL), **Android — Room (SQLite)**, synchronization — through Firebase.

---

## D. Cloud, accounts, data

### D1. Cloud in the first version 🔴
- a) No: all data on the device
- b) Yes: accounts and synchronization immediately

Recommendation: **a**. Blocking must work without internet access; cloud comes when synchronization is introduced.

Decision: **a** — the first version has no cloud; all data is on the device. (2026-10-02)

Superseded on 2026-11-18 by [K3](#k3-cloud-availability-and-cost-constraint): cloud is allowed from the
first version when needed, with zero/minimal expenses as the priority. The original entry is retained as history.

### D2. Cloud platform when needed 🟢
- a) Supabase (PostgreSQL; a free project sleeps after a week of inactivity)
- b) Firebase (does not sleep; better Android/iOS support)
- c) Decide later

Recommendation: **c**. Design the data model so synchronization can be added.

Decision: **b** — Firebase. (2026-10-02)

### D3. Synchronization and “one session across all devices” — a paid feature? 🟢
Recommendation: **yes, premium**. Requires cloud and costs money for the server.

Decision: Minimal basic functionality is free, **including synchronization**. Premium has a **3-day trial period**. (2026-10-02)

Clarification: the trial period is 7 days (see G1). (2026-11-16)

### D4. Collecting analytics and crash reports 🟡
- a) Collect nothing
- b) Crash reports only (Crashlytics / Sentry)
- c) Crashes + anonymous usage analytics with user consent

Recommendation: **b**; c only with explicit consent (GDPR). Trust is key for a blocker.

Decision: **Detailed technical analytics** + **user statistics** (viewing personal progress). (2026-10-02)

### D5. Exporting and importing settings to/from a file 🟢
What this means: saving profiles, lists, and settings to a file and loading them back — without an account or cloud.

Why it might be needed:
- transfer settings to another device on the same platform without an account;
- make a backup before reinstalling the OS or application;
- share a profile (for example, “Exam preparation”) with a friend or classmates.

Options:
- a) Unnecessary: free synchronization through an account is enough for transfer and backup (D3)
- b) Export/import all settings to/from a file
- c) b + sharing individual profiles (as a file or link) as templates

Recommendation: **c, with low priority** (after the MVP). Restriction: during a strict-level session,
import must not weaken protection (PROT-004).

**a** — no file export. Synchronization and settings backups are **only through an account**; data is stored on the backend. A local copy remains on the device for offline operation (D1, GEN-005). (2026-11-16)

---

## E. Blocking features

### E1. What to block in the Android MVP (multiple choices allowed) 🔴
- a) Applications
- b) Websites
- c) Notifications from blocked applications
- d) Keywords (in addresses, searches)
- e) Parts of applications (Shorts, Reels)

Recommendation: **a + b** in the MVP, then c and d. e is expensive to maintain: it breaks when these applications update.

Decision: **All of the above** (a–e). (2026-10-02)

### E2. How blocking starts (multiple choices allowed) 🔴
- a) Manually: a session for N minutes
- b) On a schedule
- c) Daily time limit per application
- d) Launch count limit
- e) By location or Wi-Fi

Recommendation: **a + b** in the MVP, c next. d — Android only; e — later.

Decision: **Many methods** (a–e); users choose what suits them. (2026-10-02)

### E3. Profiles (multiple rule sets: “Study”, “Sleep”, “Work”) 🟡
Recommendation: **yes, from the MVP**. Adding them late requires expensive data model changes.

Decision: **Yes.** (2026-10-02)

### E4. Website blocking on Android — method 🟡
- a) Reading supported browsers' address bars through accessibility services
- b) A local VPN with DNS filtering (occupies the VPN slot, conflicts with the user's regular VPN)
- c) Both, with a choice

Recommendation: **a** in the MVP; investigate b. Finalize after the prototype (task T008).

Decision: **c** — both methods. (2026-10-02)

### E5. Unsupported browsers 🟡
If a browser cannot be filtered, should it be blocked entirely during a session?

Recommendation: **yes** in allowlist mode (otherwise any other browser provides a bypass).

Decision: **Yes.** (2026-10-02)

### E6. What is always allowed 🟡
Calls and emergency calls — always. What else: SMS, alarm, maps, calculator?
Is the list user-defined or fixed?

Recommendation: **calls and emergency calls — always**; everything else — a configurable “always allowed” list with sensible defaults.

Decision: In allowlist mode, **basic essential applications** that are not distracting are allowed by default. (2026-10-02)

### E7. Empty allowlist 🟡
Currently, an empty application list blocks everything, while an empty website list restricts nothing.
- a) A session cannot start while the list is empty (warning)
- b) Empty = only “always allowed” items are permitted
- c) Leave it as is

Recommendation: **a**: explicit behavior is better than surprising behavior.

Decision: An empty list means **no restrictions**. (2026-10-02)

### E8. Blocking notifications 🟢
Currently disabled on Windows. Is a blocking screen sufficient, or are system notifications also needed?

Recommendation: **a blocking screen is sufficient**; notifications are an option in settings.

Decision: Blocking notifications exist and **can be disabled in settings**. (2026-10-02)

### E9. Usage statistics 🟢
- a) Not needed in the MVP
- b) Basic: number of sessions, number of blocks
- c) Detailed: time per application, charts (on iOS — system report only)

Recommendation: **b** in the MVP.

Decision: **c** — detailed statistics. (2026-10-02)

---

## F. Protection against circumvention

### F1. Protection levels 🔴
Proposal (from gentle to strict); levels may be removed or added:
1. **Normal** — blocking can be removed at any time
2. **With friction** — a pause of N seconds and/or entering a phrase before removal
3. **Password / partner** — removal requires a code (a friend may hold it)
4. **Strict** — blocking cannot be removed before the session ends; settings cannot be changed and the application cannot be uninstalled

Recommendation: **all four**. On iOS, level 4 comes with an honest caveat (circumvention through Settings is possible).

Decision: **All four levels**; the user chooses. (2026-11-16)

### F2. Emergency exit from strict mode 🔴
- a) None at all
- b) Limited: for example, once a week, with a delay of 10–15 minutes
- c) Through a partner

Recommendation: **b + c**. Without emergency exit, users uninstall the application and write bad reviews;
stores also dislike “locked” devices.

Decision: **b + c** — limited emergency exit and exit through a partner; the method and parameters are chosen and configured in advance. (2026-11-16)

Clarification: option c (through a partner) is deferred. Emergency exit is **available by default**; it can be disabled entirely — an option when choosing the strict level. (2026-11-16)

### F3. Android uninstall protection (device administrator privileges) 🟡
Recommendation: **yes, only in strict mode and with explicit consent**.

Decision: **Yes**, only in strict mode and with explicit consent. (2026-11-16)

### F4. Blocking Android Settings screens in strict mode 🟡
Which ones: accessibility settings, our application's page, date and time?

Recommendation: **yes, specifically those three**, rather than all of Settings (otherwise the user cannot, for example, connect to Wi-Fi).

Decision: **Targeted screens**, not all of Settings; the option can be enabled or disabled. (2026-11-16)

### F5. iOS: accepting that strict mode can be bypassed through Settings 🟡
- a) Yes, with an honest warning in the UI
- b) Yes, and notify a partner on circumvention (if technically possible)

Recommendation: **b**.

Decision: **b** — warn in the UI and notify a partner on circumvention. (2026-11-16)

Clarification: partner notifications are deferred together with partner functionality; the UI warning remains. (2026-11-16)

### F6. Changing device time as a bypass 🟢
Recommendation: **account for this**: session timers must use time that the user cannot adjust.

Decision: **Yes** — the session timer uses time that the user cannot adjust. (2026-11-16)

---

## G. Monetization

### G1. Model 🔴
- a) Completely free
- b) Freemium: basic features free, premium via subscription
- c) Freemium: subscription + a “lifetime” option (one-time purchase)
- d) Paid application

Recommendation: **c**. Many blocker users dislike subscriptions; a “lifetime” option addresses that objection.

Decision: Free tier + **a one-week trial that automatically renews** into a subscription. No “lifetime” option. Subscriptions: **1, 3, 6, 12 months**. (2026-11-16)

Clarification: the trial period is **7 days** (replaces the “3 days” in D3). (2026-11-16)

### G2. What is free and what is paid 🟡
Proposal:
- Free: application and website blocking, manual sessions, 1–2 profiles, protection levels 1–2
- Premium: unlimited schedules, all profiles, protection levels 3–4, statistics, (later) synchronization

Recommendation: **as proposed**; refine along the way.

Decision: Decide along the way; the free tier is **minimal**. Emphasis on the trial period, when the user fully tests the application and then subscribes. (2026-11-16)

### G3. Advertising 🟢
Recommendation: **none**. Advertising in a concentration application contradicts its purpose.

Decision: **No advertising.** (2026-11-16)

### G4. Purchases across platforms 🟢
- a) Each platform purchased separately
- b) One subscription for all platforms (requires accounts; RevenueCat is an option)

Recommendation: **a** at launch; **b** together with synchronization.

Decision: **One subscription grants access on all platforms.** (2026-11-16)

### G5. When to enable paid features 🟡
- a) From the first public release
- b) Free initially; premium after gaining users

Recommendation: **a**, but with a generous free version. Adding payment later is harder (users perceive it as something being “taken away”).

Decision: **From the first public release.** (2026-11-16)

> Related to G6: sales will most likely require a legal business structure. Owner's decision: the question is deferred
> until the first paid release. (2026-11-16)

### G6. Legal business structure for sales (Slovakia) 🟢
When should a sole trade (živnosť) or company be registered? In the EU, the seller's name
and contact information will be visible for a paid application in the store.

Recommendation: **before the first paid release**; clarify with an accountant.

Decision: **Do not register one yet.** (2026-11-16)

---

## H. Release and distribution

### H1. Android channels 🟢
- a) Google Play only
- b) Google Play + an APK on the product's own website

Recommendation: **a**. Account for the following: a new personal Google Play developer account must conduct
closed testing with at least 12 testers for 14 days before release.

Decision: **a** — Google Play only. (2026-11-16)

### H2. Windows channels 🟢
- a) The product's own website (installer) — requires a paid code signing certificate; otherwise SmartScreen scares users
- b) Microsoft Store — the store handles signing, but an agent with administrator privileges may not meet the requirements
- c) Both

Recommendation: **decide later**; after freezing Windows (B2), this is not urgent.

Decision: **c** — the product's own website and Microsoft Store. (2026-11-16)

### H3. Website / landing page and privacy policy 🟡
Stores require a privacy policy (especially when accessibility services are used).

Recommendation: **a simple static website** (free, for example GitHub Pages).

Decision: **A simple website.** (2026-11-16)

### H4. Target date for the first release 🟢
Is there a desired date or milestone (for example, “before the exam period”)?

Decision: **No target date.** (2026-11-16)

---

## I. Current Windows version (open questions from the specification)

### I1. Allowing an application by `.exe` filename alone (APP-003) 🟢
Currently, any `code.exe` in any folder is considered allowed.
Recommendation: **keep full-path matching only**.

Decision: **Yes** — full path only (APP-003 is removed). (2026-11-16)

### I2. The `C:\` root is always allowed (FLD-004) 🟢
Recommendation: **keep it if Explorer needs it for navigating back**; otherwise remove it. Requires verification.

Decision: **Keep** `C:\` always allowed. (2026-11-16)

### I3. Website matching (SITE-002) 🟢
Does `example.com` mean only that domain, or its subdomains too?
Recommendation: **with subdomains** — users expect this, and Chrome works this way.

Decision: **With subdomains.** (2026-11-16)

### I4. Browsers on Windows (SITE-005) 🟢
Currently only Chrome is restricted.
Recommendation: **add Edge** (the same policy mechanism), and block other browsers as applications in allowlist mode.

Decision: **All browsers with the same policy mechanism**; block the rest in allowlist mode. (2026-11-16)

Clarification: browsers fall into two groups — **Chromium-based** (a shared policy mechanism) and **Firefox** (its own policy mechanism, supported separately). A separate user-facing browser selection system is not needed. (2026-11-16)

Clarification: support **the most used browsers according to statistics**; all others are automatically blocked as unsupported. (2026-11-16)

Clarification: **Opera is also supported** (it does not read enterprise policies — it needs its own filtering method, task T011). (2026-11-16)

### I5. Protection on Windows (WIN-005, WIN-007) 🟢
Website policy in `HKCU` is easily removed; “Exit” in the tray disables the mode.
Recommendation: **in strict mode — remove “Exit”, put policies in `HKLM`, run the agent as a Windows service**.

Decision: **Yes** (in strict mode — no “Exit”, policies in `HKLM`, agent as a Windows service). Protection levels also apply on desktop; strict mode needs an option that makes exit more difficult. (2026-11-16)

Clarified/superseded on 2026-11-18 by [K1](#k1-windows-service-and-session-behavior): one service for all
levels; closing UI is separate from protected session exit; restrictions must not affect another Windows
session, so machine-wide HKLM policies that do so are no longer approved. Controlled-stop and crash cleanup
also follow K1 rather than leaving restrictions behind.

---

## J. Development scheduling

### J1. Linux development hold

Decision: **Stop Linux development and conserve the current Linux work until the owner explicitly decides to resume it.** Confirmed by the owner in chat on 2026-11-18; recorded by [T053](../../tasks/T053-record-review-and-conserve-linux-work.md).

This is a scheduling decision. B1 and B5 remain the historical platform and functionality targets; Linux is not cancelled. Preserve existing Linux research, specifications, task records and source placement, including their unverified findings and unanswered questions.

The hold covers Linux research and prototypes, implementation, environment/tool provisioning, platform tests, CI, packaging and other Linux-specific work, including Linux-only parts of mixed future tasks. A new explicit owner instruction is required before any of that work resumes; an agent must not infer resumption from dependency completion or a general request to continue the backlog. Future work described in preserved Linux records is deferred, not authorized.

Windows work and shared code work performed on Windows may continue under their own task gates. Keep the future Windows/Linux sharing architecture and platform boundaries intact. Tests of pure Unix-style path data on Windows are permitted shared-code checks; they do not verify Linux platform behavior or resume Linux implementation.

## K. Owner answers on service behavior, Android and cloud — 2026-11-18

Recorded by [T057](../../tasks/T057-record-owner-service-android-and-cloud-decisions.md) from the owner's explicit chat answers and follow-up clarification. These are accepted product decisions, not acceptance of implementation tasks or ADR-0009. Existing historical decisions remain above; the supersession below is intentional. Linux remains on hold under J1.

### K1. Windows service and session behavior

| ADR-0009 question | Confirmed owner decision |
| --- | --- |
| Q1 | One Windows service is used for all protection levels. Service installation/running alone does not activate blocking. |
| Q2 | Each Windows user has their own data, profiles/settings and sessions. FocusManager is not shared administration of other users. |
| Q3 | Switching to another account is an accepted protection boundary. An optional, user-enabled restriction against switching accounts is a separate capability requiring feasibility/design work; it is not currently implemented. |
| Q4 | Closing/hiding the UI leaves enforcement running. Ending a session follows its protection-level check. Service stopping is a separate lifecycle action, with the allowed-exit conditions enforced before a controlled stop. |
| Q5 | Whenever the service starts or recovers, active rules apply immediately to already-running forbidden applications as well as subsequent launches. Reboot/crash recovery must not capture a new blanket exemption snapshot. This overrides the previous Windows recovery behavior. Resolving an unidentifiable process owner remains a technical/design gap; do not terminate another user's processes speculatively. |
| Q6 | Enabled blocking notifications must be delivered even when the main UI is closed. |
| Q7 | The application starts at OS login by default; settings can disable this. Separately, automatically starting blocking when the application launches is configurable and disabled by default. An existing active session's recovery under Q5 is distinct from starting a new session through this toggle. No forced reopening of a deliberately closed UI was requested. |
| Q8 | No application/API panel to view, change or terminate another Windows user's data/session is provided, including for administrators. This does not promise resistance to an administrator using OS powers outside FocusManager. |
| Q9 and Q16(a) | Show truthful per-category enforcement state, and distinguish unavailable agent, incompatible version and authorization failure. Internal implementation identifiers need not become user-facing terminology. |
| Q10 | Helper termination is treated as a technical degraded state: relaunch/retry and show the affected capability's status. Do not invent punitive or intent-based behavior; any stronger protection response needs a separate owner decision. |
| Q11 | Preserve and migrate user data during updates. Current development databases are entirely test data and may be recreated/deleted if migration effort is disproportionate; this exception does not authorize discarding future real user data. No database was deleted by recording this decision. |
| Q12 | Exempt FocusManager's own verified installed components, rather than any executable merely matching a FocusManager filename. The technical verification belongs to the design/implementation. |
| Q13 | Offer an explicit choice to retain settings during permitted uninstall and a clear full-removal path. Applied restrictions/policies must be cleaned up even when settings are retained. No implicit default for that retention choice was selected. |
| Q14 | A higher Windows minimum for a Store/service package is acceptable in principle. Establish the actual packaging/install/update/uninstall result before selecting the version or changing the general minimum. |
| Q15 | Restrictions/policies must affect only the associated Windows OS session, not other sessions/users. Machine-wide strict HKLM policies are not acceptable when they break this boundary. This supersedes that part of I5. Per-account registry targeting alone is not proof of isolation between simultaneous sessions of the same account. |
| Q16(b) | Once the service stops, all ongoing FocusManager restrictions must be removed. A controlled stop first satisfies the allowed-exit/protection conditions; strict mode does not justify leaving restrictions after an allowed stop. In the follow-up the owner explicitly extended cleanup to crashes/forced termination, using a separate mechanism. Recovery of an active session then reapplies its rules under Q5. |

[T058](../../tasks/T058-reconcile-service-design-with-owner-decisions.md) owns reconciliation with the proposed ADR and service tasks, including an independent cleanup mechanism, cleanup/restart races, same-user multiple-session browser-policy limits, session-scoped stopping and current implementation discrepancies. The mechanism and its feasibility have not been accepted. Q1–Q16 should no longer be treated as a wholly unanswered list; residual subquestions and newer Q17/Q18 must be assessed separately.

The optional account-switch restriction is assigned to [T059](../../tasks/T059-research-optional-windows-account-switch-restriction.md) and PROT-012. Approval of the option does not establish that Windows exposes a mechanism satisfying the required isolation.

### K2. Android protection and validation

The owner accepted all six Android recommendations from the preceding chat response:

1. Use a policy-compliant Google Play model, disclose genuine strict-level limitations, and do not promise uninstall/disable resistance without technical and policy evidence.
2. Evaluate Accessibility per concrete purpose, with required disclosure/consent and approval gates; no blanket authorization for anti-disable behavior or deceptive accessibility-tool classification.
3. A persistent foreground-service notification is acceptable when needed for the selected working mechanism.
4. Do not automatically transfer desktop snapshot exemptions to Android background applications. Use explicit allowed/essential applications; any special foreground-at-start exception requires further prototype-backed definition.
5. Essential exceptions follow necessary phone functions (including emergency use); Settings, Play Store and the installer are not automatically exempted wholesale. The exact resolved package/default matrix remains implementation/research work within the existing essential-app requirements.
6. Mechanism acceptance needs emulator coverage plus actual physical-device evidence. No device/SDK run, prototype, permissions grant or Android ADR acceptance is established by these answers.

The application remains a self-control tool. Historical Android anti-disable/uninstall assumptions must be checked against these decisions in T008/T030/T031/T044; they are not an authorization to violate Play policy. Linux work remains on hold.

### K3. Cloud availability and cost constraint

Cloud may be used from the first version whenever required by the product; there is no longer an owner requirement to exclude cloud from the first version. This supersedes D1's earlier no-cloud scheduling choice. Firebase remains the selected cloud platform (D2), while local storage, offline blocking and optional accounts for the applicable free features remain in effect.

The owner's priority is zero expenses or minimal expenses. No exact monthly spending ceiling, payment-method attachment, paid plan upgrade or deployed external resource was authorized. Do not translate this into a guarantee that all required production services are free at any scale. [T060](../../tasks/T060-estimate-cloud-costs-before-enabling-paid-services.md) must estimate the minimum necessary services and realistic usage, compare no-billing/free-quota options, explain quota failure behavior and identify paid-activation decisions before account changes.

Later clarification L2 supersedes the previously unknown budget here: target EUR 0, accepted ceiling
EUR 5/month, and Blaze acceptable if required. No external plan/billing/resource change is performed by
recording this direction, and no technical hard spending cap is implied.

The previous conflict recorded in SYNC about a mandatory cloud-free first version is resolved by this clarification.
Subsequent L2/L3 answers below select sign-in methods, remove shared cross-device session control and set the
cloud budget; use those later decisions rather than treating this historical list of open matters as current.

### K4. Opera mechanism clarification requested

The owner asked whether the same mechanism as Chrome can work and whether a more labor-intensive solution is actually necessary. This is a request to verify the simplest viable mechanism, not acceptance of an extension, proxy/WFP approach, browsing-data transfer, publication or new developer accounts. T011 must first verify current Opera/Opera GX policy behavior in a bounded Windows prototype, record versions and observations, and only then recommend alternatives if needed. No Linux prototype is permitted under J1.

## L. Owner profile, offline, billing and privacy answers — 2026-11-18

Recorded by [T061](../../tasks/T061-record-profile-offline-billing-and-default-decisions.md) from the owner's explicit answers 1–15. These product decisions supersede conflicting historical text; they do not accept implementation tasks or authorize external actions. Linux remains on hold under J1, and concurrent review-closeout work is preserved.

### L1. Profiles, default exceptions and protection

- Restrictions from all active profiles combine. A permission from one profile does not cancel another profile's restriction; an empty category imposes no restriction. Ending one profile removes only its contribution. There are no predefined or seeded profiles, including no automatic Study profile: users create/configure their own.
- The owner explicitly delegated selection of a minimal Android/Windows default exception set. All ordinary default exceptions are removable on both platforms and appear in a dedicated, clearly named view. In the follow-up the owner confirmed a separate protected technical group for critical OS components; these are displayed separately and are not removable ordinary exceptions.
- Strict emergency exit defaults to one use per day with a five-minute delay. The existing ability to configure protection in advance remains. Day/reset/timezone/quota-sharing details must be defined in the protected timer design; changing the system clock must not provide an extra exit.
- Friction uses the existing pause and/or phrase; additional tasks are not needed now. In the follow-up the owner confirmed that exit waiting progresses only while the FocusManager waiting screen is visible. Merely leaving the device display on is insufficient. Hiding/closing/minimizing that screen, locking the device or switching the display off pauses the wait; returning resumes the accumulated wait rather than counting hidden time.
- On application startup, restore the last-used profile selection by default; users may specify the selection explicitly. This selects what to use when automatic blocking is enabled; it does not override K1's default-off new-blocking toggle or active local-session recovery.
- The optional account-switch restriction is a separate setting. Emergency exit removes it with all other relevant restrictions. This does not waive Windows session isolation or prove a supported mechanism.

The concrete exception set is recorded below by T061. The owner confirmed both system-component and waiting-screen clarifications in chat on 2026-11-18. These are product requirements, not evidence that platform integration already works.

### L2. Account synchronization, offline operation and cloud cost

- Sign-in uses Google and email, as proposed and accepted. Apple sign-in was not added by this answer; future iOS work remains deferred.
- Synchronization conflicts use the latest change. Technical ordering, timestamp/tie handling and deletion representation must implement that visible rule without assuming trustworthy client clock ordering or losing the distinction between edits and account identity.
- The owner clarified that a shared account was intended, not a shared live blocking session. Starting/ending blocking on a PC must not automatically start/end blocking on a phone or another device. This explicitly removes the prior SYNC-004/common-session direction; account data, profile/settings/history synchronization and shared entitlements are distinct from live session control.
- All local client functions must work offline. Blocking, profile editing, local schedules/timers and local data/history must not require a live cloud request. Initial registration/sign-in, synchronization and new store/payment transactions inherently require connectivity; their temporary unavailability must not disable local functions. Do not introduce an unapproved remote entitlement check into every blocking action. Detailed future paid entitlement expiry/offline rules remain part of the owner-held commercial work.
- Firebase is selected and Blaze is acceptable if needed. Target cloud cost is EUR 0; EUR 5 per month is acceptable, based on the euro-denominated budget example being answered. T060 must estimate and design within that operating budget and explain actual control limits. This is not a command to enable billing/deploy or a claim that a billing alert imposes a hard cap. Costs above the agreed budget require a new owner decision.

### L3. Billing channels and owner-controlled commercial decisions

- Mobile purchases use Google Play on Android and App Store on iOS. Desktop purchases use Stripe. iOS remains deferred; this answer does not start iOS development.
- Exact prices, free/paid feature selection, limits and remaining trial-eligibility/commercial choices are deferred until the owner explicitly requests that work. Prepare configurable, trusted billing/entitlement infrastructure boundaries so those choices can be supplied later; do not invent a tariff matrix or turn unfinished choices into hardcoded product rules.
- Existing accepted subscription durations/trial duration remain historical product inputs unless separately changed; no new price, tier or trial-eligibility limit is selected here. The paid-public-release target remains a later release gate, not permission to decide commercial details now.
- Existing Microsoft Store distribution intent is not silently cancelled, but neither is Microsoft Store billing assumed. T024 must verify the chosen Stripe/payment/distribution combination and return any actual incompatibility to the owner rather than substituting providers or channels.

### L4. Registration consent and deferred content collection

- Collection of popular blocked applications/sites for template analytics is deferred. No such collection or research implementation is authorized by this handoff.
- Consent is requested during registration. Existing explicit analytics-consent and withdrawal requirements remain; registration is not evidence of affirmative consent. Record separate purposes for crash reports and usage analytics. Users who have not registered must not be treated as having granted consent; local blocking remains available offline/account-free under SYNC-001/GEN-005.
- General keyword analysis of visible screen/page text is deferred and is not currently considered. Implement the accepted keyword scope using addresses and search queries. This does not by itself cancel the separately specified application/website section feature; any access used for that feature still follows K2's per-purpose policy/prototype gates.

### L5. Blocked-notification viewing

Provide a dedicated Blocked notifications tab to view notifications hidden by FocusManager, including while a blocking session is active; do not restrict viewing to an end-of-session summary. Viewing the record does not exempt a blocked application or a link from current rules. The view and its required local data work offline; no cloud upload of notification content is authorized. Platform feasibility, available fields, retention/deletion controls and sensitive-content handling must be defined in the owning bounded tasks before implementation.

### L6. Default set selected under the owner's delegation

The dedicated view is **Default Exceptions** (Russian label: **«Исключения по умолчанию»**).
It lists every default permission, with its function/reason, resolved installed identity and removability,
including visible technical/non-app defaults rather than hidden bypasses. This is a product default chosen
by the agent at the owner's explicit request, not a claim that an OS vendor mandates these applications.

| Platform | Ordinary removable defaults |
| --- | --- |
| Android | Contacts, the default SMS Messages application, Clock (alarms/timers), Calculator |
| Windows | File Explorer, Clock, Calculator |

Use the installed app fulfilling the function rather than assuming vendor package names or installing a
missing utility. Do not duplicate Contacts if it is integrated into Phone. No automatic browser, maps,
mail/general messenger, social/media, Settings/store/installer, command-shell or Task Manager permission
is selected. Users may add their own profile rules. Ordinary defaults supplement allowlists; they do not
override an explicit block contributed by another profile or folder/site rules.

Phone/emergency functions, input, launcher/System UI and critical Windows components must be represented
transparently in a separate **Protected System Components** group (Russian label: **«Защищённые компоненты системы»**),
as confirmed by the owner. These technical components are not removable ordinary exceptions; FocusManager
does not target them for termination or disruptive blocking. Preserve functional/emergency access under
APP-005/AND-003 rather than silently allowing every system package/directory. Existing Windows root-folder rule C:\ without subfolders
(FLD-004/I2) is retained and shown as a non-app default. No broad Windows/WindowsApps-directory exemption
is approved merely to implement these ordinary defaults. Current enforcement still needs reconciliation
and tests before a removable UI entry can be claimed to change real blocking.
