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
in settings. Consent is requested during registration (owner [L4](decisions.md#l4-registration-consent-and-deferred-content-collection),
2026-11-18), with crash reporting and usage analytics represented as distinct purposes. Creating an account
does not itself grant consent. Unregistered use has no inferred telemetry consent; local client functions
remain available without registration or internet. Withdrawal remains available in settings.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: a legal review (GDPR, ePrivacy) is needed before release.

### TEL-004 List contents are not sent to analytics
The user's application names, website addresses, keywords, and folder paths are not sent
to analytics.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Owner decision L4: collection of popular blocked applications/sites is deferred. Do not collect these
contents, including purportedly anonymized popular-item statistics, unless the owner revisits the decision.

### TEL-005 Privacy policy
The application has a privacy policy describing the data collected (required by stores).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
