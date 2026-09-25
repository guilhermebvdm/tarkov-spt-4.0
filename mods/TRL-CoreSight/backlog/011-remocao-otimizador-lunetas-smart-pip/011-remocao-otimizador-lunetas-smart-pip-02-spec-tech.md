# 011 — remocao-otimizador-lunetas-smart-pip · Spec Técnica

**Mod:** TRL-CoreSight  
**Data:** 2026-09-20  
**Spec Funcional:** [011-remocao-otimizador-lunetas-smart-pip-01-spec.md](011-remocao-otimizador-lunetas-smart-pip-01-spec.md)  
**Status:** 🔵 Em andamento  

## 1. Arquitetura e Mapeamento de Mudanças

O Smart PiP estava espalhado por 5 pontos do mod:

```
[Plugin.cs]
  └── Registra OpticVolumetricsPatch (Harmony) [REMOVER]

[Patches/OpticVolumetricsPatch.cs]
  └── Postfix em OpticComponentUpdater.LateUpdate [DELETAR ARQUIVO]

[Configuration/ModConfig.cs]
  └── Seção 7 com 4 ConfigEntries [REMOVER]

[Core/InteriorOcclusionWatcher.cs]
  └── Flag _isPiPAimActive e cálculo condicional de PiPShadowDistance [REMOVER]

[Core/PerformanceManager.cs]
  ├── Leitura de CurrentScope.IsOptic [REMOVER]
  ├── SetPiPAimActive(isPiPOptic) [REMOVER]
  ├── Ramo isPiPOptic em HandleDynamicLOD [REMOVER]
  └── Linha PiP Optic no OnGUI debug [REMOVER]
```

## 2. Detalhamento das Alterações

### 2.1 `Patches/OpticVolumetricsPatch.cs` & `Plugin.cs`
- Excluir o arquivo `modded/Patches/OpticVolumetricsPatch.cs`.
- Em `Plugin.cs`, remover a linha:
  ```csharp
  new OpticVolumetricsPatch().Enable();
  ```
- Atualizar a constante de versão do plugin para `0.4.17`.

### 2.2 `Configuration/ModConfig.cs`
- Remover as 4 propriedades estáticas:
  - `EnableSmartPiP`
  - `PiPShadowDistance`
  - `PiPExternalLODBias`
  - `StripOpticVolumetrics`
- Remover o bloco de inicialização no método `Init`:
  ```csharp
  // 7. Otimizador de Lunetas (Smart PiP)
  EnableSmartPiP = config.Bind(...)
  PiPShadowDistance = config.Bind(...)
  PiPExternalLODBias = config.Bind(...)
  StripOpticVolumetrics = config.Bind(...)
  ```

### 2.3 `Core/InteriorOcclusionWatcher.cs`
- Remover:
  - `private bool _isPiPAimActive;`
  - `public bool IsPiPAimActive => _isPiPAimActive;`
  - `public void SetPiPAimActive(bool active)`
- Em `OnUpdate()`:
  - Condição de ativação:
    ```csharp
    // Antes:
    if (!ModConfig.ModEnabled.Value || (!ModConfig.EnableInteriorShadowCulling.Value && !ModConfig.EnableSmartPiP.Value))
    // Depois:
    if (!ModConfig.ModEnabled.Value || !ModConfig.EnableInteriorShadowCulling.Value)
    ```
  - Cálculo de distância alvo:
    ```csharp
    // Antes:
    float baseDistance = ...;
    if (_isPiPAimActive && ModConfig.EnableSmartPiP.Value)
        _targetShadowDistance = Mathf.Min(baseDistance, ModConfig.PiPShadowDistance.Value);
    else
        _targetShadowDistance = baseDistance;

    // Depois:
    _targetShadowDistance = _isInside
        ? ModConfig.InteriorShadowDistance.Value
        : ModConfig.ExteriorShadowDistance.Value;
    ```

### 2.4 `Core/PerformanceManager.cs`
- Em `Update()`:
  - Substituir:
    ```csharp
    bool isAiming = _mainPlayer != null && _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;
    bool isPiPOptic = isAiming && _mainPlayer.ProceduralWeaponAnimation.CurrentScope != null && _mainPlayer.ProceduralWeaponAnimation.CurrentScope.IsOptic;

    _shadowWatcher?.SetPiPAimActive(isPiPOptic);
    _shadowWatcher?.OnUpdate();
    ...
    HandleDynamicLOD(isAiming, isPiPOptic);
    ```
    por:
    ```csharp
    bool isAiming = _mainPlayer != null && _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;

    _shadowWatcher?.OnUpdate();
    ...
    HandleDynamicLOD(isAiming);
    ```
- Em `HandleDynamicLOD`:
  ```csharp
  private void HandleDynamicLOD(bool isAiming)
  {
      if (!ModConfig.EnableDynamicLODBias.Value || _mainPlayer == null || _mainPlayer.HandsController == null)
      {
          return;
      }

      float targetLod = isAiming ? ModConfig.AimLODBias.Value : ModConfig.BaseLODBias.Value;

      if (Math.Abs(_currentLodBias - targetLod) > 0.02f)
      {
          _currentLodBias = Mathf.MoveTowards(_currentLodBias, targetLod, 4.0f * Time.deltaTime);
          QualitySettings.lodBias = _currentLodBias;
      }
  }
  ```
- Em `OnGUI()`:
  - Remover resolução de `isPiPOptic` e a linha `$"PiP Optic: ...\n"`.

### 2.5 `TRL-CoreSight.csproj`
- Remover `<Compile Include="Patches\OpticVolumetricsPatch.cs" />`.
- Atualizar `<Version>0.4.17</Version>`.
