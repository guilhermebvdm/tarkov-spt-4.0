# 050 — As-built · Fatia 050.0 (Infra + Bulwark + Pack Mule)

**Mod:** CustomClasses · **Data:** 2026-06-22 · **Status:** 🔵 050.0 compila + instalado; pendente validação in-game
**Refs:** [01-spec](./050-signature-patches-01-spec.md) · [02-spec-tech](./050-signature-patches-02-spec-tech.md)

## O que foi feito (050.0)

| Arquivo (modded/Client) | Mudança |
|---|---|
| `SkillMultipliers.cs` | + `ClassNameEn` (chave EN estável) + `IsLocalClass(nameEn)` (gating idioma-independente) |
| `PerksConfig.cs` *(novo)* | F12: Bulwark (Enabled, DamageTaken=0.85) · Pack Mule (Enabled, CarryLimitBonus=0.30) |
| `Patches/BulwarkPatch.cs` *(novo)* | Prefix `Player.ApplyDamageInfo` → `damageInfo.Damage *= 0.85` se MainPlayer local + classe Tank |
| `Patches/PackMulePatch.cs` *(novo)* | Postfix getter `SkillManager.CarryingWeightRelativeModifier` → piso `1.30` se Skills do MainPlayer + Scavenger/Tank |
| `Plugin.cs` | `PerksConfig.Bind(Config)` + `.Enable()` dos 2 patches |

## Verificação

- **`compile-mod.sh CustomClasses` → ✅ Build OK, 0 erros / 0 avisos** (client `CustomClasses-Client.dll` + server). Instalado em `BepInEx/plugins/CustomClasses`.
- Gating idioma-independente (`classNameEn` = `name`); só MainPlayer local (não bots); F12 lido no apply-time.

## 🔴 Achado importante (escalado) — install diverge do repo

O guard anti-clobber (item 019) **pulou a cópia do `config/`**:
- **`cacador/fuzileiro/medicoDeCombate/saqueador/tanque.jsonc` — install MAIS NOVO que o repo** → as edições do **editor web foram pro install, não pro repo**. ⚠️ A "matriz conferida 1:1" foi contra o **repo (possivelmente stale)**.
- **`furtivo.jsonc` repo-only · `fantasma.jsonc` install-only** → o rename 054 está no repo mas **NÃO no install** (o jogo ainda tem "Ghost"/"Fantasma").

**Não rodei `sync-classes`/`--force-config`** (risco de clobber das edições do editor OU de perder o rename) — decisão de reconciliação = humana.

## Pendente (gate humano/externo)

1. **Reconciliar install↔repo:** `/sync-classes` (puxa as edições do editor pro repo) → re-aplicar o rename 054 → propagar pro install. Confirmar editor web fechado.
2. **Re-verificar a matriz** contra os configs reconciliados (a conferência 1:1 foi vs repo stale).
3. **Validação in-game do 050.0:** Tanque perde ~15% menos HP num hit; Saqueador/Tanque +30% no limite de peso; zero efeito em outras classes; F12 muda ao vivo.

> **050.0 (perks Bulwark/Pack Mule) NÃO depende do rename 054** — gateiam em `Tank`/`Scavenger` (nomes inalterados). Dá pra validar já.

## UI (2026-06-22) — Pack Mule no stash + notificação de raid

| Arquivo (modded/Client) | Mudança |
|---|---|
| `Patches/PackMulePatch.cs` | gate ajustado: **na raid** só MainPlayer; **fora da raid** (GameWorld null) gateia só pela classe → **+30% reflete no stash**. |
| `PerksCatalog.cs` *(novo)* | catálogo **bilíngue EN/pt-br** de perks/drawbacks por classe (chave `name`); base tb p/ o 053. `BuildNotificationText()` (perks verde / drawbacks vermelho via `MultiplierFormat`). |
| `Patches/RaidPerksNotificationPatch.cs` *(novo)* | Postfix `GameWorld.OnGameStarted` → **delay 3s (pós-load)** → `NotificationManagerClass.DisplayMessageNotification` (1 notificação multilinha). Ignora hideout. |
| `PerksConfig.cs` | + `ShowRaidPerksNotification` (F12, default on). |
| `Plugin.cs` | `.Enable()` do patch novo. |

