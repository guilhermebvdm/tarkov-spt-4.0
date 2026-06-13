# 037 — Performance: cache de validação + índices do catálogo — As-built

**Mod:** CustomClasses
**Data:** 2026-06-12
**Refs:** [01-spec](./037-performance-cache-01-spec.md) · [02-spec-tech](./037-performance-cache-02-spec-tech.md) · [03-review](./037-performance-cache-03-spec-tech-review-01.md)

> Mod server-only (Blazor Server + serviços DI SPT). Sem patch Harmony. Implementação autônoma (usuário ausente — premissas registradas abaixo, não houve aprovação interativa).

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/ClassEditorService.cs` | EDITADO — **(a) cache de validação**. `ConcurrentDictionary<string, CachedEntry>` keyed por `FileName`, validade por `FileStamp(LastWriteTimeUtc.Ticks, Length)`. `ListClassFiles` refatorado em hot/cold: HOT = leitura de dicionário (zero dry-run), COLD = `BuildEntry` (parse + `ValidateAndBuild`) gravado no cache. Descarte de órfãos na varredura (corner case §50). `BuildEntry` extraído (recebe `Func<string,bool> isRegistered` p/ não acoplar ao tipo de `GetProfileTemplates()`). `ApplyCrossFileCollisions` extraído — cria **lista nova** (`entry with { Diagnostics = [.. entry.Diagnostics, diag] }`) só p/ a lista de retorno; cache fica com a lista limpa (PA-R1-02). `Invalidate(fileName)` em `Save` (após write) e `Delete` (Create/Duplicate herdam via `Save`). **Contrato 037→030:** `public IReadOnlyList<ClassFileEntry> GetCachedEntries()` com XML-doc (gancho `// 030: ...`). Stopwatch `[perf] ListClassFiles: X hot / Y cold in Z ms`. |
| `modded/Server/CatalogService.cs` | EDITADO — **(b) índices lazy**. Primary-ctor trocado por ctor de corpo (field-initializers não podem referenciar params do primary-ctor ao montar os `Lazy`). 4 `Lazy<T>`: `_searchIndex` (`SearchIndexRow` pré-computado: tpl + en/pt/short/templateName lower + display + categoria), `_handbookIndex` (tpl→categoria), `_childrenByParent` (árvore pai→filhos — PA-R1-04, substitui o `_categoryDescendants` ambíguo), `_clothingBySide` (Usec/Bear em 1 passada). `Search` varre `_searchIndex.Value` com as **5 fontes de match** originais (PA-R1-01: `TemplateNameLower` preservado) e ordem idêntica categoria→match→filter→GetPrice→add→cap (PA-R1-06). `GetClothing` lê `_clothingBySide.Value`. `CollectCategoryWithDescendants` faz só BFS sobre o mapa cacheado. Logger `ISptLogger<CatalogService>` injetado (PA-R1-08). Stopwatch em `Search`/`BuildSearchIndex`/`BuildClothingBySide`. |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — **(c) recompute mais barato**. `RecomputeLoadoutCost`: **1** `ComputeLoadoutCost` no caminho quente; `_stashCost`/`CheckStashCapacity` só com a aba Stash visível (`_stashTabVisible`, índice 6, via `@bind-ActivePanelIndex="ActivePanelIndex"`). Entrar na aba Stash dispara 1 recompute (popula subtotal/capacidade antes do 1º display). `ScheduleRecompute` (debounce 250 ms via `CancellationTokenSource` + `Task.Delay` + `InvokeAsync`) ligado nos `ItemSpecEditor OnChanged` (equipped + stash); add/remove de linha segue imediato. `FlushPendingRecompute` no início de `SaveAsync` (corner case §49/§53 — capacidade nunca fica não-computada). `@implements IDisposable` + guard `_disposed` no `StateHasChanged` pós-delay (PA-R1-07). Stopwatch em `LoadFromDisk` (`[perf] ClassEdit.LoadFromDisk`). Logger via `@inject ISptLogger<ClassEditorService>`. |
| `modded/Server/Web/ClassEditModel.cs` | NÃO MODIFICADO (confirmação §4/§7) — `FromDefinition` já faz cópia profunda (`ItemSpecModel.FromSpec` recursa em Contents/Mods; `ToDict`/`BuildLoadout` reconstroem). O form não aliasa a `Definition` cacheada. Verificado e mantido intocado. |
| `modded/Server/ClassRegistrar.cs` | NÃO MODIFICADO — a cache vive no `ClassEditorService`; `ValidateAndBuild` intocado (nenhuma mudança foi necessária p/ suportar o cache). |

## Decisões de implementação

