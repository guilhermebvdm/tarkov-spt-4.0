# 010 — UI dos multiplicadores de skill · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [010-ui-multiplicadores-skill-01-spec.md](010-ui-multiplicadores-skill-01-spec.md)
**Spec técnica:** [010-ui-multiplicadores-skill-02-spec-tech.md](010-ui-multiplicadores-skill-02-spec-tech.md)
**Asbuild:** [010-ui-multiplicadores-skill-05-asbuild.md](010-ui-multiplicadores-skill-05-asbuild.md)
**Data:** 2026-06-07

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 3
>
> CR-01-02 e CR-01-03 aplicados via `/apply-code-review`. CR-01-01 (🟡 modo grade) **pendente** — aguarda validação in-game.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟡 | Modo grade da tela de Skills: seta/tooltip não cobertos | Pendente (validar in-game) |
| CR-01-02 | B — Bug latente | 🟢 | `SetMessageText("")` em hover pode piscar tooltip vazio | ✅ Aplicado |
| CR-01-03 | F — Melhoria | 🟢 | `Init` re-resolve tooltip + recria string a cada refresh | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · C — Gap vs. spec · 🟡 Médio

**Modo grade da tela de Skills: seta/tooltip não cobertos**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillPanelPatch.cs`](../../modded/Client/Patches/SkillPanelPatch.cs)

**Problema:** a tela de Skills tem alternância **lista/grade** (botões no topo direito — corner case da spec funcional). O marcador `±X%` e o tooltip dependem do `SkillPanel._name`, que existe no modo **lista**. No modo **grade**, as skills tendem a renderizar via `SkillIcon` direto (só o ícone, sem `SkillPanel`/nome) — então no modo grade provavelmente **só a borda** aparece (via `SkillIconBorderPatch`), sem seta nem tooltip. Não foi verificado in-game e o escopo não declara qual modo é suportado.

**Por que importa:** num modo de exibição o usuário vê metade do destaque (borda sim, seta/tooltip não) — pode parecer bug. A spec pedia "funcionar em ambos os modos OU declarar qual é suportado".

**Sugestão:** validar in-game os dois modos. Resolução mínima aceitável: **declarar** que seta+tooltip são do modo **lista** (a borda funciona em ambos) e registrar no as-built. Se quiser o tooltip também no modo grade, o `SkillIcon` já tem hover próprio (`HoverTrigger` no `_icon`) — daria pra anexar a frase ali num follow-up. Decidir.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (validar + declarar "lista" como modo suportado p/ seta/tooltip; borda nos dois)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-01-02 · B — Bug latente · 🟢 Menor · ✅ Aplicado em 2026-06-07

**`SetMessageText("")` enquanto em hover pode piscar tooltip vazio**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillPanelPatch.cs:44-50`](../../modded/Client/Patches/SkillPanelPatch.cs#L44)

**Problema:** no ramo `!has`, a ordem é `tmp.text=""` → `area.SetMessageText("")` → `marker.SetActive(false)`. `HoverTooltipArea.SetMessageText` chama `Show()` se `bool_1` (hover ativo) — e `Show()` não checa string vazia (só o `OnPointerEnter` checa). Se o cursor estiver sobre o marcador no exato refresh que o transforma em "sem fator" (scroll/reciclagem), pode haver um flash de tooltip vazio antes do `SetActive(false)`/`OnDisable` fechar.

**Por que importa:** cosmético e raro (precisa hover + reciclagem no mesmo frame), mas é um flicker evitável.

**Sugestão:** inverter a ordem no ramo `!has` — `marker.SetActive(false)` **antes** de `SetMessageText("")` (o `OnDisable` do componente já fecha o tooltip); ou simplesmente não chamar `SetMessageText` quando vai desativar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** [SkillPanelPatch.cs](../../modded/Client/Patches/SkillPanelPatch.cs) — ramo `!has` agora faz `marker.SetActive(false)` antes de qualquer mudança de mensagem (removido o `SetMessageText("")`); `OnDisable` do `HoverTooltipArea` fecha o tooltip.

### CR-01-03 · F — Melhoria · 🟢 Menor · ✅ Aplicado em 2026-06-07

**`Init` re-resolve o tooltip e recria a string a cada refresh**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillPanelPatch.cs:54-56`](../../modded/Client/Patches/SkillPanelPatch.cs#L54)

**Problema:** `method_1` roda a cada ganho de XP/refresh; a cada vez chama `area.Init(ItemUiContext.Instance.Tooltip, MultiplierFormat.TooltipText(...), rawText:true)`, que re-resolve o `SimpleTooltip` e remonta a string. Não é hot-path por-frame (refresh de UI de skill), então o custo é baixo.

**Por que importa:** alocação/trabalho desnecessário repetido; só vale como limpeza.

**Sugestão:** opcional — resolver o `SimpleTooltip` uma vez na criação (`GetOrCreateMarker`) e nos refreshes usar só `area.SetMessageText(text, rawText:true)`. Ou aceitar como está (legível e barato).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** [SkillPanelPatch.cs](../../modded/Client/Patches/SkillPanelPatch.cs) — `GetOrCreateMarker` chama `area.Init(ItemUiContext.Instance.Tooltip, "", rawText:true)` **uma vez** na criação; os refreshes usam só `SetMessageText(...)` (sem re-resolver o tooltip).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 01 criada via `/code-review` — 0 🔴 · 0 🟠 · 1 🟡 · 2 🟢. (Descartado um falso-positivo de "cache stale entre perfis": `RequestHandler.SessionId` é readonly/boot e trocar de perfil no SPT relança o jogo.) |
| 2026-06-07 | `/apply-code-review` — aplicados CR-01-02, CR-01-03; pendente CR-01-01 (🟡 modo grade, aguarda validação in-game). Recompilado 0 warn/err. |
