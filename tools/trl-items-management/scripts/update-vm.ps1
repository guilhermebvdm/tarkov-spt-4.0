<#
  update-vm.ps1 — instala/atualiza o mod TRL-ItemsManagement na VM do servidor SPT, em UM comando.
  Vai DENTRO do trl-release-vX.Y.Z.zip; rode a partir da pasta extraída do bundle.

  Substitui a arquitetura antiga (viewer Node standalone na porta 8080 + mod companion
  TRLTraderPrices) por um único mod C# que serve a própria UI de dentro do processo do
  SPT.Server (porta do próprio SPT, ex. 6969) — não sobe mais nenhum processo Node
  persistente; o Node só é invocado sob demanda (rescan/refresh) pelo próprio mod.

  MIGRAÇÃO (1ª vez, automática e idempotente): se `user\mods\TRLTraderPrices\` ainda existir,
  o script faz backup de `config\overrides.json`/`config\buy-overrides.json`, MOVE a pasta
  inteira pra fora de `user\mods\` (renomear no lugar NÃO desabilita um mod — o SPT varre
  toda subpasta de user\mods\ pelo conteúdo, não pelo nome) e copia os overrides preservados
  pro config\ do mod novo — só na 1ª vez (se o mod novo já tiver overrides.json, não sobrescreve
  nunca, pra não perder edição feita depois pela UI nova). Também para/remove o serviço NSSM ou
  a Scheduled Task do viewer antigo, se existir, e mata qualquer node.exe rodando `serve.js`.

  Preços/ban/nível de flea editados no viewer antigo vivem direto em SPT_Data (ragfair.json,
  items.json, globals.json) — não em pasta de mod nenhuma, então SEMPRE sobrevivem a esta
  migração automaticamente (o script nunca toca em SPT_Data).

  Idempotente e seguro pro resto: NUNCA sobrescreve `.env`, `config\*.json` (overrides/
  bans/pipeline) do mod novo, nem os caches do pipeline (`cache\`, `logs\`) — só código
  (DLLs, wwwroot) e o catálogo (`data\*.json`, regenerado a cada run).

  Uso na VM (após extrair o bundle):
    powershell -ExecutionPolicy Bypass -File .\update-vm.ps1
  Opções:
    -SptPath <path>    raiz do SPT (contém SPT_Data).        Default: D:\SPT 4.0\SPT
    -GameRoot <path>   raiz do jogo (contém BepInEx\).        Default: pai de -SptPath
    -ToolDir <path>    pasta do pipeline Node (sem viewer).    Default: D:\trl-items-management
    -NodeExe <path>    node.exe.                               Default: C:\Program Files\nodejs\node.exe
    -Fetch             também repuxa preços online (precisa internet + .env com a key)
    -NoStartGame       só atualiza; não relança o SPT.Server.exe
#>
param(
  [string]$SptPath = "D:\SPT 4.0\SPT",
  [string]$GameRoot,
  [string]$ToolDir = "D:\trl-items-management",
  [string]$NodeExe = "C:\Program Files\nodejs\node.exe",
  [switch]$Fetch,
  [switch]$NoStartGame
)
$ErrorActionPreference = "Stop"
$bundle = $PSScriptRoot
if (-not $GameRoot) { $GameRoot = Split-Path $SptPath -Parent }
function Step($m) { Write-Host "`n== $m ==" -ForegroundColor Cyan }

# O passo 1/6 (parar/remover serviço NSSM ou Scheduled Task do viewer antigo) engole erro de
# permissão de propósito (-EA SilentlyContinue), pra um setup sem esses artefatos não travar a
# run. Sem elevação, se o serviço/task antigo rodar como SYSTEM, a remoção falha CALADA — o
# script segue achando que não tinha nada pra remover, e o processo antigo pode voltar sozinho
# num reboot da VM. Só um aviso; não bloqueia (o resto da migração não precisa de admin).
$isElevated = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isElevated) {
  Write-Host "AVISO: sessão não elevada (Run as Administrator). Se o viewer/serviço antigo rodar como SYSTEM, a limpeza do passo 1/6 pode falhar em silêncio — confira o log abaixo e, se o setup antigo usava serviço/task, rode de novo elevado se sobrar algo." -ForegroundColor Yellow
}

