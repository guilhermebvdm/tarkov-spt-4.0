# 034 — Loadout visual · Auto-review da spec técnica (rev. 01)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-12
**Revisa:** [034-loadout-visual-02-spec-tech.md](./034-loadout-visual-02-spec-tech.md)

Auto-review adversarial da spec técnica contra o código real. Severidade: 🔴 bloqueador (resolvido no 02 antes de codar) · 🟡 atenção · 🟢 ok.

## 🔴 Bloqueadores (achados e resolvidos no 02)

### CR-034-01 — 🔴 Aba Stash do editor opera sobre `ItemSpecModel`, não `LoadoutCostEntry`
**Achado:** o 02 descreve o filtro/agrupamento da aba Stash "por nome/shortname/tpl do item-raiz". Mas `ClassEdit` edita `_model.Stash`, que é `List<ItemSpecModel>` (`ClassEditModel.cs` — `ItemSpecModel` tem `Tpl`/`Preset`/`Count`, **sem** `Name` nem `Subtotal`). O `StashPanel` read-only recebe `List<LoadoutCostEntry>` (já precificado, com `Name`). São **dois mundos de dados distintos** — a spec misturava. Se o agrupamento do editor tentasse usar `LoadoutCostEntry`, não compilaria (o editor não tem essa lista por linha do modelo).
**Risco:** implementação confusa, possível tentativa de cruzar `_stashCost.Items` com `_model.Stash` por índice (frágil — presets expandem em várias entries).
**Resolução (02):** a tabela MODIFICAR e o contrato deixam explícito: no **editor**, o filtro/agrupamento resolve nome+categoria **por `ItemSpecModel`** — resolve o tpl do item-raiz (preset > tpl, igual ao `ClassViewItemSpec`) e chama `Catalog.GetItemName(rootTpl)` + `Catalog.GetCategoryId(rootTpl)`. **Não** usa `LoadoutCostEntry`. O agrupamento é só de exibição sobre `_model.Stash`; a ordem do modelo não muda. (Adendo aplicado ao 02 — ver "Contrato dos componentes" e nota abaixo.)

### CR-034-02 — 🔴 `StashPanel` não recebe `_loadoutCost` → onde ficam os warnings?
**Achado:** o 033 renderiza `_loadoutCost.Warnings` e o alerta "stash existe mas nenhuma linha precificada" **dentro** do bloco `#cc-stash`, antes da tabela. O 02 passa só `Lines` (`_stashLines`) ao `StashPanel` — se o conteúdo todo do bloco for substituído por `<StashPanel>`, esses warnings somem (regressão do 033).
**Resolução (02):** o contrato do `StashPanel` (seção "Contrato dos componentes") fixa que os **warnings permanecem no `ClassDetail`**, renderizados acima do `<StashPanel Lines="@_stashLines"/>`, porque dependem de `_loadoutCost`/`def` que o painel não recebe. O painel só agrupa/desenha `Lines`. Critério de aceite do 01 ("warnings continuam aparecendo acima do grid") cobre isso.

### CR-034-03 — 🔴 `GetCategoryName` por linha regride o objetivo do 037
**Achado:** `GetCategories()` reconstrói a `List<CatalogCategory>` a cada chamada (varre `databaseService.GetHandbook().Categories` + locale). Chamar `GetCategoryName(tpl)` por linha de stash (N linhas) faz N reconstruções da lista inteira — exatamente o tipo de scan-por-render que o 037 eliminou.
**Resolução (02):** nota "Decisão de performance (037-aware)": o `StashPanel` chama `GetCategories()` **uma vez** no `OnParametersSet`, monta um `Dictionary<string,string>` id→nome local, e usa `GetCategoryId(tpl)` (O(1) sobre o `_handbookIndex` lazy) por linha. `GetCategoryName` fica só como conveniência single-tpl (GearPanel/ItemTooltip, poucas chamadas). Assinatura `GetCategoryId` adicionada.

## 🟡 Atenção (mitigado / registrado)

### CR-034-04 — 🟡 Tokens CSS do viewer não existem nesta app
**Achado:** o `profiles.css` referenciado usa `var(--accent)`, `var(--space-N)`, `var(--border-subtle)`, `--radius-md` — tokens do viewer standalone. O `customclasses.css` do 033 roda dentro do MudBlazor e **não define** esses tokens; copiá-los cegamente deixaria as classes sem cor/spacing.
**Resolução (02):** nota explícita no bloco CSS: usar `var(--mud-palette-*)` ou valores literais (como o 033 fez), não copiar `var(--space-*)`/`var(--accent)`. Port é de **layout**, não de tokens.

### CR-034-05 — 🟡 Nome curto na célula: visível sempre ou só no fallback?
**Achado:** o 01 diz "ícone + nome curto truncado" e "offline → sobra o nome". Ambíguo: o nome aparece sempre (sob/ao lado do ícone) ou só quando a img falha?
**Decisão (02):** o `cc-item-cell__name` é renderizado **sempre** (sob a célula, como label), com ellipsis. Quando a img carrega, o usuário vê ícone + label; offline, vê só o label. Isso evita JS para detectar o `onerror` e trocar layout — o label é estático e o `onerror` só esconde a `<img>` (padrão 023). Registrado no contrato do `GearPanel`/CSS. *(Sem mudança de comportamento offline — só remove a ambiguidade.)*

## 🟢 Verificado OK

- **Pontos de extensão do 033 existem e têm contrato congelado** (`ClassDetail.razor:218`, `:238`) — confirmado lendo o arquivo. `def.Loadout?.Equipped` é `Dictionary<string,ItemSpec>?`; `_stashLines` é `List<LoadoutCostEntry>`.
- **`_props.Width/Height`** existem como `int?` em `TemplateItem.Properties` (`TemplateItem.cs:123-128`) — `GetItemDimensions` é viável e o default 1×1 cobre o `null`.
- **`_handbookIndex`** (tpl→categoria id) já existe e é lazy/thread-safe (037) — `GetCategoryId`/`GetCategoryName` reusam sem novo índice.
- **`ResolveDefaultPreset/ResolvePremiumPreset` são `internal`** e os componentes estão no assembly `CustomClasses` — acesso ok (o `ClassViewItemSpec` já os usa).
- **Ícone tarkov.dev + `onerror`** é padrão estabelecido (`ItemPicker.razor:69`, `ItemSpecEditor.razor:43`) — degradação offline já provada.
- **Território respeitado:** o diff toca só os blocos `#cc-equipped`/`#cc-stash` do `ClassDetail`, a aba Stash do `ClassEdit`, os 3 componentes novos, os getters do `CatalogService` e o CSS (adições). Header (036) e coluna esquerda intocados.
- **037 não regride:** filtro do editor é client-side sobre `_model.Stash`, sem `ScheduleRecompute`; getters novos são read-only sem novo eager index.

## Resultado

5 achados (3 🔴, 2 🟡). **Todos resolvidos no 02** antes de codar. **0 bloqueadores abertos.**

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Auto-review criado via `/review-technical-spec` (autônomo); CR-034-01..03 (🔴) e 04..05 (🟡) resolvidos no 02 |
