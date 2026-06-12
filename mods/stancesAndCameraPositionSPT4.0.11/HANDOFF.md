# HANDOFF — stancesAndCameraPositionSPT4.0.11 (itens 004/008/009/010)

> **Data:** 2026-06-11 · **De:** Claude (sessão autônoma) · **Para:** próximo dev
> **HEAD:** `02e68d2` (branch `main`, pushar antes de continuar se ainda local)
> **Build:** ✅ compila (0 erros) · **In-game:** ❌ praticamente nada funciona ainda — ver bloqueio abaixo.

## TL;DR — onde paramos

Uma "leva" de ajustes foi implementada nos itens **004 (Mount)**, **008 (Esvaziar câmara)**, **009 (Wiggle)** e **010 (Manual Chambering)** + Fase 0 (build) + reorg do F12. **Tudo compila e foi commitado/pushado, mas o teste in-game do usuário mostrou que quase nada funciona** — a única coisa confirmada é que o "grude" do mount **parou** de acontecer ao encostar a arma em superfícies (que era o objetivo do passivo, OU sintoma de que o patch nem roda — ver bloqueio).

## 🔴 BLOQUEIO ATUAL — fazer isto PRIMEIRO

**Hipótese forte:** um patch falha no `.Enable()` (target Harmony não resolve em EFT 0.16 — `GClass` volátil ou `method_NN` renomeado) e **aborta o `Awake`**, derrubando todos os patches registrados depois dele. Isso explica "grude parou (o `MountingCollisionPatch` nunca roda) + 007/008/010 mortos". Suspeito nº 1: `FirearmCollisionDetectPatch` (`method_11`), que era o 1º do bloco de mount.

**Já mitigado:** o commit `02e68d2` envolve **cada** `.Enable()` num helper `SafeEnable` ([Plugin.cs](modded/Plugin.cs)) que isola a falha e loga `[enable] OK <nome>` / `[enable] FAIL <nome> -> <exceção>`. Também movi a detecção de superfície do mount para o `Update` (não depende mais do `method_11`).

**PRÓXIMO PASSO (crítico):**
1. Instalar a DLL nova (`/compile-mod stancesAndCameraPositionSPT4.0.11 --flat`) e rodar uma raid.
2. Abrir `D:/SPT/BepInEx/LogOutput.log` e procurar as linhas **`[enable]`**. Qualquer `[enable] FAIL <X>` identifica o patch que não resolve em 0.16 e a exceção.
3. Para o(s) patch(es) que falham: corrigir o target (`GetTargetMethod`) consultando o **Assembly-CSharp 0.16 real** (em `D:/SPT/EscapeFromTarkov_Data/Managed/`) — decompilar com dnSpy/ILSpy ou usar `scratch/DumpSPT`. Os nomes `GClass####`/`method_##` usados foram validados por reflection numa sessão anterior, mas **nomes obfuscados mudam entre builds** — reconfirmar.
4. Habilitar Debug logging no BepInEx (`BepInEx/config/BepInEx.cfg` → `[Logging.Disk] LogLevels = ... Debug`) para ver os logs de comportamento `[Mount]` / `[Wiggle]` / `[ActionStance]` / `[ManualChamber]` e confirmar se os patches **disparam** (não só habilitam).

## Estado por item

