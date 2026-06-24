# 014 — Sync de stances Fika · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**As-built:** [014-sync-stances-fika-05-asbuild.md](014-sync-stances-fika-05-asbuild.md)
**Data:** 2026-06-22

> Análise por **2 revisores independentes** (sub-agents de contexto limpo). IDs `CR-01-MM` permanentes.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 2 · Total: 3

Ambos confirmaram a implementação **correta e coerente com a spec**: gate `!IsYourPlayer` hermético (kick/hold-breath só local — AP-02 garantido), fórmula de `SetPositionAndRotation` **idêntica** ao local, spring state **por-instância** (isolado por player), lifecycle do component seguro (destruído com o observed), prone/non-firearm/null-safety OK, timing viável. 1 achado acionável (validação de input de rede).

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | B | 🟡 | `_stance` (int da rede) sem validação de bounds antes do cast `(Stance)` | ✅ Aplicado |
| CR-01-02 | F | 🟢 | `GetComponent<ObservedStanceAnimator>()` por frame | Deferido |
| CR-01-03 | E | 🟢 | Lógica de prone `!(_observedPlayer != null && …)` — já segura, clareza opcional | Deferido |

---

### CR-01-01 · B — Bug latente · 🟡 Médio

**`_stance` recebido da rede não é validado antes de `(Stance)_stance`**

**Local:** [`ObservedStanceAnimator.cs:23-26, 39`](../../modded/Networking/ObservedStanceAnimator.cs)

**Problema:** `SetStance(int stance, …)` grava `_stance` direto do `StanceSyncPacket.Stance` (dado de rede). O cast `(Stance)_stance` não valida bounds — um valor fora de `0..3` (cliente bugado/versão divergente) vira um enum inválido. O `switch` em `GetTargetRotation` tem `_ => Vector3.zero` (não crasha), mas `inStance = _stance > 0` ficaria `true` com offset zero (estado incoerente), e input de rede não-confiável deve ser saneado na borda.

**Por que importa:** higiene de dados de rede (boundary validation) + evita estado incoerente silencioso.

**Sugestão:** clampar no `SetStance`:
```csharp
public void SetStance(int stance, bool isAiming)
{
    _stance = (stance < 0 || stance > 3) ? 0 : stance;
    _isAiming = isAiming;
}
```

**Decisão:** `[x]` Aceitar sugestão · `[ ]` Aceitar com modificação · `[ ]` Rejeitar

---

### CR-01-02 · F — Performance · 🟢 Menor

**`player.gameObject.GetComponent<ObservedStanceAnimator>()` a cada frame por observed**

**Local:** [`ApplyComplexRotationPatch.cs:162`](../../modded/Patches/ApplyComplexRotationPatch.cs)

**Problema:** `GetComponent` por frame por observed player. Custo O(1) (cache interno do Unity), null-safe via `?.`. Trivial, mas poderia ser cacheado (dict por ProfileId no `FikaSyncManager`).

**Por que importa:** micro-otimização; só relevante com muitos observados.

**Sugestão:** opcional — cachear o component. Não compensa a complexidade de limpeza agora.

**Decisão:** `[ ]` Pendente · `[x]` Rejeitar (deferir — custo trivial)

---

### CR-01-03 · E — Legibilidade · 🟢 Menor

**`bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose)`**

**Local:** [`ObservedStanceAnimator.cs:38`](../../modded/Networking/ObservedStanceAnimator.cs)

**Problema:** a forma é **já null-safe** (short-circuit + o operador `!=` do Unity trata objeto destruído como null). Um revisor sugeriu a forma `_stance > 0 && (_observedPlayer == null || !_observedPlayer.IsInPronePose)` — **logicamente equivalente**, só mais legível.

**Por que importa:** clareza; sem mudança de comportamento.

**Sugestão:** opcional — reescrever para a forma com `== null ||`. Não-bloqueante.

**Decisão:** `[ ]` Pendente · `[x]` Rejeitar (deferir — já correto)

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Code review 01 (2 revisores independentes) — 0 🔴, 1 🟡, 2 🟢 |
| 2026-06-22 | Aplicado CR-01-01 (clamp do stance 0..3 em `SetStance`). CR-01-02/03 deferidos. Recompila 0 erros. |
