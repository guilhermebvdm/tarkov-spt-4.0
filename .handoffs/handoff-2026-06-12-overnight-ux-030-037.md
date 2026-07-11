# Handoff — Overnight UX épico 030–037 (2026-06-12)

Execução autônoma noturna do épico de UX do editor CustomClasses (waves UX-W0..W4).
Usuário ausente; agentes decidiram com bom senso e registraram premissas.

> **Baseline:** commit `3634548`. **Sem push** (todos os commits locais; usuário revisa e dá push).

## W0 — 037 (performance: cache + índices + throttle)

- **Status:** 🟢 entregue (implementado + revisado + compila + boota).
- **Incidente:** o Workflow da W0 estourou o **limite de sessão** após escrever o código (spec/tech-spec/review + code-mod rodaram; build/finalize falharam). O loop principal **salvou** o estado: build confirmado 0/0 e commit manual.
- **Commits:** `d180195` (cache ClassFileEntry por mtime+length, invalidação Save/Delete/Create/Duplicate; índices Lazy thread-safe no CatalogService tratando DB imutável pós-boot; ClassEdit com 1 ComputeLoadoutCost + CheckStashCapacity só na aba Stash + debounce ~250ms + FlushPendingRecompute no save; instrumentação Stopwatch LogDebug) · `8e38bde` (code-review 037, doc 04) · `515ab1c` (status 🟡→🟢).
- **Code-review:** 0 fixes necessários (código correto/fiel à spec). 5 achados adiados, todos já documentados nas specs: CR-01-01 alias da lista Diagnostics (invariante aceita), CR-01-02 dispose de `_recomputeCts` (leak marginal, fix tem race), CR-01-03 `GetCachedEntries()` re-varre FS (disciplina do 030), CR-01-04 `StashTabIndex=6` hard-coded, CR-01-05 índices Lazy obsoletos se mod mutar DB em runtime (risco aceito).
- **PENDÊNCIA (não-código):** medição quantitativa before/after — os logs `[perf]` são `LogDebug` e o server roda em Info; precisa subir com log Debug + um baseline pré-037 pra comparar. Validação qualitativa OK (ver "Validação runtime" no fim).

## W1 — 030 + 031

### Status por item

| Item | Título | Status | Causa |
|---|---|---|---|
| 030 | Sidebar persistente de classes | 🟢 Vivo (build limpo) | — |
| 031 | Skills em ordem canônica (componente) | 🟢 Vivo (build limpo) | — |

Nenhum item bloqueado nesta wave. Rebuild final `dotnet build -c Release --no-incremental`: **0 erros / 0 avisos**.

### Hashes

- `73228e9` — feat(CustomClasses): persistent class sidebar (030)
- `1db0dad` — feat(CustomClasses): canonical skills component (031)
- `e33034d` — docs(CustomClasses): mark 030/031 status

### Arquivos

**030** (sidebar + guard):
- `mods/CustomClasses/modded/Server/Web/Shared/NavMenu.razor`
- `mods/CustomClasses/modded/Server/Web/Layouts/BaseLayout.razor`
- `mods/CustomClasses/backlog/030-sidebar-classes/` (specs 01–05)

**031** (skills canônicas):
- `mods/CustomClasses/modded/Server/SkillMaster.cs`
- `mods/CustomClasses/modded/Server/Web/Shared/SkillCanonicalList.razor`
- `mods/CustomClasses/modded/Server/Web/Pages/ClassDetail.razor`
- `mods/CustomClasses/modded/Server/Web/Pages/ClassEdit.razor`
- `mods/CustomClasses/backlog/031-skills-ordem-canonica/` (specs 01–05)

### Premissas registradas

- **ClassEditorService.cs não foi tocado** nesta wave (não havia `ListClassSummaries` novo no working tree), então não entrou no commit do 030. O `ListClassSummaries()` leve descrito no escopo do 030 ou já existia ou está coberto pelo que está em NavMenu/BaseLayout — confirmar no diff se necessário.
- Handoff target não existia ainda; **criei o arquivo** `handoff-2026-06-12-overnight-ux-030-037.md` com esta seção.
- Stage feito com **pathspec explícito por item** (nunca `git add -A`). Conferido com `git status --porcelain` que nada fora do território entrou (excluídos: `docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/*`).
- Aviso CRLF/LF ao stagear `SkillMaster.cs` (normalização de fim de linha do Git) — inofensivo.
- **Validação só por build/hash**: nenhuma validação no jogo/browser foi feita (usuário ausente). Recomenda-se smoke real no viewer antes de considerar 030/031 "aprovados" (memória `feedback_spt_validation.md`).

### Achados ADIADOS (para o usuário decidir)

