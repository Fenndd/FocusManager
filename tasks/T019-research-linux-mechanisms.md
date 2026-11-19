---
id: T019
title: Research Linux blocking mechanisms
milestone: M4
status: planned
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/platforms/linux.md", "tasks/**", "ROADMAP.md"]
---

# T019 — Research Linux blocking mechanisms

## Goal

Validate Linux enforcement, service, and packaging mechanisms, then turn platform hypotheses into requirements and implementation tasks.

## Sources

Legacy classification: type `research`; platform `linux`; priority `medium`.

Requirement identifiers: `LNX-001`, `LNX-002`, `LNX-003`, `APP-001`, `SITE-005`, `FLD-001`, `CNT-003`, `GEN-006`.

Legacy ADR associations: ADR-0007.

[Linux requirements](../docs/spec/platforms/linux.md), [application rules](../docs/spec/apps.md), [site rules](../docs/spec/sites.md), [folders](../docs/spec/folders.md), [content](../docs/spec/content.md), [product requirements](../docs/spec/product.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Linux is a main platform with Windows-equivalent functionality, including folder blocking (owner decisions B1, B5). `docs/spec/platforms/linux.md` is a draft containing hypotheses.

## Scope

In scope:
1. Compare process detection/termination and application identities including sandboxed packaging on a bounded Ubuntu LTS sample; identify untested distro/kernel gaps.
2. Prototype supported-browser policies and root systemd/user-restricted IPC as foundation mechanisms.
3. Record evidence and a proposed Linux mechanisms ADR. Obtain owner acceptance for supported distribution/environment decisions before dependent implementation; do not replace hypotheses with unsupported requirements.
4. Update T020/T042 using the selected mechanisms and create child research tasks for remaining core matrices. T043 owns folder, notification and packaging research; broader deployment verification is planned through T041.

Out of scope: production Linux code, folder/notification/package prototypes moved to T043.

Planning change (T026, 2026-11-17): Split broad Linux matrices into core feasibility T019 and integration/packaging research T043; production host and enforcement are T020/T042.

Scheduling hold (T053, 2026-11-18): [Owner decision J1](../docs/spec/decisions.md#j1-linux-development-hold) conserves this `planned` task. Do not continue Linux research or prototypes, implementation, environment/tool setup, platform tests, CI or packaging until the owner explicitly decides to resume Linux work and the task's other gates are met. Dependency completion or a general request to continue the backlog does not lift the hold. Existing scope, sources, findings and future Windows/Linux sharing remain preserved; Linux-only portions of mixed future tasks are also deferred. T019 changes from `ready` to `planned` because this owner scheduling gate is unresolved; the task schema has no paused status. The future-work proposals preserved in the Result below are deferred, not instructions to execute or create further Linux work during the hold. Requirements and acceptance criteria are unchanged.

## Acceptance Criteria

1. A Linux foundation mechanisms ADR records tested versions, access boundaries, application/browser outcomes, limitations and explicit acceptance status.
2. T020/T042 have implementation-ready boundaries based on accepted findings; unresolved distribution decisions gate dependent work.
3. T043 retains folder/notification/package work and named tasks cover remaining research; untested hypotheses are not marked verified.

## Checks

- Execute the bounded process/browser/IPC sample, recording OS/kernel/browser versions, permissions and limitations.
- Review ADR acceptance, owner distribution decision, updated T020/T042 scopes and T043 handoff. Record actual prototype commands; none exist yet.

## Open Questions

When will the owner explicitly decide to resume Linux work? This scheduling gate remains unresolved independently of the listed dependencies; an agent may not resolve it on the owner's behalf.

## Result

**Status of this record (2026-11-18):** desk research only (read-only; kernel, systemd, vendor documentation and repository files). **No prototype was run; no OS, kernel, browser, systemd or snapd version was tested**, so every behaviour claim about a running Linux system is documentation-derived. The task was `ready` when the desk research was recorded; T053 subsequently changed it to `planned` under the owner hold above. Nothing below is accepted: the recommendation is a proposal and options for the owner to choose from; no mechanisms ADR or research-derived specification change was made. Researcher tier/model/effort is not recorded in the journal result; the main agent adds it if needed.

### Question researched

Which Linux mechanisms can enforce application and website blocking across desktop environments (process monitoring via `/proc` and the proc connector, fanotify, eBPF, D-Bus, X11 vs Wayland window detection, systemd service plus Unix socket, hosts/DNS/proxy/browser-extension/policy site blocking, kill vs deny), and what supported-environment matrix options follow? Options only; the owner chooses.

### Recommendation (PROPOSAL, not accepted)

1. APP-001 is launch blocking, so it needs process or exec events, not active-window detection. Wayland window detection is fragmented (GNOME and KWin implement neither wlr-foreign-toplevel-management nor ext-foreign-toplevel-list; GNOME's `Introspect.GetWindows` is restricted), and Ubuntu 26.04, Fedora 43 GNOME and Plasma 6.8 have dropped their X11 sessions. Proposed foundation: a compositor-independent process layer, a root C# agent listening to the proc connector (`cn_proc`), reconciling with `/proc`, resolving identity from `/proc/<pid>/exe` plus cgroup/scope names and `/.flatpak-info`, killing by cgroup via pidfd or `cgroup.kill`. Prototype fanotify `FAN_OPEN_EXEC_PERM` (deny before the program runs) as the preventive layer, with kill-after-launch as the fallback. Defer eBPF LSM (Ubuntu's `CONFIG_LSM` lists no `bpf`, per the September 2026 Ubuntu kernel-team thread).
2. Sites: vendor enterprise policies as the primary mechanism, as on Windows (Chromium-family JSON in per-browser managed directories; Firefox `policies.json` WebsiteFilter), with DoH locked off by policy. hosts/DNS/proxy only as a supplement (`/etc/hosts` has no subdomain wildcards; browsers can bypass system DNS via DoH). Snap/Flatpak browsers unsupported until their policy paths are proven, which means SITE-006 blocks them.
3. IPC: a root systemd unit plus a pathname `AF_UNIX` socket in a root-owned directory (`SocketMode`/`SocketUser`/`SocketGroup`), authenticated with `SO_PEERCRED`.
4. Matrix: narrowest defensible first target is Ubuntu 26.04 LTS (GNOME 50, Wayland-only, kernel 7.0) with native deb Chrome/Chromium-family/Firefox; other cells only after the prototype matrix passes. Every cell stays "research" until prototype evidence exists.

### Options compared

| Option | Key pros | Key cons |
| --- | --- | --- |
| A. `cn_proc` listener + `/proc` reconcile + kill (baseline) | Session-independent: works on every DE, X11 and Wayland; no special kernel config; notification-only, so an agent bug cannot block exec system-wide; matches the existing "terminate the process" hypothesis | Reactive (window may flash before the kill); queue overflow can lose events (hence `/proc` reconcile); agent must run in the init user and PID namespaces or a hardened/containerized unit silently receives nothing; hand-written netlink P/Invoke in .NET (unverified); killing one PID can miss helpers unless the cgroup is killed |
| B. fanotify `FAN_OPEN_EXEC_PERM` (deny before execution), A as fallback | True prevention (caller gets EPERM); same API family is the folder-blocking candidate (FLD-001, T043); documented stable kernel ABI | Needs `CAP_SYS_ADMIN`; agent sits in the exec critical path, so a stalled agent stalls every exec (needs a hard deadline and watchdog; the man page documents no timeout); direct exec only, not interpreter-read scripts; marks are per mount/filesystem (new mounts need new marks); marks on zero-fsid filesystems such as FUSE fail with ENODEV since 6.8; no events for network filesystems; identity at exec time may be incomplete (unverified) |
| C. eBPF LSM / tracepoint + `bpf_send_signal` | In-kernel decision, no user-space stall risk; can deny at exec | Hard kernel-config dependency: Ubuntu builds `CONFIG_BPF_LSM` but does not list `bpf` in `CONFIG_LSM` (inactive by default; a September 2026 patch was NACKed or unresolved, outcome unverified); `lsm=bpf` boot parameter is intrusive; needs a BPF toolchain and C or Rust beside the C# agent (conflicts with ADR-0007's stack; needs its own ADR). Recommend deferral |
| D. Window / active-app detection (X11 EWMH, Wayland compositor-specific) | Only route to per-window behaviour (FLD-005 navigation, CNT-002 section matching in a native GUI) | Not generically available on Wayland (Mutter and KWin lack both toplevel protocols; `ext-foreign-toplevel-list` has no "activated" state; GNOME `Introspect.GetWindows` allow-listed; KWin needs `loadScript` over D-Bus in a running session); per-DE helper in the user session, easy to stop; X11 sessions are disappearing. Use only for later per-DE features (T043), not APP-001 |
| App identity (APP-002): exe path; systemd app-scope `ApplicationID`; Flatpak ID from `/.flatpak-info`; Snap name from the cgroup scope; AppImage via `$APPIMAGE` | Combining exe path with the packaging-specific ID gives one stable ID per format; cgroup identity also names the whole tree to kill | systemd app-scope convention is work-in-progress and covers only DE-launched apps; Snap exe paths include revisions (unverified); Flatpak `/.flatpak-info` readability via `/proc/<pid>/root` is not in the man page; Snap `SNAP_APP_NAME` needs snapd 2.77+ (unverified); APP-006 must map `.desktop` entries to IDs |
| W1. Browser enterprise policies (primary; mirrors Windows SITE-005) | Vendor-supported, root-owned, works on Wayland and X11, no network interception; Chromium filter format already matches SITE-002; test vectors reusable from Windows | Browser restart needed; per-browser path table; Snap/Flatpak browsers read policy elsewhere (Firefox snap auto-connects `/etc/firefox/policies`; Chromium snap path has conflicting reports, bug 1714244; Flatpak Brave only via `host-etc`); WebsiteFilter subdomain/subresource behaviour undocumented; Opera on Linux unverified; unmanaged browsers need SITE-006 blocking by process |
| W2. hosts file / local DNS / proxy (supplement) | Browser-independent for apps using system DNS (curl, Electron, unsupported browsers) | `/etc/hosts` has no wildcard/subdomain semantics, so SITE-002 is not met; browser DoH bypasses system DNS unless locked off; user can change DNS or use a VPN; a proxy or TLS interception needs a root CA (heavy privacy/security decision outside this task's authority); unsuited to path rules (CNT-002). Not recommended as primary |
| W3. Browser extension (force-installed, native messaging) | Can support path/section rules (CNT-002) and per-tab behaviour | Needs store publication (review lead time); force-install depends on the same policy mechanism, so it does not help where W1 paths fail; Opera and unsupported browsers not covered; extra privileged channel to design. Defer |
| IPC/service: root systemd unit + `AF_UNIX` socket (LNX-002) | Standard and documented; `Restart=always` plus StartLimit handles crashes; UI can reconnect after restarts (matches ADR-0007) | Pathname socket permissions must not be relied on for security, so credential checks are mandatory; multi-user access needs a model decision; a user with sudo can still stop the service or edit policy files (protection design, T034); `PassPIDFD` needs systemd 258 (LTS availability unverified) |
| Kill vs deny (decision input) | Combination covers new launches and already-running processes and degrades gracefully if fanotify is unavailable | Deny may surface as a launcher error that looks like a crash (EPERM) so the UI needs a notification path; kill may flash the window; neither stops root, processes outside the cgroup, or renamed binaries if matching is by path only |
| M1. Narrow first target: Ubuntu 26.04 LTS, deb, GNOME Wayland | Smallest test surface; current platform facts; no X11 branch | Excludes Fedora/KDE and Ubuntu 24.04 users; Ubuntu's default Chromium and Firefox are snaps (Firefox snap policy support exists, Chromium snap doubtful), so value is limited unless the snap-browser matrix passes |
| M2. Ubuntu + Fedora/Debian, GNOME and KDE on Wayland | Matches the initial LNX-001 proposal; A, B and W1 are DE-independent, so added cost is mostly packaging and path tables | Two package formats; more browser path variants; fanotify features gated by kernel version (PIDFD reporting 5.15, `FAN_MARK_IGNORE` 6.0, zero-fsid ENODEV 6.8, `FAN_DENY_ERRNO` 6.13), so a minimum kernel decision is needed; SELinux on Fedora may interfere (unverified) |
| M3. Broad: M2 + X11 sessions, Snap/Flatpak/AppImage browsers, other DEs | Largest coverage; closest to "identical functionality" (B5) | Needs per-DE code and many untested policy paths; high maintenance; cannot be claimed supported without a large prototype matrix |

### Owner decisions needed (11, verbatim from the research)

1. LNX-001 / H2: which distributions, versions and package formats (deb, rpm, Flatpak, AppImage) are supported for the first Linux release; choose among matrix options M1, M2, M3 or a different list. Include whether Ubuntu 24.04 LTS (still has an X11 session) is in scope.
2. Display server scope: Wayland-only, or also X11 sessions. This decides whether any window-based feature (Mechanism D) is built at all.
3. Minimum supported kernel version (fanotify and cgroup features are version gated, e.g. FAN_OPEN_EXEC_PERM 5.0, PIDFD reporting 5.15, zero-fsid ENODEV on mount/filesystem marks 6.8).
4. Application identity on Linux: is the executable path (as on Windows) enough, or must Snap, Flatpak and AppImage apps be first-class with their own identifiers in profile lists (APP-002, APP-006)?
5. Kill-after-launch versus deny-before-launch as the required user experience (APP-001): is a brief window flash acceptable? Is it acceptable that a stalled enforcer could block all program launches until a watchdog fires, if fanotify is chosen?
6. Sandboxed browsers (Snap and Flatpak): if their policy paths cannot be proven, accept that Chromium snap, Flatpak Chrome/Brave and similar are 'unsupported' and blocked as applications (SITE-006), even though Chromium and Firefox are snaps by default on Ubuntu? Or require native packages only?
7. Opera and other browsers on Linux (SITE-005 lists Opera as supported on desktop, T011 researches it): same treatment as Windows, or defer?
8. Whether hosts, DNS or proxy filtering may be used as a supplement (they cannot meet SITE-002 alone), and whether any TLS-intercepting proxy with a root CA is out of bounds.
9. Whether a force-installed browser extension is acceptable as a later mechanism (store publication dependency).
10. Root installation: is a package install that requires administrator privileges (polkit/sudo) acceptable, and is a Linux user with sudo/root explicitly outside the protection model (T034)?
11. Acceptance of ADR-0010 and permission to update T020/T042 scopes (T019 acceptance criteria 2 and 3 depend on it). (The ADR number is the researcher's; the main agent allocates it.)

### Unverified (16, none softened)

1. No prototype was executed: no OS, kernel, browser, systemd or snapd versions were tested.
2. Behaviour of fanotify `FAN_OPEN_EXEC_PERM` with Flatpak, Snap and AppImage launches, including whether FUSE-hosted AppImage mounts are markable (zero-fsid ENODEV since 6.8 per the man page, not tried).
3. Whether `/proc/<pid>/exe` and `/proc/<pid>/cgroup` are fully populated when a fanotify exec event or `cn_proc` exec event is delivered.
4. fanotify permission-event timeouts, and whether the kernel limits a non-responding listener (fanotify(7) documents none).
5. Which kernel version introduced unprivileged `cn_proc` subscription in `cn_bind` and `cgroup.kill` (not stated in the fetched sources; `cgroup.kill` 5.14 and `pidfd_open` 5.3 are from memory).
6. Whether Ubuntu merged `bpf` into `CONFIG_LSM` after the September 2026 patch thread (one message is a NACK; outcome not seen).
7. Ubuntu 24.04 kernel (6.8) and X11 availability (search-result summaries, not an official fetch); Debian 13 default Wayland session (kernel 6.12 and GNOME 48 per secondary sources); Plasma 6.8 release date and distro availability (secondary sources; KDE's own blog post seen only in search).
8. Policy directory for Brave (support.brave.app returned 403), Edge (community sources only), Vivaldi, Opera and Chromium snap (conflicting reports, Launchpad bug 1714244); Flatpak Chrome/Edge/Firefox policy locations.
9. Firefox WebsiteFilter subdomain matching and subresource blocking (not stated in the fetched admin docs); Chrome URLBlocklist 1000-entry limit (not on the fetched help page) and subresource/iframe behaviour.
10. Chrome `DnsOverHttpsMode` official value names and Linux support (the official policy list returned no policy text; only third-party sources).
11. Official cgroup scope naming per DE in practice (systemd.io page marked work-in-progress; GNOME 50 and Plasma 6.8 behaviour untested); Snap `SNAP_APP_NAME` (snapd 2.77+) contents; Snap exe path stability across revisions.
12. Flatpak: readability of `/.flatpak-info` through `/proc/<pid>/root` from the host as root, and whether `[Instance]` offers a stable PID mapping (not in the metadata man page).
13. Folder blocking and notification hiding were out of T019 scope (T043) and were not researched beyond fanotify properties.
14. Whether .NET can host netlink and fanotify without a native helper (P/Invoke feasibility, AOT/trimming implications) was not researched.
15. Real-world distribution shares (Wayland vs X11, Ubuntu vs Fedora vs Debian) were not retrieved; matrix sizing is by availability and release facts only.
16. SELinux (Fedora) and AppArmor effects on a root agent, socket directory and fanotify were not researched.

### Acceptance criteria and checks: NOT met

- Criterion 1 (Linux foundation mechanisms ADR with tested versions, access boundaries, application/browser outcomes, limitations, explicit acceptance status): **not met**. No ADR was written; no versions were tested; no owner acceptance exists.
- Criterion 2 (T020/T042 have implementation-ready boundaries from accepted findings): **not met**. Nothing is accepted and the distribution decisions are open; T020/T042 were not changed.
- Criterion 3 (T043 keeps folder/notification/package work; named tasks cover remaining research; untested hypotheses not marked verified): **partly met**. No hypothesis was marked verified and T043 scope was respected, but the named child research tasks do not exist yet.
- Check "execute the bounded process/browser/IPC sample, recording OS/kernel/browser versions, permissions and limitations": **not verified** (nothing run; no prototype command exists).
- Check "review ADR acceptance, owner distribution decision, updated T020/T042 scopes and T043 handoff": **not verified** (none exist).

### Follow-up work needed (prototype matrices)

1. Matrix 1 (Ubuntu 26.04 VM): process detection comparing `cn_proc` listener, `/proc` polling and fanotify `FAN_OPEN_EXEC_PERM`; record kernel, latency, missed events, stall/watchdog behaviour, kill-vs-deny UX, with actual commands.
2. Matrix 2: application identity for deb, Snap, Flatpak and AppImage apps (exe path, cgroup scope, `/.flatpak-info`, desktop-file ID) and whole-scope kill on each.
3. Matrix 3: browser policy apply/cleanup for Chrome, Chromium (deb and snap), Edge, Brave, Firefox (deb and snap), Flatpak variants and Opera/Vivaldi; verify with `chrome://policy`, `brave://policy`, `about:policies`; test DoH-off policies and restart behaviour.
4. Matrix 4: systemd unit plus `AF_UNIX` socket with `SO_PEERCRED`, restart/reconnect and non-root stop attempts (feeds [T020](T020-bootstrap-linux-agent.md)).
5. Deferred research: eBPF LSM feasibility per distro (only if fanotify proves inadequate); supplementary hosts/DNS filtering, DoH handling and a possible companion extension.
6. Draft the ADR from this record and the later prototype results, then request owner acceptance (criterion 1).
7. After the owner picks a matrix: update T020/[T042](T042-implement-linux-enforcement.md) scopes, split T042 into process-enforcement and browser-policy children if required, and add shared Linux path/identity test vectors via [T022](T022-add-shared-test-vectors.md).
8. [T043](T043-research-linux-folders-notifications-packaging.md) handoff: fanotify folder prototype using the mark/ENODEV constraints, Wayland window-navigation limits for FLD-005, notification hiding on GNOME/KDE, packaging for the chosen distributions.

### Deferred record edits (proposed only; not made, pending owner decisions and prototypes)

- New Linux mechanisms ADR (status Proposed, extends ADR-0007) with an evidence table (all rows "desk research 2026-11-18" until prototyped), options, proposed direction marked "proposal, not accepted", owner decisions, and a limitations section listing zero prototype runs. The researcher suggested 0010 (the Android and Opera researchers did too), so the main agent must allocate the number; also add the row to the [ADR README](../docs/architecture/decisions/README.md).
- [`docs/spec/platforms/linux.md`](../docs/spec/platforms/linux.md): "Detecting process launches" row (cn_proc init-namespace limit, fanotify exec-perm, eBPF LSM inactive by default on Ubuntu, `/proc` fallback, desk research only); "Blocking applications" row (identity candidates, unverified); "Blocking websites" row (policy directories per vendor docs; Snap/Flatpak unverified; hosts/DNS does not meet SITE-002); LNX-001 (keep the `> ❓ Question`, add facts: no X11 session on Ubuntu 26.04, Fedora 43 GNOME, Plasma 6.8); LNX-002 candidate transport note (status stays "planned"). Page stays "draft"; no requirement status changes.
- [`docs/spec/apps.md`](../docs/spec/apps.md) header: Flatpak/Snap/AppImage identifier format pending owner decision. [`docs/spec/sites.md`](../docs/spec/sites.md) SITE-005: Linux policy locations in the ADR; sandboxed browsers unsupported until verified (flag as proposal).
- T020 and T042 scope planning notes; T043 handoff note (fanotify marks, zero-fsid ENODEV on FUSE 6.8+, Wayland FLD-005 limit). No status or `depends_on` changes.
- ROADMAP/tasks: add the follow-up research tasks as `ready` (main agent assigns IDs, `area`, `depends_on`) only after the owner picks a matrix option.

### Sources (all consulted by the researcher)

Repository files read: this task; [linux.md](../docs/spec/platforms/linux.md); [apps.md](../docs/spec/apps.md); [sites.md](../docs/spec/sites.md); [content.md](../docs/spec/content.md); [folders.md](../docs/spec/folders.md); [product.md](../docs/spec/product.md) (GEN-001..GEN-006); [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md); [AI_ORCHESTRATION.md](../docs/engineering/AI_ORCHESTRATION.md); headers/scope of [T020](T020-bootstrap-linux-agent.md), [T042](T042-implement-linux-enforcement.md), [T043](T043-research-linux-folders-notifications-packaging.md).

Kernel and process mechanisms:
- https://man7.org/linux/man-pages/man7/fanotify.7.html; https://man7.org/linux/man-pages/man2/fanotify_init.2.html; https://man7.org/linux/man-pages/man2/fanotify_mark.2.html; https://spinics.net/lists/linux-fsdevel/msg196495.html; https://raw.githubusercontent.com/torvalds/linux/master/drivers/connector/cn_proc.c; https://raw.githubusercontent.com/torvalds/linux/master/drivers/connector/connector.c; https://man7.org/linux/man-pages/man2/pidfd_send_signal.2.html; https://docs.kernel.org/admin-guide/cgroup-v2.html; https://docs.kernel.org/bpf/prog_lsm.html; https://raw.githubusercontent.com/torvalds/linux/master/security/Kconfig; https://lists.ubuntu.com/archives/kernel-team/2026-September/171517.html

Distributions and window detection:
- https://www.theregister.com/2026/04/24/ubuntu_resolute_raccoon/; https://fedoraproject.org/wiki/Changes/WaylandOnlyGNOME; https://blogs.kde.org/2025/11/26/going-all-in-on-a-wayland-future/; https://linuxiac.com/kde-plasma-6-8-released-drops-x11-sessions-after-30-years-of-kde/; https://debian.org/News/2025/20250809; https://wayland.app/protocols/ext-foreign-toplevel-list-v1; https://wayland.app/protocols/wlr-foreign-toplevel-management-unstable-v1; https://discourse.gnome.org/t/unable-to-call-the-remote-getwindows-gnome-method-via-dbus/21201; https://docs.rs/crate/kdotool/0.2.3/source/src/lib.rs

Application identity and IPC:
- https://systemd.io/DESKTOP_ENVIRONMENTS/; https://man7.org/linux/man-pages/man5/flatpak-metadata.5.html; https://gitlab.gnome.org/GNOME/gnome-usage/-/merge_requests/63; https://pkg.go.dev/github.com/lephar/snapd/sandbox/cgroup; https://docs.appimage.org/packaging-guide/environment-variables.html; https://www.freedesktop.org/software/systemd/man/latest/systemd.socket.html; https://www.freedesktop.org/software/systemd/man/latest/systemd.service.html; https://man7.org/linux/man-pages/man7/unix.7.html

Browser policies:
- https://support.google.com/chrome/a/answer/9027408; https://support.google.com/chrome/a/answer/9942583; https://www.chromium.org/administrators/linux-quick-start; https://mozilla.github.io/policy-templates/; https://firefox-admin-docs.mozilla.org/reference/policies/websitefilter/; https://firefox-admin-docs.mozilla.org/reference/policies/dnsoverhttps/; https://forum.snapcraft.io/t/auto-connecting-the-system-files-interface-for-the-firefox-snap; https://forum.snapcraft.io/t/chromium-policies/19376; https://discourse.flathub.org/t/question-where-are-brave-policies-located-in-flatpak/9304; https://github.com/TommyTran732/Microsoft-Edge-Policies; https://support.brave.app/hc/en-us/articles/360039248271 (fetch returned 403; used only as a pointer from search)

Cited in the researcher's option evidence but not in its source list (surfaced by search or secondary; several not fetched):
- https://man7.org/linux/man-pages/man5/hosts.5.html (via search; the absence of wildcard text is inference); https://support.google.com/chrome/a/answer/9867568 (ExtensionSettings; surfaced by search, not fetched); https://support.google.com/chrome/a/answer/7517525 (Linux extension policies; surfaced by search, not fetched); https://fedoramagazine.org/whats-new-fedora-workstation-43/ (search result)
- Also cited without a URL: gnome-system-monitor merge request 132 (reading `/proc/<pid>/root/.flatpak-info`, cgroup fallback); the snapd cgroup-tracking test (Snap scope name `snap.<name>-<uuid>.scope`).
