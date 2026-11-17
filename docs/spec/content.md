# Content (CNT)

Blocking the contents of applications and websites rather than the entire application or website (decision E1).

### CNT-001 Keywords
Keywords can be added to a profile. During a session, pages and search queries
containing these words in the address or query text are blocked.

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a (no access to content)
Note: on Android — through accessibility services (address bar, search field); on desktop, the method
depends on the browser (URL pattern policies, browser extension) — requires research.
> ❓ Question: should keywords also be searched for in visible on-screen text (Android permits this, but it is more demanding and
> more sensitive from a privacy perspective)?

### CNT-002 Application and website sections
Individual sections can be blocked during a session while leaving the rest available
(for example, YouTube Shorts, Instagram Reels, a recommendation feed).

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a
Note: on Android — recognizing application screens through accessibility services (breaks when these
applications update; a mechanism for quick rule updates is needed); on desktop — website sections
identified by address (for example, `youtube.com/shorts`).

### CNT-003 Notifications from blocked applications
If enabled in the profile, notifications from blocked applications are hidden during a session.

Platforms: Android — planned · Windows — research · Linux — research · iOS — n/a
> ❓ Question: should a summary of hidden notifications be shown after the session?