Build ✅ (0 erros). **Pendente in-game:** ver checklist abaixo.

### 🟡 Pendente — indicador (setinha ▲ verde + tooltip) no peso do stash (Feature 1, parte visual)
O **funcional** (peso +30% no stash) **está feito**. Falta a **setinha verde pra cima + tooltip** no número do peso. *(Correção do usuário: é **setinha**, não seringa.)*

**Padrão CONFIRMADO (reuso do `SkillPanelPatch`):** marcador = `GameObject` filho com `TextMeshProUGUI` (`MultiplierFormat.Marker(f)` → "▲ +X%") + `HoverTooltipArea.Init(ItemUiContext.Instance.Tooltip, txt, rawText:true)` / `SetMessageText` → **mesmo tooltip da tela de Skills**. Gate: `PerksConfig.PackMuleEnabled` + `SkillMultipliers.IsLocalClass("Scavenger"|"Tank")`.

**Recon que falta (bloqueio):** o **elemento de UI do peso no stash NÃO está no decompile curado**. Achar a classe/campo TMP que mostra o peso total (a que assina `OnTotalWeightUpdated` / lê `Inventory.TotalWeight`) — provável `EFT.UI` (inventory/stash panel). Ferramentas confirmadas: **`ilspycmd`** (`/c/Users/guime/.dotnet/tools/ilspycmd`) sobre **`/d/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`** → decompilar a UI do inventário e localizar o painel/refresh do peso. *(Symbol-grep rápido foi inconclusivo → precisa do decompile completo dessa área.)*

**Implementação (depois do recon):** Postfix no refresh do painel de peso → criar/atualizar o marcador ▲ ao lado do TMP do peso (igual `SkillPanelPatch.GetOrCreateMarker`), tooltip = "Pack Mule: +30% carry / +30% de carga". **Deferido** (UI visual = gate de validação in-game + decompile pesado).

### ⚠️ Riscos a validar in-game (Feature 2)
- **AP-03:** `GameWorld.OnGameStarted` é virtual — se o GameWorld da raid **sobrescreve**, o Postfix na base pode não disparar (a notificação não aparece). Confirmar in-game; se não aparecer, patchar o override.
- Duração/multilinha da notificação (default) — ajustar se sumir rápido.

## Fatia 050.1 (2026-06-22) — movimento/inércia ✅ (4/4, compila)

| Arquivo | Mudança |
|---|---|
| `Patches/ClassMovementPatches.cs` *(novo)* | `MaxSpeedPatch`+`SprintSpeedPatch` (postfix `MovementContext.MaxSpeed/SprintSpeed`, gate MainPlayer+classe) + `OverladenInertiaPatch` (postfix `BasePhysicalClass.OnWeightUpdated`→`Inertia`). |
| `PerksConfig.cs` | + Heavy Frame (Enabled, MoveSpeed=0.90) + Overladen (Enabled, Inertia=1.50). |
| `Plugin.cs` | `.Enable()` dos 3 patches. |

- ✅ **Heavy Frame** (Tanque −10% vel) e **Overladen** (Saqueador inércia ∝ peso) — compila (0 erros), instalado. Compõem com o stances (postfix-mult).
- ✅ **Rooted** (Caçador −15% vel em ADS — gate `HandsController is Player.FirearmController && IsAiming`) e **Execution** (Furtivo +10% vel c/ melee na mão — gate `HandsController is Player.KnifeController`) — adicionados ao `ClassMoveSpeed.Apply` + F12. Compila.
- **Validação in-game (gate):** Tanque anda −10%; Saqueador carregado fica mais "clunky" (inércia); zero efeito em outras classes; F12 ao vivo.