**030** (code review 01, 0 aplicados / 5 adiados):
- **CR-05** — `FilteredRows()` aloca List novo por render quando há filtro. Eficiência marginal em lista pequena; não é bug nem leak. Memoizar é decisão de design.
- **CR-06** — `@bind-Value:after=StateHasChanged` redundante com o re-render do `@bind`. Cosmético/ruído; inofensivo (Blazor coalesce o render).
- **CR-07** — `SidebarRow.FileName` e `.HasError` não são lidos no render. Fio solto cosmético; record imutável custo zero, pode servir a uso futuro. Remover muda o view-model.
- **CR-08** — Guard depende de `ClassEdit.razor` (025/026) para ter sinal de dirty. Wire-up do `IsDirty`/`SaveAsync`/`Discard` vive fora do território 030; dependência cross-item já documentada.
- **CR-09** — `EditGuardState` exposto como tipo aninhado público em `BaseLayout`. Decisão de arquitetura (vs POCO de namespace próprio); funciona e mover afetaria o consumidor em 025/026.

**031** (code review 01, 1 aplicado / 5 adiados):
- **CR-01-01** — `MultiplierOf`/`CostOf` fazem varredura linear por linha renderizada. Otimização, não correção. Escala real (≤35 linhas) torna o ganho irrelevante.
- **CR-01-02** — `BuildCompareLookup()` reconstruído duas vezes por render. Caminho exclusivo do modo Compare (item 036), sem chamador real ainda. Consolidar cruza a fronteira do 036.
- **CR-01-04** — `_outsideToAdd` no predicado de exibição do overflow é ramo morto. Inofensivo (`overflow.Count>0` já cobre o caso). Remoção é simplificação de layout.
- **CR-01-05** — Chip de multiplicador some para skill fora da canônica com nível 0. Borda de borda; fix exige decidir unir chaves de `Multipliers` ao overflow — escolha de design ambígua.
- **CR-01-06** — Duplicata de skill em `EditRows`: display last-wins vs edição/save first-wins. Só afeta arquivo malformado (UI impede duplicatas); alinhar é mudança de comportamento ambígua sobre input inválido.

---

## W2 — 032 + 033

> **Data:** 2026-06-12 (GMT-3) · **Agente:** finalizador W2 · **Build:** Release `--no-incremental` → 0 erros / 0 avisos (`CustomClasses-Server.dll`).

### Status & hashes

| Item | Status | Hash | Descrição |
|---|---|---|---|
| 032 | 🟢 vivo | `2e3ea9c` | Matriz skills × classes (heatmap), página `/customclasses/skills` + nav link |
| 033 | 🟢 vivo | `d52211f` | Detalhe single-screen (dashboard denso, full-width, `.cc-dense`) |
| — | docs | `adcb04d` | mod-backlog.md: 032/033 → 🟢 |

### Arquivos

**032** (matriz de skills):
- `mods/CustomClasses/modded/Server/Web/Pages/SkillsMatrix.razor` (novo)
- `mods/CustomClasses/modded/Server/Web/Shared/NavMenu.razor` (+nav link "Skills matrix")
- `mods/CustomClasses/backlog/032-matriz-skills/` (specs 01–05)

**033** (single-screen dashboard):
- `mods/CustomClasses/modded/Server/Web/wwwroot/css/customclasses.css` (novo)
- `mods/CustomClasses/modded/Server/Web/Pages/ClassDetail.razor` (reescrita: −259/+ layout denso)
- `mods/CustomClasses/modded/Server/Web/Layouts/BaseLayout.razor` (+hook de stylesheet)
- `mods/CustomClasses/backlog/033-detalhe-single-screen/` (specs 01–05)

### Premissas registradas

- Stage por **pathspec explícito por item** (nunca `git add -A`). `git status --porcelain` conferido após cada stage: nada fora de território entrou. Excluídos e deixados unstaged: `docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/`.
- **NavMenu.razor** (vizinho do 030) e **BaseLayout.razor / ClassDetail.razor** (vizinhos do 031) já estavam no working tree pré-existentes; o diff de cada um foi confeirido como pertencente exclusivamente ao seu item (032 = só o nav link novo; 033 = layout/hook/CSS).
- Primeiro commit do 032 saiu com `@` literal no subject (here-string PowerShell `@'...'@` interpretado pelo Bash). Corrigido via `git commit --amend -F arquivo` — hash final `2e3ea9c`. Nenhum push feito, amend seguro.
- **Validação só por build/hash**: sem smoke no jogo/browser (usuário ausente). Recomenda-se validar no viewer SPT antes de "aprovado" — em especial os pontos de layout adiados abaixo (memória `feedback_spt_validation.md`).
- 030/031 (sidebar + `SkillCanonicalList`/`SkillMaster`) reutilizados, não duplicados.

