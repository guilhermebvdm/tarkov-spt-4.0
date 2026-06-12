# 037 — Performance: cache de validação + índices do catálogo · Code Review 01

**Mod:** CustomClasses
**As-built revisado:** [037-performance-cache-05-asbuild.md](./037-performance-cache-05-asbuild.md)
**Spec técnica:** [037-performance-cache-02-spec-tech.md](./037-performance-cache-02-spec-tech.md)
**Review técnica:** [037-performance-cache-03-spec-tech-review-01.md](./037-performance-cache-03-spec-tech-review-01.md)
**Data:** 2026-06-12

> Revisão do código **realmente entregue** no commit `d180195` (3 arquivos editados: `ClassEditorService.cs`, `CatalogService.cs`, `Web/Pages/ClassEdit.razor`). O code-review foi pulado na entrega por limite de sessão (status 🟡). Execução **autônoma** (usuário ausente, não aprovável): aplicam-se apenas achados **SEGUROS** (null/crash/exceção, build-breaker, fuga de spec com fix local inequívoco, leak/dispose, thread-safety claramente quebrada, fio solto óbvio). Achados de design/arquitetura, fix ambíguo ou otimização marginal ficam **ADIADOS** (não tocam código). Build de referência: `dotnet build -c Release --no-incremental` → **0 erros / 0 avisos** (net9.0, MudBlazor 8.13.0).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 SEGUROS · 🟢 Menores: 0 SEGUROS · Adiados: 5 · Aplicados: 0

**Veredito:** o código está correto, compila limpo (0/0) e implementa fielmente a spec técnica 02 + os refinamentos da review técnica 01 (PA-R1-01..08) e do as-built (PA-AB-01..04). Não há achado na classe SEGURA — nenhum null/crash, build-breaker, fuga de spec, leak/dispose ou thread-safety quebrada. Os 5 pontos abaixo são trade-offs de design/hardening **já reconhecidos** pela spec/review/as-built (ou otimizações marginais) e ficam ADIADOS; tocá-los reabriria decisão de design sem aprovação. A pendência real do item é **quantitativa** (medição before/after + verificação in-game), não de código — registrada no as-built §40/§54.

## Categorias

- **A — Gap** · **B — Edge Case** · **C — Erro de Lógica** · **D — Design/Arquitetura**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

## O que foi verificado e está correto

