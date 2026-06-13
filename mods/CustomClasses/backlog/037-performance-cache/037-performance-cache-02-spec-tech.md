# 037 — Performance: cache de validação + índices do catálogo · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [037-performance-cache-01-spec.md](037-performance-cache-01-spec.md)
**Criado:** 2026-06-12

> Este item é um mod **server-only** (Blazor Server + serviços DI SPT). Não há patch de `Assembly-CSharp` / Harmony — a seção "Pontos de patch" do template não se aplica; o trabalho é cache em memória sobre serviços já existentes. As referências de código são para o **próprio mod** (`mods/CustomClasses/modded/Server/...`) e para os serviços SPT (`DatabaseService`, `LocaleService`) que ele consome.

## 1. Estratégia

Três caches independentes, nenhum altera comportamento observável (mesmos diagnósticos, custos, capacidade):

1. **(a) Cache de `ClassFileEntry`** no `ClassEditorService`, keyed por `(FileName, mtime-ticks, length)`. O dry-run pesado (`ClassRegistrar.ValidateAndBuild` → deep clone + `InventoryBuilder`) só roda quando a chave de um arquivo muda. `ListClassFiles()` quente = varredura de diretório (barata) + leitura de dicionário. A passada agregada de colisão de nome (CR-EP-06) roda **sempre**, sobre as entries (cacheadas ou não).
2. **(b) Índices `Lazy<T>`** no `CatalogService`: índice de busca pré-computado, `GetClothing` por side, árvore de categorias. Construídos no **primeiro acesso pós-boot** (nunca eager no ctor do singleton — premissa de DB imutável pós-boot, ver §7).
3. **(c) Recompute mais barato** no `ClassEdit.razor`: uma chamada de `ComputeLoadoutCost` derivando o subtotal stash do breakdown; `CheckStashCapacity` só quando a aba Stash está visível; debounce ~250 ms do recompute durante digitação.

Mais **(e)** instrumentação `Stopwatch` (log debug) nos hot paths e **(d)** decisão sobre prerender.

### Premissas autônomas (revisão 2026-06-12, usuário ausente — não aprovável)

- **PA-037-01 (chave de cache):** a chave usa `(mtime-ticks, length)` em vez de hash de conteúdo. Resolução de FS ~1–2 s é aceita (uso local single-user); a invalidação interna por `Save`/`Delete`/`Create`/`Duplicate` cobre o corner case "duas escritas no mesmo segundo" independentemente do mtime (corner case da spec funcional §45). `length` complementa o mtime para reduzir colisão de granularidade.
- **PA-037-02 (cache de entry, não da lista):** o cache é um `ConcurrentDictionary<string, CachedEntry>` keyed por `FileName`. A **varredura de diretório** (`fileUtil.GetFiles`) continua rodando a cada `ListClassFiles()` — só o resultado do dry-run por arquivo é cacheado. Isso satisfaz "arquivo novo/deletado por fora aparece/some" (corner cases §50/§51). Entradas órfãs (arquivo sumiu) são removidas do dicionário na varredura.
- **PA-037-03 (thread-safety):** `ConcurrentDictionary` para o cache de entries; `Lazy<T>` em modo default `ExecutionAndPublication` para os índices do catálogo. Sem lock adicional — entries são imutáveis após criadas (record + listas só lidas), índices são read-only após construção.
- **PA-037-04 (derivação stash-only):** o subtotal do stash **não** pode ser obtido filtrando `LoadoutCostBreakdown.Items` por `Context == "stash"`, porque contents/ammo de itens do stash recebem contexto `"contents"`/`"ammo"` — rótulo compartilhado com o equipado (confirmado em `CostService.cs:38` e no as-built do 028 §61). Solução sem mudar `CostService`: ver §5 (computar a partir da definição só-stash continua sendo 1 passada extra barata, OU mudar `CostService` para taggear proveniência — decisão: **manter a 2ª chamada só-stash, mas dispará-la apenas quando a aba Stash está visível**, igual `CheckStashCapacity`; com a aba fechada o custo total já é 1 chamada e o stash subtotal não é exibido). Isso satisfaz o item (c) "1 chamada de `ComputeLoadoutCost` para o custo total" no caminho quente de digitação na aba Equipped, sem regredir o subtotal do 028.
- **PA-037-05 (prerender):** default = manter o prerender e confiar no cache (decisão da spec funcional §61). Só investigar `prerender:false` se a medição pós-cache mostrar > ~50 ms de prerender na 2ª navegação. Registrado em §7/§8.

