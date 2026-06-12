# 035 — Densidade global + redução de cliques · Code Review 01

**Mod:** CustomClasses
**As-built revisado:** [035-densidade-cliques-05-asbuild.md](./035-densidade-cliques-05-asbuild.md)
**Spec técnica:** [035-densidade-cliques-02-spec-tech.md](./035-densidade-cliques-02-spec-tech.md)
**Review técnica:** [035-densidade-cliques-03-spec-tech-review-01.md](./035-densidade-cliques-03-spec-tech-review-01.md)
**Data:** 2026-06-12

> Revisão do código **realmente entregue** (git diff dos 7 arquivos editados + 2 criados: `customclasses.js`, `UiPrefs.cs`). Execução **autônoma** (usuário ausente, não aprovável): aplicam-se apenas achados **SEGUROS** (correção de null/crash, build-breaker, fuga de spec com fix local inequívoco, leak/dispose, JS interop quebrado). Achados de design/layout ou de fix ambíguo ficam **ADIADOS** (não tocam código). Build de referência: `dotnet build -c Release --no-incremental` → **0 erros / 0 avisos** (MudBlazor 8.13.0, net9.0).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 SEGUROS · Adiados: 4 · Aplicados: 0

**Veredito:** o código está correto, compila limpo, e implementa fielmente a spec técnica + os refinamentos PA-R1-01..09. Não há achado na classe SEGURA (nenhum null/crash, build-breaker, fuga de spec, leak/dispose ou interop quebrado). Os 4 pontos abaixo são trade-offs de timing/design **já reconhecidos** na spec/review e ficam ADIADOS — tocá-los seria reabrir decisão de design sem aprovação.

## O que foi verificado e está correto

- **Dispose / leak (CR scope: leak/dispose).** `ClassEdit.Dispose` chama `ccPrefs.unregisterSaveShortcut` (em `try/catch`) **e** `_dotNetRef?.Dispose()`. O JS `registerSaveShortcut` chama `unregisterSaveShortcut()` na entrada → no máximo **um** `keydown` global existe a qualquer momento (sem acúmulo entre instâncias de `ClassEdit`). `NavMenu` continua `IDisposable` e remove o `LocationChanged`. Sem vazamento.
- **JS interop (CR scope: interop quebrado).** `window.ccPrefs` é IIFE plana (`<script src>`, não-módulo) servida pelo mesmo mount do css/icons — padrão comprovado do repo. `get/set/remove` com `try/catch` (localStorage negado/quota → degrada). `invokeMethodAsync('OnSaveShortcut')` casa com `[JSInvokable] public async Task OnSaveShortcut()`. `registerSaveShortcut` recebe `DotNetObjectReference<ClassEdit>`. Assinaturas batem.
- **Prerender x interop (CR scope: crash).** Todo acesso a `IJSRuntime` está em `OnAfterRenderAsync(firstRender)` ou em handler de evento — nunca em `OnInitialized`/`OnParametersSet`. `UiPrefs` engole `JSException`+`InvalidOperationException` → default. Sem `InvalidOperationException` de prerender.
- **Null safety (CR scope: NRE).** Sort lambdas usam `r.SkillCost?.Total ?? double.MaxValue` e `r.HasError ? double.MaxValue : r.LoadoutTotal` (sem NRE em classe inválida). Botões Edit `Disabled` quando `Definition is null` / só renderizam com `HasDefinition`. Parsing de `localStorage` (`Split('|')` com checagem de `Length`, `int.TryParse`) tolera chave corrompida.
- **Build-breaker (CR scope).** `ToggleSortDirection()` existe e retorna `Task` no MudBlazor 8.13.0 (awaited). `MudIcon` sem `title` (título no `<span>` wrapper — evita prop inexistente). `_imports.razor` ganhou `@using Microsoft.JSInterop`. Compila 0/0.
- **Fuga de spec (CR scope).** `SkillsTabIndex=1` espelha o mapa de abas do `ClassEdit` com comentário cruzado nos dois arquivos. `ClampTab(0..StashTabIndex)`. Query `?tab` aplicada uma vez por instância (`_tabFromQueryApplied`). Tudo conforme PA-035-04 / PA-R1-03.

