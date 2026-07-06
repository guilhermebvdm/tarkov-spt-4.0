# 056 — Indicador de perk no peso · Revisão técnica 01

**Mod:** CustomClasses
**Spec técnica:** [056-perk-stat-indicators-02-spec-tech.md](056-perk-stat-indicators-02-spec-tech.md)
**Data:** 2026-07-03

> 🔴 0 · 🟡 2 · 🟢 1 — sem bloqueador; pode ir pro `/code-mod`. Os 🟡 são de posicionamento/timing (verify-in-game),
> herdados do mesmo molde (`SkillPanelPatch`) que já funciona in-game.

## Índice

| ID | Cat | Impacto | Título | Decisão |
|---|---|---|---|---|
| PA-01-01 | B — Edge | 🟡 | `UI.BindEvent` pode não invocar o handler de peso na hora → `_maxValue` vazio no 1º `method_0` | Aceito — verify-in-game |
| PA-01-02 | B — Edge | 🟡 | Marcador "▲ +30%" pode transbordar o sub-painel do peso | Aceito — verify-in-game |
| PA-01-03 | A — Gap | 🟢 | `ItemUiContext.Instance.Tooltip` null | OK — coberto pelo try/catch |

---

### PA-01-01 · B — Edge · 🟡
**`UI.BindEvent(OnWeightUpdated)` pode só assinar (sem invoke imediato) → no 1º `method_0` do Show o `_maxValue`
ainda está vazio → `preferredWidth==0` → marcador colado no início.**
A posição usa `maxTmp.preferredWidth` (molde do `SkillPanelPatch`, que funciona porque o `_name` sempre tem texto).
Se o peso ainda não foi setado, a posição fica errada até o próximo refresh.
**Mitigação:** `method_0` roda de novo no `Update` (`HealthParametersPanel.cs:240`) → re-posiciona; e o peso é setado
cedo (BindEvent no EFT normalmente invoca na hora). Se aparecer deslocado no gate, trocar `preferredWidth` por
ancoragem ao `rect` do `_maxValue` (offset fixo). **Decisão:** aceitar; verificar in-game.

### PA-01-02 · B — Edge · 🟡
**O marcador ao lado do `_maxValue` pode invadir o layout do sub-painel de peso** (o painel de peso é mais estreito que
a linha de skill). Cosmético.
**Decisão:** aceitar; ajustar `MarkerGap`/`fontSize` no gate se colidir (mesmo tipo de ajuste que o `SkillPanelPatch` já
exigiu). Verify-in-game.

### PA-01-03 · A — Gap · 🟢
**`ItemUiContext.Instance.Tooltip` pode ser null** ao criar o `HoverTooltipArea`. Improvável (a aba Health abre sob o
contexto de inventário, com `ItemUiContext` ativo — mesma premissa do `SkillPanelPatch:105`). Se ocorrer, o `Init`
lança **dentro** do try/catch do Postfix → logado, sem crash. **Decisão:** OK como está.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Revisão técnica 01 via `/review-technical-spec` — 0 🔴; 2 🟡 (posição/timing) + 1 🟢, aceitos |