### Achados ADIADOS (para o usuário decidir)

**032** (code review 01, 0 aplicados / 4 adiados):
- **CR-01-01** — `@using CustomClasses.Web` não utilizado em `SkillsMatrix.razor`. Higiene/cosmético: copiado de `SkillCanonicalList` mas nenhum tipo de `CustomClasses.Web` é referenciado (tipos usados estão no namespace raiz `CustomClasses`). Razor não emite warning; build verde. Fora das categorias seguras de auto-fix.
- **CR-01-02** — `ColumnSpan` re-executa `VisibleColumns().ToList()` por separador. Eficiência, não correção: ambos dependem só de `_showDisabled` (estável no render), então o colspan sempre bate com as colunas renderizadas. Só aloca uma List extra por header de categoria/overflow; irrelevante na escala realista (Y1). Fix exige reorg de render, não bug-fix local.
- **CR-01-03** — `MudChip` de multiplicador em célula estreita (~52px) pode estourar layout. Design/layout: precisa validação visual no servidor SPT (as-built já lista como pendente). Fidelidade ao viewer mas com restrição de largura do header rotacionado do editor. Não é crash.
- **CR-01-04** — `title` em células/headers não previsto explicitamente na spec. Fuga de spec ADITIVA e cosmética, já registrada como premissa na as-built (paridade com `profiles-skills.js`). Sem fix inequívoco a aplicar (remover pioraria a UX).

**033** (code review 01, 0 aplicados / 4 adiados):
- **DEF-1** — `MaxWidth.False` full-width pode espremer colunas em viewport estreito (~960px–full). Design/cosmético, desktop-first; R6 do 03 já delega ajuste fino ao 035. Não é bug.
- **DEF-2** — `!important` em `.cc-dense td/th`. Design; escopado a `.cc-dense` e documentado (PT-2/R7). Alternativa por especificidade pura é frágil contra o CSS do MudBlazor.
- **DEF-3** — Ordem dos slots de equipped / chips de hideout depende da ordem do dicionário. Cross-território: ordem canônica de slots é meta do 034 (GearPanel). Comportamento idêntico ao painel anterior.
- **DEF-4** — Estilos inline remanescentes nos badges (font-size/width). Design/cosmético sem bug; refino para o 035 (densidade global), não para o 033.

## W3 — 034

> **Data:** 2026-06-12 (GMT-3) · **Agente:** finalizador W3 · **Build:** Release `--no-incremental --nologo` → 0 erros / 0 avisos (`CustomClasses-Server.dll`).

### Status & hashes

| Item | Status | Hash | Descrição |
|---|---|---|---|
| 034 | 🟢 vivo | `72866cc` | Painéis visuais de loadout: `GearPanel` (slots + ícone tarkov.dev dimensionado por W×H) + `StashPanel` (grid agrupado por categoria, badge qty, subtotal ₽) + `ItemTooltip` (hover nome/categoria/tamanho/preço); aba Stash do editor ganha agrupamento por categoria + filtro. Fecha a meta single-screen do 033; degrada pra texto offline. |
| — | docs | `6a03e12` | mod-backlog.md: 034 → 🟢 |

### Arquivos

**034** (loadout visual):
- `mods/CustomClasses/modded/Server/Web/Shared/GearPanel.razor` (novo)
- `mods/CustomClasses/modded/Server/Web/Shared/StashPanel.razor` (novo)
- `mods/CustomClasses/modded/Server/Web/Shared/ItemTooltip.razor` (novo)
- `mods/CustomClasses/modded/Server/Web/Pages/ClassDetail.razor` (modificado — `#cc-equipped`→`GearPanel`, `#cc-stash`→`StashPanel`)
- `mods/CustomClasses/modded/Server/Web/Pages/ClassEdit.razor` (modificado — aba Stash: filtro + agrupamento por categoria)
- `mods/CustomClasses/modded/Server/Web/wwwroot/css/customclasses.css` (modificado — só classes novas `cc-item-*`/`cc-gear-*`/`cc-stash-*`)
- `mods/CustomClasses/modded/Server/CatalogService.cs` (modificado — getters `GetItemDimensions`/`GetCategoryId`/`GetCategoryName` sobre o `_handbookIndex` do 037, sem índice novo)
- `mods/CustomClasses/backlog/034-loadout-visual/` (specs 01–05)

### Premissas registradas