## 2. Pontos de patch

Não aplicável — mod server-only, sem patch de `Assembly-CSharp`/Harmony. Ver §1.

## 3. Novas propriedades F12 (BepInEx)

Não aplicável — sem `ConfigEntry` (mod server, não plugin BepInEx).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| [`modded/Server/ClassEditorService.cs`](../../modded/Server/ClassEditorService.cs) | MODIFICAR | (a) Cache `ConcurrentDictionary` de entries keyed por `(mtime,length)` em `ListClassFiles()` (`:88-141`); invalidação em `Save`/`Delete`/`Create`/`Duplicate` (`:172/222/330/368`). **Contrato 037→030:** expor `IReadOnlyList<ClassFileEntry> GetCachedEntries()` (view leve da cache, sem re-rodar dry-run) + `Stopwatch` (e). |
| [`modded/Server/CatalogService.cs`](../../modded/Server/CatalogService.cs) | MODIFICAR | (b) `Lazy<List<SearchIndexRow>>` (tpl+en+pt+shortname+categoria+preço lower); `Lazy<…>` por side em `GetClothing` (`:515`); `Lazy<…>` da árvore de categorias (`:232/244`). `Search` (`:161`) varre a lista compacta. Corrigir comentário obsoleto `:231`. `Stopwatch` em `Search`. |
| [`modded/Server/Web/Pages/ClassEdit.razor`](../../modded/Server/Web/Pages/ClassEdit.razor) | MODIFICAR | (c) `RecomputeLoadoutCost` (`:776-792`): 1 `ComputeLoadoutCost` no caminho quente; `_stashCost`/`CheckStashCapacity` só com aba Stash visível; debounce ~250 ms (`System.Timers.Timer`/`CancellationTokenSource`) nos handlers de count/level; flush do pendente antes de `SaveAsync` (`:650`). `Stopwatch` em `LoadFromDisk` (`:567`). |
| [`modded/Server/Web/ClassEditModel.cs`](../../modded/Server/Web/ClassEditModel.cs) | NÃO MODIFICAR (confirmação) | `FromDefinition` (`:176`) já faz cópia profunda (`ItemSpecModel.FromSpec` recursa em Contents/Mods criando objetos novos; `ToDict`/`BuildLoadout` reconstroem). O form **não aliasa** a `Definition` cacheada. Ver §7 (aliasing). |
| `modded/Server/ClassRegistrar.cs` | NÃO MODIFICAR | A cache vive no `ClassEditorService`; `ValidateAndBuild` permanece intocado. Listado só para registrar que não precisou de mudança. |

## 5. Stubs de código

> Server-only — sem `// ref: Assembly-CSharp`. Refs apontam para o próprio mod e serviços SPT.

### (a) Cache de entries + contrato 037→030

