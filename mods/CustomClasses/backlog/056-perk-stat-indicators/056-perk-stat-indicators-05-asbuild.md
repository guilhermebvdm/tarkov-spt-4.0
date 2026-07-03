# 056 — Indicador de perk no peso (Pack Mule) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [056-perk-stat-indicators-01-spec.md](056-perk-stat-indicators-01-spec.md)
**Spec técnica:** [056-perk-stat-indicators-02-spec-tech.md](056-perk-stat-indicators-02-spec-tech.md)
**Última review técnica:** [056-perk-stat-indicators-03-spec-tech-review-01.md](056-perk-stat-indicators-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-03

> Client-side, só exibição. Compilado 0/0 (DLL 108032 bytes). Escopo mínimo (do recon): **só o peso** ↔ Pack Mule.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Client/Patches/WeightMarkerPatch.cs` | Postfix em `HealthParametersPanel.method_0`: marcador "▲ +X%" + `HoverTooltipArea` ao lado do `_weight._maxValue`, gate = Pack Mule (classe local Scavenger/Tank + `PackMuleEnabled` + `ShowOnUi` + fator≠1). Molde do `SkillPanelPatch`. |
| MODIFICADO | `modded/Client/MultiplierFormat.cs` | + `CarryTooltip(factor, className)` (i18n): "Limite de carga +X% pela Classe … (Pack Mule, piso)". |
| MODIFICADO | `modded/Client/Plugin.cs` | `new WeightMarkerPatch().Enable();` (após o `PackMulePatch`). |

## Refs verificadas (DLL vivo — `ilspycmd`)

| Ref | Uso |
| --- | --- |
| `HealthParametersPanel.method_0()` (`:169`) | ponto de patch (Show `:134` + Update `:240`) |
| `HealthParametersPanel._weight` (`:37`) → `HealthParameterPanel._maxValue` (`:16`, `TMP_Text`) | âncora do marcador |
| `HealthParametersPanel.cs:67/251` `UpperOverweightLimit × CarryingWeightRelativeModifier` | o `Max` que o `PackMulePatch` já bufa |

## PA-NN-MM (review 01)

| ID | Impacto | Situação no build |
| --- | --- | --- |
| PA-01-01 | 🟡 | Posição via `preferredWidth` (molde `SkillPanelPatch`); `Update`/`method_0` re-posiciona. Verify-in-game. |
| PA-01-02 | 🟡 | Marcador pode invadir o sub-painel estreito do peso; ajuste de `MarkerGap` no gate. Verify-in-game. |
| PA-01-03 | 🟢 | `ItemUiContext.Instance.Tooltip` null → try/catch cobre (sem crash). |

## Mudanças posteriores

| Data | Origem | Mudança |
| --- | --- | --- |
| 2026-07-03 | validação in-game (screenshot) | Marcador funcionou (tooltip OK) mas mal posicionado (foi p/ a direita, sobre a stamina). + F12 `Weight Marker — X/Y offset` (`Perks — UI`, −600..600, F12-live) aplicado em `WeightMarkerPatch` pra reposicionar (ex.: acima do "20.7 kg"). DLL 109056, compile 0/0. |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-03 | Build concluído via `/code-mod` (compile 0/0, DLL 108032 bytes) |
