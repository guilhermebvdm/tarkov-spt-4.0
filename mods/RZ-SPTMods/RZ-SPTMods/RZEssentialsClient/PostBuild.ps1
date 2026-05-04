param (
    [string]$AssemblyName,
    [string]$Configuration,
    [string]$Framework
)

$scriptDir = $PSScriptRoot
$sptInstallPath = "D:\Games\SPT_4_0_11"
$sptClientModsPath = Join-Path $sptInstallPath "BepInEx\plugins"

$modOutput = Join-Path $scriptDir "bin\$Configuration\$Framework"

$filesToCopy = @("$AssemblyName.dll")
if ($Configuration -eq "Debug") {
    $filesToCopy += "$AssemblyName.pdb"
}

if (-not (Test-Path $sptClientModsPath)) {
    New-Item -ItemType Directory -Path $sptClientModsPath -Force | Out-Null
}

foreach ($file in $filesToCopy) {
    $src = Join-Path $modOutput $file
    if (Test-Path $src) {
        Write-Host "Copying $file → $sptClientModsPath"
        Copy-Item -Path $src -Destination $sptClientModsPath -Force
    } else {
        Write-Warning "Not found: $src"
    }
}