- **DESVIO da lista de stage do prompt:** o prompt listou os 6 arquivos visuais (3 Shared + ClassDetail + ClassEdit + CSS) mas NÃO `CatalogService.cs`. Incluí `CatalogService.cs` no commit 034 mesmo assim. Razão: o diff dele é **exclusivamente** os 3 getters do 034 (comentário `// item 034`, último commit anterior = 037 já commitado em `d180195`, sem vazamento de outro item); e os `.razor` staged **chamam** esses getters — omiti-los produziria um commit que **não compila** isoladamente (quebra de atomicidade). Decisão autônoma de bom senso (usuário ausente). Build Release pós-stage: 0/0.
- Stage por **pathspec explícito por arquivo** (nunca `git add -A`). `git status --porcelain` conferido após o stage: confirmado que NÃO entraram `docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/` — todos permanecem unstaged.
- Commit via `git commit -F <arquivo temp>` (`.git/COMMIT_034.txt`, removido após), evitando o bug de `@` literal do here-string PowerShell visto no W2 (032).
- **Validação só por build/hash**: sem smoke no jogo/browser (usuário ausente). Render real (ícones tarkov.dev online/offline, single-screen 1080p, tooltip hover) NÃO verificado — plano §1-5 da spec técnica pendente (memória `feedback_spt_validation.md`). Compilação client+server acoplada (`project_customclasses_session_split`): só o csproj Server foi compilado, conforme o prompt.
- Reuso confirmado, sem duplicação: contratos congelados do 033 (`def.Loadout?.Equipped`, `_stashLines`/`LoadoutCostEntry`), `_handbookIndex` lazy do 037 (sem índice novo), CSS do 033 (`cc-equip-slot*`) preservado.

### Achados ADIADOS (para o usuário decidir)

**034** (code review 01, 2 aplicados / 4 adiados). Aplicados já no commit `72866cc`: CR-01-01 (CS8625 — `MongoId?` ternário, cast `(MongoId?)null` em `GearPanel.razor`) e CR-01-02 (MUD0002 — `Dense="true"` ilegal removido de `MudTextField` em `ClassEdit.razor`). Adiados:
- **CR-01-D1** — Aba Stash do editor sem mensagem "filtro sem resultados". `BuildStashGroups()` retorna lista vazia e o `@foreach` não renderiza nada (aba some, sem aviso). Fuga de spec real (corner case linha 72), MAS o fix carrega **decisão de UX** (texto exato, idioma pt/en, posição) — não inequívoco/local. Fix sugerido: `@if (!groups.Any() && filtro ativo)` com `<MudText>` curto após o campo de filtro. Arquivo: `ClassEdit.razor:461`.
- **CR-01-D2** — `MissingPriceBadge` órfão em `ClassDetail.razor:456-461`. Dead code (só usado no `RowTemplate` da `MudTable` removida); não quebra build nem gera warning (Razor não avisa método privado de instância não usado). Remover é limpeza de fio solto mas toca região fora do diff visual estrito e o `StashPanel` poderia reusar o conceito de badge — manutenção, não bug. Cleanup opcional.
- **CR-01-D3** — Divergência de resolução de grupo: `StashPanel` agrupa por `line.Tpl` (tpl expandido pelo CostService) via `GetCategoryId` + mapa id→name; `ClassEdit.BuildStashGroups` agrupa pelo **root tpl** (preset>tpl) via `GetCategoryName`. Mesma taxonomia (handbook) mas entrada distinta por construção — uma linha de preset pode cair em grupos diferentes entre detalhe e editor. **Comportamento esperado** (fontes de dados distintas: `LoadoutCostEntry` expandido vs `ItemSpecModel` cru); unificar exige decisão de produto sobre qual taxonomia é "a verdade". Arquivos: `StashPanel.razor:66-74` vs `ClassEdit.razor:907-922`.
- **CR-01-D4** — `OrderBy` de grupos por `Name` localizado (`OrdinalIgnoreCase`): ordem dos grupos muda com o idioma e "Other" não tem posição fixa (cai alfabeticamente). Escolha de layout/ordenação (design), não correção. Arquivos: `StashPanel.razor:89`, `ClassEdit.razor:920`.

## W3 — 036

> **Data:** 2026-06-12 (GMT-3) · **Agente:** finalizador W3 (036) · **Build:** Release `--no-incremental --nologo` → 0 erros / 0 avisos (`CustomClasses-Server.dll`, 00:00:01.91).

### Status & hashes

| Item | Status | Hash | Descrição |
|---|---|---|---|
| 036 | 🟢 vivo | `b4dc2cf` | Comparação A×B no detail: picker "Compare with…", `SkillCanonicalList` (031) ganha coluna fantasma B com deltas por skill, chips de delta de custo ponderado / loadout ₽ no header, hideout + outfit lado a lado em 2 colunas; B fixa enquanto A navega pelo sidebar; deep-link `?compare=`. Read-only/efêmero, CSS aditivo. |
| — | docs | `68512d1` | mod-backlog.md: 036 → 🟢 |

