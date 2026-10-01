<#
.SYNOPSIS
  Checkpoint 4 of the ORBIT 2.1 port: boots a throwaway copy of the SPT 4.0 server with only Orbit.Server.dll
  installed and checks that the mod loads, that every panel page answers and that the client routes return JSON.

.DESCRIPTION
  Nothing is written to the real SPT install. The server folder is copied WITHOUT its user/ folder (profiles,
  other mods) into -WorkDir, the HTTP port is changed so it cannot collide with a running server, and the
  process is stopped at the end. The copy is reused on later runs.

  What it proves: mod load on this server version, server-side render of each page, the three routes.
  What it does not prove: the interactive part of the panel (clicks, Save) — that needs a browser.

.EXAMPLE
  powershell -File mods\ORBIT-2.1\scripts\server-smoke-test.ps1
#>
param(
    [string]$SptPath,
    [string]$WorkDir = (Join-Path $env:TEMP 'orbit-server-smoke'),
    [int]$Port = 6979,
    [switch]$KeepRunning
)

$ErrorActionPreference = 'Stop'
$modRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Split-Path -Parent (Split-Path -Parent $modRoot)

if (-not $SptPath) { $SptPath = $env:SPT_PATH }
if (-not $SptPath) {
    $file = Join-Path $repoRoot '.spt-path'
    if (Test-Path $file) {
        $line = Get-Content $file | Where-Object { $_ -match '^SPT_PATH=' } | Select-Object -Last 1
        if ($line) { $SptPath = $line.Substring('SPT_PATH='.Length).Trim() }
    }
}
if (-not $SptPath) { $SptPath = 'D:/SPT' }

$serverSource = Join-Path $SptPath 'SPT'
$dll = Join-Path $modRoot 'builds\server\Orbit.Server.dll'
if (-not (Test-Path (Join-Path $serverSource 'SPT.Server.exe'))) { throw "SPT.Server.exe not found under $serverSource" }
if (-not (Test-Path $dll)) { throw "$dll not found: run /compile-mod ORBIT-2.1 --no-install first" }

if (-not (Test-Path (Join-Path $WorkDir 'SPT.Server.exe'))) {
    "Copying the server (without user/) from $serverSource to $WorkDir ..."
    New-Item -ItemType Directory -Force $WorkDir | Out-Null
    Get-ChildItem $serverSource | Where-Object { $_.Name -ne 'user' } | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $WorkDir $_.Name) -Recurse -Force
    }
}

$http = Join-Path $WorkDir 'SPT_Data\configs\http.json'
$json = Get-Content $http -Raw | ConvertFrom-Json
$json.port = $Port
$json.backendPort = $Port
$json | ConvertTo-Json -Depth 8 | Set-Content $http -Encoding utf8

$modDir = Join-Path $WorkDir 'user\mods\ORBIT-2.1'
New-Item -ItemType Directory -Force $modDir | Out-Null
Copy-Item $dll (Join-Path $modDir 'Orbit.Server.dll') -Force

Get-Process -Name 'SPT.Server' -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -and $_.Path.StartsWith($WorkDir, [StringComparison]::OrdinalIgnoreCase) } |
    ForEach-Object { Stop-Process -Id $_.Id -Force }

$started = Get-Date
# The SPT server needs a real console window; it exits when its output is redirected.
$process = Start-Process -FilePath (Join-Path $WorkDir 'SPT.Server.exe') -WorkingDirectory $WorkDir -WindowStyle Minimized -PassThru
$log = $null
$ready = $false
$deadline = (Get-Date).AddSeconds(120)
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    if ($process.HasExited) { break }
    $log = Get-ChildItem (Join-Path $WorkDir 'user\logs') -Recurse -Filter *.log -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($log -and $log.LastWriteTime -ge $started) {
        $tail = (Get-Content $log.FullName -Tail 4 -Encoding utf8) -join ' '
        if ($tail -match 'O servidor iniciou|Server has started|Happy playing') { $ready = $true; break }
    }
}

$failures = 0
function Check([string]$what, [bool]$ok, [string]$detail) {
    if ($ok) { "  OK    $what  $detail" } else { $script:failures++; "  FAIL  $what  $detail" }
}