## Fatia 050.2 (2026-06-23) — recuo/aim-punch + Adrenaline ✅ (4/4, compila)

> Recon spike (subagente, decompile do `D:/SPT`) achou os pontos: `PWA.Shoot(str)` (recuo), `ForceEffector.AddForce` (aim-punch de hit), `FirearmController.GetWeaponReloadAnimationSpeed` (recarga), `PWA.UpdateWeaponVariables`→`_aimingSpeed` (ADS). `Player.ApplyDamageInfo` (gatilho).

| Arquivo (modded/Client) | Mudança |
|---|---|
| `Patches/ClassWeaponPatches.cs` *(novo)* | `ShootRecoilPatch` (prefix `PWA.Shoot(ref str)`: Shaky Hands ×1.25 + Adrenaline ×0.7) · `AimPunchPatch` (prefix `ForceEffector.AddForce`: Rattled ×1.5) · `ReloadSpeedPatch` (postfix `GetWeaponReloadAnimationSpeed`) · `AdsSpeedPatch` (postfix `UpdateWeaponVariables`→`_aimingSpeed`). |
| `AdrenalineState.cs` *(novo)* | state-machine `Time.time` (janela 25s renovável / cd 120s; auto-expira entre raids). |
| `Patches/AdrenalineTriggerPatch.cs` *(novo)* | postfix `Player.ApplyDamageInfo` → `Trigger()` se **causar** (`damageInfo.Player==local`) ou **receber** (`__instance==local`) dano (Rifleman). |
| `PerksConfig.cs` | + Shaky Hands, Rattled, Adrenaline (6 entries: Enabled/Window/Cooldown/Recoil/Reload/ADS). |
| `Plugin.cs` | `.Enable()` (Adrenaline dentro de try/catch — injeção de campo privado pode falhar em runtime). |

- ✅ **Shaky Hands** (Médico recuo ×1.25) · ✅ **Rattled** (Furtivo aim-punch ×1.5) · ✅ **Adrenaline** (Fuzileiro: recuo ×0.7 + recarga ÷0.8 + ADS ÷0.8 na janela).
- ✅ **Cool Under Fire** (Fuzileiro): **re-escopado** (decisão do usuário 2026-06-23) — supressão não existe no cliente → vira **−50% de flinch ao levar dano** (mesmo `ForceEffector.AddForce`, branch oposto ao Rattled). A parte de **anti-jam ×0.5** segue planejada no 050.3.
- ⚠️ **Gates de RUNTIME (compile não pega):**
  1. `____aimingSpeed` (injeção de campo privado `_aimingSpeed`): se o nome divergir, o patch de ADS não aplica (try/catch loga, resto segue). Validar que o ADS acelera na janela.
  2. `damageInfo.Player` como atacante: confirmar que **causar dano** dispara a Adrenaline (senão cai pra só "receber dano").
  3. `GetWeaponReloadAnimationSpeed`: recon alertou que pode ser consumido animator-side — validar se a recarga realmente acelera (fallback: `SetSpeedParameters`/`SetSpeedReload`).

## Fatia 050.3 (2026-06-23) — combate/saúde ✅ (3/4, compila)

> Recon spike (subagente, decompile do `D:/SPT`): `Player.ApplyDamageInfo` (melee, `DamageType==Melee` + atacante local), `ActiveHealthController.ChangeEnergy/ChangeHydration` (drain), `FirearmController.GetTotalMalfunctionChance` (jam).

| Arquivo (modded/Client) | Mudança |
|---|---|
| `Patches/ClassCombatHealthPatches.cs` *(novo)* | `ExecutionMeleePatch` (prefix `ApplyDamageInfo`: melee ×5 se atacante local + Stealth) · `ChangeEnergyPatch`+`ChangeHydradationPatch` (prefix, drain ×1.3 só local Tank, `value<0`) · `MalfunctionChancePatch` (postfix `GetTotalMalfunctionChance` ×0.5, Rifleman). |
| `Patches/AdrenalineTriggerPatch.cs` | **FIX:** "causar dano" agora compara `damageInfo.Player.iPlayer.ProfileId` (era `ReferenceEquals` num `IPlayerOwner` → nunca disparava). |
| `PerksConfig.cs` | + Heavy Frame hunger/thirst (1.3), Execution melee (Enabled, 5.0), Cool Under Fire malf chance (0.5). |
| `Plugin.cs` | `.Enable()` (metabolism em try/catch — tipo do DLL). |

