# 037 — Performance: cache de validação + índices do catálogo · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** lentidão reportada pelo usuário no viewer + análise dos hot paths
**Épico:** UX do editor (030–037) · **Wave:** UX-W0 — **PRIMEIRO de todos** (030+ consomem o cache) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 037`. Não é a spec.

## Diagnóstico (confirmado no código)

1. **`ClassEditorService.ListClassFiles()` roda `ClassRegistrar.ValidateAndBuild` (dry-run COMPLETO: deep clone do template base inteiro via `ICloner` + `InventoryBuilder` empacotando loadout + outfit + hideout) para CADA arquivo** (`ClassEditorService.cs:114`) — e é chamado por **todas** as páginas: `Classes.razor:126`, `ClassDetail.razor:407`, `ClassEdit.razor:580`. Navegar para QUALQUER vista = 11 dry-runs pesados.
2. **Prerender duplo do Blazor**: cada página executa `OnInitialized` 2× (prerender estático + attach do circuito) → **22 dry-runs por navegação**. É a lentidão dominante que o usuário sente ao trocar de página.
3. **`CatalogService` sem índices:** `Search` varre `GetItems()` inteiro com resolução de locale por item a cada busca (`CatalogService.cs:175`); `GetClothing` varre customization + locale a cada render da aba Outfit (`:520` — e a aba monta 4 pickers); árvore de categorias recalculada por chamada (`:235-293`).
4. **`ClassEdit` recalcula caro demais por interação:** cada mudança dispara `ToDefinition()` + `ComputeLoadoutCost` **2×** (total e stash-only, `ClassEdit.razor:784-787`) + `CheckStashCapacity` (probe trees + GridPacker, `:791`) — em loadouts grandes, isso roda a cada keystroke de count.

## Escopo

- **(a) Cache de entries no `ClassEditorService`:** `ClassFileEntry` cacheada por arquivo, keyed por `(fileName, mtime+length)` — dry-run só quando o arquivo mudou. Invalidação: `Save`/`Delete`/`Create`/`Duplicate` (já passam pelo service) invalidam a própria entry; revalidação por mtime cobre edição externa (`/sync-classes`, mão). `ListClassFiles()` quente = leitura de dicionário. O `ListClassSummaries()` do 030 **é uma view desse cache** (não uma segunda implementação).
- **(b) Índices lazy no `CatalogService`:** índice de busca pré-computado 1× (`Lazy<T>`: tpl + nome en/pt lowercase + shortname + categoria + preço — DB é imutável pós-boot); `GetClothing` cacheado por side; árvore de categorias cacheada. `Search` quente vira scan de lista compacta em memória.
- **(c) Recompute do `ClassEdit` mais barato:** derivar o custo do stash do breakdown do custo total (1 chamada de `ComputeLoadoutCost`, não 2); `CheckStashCapacity` só quando a aba Stash está visível; **throttle/debounce ~250ms** no recompute durante digitação.
- **(d) Prerender:** investigar `prerender: false` por página (render mode do host permite override?) — se não, o cache (a) já torna o prerender barato; registrar a decisão na spec.
- **(e) Medição (obrigatória no DoD):** instrumentar com `Stopwatch` (log debug) os tempos de `ListClassFiles` frio/quente, `Search`, navegação entre páginas — números ANTES e DEPOIS no as-built.

## Riscos / atenção

- Cache de entries guarda `RegistrationPlan`/`Definition` — atenção a aliasing (entry cacheada não pode ser mutada pelo form de edição; `ClassEditModel.FromDefinition` já copia, conferir).
- Invalidação por mtime tem resolução de FS (~1-2s) — aceitável (uso local); documentar.
- Não otimizar o que não dói: `ProfilesUsingEdition` (só no dialog de delete, em Task.Run) fica como está.

## Refs

- `ClassEditorService.cs:88-145` (ListClassFiles/dry-run), `ClassRegistrar.cs` (ValidateAndBuild), `CatalogService.cs:175/520` (scans), `Web/Pages/ClassEdit.razor:712/784-791` (recompute)

## DoD (resumo)

- Navegação lista→detail→edit com cache quente **sem dry-run nenhum** (logs comprovam); números antes/depois no as-built.
- Busca de item e aba Outfit sem scan completo do DB por interação.
- Edição de count/nível não trava digitação (recompute throttled).
