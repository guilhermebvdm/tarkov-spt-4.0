# 015 — Bloquear mount ativo em Stance 1/2/3 · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [015-bloquear-mount-ativo-stances-02-spec-tech.md](015-bloquear-mount-ativo-stances-02-spec-tech.md)
**Data:** 2026-07-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`. Refs do Assembly conferidas: `Player.TryMountWeapon` ([Player.cs:26218](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26218) — `public void`, confirmado) e `MovementContext.StartExitingMountedState`/`ExitMountedState` ([MovementContext.cs:2985/2996](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2985) — ambos `public`, confirmado).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 3 · ✅ Resolvidos: 2 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B | 🟡 | Guard não exclui **prone** — desmontaria/bloquearia mount deitado (o 011 já trata) | ✅ Resolvido na spec |
| PA-01-02 | A | 🟡 | Seção F12 do 011 não confirmada — risco de seção duplicada no menu | Aceito (resolver no code-mod) |
| PA-01-03 | B | 🟢 | `StartExitingMountedState` no-op se flag de animação e estado de controle divergem | Aceito (validação in-game) |
| PA-01-04 | B | 🟢 | Chamar `StartExitingMountedState` manualmente mexe no handler de evento nativo | Aceito (validação in-game) |
| PA-01-05 | C | 🟢 | `ECommand.WeaponMounting=145` no §6 confunde linha com valor | ✅ Resolvido na spec |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante

**O guard (`Stance != Default && !IsAiming`) não exclui prone — o mount deitado seria bloqueado/desmontado**

**Problema:** o gate do Prefix e do tick (spec §5) é `StanceManager.CurrentStance != Stance.Default && !isAiming`. Não considera **prone**. Se o jogador estiver em Stance 1/2/3 e deitar (prone) para usar apoio/bipé, o `TickActiveMountGuard` tenta desmontar e o `BlockActiveMountPatch` bloqueia a ativação — mas em prone o mount é justamente o caso legítimo. O próprio item **011** cede ao vanilla em prone: `PassiveMountDetectPatch.cs:56` inclui `player.IsInPronePose` entre as condições que abortam o detector passivo.

**Por que importa:** um jogador em Stance 1/2/3 que deita para montar teria o mount recusado ou desfeito — regressão de uma mecânica desejada, e inconsistência com o 011.

**Sugestão:** adicionar `!player.IsInPronePose` ao guard nas duas mãos (Prefix e tick). No `BlockActiveMountPatch.Prefix`: `if (StanceManager.CurrentStance != Stance.Default && !isAiming && !__instance.IsInPronePose) return false;`. No `TickActiveMountGuard`: incluir `&& !player.IsInPronePose` na condição de desmontar. Reusa exatamente o guard que o 011 já aplica ([PassiveMountDetectPatch.cs:56](../../modded/Patches/PassiveMountDetectPatch.cs#L56)).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-02 · A — Gap · 🟡 Importante

**Nome exato da seção F12 do item 011 não confirmado — risco de criar seção duplicada**

**Problema:** a spec (§3) coloca a ConfigEntry na seção `Weapon Mounting` mas anota "confirmar/reusar a seção do 011". Se o 011 usa outro nome literal, o BepInEx cria uma **seção nova** no F12 (casa por `(section, key)`), separando a config do 015 das do mount e poluindo o menu.

**Por que importa:** UX do F12 (config órfã) e possível confusão para o usuário; sem impacto funcional, mas evitável de graça.

**Sugestão:** antes de bindar, no `/code-mod`, fazer `grep "Config.Bind(" modded/Plugin.cs` filtrando os binds do 011 (mount passivo) e **copiar o literal exato** da seção. Se o 011 não tiver uma seção de mount própria (usar outra), decidir uma canônica (`Weapon Mounting`) e registrar no `PROPRIEDADES.md`. Fechar este ponto com o nome real citado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · B — Edge Case · 🟢 Menor

**`StartExitingMountedState` é no-op se `IsMountedState` (animação) está true mas `OverridenControlsState` já não é idle**

**Problema:** o tick dispara `StartExitingMountedState()` quando `pwa.IsMountedState`. Mas esse método só age se `OverridenControlsState is IdleWeaponMountingStateClass` ([MovementContext.cs:2987](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2987)). Numa janela rara (flag de animação ainda true, estado de controle já trocado), o desmontar seria no-op e o `_mountGuardExiting` ficaria `true`, sem re-tentar.

**Por que importa:** cenário raro; no pior caso a arma fica visualmente montada até o próximo evento. Já há fallback documentado (§7).

**Sugestão:** manter `StartExitingMountedState` como primário; se a validação in-game mostrar mount preso, trocar para `MovementContext.ExitMountedState()` (hard cut, também emite `OnMounting(Exit)`). Nenhuma mudança na spec necessária — só registrar como ponto de atenção no teste.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-04 · B — Edge Case · 🟢 Menor

**Chamar `StartExitingMountedState()` manualmente remove o handler `OnStartExitMountedState`**

**Problema:** `StartExitingMountedState` faz `PlayerMountingPointData.OnStartExitMountedState -= StartExitingMountedState` ([MovementContext.cs:2991](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2991)). Chamá-lo por fora do fluxo nativo pode desassinar um handler que o jogo esperava disparar, ou causar double-exit se o jogo também iniciar a saída no mesmo frame.

**Por que importa:** o `_mountGuardExiting` já limita a 1 disparo por episódio, então o risco é baixo; mas vale confirmar in-game que montar → stance → voltar a Stance 0 → montar de novo funciona (o `EnterMountedState` re-adiciona o handler).

**Sugestão:** manter a abordagem; adicionar ao checklist de validação in-game o ciclo "montar → entrar em stance (desmonta) → voltar a Stance 0 → montar de novo" para confirmar que não há estado presoo. Sem mudança de código a priori.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-05 · C — Erro de Lógica · 🟢 Menor

**`ECommand.WeaponMounting=145` no diagrama §6 confunde número da linha com valor do enum**

**Problema:** o §6 escreve `ECommand.WeaponMounting=145`. Pela investigação, `145` é a **linha** de `ECommand.cs`, não o valor do enum (`EGameKey.WeaponMounting = 118`). Além disso o path `EFT/InputSystem/ECommand.cs` não foi confirmado neste review (não é ponto de patch — é ilustrativo do fluxo de input, que vem de assembly externo).

**Por que importa:** confusão de leitura; nenhum impacto de implementação (o input não é patcheado — o patch é em `TryMountWeapon`, o ponto onde o input aterrissa).

**Sugestão:** trocar `ECommand.WeaponMounting=145` por "tecla dedicada de mount (input externo)" no diagrama, sem citar número.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Review técnica 01 — 0 🔴, 2 🟡 (prone no guard, seção F12), 3 🟢. Refs de patch confirmadas no Assembly. |