# --- sanity: bundle + alvos existem ---
if (-not (Test-Path "$bundle\TRL-ItemsManagement\server\TRLItemsManagement-Server.dll")) { throw "bundle inválido: rode o update-vm.ps1 de DENTRO da pasta extraída do zip (falta TRL-ItemsManagement\server\)." }
if (-not (Test-Path "$bundle\TRL-ItemsManagement\client\TRLItemsManagement-Client.dll")) { throw "bundle inválido: falta TRL-ItemsManagement\client\TRLItemsManagement-Client.dll." }
if (-not (Test-Path $NodeExe))            { throw "node.exe não encontrado em '$NodeExe' (ajuste -NodeExe)." }
if (-not (Test-Path "$SptPath\SPT_Data")) { throw "SPT_Data não encontrado em '$SptPath' — -SptPath tem que ser a pasta que CONTÉM SPT_Data\ (o server root, ex. 'D:\SPT 4.0\SPT'), não a raiz do jogo." }
if (-not (Test-Path "$GameRoot\BepInEx")) { throw "BepInEx não encontrado em '$GameRoot' — -GameRoot tem que ser a pasta que CONTÉM BepInEx\ (a raiz do jogo, ex. 'D:\SPT 4.0'). Default é o pai de -SptPath; ajuste -GameRoot direto se a instalação não seguir esse padrão." }

$serverDir = Join-Path $SptPath "user\mods\TRL-ItemsManagement"
$clientDir = Join-Path $GameRoot "BepInEx\plugins\TRL-ItemsManagement"
$oldModDir = Join-Path $SptPath "user\mods\TRLTraderPrices"

# Suposição: só 1 instalação de SPT.Server rodando nesta máquina. Mata por NOME (não dá pra
# filtrar por -SptPath com segurança sem elevação — .Path de outro processo exige admin) — se
# um dia essa VM rodar 2 instalações, isso mata a errada.
Step "0/6 parando SPT server"
Get-Process SPT.Server -EA SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 800

Step "1/6 migração única do setup antigo (viewer Node + TRLTraderPrices), se ainda presente"
$migratedOverrides = $null
$migratedBuyOverrides = $null

# 1a. viewer antigo: serviço NSSM e/ou Scheduled Task, se existirem — parar e remover
$oldService = Get-Service -Name "TRLItemsManagement" -EA SilentlyContinue
if ($oldService) {
  Write-Host "  -> removendo serviço antigo do viewer (NSSM): TRLItemsManagement"
  Stop-Service -Name "TRLItemsManagement" -Force -EA SilentlyContinue
  sc.exe delete "TRLItemsManagement" | Out-Null
  if ($LASTEXITCODE -ne 0) { Write-Host "  AVISO: 'sc.exe delete' falhou (código $LASTEXITCODE) — o serviço antigo pode ainda estar registrado. Confira com 'sc query TRLItemsManagement' e remova manualmente se precisar." -ForegroundColor Yellow }
}
$oldTask = Get-ScheduledTask -TaskName "TRLItemsManagement" -EA SilentlyContinue
if ($oldTask) {
  Write-Host "  -> removendo Scheduled Task antiga do viewer: TRLItemsManagement"
  Stop-ScheduledTask -TaskName "TRLItemsManagement" -EA SilentlyContinue
  Unregister-ScheduledTask -TaskName "TRLItemsManagement" -Confirm:$false -EA SilentlyContinue
}
# viewer pode também ter sido rodado na mão (sem serviço/task) — mata o node servindo serve.js
Get-CimInstance Win32_Process -Filter "Name='node.exe'" -EA SilentlyContinue |
  Where-Object { $_.CommandLine -match 'serve\.js' } |
  ForEach-Object { Write-Host "  -> matando node.exe (serve.js) pid $($_.ProcessId)"; Stop-Process -Id $_.ProcessId -Force -EA SilentlyContinue }