```csharp
// modded/Server/ClassEditorService.cs
using System.Collections.Concurrent;
using System.Diagnostics;

// Chave de validade do cache de uma entry — invalida em mudança de mtime OU tamanho.
private readonly record struct FileStamp(long MTimeTicks, long Length);

private sealed record CachedEntry(FileStamp Stamp, ClassFileEntry Entry);

// keyed por FileName (bare). Thread-safe: Blazor Server pode ter circuitos concorrentes (PA-037-03).
private readonly ConcurrentDictionary<string, CachedEntry> _entryCache = new(StringComparer.OrdinalIgnoreCase);

public List<ClassFileEntry> ListClassFiles()
{
    var sw = Stopwatch.StartNew();
    var hits = 0; var misses = 0;

    var entries = new List<ClassFileEntry>();
    if (!fileUtil.DirectoryExists(ClassesPath)) { return entries; }

    var files = /* … mesma varredura *.json/*.jsonc ordenada … */ ;
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var templates = databaseService.GetProfileTemplates();

    foreach (var file in files)
    {
        var fileName = fileUtil.GetFileNameAndExtension(file);
        seen.Add(fileName);
        var stamp = StampOf(file);   // FileInfo.LastWriteTimeUtc.Ticks + Length

        if (_entryCache.TryGetValue(fileName, out var cached) && cached.Stamp == stamp)
        {
            entries.Add(cached.Entry);   // QUENTE: zero dry-run
            hits++;
            continue;
        }

        var entry = BuildEntry(file, fileName, templates);   // FRIO: parse + ValidateAndBuild
        _entryCache[fileName] = new CachedEntry(stamp, entry);
        entries.Add(entry);
        misses++;
    }

    // Corner case §50: descartar entradas órfãs (arquivo sumiu do diretório).
    foreach (var stale in _entryCache.Keys.Where(k => !seen.Contains(k)).ToList())
    {
        _entryCache.TryRemove(stale, out _);
    }

    // CR-EP-06: a passada AGREGADA de colisão de nome roda SEMPRE (barata; opera sobre as entries).
    // ATENÇÃO (PA-R1-02): `entry.Diagnostics` é uma List<> MUTÁVEL e `entry with { ... }` faz
    // shallow-copy — o record novo COMPARTILHA a mesma referência de lista. Logo NÃO basta `with`:
    // é preciso copiar a PRÓPRIA LISTA. O cache (gravado em BuildEntry, ANTES desta passada) guarda
    // a entry com a lista LIMPA; ApplyCrossFileCollisions produz entries novas com lista nova só para
    // a lista de retorno. Sem isso a colisão gruda no cache e re-acumula a cada ListClassFiles.
    entries = ApplyCrossFileCollisions(entries);

    logger.Debug($"[CustomClasses] ListClassFiles: {hits} hot / {misses} cold in {sw.ElapsedMilliseconds} ms");
    return entries;
}

/// <summary>
///     CONTRATO 037→030: view leve da cache — as entries já validadas, SEM disparar dry-run.
///     O futuro ListClassSummaries() (item 030) deve projetar a partir DESTA lista (nome, enabled,
///     registered, contagem de Error/Warning), nunca re-rodar ValidateAndBuild. Garante o source of
///     truth único entre a lista (030) e o detalhe/edição.
/// </summary>
public IReadOnlyList<ClassFileEntry> GetCachedEntries() => ListClassFiles();

private void Invalidate(string fileName) => _entryCache.TryRemove(fileName, out _);

// PA-R1-02: a passada agregada NUNCA muta a List<Diagnostic> que o cache segura.
// Para cada entry envolvida em colisão, cria uma entry NOVA com uma LISTA NOVA
// (`[.. entry.Diagnostics, diag]`). Entries sem colisão são retornadas por referência (sem cópia).
private List<ClassFileEntry> ApplyCrossFileCollisions(List<ClassFileEntry> entries)
{
    var collisions = entries
        .Where(e => !string.IsNullOrWhiteSpace(e.Definition?.Name))
        .GroupBy(e => e.Definition!.Name!.Trim(), StringComparer.OrdinalIgnoreCase)
        .Where(g => g.Count() > 1)
        .ToList();
    if (collisions.Count == 0) { return entries; }

    // mapa fileName → entry recriada (com lista nova) para os arquivos colididos
    var rebuilt = new Dictionary<string, ClassFileEntry>(StringComparer.OrdinalIgnoreCase);
    foreach (var group in collisions)
    {
        var involved = string.Join(", ", group.Select(e => e.FileName));
        foreach (var entry in group)
        {
            var diag = new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.DuplicateClassName,
                $"'{entry.FileName}': class name '{group.Key}' is declared by {group.Count()} files ({involved}) — "
                + "at boot only the first (alphabetical) registers; the others are skipped (EditionCollision). "
                + "Rename or delete the duplicates.");
            // LISTA NOVA — o cache continua com a entry original (lista limpa).
            rebuilt[entry.FileName] = entry with { Diagnostics = [.. entry.Diagnostics, diag] };
        }
    }
    return entries.Select(e => rebuilt.TryGetValue(e.FileName, out var r) ? r : e).ToList();
}
```

