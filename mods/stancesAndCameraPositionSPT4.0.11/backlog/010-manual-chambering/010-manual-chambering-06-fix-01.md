# 010 — Manual Chambering · 06-fix-01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-11
**Status:** 🟡 Implementado — requer validação in-game
**Origem:** documento de produto do usuário (Backlog — Ajustes). Item implementado fora do fluxo formal na sessão paralela; este é o primeiro artefato de backlog.

## Sintoma

A arma ainda carrega a primeira bala automaticamente nos cenários em que o comportamento desejado é exigir ação manual do jogador (puxar o ferrolho).

## Causa raiz

A implementação era uma **port direta do RealismMod 1.6.4 (SPT 3.11)** sem adaptação ao EFT 0.16. Comparando com a referência decompilada em `mods/RealismMod/Client/DLL descompilada/RealismMod/RealismMod/`:

1. **`ManualChamberingState.CanLoadChamber` default `true`** — o Realism usa `false` ([Plugin.cs:49](../../../RealismMod/Client/DLL%20descompilada/RealismMod/RealismMod/Plugin.cs#L49)). Com `true`, a condição de carregar a bala no `StartEquipWeapPatch` era satisfeita → auto-chamber no equip/spawn. **Esta é a causa central do sintoma.**
2. **`PreChamberLoadPatch` (`method_18`) mutava `CanLoadChamber = false`** — o Realism só seta `BlockChambering = true`. A mutação extra deixava estado residual entre operações.
3. **Faltava o `StartReloadPatch`** (reset de flags no início do reload) presente no Realism — sem ele, estado residual podia travar o reload (risco de softlock).
4. **Sem reset de estado entre raids** — flags estáticas vazavam para a raid seguinte.
5. **Sem toggles separados por cenário** (o produto pedia início-de-raid e reload separados).

## Correção (alinhada ao RealismMod + APIs 0.16 validadas)

| Arquivo | Mudança |
|---|---|
| `modded/Patches/ManualChamberingPatches.cs` | `CanLoadChamber` default `false` + `ManualChamberingState.Reset()` + `JustSpawned`; `PreChamberLoadPatch` só seta `BlockChambering` (gate `_ManualChamberingOnReload`); `StartReloadMagBlockPatch` → `StartReloadResetPatch` (reset, espelha Realism); `StartEquipWeapPatch` distingue spawn (cenário 1, gate `_ManualChamberingOnRaidStart`) de equip mid-raid (cenário 2); logs `[ManualChamber]` por gatilho. |
| `modded/Patches/RaidLifecyclePatches.cs` | `OnGameStarted`: `Reset()` + `JustSpawned = true`; `OnDestroy`: `Reset()`. |
| `modded/Plugin.cs` | Configs `_ManualChamberingOnRaidStart` + `_ManualChamberingOnReload`; registro `StartReloadResetPatch`; descrição do master toggle como kill-switch. |

## APIs (validadas Assembly 0.16, ver plano §APIs)

`GClass2055` (equip/chamber), `GClass2016` (reload), `method_18` (auto-chamber), `ECommand.ChamberUnload (92)` (input manual).

## Critérios de aceite

- [ ] Início de raid com câmara vazia + `OnRaidStart=on` → não chambera; puxar ferrolho carrega.
- [ ] Equipar/trocar arma mid-raid com câmara vazia → não chambera (master toggle).
- [ ] Reload com câmara vazia + `OnReload=on` → não chambera após inserir mag.
- [ ] Comando manual (`ChamberUnload`) sempre funciona; sem softlock.
- [ ] Master toggle off → vanilla em todos os cenários (kill-switch).
- [ ] Sem resíduo entre raids/troca de arma/morte.

## Premissas assumidas (decididas autonomamente — validar in-game)

1. **Default `CanLoadChamber=false`** é a correção central (alinha ao Realism); explica o sintoma.
2. **`_ManualChamberingOnReload` aplicado no `method_18`** é uma aproximação do cenário 3 — `method_18` é o auto-chamber genérico (predominante em reload), pois não há ponto-só-reload limpo em 0.16. Pode afetar marginalmente outros caminhos de auto-chamber.
3. **`JustSpawned`** distingue spawn de equip mid-raid; é consumido no primeiro equip pós-`OnGameStarted`.
4. **Maior incerteza do lote.** Risco de softlock se os targets `GClass2055/2016`/`method_18` divergirem do esperado em 0.16. Mitigação: o master toggle restaura o vanilla; os logs `[ManualChamber]` revelam qual gatilho dispara.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Claude (autônomo) | 06-fix-01: alinhamento ao RealismMod decompilado + default `CanLoadChamber=false` + reset de estado + toggles por cenário. Não testado in-game. |
