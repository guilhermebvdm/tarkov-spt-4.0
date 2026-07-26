# 079 — Realocação de levers existentes · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** o épico [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) §B (este item é realocação mecânica; o épico é a spec funcional).
**Criado:** 2026-07-26

> Mapa de pontos por lever levantado por sub-agent (arquivo:linha confirmados). Sem mecânica nova — mexe em gates de classe, catálogo, F12 e remove 2 patches.

## Decisões de config (assumidas, g-autodev)

- **Rattled → Medic** e **Falta de habilidade → Medic+Scav**: reusam o MESMO ConfigEntry (mesmo valor da planilha) — gate por `IsClass A || IsClass B`. Sem desdobrar (o valor é idêntico entre as classes).
- **Light Frame** (Hunter+Stealth, carga −0.2) e **Saque Barulhento** (Rifleman, volume 1.3): **lever próprio novo** (valor distinto do Pack Mule/Silent Looter originais).
- **Nomes EN novos:** `Falta de habilidade` → EN **"Unskilled"**; `Saque Barulhento` → EN **"Loud Looter"**.

## Plano por lever

### REMOVER — Mobile Surgery (Medic)
- `Plugin.cs:228-235` — remover o bloco try/catch com `new MobileSurgeryPatch().Enable();`.
- `Patches/ClassMedicPatches.cs:362-400` — remover a classe `MobileSurgeryPatch`.
- `PerksConfig.cs:76` + `:231-233` — remover `MobileSurgeryEnabled` (campo + Bind).
- `PerksCatalog.cs:83` — remover a `Flag` line "Mobile Surgery" do grupo `combat_medic`.
- **077/ICM:** `CombatMedicAllyPerks.AllyMobileSurgeon()` (referencia `MobileSurgeryEnabled`) → vira `return false` (com comentário 079). Efeito: o ICM **sempre imobiliza** o operador na cirurgia de aliado (a parte de movimento do 077 vira "sempre parado"). O resto do 077 (tempo) intacto.

### REMOVER — Overladen (Scav)
- `Plugin.cs:129` — remover `new OverladenInertiaPatch().Enable();`.
- `Patches/ClassMovementPatches.cs:137-180` — remover a classe `OverladenInertiaPatch`.
- `PerksConfig.cs:125-126` + `:423-430` — remover `OverladenEnabled`/`OverladenInertia`.
- `PerksCatalog.cs:169-172` (grupo `overladen`) + `:215` (tirar `"overladen"` de `ByClass["Scavenger"]`).

### RENAME + ligar — Shaky Hands → "Falta de habilidade" (Medic + Scav)
- `PerksConfig.cs:245-247` — key `"Shaky Hands — Enabled"` → `"Unskilled — Enabled"`, **default false → true** (⚠️ rename da key reseta o salvo — changelog).
- `PerksConfig.cs:248-252` — key `"Shaky Hands — Recoil mult"` → `"Unskilled — Recoil mult"` (default 1.25 mantido).
- `ClassWeaponPatches.cs:39` — gate `IsLocalClass("Combat Medic")` → `IsLocalClass("Combat Medic") || IsLocalClass("Scavenger")`.
- `PerksCatalog.cs:92-95` — grupo `shaky_hands`: nome EN "Shaky Hands"→"Unskilled", PT "Mãos Trêmulas"→"Falta de habilidade"; label "recuo"/"recoil" mantido.
- `PerksCatalog.cs:215` — adicionar `"shaky_hands"` a `ByClass["Scavenger"]` (já está no Medic:211).
- Campos C# `ShakyHandsEnabled`/`ShakyHandsRecoil` — **nome interno mantido** (só o rótulo muda), evita refactor amplo.

### ADD — Rattled → Combat Medic (aim-punch 1.5)
- `ClassWeaponPatches.cs:265` — gate `IsLocalClass("Stealth")` → `IsLocalClass("Stealth") || IsLocalClass("Combat Medic")`. (⚠️ é `else if` com Cool Under Fire/Rifleman — Medic entra no branch do Rattled.)
- `PerksCatalog.cs:211` — adicionar `"rattled"` a `ByClass["Combat Medic"]` (reusa o grupo `rattled`).

