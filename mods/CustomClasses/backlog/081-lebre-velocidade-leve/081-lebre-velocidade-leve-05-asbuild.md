# 081 — Lebre (velocidade quando leve) · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.8.0 → **0.9.0**

Perk NOVO (Scavenger): +30% de velocidade de movimento enquanto **não está pesado**.

## Implementação (sem patch novo)
Reusa `ClassMoveSpeed.Apply` (o mesmo motor do Heavy Frame/Execution, chamado pelos getters `MaxSpeed`/`SprintingSpeed`). Branch novo: `IsLocalClass("Scavenger") && p.Physical.Overweight <= 0f` → `m *= LebreSpeed (1.30)`.

- **Limiar resolvido:** o "não pesado" usa o estado NATIVO do EFT — `BasePhysicalClass.Overweight == 0` (o "ícone de bigorna" aparece quando `> 0`). Sem inventar %.
- **Reativo:** o `Overweight` é lido fresco a cada cálculo de velocidade (getter sem estado) → lootou e ficou pesado ⇒ Lebre desliga sozinho; descarregou ⇒ volta. Não acumula (doc do 074).

## Arquivos
| Ação | Path | Resumo |
|---|---|---|
| MOD | `PerksConfig.cs` | +`LebreEnabled`/`LebreSpeed` (1.30, SecScavenger) |
| MOD | `Patches/ClassMovementPatches.cs` | branch Lebre no `ClassMoveSpeed.Apply` (Scav + Overweight≤0) |
| MOD | `PerksCatalog.cs` | +grupo `lebre` (Hare/Lebre); ByClass Scavenger +lebre |

## Auto-revisão
- **Gate/075:** `ReferenceEquals(ctx, MainPlayer.MovementContext)` (herdado do `ClassMoveSpeed.Apply`) + `IsLocalClass` → só o player local; bots/peers intactos. ✅
- **Semântica:** `m *= 1.30` no MaxSpeed/SprintingSpeed (velocidade real = RelativeSpeed × MaxSpeed) → +30% estável, sem loop (getter sem estado — lição do 074). ✅
- **Corner:** transição no limiar de peso é natural (Overweight é contínuo 0→1; `<= 0` = exatamente sem sobrepeso). Sem flicker (o getter recalcula, não alterna abruptamente). Interação com Heavy Frame: N/A (classes diferentes). Overladen (removido no 079) não conflita.
- **Sync:** velocidade replica via Fika nativo (como Heavy Frame). ✅

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; limiar = overweight nativo; 0.9.0, build 0/0 |
