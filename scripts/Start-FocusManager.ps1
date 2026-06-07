[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [int]$AgentStartupDelaySeconds = 2
)

$ErrorActionPreference = "Stop"

function Get-FirstExistingFile {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Paths,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    foreach ($path in $Paths) {
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            return (Resolve-Path -LiteralPath $path).Path
        }
    }

    $searched = $Paths -join [Environment]::NewLine
    throw "$Description was not found. Checked:$([Environment]::NewLine)$searched"
}

function Get-MSBuildPath {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $vswhere -PathType Leaf) {
        $discoveredPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" |
            Select-Object -First 1

        if (-not [string]::IsNullOrWhiteSpace($discoveredPath) -and
            (Test-Path -LiteralPath $discoveredPath -PathType Leaf)) {
            return $discoveredPath
        }
    }

    $candidatePaths = @(
        (Join-Path $env:ProgramFiles "Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"),
        (Join-Path $env:ProgramFiles "Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"),
        (Join-Path $env:ProgramFiles "Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"),
        (Join-Path $env:ProgramFiles "Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe")
    )

    foreach ($path in $candidatePaths) {
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            return $path
        }
    }

    return $null
}

$scriptDirectory = Split-Path -Parent $PSCommandPath
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $scriptDirectory "..")).Path
$solutionPath = Join-Path $repoRoot "FocusManager.sln"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "FocusManager.sln was not found at $solutionPath."
}

Write-Host "Building FocusManager ($Configuration)..."
$msBuildPath = Get-MSBuildPath
if ($msBuildPath) {
    & $msBuildPath $solutionPath /t:Build "/p:Configuration=$Configuration" /v:minimal
}
else {
    dotnet build $solutionPath -c $Configuration -v minimal
}

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

$agentExe = Get-FirstExistingFile `
    -Description "FocusManager.Agent.exe" `
    -Paths @(
        (Join-Path $repoRoot "src\FocusManager.Agent\bin\$Configuration\net8.0-windows\FocusManager.Agent.exe"),
        (Join-Path $repoRoot "src\FocusManager.Agent\bin\$Configuration\net8.0-windows\win-x64\FocusManager.Agent.exe")
    )

$appExe = Get-FirstExistingFile `
    -Description "FocusManager.App.exe" `
    -Paths @(
        (Join-Path $repoRoot "src\FocusManager.App\bin\$Configuration\net8.0-windows10.0.19041.0\win-x64\FocusManager.App.exe"),
        (Join-Path $repoRoot "src\FocusManager.App\bin\$Configuration\net8.0-windows10.0.19041.0\FocusManager.App.exe")
    )

$elevatedAgentLauncher = Join-Path $scriptDirectory "Start-FocusManager.AgentElevated.ps1"
if (-not (Test-Path -LiteralPath $elevatedAgentLauncher -PathType Leaf)) {
    throw "Elevated agent launcher was not found at $elevatedAgentLauncher."
}

Write-Host "Restarting FocusManager.Agent as administrator..."
$elevatedArguments = "-NoProfile -ExecutionPolicy Bypass -File `"$elevatedAgentLauncher`" -AgentExe `"$agentExe`""
Start-Process -FilePath "powershell.exe" -ArgumentList $elevatedArguments -Verb RunAs -WindowStyle Hidden

if ($AgentStartupDelaySeconds -gt 0) {
    Start-Sleep -Seconds $AgentStartupDelaySeconds
}

Write-Host "Starting FocusManager.App..."
Start-Process -FilePath $appExe -WorkingDirectory (Split-Path -Parent $appExe)

Write-Host "FocusManager started."
