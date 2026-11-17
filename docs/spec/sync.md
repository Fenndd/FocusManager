# Account and synchronization (SYNC)

The cloud platform is Firebase (decision D2). The first version has no cloud (decision D1).

> ❓ Question (T037, identified by T026 on 2026-11-17): how does the first version without cloud relate
> to PAY-005 (account-backed cross-platform subscriptions) and PAY-006 (paid features from the first
> public release)? Is a local-only internal milestone intended before the first public paid release,
> or should the release sequence be clarified differently? No interpretation has been accepted.

### SYNC-001 Local first
All data is stored on the device; the application works fully without an account or internet connection (GEN-005).

Platforms: Android — planned · Windows — partial (currently requires a PostgreSQL server — WIN-006) · Linux — planned · iOS — deferred

### SYNC-002 Optional account
An account is needed only for synchronization. All free features are available without an account.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: an account is also needed for a subscription to work across all platforms (PAY-005).
> ❓ Question: sign-in methods (Google, Apple, e-mail)?

### SYNC-003 Free synchronization
Synchronizing profiles, settings, and statistics between the user's devices is part of the free
basic functionality (decision D3).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: with many free users, Firebase will exceed its free limits —
cost must be estimated before synchronization launches.

### SYNC-004 One session across all devices
A session started on one device also applies to the user's other devices
(the “One session across all devices” direction, `product.md`).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: free or premium (decision G2)?

### SYNC-005 Platform-specific data
Application and folder lists are platform specific and synchronize only between devices
on the same platform; websites, keywords, and schedules synchronize across all platforms.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: on iOS, applications are opaque tokens that cannot be synchronized even between iPhones.

### SYNC-006 Backup and transfer only through an account
Data for users with an account is stored on the backend; settings are backed up and transferred to a new
device only by signing into the account. There is no file export or import (decision D5).
A local copy remains on the device so blocking works without internet access (GEN-005).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
