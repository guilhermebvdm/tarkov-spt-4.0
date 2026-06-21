# 048 — Infra de skill custom (padrão SE, sem prepatcher) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6, Fase 5 (decisão de arquitetura #2 — [class-levers.md](../../docs/class-levers.md) §1)
**Wave:** R-W1 · **Deps:** 047 (soft)

> Brief de kickoff — insumo para `/create-spec 048`. Não é a spec.

## Objetivo

Base reutilizável para as **skills custom** (🧪): reviver um slot `ESkillId` morto + ler `mgr.<skill>.Level` num patch (estilo `UpdateWeaponsPatch` do SE), com gating por `skillMultipliers` (×0 congela). **SEM prepatcher Mono.Cecil** (frágil/ofuscado). Consumida pelo item 049.

## Escopo

- Mapear o pipeline do Skills-Extended (`Core/Patches/CreateSkillPatches.cs` + `SkillManagerExt`) e portar o mínimo p/ registrar uma skill custom que **aparece no menu, ganha XP e persiste** no perfil.
- Helper p/ ler nível de skill por player no client (consumido por 049).
- Gating de XP: confirmar que `skillMultipliers[skill]=0` + início 0 congela a skill (`OnTriggerPatch.cs:33`, `val *= factor`).
- Gating de classe em runtime: ler `player.Profile.Info.GameVersion` (decisão #4).

## Riscos / atenção

- Slot `ESkillId` morto precisa existir no enum e **não colidir com o SE**. Validar quais slots estão livres.
- Compat com SE quando presente (item 006): não duplicar revives.

## Refs

- [../../../Skills-Extended/modded](../../../Skills-Extended/modded) — pipeline de referência
- [../../modded/Client/Patches/OnTriggerPatch.cs](../../modded/Client/Patches/OnTriggerPatch.cs) — gating de XP
- [../../docs/class-levers.md](../../docs/class-levers.md) §1 (decisões #2/#3/#4)
- Skills `spt-mod-best-practices`, `csharp-mod-best-practices`

## DoD (resumo)

- 1 skill custom de prova aparece no menu, sobe XP, **congela** com mult 0, e tem o nível lido num patch. Persistente no perfil.
- Padrão de `ConfigEntry` (F12) estabelecido para os parâmetros de efeito — base da configurabilidade das 🧪/🔧 (decisão #8, [class-levers.md §6.4](../../docs/class-levers.md)); server-side com nota de "restart para aplicar 100%".
- A skill custom de prova aparece no **viewer do editor** (adicionada ao `SkillMaster.cs`, padrão da seção "Gems (SE)" do 047) — convenção: skill custom nova entra no SkillMaster, não no dump de "outside canonical" (removido no 047).
