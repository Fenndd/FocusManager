# Monetization (PAY)

Decisions D3, G1–G6, as clarified by the owner's [L3 decision](decisions.md#l3-billing-channels-and-owner-controlled-commercial-decisions)
on 2026-11-18. Prices, the exact free/paid feature and limit matrix, trial eligibility and offline
paid-entitlement policy are on an explicit owner hold until the owner requests those decisions.
The current direction is flexible, secure entitlement and payment infrastructure with placeholders
for later commercial choices. This does not select a tier matrix, make every feature free or authorize
implementation. [T024](../../tasks/T024-research-subscriptions-and-payments.md) researches feasibility;
[T040](../../tasks/T040-plan-paid-entitlements.md) retains the planning and owner gates.

### PAY-001 Free tier
Minimal basic functionality is available for free, including synchronization between devices (decisions D3, G2).
The main purpose of the free tier is to let users try the application and lead them to the trial period.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question (owner hold, L3): exact free/paid features and limits; revisit only at the owner's explicit request.

### PAY-002 Trial period
The trial period is **7 days**, with full access to all features. Afterward, the subscription renews
automatically unless the user cancels it (decision G1).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question (owner hold, L3): trial eligibility, including whether it is once per account; revisit only at the owner's explicit request.

### PAY-003 Subscriptions
Subscriptions for 1, 3, 6, and 12 months. There is no “lifetime” purchase (decision G1).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: prices remain undecided and on the L3 owner hold.

### PAY-004 No advertising
The application has no advertising in either the free or paid tier (decision G3).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PAY-005 One subscription for all platforms
A subscription purchased on any platform grants access to paid features on all the user's platforms
(decision G4). The user signs into an account for this (SYNC-002).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Payment channels selected by L3: **Google Play on Android, App Store on iOS when pursued later, and
Stripe on desktop**. iOS development remains deferred; Linux development remains on J1 hold. Windows
website and Microsoft Store distribution (H2, GEN-006) remain targets. A distribution listing is not
an automatic choice of Microsoft Store billing. T024 must verify the compatibility of the intended
Store distribution and Stripe flow; any incompatibility returns to the owner rather than silently
removing a channel or substituting a provider.

Cross-platform account/entitlement tracking still needs an architectural solution and trusted
validation. Evaluate accounting integration such as RevenueCat versus custom Firebase tracking within
the selected payment channels. Local client functions remain offline-capable without a cloud check
for every blocking action (GEN-005); exact paid-entitlement offline policy is on the L3 owner hold.

### PAY-006 Paid tier from the first release
Paid features and the trial period are available from the first public release (decision G5).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: sales may require a legal business structure (decision G6 — do not register one yet);
the question is deferred until the first paid release.
