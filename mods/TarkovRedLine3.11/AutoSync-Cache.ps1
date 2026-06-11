# Nome do arquivo: AutoSync-Cache.ps1
# Garante que estamos na pasta certa (pasta raiz do server)
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $PSScriptRoot

$Host.UI.RawUI.WindowTitle = "Sincronizador de Cache e SPT Server Watcher"

# --- CONFIGURAÇÃO DE CAMINHOS ---
$ServerPath = Join-Path $PSScriptRoot "SPT"
$ServerExePath = Join-Path $ServerPath "SPT.Server.exe"
$ServerCachePath = Join-Path $ServerPath "user\cache"

$HeadlessExePath = Join-Path $PSScriptRoot "FikaHeadlessManager.exe"
$UpdaterCachePath = Join-Path $PSScriptRoot "Launcher-Updater\mods_repo\user\cache"
# --------------------------------

Write-Host "--- GERENCIADOR DE CACHE 3D E SERVIDOR SPT ---" -ForegroundColor Cyan
Write-Host "Local: $PSScriptRoot"
Write-Host "----------------------------------------------"

# Validação basica
if (!(Test-Path $ServerExePath)) {
    Write-Host "ERRO CRITICO: Arquivo '$ServerExePath' nao foi encontrado!" -ForegroundColor Red
    Read-Host "Pressione ENTER para sair..."
    exit
}

# --- CHECAGEM AUTOMATICA DE MODS ---
$ModsPath = Join-Path $ServerPath "user\mods"
$HashFilePath = Join-Path $PSScriptRoot "ultimo_mod_hash.txt"

Write-Host "Verificando se houve alteracao na pasta de mods..." -ForegroundColor Yellow

function Get-ModsStateHash {
    param([string]$path)
    if (-not (Test-Path $path)) { return "N/A" }
    
    # Lista arquivos ignorando logs/configs e confia apenas no caminho e tamanho (evita falsos positivos de arquivos modificados pelo servidor)
    $files = Get-ChildItem -Path $path -Recurse -File -Exclude "*.log", "*.txt", "*.json" -ErrorAction SilentlyContinue | Sort-Object FullName
    $combinedString = ""
    foreach ($f in $files) {
        $combinedString += "$($f.FullName)_$($f.Length)"
    }
    
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($combinedString))
    return [BitConverter]::ToString($hashBytes).Replace('-', '')
}

$currentHash = Get-ModsStateHash $ModsPath
$savedHash = ""
if (Test-Path $HashFilePath) {
    $savedHash = Get-Content -Path $HashFilePath
}