### Arquivos

**036** (comparação A×B):
- `mods/CustomClasses/modded/Server/Web/Pages/ClassDetail.razor` (modificado, +256/−29 — picker `Compare with…`, estado de compare + reset no topo de `Reload()`, `ResolveCompare`, badges A×B, `DeltaChip`, `HideoutBlock`/`OutfitBlock`, layout 2 colunas)
- `mods/CustomClasses/modded/Server/Web/wwwroot/css/customclasses.css` (modificado, +8 aditivo — `.cc-cmp-b`, `.cc-cmp-2col`, `.cc-cmp-col__head`)
- `mods/CustomClasses/backlog/036-comparacao-classes/` (specs 01–05)
- **NÃO tocado:** `SkillCanonicalList.razor` — o suporte a `Compare`/`DeltaCell`/overflow de B já era do 031 (confirmado por `git status` limpo); a lista de stage do prompt o incluía por precaução, mas não havia diff a commitar.

### Premissas registradas

- **SkillCanonicalList ausente do commit por design:** o prompt mandou stage explícito de `SkillCanonicalList.razor`, mas `git status --porcelain` mostrou o arquivo **limpo** (sem modificações deste item — os hooks de compare/delta são contrato pré-existente do 031). `git add` de path sem diff é no-op; o commit 036 ficou com 7 arquivos (ClassDetail + CSS + 5 specs), conforme o diff real. Sem desvio de comportamento.
- **00-kickoff já commitado:** `036-comparacao-classes-00-kickoff.md` foi commitado em sessão anterior (tracked/clean); o `git add` da pasta de backlog só staged os specs novos 01–05.
- Stage por **pathspec explícito** (nunca `git add -A`); precedido de `git reset -q` para limpar índice. `git status --porcelain` conferido pós-stage: confirmado que NÃO entraram `docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/` — todos permanecem unstaged.
- Commit via `git commit -F .commitmsg-036.txt` (arquivo temp na raiz, untracked, removido após), evitando o bug de `@` literal do here-string PowerShell.
- **Validação só por build/hash** (usuário ausente): sem smoke no jogo/browser. Render real do picker, coluna fantasma, deep-link `?compare=` e layout 2-col em 1080p NÃO verificado (memória `feedback_spt_validation.md`). Só o csproj Server foi compilado (acoplamento client+server — `project_customclasses_session_split`).
- Reuso confirmado, sem duplicação: `SkillCanonicalList`/`DeltaCell` (031), `ClassDetail` single-screen (033), `CatalogService`/índices (037), CSS do 033/034 preservado. 🔴-R1/R2/R3 do spec-tech-review (03) respeitadas in-code.

### Achados ADIADOS (para o usuário decidir)

**036** (code review 01 `04-code-review-01.md`, **0 aplicados / 7 adiados** — implementação compila limpa, segue spec-tech 02 e decisões 🔴 do 03; nenhum bug/crash/build-breaker/leak/fuga inequívoca no diff). Adiados (todos quality/design, não tocam código):
- **CR-01-D1** — Dead field `_compareEntry` em `ClassDetail.razor:332`: escrito em `:351` (reset) e `:428` (resolve), **nunca lido** (gating usa `_compareDef`/`IsComparing`). Código morto inócuo; remoção é cleanup de qualidade, não correção — não há consumidor quebrado. Candidato a pass de qualidade.
- **CR-01-D2** — `_compareDef!.Name` pode ser `null` no label do picker (`:43` "Comparing: ") e no cabeçalho da coluna B (`:234`/`:257` "B — "): renderiza string vazia (não crash — `!` é null-forgiving de compilação, interpolação de `null` → vazio). O `MudMenuItem` de candidato (`:55`) já usa fallback `?? Path.GetFileNameWithoutExtension(c.FileName)`; label/head de B não. Fix = decisão de UI (qual fallback). Sugestão: reusar `?? Path.GetFileNameWithoutExtension(_compareEntry!.FileName)`.
- **CR-01-D3** — Polaridade de cor divergente: `DeltaChip` (`:545-565`) pinta da ótica de A (▲ verde = A maior); `DeltaCell` do `SkillCanonicalList` (031) pinta B−A (▲ verde = B maior). **Decisão de design registrada** (🔴-R1 no 03, comentada in-code `:537-543`/`:200-202`), com rótulos "vs B" / "Δ B−A" pra desambiguar. Unificar exigiria reescrever contrato 031→036 (proibido por PA-036-03) ou inverter badges — design, não bug. Reavaliação no 035 (🟡-Y2).
- **CR-01-D4** — `higherIsA` removido do `DeltaChip` (spec-tech `:191` previa o parâmetro): todas as 3 métricas pintam verde = "A tem o número maior". Skill cost / loadout ₽ maiores não são inequivocamente "bons". Decisão v1 registrada (🟡-Y2 no 03, premissa no 05) — leitura comparativa, não veredito de balanceamento; mitigada pelo `SkillTotalChip` (budget) ao lado. Mudar semântica de cor = design.
- **CR-01-D5** — Multiplicadores de B não aparecem lado a lado: `SkillCanonicalList` mostra ±% só de A (limitação v1). Decisão 🔴-R2 (opção A); implementar exigiria parâmetro aditivo `CompareMultipliers` no componente 031 (território compartilhado, fora da v1). Follow-up nomeado.
- **CR-01-D6** — Nomes de roupa de B caem no fallback "id cru": `OutfitBlock(_compareDef.Outfit)` usa `ClothingLabel` que resolve só via `_clothingNames` (populado p/ A em `Reload`). 🟡-Y4 registrado — comparação textual de outfit fora de escopo aprofundar; popular catálogo de B = custo sem valor proporcional.
- **CR-01-D7** — `SkillCanonicalList.razor:181` (`BuildOverflowEntries`) chama `BuildCompareLookup()` 2×/render (o `@{}` de topo já montou `compareLevels`), reconstruindo o dict de B. **Pré-existente do 031**, fora do diff do 036; micro-ineficiência (≤~70 skills), não bug; tocar 031 é cross-território (PA-036-03). Registrado p/ quem mexer no 031.