> **Nota PA-R1-05 (contrato 037→030):** `GetCachedEntries()` evita o **dry-run pesado** (o ganho real), mas ainda faz a varredura de diretório (`fileUtil.GetFiles`) + `StampOf` (um `FileInfo` por arquivo) e a passada CR-EP-06 a cada chamada — barato, **mas não custo-zero**. O item 030 deve chamá-lo **uma vez por navegação/render** (cachear o resultado no componente), nunca por item de lista num loop de render.

> **Nota sobre o contrato 037→030 (CRÍTICO):** o ponto de extensão é `GetCachedEntries()` (retorna `IReadOnlyList<ClassFileEntry>`). `ClassFileEntry` (`ClassEditorService.cs:19-24`) já carrega tudo que um summary precisa: `FileName`, `Enabled`, `Registered`, `Definition?.Name`/`DisplayName`, `Diagnostics`. O item 030 implementa `ListClassSummaries()` como `GetCachedEntries().Select(e => new ClassSummary(...))` — **projeção pura, zero validação**. Se 030 precisar de um campo não presente hoje, ele adiciona ao `ClassFileEntry` (que já é o resultado do dry-run), nunca uma segunda passada. Decisão de não introduzir já o tipo `ClassSummary` no 037: YAGNI — o shape do summary é território do 030; 037 só garante que a cache é **publicamente projetável** sem custo.

Invalidação nos 4 entry points (chamar `Invalidate(fileName)` **após** a escrita bem-sucedida):

```csharp
// Save(...)  (ClassEditorService.cs:213, depois do Audit)        → Invalidate(fileName);
// Delete(...) (ClassEditorService.cs:250, depois do Audit)        → Invalidate(fileName);
// Create/Duplicate chamam Save internamente, então já invalidam o próprio arquivo;
//   o critério de aceite "revalida exatamente 1 entry" é satisfeito porque só a entry
//   tocada sai do cache — as outras 10 permanecem quentes.
```

### (b) Índices lazy no CatalogService