- **Esquema de invalidação (CR scope: correção).** `StampOf` = `(FileInfo.LastWriteTimeUtc.Ticks, Length)` — invalida em mudança de mtime OU tamanho (cobre granularidade de FS parcialmente; o tamanho complementa). `Invalidate(fileName)` é chamado **após** a escrita bem-sucedida em `Save` (`:333`, após `WriteFile`+hot-apply) e `Delete` (`:371`, após `DeleteFile`); `Create`/`Duplicate` invalidam via `Save` interno. O critério "revalida exatamente 1 entry" é honrado: só a entry tocada sai do `ConcurrentDictionary`. Órfãos (arquivo sumiu) são descartados na varredura (`:154-158`). Arquivo novo por fora aparece via `GetFiles` (cache é por-entry, não da lista). **Correto.**
- **Thread-safety (CR scope: thread-safety).** `_entryCache` é `ConcurrentDictionary` (`StringComparer.OrdinalIgnoreCase`); os 4 índices do catálogo são `Lazy<T>` em modo default `ExecutionAndPublication` (thread-safe para valor construído 1× e só lido). Entries são imutáveis após cacheadas (a passada CR-EP-06 nunca muta a entry do cache — ver abaixo). Dois circuitos Blazor concorrentes que COLD-buildam a mesma entry fazem last-write-wins de entries equivalentes (inócuo); a orphan-cleanup materializa as chaves (`.ToList()`) antes do `TryRemove` (seguro em `ConcurrentDictionary`). **Correto.**
- **Aliasing da entry cacheada (CR scope: corrupção de estado compartilhado).** O risco-chave da spec (§46): a passada CR-EP-06 (`ApplyCrossFileCollisions`) cria **lista nova** (`entry with { Diagnostics = [.. entry.Diagnostics, diag] }`) só para a lista de retorno — o cache (gravado em `BuildEntry`, ANTES da passada) fica com a lista LIMPA. Confirma que a colisão não "gruda" nem re-acumula a cada `ListClassFiles` (PA-R1-02 implementado corretamente). `ClassEditModel.FromDefinition` faz cópia profunda (não revalidei o arquivo — confirmado pela review técnica §7 e mantido NÃO MODIFICADO) → o form não aliasa a `Definition` cacheada. **Correto.**
- **Debounce + flush no save (CR scope: correção/dado obsoleto).** `ScheduleRecompute` (debounce 250 ms via `CancellationTokenSource` + `Task.Delay` + `InvokeAsync`) ligado nos `ItemSpecEditor OnChanged` (Equipped `:412` + Stash `:487`); add/remove de linha recomputa imediato (evento discreto). `SaveAsync` chama `FlushPendingRecompute()` no início (`:762`): cancela o pendente e recomputa custo total + subtotal stash + capacidade **incondicionalmente** (capacidade nunca fica "nunca computada" — corner cases §49/§53). **Correto.**
- **1 chamada no caminho quente + capacidade lazy (CR scope: fuga de spec).** `RecomputeLoadoutCost` (`:885`): **1** `ComputeLoadoutCost` (custo total); `_stashCost`/`CheckStashCapacity` só com `_stashTabVisible` (aba index 6). Entrar na aba Stash (setter `ActivePanelIndex` `:557`) dispara 1 recompute para popular subtotal/capacidade antes do 1º display (PA-AB-01). **Correto.**
- **Dispose / leak (CR scope: leak/dispose).** `@implements IDisposable`; `Dispose` seta `_disposed`, cancela e **dispõe** o `_recomputeCts` final, desregistra o Ctrl+S e dispõe o `_dotNetRef`. O `StateHasChanged` pós-delay é guardado por `if (!_disposed)` + `try/catch (ObjectDisposedException)` (PA-R1-07 implementado). **Correto** (com a ressalva marginal CR-01-02 abaixo).
- **Paridade funcional do Search (CR scope: fuga de spec).** As **5 fontes de match** preservadas (tpl exato, en, pt, short, `template.Name` interno — PA-R1-01), mesma ordem categoria→match→filter→GetPrice→add→cap@limit (PA-R1-06), `filter` antes do cap (CR-EP-07). Preço NÃO congelado no índice (`GetPrice` por-hit, PA-037-08). **Correto.**
- **Índice de categoria (CR scope: dict vazio / PA-R1-04).** O `_categoryDescendants` ambíguo da spec foi substituído por `_childrenByParent` (`Lazy<Dictionary<string, List<string>>>`, construído 1×); `CollectCategoryWithDescendants` faz só BFS sobre o mapa imutável cacheado. **Correto** — o gap da review técnica foi sanado na implementação.
- **Comentário obsoleto `CatalogService.cs:231`.** A spec mandava corrigir o comentário "Built per call — DB is live (mods)". `BuildHandbookIndex` (`:313-323`) não contém mais esse comentário; o cabeçalho dos índices (`:99-104`) documenta a premissa de DB imutável pós-boot. **Resolvido.**

## Achados ADIADOS (não tocam código)

### CR-01-01 · 🟢 D — `_loadDiagnostics = entry.Diagnostics` aliasa a lista cacheada de longa vida — ADIADO

**Arquivo:** `ClassEdit.razor` (`LoadFromDisk` `:690`).

`_loadDiagnostics = entry.Diagnostics` pega a referência direta da lista que agora vive no `_entryCache` (compartilhada por todos os circuitos). **Por que ADIAR (não é SEGURO):** é exatamente o residual já analisado em PA-R1-03 e aceito em PA-AB-03 como invariante — **nenhum** caminho de UI muta `_loadDiagnostics` (só `foreach` de leitura na render; reset via `= []` em `LoadFromDisk`/`Discard`, que reatribui, não muta). A passada CR-EP-06 já não toca a lista cacheada (PA-R1-02). A cópia defensiva (`[.. entry.Diagnostics]`) é hardening de baixo custo, mas mexer aqui é decisão de design (introduzir cópia onde a spec decidiu tratar como invariante), não correção de bug. O mesmo vale para `ClassDetail`/`Classes`, que também só leem `entry.Diagnostics`.

### CR-01-02 · 🟢 B — `_recomputeCts` antigo não é disposto entre keystrokes (CTS leak marginal) — ADIADO

**Arquivo:** `ClassEdit.razor` (`ScheduleRecompute` `:913-939`).