# 1b. TRLTraderPrices (mod companion antigo): backup do config + MOVER pra fora de user\mods\
#     (renomear no lugar não desabilita — SPT varre toda subpasta por conteúdo, não por nome)
if (Test-Path $oldModDir) {
  $backupDir = Join-Path $GameRoot "_backup-migration-items-management-$(Get-Date -Format 'yyMMdd-HHmmss')"
  New-Item -ItemType Directory -Force $backupDir | Out-Null
  if (Test-Path "$oldModDir\config\overrides.json") {
    Copy-Item "$oldModDir\config\overrides.json" "$backupDir\overrides.json"
    $migratedOverrides = "$backupDir\overrides.json"
    Write-Host "  -> backup: config\overrides.json -> $backupDir"
  }
  if (Test-Path "$oldModDir\config\buy-overrides.json") {
    Copy-Item "$oldModDir\config\buy-overrides.json" "$backupDir\buy-overrides.json"
    $migratedBuyOverrides = "$backupDir\buy-overrides.json"
    Write-Host "  -> backup: config\buy-overrides.json -> $backupDir"
  }
  $removedDir = Join-Path $GameRoot "_removed-mods\TRLTraderPrices"
  New-Item -ItemType Directory -Force (Join-Path $GameRoot "_removed-mods") | Out-Null
  if (Test-Path $removedDir) { Remove-Item $removedDir -Recurse -Force }
  Move-Item $oldModDir $removedDir
  Write-Host "  -> TRLTraderPrices movido pra fora de user\mods\: $removedDir"
} else {
  Write-Host "  (TRLTraderPrices já não está em user\mods\ — nada a migrar aqui, run normal)"
}

Step "2/6 instalando o mod novo (server + client)"
# server: wwwroot é código, sobrescreve sempre; config\ e data\ NUNCA vêm no bundle (ver
# package-release.sh) e por isso nunca são tocados aqui — sobrevivem a qualquer update.
# /R:5 /W:2 (nunca o default do robocopy, /R:1000000 /W:30) — se o processo não soltou o lock
# da DLL a tempo (sleep de 800ms acima pode não bastar), falha em ~10s com erro claro em vez de
# ficar preso tentando de novo por horas.
New-Item -ItemType Directory -Force $serverDir | Out-Null
if (Test-Path "$serverDir\wwwroot") { Remove-Item "$serverDir\wwwroot" -Recurse -Force }
robocopy "$bundle\TRL-ItemsManagement\server" "$serverDir" /E /R:5 /W:2 /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy do server falhou (código $LASTEXITCODE) — confira se o SPT.Server realmente parou (lock de arquivo) e rode de novo." }

# clobber simétrico ao wwwroot do server — evita DLL/pdb órfã se uma versão futura remover arquivo
New-Item -ItemType Directory -Force $clientDir | Out-Null
Get-ChildItem $clientDir -EA SilentlyContinue | Remove-Item -Recurse -Force
robocopy "$bundle\TRL-ItemsManagement\client" "$clientDir" /E /R:5 /W:2 /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy do client falhou (código $LASTEXITCODE) — confira se o SPT.Server (ou o próprio jogo) realmente parou e rode de novo." }

