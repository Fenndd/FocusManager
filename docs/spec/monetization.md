# Monetization (PAY)

Decisions D3, G1–G6. The contents of the free and paid tiers will be refined along the way (decision G2).

### PAY-001 Free tier
Minimal basic functionality is available for free, including synchronization between devices (decisions D3, G2).
The main purpose of the free tier is to let users try the application and lead them to the trial period.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question (decision G2): the exact contents of the free tier.

### PAY-002 Trial period
The trial period is **7 days**, with full access to all features. Afterward, the subscription renews
automatically unless the user cancels it (decision G1).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: is the trial period available once per account?

### PAY-003 Subscriptions
Subscriptions for 1, 3, 6, and 12 months. There is no “lifetime” purchase (decision G1).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: prices have not been decided.

### PAY-004 No advertising
The application has no advertising in either the free or paid tier (decision G3).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PAY-005 One subscription for all platforms
A subscription purchased on any platform grants access to paid features on all the user's platforms
(decision G4). The user signs into an account for this (SYNC-002).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: on Android, purchases go through Google Play; on desktop, through the product's own website or Microsoft Store
(decision H2). Cross-platform subscription tracking needs an architectural solution (for example, RevenueCat or
custom tracking in Firebase).
> ❓ Question: payment method on desktop (a payment service for the product's own website) and on Linux.

### PAY-006 Paid tier from the first release
Paid features and the trial period are available from the first public release (decision G5).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: sales may require a legal business structure (decision G6 — do not register one yet);
the question is deferred until the first paid release.
