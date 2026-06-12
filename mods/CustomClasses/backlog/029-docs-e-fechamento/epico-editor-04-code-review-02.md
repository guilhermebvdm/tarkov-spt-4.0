# Épico — Editor web de classes (itens 018–029) · Code Review consolidado 02

**Mod:** CustomClasses
**Escopo:** 2ª passada pós-fixes da rodada 01 ([epico-editor-04-code-review-01.md](./epico-editor-04-code-review-01.md)) — foco em (1) código novo dos fixes CR-EP-01/02/04–09 (`InventoryBuilder.PackSpecsIntoGrids`/`PlaceSpecTrees`/`LoadAmmo`, `CostService.ComputeLoadoutCost`/`CheckStashCapacity`, guard do `compile-mod.sh` + deleção no `sync-classes.sh`), (2) camada Web/Razor em profundidade (`ClassEdit`/`ClassEditModel`/`ItemSpecEditor`/diálogos 027/`Classes`/`ClassDetail`/pickers), (3) segurança/robustez do editor como serviço HTTP local.
**Data:** 2026-06-10

> Code review focado em **bugs de correção** (não estilo). Método: leitura integral dos arquivos do foco + caminhada lado a lado builder×custo×capacity com exemplos concretos (arma preset+ammo no stash, mochila com contents, 100k roubles) + teste empírico de filesystem onde necessário. Achados NÃO repetem pontos tratados/aceitos na rodada 01. IDs `CR2-EP-NN`. Severidades: **CR2-BLOQ** (quebra real) · **CR2-MAIOR** (bug em caso plausível) · **CR2-MENOR** (edge raro / robustez).

## Resumo

