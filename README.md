# FocusManager

## Dev launch

Use `Start-FocusManager.cmd` from the repository root to run FocusManager during development.

The launcher builds the solution, starts `FocusManager.Agent` as administrator, waits briefly for the agent to initialize, and then starts the regular UI app. Confirm the UAC prompt for the agent when Windows asks.

Do not start the agent with plain `dotnet run` when application blocking is needed. The process-start monitor uses Windows WMI and may require elevated permissions.
