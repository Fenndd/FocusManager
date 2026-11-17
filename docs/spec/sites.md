# Websites (SITE)

### SITE-001 Blocking websites
During a session, opening a website forbidden by the profile's rules (PRF-002) is blocked.

Platforms: Android — planned · Windows — partial (Google Chrome only, allowlist only) · Linux — planned · iOS — deferred

### SITE-002 Host pattern format
A list entry is a host pattern:
- `example.com` — the domain itself **and all its subdomains** (decision I3);
- `*.example.com` — the same (accepted for compatibility);
- `*` — any host.

Matching is case insensitive. Scheme, path, and port are ignored in the pattern (website sections — CNT-002).

Platforms: Android — planned · Windows — partial · Linux — planned · iOS — deferred
Note: **behavior change.** Chrome policy already works this way, but `RuleEvaluator` currently checks
only the exact host for `example.com` — a `change` task is needed.

### SITE-003 Input normalization
When adding a pattern, it is cleaned up: whitespace and `/` are trimmed; input containing internal whitespace is rejected;
duplicates are not added.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### SITE-004 Empty list means no restrictions
If the profile's website list is empty, websites are not restricted (PRF-004).

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### SITE-005 Supported browsers
The platform's **most used browsers** according to statistics (StatCounter) are supported; the list is reviewed
against statistics before major releases. All other browsers are unsupported (SITE-006). Decision I4.

Initial list (StatCounter, October 2026):
- Desktop: Chrome (~67%), Edge (~13%), Firefox (~6%), Opera (~3%), Brave (~1%). Safari is macOS only and is not needed;
- Android: Chrome, Samsung Internet, Opera, Firefox (global shares include iOS; a separate Android share was not found).

Desktop browsers fall into two groups:
- **Chromium-based** (Chrome, Edge, Brave, Vivaldi, Chromium, etc.) — a shared enterprise policy mechanism
  (`URLBlocklist`/`URLAllowlist`); each browser reads them from its own registry key/directory,
  so FocusManager maintains an internal list of known browsers and their locations;
- **Firefox** — its own policy mechanism (`WebsiteFilter`), supported separately.

The user does not need to select browsers: policies apply to all installed supported browsers.
**Opera is supported** (owner's decision, 2026-11-16), but, according to user reports, does not read
enterprise policies — it needs a separate filtering method (task T011). Until that is implemented,
Opera is blocked as unsupported (SITE-006).
On Android, supported browsers depend on the selected filtering method (AND-005).

Platforms: Android — planned · Windows — partial (Google Chrome only) · Linux — planned · iOS — deferred

### SITE-006 Unsupported browsers are blocked
If a profile restricts websites, all browsers outside the supported list (SITE-005) are automatically blocked
entirely as applications during a session — in both modes (decisions E5, I4).
Otherwise, website restrictions can be bypassed by launching another browser.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: on Android, VPN filtering filters websites in all browsers; browsers with their own
encrypted DNS may bypass the filter — investigate (AND-005).