- **`BuildEntry` recebe `Func<string,bool> isRegistered`** em vez do dicionário de templates tipado: o tipo de retorno de `DatabaseService.GetProfileTemplates()` não está em uso explícito no mod e nomeá-lo num parâmetro acoplaria desnecessariamente; a closure `IsRegistered` no `ListClassFiles` resolve `templates.ContainsKey` mantendo o tipo inferido (`var templates = ...`).
- **`CatalogService` deixou de usar primary constructor**: o wiring dos `Lazy<T>` (que referenciam métodos de instância `BuildXxx`) exige um corpo de ctor — field-initializers de `Lazy` não enxergam params do primary-ctor. Campos `readonly` privados substituem os params; comportamento idêntico.
- **`_categoryDescendants` → `_childrenByParent`** (PA-R1-04): cacheia a árvore pai→filhos (parte cara) e mantém o BFS por-root por chamada (barato), em vez de um dicionário por-root de escrita ambígua. `CollectCategoryWithDescendants` mantém assinatura/semântica.
- **`GetClothing` agora retorna a lista cacheada por referência** (não mais uma lista fresca por chamada). Consumidores (`ClassEdit.LoadCatalogs` → `_clothingNames`, `CustomizationPicker`) só leem — respeitam o invariante read-only dos índices (§7).
- **Preço NÃO congelado no índice** (PA-037-08): `GetPrice` continua por-hit (lê o flea table, sobrescrito em runtime neste repo). O índice elimina a resolução de locale no scan completo, não o lookup de preço.
- **Logger no `ClassEdit.razor`**: injetado `ISptLogger<ClassEditorService>` (tipo concreto conhecido, registro open-generic do SPT) só para os logs `[perf]` de navegação — não havia logger na página.
- **Aba Stash index 6 hard-coded** (`StashTabIndex`): comentário documenta a ordem dos 7 painéis. Se a ordem das abas mudar, ajustar a constante.

## Premissas autônomas (usuário ausente)

- **PA-AB-01:** entrar na aba Stash dispara um `RecomputeLoadoutCost` (via setter `ActivePanelIndex`) para que subtotal/capacidade — pulados no caminho quente da aba Equipped — estejam frescos no 1º render da aba. Decisão própria do code-mod (a spec só exigia "só com aba Stash visível"; sem isto o subtotal apareceria stale/nulo ao abrir a aba).
- **PA-AB-02:** `FlushPendingRecompute` roda o recompute **completo** (capacidade incluída) independentemente da aba ativa, para garantir o invariante §53 (capacidade nunca "nunca computada") mesmo que o Save ocorra na aba Equipped durante a janela de debounce.
- **PA-AB-03 (PA-R1-03):** os consumidores (`ClassEdit`/`ClassDetail`/`Classes`) tratam `entry.Diagnostics` como **imutável** (só leem). Não foi adicionada cópia defensiva em `LoadFromDisk` (`_loadDiagnostics = entry.Diagnostics`) — nenhum caminho de UI muta a lista hoje, e a passada CR-EP-06 já não toca a lista cacheada (PA-R1-02). Anotado como invariante a respeitar; cópia defensiva fica como hardening futuro de baixo custo.
- **PA-AB-04:** debounce fixo em 250 ms (constante inline). Não exposto como config (mod server, sem F12).

## Build

- `dotnet build` — NÃO executado neste estágio (estágio dedicado faz o build). TODO validar 0 erros / 0 warnings.

## Medição before/after (TODO — preencher na validação runtime)

> Coletar dos logs `[perf]` (LogDebug) no server real SPT 4.0, navegando no editor com N classes instaladas. Memória `feedback_spt_validation`: escrita+hash não basta, validar no jogo/UI.

| Métrica | Before (sem cache) | After (com cache) | Fonte do número |
|---|---|---|---|
| `ListClassFiles` 1ª navegação (frio) | TODO | TODO | `[perf] ListClassFiles: 0 hot / N cold in X ms` |
| `ListClassFiles` navegação seguinte (quente) | TODO | TODO | `[perf] ListClassFiles: N hot / 0 cold in Y ms` (esperado ≈ 0 dry-run) |
| `ListClassFiles` após 1 Save | TODO | TODO | esperado `N-1 hot / 1 cold` (revalida exatamente 1 entry) |
| `Search` 1ª busca (constrói índice) | TODO | TODO | `[perf] BuildSearchIndex: R rows in X ms` + `Search ... (cold/index-built)` |
| `Search` buscas seguintes (quente) | TODO | TODO | `[perf] Search '...': K hit(s) over R rows (hot) in Y ms` |
| `ClassEdit.LoadFromDisk` (frio/quente) | TODO | TODO | `[perf] ClassEdit.LoadFromDisk '...' in Z ms` |
| Recompute por keystroke (aba Equipped) | TODO (≥2 ComputeLoadoutCost + capacidade) | TODO (1 ComputeLoadoutCost, debounced) | inspeção de comportamento + tempo de resposta do campo |

## Verificação funcional (TODO — validação runtime)

- [ ] Mesmos diagnósticos / custos / capacidade que antes do 037 (nenhum resultado funcional muda — spec §40).
- [ ] Busca: nenhum item some por falta de locale; match por `template.Name` interno preservado (PA-R1-01).
- [ ] Navegação quente: `0 cold` dry-run; após Save: exatamente `1 cold`.
- [ ] Edição externa de um arquivo (`/sync-classes` ou hand-edit) reflete na próxima leitura (revalidação por mtime).
- [ ] Colisão de nome entre 2 arquivos não "gruda"/acumula no cache (renomear/remover um → erro some na próxima navegação) (PA-R1-02).
- [ ] Campo count/level não trava ao digitar rápido (debounce); Save durante a digitação persiste o valor correto (flush).
- [ ] (d) Prerender: medir 2ª navegação; se prerender > ~50 ms decidir `prerender:false`, senão manter (PA-037-05). Registrar número.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | As-built criado via `/code-mod` (autônomo). Cache (a) + índices lazy (b) + recompute (c) + instrumentação (e) implementados. Medição before/after e verificação in-game deixadas como TODO para o estágio de validação runtime. |
