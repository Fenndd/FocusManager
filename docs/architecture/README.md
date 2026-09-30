# FocusManager Architecture (Draft)

## Layers
- **FocusManager.App**: WinUI 3 desktop UI for mode switch and whitelist editing.
- **FocusManager.Agent**: background runtime that watches new launch/open events and applies enforcement.
- **FocusManager.Core**: domain models, rule contracts, and business abstractions.
- **FocusManager.Infrastructure**: persistence and Windows-specific integrations.
- **FocusManager.Contracts**: IPC DTO/messages shared by App and Agent.

## Runtime idea
1. User enables study mode in UI.
2. Agent captures current session snapshot (already-running processes are ignored).
3. Agent enforces only new actions:
   - new process starts,
   - new Explorer folder openings,
   - Chrome whitelist policy application.
4. UI can disable mode and edit whitelists.