Cada `ScheduleRecompute` cria um novo `CancellationTokenSource` e cancela o anterior (`_recomputeCts?.Cancel()`), mas **não** dispõe o antigo — só o último é disposto em `Dispose()`. Um CTS cancelado retém um `WaitHandle` (alocado preguiçosamente) até o GC. **Por que ADIAR (otimização marginal):** o `WaitHandle` só é materializado se `.Token.WaitHandle` for acessado, o que `Task.Delay(ct)` faz internamente — então há um leak real, porém minúsculo (um handle por keystroke até o GC, num fluxo single-user). Dispor o antigo no `Cancel()` introduziria uma race com o `Task.Delay` que ainda observa o token (poderia lançar `ObjectDisposedException` no `await Task.Delay`). O fix correto (dispor dentro do próprio continuation, após o `catch`) é defensável mas não-trivial e não é um leak/dispose **claro** — é trade-off de timing. Fica para decisão de design.

### CR-01-03 · 🟢 A — `GetCachedEntries()` re-roda varredura de FS + CR-EP-06 a cada chamada (contrato 037→030) — ADIADO

**Arquivo:** `ClassEditorService.cs` (`GetCachedEntries` `:182`).

`GetCachedEntries() => ListClassFiles()` evita o dry-run pesado (o ganho real), mas ainda faz `GetFiles` + um `FileInfo` por arquivo + a passada CR-EP-06 a cada chamada — não é custo-zero. **Por que ADIAR (refinamento de doc, não bug):** a XML-doc (`:174-180`) **já** documenta isso explicitamente (PA-R1-05) e orienta o 030 a chamar uma vez por navegação. Não há consumidor do método hoje (é gancho para o 030); nenhum hot-path o invoca em loop. Não há código a corrigir — só a disciplina futura do 030, já registrada no contrato.

### CR-01-04 · 🟢 B — `StashTabIndex = 6` hard-coded acopla à ordem dos painéis — ADIADO

**Arquivo:** `ClassEdit.razor` (`StashTabIndex` `:549`).

A aba Stash é identificada por índice literal (6); reordenar os `MudTabPanel` quebra o gating de capacidade silenciosamente. **Por que ADIAR (design reconhecido):** documentado no as-built §27 e no comentário cruzado (`:546-548`) que espelha `SkillsMatrix.SkillsTabIndex` (item 035). Derivar o índice dinamicamente do conjunto de painéis seria mais robusto, mas é decisão de design (e o mesmo padrão já foi aceito no 035). Sem bug no estado atual.

### CR-01-05 · 🟢 D — `_searchIndex`/índices ficam obsoletos se um mod mutar a DB em runtime — ADIADO

**Arquivo:** `CatalogService.cs` (índices `Lazy<T>` `:105-126`).

Os índices assumem DB imutável pós-boot; um mod que mute `GetItems`/`GetHandbook`/`GetCustomization`/locales em runtime veria índice obsoleto. **Por que ADIAR (risco aceito na spec):** premissa PA-037-06 explícita — caso não suportado pelo SPT (mods mutam a DB só no `PostDBModLoader`/load order). Mitigação de ordem (índices nunca `.Value` no ctor, só no 1º acesso pós-boot) está implementada corretamente. Documentado e aceito; não há ação de código.

## Aplicados

Nenhum. Não há achado na classe SEGURA — o código entregue está correto e fiel à spec.

## Build após review

`dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release --no-incremental --nologo` → **0 erros / 0 avisos** (sem alterações de código nesta passada; build reconfirma o baseline da entrega `d180195`).

## Pendência (não-código) que mantém o item curto de "fechado"

A spec funcional exige **medição quantitativa before/after** (mediana de ≥3 amostras: listagem fria/quente, busca fria/quente, navegação) e **verificação funcional in-game** (memory `feedback_spt_validation`: escrita+hash não basta). Ambas estão como TODO no as-built §40/§54 — exigem um server SPT 4.0 rodando. O **código** está implementado + revisado + compila; a evidência quantitativa fica documentada como pendência.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Code review 01 criado via `/code-review` (autônomo — usuário ausente). 0 achados SEGUROS aplicados; 5 trade-offs de design/hardening/doc ADIADOS (CR-01-01..05), todos já reconhecidos pela spec/review-01/as-built. Build de referência 0/0. Nenhuma alteração de código. Pendência real do item é a medição before/after + verificação in-game (runtime). |