## W4 — 035

| Item | Status | Hash | Resumo |
|---|---|---|---|
| 035 | 🟢 vivo | `fdc9439` | Passada de polimento sobre 030–037: lista com 3 colunas ordenáveis + Edit por linha; sidebar Edit no hover + navegação edit→edit preservando a aba; célula/header da matriz → `/edit?tab=1` (Skills); pin do drawer (Mini↔Persistent); `Ctrl/Cmd+S` salva (1º JS interop do mod); preferências persistidas em `localStorage` (`cc.ui.*`: drawerPinned, editTab, listSort, matrixToggles, sidebarFilter). Densidade (a) reduziu-se a `MudTabs PanelClass pa-4→pa-2` (resto já denso). Server-only Blazor/Razor/JS; sem Harmony. Build Release `--no-incremental`: **0 erros / 0 avisos**. |
| — | docs | `2d52e37` | mod-backlog.md: 035 → 🟢 |

### Arquivos (commit `fdc9439`, 14 paths)

**Criados:**
- `mods/CustomClasses/modded/Server/Web/wwwroot/js/customclasses.js` — `window.ccPrefs` (IIFE, `<script src>` não-módulo): `get/set/remove` sobre `localStorage` com `try/catch`; `registerSaveShortcut(dotNetRef)` (keydown global em capture, Ctrl/Cmd+S → `preventDefault` + `invokeMethodAsync('OnSaveShortcut')`, guard de pathname `/edit`); `unregisterSaveShortcut()`.
- `mods/CustomClasses/modded/Server/Web/UiPrefs.cs` — helper estático de interop (`namespace CustomClasses.Web`), chaves `const`, `GetAsync/SetAsync/GetIntAsync/GetBoolAsync` engolindo `JSException`+`InvalidOperationException` (prerender) → default.

**Editados:**
- `_imports.razor` — `@using Microsoft.JSInterop`.
- `Layouts/BaseLayout.razor` — `<script src>` no `<HeadContent>`; pin do drawer (`Variant` Mini↔Persistent, `OpenMiniOnHover="@(!_drawerPinned)"`); botão de pin; reconciliação no `OnAfterRenderAsync`.
- `Pages/Classes.razor` — 3 colunas ordenáveis (`MudTableSortLabel`; null→`double.MaxValue`); Edit por linha; persistência de sort (`cc.ui.listSort`), restauração via `@ref` + `ToggleSortDirection()`.
- `Pages/ClassEdit.razor` — `[SupplyParameterFromQuery(Name="tab")]`; aplica `?tab` 1×/instância; registra/desregistra Ctrl+S; `[JSInvokable] OnSaveShortcut`→`InvokeAsync(SaveAsync)`; setter de `ActivePanelIndex` grava `cc.ui.editTab`; `Dispose` desregistra + `_dotNetRef.Dispose()`; `PanelClass pa-2`.
- `Pages/SkillsMatrix.razor` — `NavigateTo(col)`→`/edit?tab=1` (`SkillsTabIndex=1` espelha o mapa de abas); toggles `_showDisabled`/`_showMultipliers` persistidos (`cc.ui.matrixToggles`).
- `Shared/NavMenu.razor` — Edit no hover (`.cc-sidebar-edit` scoped); `?tab={_editTab}` no ramo edit→edit, `_editTab` re-lido a cada `LocationChanged`; filtro persistido (`cc.ui.sidebarFilter`).
- `mods/CustomClasses/docs/class-editor.md` — linha no Histórico de Alterações registrando a entrega do código 035.
- `mods/CustomClasses/backlog/035-densidade-cliques/` — specs 01–05 (00-kickoff já era tracked de sessão anterior).