| Item | Arquivos | O que deveria fazer | Como testar / suspeita |
|---|---|---|---|
| **004 Mount** | `modded/MountingManager.cs`, `modded/Patches/WeaponMountingPatch.cs` (3 classes + `FirearmCollisionDetectPatch`), `modded/Patches/MountingInputPatch.cs` | Passivo (encostar) = só recoil/sway reduzidos + ícone transparente, **sem grude**. Ativo (tecla nativa de mount do EFT **ou** `Weapon Mounting Hotkey`=Mouse3) = grude + ícone sólido; sair = volta Stance 0 limpa. Detecção de superfície via raycast (`EMountState{None,Passive,Active}`). | Confirmar nos logs `[Mount] None->Passive/Active`. Se nunca aparece "Passive" ao encostar, a detecção (`DetectBracing` no `Update`) não acha superfície → revisar layer mask `LayerMaskClass.HighPolyWithTerrainMask` e os pontos de raycast a partir de `WeaponRootAnim`. |
| **008 Esvaziar câmara** | `modded/Patches/ActionStancePatches.cs` (classe `ActionStanceUnloadChamberPatch`) | Ao esvaziar a câmara, sobe pra Stance 0, executa, volta. | Target `GClass2046.Start()` — **provável `[enable] FAIL`** (GClass volátil). Fim depende de `method_45` (OnIdle) — pode deixar stance presa (finding F5). |
| **009 Wiggle** | `modded/StanceManager.cs` (`RequestWiggle`/`ConsumeWiggleRequest`/`ApplyUserStance`), `modded/Patches/SpringGetPatch.cs` (bloco wiggle + frame-guard) | Tranco na arma só na troca **intencional** de stance (V/scroll/hotkey), não ao colidir/montar. | `SpringGetPatch` é registrado cedo (deve habilitar). Se o wiggle não dispara em troca nenhuma: verificar se `ApplyUserStance` é chamado (logar) e se o `_wiggleThisFrame`/frame-guard em `SpringGetPatch` consome no mesmo frame. **Possível bug de timing** entre `StanceManager.Update` (seta request) e `Spring.Get` (consome). |
| **010 Manual Chambering** | `modded/Patches/ManualChamberingPatches.cs` (port do RealismMod), `modded/Patches/RaidLifecyclePatches.cs` (reset) | Não auto-chamberar no spawn/equip/reload com câmara vazia; puxar ferrolho manual (tecla nativa `ChamberUnload`). | **Maior risco / maior incerteza.** Targets `GClass2055`/`GClass2016`/`method_18` — checar `[enable]`. Se habilitam mas não funcionam: comparar fluxo de flags com o Realism (`mods/RealismMod/Client/DLL descompilada/RealismMod/RealismMod/`). Master toggle `Enable Manual Chambering` = kill-switch (volta vanilla). |

## Como buildar

```bash
# .spt-path na raiz (gitignored) define o SPT install; copiar de .spt-path.example se faltar:
cp .spt-path.example .spt-path        # ajustar SPT_PATH= se != D:/SPT
bash .agents/scripts/compile-mod.sh stancesAndCameraPositionSPT4.0.11 --flat
# Saída: D:/SPT/BepInEx/plugins/shwngFpsCameraStances4.dll
```

> ⚠️ As mudanças do `.agents/scripts/compile-mod.sh` (IMGUIModule + Fika.Core no `resolve_references`, leitura do `.spt-path`) estão **no working tree, NÃO commitadas** — o arquivo tinha trabalho não-commitado da sessão **CustomClasses** (item 019/020). Necessárias para o build; commitar com `git add -p` separando do CustomClasses.

## Rastreabilidade / referências

- **Plano completo:** `~/.claude/plans/backlog-ajustes-de-kind-phoenix.md` (2 rodadas de revisão crítica).
- **Por item:** `backlog/{004,008,009,010}-*/...-06-fix-01.md` (sintoma → causa → correção → critérios → **premissas a validar in-game**).
- **Narrativa da sessão:** `memory/sessions.md` (Sessão 4a madrugada + 4b code-review).
- **Referência funcional (RealismMod 3.11 decompilado):** `mods/RealismMod/Client/DLL descompilada/RealismMod/RealismMod/` — `CollisionPatch.cs` (bracing/raycast, modelo do 004), `KeyInputPatch1/2.cs` (chamber/mount input), `PreChamberLoadPatch.cs`, `StartEquipWeapPatch.cs`, `StartReloadPatch.cs`, `SetAmmoOnMagPatch.cs`.
- **Commits da sessão:** `49d3cf7`…`02e68d2` (todos `(stances)`).

## Findings de code-review NÃO resolvidos (documentados, validar in-game)

- **F3** (010): `JustSpawned` consumido pelo 1º equip — imprecisão se a 1ª arma não for de câmara única.
- **F5** (008): fim do esvaziar-câmara depende de `method_45` (sem callback) → risco de stance "levantada" até correr.
- **F7** (008): fallback ao `MainPlayer` pode disparar swap por peer remoto em Fika.
- **F8** (010): falta guard `Stationary` nos animator-patches (afeta metralhadora montada).

## Decisões/premissas-chave assumidas (podem ser revertidas)

- **004:** sistema de mount **próprio** (não usa o `IsInMountedState` nativo); suprime o mount nativo via cmd 140 exceto com bipé; `method_23` (reset de pose) **omitido** por risco com springs; magnitudes do grude podem precisar re-tuning.
- **010:** `CanLoadChamber` default `false` (era `true` — bug raiz); alinhado ao Realism.
- **Processo:** o pipeline SDD foi cumprido de forma pragmática (gerados `06-fix-01.md` por item em vez de rodar cada slash command isolado).
