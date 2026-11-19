# Linux platform (LNX) — draft

A primary platform (decision B1), with the same functionality as Windows (decision B5).
Status: **draft**. The mechanisms below are hypotheses; feasibility must be confirmed through research
(task T019). A C#/.NET agent shares code with the Windows agent; the UI is the same Tauri application as on Windows (ADR-0007).

**Development hold (owner decision, 2026-11-18):** Linux work is conserved and must not continue until the owner explicitly decides to resume it. See [decision J1](../decisions.md#j1-linux-development-hold). This includes research, prototypes, implementation, environment/tool setup, platform tests, CI and packaging, including Linux-only portions of mixed tasks. Existing requirements, hypotheses, research and future Windows/Linux sharing remain preserved; this hold does not cancel the platform or verify any capability. Windows and shared-code work on Windows may continue within their own task gates, including tests of pure Unix-style path data without Linux platform implementation.

## Proposed mechanisms

| Feature | Linux mechanism | Challenges |
|---------|----------------|-----------|
| Agent | System service (systemd) with root privileges + user UI process | Installation requires administrator privileges |
| Detecting process launches | Netlink proc connector or eBPF (root), fallback — polling `/proc` | Different kernels and distributions |
| Blocking applications | Terminating the process | Flatpak/Snap applications launch through wrappers — application identification is more complex |
| Blocking websites | Chromium browser policies (`/etc/opt/chrome/policies/managed/` and equivalents) and Firefox (`policies.json`, `WebsiteFilter`), as on Windows (SITE-005); other browsers — SITE-006 | Browsers in Flatpak/Snap read policies from different locations |
| Blocking folders | Investigate: `fanotify` (deny opening at the filesystem level, root) or integration with file managers (Nautilus, Dolphin) | No single file manager like Explorer; Wayland does not allow access to other applications' windows |
| Other applications' notifications | Investigate: the desktop environment's “Do Not Disturb” mode (GNOME, KDE) | Depends on the environment |
| FocusManager notifications | Freedesktop Notifications (D-Bus) | — |
| Storage | Local (SQLite — decision C6) | — |

## Requirements

### LNX-001 Supported distributions and environments
Popular distributions and desktop environments covering the majority of users are supported
(GEN-004). Initial proposal: Ubuntu LTS, Fedora, Debian; GNOME and KDE; X11 and Wayland.

Platforms: Linux — research
> ❓ Question: confirm the list and distribution format (deb/rpm, Flatpak, AppImage) — decision H2.

### LNX-002 Agent with administrator privileges
The agent runs as a system service with root privileges (needed for process monitoring, browser policies,
and folder blocking). The UI runs as the user and communicates with the agent through local IPC
accessible only to that user.

Platforms: Linux — planned

### LNX-003 System processes
Kernel, systemd, desktop environment, and window manager processes, as well as
FocusManager itself, are not blocked (APP-005).

Platforms: Linux — planned
