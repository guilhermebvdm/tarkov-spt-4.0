# 025 — Edição de campos simples + outfit — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Refs:** [01-spec](./025-edit-campos-simples-01-spec.md) · [021 ClassEditorService] · [022 CostService/CatalogService] · [023 CustomizationPicker] · [024 ClassDetail]

## View-model: `ClassEditModel` (Web/ClassEditModel.cs)

`ClassDefinition` é record imutável (`init`-only) — o form NÃO edita o DTO. Decisão: classe mutável dedicada com conversão nas bordas:

- `FromDefinition(def)` — dicts viram listas de rows (`SkillLevelRow`, `SkillFactorRow`, `HideoutRow`) p/ add/remove/bind in-place; `LocalizedText` abre em 4 strings (en/pt × displayName/description); outfit abre em 4 strings (UsecUpper…BearLower); `Loadout` é **pass-through** (referência intacta — 026/028 são donos).
- `ToDefinition()` — reconstrói o record: strings vazias → `null` (omitidas na serialização — arquivo salvo fica próximo do manual); tabelas vazias → `null`; `Outfit`/`OutfitSide` só emitidos com algum valor; duplicata de chave (não acontece via UI) resolve por `TryAdd` (primeira vence).
- `LocalizedText` round-trip: o converter do item 021 já serializa "só En" como string legada e "com Pt" como objeto — nenhum tratamento extra aqui.

## Página: `Web/Pages/ClassEdit.razor`

- `@page "/customclasses/classes/{FileName}/edit"` — `FileName` SEM extensão (rota com `.jsonc` no último segmento pode virar static-file). Resolução idêntica ao ClassDetail: `ListClassFiles()` → match exato, depois match sem extensão. Guarda `_resolvedFileName` (COM extensão) como alvo do Save.
- Carrega via `ListClassFiles` (não `Load`) p/ mostrar os mesmos diagnostics de dry-run do viewer no topo. Parse error → diagnostics + sem form.
- **Shell:** `MudTabs` com 7 panels; Equipped/Stash são `MudAlert` placeholder (026/028) mostrando contagens preservadas.
- **Custo ao vivo (h):** `RecomputeSkillCost()` = `CostService.ComputeSkillCost(_model.ToDefinition())`, disparado por `@bind-Value:after` nos níveis e em add/remove de skill. Por-linha: lookup no breakdown (`SkillCostEntry` por nome ordinal). `ComputeLoadoutCost` roda 1× no load (loadout não editável aqui).
- **Selects com sentinela `""`:** `BaseEditionSelect`/`IconSelect` são propriedades adaptadoras ("" ⇄ null) — evita `MudSelectItem` com `Value=null`.
- **baseEdition:** `CatalogService.GetEditionKeys()` filtradas por `!ClassVisualRegistry.Contains(k)` (só vanilla/outros mods ficam; classes deste mod saem). Edge: `baseEdition` atual fora da lista é re-adicionado p/ não sumir do select.
- **iconFile:** enumeração via filesystem do INSTALL — `ModHelper.GetAbsolutePathToModFolder(typeof(ClassEditorService).Assembly)` + `wwwroot/icons/*.png` (mesma resolução de path que o ClassEditorService usa p/ `config/classes/`; robusto a onde o mod está instalado). Preview por `/CustomClasses-Server/icons/{file}` (mount estático do 020).
- **nameColor:** `MudTextField` + swatch + validação regex `^#[0-9a-fA-F]{6}$` (campo e re-checagem no Save — cor inválida bloqueia client-side com snackbar; vazio = válido/omitido).
- **Skills add:** `Enum.GetNames<SkillTypes>()` completo (sem filtrar skills "mortas" — decisão da spec) menos os já usados, alfabético. **Hideout add:** `Enum.GetNames<HideoutAreas>()` menos `NotSet` e usados (parse no HideoutBuilder usa o mesmo enum).
- **Outfit (f):** helper `OutfitSlot(title, side, slotKind, get, set)` (RenderFragment) — 4 cards com label resolvido (dict id→nome via `Catalog.GetClothing`, mesma fonte do ClassDetail), botão clear e `CustomizationPicker` embutido.

## Fluxo de save (g)

1. Re-valida `nameColor` client-side (única validação que o pipeline do server não cobre).
2. `Task.Run(() => EditorService.Save(_resolvedFileName, _model.ToDefinition(), hotApply: true))` — fora do thread do circuito Blazor (IO + dry-run).
3. Service: dry-run `ValidateAndBuild(allowReplace:true)` → Error aborta ANTES de escrever → `.bak1..3` rotativo → write JSON indentado → hot-apply (`Commit`; `enabled:false` → `Remove`) → audit.
4. Sucesso → snackbar verde + banner fixo com os limites do hot-apply; warnings do dry-run continuam visíveis. Falha → diagnostics `[Code] Message` em MudAlert; `_savedOnce=false`.
5. **Discard** → `LoadFromDisk()` (re-resolução + `FromDefinition`).

Caveat herdado do 021 (documentado no service): comentários JSONC são perdidos no re-serialize; `.bak1` preserva o último estado manual.

## Decisões de UI

- Toolbar **sticky** (Save/Discard + 2 totais) em vez de AppBar/footer — visível em qualquer aba sem mexer no BaseLayout (compartilhado).
- `name` disabled + `MudTooltip` (texto aponta item 027) — sem caminho de rename nesta página, nem escondido.
- Aviso de unsaved changes ao navegar: NÃO implementado (opcional no kickoff).
- Labels/textos da UI em inglês (consistente com Classes/ClassDetail); docs pt-BR.

## Arquivos

| Arquivo | Ação |
|---|---|
| `modded/Server/Web/ClassEditModel.cs` | NOVO — view-model mutável + rows + conversões |
| `modded/Server/Web/Pages/ClassEdit.razor` | NOVO — página de edição (shell de abas) |
| `modded/Server/Web/Pages/ClassDetail.razor` | EDITADO — botão "Edit" no header |
