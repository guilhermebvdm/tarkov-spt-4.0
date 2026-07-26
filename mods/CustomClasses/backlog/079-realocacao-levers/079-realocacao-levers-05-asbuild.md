# 079 — Realocação de levers · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.6.3 → **0.7.1**

> Realocação mecânica (sem perk novo). Executado via g-autodev. Spec funcional = o épico.

## Arquivos alterados

| Ação | Path | Resumo |
|---|---|---|
| MOD | `PerksConfig.cs` | −Mobile Surgery, −Overladen; Shaky Hands→**Unskilled** (key F12 + default OFF→**ON**); +LightFrame(−0.20), +LoudLooter(1.30) |
| MOD | `Patches/ClassWeaponPatches.cs` | recuo Unskilled: gate Medic **+ Scav**; Rattled: gate Stealth **+ Medic** |
| MOD | `Patches/ClassSoundPatches.cs` | `SilentLooter.MultFor` + Rifleman(LoudLooter); `InteractionSoundPatch` reestruturado (Scav OU Rifleman) |
| MOD | `Patches/PackMulePatch.cs` | `LocalBonus` +Hunter/Stealth (LightFrame negativo); Postfix bônus≥0=piso, **<0=teto** |
| MOD | `CombatMedicAllyPerks.cs` | `AllyMobileSurgeon() => false` (Mobile Surgery removido) |
| DEL | `Patches/ClassMedicPatches.cs` | classe `MobileSurgeryPatch` removida |
| DEL | `Patches/ClassMovementPatches.cs` | classe `OverladenInertiaPatch` removida |
| MOD | `PerksCatalog.cs` | −Mobile/−Overladen; shaky_hands→Unskilled; +light_frame/+loud_looter; ByClass atualizado |
| MOD | `Plugin.cs` | removidos `.Enable()` de Mobile/Overladen |
| MOD | `MultiplierFormat.cs` | (fix F1) tooltip de carga: cor por sinal + "piso/teto" (era verde+"Pack Mule" hardcoded) |
| MOD | `Patches/RaidPerksNotificationPatch.cs` | (fix F3) comentário stale Overladen→Pack Mule/Light Frame |
| MOD | `../TRL-ImmersiveCombatMedicine/.../BandAidController.cs` | (fix F4) comentário stale sobre Mobile Surgery (só comentário; ICM não recompilado) |

## Code-review (sub-agent adversarial) — 0 bloqueadores

| ID | Sev | Achado | Resolução |
|---|---|---|---|
| F1 | 🟡 | Tooltip do marcador de peso mostrava `-20%` em VERDE + "Pack Mule floor" (Light Frame é drawback/teto) | **Corrigido** — cor por sinal + "piso/teto" |
| F2 | 🟡 | Loud Looter (1.30) **compõe** com Loud Operator (1.30, aplicado a todo som no SAIN) → loot do Rifleman percebido pela IA a **~1.69×**, não 1.30× | **Aceito como limitação** — compounding é pré-existente do Loud Operator; não mexer nele (fora de escopo). Se quiser 1.3 exato: baixar o Loud Looter ou desligar um. Só afeta o canal SAIN-loot. |
| F3 | 🟢 | Comentário citava o `OverladenInertiaPatch` removido | **Corrigido** |
| F4 | 🟢 | Comentário do ICM citava "exceto Mobile Surgery" | **Corrigido** |

**Verificado limpo:** sem vazamento de gate (075) — todos os branches novos mantêm gate de instância/`IsLocalClass`; PackMule teto correto (`CarryingWeightRelativeModifier` sempre ≥1); InteractionSound idêntico ao early-return antigo quando OFF; sem resíduos de código das remoções; ByClass↔Library sem órfãos.

## Assunções (g-autodev)
- Rattled e Unskilled **compartilham** o ConfigEntry entre classes (mesmo valor da planilha) — sem desdobrar.
- Light Frame e Loud Looter ganham **lever próprio**; nomes EN = "Light Frame" e "Loud Looter" (Unskilled = "Falta de habilidade").
- Rename da key F12 (Shaky→Unskilled) reseta o valor salvo — o default nasce ON de propósito.

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; code-review adversarial (0 bloqueadores, F1/F3/F4 corrigidos, F2 documentado); 0.7.1 |
