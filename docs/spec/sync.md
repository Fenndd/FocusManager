# Account and synchronization (SYNC)

The cloud platform is Firebase (D2, ADR-0006). [K3](decisions.md#k3-cloud-availability-and-cost-constraint)
permits cloud from the first version when needed, superseding D1's first-version/no-cloud restriction.
The owner's later [L2 clarification](decisions.md#l2-account-synchronization-offline-operation-and-cloud-cost)
sets Google and email sign-in, latest-change-wins conflicts, independent local sessions and the accepted
operating budget below. These decisions do not schedule every synchronization feature for the first release.

[T037](../../tasks/T037-resolve-account-and-sync-contract.md) still owns the versioned technical
contract, account/subscription binding, deletion and reconnect details. [T060](../../tasks/T060-estimate-cloud-costs-before-enabling-paid-services.md)
will estimate costs: target EUR 0/month, with EUR 5/month accepted as the operating-budget ceiling. Blaze is acceptable if
needed; this is an operating budget and plan permission, not an actual billing upgrade, enforceable technical spending
cap or authorization to change external resources. Costs above EUR 5/month need a new owner decision.

### SYNC-001 Local first
All data is stored on the device. All local client functions work offline; local blocking does not
require an account or a live cloud check for each action (GEN-005, L2). Login/registration, cloud
synchronization and store purchases inherently require network access. The offline paid-entitlement
policy remains deferred under [L3](decisions.md#l3-billing-channels-and-owner-controlled-commercial-decisions);
no entitlement grace period or permanent offline grant is selected here.

Platforms: Android — planned · Windows — partial (local SQLite storage, no database server — WIN-006) · Linux — planned · iOS — deferred

### SYNC-002 Optional account
Local functionality is available without an account. An account enables synchronization and a
subscription shared across platforms (PAY-005). Sign-in uses **Google and email** (L2). Apple sign-in
is not added as a requirement by this decision; iOS remains deferred.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### SYNC-003 Free synchronization
Synchronizing profiles, settings, and statistics between the user's devices is part of the free
basic functionality (D3). Synchronization conflicts use **the latest change wins** (L2). This product
rule still needs a versioned technical contract for ordering/ties, deletion, offline/reconnect and
statistics merging in T037. Shared history does not control another device's active blocking session.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: costs depend on services and usage. T060 must estimate authentication, documents, traffic and
any trusted entitlement backend, including quotas and cost controls. The EUR 0 target and EUR 5/month
accepted operating-budget ceiling do not establish free operation at scale or an enforceable technical spending cap.

### SYNC-004 Removed: one session across all devices
**Removed by L2 on 2026-11-18.** The former requirement and A3 direction for automatic cross-device
session start/stop are superseded. This identifier stays reserved for traceability; it is not an
active requirement and must not be reused for another capability. A shared account has independent
local sessions: starting or stopping blocking on a PC does not start or stop blocking on a phone,
and vice versa. Profiles/settings/history may synchronize under the remaining requirements.

### SYNC-005 Platform-specific data
Application and folder lists are platform specific and synchronize only between devices
on the same platform; websites, keywords, and schedules synchronize across all platforms.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: on iOS, applications are opaque tokens that cannot be synchronized even between iPhones.
Linux development remains on the J1 owner hold; iOS remains deferred.

### SYNC-006 Backup and transfer only through an account
Data for users with an account is stored on the backend; settings are backed up and transferred to a new
device only by signing into the account. There is no file export or import (D5).
A local copy remains on the device so all local functions work offline (GEN-005, L2).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
