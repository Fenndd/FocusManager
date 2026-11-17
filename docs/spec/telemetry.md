# Technical analytics and privacy (TEL)

Detailed technical analytics for the developer (decision D4). The developer is based in the EU —
GDPR applies.

### TEL-001 Crash reports
The application sends crash and error reports (stack trace, application and OS versions, device model).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### TEL-002 Usage analytics
The application collects detailed technical analytics: which features are used, blocking errors,
performance, and the initial setup funnel (granting permissions, etc.).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### TEL-003 User consent
Usage analytics (TEL-002) are collected only with explicit consent; consent can be withdrawn
in settings. Whether consent is required for crash reports (TEL-001) needs clarification.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: a legal review (GDPR, ePrivacy) is needed before release.

### TEL-004 List contents are not sent to analytics
The user's application names, website addresses, keywords, and folder paths are not sent
to analytics.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: is collecting anonymized statistics on commonly blocked websites/applications acceptable
> (useful for ready-made profile templates)? Only with separate consent.

### TEL-005 Privacy policy
The application has a privacy policy describing the data collected (required by stores).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