### Achados ADIADOS (code review 01 `04-code-review-01.md` — **0 aplicados / 4 adiados**)

Build de referência 0/0; 0 achados na classe SEGURA (null/crash, build-breaker, fuga de spec com fix local inequívoco, leak/dispose, interop quebrado). Todos os 4 são trade-offs de timing/design **já reconhecidos** na spec/review — tocá-los reabriria decisão de design (proibido sem aprovação). Nenhum toca código.

- **CR-01-01 · 🟢 `OnSaveShortcut` pode rodar em componente disposto (race).** `ClassEdit.razor` (`OnSaveShortcut`/`Dispose`). Entre keypress e entrega do `invokeMethodAsync`, se o `ClassEdit` for disposto, `InvokeAsync(...)` pode lançar `ObjectDisposedException`. ADIADO: `Dispose` desregistra o `keydown` **antes**; guard de pathname `/edit` no JS é backstop; chamada em voo vira promise rejeitada não-observada no JS (inócua). Race estreita já reconhecida em PA-R1-06 e no risco "DotNetObjectReference" da spec §7. Fix defensável (`if (_disposed) return;` no topo de `OnSaveShortcut`) mexe em fluxo de evento já coberto por dispose — decisão de design, não correção de crash inequívoca.
- **CR-01-02 · 🟢 `_editTab` no NavMenu re-lido fire-and-forget no `LocationChanged`.** `NavMenu.razor` (`OnLocationChanged`→`RefreshEditTabAsync`). Não aguardado; `_editTab` atualiza um render depois → clique imediatíssimo em outra classe logo após navegar poderia carregar a aba anterior. ADIADO: projetado e documentado (PA-035-04); query `?tab` só lida ao clicar em outra classe; defasagem de 1 render aceitável (single-user local). Await síncrono mudaria o modelo de timing — design.
- **CR-01-03 · 🟢 Flash default→persistido a cada navegação para página com pref.** Todos os `OnAfterRenderAsync` (drawer pin, aba, ordenação, toggles, filtro). Reconciliação pós-circuito monta com default e aplica o salvo → "flash" de 1 frame. ADIADO: aceito em PA-035-03/PA-R1-09 (single-user; prerender estático sem JS). Sem fix local sem mudar a estratégia de render (persistência server-side, fora de escopo).
- **CR-01-04 · 🟢 Restauração de ordenação re-dispara `SortDirectionChanged` (re-persistência).** `Classes.razor` (`OnAfterRenderAsync`→`ToggleSortDirection`). Restaurar via `ToggleSortDirection()` re-dispara `SortDirectionChanged`→`OnSortChanged`, re-gravando o **mesmo** valor. ADIADO: idempotente (`<label>|<dir>`), em `try/catch` que degrada p/ ordem-de-arquivo se a API do Mud divergir. Mecanismo confirmado contra `MudBlazor.xml` 8.13.0 (PA-AB-035-02).

### Premissas registradas (W4)

- **PA-AB-035-01 (escopo da entrega):** a "passada de regressão visual Chrome MCP + re-medição dos tempos do 037" do kickoff **NÃO** faz parte desta entrega — fica para o **orquestrador** executar na validação final com o server real. Esta entrega é só **código + docs** (instrução explícita da tarefa). A verificação funcional do `05-asbuild.md` (densidade sem MUD0002, sort/persistência, Ctrl+S, deep-links) permanece TODO de runtime (memória `feedback_spt_validation.md`).
- **PA-AB-035-02 (API de sort):** restauração via `ToggleSortDirection` escolhida da inspeção do `MudBlazor.xml` 8.13.0 (`MudTable<T>` não expõe `SetSortLabel` público; `SetSortDirection` não re-ordena). `try/catch` degrada p/ ordem de arquivo em drift de API. Confirmar visualmente no runtime.
- **PA-AB-035-03 (`@using Microsoft.JSInterop`):** adicionado ao `_imports.razor` (não por arquivo) p/ as 4 páginas com interop.
- **PA-AB-035-04 (densidade já presente):** pickers/diálogos já densos (confirma PA-R1-05); passada (a) reduziu-se a `PanelClass pa-4→pa-2`; sem regra css global nova. `MUD0002` evitado (CR-01-02 do 034 respeitado).
- **PA-AB-035-05 (Dispose obrigatório do listener):** `ClassEdit.Dispose` chama `unregisterSaveShortcut` + `_dotNetRef.Dispose()`; guard de pathname no JS é backstop, não substituto.
- **Stage por pathspec explícito** (nunca `git add -A`), precedido de `git reset -q`. `git status --porcelain` conferido pós-stage: confirmado que NÃO entraram `docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/` — todos permanecem unstaged (são de outras frentes).
- **Nota de processo:** `git commit --nologo` (flag inexistente) falhou silenciosamente na 1ª tentativa sem criar commit; refeito sem a flag via `git commit -F`. Hash final `fdc9439`.
- Commit via `git commit -F .commitmsg-035.txt` (arquivo temp na raiz, removido após), evitando o bug de `@` literal do here-string PowerShell. Sem push (aguarda aprovação).