## Achados ADIADOS (não tocam código — design/timing reconhecido)

### CR-01-01 · 🟢 `OnSaveShortcut` pode rodar em componente disposto (race) — ADIADO

**Categoria:** B — Edge Case. **Arquivo:** `ClassEdit.razor` (`OnSaveShortcut` / `Dispose`).

Entre o keypress e a entrega do `invokeMethodAsync`, se o `ClassEdit` for disposto, `InvokeAsync(...)` pode lançar `ObjectDisposedException`. **Por que ADIAR (não é SEGURO):** `Dispose` desregistra o `keydown` **antes** que novos eventos cheguem, e o guard de pathname `/edit` no JS é backstop adicional; uma chamada em voo vira promise rejeitada **não-observada** no lado JS (inócua, sem crash de UI). É a race estreita já reconhecida em PA-R1-06 e no risco "DotNetObjectReference" da spec §7. Um fix (ex.: `if (_disposed) return;` no topo do `OnSaveShortcut`) seria defensável, mas mexe em fluxo de evento já coberto por dispose — fica para decisão de design, não como correção de crash inequívoca.

### CR-01-02 · 🟢 `_editTab` no NavMenu re-lido fire-and-forget no `LocationChanged` — ADIADO

**Categoria:** B — Edge Case. **Arquivo:** `NavMenu.razor` (`OnLocationChanged` → `RefreshEditTabAsync`).

`RefreshEditTabAsync` não é aguardado; `_editTab` atualiza "um render depois". Teoricamente um clique imediatíssimo numa outra classe logo após navegar poderia carregar a aba anterior. **Por que ADIAR:** explicitamente projetado e documentado (comentário no código + PA-035-04): a query `?tab` só é lida quando o usuário clica em outra classe; a defasagem de um render é aceitável (single-user local). Alterar para await síncrono mudaria o modelo de timing — design, não bug.

### CR-01-03 · 🟢 Flash default→persistido a cada navegação para página com pref — ADIADO

**Categoria:** B — Edge Case. **Arquivos:** todos os `OnAfterRenderAsync` (drawer pin, aba, ordenação, toggles, filtro).

A reconciliação pós-circuito monta com default e depois aplica o valor salvo → "flash" de um frame. **Por que ADIAR:** aceito em PA-035-03 / PA-R1-09 (single-user local; o prerender estático não tem JS). Não há fix local sem mudar a estratégia de render (ex.: persistência server-side), que está fora de escopo.

### CR-01-04 · 🟢 Restauração de ordenação re-dispara `SortDirectionChanged` (re-persistência) — ADIADO

**Categoria:** A — Gap (mecanismo). **Arquivo:** `Classes.razor` (`OnAfterRenderAsync` → `ToggleSortDirection`).

Restaurar via `ToggleSortDirection()` re-dispara `SortDirectionChanged` → `OnSortChanged` re-grava o **mesmo** valor. **Por que ADIAR:** idempotente (mesmo `<label>|<dir>`), envolto em `try/catch` que degrada para ordem-de-arquivo se a API do Mud divergir. Sem efeito incorreto; é o mecanismo confirmado contra o `MudBlazor.xml` 8.13.0 (PA-AB-035-02). Validação visual fica para o runtime.

## Aplicados

Nenhum. Não há achado na classe SEGURA.

## Build após review

`dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release --no-incremental --nologo` → **0 erros / 0 avisos** (sem alterações de código nesta passada; build reconfirma o baseline da entrega).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Code review 01 criado via `/code-review` (autônomo — usuário ausente). 0 achados SEGUROS; 4 trade-offs de timing/design reconhecidos pela spec/review-01 ADIADOS (CR-01-01..04). Build de referência 0/0. Nenhuma alteração de código aplicada. |
