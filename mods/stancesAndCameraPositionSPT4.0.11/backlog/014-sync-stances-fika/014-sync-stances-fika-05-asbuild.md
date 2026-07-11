# 014 — Corrigir sync visual de stances no Fika · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-22
**Spec funcional:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Spec técnica:** [014-sync-stances-fika-02-spec-tech.md](014-sync-stances-fika-02-spec-tech.md)
**Reviews técnicas:** [03-spec-tech-review-01.md](014-sync-stances-fika-03-spec-tech-review-01.md) (0 🔴)

> Corrige a aplicação remota: o offset de stance passa a ser escrito no `HandsContainer.WeaponRootAnim` do jogador observado (braço **e** arma juntos), aditivo sobre a pose nativa (lean/ombro/mira), em vez de girar o `PlayerBones.Spine3`. Networking inalterado. Compila 0 erros; **aguarda validação in-game (2 clientes Fika)**.

## Arquivos alterados

| Ação | Arquivo | Resumo |
|---|---|---|
| MODIFICADO | `modded/Networking/ObservedStanceAnimator.cs` | Removido `LateUpdate`/Spine3. Virou state-holder por player + `ApplyTo(pwa, weaponPosition, weapRotation, dt)` que avança o spring e escreve o `WeaponRootAnim` (mesma fórmula do local). |
| MODIFICADO | `modded/Patches/ApplyComplexRotationPatch.cs` | Gate `!IsYourPlayer` desviado para `ObservedStanceAnimator.ApplyTo` (com safeguards), antes do bloco MainPlayer. `SpringLerpAngle`/`SpringLerp` agora `public`. |
| MODIFICADO | `modded/StanceManager.cs` | Overloads `GetTargetRotation(Stance,bool)` / `GetTargetPosition(Stance,bool)`; os existentes delegam com `CurrentStance`. |

> Networking (`FikaSyncManager`/`StanceSyncPacket`) **inalterado** — já enviava `ProfileId`+`Stance`+`IsAiming` corretamente.

## Pontos da review tratados no build

| ID | Resumo | Como foi tratado |
|---|---|---|
| PA-01-01 | Timing Postfix × cópia PlayerBones | Mantido (Postfix de `ApplyComplexRotation` dentro do `ProcessEffectors`, antes da cópia). **Validar in-game**; plano B documentado. |
| PA-01-02 | Coexistência lean/ombro | Offset aditivo sobre `weapRotation` (pose nativa). **Validar combinações in-game.** |
| PA-01-03 | Edição de `RaidLifecyclePatches` desnecessária | Removida do escopo (estado nos components). |

## Pendências de validação in-game (2 clientes Fika, antes de 🟢)

- O outro player vê a **arma acompanhar** a stance (não só o tronco); pose remota = pose local.
- **Stance + lean** (esq/dir) e **stance + troca de ombro** combinam sem conflito; sequências nos dois sentidos.
- Lean/ombro **vanilla** seguem funcionando.
- ADS + stance no remoto; troca rápida; vários remotos; spawn/despawn/morte sem órfão/erro.

## Evolução pós-build (fixes + code-review 02)

A arquitetura migrou após a validação in-game (ver fixes): `ApplyComplexRotation`/`ProcessEffectors` não chegavam à render do observado — o `Kinematics` (ObservedPlayer.cs:1889) sobrescrevia o `Weapon_Root_Anim`. Solução atual em [06-fix-02](014-sync-stances-fika-06-fix-02.md): **Postfix de `ObservedPlayer.ObservedVisualPass`** (`ObservedStanceVisualPatch`) escrevendo no transform FINAL `PlayerBones.Weapon_Root_Anim`.

Code review 02 ([04-code-review-02.md](014-sync-stances-fika-04-code-review-02.md)) aplicado em 2026-07-09:

| Ação | Arquivo | Resumo |
|---|---|---|
| MODIFICADO | `modded/Networking/ObservedStanceAnimator.cs` | CR-02-01: guarda anti-acúmulo (`_lastWrittenRot/Pos/_hasWritten`) — pula o frame se o vanilla não re-setou o `Weapon_Root_Anim` (early-return de `ObservedVisualPass`). |
| MODIFICADO | `modded/StanceManager.cs` | CR-02-02: `TickAdsNetworkSync()` + `_lastSentAiming` — reenvia o stance ao mirar/desmirar sem trocar de stance. |
| MODIFICADO | `modded/Plugin.cs` | CR-02-02: chamada de `TickAdsNetworkSync()` no `Update`. |
| REMOVIDO | `modded/FikaNetworkSync.cs` | CR-02-04: sistema de sync legado morto (2º `StanceSyncPacket` conflitante; `Init` nunca chamado). |
| REMOVIDO | `modded/PlayerStanceController.cs` | CR-02-04: experimento abandonado, só referenciado pelo `FikaNetworkSync` morto. |

> **Build (após reorganização 2026-07-09):** `modded/` é o fork **canônico** (a versão antiga foi para `modded-bak/`). `/compile-mod` agora resolve `modded/` corretamente, e o csproj puxa `Fika.Core` da raiz `references/` (build self-contained, sem `mods/references/` temporário). **Deploy:** o DLL roda de `D:/SPT/BepInEx/plugins/RealisticMobility/` (junto dos assets `.ogg`/`.png`) — cópia manual, pois o `/compile-mod` instala em `plugins/<AssemblyName>/`. DLL atual: hash `08cde2584d6e`, 138752 B.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Build concluído via `/code-mod` — compila 0 erros; status 🟡 (aguarda validação in-game) |
| 2026-06-22 | Code review 01 (2 revisores independentes) + aplicação: clamp do stance de rede (CR-01-01). Recompila 0 erros. |
| 2026-07-09 | Code review 02 (validação por referências) + aplicação: CR-02-01 (anti-acúmulo), CR-02-02 (sync ADS), CR-02-04 (remoção de código morto). CR-02-03/05/06 deferidos. Build manual do `modded` 0 erros; instalado em `RealisticMobility/` (hash `08cde2584d6e`). Aguarda validação in-game (2 clientes Fika). |