- ✅ **Execution** melee ×5 (Furtivo) · ✅ **Heavy Frame** fome/sede ×1.3 (Tanque) · ✅ **Cool Under Fire** anti-jam ×0.5 (Fuzileiro).
- 🟡 **Combat Medic (Médico) DEFERIDO** — med use ×0.7 + cirurgia ×0.5 estão em `ActiveHealthController.DoMedEffect`, mas a duração é **var local** (precisa **transpiler** ou patchar `HealthEffectsComponent.UseTimeFor` + `FirearmsAnimator.SetUseTimeMultiplier` p/ casar efeito+animação). O **"cirurgia sem lock de movimento" não foi localizável no estático** (provável animação full-body) → precisa **investigação em runtime**. É a única feature do 050.3 que sobra.
- ⚠️ **Gate de runtime (compile não pega):** o fix do `damageInfo.Player.iPlayer.ProfileId` (Adrenaline + melee) — confirmar in-game que "causar dano" dispara/escala.

## Fatia 050.4 (2026-06-23) — som/arma/handling ✅ (6/7, compila)

> Recon spike (subagente): som via `Player.method_67` (raio de audibilidade — funil de movimento) + `Player.PlayInteractionSound` (loot); ergo via getter `FirearmController.TotalErgonomics`; ADS via `_aimingSpeed` (já tinha); Iron Lungs via `PlayerPhysicalClass.method_12` (consumo O₂).

| Arquivo (modded/Client) | Mudança |
|---|---|
| `Patches/ClassSoundPatches.cs` *(novo)* | `SoundRadiusPatch` (postfix `method_67`: Ghost Step ×0.4 / Loud Operator ×1.3) · `InteractionSoundPatch` (prefix `PlayInteractionSound`: Silent Looter ×0.4). |
| `Patches/ClassWeaponPatches.cs` | + `HeavyWeaponErgoPatch` (postfix `TotalErgonomics` ×1.15, Tank+pesada) · branch Bunker recuo ×0.85 no `ShootRecoilPatch` · branch Sharpshooter ADS no `AdsSpeedPatch` (reestruturado gate-first) · helper `HeavyWeapon` (weapClass machinegun/grenadeLauncher). |
| `Patches/ClassCombatHealthPatches.cs` | + `IronLungsPatch` (postfix `PlayerPhysicalClass.method_12` ×0.5 O₂, Hunter). |
| `PerksConfig.cs` | + Ghost Step, Loud Operator, Silent Looter, Bunker (recoil+ergo), Sharpshooter, Iron Lungs. |
| `Plugin.cs` | `.Enable()` (method_67 / ergo getter / method_12 em try/catch — obfuscados/aninhados). |

- ✅ **Ghost Step** (Furtivo, som ×0.4) · ✅ **Loud Operator** (Fuzileiro, som ×1.3) · ✅ **Silent Looter** (Saqueador, loot ×0.4) · ✅ **Bunker** (Tanque, recuo ×0.85 + ergo ×1.15 c/ arma pesada) · ✅ **Sharpshooter** (Caçador, ADS ×0.85) · ✅ **Iron Lungs** (Caçador, fôlego ~2×).
- 🟡 **Quick Hands (Saqueador) DEFERIDO** → "Search Double" é **server-side** (buff de skill `SearchDouble`); o lever client (`CanStartNewSearchOperation`) pode ser re-validado pelo servidor. **Melhor ativar via server mod** (coordenar com a sessão do editor).
- 🟡 **Iron Lungs sway DEFERIDO** (a duração foi feita; o sway é `BreathEffector.Process` — output reescrito todo frame + injeção de campo frágil).
- ⚠️ **Gates de runtime:** `method_67` (raio de som realmente muda?), `method_12` (fôlego dobra?), `TotalErgonomics` postfix (ergo aplica?), weapClass "machinegun"/"grenadeLauncher" (bater com a DB do server).

