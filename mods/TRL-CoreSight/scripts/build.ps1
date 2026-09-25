$ErrorActionPreference = "Stop"

$modRoot = Split-Path -Parent $PSScriptRoot
$moddedDir = Join-Path $modRoot "modded"
$csproj = Join-Path $moddedDir "TRL-CoreSight.csproj"
$buildsDir = Join-Path $modRoot "builds"

Write-Host "Compilando TRL-CoreSight..." -ForegroundColor Cyan

# Compilação via dotnet build
dotnet build $csproj -c Release --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha na compilação do TRL-CoreSight."
    exit $LASTEXITCODE
}

# Localiza DLL gerada
$binRelease = Join-Path $moddedDir "bin/Release"
$dll = Join-Path $binRelease "TRL-CoreSight.dll"
$pdb = Join-Path $binRelease "TRL-CoreSight.pdb"

if (Test-Path $dll) {
    Copy-Item -Force $dll (Join-Path $buildsDir "TRL-CoreSight.dll")
    if (Test-Path $pdb) {
        Copy-Item -Force $pdb (Join-Path $buildsDir "TRL-CoreSight.pdb")
    }
    Write-Host "✓ Build isolada gerada com sucesso em: $buildsDir" -ForegroundColor Green
} else {
    Write-Error "DLL compilada não encontrada em: $binRelease"
    exit 1
}
