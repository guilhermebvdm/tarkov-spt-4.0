# 011 — Fix 01 · Coordenador único de stamina de braço (Perfil B)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [011-mount-passivo-vanilla-01-spec.md](011-mount-passivo-vanilla-01-spec.md)
**Asbuild:** [011-mount-passivo-vanilla-05-asbuild.md](011-mount-passivo-vanilla-05-asbuild.md)
**Criado:** 2026-06-21
**Disparado por:** feedback in-raid — conflito de gestão de stamina após entregar o passivo (011).

## Contexto

Em raid, a stamina de braço na **stance 0** oscilava (subia/descia) e às vezes "não entendia que saiu" do mount; mexer no `Stamina Multiplier` no F12 "consertava" temporariamente. Sintoma de **cabo-de-guerra**: 5 fontes escreviam na `HandsStamina` sem coordenação, cada uma cedendo a um subconjunto diferente de estados.

Fontes em conflito:
- Tick por stance ([`StanceManager.TickStanceStamina`](../../modded/StanceManager.cs)) — cedia a ADS/mount-ativo/prone, **mas não ao mount passivo**.
- Restauração ([`StanceStaminaRecoveryPatch`](../../modded/Patches/StanceStaminaRecoveryPatch.cs) → `GetHandsRestorationFunc`) — regen 5/2.5 fixos.
- Hold-breath drain ([`ApplyComplexRotationPatch`](../../modded/Patches/ApplyComplexRotationPatch.cs)) — **não cedia a nada**.
- Prone (`HandsStaminaConsume/Process`) — só prone.
- Vanilla EFT — ADS/hipfire/prone.

## Causa raiz

No mount **passivo** a restauração regenerava (`2.5`) enquanto o tick **continuava drenando** (stance 0 com `Multiplier < 1`), porque o tick só cedia ao mount *ativo* (`StanceManager.cs:1334`, antigo) — daí a oscilação. O `IsBracing` preso (sem espelhar os guards do `TryMountWeapon`) causava regen-fantasma parado ("não desce"); o `SettingChanged` ao mexer no F12 re-snapava o `Multiplier` e mascarava o sintoma.

## Solução: um único dono da stamina por frame

Coordenador [`ArmStamina.Resolve`](../../modded/ArmStaminaCoordinator.cs) resolve **um** modo por frame (prioridade: **Prone → MountActive → MountPassive → HoldBreath → ADS(vanilla) → StanceDrain → Vanilla**). Cada fonte consulta o coordenador e só age no seu modo. Nos modos de mount o **consumo de aim-drain é zerado** (como no prone) e tudo é controlado pela restauração → resultado determinístico.

### Perfil B (resultado por quadrante, stance 0)

| Stance 0 | Sem ADS | Com ADS |
|---|---|---|
| Sem mount | segue `Stance Multiplier` (drena se <1) | drena (vanilla) |
| **Passivo** | recupera leve (`Passive Mount Stamina Regen`) | **parado** (segura, sem regen) |
| **Ativo** | recupera forte (`Active Mount Stamina Regen`) | recupera leve |

### Cenários cercados (espelhando `TryMountWeapon`, Player.cs:26220)

O passivo só ativa onde o vanilla permite mount — `IsBracing` é limpo em: recarga (`IsInReloadOperation`), equip/troca (`IsInSpawnOperation`/`IsInRemoveOperation`), interação (`IsInInteraction`), blindfire (`BlindFire != 0`), no ar (`!IsGrounded`), metralhadora fixa (`IsStationaryWeaponInHands`), arma não-montável (`!Weapon.IsMountable`). Toggle `Enable Passive Mount` OFF agora limpa `IsBracing` na hora.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/ArmStaminaCoordinator.cs` | **CRIADO** — enum `ArmStaminaMode` + `ArmStamina.Resolve` (autoridade única). |
| `modded/Patches/StanceStaminaRecoveryPatch.cs` | Restauração via coordenador (Perfil B, ADS-aware); `Consume` zera aim-drain nos modos de mount (não só prone). |
| `modded/Patches/PassiveMountDetectPatch.cs` | Guards do `TryMountWeapon`; `ClearBracing` no toggle OFF. |
| `modded/StanceManager.cs` | `TickStanceStamina` só age no modo `StanceDrain` (via coordenador). |
| `modded/Patches/ApplyComplexRotationPatch.cs` | Hold-breath drain só no modo `HoldBreath` (cede a mount/prone). |
| `modded/Plugin.cs` | 2 configs: `Active Mount Stamina Regen` (5), `Passive Mount Stamina Regen` (2.5). |
| `PROPRIEDADES.md` | Documenta as 2 configs novas. |

## Deferido para calibração in-game (pós-validação)

- **Histerese** na borda encostar↔soltar — só se houver flicker observável.
- **Raycast "para baixo" pegando o chão** parado — tuning de geometria/origem (exige ver o falso-positivo real).
- **Hideout/range** — alinhar `IsActiveContext` do passivo com o do tick.

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros
- [ ] **In-raid:** os 6 quadrantes (stance 0 × {sem mount, passivo, ativo} × {hipfire, ADS}) batem com o Perfil B
- [ ] Sem oscilação na stance 0; sem regen-fantasma parado; sem precisar mexer no F12 para "destravar"
- [ ] Passivo cede em recarga/interação/no-ar/metralhadora-fixa/arma-não-montável
- [ ] **Fika/multiplayer:** stamina/buffs só no MainPlayer — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Fix criado e implementado — coordenador único + Perfil B + guards `TryMountWeapon`. Compila 0 erros; aguarda validação in-game. |