"== Server start"
Check 'server reached "started"' $ready $(if ($process.HasExited) { "process exited with code $($process.ExitCode)" } else { "pid $($process.Id), port $Port" })

if ($ready) {
    $lines = Get-Content $log.FullName -Encoding utf8 | Where-Object { $_ -match '^\[(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d)' -and [datetime]$Matches[1] -ge $started.ToUniversalTime().AddSeconds(-2) }
    Check 'mod loaded by the SPT mod loader' ([bool]($lines -match 'ORBIT Server.*com\.chazut\.orbit\.server')) (($lines -match 'ORBIT Server.*GUID' | Select-Object -First 1) -replace '^\[[^\]]+\]\[[^\]]+\]\[[^\]]+\] ', '')
    Check 'mod OnLoad ran' ([bool]($lines -match '\[ORBIT\] Server mod loaded')) ''

    $base = "https://127.0.0.1:$Port"
    $tmp = Join-Path $WorkDir 'smoke-last.bin'
    "== Panel pages (server-side render)"
    $pages = '/orbit', '/orbit/presets', '/orbit/factions', '/orbit/general', '/orbit/looting', '/orbit/extraction',
        '/orbit/player-scav', '/orbit/main-objectives', '/orbit/poi-guard', '/orbit/zones-config', '/orbit/zone-editor',
        '/orbit/personalities', '/orbit/ghost-mode'
    foreach ($page in $pages) {
        $code = & curl.exe -k -s -o $tmp -w '%{http_code}' "$base$page"
        $html = Get-Content $tmp -Raw -Encoding utf8
        $title = if ($html -match '<title>([^<]*)</title>') { $Matches[1] } else { '(no title)' }
        $hasCss = $html -match '_content/MudBlazor/MudBlazor\.min[^"]*\.css'
        $hasJs = $html -match '_content/MudBlazor/MudBlazor\.min[^"]*\.js'
        Check "GET $page" (($code -eq '200') -and ($title -match 'ORBIT') -and $hasCss -and $hasJs) "http=$code title='$title' mudblazor-css=$hasCss mudblazor-js=$hasJs"
    }

    "== Client routes"
    foreach ($route in '/orbit/config', '/orbit/zones') {
        $code = & curl.exe -k -s -o $tmp -w '%{http_code}' -H 'responsecompressed: 0' "$base$route"
        $parsed = $null
        try { $parsed = Get-Content $tmp -Raw -Encoding utf8 | ConvertFrom-Json } catch { }
        $keys = if ($parsed) { ($parsed.PSObject.Properties.Name | Select-Object -First 4) -join ', ' } else { 'not JSON' }
        Check "GET $route" (($code -eq '200') -and ($null -ne $parsed)) "http=$code first keys: $keys"
    }
    $body = Join-Path $WorkDir 'smoke-body.json'
    '{"MapId":"bigmap","CatalogRevision":1,"Floors":{"ZoneDormitory":"base"}}' | Set-Content $body -Encoding ascii -NoNewline
    $code = & curl.exe -k -s -o $tmp -w '%{http_code}' -H 'responsecompressed: 0' -H 'requestcompressed: 0' -H 'Content-Type: application/json' --data-binary "@$body" "$base/orbit/zones/native-floors"
    Check 'POST /orbit/zones/native-floors' ($code -eq '200') "http=$code body=$((Get-Content $tmp -Raw))"

    "== Server log"
    $bad = Get-Content $log.FullName -Encoding utf8 | Where-Object { $_ -match '^\[(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d)[^\]]*\]\[(Error|Warn|Warning|Fatal|Critical)\]' -and [datetime]$Matches[1] -ge $started.ToUniversalTime().AddSeconds(-2) }
    Check 'no warning or error lines in this run' (-not $bad) "$(@($bad).Count) line(s)"
    $bad | Select-Object -First 10 | ForEach-Object { "        $_" }
}

if (-not $KeepRunning -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force; "test server stopped" }
elseif ($KeepRunning) { "test server left running: pid $($process.Id), https://127.0.0.1:$Port/orbit" }

if ($failures -eq 0) { "server-smoke-test: PASSED" } else { "server-smoke-test: $failures FAILURE(S)" }
exit $(if ($failures -eq 0) { 0 } else { 1 })
