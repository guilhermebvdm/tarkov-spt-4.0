# 037 — Performance: cache de validação + índices do catálogo · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [037-performance-cache-02-spec-tech.md](037-performance-cache-02-spec-tech.md)
**Data:** 2026-06-12

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-R1-MM`. Execução autônoma (usuário ausente, não aprovável): cada 🔴 bloqueador foi **resolvido in-place na spec técnica** e marcado como resolvido aqui. Apenas decisões de design genuinamente ambíguas ficariam abertas.

## Resumo

> 🔴 Bloqueadores: 0 (2 encontrados, 2 resolvidos) · 🟡 Importantes: 3 · 🟢 Menores: 3 · ✅ Resolvidos: 2 · Total: 8

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-R1-01 | C — Erro de Lógica | 🔴→✅ | Índice de busca perde o match por `template.Name` (regressão funcional) | ✅ Resolvido |
| PA-R1-02 | C — Erro de Lógica | 🔴→✅ | CR-EP-06 muta a `List<Diagnostic>` da entry cacheada (`with` copia a referência) | ✅ Resolvido |
| PA-R1-03 | B — Edge Case | 🟡 | `_loadDiagnostics = entry.Diagnostics` aliasa a lista cacheada de longa vida | 🟡 Aberto (mitigado por PA-R1-02) |
| PA-R1-04 | A — Gap | 🟡 | `_categoryDescendants` Lazy declarado mas nunca populado / não usado | 🟡 Aberto |
| PA-R1-05 | A — Gap | 🟡 | Contrato 037→030: `GetCachedEntries()` re-roda a varredura+CR-EP-06 a cada chamada | 🟡 Aberto |
| PA-R1-06 | C — Erro de Lógica | 🟢 | `BuildSearchIndex` ignora preço mas `Tpl` exato deveria casar mesmo fora do escopo | 🟢 Aberto |
| PA-R1-07 | B — Edge Case | 🟢 | Debounce: circuito desconectado durante `Task.Delay` → `InvokeAsync` lança | 🟢 Aberto |
| PA-R1-08 | A — Gap | 🟢 | `ISptLogger<CatalogService>` precisa entrar no ctor — quebra a assinatura DI | 🟢 Aberto |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-R1-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-06-12

**Índice de busca perde o match por `template.Name` (regressão funcional)**

**Problema:** o `Search` atual (`CatalogService.cs:193-197`) casa contra **cinco** fontes: `tpl` exato, `enName`, `ptName`, `shortName` **e `template.Name`** (`Contains(template.Name, q)`). O `SearchIndexRow` proposto na spec (§5b) só carrega `EnNameLower/PtNameLower/ShortNameLower/Tpl` para o **match**; `EnNameDisplay` cai em `template.Name` apenas como rótulo de exibição, mas não entra no predicado `match`. Itens cujo único texto pesquisável é o `template.Name` interno (sem locale en/pt/short) **deixam de aparecer** na busca quente.

**Por que importa:** viola o critério de aceite "nenhum resultado funcional muda" (spec funcional §40) e a própria premissa PA-037-07 ("nenhum item indexável pode sumir da busca por falta de locale"). É uma regressão silenciosa de cobertura de busca.

**Sugestão:** adicionar `TemplateNameLower` ao `SearchIndexRow` e incluí-lo no predicado `match`, espelhando exatamente as cinco condições de `Search:193-197`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (resolvido autonomamente na spec técnica)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5(b) da spec técnica atualizada — `SearchIndexRow` ganhou `TemplateNameLower` e o predicado `match` em `Search` agora inclui `row.TemplateNameLower?.Contains(qLower)`, replicando as 5 fontes de match originais. PA-037-07 reforçada para cobrir o `template.Name` como fonte de match (não só de display).

### PA-R1-02 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-06-12

**CR-EP-06 muta a `List<Diagnostic>` da entry cacheada — `with` copia a referência, não a lista**

**Problema:** `ClassFileEntry` (`ClassEditorService.cs:19-24`) é um record cujo campo `Diagnostics` é um `List<ClassDiagnostic>` **mutável**. A passada CR-EP-06 atual faz `entry.Diagnostics.Add(...)` (`:133`). A spec (§5a stub + §7) diz "recriar entries afetadas (com Diagnostics copiados) só para a lista de retorno; o cache guarda a entry SEM colisão" — mas o stub é um comentário `/* … */` sem o mecanismo. Um `entry with { ... }` **não** resolve: `with` faz shallow-copy e o novo record compartilha a **mesma referência de lista** `Diagnostics`. Logo `novaEntry.Diagnostics.Add(colisão)` continua mutando a lista que o cache segura → a colisão "gruda" e é re-adicionada a cada `ListClassFiles` (acumulação) e reaparece após o usuário resolver o duplicado.

**Por que importa:** quebra o corner case explícito da spec funcional §46/§52 (colisão não pode grudar no cache) e causa **acumulação de diagnósticos duplicados** a cada navegação (a lista cacheada cresce sem limite). Bug garantido assim que existem 2 arquivos com mesmo `name`.

**Sugestão:** `ApplyCrossFileCollisions` deve, para cada entry envolvida em colisão, produzir uma entry **nova com uma `List` nova**: `entry with { Diagnostics = [.. entry.Diagnostics, collisionDiag] }`. As entries não-colididas são retornadas por referência (sem cópia). O cache nunca recebe a entry com colisão (continua guardando a original limpa). Documentar que `BuildEntry` grava no cache **antes** de `ApplyCrossFileCollisions` rodar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (resolvido autonomamente na spec técnica)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5(a) da spec técnica: `ApplyCrossFileCollisions` reescrito com cópia **da lista** (`entry with { Diagnostics = [.. entry.Diagnostics, diag] }`), não apenas do record. Adicionada nota de ordem: o cache é populado em `BuildEntry` (lista limpa) **antes** da passada de colisão; a passada opera sobre uma lista de retorno separada. §7 (Aliasing) reforçada para deixar explícito que `with` sozinho compartilha a `List`.

### PA-R1-03 · B — Edge Case · 🟡 Importante

**`_loadDiagnostics = entry.Diagnostics` aliasa a lista cacheada de longa vida**

**Problema:** `ClassEdit.LoadFromDisk` (`:590`) faz `_loadDiagnostics = entry.Diagnostics` — pega a referência direta da lista da entry. Hoje isso é inofensivo porque a entry é descartada após o render. Com o cache (a), essa lista passa a ser **a mesma instância que vive no `_entryCache`**. Se qualquer caminho de UI mutar `_loadDiagnostics` (hoje nenhum o faz, mas é uma armadilha futura), corromperia a entry cacheada compartilhada por todos os circuitos. O mesmo vale para `ClassDetail`/`Classes` que consomem `entry.Diagnostics`.

**Por que importa:** não causa bug hoje, mas é exatamente o tipo de aliasing que a spec funcional §46 pede para evitar. Resolvido o PA-R1-02 (cache guarda lista limpa), o risco residual é o consumidor mutar a lista — não há mutação hoje, então não bloqueia.

**Sugestão:** registrar na spec técnica (§7) o invariante "entries cacheadas e suas `Diagnostics` são tratadas como imutáveis pelos consumidores (ClassEdit/ClassDetail/Classes só leem)". Opcional defensivo: `LoadFromDisk` copiar (`[.. entry.Diagnostics]`) ou a entry expor `IReadOnlyList`. Decisão de baixo custo, não obrigatória para o caminho atual.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: não é bloqueador (nenhum consumidor muta hoje). Anotado na spec técnica §7 como invariante a respeitar no código. Decisão de copiar-defensivo fica para o `/code-mod`.

### PA-R1-04 · A — Gap · 🟡 Importante

**`_categoryDescendants` Lazy declarado mas nunca populado / inconsistente com `CollectCategoryWithDescendants`**

**Problema:** §5(b) declara `Lazy<Dictionary<string, HashSet<string>>> _categoryDescendants` e inicializa com `new(StringComparer.Ordinal)` vazio ("preenchido sob demanda por root"). Mas `CollectCategoryWithDescendants` (`CatalogService.cs:244`) recalcula o mapa pai→filhos **inteiro** a cada chamada e faz BFS por root. A spec não diz como o `Lazy` é populado nem quem grava nele — `Lazy<T>` constrói o **valor** uma vez, não memoiza por-root sob demanda. Do jeito escrito, o dicionário fica permanentemente vazio. A árvore pai→filhos (cara de reconstruir) é que deveria ser cacheada, não o resultado por-root.

**Por que importa:** o item promete "árvore de categorias pré-computada" (spec funcional §21). Como está, ou o cache não funciona (dict vazio) ou precisa de lógica de escrita concorrente não especificada (race no `Lazy` de dict mutável).

**Sugestão:** trocar por **um** `Lazy<Dictionary<string, List<string>>> _childrenByParent` (a árvore pai→filhos, construída 1×), e `CollectCategoryWithDescendants` faz só o BFS sobre esse mapa imutável a cada chamada (BFS é barato; o caro era montar o mapa). Remover o `_categoryDescendants` por-root (memoização desnecessária e com semântica de escrita ambígua).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: não bloqueia o caminho de busca (escopo por categoria é opcional e secundário); é uma correção de design do índice de categoria. Anotado na spec técnica §5(b) como ajuste recomendado.

### PA-R1-05 · A — Gap · 🟡 Importante

**Contrato 037→030: `GetCachedEntries()` re-executa varredura de diretório + CR-EP-06 a cada chamada**

**Problema:** `GetCachedEntries() => ListClassFiles()` (§5a) não é "view leve da cache": cada chamada faz `fileUtil.GetFiles` (I/O de diretório) + `StampOf` por arquivo (`FileInfo` por arquivo) + a passada CR-EP-06. O ganho do cache é pular o **dry-run pesado**, o que é mantido; mas a XML-doc vende "sem custo / view leve", o que é impreciso. Se o item 030 chamar `GetCachedEntries()` em loop de render (ex.: sidebar reativa), paga varredura de FS repetida.

**Por que importa:** o contrato 037→030 é destacado como CRÍTICO na spec. Vender custo-zero quando há I/O de diretório por chamada pode levar o 030 a chamá-lo em hot path de render, reintroduzindo lentidão. O foco do prompt é "contrato 037→030 bem definido".

**Sugestão:** corrigir a XML-doc/§5a: `GetCachedEntries()` evita o **dry-run** (caro), mas ainda faz varredura de diretório + stat por arquivo (barato, mas não zero). Orientar o 030 a chamar **uma vez por navegação/render** (cachear o resultado no componente), não por item de lista. Alternativamente, o 030 pode receber a `List<ClassFileEntry>` já materializada de quem navegou. Não introduzir `ClassSummary` agora (YAGNI mantido).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: refinamento do contrato (texto/doc), não bug. Anotado na spec técnica §5(a) — descrição de `GetCachedEntries` ajustada para não prometer custo-zero.

### PA-R1-06 · C — Erro de Lógica · 🟢 Menor

**`Tpl` exato deveria casar mesmo fora do `parentCategoryId` (paridade com o atual)**

**Problema:** no `Search` atual o filtro de categoria (`:183`) roda **antes** do match, então um `tpl` exato fora do escopo de categoria também é filtrado — comportamento idêntico no stub novo (`scope` checado antes do match). OK, é paridade. Mas vale confirmar explicitamente que a ordem categoria→match→filter→price é **idêntica** à atual (categoria primeiro, depois match, depois `filter` predicate, depois cap), porque o stub reordena levemente (price computado só após match+filter — correto, mas não documentado como equivalência).

**Por que importa:** clareza; o critério "resultados idênticos" depende da ordem de filtragem e do ponto de aplicação do `limit`.

**Sugestão:** adicionar uma linha em §5(b) afirmando a equivalência de ordem: `categoria → match(5 fontes) → filter(tpl) → GetPrice → add → cap@limit`, igual a `Search:183-222`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: documental. Anotado na spec técnica §5(b).

### PA-R1-07 · B — Edge Case · 🟢 Menor

**Debounce: circuito desconectado durante `Task.Delay` → `InvokeAsync`/`StateHasChanged` lança**

**Problema:** `ScheduleRecompute` (§5c) agenda `InvokeAsync(async () => { await Task.Delay(250); RecomputeLoadoutCost(); StateHasChanged(); })`. Se o usuário navegar para fora (circuito disposto) durante os 250 ms, `StateHasChanged()` num componente disposto lança `ObjectDisposedException`/`InvalidOperationException` no thread do timer.

**Por que importa:** exceção não-observada em background pode derrubar o circuito ou poluir o log. Cenário real (digita e clica em "voltar" rápido).

**Sugestão:** guard com try/catch em torno do `StateHasChanged()` (swallow se disposto), ou checar um flag `_disposed` setado em `Dispose()`. A `TaskCanceledException` do `Delay` já é tratada; falta a do dispose. Padrão barato.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: hardening. Anotado na spec técnica §5(c).

### PA-R1-08 · A — Gap · 🟢 Menor

**Instrumentação exige `ISptLogger<CatalogService>` no ctor — assinatura DI muda**

**Problema:** `CatalogService` hoje **não** injeta logger (ctor `:85-88` = `DatabaseService, ItemHelper, LocaleService`). §5(b) usa `logger.Debug(...)` em `BuildSearchIndex`/`Search` e o checklist §8(e) já prevê "injetar `ISptLogger<CatalogService>` se ainda não houver". O stub `BuildSearchIndex` tem o placeholder `logger /* via ISptLogger se injetado; senão sem log */ ;` que não compila.

**Por que importa:** o critério de aceite exige logs de tempo de busca (frio/quente) — sem logger não há instrumentação (e), que é DoD. O stub atual é não-compilável (placeholder).

**Sugestão:** adicionar `ISptLogger<CatalogService> logger` ao ctor primário do `CatalogService` e usar nos dois pontos. É injeção padrão SPT (o `ClassEditorService` já o faz). Substituir o placeholder por chamada real.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

> Aberto: trivial (injeção padrão), mas registrado porque o stub atual não compila. Anotado na spec técnica §5(b)/§8(e).

## Avaliação dos eixos pedidos

- **Invalidação (mtime + save):** correta. `(LastWriteTimeUtc.Ticks, Length)` + invalidação interna nos 4 entry points cobre a granularidade de FS (corner case §45). `Create`/`Duplicate` invalidam um **nome novo** (no-op no cache) e o arquivo aparece via varredura — correto, sem furo. Órfãos descartados na varredura — correto.
- **Aliasing/mutação da entry cacheada:** o risco **real** é a mutação da `List Diagnostics` pela passada CR-EP-06 (PA-R1-02, resolvido) — `with` sozinho não protege. A `Definition` está segura (deep copy em `FromDefinition`, confirmado em `ClassEditModel.cs:176-221`). Aliasing de `_loadDiagnostics` é residual e inofensivo hoje (PA-R1-03).
- **Thread-safety dos índices Lazy:** `Lazy<T>` em `ExecutionAndPublication` (default) é correto para índices read-only construídos 1×. **Exceção:** o `_categoryDescendants` mutável-sob-demanda (PA-R1-04) viola essa premissa (escrita concorrente num dict não-thread-safe) — por isso a sugestão de trocá-lo por um `Lazy` imutável + BFS local. `ConcurrentDictionary` para o cache de entries é adequado; a passada CR-EP-06 concorrente fica segura **se** PA-R1-02 for implementado com lista nova (sem mutar a compartilhada).
- **Contrato 037→030:** o ponto de extensão (`GetCachedEntries() → IReadOnlyList<ClassFileEntry>`, 030 projeta sem revalidar) é sólido em intenção. Único ajuste: não vender custo-zero — há varredura de FS por chamada (PA-R1-05).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Review 01 criada via `/review-technical-spec` (autônoma) — 2 🔴 (resolvidos in-place) · 3 🟡 · 3 🟢 |
