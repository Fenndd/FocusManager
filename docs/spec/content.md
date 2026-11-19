# Content (CNT)

Blocking the contents of applications and websites rather than the entire application or website (decision E1).

### CNT-001 Keywords
Keywords can be added to a profile. During a session, pages and search queries
containing these words in the address or query text are blocked.

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a (no access to content)
Note: on Android — through accessibility services (address bar, search field); on desktop, the method
depends on the browser (URL pattern policies, browser extension) — requires research.
Owner decision [L4](decisions.md#l4-registration-consent-and-deferred-content-collection), 2026-11-18:
keyword scope is addresses and search queries. General analysis of visible on-screen/page text is deferred
and must not be researched/implemented until the owner revisits it. This is separate from the specifically
requested section-identification behavior in CNT-002, whose mechanism still needs policy/feasibility evidence.

### CNT-002 Application and website sections
Individual sections can be blocked during a session while leaving the rest available
(for example, YouTube Shorts, Instagram Reels, a recommendation feed).

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a
Note: on Android — recognizing application screens through accessibility services (breaks when these
applications update; a mechanism for quick rule updates is needed); on desktop — website sections
identified by address (for example, `youtube.com/shorts`).

### CNT-003 Notifications from blocked applications
If enabled in the profile, notifications from blocked applications are hidden during a session.

A dedicated **Blocked notifications** tab lets the user inspect the hidden notifications, including while
a session is active (owner [L5](decisions.md#l5-blocked-notification-viewing)). This is a local/offline view,
not merely a summary shown after the session. Available content depends on the platform permissions and
mechanism; no cloud upload of notification content is authorized. Opening an entry must not bypass the
profile's application/site rules.

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a
Note: no inbox/storage implementation exists yet. T035 must create bounded platform/storage/UI tasks and
record unavailable capabilities honestly; Linux portions stay on hold.
> ❓ Question: define retention, user clearing and sensitive-content display behavior before implementing storage; do not invent cloud retention or content collection.
