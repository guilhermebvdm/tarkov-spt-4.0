# 051 — Levers da zona stances · As-Built

**Mod:** CustomClasses + stancesAndCameraPositionSPT4.0.11 (coordenado)
**Spec funcional:** [051-stances-zone-levers-01-spec.md](051-stances-zone-levers-01-spec.md)
**Spec técnica:** [051-stances-zone-levers-02-spec-tech.md](051-stances-zone-levers-02-spec-tech.md)
**Review técnica:** [051-stances-zone-levers-03-spec-tech-review-01.md](051-stances-zone-levers-03-spec-tech-review-01.md) (7 pontos, 🔴 0 — resoluções incorporadas)
**Build:** 2026-07-04

## Arquivos alterados

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/StaminaController.cs` | Campo-CONTRATO `public static Func<float> ExternalHandsDrainMult` (sem `?` — PA-01-03) + composição no ramo de DRENO do Tick (`delta<0 → delta *= Clamp(hook(),0,2)`; cópia local do delegate — PA-01-04). Null = byte-idêntico ao anterior. |
| CRIADO | `mods/CustomClasses/modded/Client/StancesArmStaminaBridge.cs` | Soft-detect (`TypeByName("CameraRotationMod.StaminaController")`) + `Factor()`: Hunter em ADS → SteadyArmsDrain (0.65) · Tank c/ arma pesada (`HeavyWeapon.InHand`) → TirelessArmsDrain (0) · senão 1. Null-guards, warn-once, guard de NaN (PA-01-02/05). |
| MODIFICADO | `mods/CustomClasses/modded/Client/Plugin.cs` + `Patches/RaidPerksNotificationPatch.cs` | `TryAttach()` no Awake + re-try no raid-start (PA-01-01 — todos os plugins carregados). |
| MODIFICADO | `mods/CustomClasses/modded/Client/PerksConfig.cs` | F12: `Steady Arms — Enabled/ADS arm drain mult (0.65)` · `Tireless Arms — Enabled/Heavy arm drain mult (0)`. |
| MODIFICADO | `mods/CustomClasses/modded/Client/PerksCatalog.cs` | Cards "Steady Arms" e "Tireless Arms" saem do **"em breve"**. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` · `mods/stances.../memory/sessions.md` | Docs F12 + entrada de coordenação na memória do stances (contrato externo, não renomear). |

## Deploy

- CustomClasses client: compile 0/0 → `BepInEx/plugins/CustomClasses` (automático).
- **stances: compile 0/0 → deploy MANUAL** em `BepInEx/plugins/RealisticMobility/shwngFpsCameraStances4.dll`
  (2026-07-04 17:08); a pasta duplicada que o script criou (`plugins/stancesAndCameraPositionSPT4.0.11`) foi
  **removida** (double-load de GUID). ⚠️ Lembrete da memória: o launcher pode reverter builds locais (Dev Mod off).

## Validação (gate humano)

- [ ] **Caçador** em ADS (stances ativo): barra de braço esvazia ~35% mais devagar (overlay de debug do stances ajuda).
- [ ] **Tanque** segurando LMG/GL: barra de braço **não cai**; trocar pra arma leve restaura o dreno.
- [ ] Classe sem os perks: 16 cenários do stances byte-idênticos (regressão zero).
- [ ] Log no boot: `(051) fator de stamina de braço acoplado ao stances`.
- [ ] Cards Steady/Tireless Arms **sem** "· em breve" na aba CLASS.
- [ ] Fika como cliente; raid1→raid2 sem vazamento.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-04 | Build + deploy concluídos (autônomo); review técnica 01 incorporada |