## Ferramenta 052 — Perk Diagnostics Overlay (2026-06-24)

> "Super espião" pra validar os efeitos **sutis/obfuscados** solo, sem depender de "sentir".

| Arquivo | Mudança |
|---|---|
| `PerkDiagnostics.cs` *(novo)* | `PerkDiagnostics.Draw()` — overlay `OnGUI` lendo ao vivo do MainPlayer: MaxSpeed/Sprint, Inertia, CarryMod, **TotalErgonomics+weapClass**, **AimingSpeed**, estado da Adrenaline. + `PerkDiag` (valores de evento: recuo/som/jam). |
| `Plugin.cs` | `OnGUI()` → `PerkDiagnostics.Draw()`. |
| `PerksConfig.cs` | + `DiagnosticsEnabled` (seção **Diagnostics**, F12, default OFF). |
| `AdrenalineState.cs` | + `SecondsLeft`/`OnCooldown` (label do overlay). |
| `ClassWeaponPatches.cs` · `ClassSoundPatches.cs` · `ClassCombatHealthPatches.cs` | capturam `PerkDiag.LastRecoil/LastSound/LastMalfunction` quando diag on. |
| `.csproj` | + ref `UnityEngine.IMGUIModule` (GUIStyle/OnGUI). |

- **Uso:** F12 → *Diagnostics* → **Perk Diagnostics overlay** ON → entra na raid → overlay no topo-esquerdo. Troca o toggle de um perk no F12 → **o número pula** (prova patch+gate+valor). Cobre justo os pontos de risco de runtime (`TotalErgonomics`, `AimingSpeed`, `method_67`, `GetTotalMalfunctionChance`).
- Compila 0 erros, instalado.

## Checklist de validação in-game (050.0 + 054) — quando puder testar

> Pré: rebuildar (`compile-mod CustomClasses`) e reiniciar server + cliente. Compilar ≠ funcionar (AP-06).

**050.0 — Bulwark (Tanque):**
- [ ] Como **Tanque**, leve um hit conhecido (mesma arma/munição/distância) e compare o HP perdido com uma classe sem o perk → deve ser **~15% menor**.
- [ ] Como **outra classe** (ex.: Médico), o mesmo hit → dano **normal** (zero efeito) = gating ok.
- [ ] F12: mudar `Bulwark — Damage taken` (ex.: 0.5) **no meio da raid** → próximo hit já reflete (apply-time).
- [ ] Desligar `Bulwark — Enabled` no F12 → dano volta ao normal na hora.

**050.0 — Pack Mule (Saqueador + Tanque):**
- [ ] Como **Saqueador** ou **Tanque**, o limite de peso antes de *overweight* está **+30%** (~50→65 kg).
- [ ] Como outra classe → limite **normal** (gating ok). Bots não afetados.
- [ ] Não soma com Strength (piso): com Strength alta, o efetivo continua **+30%** (não +60%).

**054 — rename Furtivo:**
- [ ] Launcher mostra a edição **Furtivo** (pt) / **Stealth** (en) no lugar de Fantasma/Ghost.
- [ ] Criar perfil novo dessa classe funciona; viewer do editor mostra o novo `name`.

> ⚠️ **Pré-requisito do teste:** propagar os configs ao install (o compile pulou o `config/` por divergência). Como o repo já foi reconciliado (sync), um `compile-mod --force-config` agora empurra repo→install **com segurança** (o repo virou a verdade). Confirmar editor web fechado antes.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-22 | Guilherme | 050.0 implementado (gating + F12 + Bulwark + Pack Mule). Build ✅. Achado: install diverge do repo (editor edits não sincronizados; rename 054 não propagado). |
