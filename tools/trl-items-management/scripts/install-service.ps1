<#
.SYNOPSIS
  Instala o trl-items-management viewer como serviço Windows (via NSSM), bind localhost.

.DESCRIPTION
  Roda no SERVIDOR SPT. Valida pré-condições, faz backup único da config do SPT antes de
  qualquer escrita, e registra o serviço (auto-start, restart-on-crash, logs, bind 127.0.0.1).
  Ver DEPLOY.md pro fluxo completo. NÃO grava segredo em disco/git — a key vai só pro
  AppEnvironmentExtra do serviço.

.EXAMPLE
  .\install-service.ps1 -NodeExe "C:\Program Files\nodejs\node.exe" `
    -ToolDir "E:\tools\trl-items-management" -SptPath "E:\SPT\SPT" -Port 8080 -MarketKey "<key>"
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)] [string] $NodeExe,
  [Parameter(Mandatory=$true)] [string] $ToolDir,
  [Parameter(Mandatory=$true)] [string] $SptPath,   # RAIZ do install (contém SPT_Data/)
  [int]    $Port        = 8080,
  [string] $MarketKey   = '',
  [string] $ServiceName = 'TRLItemsManagement',
  [string] $BindHost    = '127.0.0.1'   # NÃO usar $Host (variável automática read-only do PowerShell)
)

$ErrorActionPreference = 'Stop'
function Fail($m) { Write-Error $m; exit 1 }

# ── Validações ────────────────────────────────────────────────────────────────
if (-not (Test-Path $NodeExe))                    { Fail "node.exe não encontrado: $NodeExe" }
$serveJs = Join-Path $ToolDir 'viewer\serve.js'
if (-not (Test-Path $serveJs))                    { Fail "serve.js não encontrado: $serveJs (ToolDir errado?)" }
$sptData = Join-Path $SptPath 'SPT_Data'
if (-not (Test-Path $sptData)) {
  Fail "SPT_Data não encontrado em '$sptData'. SptPath tem que ser a RAIZ do install (a pasta que contém SPT_Data\), não a própria SPT_Data."
}
$nssm = (Get-Command nssm -ErrorAction SilentlyContinue)
if (-not $nssm) {
  Fail "NSSM não encontrado no PATH. Instale (winget install nssm / choco install nssm) ou use o fallback Task Scheduler do DEPLOY.md."
}
$nssm = $nssm.Source

# ── Backup único da config do SPT (antes de qualquer escrita do tool) ─────────
$toBackup = @(
  (Join-Path $sptData 'configs\ragfair.json'),
  (Join-Path $sptData 'database\globals.json'),
  (Join-Path $sptData 'database\templates\items.json'),
  (Join-Path $sptData 'checks.dat')
)
foreach ($f in $toBackup) {
  if (Test-Path $f) {
    $bak = "$f.bak"
    if (-not (Test-Path $bak)) { Copy-Item -LiteralPath $f -Destination $bak; Write-Host "  backup: $bak" }
    else                       { Write-Host "  backup já existe (mantido): $bak" }
  } else {
    Write-Warning "  arquivo de config ausente (pulando backup): $f"
  }
}

# ── Pasta de logs ─────────────────────────────────────────────────────────────
$logDir = Join-Path $ToolDir 'logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force -Path $logDir | Out-Null }

# ── (Re)instalar serviço ──────────────────────────────────────────────────────
$exists = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($exists) {
  Write-Host "Serviço '$ServiceName' já existe — parando e reconfigurando."
  & $nssm stop $ServiceName | Out-Null
} else {
  & $nssm install $ServiceName "$NodeExe" "$serveJs" "$Port"
  if ($LASTEXITCODE -ne 0) { Fail "nssm install falhou (exit $LASTEXITCODE)" }
}

& $nssm set $ServiceName Application "$NodeExe"
& $nssm set $ServiceName AppParameters "`"$serveJs`" $Port"
& $nssm set $ServiceName AppDirectory "$ToolDir"
& $nssm set $ServiceName Start SERVICE_AUTO_START

# Env por-serviço (a key fica escopada aqui, fora do env global/git).
$envArgs = @("SPT_PATH=$SptPath", "ITEMDB_HOST=$BindHost")
if ($MarketKey -ne '') { $envArgs += "TARKOV_MARKET_API_KEY=$MarketKey" }
& $nssm set $ServiceName AppEnvironmentExtra @envArgs

# Logs + rotação + restart-on-crash com throttle.
& $nssm set $ServiceName AppStdout (Join-Path $logDir 'service-out.log')
& $nssm set $ServiceName AppStderr (Join-Path $logDir 'service-err.log')
& $nssm set $ServiceName AppRotateFiles 1
& $nssm set $ServiceName AppRotateBytes 5242880
& $nssm set $ServiceName AppThrottle 5000
& $nssm set $ServiceName AppExit Default Restart

& $nssm start $ServiceName
Start-Sleep -Seconds 2
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Serviço '$ServiceName': $($svc.Status)"
Write-Host "Viewer: http://$BindHost`:$Port/viewer/  (abra no navegador do server, via AnyDesk)"
Write-Host "Logs:   $logDir\service-out.log / service-err.log"
if (-not $svc -or $svc.Status -ne 'Running') {
  Write-Warning "Serviço não está Running — ver $logDir\service-err.log. Lembre de regerar data/items.json (build) antes."
}