```csharp
// modded/Server/CatalogService.cs
using System.Diagnostics;

/// <summary>Linha pré-computada do índice de busca (DB imutável pós-boot — PA-037-06).</summary>
private sealed record SearchIndexRow(
    string Tpl, string? EnNameLower, string? PtNameLower, string? ShortNameLower,
    string? TemplateNameLower,   // PA-R1-01: template.Name interno É fonte de match no Search atual (:197)
    string EnNameDisplay, string? ShortNameDisplay, string? CategoryId);

// Construído no 1º acesso pós-boot (NUNCA eager no ctor — premissa DB imutável, §7).
private readonly Lazy<List<SearchIndexRow>> _searchIndex;
private readonly Lazy<Dictionary<MongoId, string>> _handbookIndex;   // tpl → categoria
private readonly Lazy<Dictionary<string, HashSet<string>>> _categoryDescendants;  // cat → {cat+filhos}
private readonly Lazy<Dictionary<string, List<CatalogClothing>>> _clothingBySide; // "Usec"/"Bear"

public CatalogService(DatabaseService databaseService, ItemHelper itemHelper, LocaleService localeService)
{
    // … atribuições existentes …
    _handbookIndex = new(BuildHandbookIndex);
    _searchIndex = new(BuildSearchIndex);
    _clothingBySide = new(BuildClothingBySide);
    _categoryDescendants = new(() => new(StringComparer.Ordinal));   // preenchido sob demanda por root
}

private List<SearchIndexRow> BuildSearchIndex()
{
    var sw = Stopwatch.StartNew();
    var en = GetLocale("en");
    var pt = GetLocale("pt");
    var handbook = _handbookIndex.Value;
    var rows = new List<SearchIndexRow>();
    foreach (var (tpl, template) in databaseService.GetItems())   // ref: DatabaseService.cs:129
    {
        if (!string.Equals(template.Type, "Item", StringComparison.Ordinal)) { continue; }
        var key = tpl.ToString();
        en.TryGetValue($"{key} Name", out var enName);
        en.TryGetValue($"{key} ShortName", out var shortName);
        pt.TryGetValue($"{key} Name", out var ptName);
        // PA-037-07 (locale fallback, espelha Search atual :212): nenhum item some por falta de locale —
        // EnNameDisplay cai em template.Name/tpl.
        handbook.TryGetValue(tpl, out var categoryId);
        rows.Add(new SearchIndexRow(
            key,
            enName?.ToLowerInvariant(), ptName?.ToLowerInvariant(), shortName?.ToLowerInvariant(),
            template.Name?.ToLowerInvariant(),   // PA-R1-01: preserva o match por template.Name interno
            !string.IsNullOrWhiteSpace(enName) ? enName! : template.Name ?? key,
            shortName, categoryId));
    }
    logger /* via ISptLogger se injetado; senão sem log */ ;
    return rows;
}

public List<CatalogItem> Search(string query, string? parentCategoryId = null, int limit = 50, Func<string, bool>? filter = null)
{
    var sw = Stopwatch.StartNew();
    var results = new List<CatalogItem>();
    if (string.IsNullOrWhiteSpace(query) || limit <= 0) { return results; }
    var q = query.Trim();
    var qLower = q.ToLowerInvariant();
    var scope = parentCategoryId is null ? null : CollectCategoryWithDescendants(parentCategoryId);

    foreach (var row in _searchIndex.Value)   // QUENTE: scan de lista compacta em memória
    {
        if (scope is not null && (row.CategoryId is null || !scope.Contains(row.CategoryId))) { continue; }
        // PA-R1-01/PA-R1-06: mesmas 5 fontes de match do Search atual (:193-197), mesma ordem
        // categoria → match → filter → GetPrice → add → cap@limit.
        var match = string.Equals(row.Tpl, q, StringComparison.OrdinalIgnoreCase)
            || (row.EnNameLower?.Contains(qLower) ?? false)
            || (row.PtNameLower?.Contains(qLower) ?? false)
            || (row.ShortNameLower?.Contains(qLower) ?? false)
            || (row.TemplateNameLower?.Contains(qLower) ?? false);
        if (!match) { continue; }
        if (filter is not null && !filter(row.Tpl)) { continue; }   // CR-EP-07 mantido
        var (price, source) = GetPrice(new MongoId(row.Tpl));
        results.Add(new CatalogItem { Tpl = row.Tpl, Name = row.EnNameDisplay, ShortName = row.ShortNameDisplay,
            Price = price, PriceSource = source, CategoryId = row.CategoryId });
        if (results.Count >= limit) { break; }
    }
    // log debug elapsed (frio/quente diferenciado pelo _searchIndex.IsValueCreated antes do .Value)
    return results;
}
```

> **Preço no índice:** o kickoff lista "preço" entre os campos pré-computados. Decisão (PA-037-08): **não** congelar preço no índice — `GetPrice` lê a tabela de flea, que este repo sobrescreve em runtime via viewer/itemdb (memory `project_flea_price_formula`); cachear preço arriscaria servir valor obsoleto após um override. `GetPrice` por hit já é barato (lookup de dict) e só roda nos ≤ `limit` matches, não no catálogo inteiro. A pré-computação cara que o índice elimina é a **resolução de locale por item** no scan completo. Registrado como divergência consciente do kickoff.

### (c) Recompute do ClassEdit (debounce + 1 chamada + capacidade lazy)