Step "3/6 config: migrando overrides antigos (só 1ª vez) + garantindo pipeline.json"
New-Item -ItemType Directory -Force "$serverDir\config" | Out-Null
if ($migratedOverrides -and -not (Test-Path "$serverDir\config\overrides.json")) {
  Copy-Item $migratedOverrides "$serverDir\config\overrides.json"
  Write-Host "  -> config\overrides.json restaurado do TRLTraderPrices antigo"
} elseif (Test-Path "$serverDir\config\overrides.json") {
  Write-Host "  (config\overrides.json já existe no mod novo — preservado, não sobrescrito)"
}
if ($migratedBuyOverrides -and -not (Test-Path "$serverDir\config\buy-overrides.json")) {
  Copy-Item $migratedBuyOverrides "$serverDir\config\buy-overrides.json"
  Write-Host "  -> config\buy-overrides.json restaurado do TRLTraderPrices antigo"
}
if (-not (Test-Path "$serverDir\config\pipeline.json")) {
  $pipelineJson = @{ toolPath = $ToolDir.Replace('\','/') } | ConvertTo-Json
  Set-Content -Path "$serverDir\config\pipeline.json" -Value $pipelineJson -Encoding utf8
  Write-Host "  -> config\pipeline.json criado (toolPath = $ToolDir)"
} else {
  Write-Host "  (config\pipeline.json já existe — preservado)"
}

Step "4/6 atualizando o pipeline Node (preserva .env, cache\, logs\, data\items.json)"
New-Item -ItemType Directory -Force $ToolDir | Out-Null
robocopy "$bundle\trl-items-management-pipeline" "$ToolDir" /E /R:5 /W:2 /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy do pipeline falhou (código $LASTEXITCODE)." }

Step "5/6 regenerando o catálogo e sincronizando pro mod"
Push-Location $ToolDir
try {
  $env:SPT_PATH = $SptPath
  if ($Fetch) {
    & $NodeExe scripts\fetch-tarkov-dev.js --force
    if ($LASTEXITCODE -ne 0) { throw "fetch-tarkov-dev.js falhou (código $LASTEXITCODE) — precisa de internet." }
    & $NodeExe scripts\fetch-tarkov-market.js --force
    if ($LASTEXITCODE -ne 0) { Write-Host "  AVISO: fetch-tarkov-market.js falhou (código $LASTEXITCODE) — provável falta de TARKOV_MARKET_API_KEY em .env; seguindo sem preço de mercado (coluna opcional, não bloqueia o resto)." -ForegroundColor Yellow }
  }
  & $NodeExe scripts\load-spt.js
  if ($LASTEXITCODE -ne 0) { throw "load-spt.js falhou (código $LASTEXITCODE)." }
  & $NodeExe scripts\normalize.js
  if ($LASTEXITCODE -ne 0) { throw "geração do items.json falhou (código $LASTEXITCODE). Use -Fetch se for a 1ª vez sem cache de preços." }
} finally { Pop-Location }
New-Item -ItemType Directory -Force "$serverDir\data" | Out-Null
foreach ($f in @("items.json","categories.json","traders.json","meta.json","hideout-crafts.json")) {
  $src = Join-Path $ToolDir "data\$f"
  if (Test-Path $src) { Copy-Item $src "$serverDir\data\$f" -Force }
}

Step "6/6 subindo o SPT"
if (-not $NoStartGame) { Start-Process (Join-Path $SptPath "SPT.Server.exe") -WorkingDirectory $SptPath }

# --- relatório ---
$modVer = [Diagnostics.FileVersionInfo]::GetVersionInfo("$serverDir\TRLItemsManagement-Server.dll").FileVersion
Write-Host "`n[OK] Atualizado -> TRL-ItemsManagement $modVer" -ForegroundColor Green
Write-Host "     UI: https://127.0.0.1:6969/TRLItemsManagement-Server/index.html  (SPT precisa estar de pé)"
Write-Host "     SPT subindo: confira no log '[TRLItemsManagement] sell: applied N entries' + os overrides restaurados."
if ($migratedOverrides -or $migratedBuyOverrides) {
  Write-Host "     Migração aplicada nesta run — TRLTraderPrices antigo preservado (não apagado) em _removed-mods\ e o backup dos overrides em _backup-migration-items-management-*\." -ForegroundColor Yellow
}
