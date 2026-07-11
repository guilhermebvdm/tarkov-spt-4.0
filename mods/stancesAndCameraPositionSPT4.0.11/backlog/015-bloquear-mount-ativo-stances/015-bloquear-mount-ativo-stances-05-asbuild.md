# 015 — Bloquear mount ativo em Stance 1/2/3 · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [015-bloquear-mount-ativo-stances-01-spec.md](015-bloquear-mount-ativo-stances-01-spec.md)
**Spec técnica:** [015-bloquear-mount-ativo-stances-02-spec-tech.md](015-bloquear-mount-ativo-stances-02-spec-tech.md)
**Última review técnica:** [015-bloquear-mount-ativo-stances-03-spec-tech-review-01.md](015-bloquear-mount-ativo-stances-03-spec-tech-review-01.md) (0 🔴)
**Build inicial:** 2026-07-09

> Documentação pós-implementação. Bloqueia o mount **ativo** de superfície em Stance 1/2/3 (exceções: Stance 0, ADS, prone, bipé) e desmonta ao entrar em stance. Compila 0 erros; instalado em `RealisticMobility/` (hash `cc33e8d1b113`). **Aguarda validação in-game.**

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/.../modded/Patches/BlockActiveMountPatch.cs` | Prefix em `Player.TryMountWeapon` ([Player.cs:26218](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218)) → `false` quando local + `Stance != Default && !IsAiming && !IsInPronePose`. Bipé não passa por aqui. |
| MODIFICADO | `mods/.../modded/StanceManager.cs` | Novo `TickActiveMountGuard()` + flag `_mountGuardExiting`: desmonta via `MovementContext.StartExitingMountedState()` ([MovementContext.cs:2985](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985)) quando montado (superfície, `pwa.IsMountedState`) + Stance 1/2/3 sem ADS/prone. Idempotente. |
| MODIFICADO | `mods/.../modded/Plugin.cs` | `SafeEnable("BlockActiveMountPatch")` (junto do 011); `StanceManager.TickActiveMountGuard()` no `Update`; const `ActiveMountSettings = "Weapon Mount (Active)"`; campo + bind `_BlockActiveMountInStance` (default `true`). |
| MODIFICADO | `mods/.../PROPRIEDADES.md` | Nova seção "Apoio Ativo de Arma (`Weapon Mount (Active)`) — Item 015" com a ConfigEntry. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge · 🟡 | **prone** adicionado ao guard (`!IsInPronePose`) no Prefix e no tick — mount deitado é legítimo, igual ao 011. |
| PA-01-02 | A — Gap · 🟡 | Seção F12 confirmada: o 011 usa `Weapon Mount (Passive)`; criada a paralela `Weapon Mount (Active)` para o 015 (sem duplicar). |
| PA-01-05 | C — Lógica · 🟢 | Diagrama §6 corrigido (removido `ECommand.WeaponMounting=145`, que confundia linha com valor). |

> **PA-01-03 / PA-01-04** (🟢) não exigiram mudança de código — viram itens do checklist de validação in-game abaixo.

## Mudanças posteriores

| Data | Rodada | Mudança |
|---|---|---|
| 2026-07-09 | code-review 01 | **CR-01-01 (Opção A):** removido `TickActiveMountGuard` + `_mountGuardExiting` (StanceManager.cs) e a chamada no `Plugin.Update` — era **código morto**: o item 013 já força Stance 0 enquanto montado (roda antes no `Update` e engole o input de stance). Comportamento efetivo: **montado ⇒ trava em Stance 0** (via 013), não "desmonta". O `BlockActiveMountPatch` (bloqueio da ativação) **permanece**. CR-01-02/03 saíram junto. Recompilado 0 erros (hash `9afae5f0a146`). |

## Checklist de validação in-game (antes de fechar 🟢 definitivo)

- [ ] Em Stance 1/2/3 **sem** ADS, apontar para superfície montável **não** monta (nem prompt).
- [ ] Em **Stance 0**, mount vanilla funciona normal.
- [ ] Em **qualquer stance + ADS**, mount funciona.
- [ ] Montado (Stance 0): tentar trocar p/ Stance 1/2/3 → **fica preso em Stance 0** (mount mantido; via 013). Para trocar de stance, desmontar manualmente primeiro.
- [ ] **Prone:** mount/bipé deitado funcionam normalmente (não bloqueados). (PA-01-01)
- [ ] **Bipé:** deploy do bipé funciona em qualquer stance (exceção).
- [ ] **Fika (como CLIENTE, não host):** o bloqueio não afeta o mount de peers; observados montam/desmontam normal.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-09 | Build concluído via `/code-mod` — compila 0 erros; PA-01-01/02/05 aplicados no build. Instalado em `RealisticMobility/` (hash `cc33e8d1b113`). Status 🟢 (aguarda validação in-game). |
