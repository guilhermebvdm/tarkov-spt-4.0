# 055 — Detalhe da classe no lobby/loading · Code Review 02

**Mod:** CustomClasses
**Asbuild:** [055-class-detail-lobby-05-asbuild.md](055-class-detail-lobby-05-asbuild.md)
**Review anterior:** [055-class-detail-lobby-04-code-review-01.md](055-class-detail-lobby-04-code-review-01.md)
**Data:** 2026-07-03

> Segunda rodada com **revisor independente** (contexto limpo, não o autor) sobre os 3 arquivos + diff `663a38a`.
> **Nenhum 🔴/🟠.** Confirmou a extração 059→PerksPanelView como movimentação limpa (corpos idênticos, sem regressão)
> e o gate FIKA correto. Dois fixes aplicados aqui (F-3/F-9); o resto é verificar-in-game.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 3 · 🟢 2 · ✅ Aplicados nesta rodada: 2

## Verificações que passaram (não são bug — registradas)

| Vetor | Veredito |
| --- | --- |
| Reflection/timing/cast (`_loadingPlayers[netId]`, `IDictionary.Contains` int-boxed, cast `Component`) | OK — Postfix roda após `_loadingPlayers.Add`; boxing e cast válidos |
| Regressão da extração (059→PerksPanelView) | OK — corpos **idênticos** byte-a-byte; `CardHover`/`FadeIn` em 1 só arquivo (sem duplicata) |
| `_lastPanelClass` estático compartilhado | OK — o guard exige `childCount>0` do painel-alvo → painel novo sempre reconstrói (cache nunca fica stale) |
| Gate soft-detect FIKA (`TypeByName` + `if` no Plugin) | OK — `GetTargetMethod` nunca roda com `UiType==null`; tipo global resolve por FullName |
| `Refresh` no loading (`EnsureLoaded`/`LocalGroups`) | OK — dados locais prontos no momento do loading |
| Postfix protegido por try/catch | OK — a tela de loading nativa (todos os players) não é ameaçada |

## Achados

### CR-02-01 · Bug latente · 🟡 → ✅ Aplicado
**`Ensure()`/`Build` sem try/catch no caminho de hover (`OnPointerEnter`).**
O Postfix está protegido, e `Refresh` tem guard interno — mas `Ensure()` (que chama `Build`) rodava cru no
`OnPointerEnter`. Uma exceção ali (ex.: parent inesperado) subiria pela pilha do EventSystem.
**Fix:** extraí um `Show()` com try/catch e liguei `OnEnable` **e** `OnPointerEnter` nele. `ClassDetailLoadingPatch.cs`.

### CR-02-02 · Ciclo de vida · 🟢 → ✅ Aplicado
**Painel visível com a linha desativada (não destruída).** O painel é filho do Canvas, o hover está na linha.
**Fix:** `OnDisable` esconde o painel (`SetActive(false)`); `OnDestroy` continua destruindo. Cobre o caso de
desativação sem destruição. `ClassDetailLoadingPatch.cs`.

### CR-02-03 · Ciclo de vida · 🟡 — verificar in-game
**Re-add do player local via `ReInitAfterTransit`/`DeletePlayer` (mapa com trânsito).** No caminho normal (fim de
raid → `ClearData` destrói a linha → `OnDestroy` destrói o painel) está correto. O caso residual é o **trânsito**
(ex.: Streets), onde a linha do local pode ser removida e re-adicionada com o Canvas persistente. Sem vazamento
permanente (o `OnDestroy` da linha antiga limpa o painel antigo), mas vale confirmar.
**Ação:** testar num mapa com **trânsito**; se piscar/duplicar, ancorar o painel na própria linha.

### CR-02-04 · Posicionamento · 🟡 — verificar in-game
**Painel 600×460 à direita pode cobrir o `Stack` de players em 1280×720.** (= CR-01-01 da rodada anterior.)
Confirmado pela geometria (`anchor (1,0.5)`, `pivot (1,0.5)`, `-60px`). Ajuste de `sizeDelta`/posição no gate.

### CR-02-05 · Defensivo · 🟢 — aceito
**`GetComponentInParent<Canvas>()` null → fallback `transform.root`.** Cenário improvável (a linha é UI sob Canvas);
se ocorrer, o painel só não renderiza (sem crash). Aceito como defensivo.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-03 | Code review 02 (revisor independente) — 0 🔴/🟠; F-3/F-9 aplicados; 2 🟡 pro gate |
