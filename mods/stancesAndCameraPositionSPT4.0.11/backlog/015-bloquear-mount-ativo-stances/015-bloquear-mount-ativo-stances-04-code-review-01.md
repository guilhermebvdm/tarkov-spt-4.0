# 015 — Bloquear mount ativo em Stance 1/2/3 · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [015-...-01-spec.md](015-bloquear-mount-ativo-stances-01-spec.md)
**Spec técnica:** [015-...-02-spec-tech.md](015-bloquear-mount-ativo-stances-02-spec-tech.md)
**As-built:** [015-...-05-asbuild.md](015-bloquear-mount-ativo-stances-05-asbuild.md)
**Data:** 2026-07-09

> Análise crítica do código implementado. IDs `CR-01-MM` permanentes. Memória consultada: snapshot Sessão 6 (2026-07-09) · pendências que afetam: nenhuma 🔴.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 0 · 🟢 Menores: 2 · ✅ Aplicados: 3 · Total: 3

O **bloqueio da ativação** (Prefix em `TryMountWeapon`) está correto e entrega o requisito principal. O achado central foi uma **interação com o item 013**: o mecanismo de *desmontar* era código morto. **Decisão do usuário (2026-07-09): Opção A** — aceitar "montado trava em Stance 0" (013); o código morto do desmontar foi **removido**. CR-01-02/03 saíram junto com o código.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | D | 🟠 | `TickActiveMountGuard` (desmontar) nunca dispara — o 013 força Stance 0 quando montado | ✅ Aplicado (Opção A) |
| CR-01-02 | B | 🟢 | `player.MovementContext` sem null-check antes de `StartExitingMountedState()` | ✅ Resolvido (código removido) |
| CR-01-03 | E | 🟢 | Comentário de ref impreciso em `TickActiveMountGuard` | ✅ Resolvido (código removido) |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

### CR-01-01 · D — Arquitetura (código morto + divergência de comportamento) · 🟠 Forte · ✅ Aplicado em 2026-07-09

**O desmontar automático nunca acontece — o item 013 já força Stance 0 enquanto montado**

**Resolução (Opção A):** removidos `TickActiveMountGuard` + `_mountGuardExiting` (StanceManager.cs) e a chamada no `Plugin.Update`. Deixado um comentário no lugar explicando que o comportamento "montado ⇒ Stance 0" vem do 013. O `BlockActiveMountPatch` (bloqueio da ativação) permanece. Recompilado 0 erros (hash `9afae5f0a146`). CR-01-02/03 saíram junto com o código removido.

**Local:** [`mods/.../modded/StanceManager.cs:1319-1360`](../../modded/StanceManager.cs#L1319) (`TickActiveMountGuard`) vs [`StanceManager.cs:169-180`](../../modded/StanceManager.cs#L169) (013)

**Problema:** o `StanceManager.Update` (013) roda a cada frame e, quando `isNativeMounting` (`pwa.IsMountedState || pwa.IsBipodUsed`), **força `SetStance(Stance.Default)` e faz `return`** antes de processar as hotkeys de stance:
```csharp
if (isNativeMounting || isInProne || isStationary) {
    if (_isActionStanceActive) EndActionStance(forceCancel: true);
    if (CurrentStance != Stance.Default) SetStance(Stance.Default);   // força Stance 0
    return;                                                            // engole o input de stance
}
```
No `Plugin.Update`, a ordem é `StanceManager.Update()` **antes** de `StanceManager.TickActiveMountGuard()`. Logo, quando o jogador está montado, `CurrentStance` **já foi revertido para `Default`** — e o guard do tick (`mountedSurface && CurrentStance != Stance.Default && …`) é **sempre falso**. **`StartExitingMountedState()` nunca é chamado.**

**Por que importa:** (1) `TickActiveMountGuard` é **código morto** — nunca desmonta. (2) O comportamento efetivo **diverge da decisão do usuário**: em vez de "desmontar ao entrar em Stance 1/2/3", o 013 **trava o jogador em Stance 0 enquanto montado** (o input de troca de stance é ignorado). Ambos atendem ao objetivo maior ("mount e Stance 1/2/3 não coexistem"), mas por caminhos diferentes. O bloqueio da *ativação* (Prefix) **não** é afetado — segue correto e útil.

**Sugestão (decisão do usuário):**
- **Opção A (recomendada, simples):** aceitar o comportamento do 013 — enquanto montado, o jogador fica preso em Stance 0 (não troca). **Remover** `TickActiveMountGuard` + `_mountGuardExiting` + a chamada no `Update` (código morto), e ajustar a spec/as-built para descrever "montado ⇒ Stance 0 (via 013)" em vez de "desmonta". O Prefix (bloquear ativação) permanece.
- **Opção B:** manter o "desmontar" literal — mexer no 013 para **não** forçar Stance 0 no caso de **mount de superfície** (deixar `CurrentStance` mudar para 1/2/3 montado, então o `TickActiveMountGuard` desmonta). Mais invasivo, mexe num item já validado (013), e reintroduz o "flash" que o 013 evita. Não recomendado.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (Opção A — remover o tick, documentar "trava em Stance 0")
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟢 Menor

**`player.MovementContext` não é checado antes de `StartExitingMountedState()`**

**Local:** [`mods/.../modded/StanceManager.cs`](../../modded/StanceManager.cs#L1319) (`TickActiveMountGuard`, ramo do desmontar)

**Problema:** `player.MovementContext.StartExitingMountedState()` assume `MovementContext != null`. Em contextos de borda (frame de morte/despawn) poderia ser null → NRE capturada pelo try/catch, mas logando por frame (spam). Nota: se o CR-01-01 for aceito (Opção A), este código é removido e o ponto some.

**Por que importa:** higiene defensiva; irrelevante se o tick for removido (CR-01-01).

**Sugestão:** se o tick permanecer, adicionar `var mc = player.MovementContext; if (mc == null) return;` antes do bloco de desmontar. Se o CR-01-01 Opção A for aceito, ignorar (código sai).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir — resolvido por CR-01-01 Opção A): _________________

---

### CR-01-03 · E — Legibilidade · 🟢 Menor

**Comentário de referência genérico em `TickActiveMountGuard`**

**Local:** [`mods/.../modded/StanceManager.cs`](../../modded/StanceManager.cs#L1319)

**Problema:** o comentário `// ref: StanceManager.cs uso em isNativeMounting` não cita a linha exata (é `StanceManager.cs:157`). Perde a ancoragem clicável que o resto do mod usa.

**Por que importa:** navegabilidade; trivial. (Também some se CR-01-01 Opção A remover o tick.)

**Sugestão:** trocar por `// ref: StanceManager.cs:157 (isNativeMounting = IsMountedState || IsBipodUsed)`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Code review 01 — 0 🔴, 1 🟠 (desmontar é código morto por causa do 013), 2 🟢. O bloqueio da ativação (Prefix) está correto. |
