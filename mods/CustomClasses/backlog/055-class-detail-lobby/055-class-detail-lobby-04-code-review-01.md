# 055 — Detalhe da classe no lobby/loading · Code Review 01

**Mod:** CustomClasses
**Asbuild:** [055-class-detail-lobby-05-asbuild.md](055-class-detail-lobby-05-asbuild.md)
**Data:** 2026-07-02

> Análise crítica do código do `/code-mod` (Fatias 1+2). Compila 0/0. **Nenhum 🔴/🟠.** Os achados são
> "verificar in-game" (🟡, gate humano) + cosméticos (🟢). Item **code-complete**, aguardando validação visual.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 3 · 🟢 2 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | Posição/tamanho do painel (600×460 à direita) pode cobrir o Stack ou sair da tela | `[ ]` Verificar in-game |
| CR-01-02 | C — Gap vs. spec | 🟡 | `TypeByName("LoadingScreenUI")` (tipo sem namespace) precisa resolver — senão feature silenciosa | `[ ]` Verificar in-game (log) |
| CR-01-03 | F — Melhoria | 🟡 | Auto-visível pode ser intrusivo na tela de carregamento | `[ ]` Verificar in-game |
| CR-01-04 | D — Arquitetura | 🟢 | Image de fundo do painel captura raycast na área durante o loading | `[ ]` Aceito |
| CR-01-05 | E — Regressão | 🟢 | Paridade da aba CLASS pós-extração do `PerksPanelView` | `[ ]` Verificar in-game |

---

### CR-01-01 · B — Bug latente · 🟡

**Posição/tamanho do painel compacto pode cobrir o Stack de players ou sair da tela**

**Local:** [`ClassDetailLoadingPatch.cs` `LoadingClassHover.Ensure`](../../modded/Client/Patches/ClassDetailLoadingPatch.cs)

**Problema:** o painel é reancorado a `600×460` à direita (`anchoredPosition (-60,0)`, pivot `(1,0.5)`). Valores
empíricos — pode sobrepor a lista de players (`Stack`) ou, em resolução baixa (1280×720), invadir o centro.

**Sugestão:** verificar in-game e ajustar `sizeDelta`/`anchoredPosition` (ou ancorar num canto livre). Sem risco
funcional — só posicionamento.

**Decisão:** `[ ]` Verificar in-game

---

### CR-01-02 · C — Gap vs. spec · 🟡

**`TypeByName("LoadingScreenUI")` precisa resolver o tipo (FIKA, sem namespace)**

**Local:** [`Plugin.cs` gate FIKA](../../modded/Client/Plugin.cs) + [`ClassDetailLoadingPatch.cs:22`](../../modded/Client/Patches/ClassDetailLoadingPatch.cs#L22)

**Problema:** `LoadingScreenUI` está no **namespace global** (sem namespace no fonte FIKA). Se `AccessTools.TypeByName("LoadingScreenUI")` não casar (assembly/naming), o gate no `Plugin.cs` fica `false` → o patch **não habilita**
silenciosamente (degrada, mas sem feature).

**Sugestão:** confirmar pelo log `[CustomClasses] (055) FIKA detectado — detalhe da classe no loading da raid.` no
BepInEx console. Se **não** aparecer com FIKA presente, ajustar o nome (FullName com assembly, ou varrer por `Name`).

**Decisão:** `[ ]` Verificar in-game (conferir a linha de log)

---

### CR-01-03 · F — Melhoria · 🟡

**Auto-visível pode ser intrusivo**

**Local:** [`ClassDetailLoadingPatch.cs` `LoadingClassHover.OnEnable`](../../modded/Client/Patches/ClassDetailLoadingPatch.cs)

**Problema:** PA-01-01 resolveu pelo auto-visível (robusto, não depende de EventSystem). Mas isso mantém o painel
**sempre aberto** durante o loading — pode poluir a tela.

**Sugestão:** avaliar no gate. Se intrusivo e o EventSystem estiver ativo, trocar pra **hover-only** (o toggle já está
implementado em `OnPointerEnter`/`Exit` — bastaria não exibir no `OnEnable`).

**Decisão:** `[ ]` Verificar in-game (auto vs hover)

---

### CR-01-04 · D — Arquitetura · 🟢

**Image de fundo do painel captura raycast na sua área**

**Local:** [`PerksPanelView.Build`](../../modded/Client/PerksPanelView.cs) (Image de fundo, `raycastTarget` default true)

**Problema:** o `Image` de fundo do painel tem `raycastTarget=true` (default) → captura input na sua área durante o
loading. A tela de carregamento não tem interação crítica, então o impacto é nulo; até ajuda o hover-toggle.

**Decisão:** `[x]` Aceito (sem impacto)

---

### CR-01-05 · E — Regressão · 🟢

**Paridade da aba CLASS pós-extração do `PerksPanelView`**

**Local:** [`SkillsClassTabPatch.cs`](../../modded/Client/Patches/SkillsClassTabPatch.cs) + [`PerksPanelView.cs`](../../modded/Client/PerksPanelView.cs)

**Problema:** os métodos do painel foram movidos do `SkillsClassTabPatch` para o `PerksPanelView` sem alterar corpo. A
aba CLASS deve seguir idêntica ao 059 (mesmo código), mas é uma regressão possível do refactor.

**Sugestão:** conferir in-game que a aba CLASS continua idêntica (2 colunas, cards, ícone, posição). Compile 0/0 + bytes
coerentes já dão confiança; o gate fecha.

**Decisão:** `[ ]` Verificar in-game (paridade da aba)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-02 | Code review 01 via `/code-review` — 0 🔴/🟠; 3 🟡 (verificar in-game) + 2 🟢 |
