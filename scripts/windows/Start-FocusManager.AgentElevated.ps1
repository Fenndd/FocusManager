[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$AgentExe
)

$ErrorActionPreference = "Stop"

$resolvedAgentExe = (Resolve-Path -LiteralPath $AgentExe).Path
$agentDirectory = Split-Path -Parent $resolvedAgentExe

$runningAgents = Get-Process -Name "FocusManager.Agent" -ErrorAction SilentlyContinue
if ($runningAgents) {
    Write-Host "Stopping existing FocusManager.Agent processes..."
    $runningAgents | Stop-Process -Force
    Start-Sleep -Seconds 1
}

Write-Host "Starting FocusManager.Agent..."
Start-Process -FilePath $resolvedAgentExe -WorkingDirectory $agentDirectory