---

## Validação runtime (orquestrador, pós-W4)

- **Build integrado final:** `dotnet build CustomClasses.Server.csproj -c Release --no-incremental` → **0 erros / 0 avisos**.
- **Install:** `compile-mod.sh CustomClasses` → client + server compilados e instalados em `D:/SPT`; **config sem divergência** (guard rail 019 satisfeito, sem `--force-config`); `wwwroot/` copiado.
- **Boot smoke (fundação 037):** server subiu limpo — `[CustomClasses] Loaded 11 class(es), skipped 0` + `O servidor iniciou. Bom jogo`, **sem exceção do CustomClasses**. (Único erro no log: `Fika Discord Presence` — config de outro mod, pré-existente, irrelevante.) Bind real: `https://26.207.194.149:6969` (o IP do handoff estava correto).
- **Smoke funcional Chrome MCP:** editor 100% funcional (cert via `thisisunsafe`). Confirmado visualmente:
  - **030** sidebar persistente (pin, filtro, lista com custos + status + ação Edit).
  - **032** link "Skills matrix" na sidebar.
  - **031** SkillCanonicalList renderiza em ordem canônica (Ph→M→C→P→Special Elite), níveis 0 esmaecidos ("—"), spinbuttons inline, chips ±% de multiplicador.
  - **034** abas EQUIPPED/STASH presentes no editor.
  - **035** preferência de vista/aba persistida (URL restaurou `edit?tab=1` ao navegar).
  - 11 classes Registered com custos ponderados e ₽ corretos.
- **PENDENTE — medição quantitativa 037 (before/after):** os logs `[perf]` são `LogDebug`, suprimidos no nível Info do server. Não capturável neste smoke. **Para fechar:** subir o server com log Debug, comparar baseline pré-`d180195` × atual em (a) `ListClassFiles` frio/quente, (b) `Search`, (c) navegação entre vistas. Qualitativamente o editor navega sem travas, mas o número exige a sessão de medição.

## Resumo final

| Item | Status | Commit(s) |
|---|---|---|
| 037 cache+índices+throttle | 🟢 | `d180195` · `8e38bde` · `515ab1c` |
| 030 sidebar persistente | 🟢 | `73228e9` |
| 031 skills ordem canônica | 🟢 | `1db0dad` |
| 032 matriz de skills | 🟢 | `2e3ea9c` |
| 033 dashboard single-screen | 🟢 | `d52211f` |
| 034 loadout visual | 🟢 | `72866cc` |
| 036 comparação A×B | 🟢 | `b4dc2cf` |
| 035 densidade+cliques+prefs | 🟢 | `fdc9439` |

**8/8 entregues.** 15 commits do épico (baseline `3634548`), branch **ahead 22, SEM push**. Build integrado 0/0; boot + editor validados. Arquivos de outras frentes (`docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/`) **intactos/unstaged**.

## Próximos passos (você)

1. **Revisar e dar push** dos 15 commits (nenhum foi pushado).
2. **Medição quantitativa do 037** (sessão com log Debug — ver acima) para fechar o DoD do 037.
3. **QA visual/lógico** das telas no viewer real (build-gate ≠ correção visual): em especial heatmap da matriz em células estreitas (032 CR-01-03), responsividade do dashboard em viewport estreito (033 DEF-1), e a comparação A×B (036).
4. **Achados adiados** (todos neste relatório, por wave) — decidir quais viram follow-up: notáveis = 036 multiplicadores de B lado a lado (exige tocar componente 031), 037 dispose de `_recomputeCts`, 034 mensagem de "filtro sem resultados" na aba Stash.
5. Validação **in-game** das classes (memória `feedback_spt_validation.md`).
