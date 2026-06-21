# Mount — Diagnóstico Comparativo + Plano de Correção

**Mod:** stancesAndCameraPositionSPT4.0.11 (`modded-beta`) · **Data:** 2026-06-21
**Sintoma:** mount não ativa ao aproximar/encostar a arma em superfícies do mapa; ícones não aparecem. Nunca funcionou nesta linha.
**Objetivo:** mount deve funcionar nas mesmas superfícies do mount vanilla (pedra, árvore, parede). **Passivo** = buffs (stamina/recoil/sway) ao encostar; **Ativo** = "grude" (estilo vanilla), por tecla.

> Fontes cruzadas: vanilla EFT 0.16 (`references/eft-decompiled/`), RealismMod 0.14.8 decompilado (`mods/RealismMod/Client/DLL descompilada/`), nossa impl (`modded-beta/`).

## 1. Como funciona em cada sistema

### Vanilla EFT 0.16
- **Detecção/oclusão:** `Player.FirearmController.method_11(Vector3 origin, float ln, ref bool overlapsWithPlayer, Vector3? weaponUp)` ([Player.cs:12814](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12814)); chamado em [Player.cs:12966](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12966) com `WeaponLn`. Layer = `EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS` ([EFTHardSettings.cs:251](../../../references/eft-decompiled/Assembly-CSharp/EFTHardSettings.cs#L251)).
- **Ativação:** `Player.TryMountWeapon()` ([Player.cs:26218](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218)) via `ECommand.WeaponMounting`; exige `Weapon.IsMountable`, `IsGrounded`, não-reload/spawn/interaction.
- **Estado:** `MovementContext.IsInMountedState => PlayerMountingPointData.MountPointData != null` ([MovementContext.cs:1502](../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1502)); ergonomia/drain alterados por `MountingBonusErgo`/`BipodBonusErgo` ([Player.cs:12834](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12834)).

### RealismMod 0.14.8 (referência funcional)
- **Ponto de entrada:** PREFIX em `FirearmController.method_11` ([CollisionPatch.cs:41,209](../../RealismMod/Client/DLL%20descompilada/RealismMod/RealismMod/CollisionPatch.cs#L209)) com assinatura **3 params** `(origin, ln, weaponUp)`. Chama `DetectBracing(fc, player, ln)`.
- **Detecção** ([CollisionPatch.cs:100](../../RealismMod/Client/DLL%20descompilada/RealismMod/RealismMod/CollisionPatch.cs#L100)): timer a cada 60 chamadas; origem = `WeaponRootAnim.position`; `up = WeaponRootAnim.TransformDirection(Vector3.up)`; 3 linecasts (Top/Left/Right) + OverlapSphere; offsets `_startDownDir(0,0,-0.19)`, `_startLeftDir(0.143,0,0)`, esferas raio 0.045/0.09; layer `WEAPON_OCCLUSION_LAYERS`.
- **UI:** POSTFIX em `EftBattleUIScreen.Show` cria/anexa o ícone; `MountingUI.Update` escolhe sprite por direção e alpha por estado.

### Nossa impl (`modded-beta`)
- **DOIS caminhos de detecção** (redundância): (a) PREFIX `FirearmCollisionDetectPatch` em `method_11` ([WeaponMountingPatch.cs:15](Patches/WeaponMountingPatch.cs#L15)); (b) `MountingManager.Update` ([MountingManager.cs:106](MountingManager.cs#L106)). Ambos chamam `MountingManager.DetectBracing` ([MountingManager.cs:152](MountingManager.cs#L152)) — **geometria copiada fielmente do Realism** (mesmos offsets/raios/layer).
- **Estado próprio:** `EMountState {None,Passive,Active}`. Passivo = buffs (recoil/sway via `AddRecoilForceMountPatch`/`WeaponMountingPatch`); Ativo = grude (`MountingCollisionPatch` em `AvoidObstacles`).
- **UI:** `BattleUIScreenPatch` POSTFIX em `EftBattleUIScreen.Show` ([MountingUI.cs:98](MountingUI.cs#L98)).

## 2. O que está CONFIRMADO OK (descartado como causa)
- Todos os patches de mount **habilitam** sem `[enable] FAIL` (log).
- `MountingManager`/`MountingUI` **são instanciados** ([Plugin.cs:373,378](Plugin.cs#L373)); Awake termina.
- Sprites carregam (sem `[ResourceLoader] not found`); `_EnableWeaponMounting` default **true**.
- `WEAPON_OCCLUSION_LAYERS` existe em 0.16 e é o layer correto.
- `method_11` patchado tem param `ln` (Harmony injeta), senão o `.Enable()` teria falhado → mira um overload com `ln`.

## 3. Gaps / suspeitos (ranqueados) — a confirmar in-game
| # | Suspeito | Por quê | Confirmação |
|---|---|---|---|
| **G1** | **Detecção roda mas não acha superfície** | geometria copiada de 0.14.8; `WeaponRootAnim` (origem/orientação) pode diferir em 0.16, ou os linecasts não alcançam | log em `DetectBracing`: `CheckCover` sempre `false` ao encostar |
| **G2** | **`method_11` — overload/assinatura** | assinatura mudou (0.16 tem `ref bool overlapsWithPlayer`); `AccessTools.Method` sem assinatura pode pegar overload errado ou disparar em contexto diferente | log no `Prefix`: dispara? com que `ln`? |
| **G3** | **UI não aparece mesmo com detecção OK** | `BattleUIScreenPatch` patcha método de classe genérica (warning HarmonyX) — porém o Realism faz igual e funciona (provável benigno); ou `EftBattleUIScreen.Show` mudou em 0.16 | log no `PatchPostFix` (attach) e `MountingUI.Update` (ActiveUIScreen, MountState, color) |
| **G4** | **`ln` inconsistente entre os 2 caminhos** | `Update` usa `CalculateCellSize().X*0.1+0.15`; `method_11` usa `ln` real; cooldown faz um "ganhar" | log do `ln` em cada caminho |
| **G5** | Buffs aplicam mas são imperceptíveis | usuário não "sente" e acha que não funciona | confirmar transição de estado no log |

## 4. Melhorias de design (independente da causa)
- **Unificar a detecção num só caminho** (preferir o PREFIX `method_11` com o `ln` real do EFT; remover a duplicação no `Update`, ou vice-versa). Hoje há dois com `ln` divergente.
- **Especificar a assinatura** no `GetTargetMethod` (`AccessTools.Method(type, "method_11", new[]{typeof(Vector3), typeof(float), typeof(bool).MakeByRefType(), typeof(Vector3?)})`) para garantir o overload certo e resistir a renumeração.
- **Promover `method_NN` a resolução por assinatura** (não por número) — antipattern AP do repo.
- Após funcionar: mover o log de transição de `LogDebug` para um toggle de debug visível (hoje `[Mount]` nunca aparece).

## 5. Plano de diagnóstico (g-diagnose)
**Fase A — Instrumentar (1 build):** logs `Info [DBG-mnt]` em: `FirearmCollisionDetectPatch.Prefix` (dispara? `ln`?), `MountingManager.Update` (roda? `fc` ok?), `DetectBracing` (entrada + resultado de cada `CheckCover`), `SetMountState` (transição — trocar `LogDebug`→`Info` temporário), `BattleUIScreenPatch.PatchPostFix` (attach), `MountingUI.Update` (estado/cor). Throttle p/ não floodar.

**Fase B — Teste in-game (1 raid):** ir até parede/pedra/janela, encostar a arma; capturar `LogOutput.log`.

**Fase C — Localizar a quebra pelo log:**
- `Prefix`/`Update` não logam → patch/MonoBehaviour não roda (G2).
- logam mas `DetectBracing` não chama → guard (`fc` null).
- `DetectBracing` roda mas `CheckCover` sempre false → geometria/origem/layer (G1) → comparar `WeaponRootAnim` em 0.16, visualizar raycasts.
- `SetMountState(Passive)` ocorre mas ícone não → UI (G3).

**Fase D — Corrigir o estágio isolado + validar in-game** (lembrar: Dev Mod on / confirmar via marcador de versão no log).

## Histórico
| Data | Evento |
|---|---|
| 2026-06-21 | Diagnóstico comparativo (vanilla/Realism/nosso) + plano criado. Detecção é cópia fiel do Realism; causa exige instrumentação in-game. |
