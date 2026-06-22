<#
  update-vm.ps1 — aplica um release do TRL na VM do servidor SPT, em UM comando.
  Vai DENTRO do trl-release-vX.Y.Z.zip; rode a partir da pasta extraída do bundle.

  Idempotente e seguro: NUNCA toca em .env, em data\items.json (é regenerado) nem no
  config\overrides.json do mod (user-data). Só substitui código do viewer + a DLL.

  Uso na VM (após extrair o bundle):
    powershell -ExecutionPolicy Bypass -File .\update-vm.ps1
  Opções:
    -SptPath <path>   raiz do SPT (contém SPT_Data).      Default: D:\SPT 4.0\SPT
    -ToolDir <path>   pasta do viewer.                     Default: D:\trl-items-management
    -NodeExe <path>   node.exe.                            Default: C:\Program Files\nodejs\node.exe
    -Port <n>         porta do viewer.                     Default: 8080
    -Fetch            também repuxa preços online (precisa internet + .env com a key)
    -NoStartGame      só atualiza; não relança o SPT.Server.exe
#>
param(
  [string]$SptPath = "D:\SPT 4.0\SPT",
  [string]$ToolDir = "D:\trl-items-management",
  [string]$NodeExe = "C:\Program Files\nodejs\node.exe",
  [int]$Port = 8080,
  [switch]$Fetch,
  [switch]$NoStartGame
)
$ErrorActionPreference = "Stop"
$bundle = $PSScriptRoot
function Step($m) { Write-Host "`n== $m ==" -ForegroundColor Cyan }

# --- sanity: bundle + alvos existem ---
if (-not (Test-Path "$bundle\trl-items-management\viewer\serve.js")) { throw "bundle inválido: rode o update-vm.ps1 de DENTRO da pasta extraída do zip (falta trl-items-management\)." }
if (-not (Test-Path "$bundle\TRLTraderPrices\TRLTraderPrices.dll"))  { throw "bundle inválido: falta TRLTraderPrices\TRLTraderPrices.dll." }
if (-not (Test-Path $NodeExe))            { throw "node.exe não encontrado em '$NodeExe' (ajuste -NodeExe)." }
if (-not (Test-Path "$SptPath\SPT_Data")) { throw "SPT_Data não encontrado em '$SptPath' (ajuste -SptPath p/ a raiz do SPT)." }
if (-not (Test-Path $ToolDir))            { throw "viewer não encontrado em '$ToolDir' (ajuste -ToolDir)." }

Step "1/5 parando SPT server + viewer"
Get-Process SPT.Server -EA SilentlyContinue | Stop-Process -Force
$task = Get-ScheduledTask -TaskName "TRLItemsManagement" -EA SilentlyContinue
if ($task) {
  Stop-ScheduledTask -TaskName "TRLItemsManagement" -EA SilentlyContinue
} else {
  # viewer roda manual: mata o node que está servindo o serve.js (não outros node)
  Get-CimInstance Win32_Process -Filter "Name='node.exe'" -EA SilentlyContinue |
    Where-Object { $_.CommandLine -match 'serve\.js' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -EA SilentlyContinue }
}
Start-Sleep -Milliseconds 800

Step "2/5 atualizando o viewer (preserva .env, data, logs)"
# robocopy /E = copia tudo do bundle por cima, mantendo arquivos que só existem no destino
# (.env, logs\, data\items.json antigo). Sem /PURGE → nunca apaga user-data.
robocopy "$bundle\trl-items-management" "$ToolDir" /E /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy do viewer falhou (código $LASTEXITCODE)." }

Step "3/5 instalando a DLL do mod (preserva config\overrides.json)"
$modDir = Join-Path $SptPath "user\mods\TRLTraderPrices"
New-Item -ItemType Directory -Force $modDir | Out-Null
# o bundle traz só a DLL (+ .pdb), não a pasta config\ → overrides.json fica intacto
robocopy "$bundle\TRLTraderPrices" "$modDir" /E /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy do mod falhou (código $LASTEXITCODE)." }

Step "4/5 regenerando o catálogo (items.json)"
Push-Location $ToolDir
try {
  $env:SPT_PATH = $SptPath
  if ($Fetch) {
    & $NodeExe scripts\build.js
  } else {
    # sem rede: usa os caches de preço já presentes (load-spt lê o SPT; normalize monta o items.json)
    & $NodeExe scripts\load-spt.js
    & $NodeExe scripts\normalize.js
  }
  if ($LASTEXITCODE -ne 0) { throw "geração do items.json falhou (código $LASTEXITCODE). Use -Fetch se for a 1ª vez sem cache de preços." }
} finally { Pop-Location }

Step "5/5 subindo viewer + SPT"
if ($task) { Start-ScheduledTask -TaskName "TRLItemsManagement" }
else       { Start-Process $NodeExe -ArgumentList "viewer\serve.js","$Port" -WorkingDirectory $ToolDir }
if (-not $NoStartGame) { Start-Process (Join-Path $SptPath "SPT.Server.exe") -WorkingDirectory $SptPath }

# --- relatório ---
$toolVer = "?"
$m = Select-String -Path "$ToolDir\viewer\index.html" -Pattern 'app-version[^>]*>v([0-9.]+)' -EA SilentlyContinue
if ($m) { $toolVer = $m.Matches.Groups[1].Value }
$modVer = [Diagnostics.FileVersionInfo]::GetVersionInfo("$modDir\TRLTraderPrices.dll").FileVersion
Write-Host "`n[OK] Atualizado -> viewer v$toolVer | mod $modVer" -ForegroundColor Green
Write-Host "     Viewer: http://127.0.0.1:$Port/TRLItemsManagement/  (de Ctrl+Shift+R no navegador)"
Write-Host "     SPT subindo: confira no log '[TRLTraderPrices] applied N entries' + 'versao: $modVer'"