> 🔴 CR2-BLOQ: 0 · 🟠 CR2-MAIOR: 2 · 🟢 CR2-MENOR: 9 · Total: 11
>
> **2026-06-10:** todos os 11 achados resolvidos — 10 aplicados, 1 (CR2-EP-05) aceito como dívida
> documentada. Achados do teste de UI real (UI-01/02/03) registrados e corrigidos — ver
> [Achados do teste de UI](#achados-do-teste-de-ui-chrome-mcp). Build Release 0 err/0 warn.

## Índice

| ID | Severidade | Categoria | Título | Status |
| --- | --- | --- | --- | --- |
| CR2-EP-01 | 🟠 CR2-MAIOR | C — Gap vs. spec | `CostService.AddSpec`: `contents` NÃO é multiplicado pelo `count` da linha-pai — builder spawna `count×` contents, custo conta 1× | ✅ Aplicado |
| CR2-EP-02 | 🟠 CR2-MAIOR | B — Bug latente | Propagação de deleção (CR-EP-02) cobre **subpastas** de `classes/` que o editor nunca gerencia — draft novo em `classes/_docs/` no repo bloqueia o build e o `/sync-classes` propõe DELETÁ-LO | ✅ Aplicado |
| CR2-EP-03 | 🟢 CR2-MENOR | B — Bug latente | `sync-classes.sh`: install com `config/` mas SEM pasta `classes/` → todas as classes do repo viram "DELETED in install" (mass-delete com `--yes`) | ✅ Aplicado (abort) |
| CR2-EP-04 | 🟢 CR2-MENOR | B — Bug latente | `InventoryBuilder.Apply`: contents são empacotados ANTES do `LoadAmmo` do spec equipado — rig com `loadedMag`+`ammo` enche o carregador de uma ARMA dos contents (interação nova do CR-EP-01) | ✅ Aplicado |
| CR2-EP-05 | 🟢 CR2-MENOR | C — Gap vs. spec | Custo não conta a óptica mínima injetada por `EnsureMinimumOptic` (equipado e stash/contents) — undercost de ~4–30k ₽ por arma sem óptica | 🔶 Dívida documentada |
| CR2-EP-06 | 🟢 CR2-MENOR | C — Gap vs. spec | `CostService.AddAmmo`: árvore manual com nó `cartridges` explícito — builder pula o fill (CR-01-03), custo soma capacidade cheia (skip-check só existe p/ mag de preset) | ✅ Aplicado |
| CR2-EP-07 | 🟢 CR2-MENOR | C — Gap vs. spec | `ComputeLoadoutCost` precifica slot equipado com nome INVÁLIDO (`"Foo": {...}`) que o builder ignora com warning | ✅ Aplicado |
| CR2-EP-08 | 🟢 CR2-MENOR | B — Bug latente | `ItemPicker.RunSearchAsync`: caminho de query vazia não incrementa `_searchVersion` — busca em voo repõe resultados DEPOIS do clear | ✅ Aplicado |
| CR2-EP-09 | 🟢 CR2-MENOR | B — Bug latente | `ClassEdit.SaveAsync`: `_model.ToDefinition()` roda DENTRO do `Task.Run` com o form ainda editável — torn read / `InvalidOperationException` se o usuário mexer numa lista durante o save | ✅ Aplicado |
| CR2-EP-10 | 🟢 CR2-MENOR | F — Robustez | Diálogos 027 e `ItemPicker`: `_busy`/`_searching` sem `try/finally` em volta do `await Task.Run` — exceção deixa o diálogo travado em "busy" | ✅ Aplicado |
| CR2-EP-11 | 🟢 CR2-MENOR | B — Bug latente | `ClassEditorService.Audit`: `name`/`summary` com `\n` embutido forja linhas no `_audit.log` (TSV quebra com `\t` também) | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/Doc** · **F — Robustez/Melhoria**

---

## Pontos

### CR2-EP-01 · C — Gap vs. spec · 🟠 CR2-MAIOR

**`CostService.AddSpec`: `contents` não é multiplicado pelo `count` da linha-pai — o builder spawna `count×` os contents, o custo conta 1×**

**Local:**
- [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddSpec`, loop final (`foreach (var content in spec.Contents ?? []) AddSpec(content, equipped: false, "contents", ...)` — sem repassar `count`).
- Confirmado contra [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `PlaceSpecTrees` (loop `for i < count`: **cada unidade colocada** recursa `PackSpecsIntoGrids(items, root.Id, ..., spec.Contents)`) e o caminho simples stack-aware de `PackSpecsIntoGrids` (contents recursados **por stack colocado**).

**Problema:** a linha-pai (preset/manual/auto-preset/simples) é precificada `qty × count`, mas os `contents` entram no breakdown exatamente UMA vez, independente do `count`. O builder, corretamente, empacota uma cópia dos contents **dentro de cada unidade** colocada (contêineres têm `StackMaxSize` 1 → 1 placement por unidade). O editor permite construir esse caso pela UI (`ItemSpecEditor` no tab Stash tem `AllowCount=true` e seção Contents simultâneas).

**Cenário concreto:** linha de stash "Berkut ×3, contents: 5× morphine" → o jogo nasce com **15 morfinas** (5 em cada mochila); o "Loadout total"/"Stash value" conta **5**. Para contents caros (meds, munição em caixa, mags), o custo total fica visivelmente menor que o spawnado — a mesma "mentira de paridade" que motivou o CR-EP-01.

**Sugestão (1–3 linhas):** propagar o multiplicador: trocar o loop por `foreach (var content in spec.Contents ?? []) AddSpec(content, equipped: false, "contents", items, warnings, multiplier: count);` com um parâmetro `int multiplier = 1` em `AddSpec` aplicado em `count = Math.Max(1, spec.Count) * multiplier` (e repassado na recursão de contents aninhados).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado exatamente como sugerido (`AddSpec(..., int multiplier = 1)`, `count = Math.Max(1, spec.Count) * multiplier`, contents recursam com `multiplier: count`).
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-02 · B — Bug latente · 🟠 CR2-MAIOR

**Propagação de deleção cobre subpastas de `classes/` que o editor nunca toca — draft novo em `classes/_docs/` no repo bloqueia o build e o `/sync-classes` propõe deletá-lo do repo**

**Local:**
- [`.agents/scripts/compile-mod.sh`](../../../../.agents/scripts/compile-mod.sh) — `list_config_files` (`find "$dir/classes" -type f ...` é **recursivo**) + `install_server_config` (qualquer `classes/*` repo-only → `repo_only_classes` → BLOQUEIA a cópia do config).
- [`scripts/sync-classes.sh`](../../scripts/sync-classes.sh) — loop `DELETED` (`case "$rel" in classes/*)` — também recursivo) com `rm -f "$REPO_CONFIG/$rel"`.
- Confirmado contra [`modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) (boot é **non-recursive**: "subfolders ignored — handy for drafts", PA-01-03) e [`modded/Server/ClassEditorService.cs`](../../modded/Server/ClassEditorService.cs) (`ListClassFiles`/`Delete` usam `GetFiles(ClassesPath, false, ...)` — só o topo).

**Problema:** a premissa do fix CR-EP-02 ("um `classes/` repo-only foi deletado pelo editor no install") só vale para o **primeiro nível** de `classes/` — o editor não lista nem deleta arquivos de subpastas. Mas os dois scripts tratam `classes/**` recursivamente. Resultado para um arquivo novo em subpasta no repo (ex.: draft em `classes/_docs/` — padrão explicitamente recomendado pelo comentário do boot loader):
1. `compile-mod.sh`: vira `repo_only_classes` → **bloqueia a cópia inteira do config** com a mensagem "deleted via editor in install; use --force-config ... or sync-classes to propagate the deletion" (diagnóstico errado — nunca houve deleção);
2. `sync-classes.sh`: lista o draft como "DELETED in install (repo copy will be removed)" e, confirmado (ou com `--yes` em fluxo não-interativo), **deleta o arquivo novo do repo**.

**Cenário concreto:** criar `modded/Server/config/classes/_docs/sniper-draft.jsonc` no repo → `/compile-mod` avisa repo-only e sugere `/sync-classes` → rodar `/sync-classes --yes` (como os fluxos de agente fazem) → o draft é apagado do repo sem nunca ter existido no install.

**Por que importa:** o fix CR-EP-02 transformou "arquivo novo em subpasta no repo" (caso legítimo e documentado) num falso positivo de deleção com **perda de dado no repo** como remediação sugerida.

**Sugestão (1–3 linhas):** restringir a semântica de deleção ao escopo do editor: no `case` do `sync-classes.sh` e no `install_server_config`, tratar como deletável/bloqueante apenas `classes/<arquivo>` direto (ex.: `classes/*/*) → copy-as-new ;; classes/*) → deleção ;;` no case, e o equivalente no compile-mod — subpasta repo-only volta a ser "new file, copy".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — `case classes/*/*` antes de `classes/*` nos dois scripts: subpasta repo-only vira copy-as-new no `compile-mod.sh` (array `repo_only_safe`) e é ignorada na detecção de DELETED do `sync-classes.sh`. Testado com `_fake.jsonc` em `classes/_docs/`: compile não bloqueia, sync não propõe deleção.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-03 · B — Bug latente · 🟢 CR2-MENOR

**`sync-classes.sh`: install com `config/` presente mas SEM a pasta `classes/` → todas as classes do repo viram "DELETED"**

**Local:** [`scripts/sync-classes.sh`](../../scripts/sync-classes.sh) — `list_config_files` (`if [[ -d "$dir/classes" ]]` silenciosamente pula a subárvore) + loop `DELETED`.

**Problema:** o script exige `INSTALL_CONFIG` existente (linha 31), mas não exige `INSTALL_CONFIG/classes`. Se a pasta `classes/` do install sumir (limpeza manual, `--spt-path` apontando para um install onde o mod tem `config/` mas nunca recebeu `classes/`), `list_config_files "$INSTALL_CONFIG"` lista só os arquivos da raiz → TODO `classes/*` do repo entra em `DELETED` → com `--yes`, as 11 classes + `_docs/` são removidas do repo num comando. O preview mostra a lista, mas o fluxo não-interativo (`--yes`) não tem chance de abortar. (O `compile-mod.sh` tem o guard inverso correto: `classes/` ausente no install = "fresh install" → full copy.)

**Cenário concreto:** `bash scripts/sync-classes.sh --spt-path /e/SPT-teste --yes` contra um install secundário sem `classes/` → repo limpo de classes (recuperável via git, mas é exatamente o clobber que o item 019 promete evitar).

**Sugestão:** guard de 2 linhas no topo da detecção de deleção: `[[ -d "$INSTALL_CONFIG/classes" ]] || { echo "⚠ install has no classes/ — deletion propagation skipped"; }` e só popular `DELETED` quando a pasta existe.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Aceitar com modificação: em vez de só pular a propagação, o script **aborta** (exit 1) com mensagem clara ("install has no classes/ folder — wrong SPT path?") — install sem `classes/` indica path errado/install estranho; sincronizar qualquer coisa nesse estado é arriscado. Testado: path inexistente e `config/` sem `classes/` abortam sem propor deleções.
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-04 · B — Bug latente · 🟢 CR2-MENOR

**`InventoryBuilder.Apply`: contents empacotados ANTES do `LoadAmmo` do spec equipado — contêiner com `loadedMag`+`ammo` enche o carregador de uma arma que está DENTRO dele**

**Local:** [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `Apply` (contents em ~l.64–71, `LoadAmmo(tree, spec, ...)` em ~l.74) + `LoadAmmo` (`tree.FirstOrDefault(i => i.SlotId == "mod_magazine")`).

**Problema:** interação NOVA criada pelo CR-EP-01: antes, contents eram só `tpl+count` (nunca continham `mod_magazine`); agora um content pode ser uma arma montada (preset/mods), cujos itens são adicionados ao `tree` do slot equipado ANTES do `LoadAmmo` do spec do contêiner rodar. Se o spec equipado (rig/mochila) tiver `loadedMag: true` + `ammo` (JSON à mão — o editor só expõe ammo para armas), a busca global por `mod_magazine` no `tree` encontra o carregador da arma dos contents e o enche com a munição declarada NO RIG. O comportamento esperado (regra do schema) seria o warning "sem mod_magazine — carregador não carregado". O `CostService.AddAmmo` para o mesmo JSON emite o warning de mag ausente e não conta cartucho — custo e builder divergem.

**Cenário concreto:** `"TacticalVest": { "tpl": "<rig>", "loadedMag": true, "ammo": "<5.45 PS>", "contents": [ { "preset": "<AK-74M>" } ] }` → a AK dos contents nasce com o mag cheio de PS "do nada"; o custo não conta esses cartuchos.

**Sugestão:** em `Apply`, chamar `LoadAmmo` ANTES de `PackSpecsIntoGrids` dos contents (cartuchos não mudam o footprint — mesma justificativa do comentário em `PlaceSpecTrees`), ou restringir a busca do mag à subárvore do root equipado.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — variante (a), menor diff: `LoadAmmo` movido para ANTES do bloco de contents em `Apply` (com comentário CR2-EP-04); `PlaceSpecTrees` já fazia a ordem certa. Agora custo (warning de mag ausente) e builder divergem em nada nesse caso.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-05 · C — Gap vs. spec · 🟢 CR2-MENOR

**Custo não conta a óptica mínima injetada por `EnsureMinimumOptic` — undercost para toda arma sem óptica**

**Local:** [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddSpec`/`AddPresetItems` (nenhuma menção a óptica) vs [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `EnsureMinimumOptic` (chamado em `Apply` p/ arma equipada e em `PlaceSpecTrees` p/ arma de stash/contents; pode adicionar red dot OU mount+scope).

**Problema:** o builder injeta uma mira (e às vezes um mount) em qualquer arma cuja árvore final não tenha óptica real — itens reais no perfil, com preço de flea (red dots ~4–30k ₽). O `ComputeLoadoutCost` precifica apenas os itens do preset/árvore declarada, então toda arma "sem óptica de fábrica" (ex.: AKMS default, snipers de stash quando nenhum preset da arma tem scope) sai mais barata no breakdown do que o que o builder spawna. O `CheckStashCapacity` documenta explicitamente por que NÃO espelha a óptica (footprint idêntico) — o custo não tem justificativa equivalente e a doc do serviço promete "cost matches what the builder actually spawns".

**Cenário concreto:** `"FirstPrimaryWeapon": { "preset": "<AKMS tpl>" }` → builder adiciona um red dot determinístico (`PickSimpleOptic`); o breakdown não tem linha de óptica.

**Sugestão:** ou (a) replicar a resolução determinística (`PickSimpleOptic` é puro dado o filter — expor via `CatalogService` e somar a linha "optic (auto)"), ou (b) aceitar como dívida documentando no doc-comment do `CostService` ("auto-optic NÃO é precificada — undercost de até ~30k ₽/arma") + warning informativo no breakdown quando a árvore não tem óptica real.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): **limitação documentada** (decisão do orquestrador) — doc-comment do `CostService` agora declara o gap explicitamente ("Known accepted gap (CR2-EP-05): the minimum optic … is NOT priced — undercost of roughly 4–30k ₽ per weapon"). Espelhar exigiria duplicar a resolução de óptica do builder por valor de display marginal.

### CR2-EP-06 · C — Gap vs. spec · 🟢 CR2-MENOR

**`CostService.AddAmmo`: árvore manual com nó `cartridges` explícito — builder pula o fill, custo soma capacidade cheia (e ainda conta o nó)**

**Local:** [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddAmmo` (o skip-check de "preset already ships cartridges" só roda quando `presetMag is not null`; no ramo `manualMagTpl` não há checagem) + `AddModTree` (conta o nó `cartridges` como 1 unidade). Confirmado contra [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `LoadAmmo` (~l.487: `tree.Any(i => i.ParentId == mag.Id && i.SlotId == "cartridges")` vale para árvore manual também → fill pulado com debug CR-01-03).

**Problema:** numa árvore manual onde o usuário declarou o mag com um filho `slotId: "cartridges"` (forma válida de pré-carregar 1 stack), `loadedMag: true` faz o custo somar `capacidade × count` cartuchos que o builder NÃO vai inserir (o fill é pulado). Impacto: overcost de ~30–60 cartuchos por arma — pequeno, mas é exatamente o espelho do CR-EP-08(a) que esta rodada deveria fechar.

**Cenário concreto:** `"mods": [ { "slotId": "mod_magazine", "tpl": "<mag>", "mods": [ { "slotId": "cartridges", "tpl": "<PS>" } ] } ]` + `loadedMag: true` → builder: fill pulado; custo: +30 cartuchos.

**Sugestão:** no ramo manual, espelhar o check: se `FindMagazine(spec.Mods)` tem um filho com `SlotId == "cartridges"`, não somar o fill (mesmo comentário CR-01-03).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — novo branch em `AddAmmo`: `presetMag is null && manualMag?.Mods?.Any(SlotId == "cartridges")` → fill pulado (nó já contado por `AddModTree`), espelhando o check do builder.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-07 · C — Gap vs. spec · 🟢 CR2-MENOR

**`ComputeLoadoutCost` precifica slot equipado com nome inválido que o builder ignora**

**Local:** [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `ComputeLoadoutCost` (`foreach (var (slotName, spec) in def.Loadout?.Equipped ...) AddSpec(...)` — `slotName` nunca é validado) vs [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `Apply` (`Enum.TryParse<EquipmentSlots>` + `IsDefined`, senão "unknown equipment slot — ignored").

**Problema:** um JSON à mão com `"equipped": { "Backpcak": { "tpl": ... } }` (typo) é pulado pelo builder com warning, mas o custo soma a linha normalmente — o "Loadout total" inclui um item que nunca spawna. O editor não gera esse caso (dropdown restrito ao enum), mas o viewer/lista (024) processa arquivos manuais e mostra o total errado sem nenhum aviso.

**Sugestão:** em `ComputeLoadoutCost`, validar o nome com o mesmo `Enum.TryParse`/`IsDefined` e, quando inválido, pular a linha com warning ("the builder ignores this slot").

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — mesmo `Enum.TryParse<EquipmentSlots>` + `IsDefined` do builder; slot inválido vira warning ("unknown equipment slot — the builder ignores it; line not costed") e a linha não é precificada.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-08 · B — Bug latente · 🟢 CR2-MENOR

**`ItemPicker.RunSearchAsync`: o caminho de query vazia não invalida buscas em voo — resultados "fantasma" reaparecem depois do clear**

**Local:** [`modded/Server/Web/Shared/ItemPicker.razor`](../../modded/Server/Web/Shared/ItemPicker.razor) — `RunSearchAsync` (ramo `string.IsNullOrWhiteSpace(_query)` zera `_results`/`_searched` e retorna **sem** `++_searchVersion`).

**Problema:** o guard `_searchVersion` (elogiado na rodada 01) só é incrementado no caminho de busca não-vazia. Sequência: digitar "ak" → debounce dispara a busca A (scan completo do catálogo no `Task.Run`, lento); apagar tudo → debounce dispara `RunSearchAsync` com query vazia → lista limpa, `_searchVersion` inalterado; busca A termina → `version == _searchVersion` passa → `_results = hits` repõe os resultados de "ak" com a caixa de busca vazia (e `_searching` que o clear não tocou finalmente apaga). Cosmético, mas confunde em pickers filtrados por slot ("por que tem resultado sem query?").

**Sugestão:** 1 linha: `_searchVersion++;` (ou `var _ = ++_searchVersion;`) no início do ramo de query vazia.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — `++_searchVersion` movido para o TOPO de `RunSearchAsync` (cobre os dois ramos); o ramo vazio também zera `_searching` (a busca superada não vai mais limpá-lo — ver CR2-EP-10).
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-09 · B — Bug latente · 🟢 CR2-MENOR

**`ClassEdit.SaveAsync`: `_model.ToDefinition()` executa DENTRO do `Task.Run` enquanto o form continua editável**

**Local:** [`modded/Server/Web/Pages/ClassEdit.razor`](../../modded/Server/Web/Pages/ClassEdit.razor) — `SaveAsync` (~l.661: `await Task.Run(() => EditorService.Save(_resolvedFileName, _model.ToDefinition(), hotApply: true))`).

**Problema:** `_saving` desabilita só os botões Save/Discard — todos os campos, switches e botões "Add" dos tabs continuam ativos durante o save (que roda o pipeline completo de dry-run: clone + builders + GridPacker — dezenas/centenas de ms). `ToDefinition()` enumera `Skills`/`Multipliers`/`Hideout`/`Equipped`/`Stash` numa thread do pool; um clique em "Add"/"Delete" de linha no meio da enumeração dispara `InvalidOperationException` ("Collection was modified") que escapa do handler → erro de circuito Blazor (página quebra até reload). Mesmo sem exceção, o snapshot salvo pode ser um estado "rasgado" (metade antes, metade depois da edição).

**Cenário concreto:** loadout grande (10 linhas de stash com presets) → Save → durante o spinner, usuário adiciona uma skill → crash do circuito ou arquivo salvo com estado intermediário.

**Sugestão:** snapshot na thread do circuito antes do `Task.Run`: `var def = _model.ToDefinition(); var result = await Task.Run(() => EditorService.Save(_resolvedFileName, def, hotApply: true));` (os diálogos 027 já fazem o equivalente — capturam tudo antes).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — `var def = _model.ToDefinition();` materializado no sync context; o `Task.Run` só executa o `Save(def)`.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-10 · F — Robustez · 🟢 CR2-MENOR

**Diálogos 027 e `ItemPicker`: flags `_busy`/`_searching` sem `try/finally` — exceção no `Task.Run` deixa o diálogo permanentemente "busy"**

**Local:**
- [`modded/Server/Web/Shared/ClassLifecycleCreateDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleCreateDialog.razor) — `CreateAsync` (`_busy = true; var result = await Task.Run(...); _busy = false;`)
- [`modded/Server/Web/Shared/ClassLifecycleDuplicateDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleDuplicateDialog.razor) — `DuplicateAsync` (idem)
- [`modded/Server/Web/Shared/ClassLifecycleDeleteDialog.razor`](../../modded/Server/Web/Shared/ClassLifecycleDeleteDialog.razor) — `DeleteAsync`/`DisableAsync` (idem)
- [`modded/Server/Web/Shared/ItemPicker.razor`](../../modded/Server/Web/Shared/ItemPicker.razor) — `RunSearchAsync` (`_searching = true` sem finally)

**Problema:** `ClassEditorService.Save/Delete` fazem IO real (`File.Move` na rotação de backup, `WriteFile`) que pode lançar (arquivo aberto no editor de texto, AV lock). A exceção atravessa o `await`, o `_busy = false` nunca roda e o botão fica desabilitado para sempre (além do erro de circuito). `ClassEdit.SaveAsync` já usa `try/finally` — os diálogos não. No `ItemPicker`, `Search` lançando deixa a `MudProgressLinear` infinita.

**Sugestão:** envolver cada `await Task.Run(...)` em `try { ... } finally { _busy = false; }` (4 diálogos + `_searching` no picker).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — try/finally em `CreateAsync`, `DuplicateAsync`, `DeleteAsync` e `DisableAsync` (4 caminhos nos 3 diálogos) + `RunSearchAsync` do `ItemPicker` (finally condicionado a `version == _searchVersion` para não apagar o flag de uma busca mais nova).
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR2-EP-11 · B — Bug latente · 🟢 CR2-MENOR

**`ClassEditorService.Audit`: input do usuário entra cru no `_audit.log` — newline embutido forja linhas**

**Local:** [`modded/Server/ClassEditorService.cs`](../../modded/Server/ClassEditorService.cs) — `Audit` (`$"{ts}\t{fileName}\t{action}\t{summary}\n"`) com `summary` carregando `plan.Name`/`name` (ex.: `Save`: `$"name='{plan.Name}', ..."`; `Create`/`Duplicate`: `$"name='{trimmed}'"`).

**Problema:** o nome da classe aceita qualquer caractere por design (`ValidateNewClassName`: "Accents are fine"), incluindo `\n`/`\t` colados num campo de texto ou vindos de JSON à mão (o `Trim()` só remove das pontas). Um nome `"X\n2026-01-01T00:00:00Z\tfoo.jsonc\tdelete\t..."` injeta uma linha de auditoria falsa e quebra o formato TSV. Como o log é o rastro oficial de save/delete/create do editor (item 027), vale 1 linha de sanitização.

**Sugestão:** sanitizar no `Audit`: `summary = summary.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");` (idem `fileName`, por simetria — hoje ele já é validado como bare name, mas bare names com `\n` passam no `TryResolveClassFile`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — local function `Sanitize` aplicada a `fileName` e `summary` (`\r`/`\n`/`\t` → espaço) antes de montar a linha TSV.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Achados do teste de UI (Chrome MCP)

> Teste manual do editor no navegador (2026-06-10), fora do escopo da leitura de código da rodada 2.
> Três achados, todos corrigidos junto com os fixes desta rodada.

### UI-01 · 🟠 MAIOR (UX) — Save com campo inválido na tela reporta "Saved" · ✅ Corrigido

**Sintoma:** digitar `nameColor = "laranja"` e clicar SAVE → banner "Saved with hot-apply"; o arquivo
mantém a cor antiga válida (`#ff8800`) mas o CAMPO continua exibindo "laranja". O MudTextField com
`Validation` rejeita o bind (o modelo fica com o valor velho) e o `SaveAsync` só validava o MODELO —
que estava "válido".

**Fix aplicado** ([`Web/Pages/ClassEdit.razor`](../../modded/Server/Web/Pages/ClassEdit.razor)): o
conteúdo do form (o bloco `MudTabs` inteiro) foi envolvido num `<MudForm @ref="_form">`; `SaveAsync`
chama `await _form.Validate()` ANTES de qualquer escrita — se `!_form.IsValid`, o save é abortado com
snackbar de erro ("Save blocked — fix the invalid fields first.") e `_savedOnce = false` (o banner
"Saved" estale de um save anterior some). O check de modelo (`ValidateColor`) permanece como segunda
linha de defesa, também zerando `_savedOnce`.

### UI-02 · 🟠 MAIOR — Picker de customization mostra chaves cruas em vez de nomes · ✅ Corrigido

**Sintoma:** aba Outfit lista entradas como `674db508c2b99a67b69527fa_name` em vez de "USEC Predator";
filtrar por "Predator" acha 0 de 268. Causa: `CatalogService.GetClothing` usava `props.Name ?? item.Name`,
que para entradas de customization são chaves INTERNAS (`Blackknight_Suite`, `pmc_kit_blackknight_quest`,
`{id}_name` em itens de mods) — o nome humano vive no locale global do server sob a chave **`"{id} Name"`**.

**Fix aplicado** ([`CatalogService.cs`](../../modded/Server/CatalogService.cs) — `GetClothing`):
resolução de nome via locale primeiro (en → pt), depois `_props.Name`/`_name`, por último o id (fallback
do consumidor). Validado contra o install real (`D:/SPT/SPT/SPT_Data/database/locales/global/en.json`)
com 3 ids do `scripts/suits-catalog.json` — todos batem exatamente:

| id | `en["{id} Name"]` | suits-catalog `name` |
| --- | --- | --- |
| `64ef3fa81a5f313cb144bf89` | USEC Predator | USEC Predator |
| `6295ef7d1f798f3be747969e` | BEAR Coyote | BEAR Coyote |
| `6658a1d54de4820934746dd4` | BEAR Centurion | BEAR Centurion |

### UI-03 · 🟢 MENOR — Console: `Identifier 'MudPointerEventsNone' has already been declared` · ⚪ Aceito (padrão upstream)

**Sintoma:** SyntaxError em toda página — o `MudBlazor.min.js` executava 2×. Investigação: o host
(`SPTarkov.Server.Web`) NÃO injeta MudBlazor JS para as nossas páginas (o `App.razor` dele só carrega
`blazor.web.js`; o script existe apenas no `BaseMudBlazorLayout` DELE, que nossas páginas não usam — todas
declaram `@layout BaseLayout` nosso). A duplicação vem do ciclo de render do Blazor interativo: o layout
renderiza 2× por page load (prerender estático + attach do circuito interativo) — um `<script src>`
plano dentro do layout executa nas duas passadas.

**Tentativa de fix REVERTIDA (re-teste no Chrome, 2026-06-10):** o loader inline guardado por flag
(`window.customClassesMudJs`) falhou em runtime — o Blazor **não renderiza `<script>` com corpo** dentro
de componente (`Failed to execute 'appendChild' on 'Node': Invalid or unexpected token`), trocando um
erro cosmético por outro. **Resolução final:** revertido para o `<script src>` plano — exatamente o
padrão do Skills-Extended ([`BaseLayout.razor:19`](../../../Skills-Extended/modded/Server/Web/Layouts/BaseLayout.razor))
e do próprio host (`BaseMudBlazorLayout.razor:15`), que exibem o MESMO erro cosmético. A segunda
avaliação lança o SyntaxError mas não afeta o interop (a API já está carregada — diálogos, snackbars,
selects e virtualização validados funcionando no Chrome com o erro presente). Aceito como ruído do
padrão upstream; comentário no layout documenta.

---

## Verificado e OK (suspeitas investigadas e descartadas)

Pontos do escopo desta passada investigados a fundo que **não** geraram achado:

- **Path traversal via rota/fileName (foco 3):** a rota `/customclasses/classes/{FileName}[/edit]` usa o parâmetro APENAS para match (`string.Equals`, OrdinalIgnoreCase) contra a listagem real de disco (`ListClassFiles`); `Save`/`Delete` recebem `_resolvedFileName`/`Entry.FileName` (nomes vindos do próprio disco). Na borda do serviço, `TryResolveClassFile` rejeita qualquer coisa com separador (`fileName == Path.GetFileName(fileName)` cobre `/` e `\`, logo `..%2F...` decodificado falha) e exige extensão `.json|.jsonc`. `ProfilesUsingEdition` não recebe path do usuário (resolve `../../profiles` a partir do ModHelper).
- **`iconFile` malicioso (`"../x.png"`):** vira `src="/CustomClasses-Server/icons/../x.png"` — atributo Razor-encodado; a normalização de URL acontece no middleware de static files do framework, que não sai do root do `wwwroot`. Sem vazamento fora do diretório público.
- **Slug com nome reservado do Windows (`con`/`nul` → `con.jsonc`):** `Slugify("Çön")` produziria `con`. Testado empiricamente nesta máquina (Win11): criar `con.jsonc`/`nul.jsonc` funciona normalmente no NTFS — sem bug prático no fluxo Create/Duplicate.
- **Double-add de árvore (foco 1):** `PlaceTree` é o ÚNICO ponto que faz `dest.AddRange`; no equipado, os contents acumulam no `tree` local e `inv.Items.AddRange(tree)` roda uma vez; no stash, `dest` é `inv.Items` direto. Sem duplicação em nenhum dos três exemplos mentais (arma preset+ammo no stash; mochila com contents; 100k roubles → 1 stack, custo 100k via face value, capacity 1 célula).
- **`count > 1` composto:** `PlaceSpecTrees` chama `buildUnit()` por unidade — `ClonePresetTree`/`BuildManualTree` geram MongoIds NOVOS a cada chamada; sem ids duplicados entre unidades. `parentId`/`slotId` da raiz são re-escritos pelo `PlaceTree` na colocação (o `"main"` placeholder da árvore manual nunca vaza); filhos preservam os links internos re-mapeados.
- **`GetGrids` de template sem grids:** `PackSpecsIntoGrids` loga "contêiner sem grades — N pulados" e retorna; o `ItemSpecEditor` mostra o chip "no grids" quando há contents num root sem grids. Recursão de contents é limitada pela profundidade do próprio JSON (finita; editor capa edição em 6 níveis).
- **Paridade dos resolvers (`CatalogService.ResolveDefaultPreset/ResolvePremiumPreset/ResolveStashPreset` × `InventoryBuilder`):** lógica idêntica linha a linha (encyclopedia-default/first; most-kitted não-térmico; menor preset com óptica real), mesma fonte (`GetGlobals().ItemPresets`) e mesma ordem de iteração — custo/capacity resolvem o MESMO preset que o builder.
- **Roubles/stack no trio builder×custo×capacity:** builder split por `StackMaxSize` (1 placement p/ 100k ₽), custo `qty=count × face value 1 ₽`, capacity 1 célula `sw×sh` — consistentes.
- **`ClassEditModel` round-trip (foco 2):** skills vazias → `ToDict` null → omitido; outfit só usec → `BuildOutfit` preserva e `Bear` null; multiplier `0` round-tripa (clamp ≥0 só no registrar); hideout todo removido → null; spec com `preset`+`tpl`+`mods` simultâneos preserva tudo (UI esconde, não apaga). Estado de página não vaza entre navegações: `OnParametersSet` → `LoadFromDisk` reconstrói `_model` e zera `_savedOnce`/`_saveDiagnostics`; rows novos = `@key` novos = instâncias novas de `ItemSpecEditor` (o `_presetMode` de `OnInitialized` nunca vê um `Spec` trocado por baixo, pois todos os usos são keyed).
- **`ItemSpecEditor` (foco 2):** `SwitchModeAsync` limpa os campos do modo abandonado (tpl/mods ↔ preset/premium); `PickRootAsync` reseta ammo/loadedMag/chambered na troca de root (sem ammo órfão de calibre errado); `RemoveModAsync` remove nó+subárvore (o nó carrega seus `Mods`); órfãos de slotId renderizados com `.ToList()` (sem mutação durante render); `NotifyChangedAsync` → `Refresh()` + `OnChanged` borbulha até `RecomputeLoadoutCost` (EventCallback re-renderiza o pai automaticamente — sem `StateHasChanged` faltando).
- **MudBlazor bindings:** `@bind-Value` + `@bind-Value:after` nos numéricos/switches (two-way correto); `Value`+`ValueChanged` manual apenas onde há lógica no set (dialogs de nome, `BaseEditionSelect`/`IconSelect` com sentinela "") — sem binding one-way acidental.
- **Diálogos 027 (cancel/validação):** validação de nome live contra snapshot + re-validação autoritativa dentro de `Create`/`Duplicate` (TOCTOU coberto); `Cancel()` não escreve nada; `DeleteAsync` com arquivo já removido retorna erro amigável; `DisableAsync` recarrega do disco antes de flipar `enabled` (não salva estado stale da lista).
- **`Classes.razor`/`ClassDetail.razor`:** resolução de FileName extension-less idêntica nos dois; classe inválida (parse error) renderiza linha/página com chip "Invalid" sem NRE (`def is null` guardado em todos os acessos); pós-delete navega para a lista; pós-disable `Reload()+StateHasChanged()`.
- **Pickers:** `PresetPicker`/`AmmoPicker`/`CustomizationPicker` são síncronos com cache por chave de parâmetro (`_loadedTpl`/`_loadedKey`) — sem Task.Run/dispose pendente; `MongoId.IsValidMongoId` guarda tpl meio-digitado. `ItemPicker` dialog: double-submit impossível (seleção fecha o diálogo); buscas concorrentes não-vazias são corretamente descartadas pelo `_searchVersion` (só o caminho clear falha — CR2-EP-08).
- **Guard do `compile-mod.sh` (arrays/quoting/edges do prompt):** expansões de array vazio todas no padrão `${arr[@]+"${arr[@]}"}` ou guardadas por count (seguro com `set -u` em bash < 4.4); `while IFS= read -r` + aspas em todos os paths (espaços OK); `classes/` ausente no install = fresh copy (correto NO compile-mod — o gap é só no sync, CR2-EP-03); `.bak1`/`_audit.log` invisíveis aos globs `*.json/*.jsonc` (confirmado no install real: `teste-ui.jsonc.bak1` e `_audit.log` ignorados); `diff | head || true` seguro sob `pipefail`.
- **`nameColor` cru em `style=` (ClassDetail/Classes):** sem validação hex (diferente do ClassEdit), mas valor é atributo-encodado pelo Razor — não há breakout de atributo nem execução; pior caso é CSS extra na própria página local. Risco aceito (single-user) — não reportado como achado.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-10 | Claude (code review) | Criação — 2ª passada pós-fixes (0 BLOQ, 2 MAIOR, 9 MENOR): paridade builder×custo do código novo do CR-EP-01, escopo da propagação de deleção dos scripts, e profundidade na camada Web/Razor + segurança HTTP local. |
| 2026-06-10 | Claude (apply review) | Fixes aplicados: CR2-EP-01/02/03/04/06/07/08/09/10/11 (CR2-EP-03 com modificação: abort em vez de skip); CR2-EP-05 aceito como dívida documentada no doc-comment do CostService. Seção "Achados do teste de UI" adicionada (UI-01 MudForm gate no Save, UI-02 nomes de customization via locale `"{id} Name"`, UI-03 carga única do MudBlazor.min.js). Validações: build Release 0 err/0 warn, `bash -n` OK, teste `_fake.jsonc` em `classes/_docs/` (compile não bloqueia, sync não propõe deleção), aborts do sync com path errado/sem `classes/`, locale validada com 3 ids do suits-catalog. |
