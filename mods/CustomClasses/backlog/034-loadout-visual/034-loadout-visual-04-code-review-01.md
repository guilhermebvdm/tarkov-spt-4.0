# 034 — Loadout visual — Code review 01

**Mod:** CustomClasses
**Item:** 034 (loadout visual — gear slots + stash com ícones e tooltip)
**Revisor:** Claude (autônomo, usuário ausente)
**Data:** 2026-06-12
**Build após fixes:** ✅ `dotnet build … -c Release --no-incremental` → 0 erros, 0 avisos

## Escopo revisado

Diff/arquivos do território 034:

- `Web/Shared/GearPanel.razor` (novo)
- `Web/Shared/StashPanel.razor` (novo)
- `Web/Shared/ItemTooltip.razor` (novo)
- `Web/Pages/ClassDetail.razor` (modificado — preenche os pontos de extensão 034)
- `Web/Pages/ClassEdit.razor` (modificado — filtro + agrupamento da aba Stash)
- `Web/wwwroot/css/customclasses.css` (modificado — só classes novas `cc-gear-*`/`cc-stash-*`/`cc-item-*`)
- `CatalogService.cs` (getters novos: `GetItemDimensions`, `GetCategoryId`, `GetCategoryName`)

## Veredito

Implementação sólida e fiel à spec: contratos de dados congelados do 033 (`def.Loadout?.Equipped`,
`_stashLines`/`LoadoutCostEntry`) preservados; getters do CatalogService reusam o `_handbookIndex` lazy
do 037 (sem índice novo); resolução de root tpl (preset > tpl) espelha `ClassViewItemSpec`; degradação
offline (texto sob ícone com `onerror`) consistente. Nenhum crash/null-deref encontrado — todos os
caminhos de tpl passam por `MongoId.IsValidMongoId` antes de `new MongoId(...)`.

Os achados foram dois avisos do compilador (build warnings) com fix local e inequívoco — aplicados. O
resto é design/UX ou ambíguo — adiado.

## Aplicados (seguros)

### CR-01-01 — `MongoId?` ternário com `null` cru → CS8625 (build warning) ✅ Aplicado
**Arquivo:** `Web/Shared/GearPanel.razor:109-110`
`TryMongoId` retornava `MongoId.IsValidMongoId(raw) ? new MongoId(raw) : null;`. Como `MongoId` é value
type, o arm `null` dispara `CS8625` (literal nulo em tipo não anulável — o compilador infere o branch
como `MongoId`, não `MongoId?`). O próprio `CatalogService.TryParseMongoId` (linha 833) já resolve isso
com `: (MongoId?)null`. Fix: castar o arm para `(MongoId?)null`, alinhando ao padrão da base. Sem
mudança de comportamento — só elimina o aviso e torna a intenção explícita.

### CR-01-02 — `Dense="true"` ilegal em `MudTextField` → MUD0002 (analyzer warning) ✅ Aplicado
**Arquivo:** `Web/Pages/ClassEdit.razor:455-459`
O campo de filtro tinha `Margin="Margin.Dense" Dense="true"`. `MudTextField` não expõe `Dense` (o knob de
densidade é `Margin`), e o analyzer MudBlazor sinaliza o atributo ilegal (MUD0002). Era no-op silencioso.
Fix: remover `Dense="true"`, mantendo `Margin="Margin.Dense"` (densidade real). Sem mudança visual.

## Adiados

### CR-01-D1 — Aba Stash do editor sem mensagem de "filtro sem resultados"
**Arquivos:** `Web/Pages/ClassEdit.razor:461` (`@foreach (var group in BuildStashGroups())`)
**Por quê adiar:** A spec (corner case, linha 72) exige "Nenhum item corresponde ao filtro" quando o
filtro não casa nada. Hoje `BuildStashGroups()` retorna lista vazia e o `@foreach` simplesmente não
renderiza nada (aba some, sem aviso). É fuga de spec real, mas o **fix carrega decisão de UX** (texto
exato da mensagem, idioma pt/en, posição) — não é inequívoco/local o suficiente para a política autônoma.
Adiado para apply explícito do dono do item. Fix sugerido: um `@if (!groups.Any() && filtro ativo)` com
`<MudText>` curto após o campo de filtro.