if ($currentHash -ne $savedHash -or $savedHash -eq "") {
    Write-Host "Alteracao detectada nos mods! O Cliente sera aberto para atualizar o cache 3D." -ForegroundColor Magenta

    Write-Host "`n[1/4] Iniciando Servidor para carregamento de bundles..." -ForegroundColor Cyan
    $serverProcess = Start-Process -FilePath $ServerExePath -WorkingDirectory $ServerPath -PassThru
    
    Write-Host "Aguardando o Servidor ficar Online..." -ForegroundColor Yellow
    $serverLogsPath = Join-Path $ServerPath "user\logs\spt"
    $serverOnline = $false
    
    # Dá 2 segundos para o arquivo de log ser gerado
    Start-Sleep -Seconds 2
    
    while (-not $serverOnline -and -not $serverProcess.HasExited) {
        if (Test-Path $serverLogsPath) {
            $latestLog = Get-ChildItem -Path $serverLogsPath -Filter "spt*" -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            if ($latestLog) {
                # Busca nas últimas 10 linhas para ser mais leve
                $content = Get-Content -Path $latestLog.FullName -Tail 10 -ErrorAction SilentlyContinue
                if ($content -match "iniciou. Bom jogo" -or $content -match "Happy playing" -or $content -match "Started web server") {
                    $serverOnline = $true
                }
            }
        }
        Start-Sleep -Seconds 1
    }
    
    Write-Host "Servidor Online! Avancando..." -ForegroundColor Green
    
    Write-Host "`n[2/4] Iniciando FikaHeadlessManager para abrir o cliente..." -ForegroundColor Cyan
    $bepinexLog = Join-Path $PSScriptRoot "BepInEx\LogOutput.log"
    
    # Deleta o log antigo para não dar falso positivo na leitura
    if (Test-Path $bepinexLog) {
        Remove-Item -Path $bepinexLog -Force -ErrorAction SilentlyContinue
    }
    
    if (Test-Path $HeadlessExePath) {
        $headlessProcess = Start-Process -FilePath $HeadlessExePath -WorkingDirectory $PSScriptRoot -PassThru
    } else {
        Write-Host "FikaHeadlessManager.exe nao encontrado. Abra o jogo manualmente!" -ForegroundColor Yellow
    }
    
    Write-Host "`n>>> O JOGO ESTA ABRINDO... <<<" -ForegroundColor Yellow
    Write-Host "Aguardando o cliente reportar 'Connected to HeadlessWebSocket'..."
    
    $clientLoaded = $false
    
    while (-not $clientLoaded -and -not $headlessProcess.HasExited) {
        if (Test-Path $bepinexLog) {
            $content = Get-Content -Path $bepinexLog -Tail 10 -ErrorAction SilentlyContinue
            if ($content -match "Connected to HeadlessWebSocket") {
                $clientLoaded = $true
            }
        }
        Start-Sleep -Seconds 1
    }
    
    Write-Host "Cliente carregado com sucesso!" -ForegroundColor Green
    
    Write-Host "`n[3/4] Fechando FikaHeadlessManager e o Jogo..." -ForegroundColor Cyan
    # Fechar Cliente Tarkov (EscapeFromTarkov)
    Get-Process -Name "EscapeFromTarkov" -ErrorAction SilentlyContinue | Stop-Process -Force
    # Fechar Fika
    if ($null -ne $headlessProcess -and -not $headlessProcess.HasExited) {
        $headlessProcess | Stop-Process -Force
    }
    
    Write-Host "[4/4] Fechando Servidor para liberar os arquivos de cache..." -ForegroundColor Cyan
    if (-not $serverProcess.HasExited) {
        $serverProcess | Stop-Process -Force
    }
    
    # Aguarda processos encerrarem por segurança para não travar arquivos
    Start-Sleep -Seconds 3
    
    # Atualiza a memoria do script com o novo hash para nao repetir esse processo na proxima vez
    $currentHash | Out-File -FilePath $HashFilePath -Encoding UTF8
} else {
    Write-Host "Nenhuma alteracao nos mods detectada. Pulando carregamento de bundles no jogo." -ForegroundColor Green
}

# --- SINCRONIZAÇÃO COM ROBOCOPY ---
# Ocorre SEMPRE para garantir que delecoes manuais no cache sejam repassadas ao updater
Write-Host "`n--- SINCRONIZANDO PASTAS DE CACHE ---" -ForegroundColor Yellow
Write-Host "De: $ServerCachePath"
Write-Host "Para: $UpdaterCachePath"

# Cria a pasta do updater se por acaso não existir
if (!(Test-Path $UpdaterCachePath)) {
    New-Item -ItemType Directory -Force -Path $UpdaterCachePath | Out-Null
}

# Opções Robocopy:
# /MIR: Espelha a árvore (Copia os novos/alterados e apaga os que sumiram)
# /FFT: Assume tempo de arquivo FAT (Mais seguro para sincronização em Windows)
# /NDL /NJH /NJS /NP: Oculta logs poluentes, mostra apenas o nome dos arquivos copiados
robocopy "`"$ServerCachePath`"" "`"$UpdaterCachePath`"" /MIR /FFT /NDL /NJH /NJS /NP

$roboExit = $LASTEXITCODE
# Robocopy sai com 1, 2 ou 3 quando faz sucesso com cópias. Acima de 8 é erro.
if ($roboExit -ge 8) {
    Write-Host "ALERTA: Ocorreram erros durante a sincronizacao pelo Robocopy!" -ForegroundColor Red
} else {
    Write-Host "Sincronizacao do cache concluida com sucesso!" -ForegroundColor Green
}
Write-Host "---------------------------------------"


# --- MODO PRODUÇÃO / WATCHER LOOP ---
Write-Host "`n--- INICIANDO SERVIDOR SPT (MODO PRODUCAO) ---" -ForegroundColor Magenta
$WaitTime = 5

while ($true) {
    $timestamp = Get-Date -Format 'HH:mm:ss'
    Write-Host "[$timestamp] Iniciando Servidor..." -ForegroundColor Green
    
    # Inicia e espera fechar
    Start-Process -FilePath $ServerExePath -WorkingDirectory $ServerPath -Wait
    
    Write-Host "[$timestamp] Servidor fechou! Reiniciando em $WaitTime segundos..." -ForegroundColor Yellow
    Start-Sleep -Seconds $WaitTime
}