### ADD — Light Frame → Hunter + Stealth (carga −0.2)
- Novo config `LightFrameCarryPenalty` (default −0.20, faixa −0.5..0), seção... criar em Hunter+Stealth (ou compartilhado). Bind 1× (valor compartilhado).
- `PackMulePatch.cs:17-30` (`PackMule.LocalBonus`) — adicionar branches: `IsLocalClass("Hunter")` e `IsLocalClass("Stealth")` → somam `LightFrameCarryPenalty` (negativo). O piso `1f + bonus` (`:68-71`) com bonus negativo vira `<1` = carga reduzida. ⚠️ confirmar que o Postfix aceita bonus negativo (piso < 1) sem clamp indevido.
- `PerksCatalog.cs` — novo grupo `light_frame` (P, Percent, live: 1f + penalty, HigherBetter → com <1 vira drawback) + `ByClass["Hunter"]` e `ByClass["Stealth"]`.

### ADD — Saque Barulhento → Rifleman (volume loot 1.3)
- Novo config `LoudLooterVolume` (default 1.30, faixa 1..2) + `LoudLooterEnabled`.
- `ClassSoundPatches.cs:235` (InteractionSoundPatch) — adicionar branch `IsLocalClass("Rifleman")` → `volume *= LoudLooterVolume`.
- `ClassSoundPatches.cs:59-64` (`SilentLooter.MultFor`) — adicionar: se a classe é Rifleman, retornar `LoudLooterVolume` (>1). Assim o `SainSoundPatch:381` (Looting) faz a IA (SAIN) ouvir o loot do Rifleman **mais alto** → coop/AI via o pipeline existente (065/066).
- ⚠️ **Limitação (do mapa):** sem SAIN não há canal de IA base p/ loot (só o `AiSoundPatch` de passo). Então "a IA ouve mais" só vale com SAIN; sem SAIN, só o som local 1ª pessoa. Documentar.
- `PerksCatalog.cs` — novo grupo `loud_looter` (Percent, 1.3, LowerBetter → >1 = drawback) + `ByClass["Rifleman"]`.

## Riscos / corners
- **Gate de instância (075):** todos os branches novos gateiam por `IsLocalClass` (classe local) + os patches já barram bot/instância. Rattled/Falta usam `IsLocalClass` (MainPlayer implícito no ponto de aplicação). Confirmar no code-review que nenhum vaza p/ bot.
- **Rename Shaky→Unskilled:** reseta o valor salvo do usuário (changelog). Como estava OFF e agora nasce ON, o comportamento muda de propósito.
- **PackMule bonus negativo:** validar que o piso `Max`/`1+bonus` não clampa em 1 (senão Light Frame vira inócuo). Ler o Postfix.
- **Catálogo homogêneo:** `PerkGroup.IsPerk` deriva do 1º line — grupos novos (light_frame drawback, loud_looter drawback) devem ficar coerentes (todas as linhas do grupo mesma polaridade). São single-line, OK.
- **Estado entre raids / toggle mid-raid:** herdado (levers leem `.Value` no apply-time).

## Checklist
- [ ] PerksConfig: remover Mobile/Overladen; renomear Shaky→Unskilled (+default true); criar LightFrame + LoudLooter.
- [ ] ClassMedicPatches: remover MobileSurgeryPatch.
- [ ] ClassMovementPatches: remover OverladenInertiaPatch.
- [ ] ClassWeaponPatches: gate Shaky (Medic+Scav) + gate Rattled (Stealth+Medic).
- [ ] ClassSoundPatches: LoudLooter no InteractionSoundPatch + SilentLooter.MultFor (Rifleman).
- [ ] PackMulePatch: LocalBonus Hunter+Stealth (LightFrame negativo); validar piso.
- [ ] PerksCatalog: remover Mobile/Overladen; renomear shaky_hands; add rattled→Medic, light_frame→Hunter/Stealth, loud_looter→Rifleman, shaky_hands→Scav.
- [ ] Plugin: remover Enables Mobile/Overladen.
- [ ] CombatMedicAllyPerks.AllyMobileSurgeon → false.
- [ ] PROPRIEDADES/PROPERTIES.md + build + code-review + commit.
