# 055 — Detalhe da classe no lobby/loading · Revisão técnica 01

**Mod:** CustomClasses
**Spec técnica:** [055-class-detail-lobby-02-spec-tech.md](055-class-detail-lobby-02-spec-tech.md)
**Data:** 2026-07-02

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 2 · Total: 3
> Sem bloqueador — pode ir pro `/code-mod`. O 🟡 (gatilho hover vs auto) é resolvido aqui na direção "auto-visível default".

## Índice

| ID | Cat | Impacto | Título | Decisão |
|---|---|---|---|---|
| PA-01-01 | B — Edge | 🟡 | Hover depende de EventSystem/GraphicRaycaster ativo no loading (incógnita) | ✅ Resolvido — auto-visível default |
| PA-01-02 | A — Gap | 🟢 | Fonte do painel no loading pode vir `null` | ✅ Resolvido na spec |
| PA-01-03 | C — Lógica | 🟢 | `_lastPanelClass` estático compartilhado entre 2 hosts | ✅ Resolvido — benigno + doc |

---

### PA-01-01 · B — Edge case · 🟡 Importante

**Hover depende de EventSystem/GraphicRaycaster ativo na tela de carregamento**

**Problema:** o stub usa `IPointerEnterHandler` na linha do player local. Se a tela de carregamento do FIKA **não** tiver
um `GraphicRaycaster` no Canvas + `EventSystem` ativo (ou o cursor estiver travado durante o load), o `OnPointerEnter`
nunca dispara → o painel nunca aparece. É incógnita (só o gate confirma), e a tela é transiente (janela de hover curta).

**Por que importa:** o gatilho hover pode simplesmente não funcionar no loading, entregando "nada" — sem erro, mas sem
feature.

**Sugestão / decisão:** **auto-visível como default robusto** — ao detectar a linha do player local, o `LoadingClassHover`
faz `Build`+`Refresh`+`SetActive(true)` **imediatamente** (não espera hover), ancorado de forma **compacta ao lado da
linha**. O hover/click vira refinamento opcional (esconder/mostrar) **se** o EventSystem estiver ativo. Isso não depende
de raycast e alinha ao valor (tela transiente = você quer **ver**, não caçar com o mouse). A spec funcional já prevê
"hover · click · auto" como gatilho a decidir. Ajustar o stub §5: `LoadingClassHover` mostra no `Start()`/`OnEnable`, não
só no `OnPointerEnter`.

**Decisão:** `[x]` Aceitar — auto-visível no default; hover como toggle opcional. (Ajuste fino de posição/intrusividade no gate.)

---

### PA-01-02 · A — Gap · 🟢 Menor

**Fonte (`TMP_FontAsset`) do painel no loading pode vir `null`**

**Problema:** `PerksPanelView.Build(..., null)` no `LoadingClassHover.Ensure` passa `font = null` — no loading não há
acesso fácil à fonte do EFT como na tela de Skills.

**Sugestão / decisão:** pegar a fonte de um TMP da própria linha: `GetComponentInChildren<TMP_Text>()?.font` (a linha
tem `Nickname`/`Percentage`), com fallback `TMP_Settings.defaultFontAsset`. `PerksPanelView.Build` já deve tratar `null`
com esse fallback (herdado do 059). Registrar no checklist do `/code-mod`.

**Decisão:** `[x]` Aceitar — fonte da linha + fallback default.

---

### PA-01-03 · C — Lógica · 🟢 Menor

**`_lastPanelClass` estático compartilhado ao extrair o `PerksPanelView`**

**Problema:** o guard anti-rebuild do 059 usa `_lastPanelClass` **estático**. Extraído pro `PerksPanelView`, passa a ser
compartilhado entre a aba CLASS e o painel do loading.

**Por que (não) importa:** os dois hosts **nunca coexistem** (tela de Skills no menu × tela de carregamento na raid), e
ambos mostram sempre a **mesma classe local** → o guard checa `childCount` do painel específico (arg), então cada painel
rebuilda o seu quando vazio. O estático compartilhado é benigno.

**Sugestão / decisão:** manter estático (funciona); comentar a invariante "hosts não coexistem" no `PerksPanelView`.
Opcional futuro: marcador por-painel (component) se algum dia coexistirem.

**Decisão:** `[x]` Aceitar — manter + documentar a invariante.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Revisão técnica 01 via `/review-technical-spec` — 0 🔴; 1 🟡 (auto-visível) + 2 🟢, todos resolvidos |
