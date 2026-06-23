# 013 — Fix 01 · Sprint mantém a stance atual (sem passar pela Stance 0)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [013-refino-transicao-stance-01-spec.md](013-refino-transicao-stance-01-spec.md)
**Asbuild:** [013-refino-transicao-stance-05-asbuild.md](013-refino-transicao-stance-05-asbuild.md)
**Criado:** 2026-06-22
**Disparado por:** feedback in-game (validação do ajuste 3 do item 013).

## Contexto

No item 013, o ajuste 3 trocou a animação do spring (`Stance1→0`) por um **snap instantâneo** (`SnapToNeutral`) ao iniciar o sprint. In-game, o usuário relatou que **ainda há a passagem pela Stance 0** — antes era a arma "subindo lentamente" pela Stance 0; depois do snap virou um **"flash super rápido"** pela Stance 0 e então a corrida assume. Ou seja, o snap só mudou a *velocidade* da transição, mas **a stance ainda era forçada para 0**.

Requisito real esclarecido: ao iniciar o sprint estando em **qualquer** stance (0/1/2/3), **nada deve mudar** — a corrida acontece **inteiramente na stance atual**, sem nenhuma transição/flash pela Stance 0.

## Causa raiz

O bloco de sprint em [`StanceManager.cs`](../../modded-beta/StanceManager.cs) ainda chamava `SetStance(Stance.Default)` (force-zero) ao sprintar sem TacSprint — o `SnapToNeutral` só pulava a animação, não eliminava a mudança de stance. O comportamento correto é **não tocar na stance** durante o sprint.

## Solução

Remoção completa do mecanismo de force-zero do sprint:
- O bloco `if (isSprinting)` não chama mais `SetStance(Default)` nem `SnapToNeutral()` — apenas encerra a Action Stance e trava as hotkeys (`return`). A `CurrentStance` permanece a atual; o `ApplyComplexRotationPatch` segue aplicando os offsets daquela stance durante a corrida.
- Removido o `else if (_wasSprintingForceZero)` (restore — não há mais o que restaurar).
- Removidas as variáveis órfãs `_preSprintStance`/`_wasSprintingForceZero` e o método `ApplyComplexRotationPatch.SnapToNeutral()` (do item 013, agora sem uso).
- **TacSprint** (arma leve) segue inalterado.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-beta/StanceManager.cs` | Removido o force-zero/restore do sprint; sprint mantém a stance atual. Removidas 2 vars órfãs. |
| `modded-beta/Patches/ApplyComplexRotationPatch.cs` | Removido o método `SnapToNeutral` (sem uso). |

## Checklist de validação (obrigatório antes de marcar entregue)

- [x] Compila via `/compile-mod` sem erros
- [ ] **In-raid:** correr a partir de Stance 1/2/3 (e 0) — a arma **não passa** pela Stance 0; a corrida ocorre na stance atual; nada muda ao iniciar/encerrar o sprint
- [ ] **Arma pesada/grande:** correr na stance não causa pose quebrada/clipping inaceitável (caso ocorra, reavaliar)
- [ ] **TacSprint** (arma leve) intacto
- [ ] **Fika/multiplayer:** N/A — só MainPlayer
- [ ] **raid1 → raid2 / morte:** sem estado preso

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Fix criado — sprint deixa de forçar Stance 0; mantém a stance atual. Compila 0 erros; aguarda validação in-game. |
