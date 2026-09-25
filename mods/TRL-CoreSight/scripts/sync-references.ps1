$sptLine = Get-Content -Path ".spt-path" | Where-Object { $_ -like "SPT_PATH=*" } | Select-Object -First 1
if (-not $sptLine) {
    Write-Error "SPT_PATH not found in .spt-path"
    exit 1
}

$sptPath = $sptLine.Substring(9).Trim()
Write-Host "Resolvendo referências de: $sptPath"

$refsDir = "mods/TRL-CoreSight/modded/References"
if (-not (Test-Path $refsDir)) {
    New-Item -ItemType Directory -Force -Path $refsDir | Out-Null
}

$dlls = @(
    @{ Name = "BepInEx.dll"; Source = "$sptPath/BepInEx/core/BepInEx.dll" },
    @{ Name = "0Harmony.dll"; Source = "$sptPath/BepInEx/core/0Harmony.dll" },
    @{ Name = "Assembly-CSharp.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll" },
    @{ Name = "UnityEngine.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/UnityEngine.dll" },
    @{ Name = "UnityEngine.CoreModule.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/UnityEngine.CoreModule.dll" },
    @{ Name = "UnityEngine.PhysicsModule.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/UnityEngine.PhysicsModule.dll" },
    @{ Name = "UnityEngine.AnimationModule.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/UnityEngine.AnimationModule.dll" },
    @{ Name = "UnityEngine.IMGUIModule.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/UnityEngine.IMGUIModule.dll" },
    @{ Name = "Comfort.dll"; Source = "$sptPath/EscapeFromTarkov_Data/Managed/Comfort.dll" },
    @{ Name = "SPT.Reflection.dll"; Source = "$sptPath/BepInEx/plugins/spt/spt-reflection.dll" }
)

foreach ($item in $dlls) {
    $src = $item.Source
    $dst = Join-Path $refsDir $item.Name
    if (Test-Path $src) {
        Copy-Item -Force -Path $src -Destination $dst
        Write-Host "  -> OK: $($item.Name)"
    } else {
        Write-Warning "  -> FALTANDO: $src"
    }
}

Write-Host "Sincronização de referências concluída."