```csharp
// modded/Server/Web/Pages/ClassEdit.razor (@code)
private System.Threading.CancellationTokenSource? _recomputeCts;
private bool _stashTabVisible;   // setado pelo OnPreviewInteraction/ActivePanelIndex das MudTabs

/// <summary>Debounce ~250 ms — agrupa keystrokes de count/level (corner case "campo trava").</summary>
private void ScheduleRecompute()
{
    _recomputeCts?.Cancel();
    var cts = _recomputeCts = new();
    _ = InvokeAsync(async () =>
    {
        try { await Task.Delay(250, cts.Token); } catch (TaskCanceledException) { return; }
        RecomputeLoadoutCost();
        StateHasChanged();
    });
}

private void RecomputeLoadoutCost()
{
    if (_model is null) { return; }
    var def = _model.ToDefinition();
    _loadoutCost = CostService.ComputeLoadoutCost(def);   // 1 chamada (custo total)

    // (c) stash subtotal + capacidade só quando a aba Stash está visível (PA-037-04).
    if (_stashTabVisible)
    {
        _stashCost = CostService.ComputeLoadoutCost(new ClassDefinition { Loadout = new Loadout { Stash = def.Loadout?.Stash } });
        _stashCapacity = CostService.CheckStashCapacity(def);
    }
}

private async Task SaveAsync()
{
    FlushPendingRecompute();   // corner case §49/§53: força o pendente + capacidade antes de persistir
    // … resto inalterado (validate form → ToDefinition → Task.Run(Save)) …
}

private void FlushPendingRecompute()
{
    _recomputeCts?.Cancel();
    var def = _model!.ToDefinition();
    _stashCapacity = CostService.CheckStashCapacity(def);   // §53: capacidade nunca pode ficar "nunca computada"
    _loadoutCost = CostService.ComputeLoadoutCost(def);
}
```

> Handlers de count/level (ex. `ItemSpecEditor` binds que hoje chamam `RecomputeLoadoutCost` direto, p.ex. `AddStashLine`/`DuplicateStashLine` `:837-855`) passam a chamar `ScheduleRecompute()`. Add/remove de **linha** (evento discreto, não digitação) pode recomputar imediato; só os campos de **texto numérico** (count, level) precisam de debounce — manter a UI responsiva sem subverter o "≤1 recompute por pausa".

## 6. Fluxo de dados

```
Navegação (lista/detalhe/edição)
  → ClassEdit.LoadFromDisk (:580) / ClassDetail / Classes
    → ClassEditorService.ListClassFiles  ── QUENTE ──> _entryCache[file] (stamp ok) → 0 dry-run
                                          ── FRIO  ──> ClassRegistrar.ValidateAndBuild → grava no cache
    → (sempre) ApplyCrossFileCollisions (CR-EP-06, barato)

Busca de item (ItemPicker, por tecla)
  → CatalogService.Search → _searchIndex.Value (Lazy; 1ª vez constrói, depois scan em memória)

Edição de count/level (digitação)
  → ScheduleRecompute (debounce 250ms) → RecomputeLoadoutCost (1× ComputeLoadoutCost;
       stash subtotal/capacidade só se aba Stash visível)
  → Save → FlushPendingRecompute (garante capacidade computada) → EditorService.Save → Invalidate(file)
```

## 7. Riscos e dependências