### CR-01-D2 — `MissingPriceBadge` órfão em `ClassDetail.razor`
**Arquivo:** `Web/Pages/ClassDetail.razor:456-461`
**Por quê adiar:** O `RenderFragment MissingPriceBadge(LoadoutCostEntry)` só era usado no `RowTemplate` da
`MudTable` removida; agora é método privado não referenciado (dead code). Não quebra build nem gera aviso
(C# não avisa método privado de instância não usado em parcial Razor). Remover é limpeza de fio solto,
mas toca uma região não estritamente do diff visual e o `StashPanel` poderia querer reusar o conceito de
badge — decisão de manutenção, não bug. Adiado como cleanup opcional.

### CR-01-D3 — Divergência de resolução de grupo entre `StashPanel` e `ClassEdit`
**Arquivos:** `Web/Shared/StashPanel.razor:66-74` vs `Web/Pages/ClassEdit.razor:907-922` (`BuildStashGroups`)
**Por quê adiar:** `StashPanel` agrupa por `line.Tpl` (tpl já expandido pelo CostService) via
`GetCategoryId` + mapa `GetCategories()` id→name; `ClassEdit` agrupa pelo **root tpl** da linha de modelo
(preset > tpl) via `GetCategoryName`. As taxonomias são a mesma (handbook), mas a entrada (tpl expandido
vs root do preset) difere por construção — uma linha de preset pode cair em grupos diferentes entre
detalhe e editor. É **comportamento esperado** (fontes de dados distintas: `LoadoutCostEntry` expandido no
detalhe; `ItemSpecModel` cru no editor) e a spec aceita ambos. Unificar exigiria decisão de produto sobre
qual taxonomia é "a verdade" — design, não bug. Adiado.

### CR-01-D4 — `OrderBy` de grupos por nome localizado (ordem instável entre locales)
**Arquivos:** `Web/Shared/StashPanel.razor:89`, `Web/Pages/ClassEdit.razor:920`
**Por quê adiar:** Grupos ordenados por `Name` (`OrdinalIgnoreCase`). Como `Name` é localizado, a ordem dos
grupos muda com o idioma e "Other" não tem posição fixa (cai alfabeticamente). É escolha de layout/ordenação
(design), não correção. Adiado.

## Notas (sem ação)

- **CatalogService getters (034):** `GetItemDimensions` (default 1×1, nunca lança, nunca zero),
  `GetCategoryId` (O(1) sobre `_handbookIndex`), `GetCategoryName` (single-tpl, doc avisa explicitamente
  que o `StashPanel` NÃO o usa per-line) — todos corretos e dentro do contrato do 037. Sem leak: nada de
  `IDisposable`, sem subscrição de evento, sem `Timer`.
- **StashPanel CR-034-03:** `GetCategories()` chamado uma vez em `OnParametersSet` p/ montar o mapa
  id→name; grouping per-line usa `GetCategoryId`. Respeita o 037 (sem rebuild per-line). Correto.
- **`ItemTooltip`:** `MudTooltip` por célula (PA-034-03), hover puro, sem JS interop. `Qty` é `double?` e só
  aparece `> 1`; gear nunca passa Qty (ok). Sem clique. Correto.
- **`StashFilterChanged` → `StateHasChanged()`:** display-only, nunca chama `ScheduleRecompute`
  (respeita PA-037-04 — digitar no filtro não recalcula custo). Correto.
- **CSS:** só classes novas adicionadas; classes do 033 (`cc-equip-slot*`) preservadas (comentário do
  cabeçalho atualizado, mas as regras não foram redefinidas/removidas). Critério de aceite atendido.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Code review 01 (autônomo). 2 fixes seguros aplicados (CR-01-01 CS8625, CR-01-02 MUD0002); build limpo. 4 achados adiados (UX/design/manutenção). |
