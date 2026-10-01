<#
.SYNOPSIS
    Build the BENCH flavor and deploy it to the sandbox, with verification at every step.

.DESCRIPTION
    Exists because two silent traps have each produced a bad deploy:
      1. Incremental builds can ignore a -p:Bench=true define flip (stale DLL).
      2. `dotnet test` rebuilds the server WITHOUT Bench and clobbers bin afterwards.
    So: always rebuild LAST, always verify the BENCH marker type is in the DLL bytes
    (type names are UTF-8 in metadata — greppable; string literals are UTF-16 — not),
    and always confirm the sampler banner after restart.
#>
[CmdletBinding()]
param(
    [string] $RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string] $Sandbox  = 'C:\SPT-vanilla-sandbox'
)
$ErrorActionPreference = 'Stop'

$busy = Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($busy) { throw 'Game is running - close it before deploying.' }

Write-Host '==> Rebuilding server (BENCH)' -ForegroundColor Cyan
& dotnet build (Join-Path $RepoRoot 'CompoundingPerf.csproj') -c Release -p:SkipDeploy=true -p:Bench=true -t:Rebuild --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'server bench build failed' }

Write-Host '==> Rebuilding client (BENCH)' -ForegroundColor Cyan
& dotnet build (Join-Path $RepoRoot 'client\CompoundingPerf.Client.csproj') -c Release -p:SkipDeploy=true -p:Bench=true -t:Rebuild --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'client bench build failed' }

$serverDll = Join-Path $RepoRoot 'bin\Release\CompoundingPerf\CompoundingPerf.dll'
$clientDll = Join-Path $RepoRoot 'client\bin\Release\CompoundingPerf.Client.dll'

# Byte-level verification: the BENCH-only type names must be present.
$serverBytes = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($serverDll))
if ($serverBytes -notmatch 'BenchRecorder')      { throw 'server DLL has no BenchRecorder - BENCH define did not take' }
$clientBytes = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($clientDll))
if ($clientBytes -notmatch 'FrameStatsRecorder') { throw 'client DLL has no FrameStatsRecorder - BENCH define did not take' }
Write-Host '    BENCH markers verified in both DLLs' -ForegroundColor Green

Write-Host '==> Stopping sandbox server' -ForegroundColor Cyan
Stop-Process -Name 'SPT.Server' -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

Copy-Item $serverDll (Join-Path $Sandbox 'SPT\user\mods\CompoundingPerf\CompoundingPerf.dll') -Force
Copy-Item (Join-Path $RepoRoot 'config.json') (Join-Path $Sandbox 'SPT\user\mods\CompoundingPerf\config.json') -Force
Copy-Item $clientDll (Join-Path $Sandbox 'BepInEx\plugins\CompoundingPerf.Client\CompoundingPerf.Client.dll') -Force
Write-Host '    deployed server + config + client' -ForegroundColor Green

Write-Host '==> Starting sandbox server' -ForegroundColor Cyan
Start-Process -FilePath (Join-Path $Sandbox 'SPT\SPT.Server.exe') -WorkingDirectory (Join-Path $Sandbox 'SPT')
Start-Sleep -Seconds 50

$log = Get-ChildItem (Join-Path $Sandbox 'SPT\user\logs\spt') -Filter '*.log' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$tail = Get-Content $log.FullName -Tail 40
if (-not ($tail | Select-String 'BENCH\] server sampler armed')) { throw 'sampler banner missing after restart - deployed DLL is not the bench build' }
if ($tail | Select-String 'did NOT take|NOT applied') { throw 'override/patch warning in boot log - investigate before testing' }
Write-Host '==> Sandbox is up: bench build verified, sampler armed, no override warnings' -ForegroundColor Green