- **Aliasing (CONFIRMADO seguro):** `ClassEditModel.FromDefinition` (`ClassEditModel.cs:176`) faz **cópia profunda** — `ItemSpecModel.FromSpec` recursa criando objetos novos (`:57-81`), `ToDict`/`BuildLoadout` reconstroem dicionários/listas. O form nunca segura referência à `Definition` da entry cacheada. **Único cuidado novo:** a `ClassFileEntry` cacheada e seu `Diagnostics` não podem ser mutados após cacheados — por isso a passada CR-EP-06 (que faz `entry.Diagnostics.Add`) opera sobre **cópias** das entries afetadas, deixando o cache limpo (ver stub `ApplyCrossFileCollisions`). Sem essa cópia, uma colisão resolvida "grudaria" no cache.
- **DB imutável pós-boot (premissa PA-037-06):** os índices `Lazy<T>` assumem que `GetItems`/`GetHandbook`/`GetCustomization`/locales não mudam após o boot. O comentário obsoleto em `CatalogService.cs:231` ("Built per call — DB is live (mods)") deve ser corrigido. Risco aceito: um mod que mute a DB em runtime (caso não suportado) veria índice obsoleto. Mitigação de ordem: índices nunca são `.Value` no construtor — só no 1º acesso pós-boot, depois que todos os mods terminaram o `PostDBModLoader`.
- **Concorrência (PA-037-03):** `ConcurrentDictionary` para entries; `Lazy<T>` `ExecutionAndPublication` para índices. Entries e índices são read-only após criação. A premissa single-user do item 021 cobre escrita concorrente de arquivo; a leitura concorrente do cache fica segura por construção.
- **Prerender (d) — investigação:** `prerender:false` por página depende do render mode do host Blazor (`@rendermode`/`App.razor`/`_Host`). Como a spec funcional decidiu manter o prerender e medir, a investigação só ocorre se a medição pós-cache (e) mostrar > ~50 ms de prerender na 2ª navegação. Se inviável (host não permite override por página), registrar a decisão no as-built; o cache (a) já barateia o 2º `OnInitialized` (leitura de dicionário).
- **Granularidade de mtime:** ~1–2 s no FS. Coberto pela invalidação interna nos 4 entry points (corner case §45). Edição externa (`/sync-classes`) reflete na próxima leitura após o mtime divergir.
- **`ProfilesUsingEdition` (`:412`) fora de escopo** — não cachear (só roda no dialog de delete, em background).

## 8. Checklist de implementação

- [ ] (a) `FileStamp`/`CachedEntry` + `ConcurrentDictionary` em `ClassEditorService`; refatorar `ListClassFiles` para hot/cold + descarte de órfãos.
- [ ] (a) Extrair `BuildEntry` (parse + dry-run de UM arquivo) e `ApplyCrossFileCollisions` (passada agregada sobre cópias) de `ListClassFiles`.
- [ ] (a) `Invalidate(fileName)` em `Save` (`:213`) e `Delete` (`:250`); confirmar que `Create`/`Duplicate` herdam via `Save`.
- [ ] (a) **Contrato 037→030:** `public IReadOnlyList<ClassFileEntry> GetCachedEntries()` com XML-doc explicando que 030 projeta daqui (sem re-validar).
- [ ] (b) `SearchIndexRow` + `Lazy<>` de busca, `_handbookIndex`, `_categoryDescendants`, `_clothingBySide`; `BuildSearchIndex`/`BuildClothingBySide` no 1º acesso (não eager).
- [ ] (b) `Search` e `GetClothing` consomem os `Lazy`; corrigir comentário obsoleto `CatalogService.cs:231`.
- [ ] (c) `ScheduleRecompute` (debounce 250 ms via `CancellationTokenSource` + `Task.Delay` + `InvokeAsync`); ligar nos binds de count/level.
- [ ] (c) `RecomputeLoadoutCost`: 1 `ComputeLoadoutCost`; `_stashCost`/`CheckStashCapacity` só com `_stashTabVisible`; `_stashTabVisible` setado pela navegação de abas.
- [ ] (c) `FlushPendingRecompute` no início de `SaveAsync` (garante capacidade + custo finais — corner cases §49/§53).
- [ ] (e) `Stopwatch` + `logger.Debug` em `ListClassFiles` (hot/cold), `Search` (frio/quente), `LoadFromDisk` (navegação). Injetar `ISptLogger<CatalogService>` se ainda não houver.
- [ ] (d) Após medição, decidir prerender; registrar no as-built (números antes/depois obrigatórios — DoD).
- [ ] Verificação funcional in-game (memory `feedback_spt_validation`): mesmos diagnósticos/custos/capacidade que antes; 0 dry-run na navegação quente; 1 dry-run após save.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec técnica criada via `/create-technical-spec` (autônoma — usuário ausente). Premissas PA-037-01..08 registradas no doc. |
| 2026-06-12 | Review técnica 01 (autônoma): 2 🔴 resolvidos in-place — PA-R1-01 (match por `template.Name` preservado no índice) e PA-R1-02 (`ApplyCrossFileCollisions` copia a List, não só o record). Refinamentos 🟡/🟢 (PA-R1-03..08) anotados, ver review-01. |
