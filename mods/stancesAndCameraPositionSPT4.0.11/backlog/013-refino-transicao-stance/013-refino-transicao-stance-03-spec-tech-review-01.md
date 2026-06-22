# 013 — Refinamentos de transição de stance · Spec Tech Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica:** [013-refino-transicao-stance-02-spec-tech.md](013-refino-transicao-stance-02-spec-tech.md)
**Data:** 2026-06-21

> Análise crítica adversarial. IDs `PA-01-MM` permanentes.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 1 · Total: 2

Memória: snapshot Sessão 5 (2026-06-21) · sem pendência que afete. Refs confirmadas: campos do spring (`CurrentEuler`/`CurrentPosition` public static; `_rotVelocity`/`_posVelocity` private static — ApplyComplexRotationPatch.cs:32-37) são acessíveis pelo `SnapToNeutral` (mesma classe). `IsInStance => CurrentStance != Default` (StanceManager.cs:62) ⇒ em Default o target do spring é zero, coerente com o snap. Sem 🔴 — pode iniciar `/code-mod`.

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B | 🟡 | Ajuste 3 (snap no sprint) é visual — validar/calibrar in-game | Pendente |
| PA-01-02 | A | 🟢 | Ordem `Plugin.Update` × `ApplyComplexRotationPatch` no frame | Pendente |

---

### PA-01-01 · B — Edge Case · 🟡 Importante

**O `SnapToNeutral` elimina o spring, mas o resultado é visual — só validável in-game**

**Problema:** o fix do ajuste 3 troca a animação (spring `Stance1→0`) por um snap instantâneo dos offsets. Se ainda restar um salto perceptível (ex.: a posição da arma), a calibração (snap só da rotação, ou um spring muito rápido em vez de instantâneo) só é decidível observando o jogo.

**Por que importa:** é a essência do ajuste 3 (remover o "flash"); não dá para cravar no papel se o snap é suave o suficiente.

**Sugestão:** implementar o snap como na spec (zera euler/position/velocities), validar in-game; se houver salto, calibrar (manter `CurrentPosition` em vez de zerar, ou snap só `CurrentEuler`). Registrar como pendência de validação no asbuild. Com TacSprint ativo o caminho não é tocado (gate preservado).

**Decisão:** `[x]` Aceitar sugestão (implementar + validar/calibrar in-game) · `[ ]` Caminho alternativo

---

### PA-01-02 · A — Gap · 🟢 Menor

**Ordem de execução `Plugin.Update` (chama `SnapToNeutral`) × `ApplyComplexRotationPatch` (Postfix) não é garantida**

**Problema:** o `SnapToNeutral` é chamado no `StanceManager.Update` (via `Plugin.Update`); o spring roda no `ApplyComplexRotationPatch` (Postfix do EFT). Se o Postfix rodar antes do `Plugin.Update` no frame da transição, há ≤1 frame de defasagem até o snap valer.

**Por que importa:** no pior caso, 1 frame do spring antigo antes do snap — imperceptível, mas convém saber.

**Sugestão:** aceitar (1 frame é invisível); se a validação de PA-01-01 mostrar resíduo, considerar setar um flag lido pelo próprio `ApplyComplexRotationPatch` (que snapa no início do seu Postfix). Não bloqueia.

**Decisão:** `[x]` Aceitar sugestão (aceitar; reavaliar só se PA-01-01 pedir) · `[ ]` Caminho alternativo

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Review 01 criada via `/review-technical-spec` — 0 🔴, 1 🟡, 1 🟢. Refs (campos static, IsInStance) confirmadas. |
