# FocusManager

## Dev launch

Use `Start-FocusManager.cmd` from the repository root to run FocusManager during development.

The launcher builds the solution, starts `FocusManager.Agent` as administrator with its console window visible, waits briefly for the agent to initialize, and then starts the regular UI app. Confirm the UAC prompt for the agent when Windows asks; the UI should open shortly after the prompt is accepted.

Do not start the agent with plain `dotnet run` when application blocking is needed. The process-start monitor uses Windows WMI and may require elevated permissions.

If the UI shows `Access to the path is denied` for the agent status or whitelist pages, restart FocusManager with `Start-FocusManager.cmd`. A previously running elevated agent may still be using an old named pipe security descriptor.
