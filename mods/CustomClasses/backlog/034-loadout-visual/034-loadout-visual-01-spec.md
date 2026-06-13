# 034 — Loadout visual (gear slots + stash com ícones e tooltip)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-12

## Visão geral

O detalhe de uma classe (`ClassDetail`, item 033) mostra o equipado e o stash como **texto puro** (linhas com nome, chip de qty, tabela de preços). O viewer antigo mostrava o mesmo conteúdo como **slots visuais estilo Tarkov** (ícone do item dimensionado pelo tamanho real, agrupado em linhas de slot) e o stash como **grid de ícones agrupado por categoria** com badge de quantidade e subtotal ₽ por grupo — tudo legível "de relance", com **tooltip de hover** (nome, tamanho, preço, qty) sem nenhum clique. Este item porta essa UX visual para a coluna direita do dashboard e enriquece a aba Stash do editor com agrupamento por categoria e filtro por nome, fechando a meta single-screen herdada do 033.

A coluna direita do `ClassDetail` foi deixada pelo 033 com **dois pontos de extensão explícitos** (comentários `EXTENSION POINT 034` em `ClassDetail.razor:218` e `:238`) e contratos de dados congelados — este item só preenche esses pontos, sem tocar o header (território do 036) nem a coluna esquerda.

## Comportamento atual

- **`ClassDetail` coluna direita — Equipado:** uma `<div class="cc-equip-slot">` por slot (label textual do slot + `ClassViewItemSpec` recursivo). Sem ícones, sem layout de grid. Ordem = ordem de iteração do dicionário `Equipped`.
- **`ClassDetail` coluna direita — Stash:** `MudTable` densa com colunas Item / Tpl / Qty / Unit ₽ / Price source / Subtotal ₽, alimentada por `_stashLines` (linhas `Context=="stash"` do `LoadoutCostBreakdown`). Warnings de loadout acima da tabela. Sem ícones, sem agrupamento.
- **`ClassEdit` aba Stash:** `_model.Stash` renderizado como **~N cards em ordem do JSON** (um `MudPaper` por linha, com `ItemSpecEditor`). Sem filtro, sem agrupamento — achar um item em 27 linhas é scroll cego (kickoff §review #12).
- **`ClassEdit` aba Equipped:** um card por slot com `ItemSpecEditor` (mantém o `<img>` 28×28 tarkov.dev que o `ItemSpecEditor` já usa). Não muda neste item.
- **Ícones tarkov.dev** já são usados em `ItemPicker.razor:69` e `ItemSpecEditor.razor:43` (`https://assets.tarkov.dev/{tpl}-icon.webp`, `onerror` esconde a img). Decisão herdada do 023.
- **`CatalogService`** expõe nome (`GetItemName`), preço (`GetPrice`), categoria por busca (`Search` retorna `CategoryId`) e nome de categoria (`GetCategories`), mas **não expõe**: (a) dimensões do item (`_props.Width/Height`), (b) resolução direta tpl → categoria id/nome fora do fluxo de `Search`. Esses dados são necessários para dimensionar o ícone e agrupar o stash.

## Comportamento desejado

### GearPanel (equipado visual)
- Substitui o conteúdo textual do bloco `#cc-equipped` por um painel de **slots em grid estilo Tarkov**: cada slot mostra o **label do slot**, o **ícone do item** (tarkov.dev, dimensionado pelo tamanho real do item em células `Width×Height`) e o **nome curto truncado**.
- O ícone preenche uma célula proporcional: a base é uma unidade de célula fixa (ex.: 36px), e o item ocupa `Width` × `Height` unidades (um item 2×1 fica com o dobro da largura). Dimensões vêm do `CatalogService` (método novo — ver spec técnica).
- **Slots vazios / não presentes** aparecem esmaecidos (placeholder tracejado) ou simplesmente não são renderizados — decisão: renderizar **apenas os slots presentes** no `Equipped` (o viewer tinha layout fixo de 2 linhas; aqui o `Equipped` é um dicionário esparso, então só os slots configurados aparecem — slots vazios não fazem sentido numa classe que só declara o que muda). Premissa registrada abaixo.
- Cada ícone tem **tooltip de hover** (`ItemTooltip`): nome completo, tamanho em células, preço flea unitário, qty (quando > 1).
- **Presets / armas:** quando o `ItemSpec` é um preset (ou tpl de arma), o ícone é o do item-raiz resolvido (mesma resolução do `ClassViewItemSpec`: preset > tpl). O badge de preset/premium/qty do `ClassViewItemSpec` não é reproduzido no ícone — fica no tooltip (nome do preset + nº de partes, quando houver).
- **Offline / tpl modado sem ícone:** o `<img>` se esconde (`onerror`) e sobra a **célula com o nome curto** (degradação textual já aceita no 023). O tooltip continua funcionando (dados vêm do servidor, não do CDN).

### StashPanel (stash visual agrupado)
- Substitui a `MudTable` do bloco `#cc-stash` por um **grid de ícones agrupado por categoria do handbook** (Weapons / Armor / Mags / Ammo / Meds / …). Cada item: ícone proporcional (`Width×Height`), **badge de quantidade** (canto, quando qty > 1), tooltip de hover.
- Cada grupo tem um **header com o nome da categoria + subtotal ₽** do grupo.
- Agrupamento usa a categoria do handbook resolvida por tpl (`CatalogService` — método novo). Itens sem categoria caem num grupo **"Other"** (nunca somem).
- Warnings de loadout (`_loadoutCost.Warnings`) e o aviso "stash existe mas nenhuma linha precificada" continuam aparecendo acima do grid (comportamento do 033 preservado).
- A fonte de dados continua sendo `_stashLines` (contrato 033 congelado: `List<LoadoutCostEntry>`, já filtrado `Context=="stash"`). O painel não recalcula custo nem reconsulta o builder — só agrupa/renderiza o que recebe.
- **Offline:** ícones somem, sobra o nome + qty + subtotal por grupo (texto). Layout não quebra.

### ItemTooltip (hover, 0 cliques)
- Componente compartilhado de tooltip exibido no hover de qualquer célula de ícone (gear e stash). Conteúdo: **nome**, **categoria**, **tamanho em células** (`W×H`), **preço flea unitário** (com fonte/⚠ quando ausente), **qty** (quando aplicável).
- Implementado como `MudTooltip` envolvendo a célula (não bloqueia, sem clique). Reuso nos pickers (023) é **oportunista** — só se sair barato; não é requisito.

### Aba Stash do editor (agrupamento + filtro)
- Os cards de linha de stash passam a ser **agrupados por categoria do handbook** (mesma taxonomia do `StashPanel`), com **headers de grupo** (nome da categoria + contagem de linhas). Decisão: headers **não colapsáveis** na v1 (o kickoff sugere "colapsáveis" mas isso adiciona estado por-grupo; agrupamento + filtro já resolvem o scroll cego — colapsar fica como melhoria futura). Premissa registrada.
- **Campo de filtro por nome** no topo da aba: digitar filtra os cards exibidos por nome/shortname/tpl do item-raiz de cada linha (case-insensitive, substring). Grupos sem itens visíveis somem enquanto o filtro está ativo.
- O **"Add item"** continua igual (abre o picker, adiciona ao fim de `_model.Stash`). A ordem subjacente de `_model.Stash` **não muda** — o agrupamento é só de exibição (não reordena o modelo persistido).
- A edição de cada linha continua via `ItemSpecEditor` com `AllowCount=true` e `OnChanged="ScheduleRecompute"` — sem redesign do editor (kickoff: "os forms ficam").
- O filtro/agrupamento é **puramente client-side sobre o modelo já carregado** — não dispara recompute nem revalidação (respeita o 037: digitar no filtro não recalcula custo).

### Estilos
- Novas classes de gear/stash/tooltip são **adicionadas** ao `customclasses.css` existente (do 033). O arquivo **não é reescrito** — as classes de densidade/layout do 033 (`cc-dash`, `cc-section`, `cc-equip-slot`, etc.) permanecem intactas. As novas classes usam prefixo `cc-` (ex.: `cc-gear-*`, `cc-stash-*`).

## Critérios de aceite

- [ ] No `ClassDetail`, o bloco Equipado mostra cada slot configurado como **ícone dimensionado** (`Width×Height`) + label do slot + nome curto; o bloco Stash mostra **ícones agrupados por categoria** com badge de qty e subtotal ₽ por grupo.
- [ ] Hover em qualquer ícone (gear ou stash) abre tooltip com nome, categoria, tamanho em células, preço flea e qty — **sem clique**.
- [ ] Sem internet (ícones 404), as células degradam para **nome curto + qty** sem quebrar o layout; o tooltip continua funcionando.
- [ ] A coluna esquerda do `ClassDetail` (skills/hideout/outfit), o header (036) e os contratos de dados dos pontos de extensão (`def.Loadout?.Equipped`, `_stashLines`) ficam **inalterados** — o diff toca só o conteúdo dos blocos `#cc-equipped` e `#cc-stash`.
- [ ] A aba Stash do `ClassEdit` agrupa os cards por categoria do handbook e tem um campo de filtro por nome no topo; filtrar/agrupar **não reordena** `_model.Stash` nem dispara recompute de custo.
- [ ] O total de custo do loadout (badge do header) e os subtotais por grupo do stash **batem** com os valores que a `MudTable` do 033 mostrava para a mesma classe (mesma fonte `_stashLines`/`_loadoutCost`).
- [ ] `customclasses.css` ganhou só classes novas (gear/stash/tooltip); nenhuma classe do 033 foi removida ou redefinida.
- [ ] A meta single-screen do 033 (classe completa em 1080p com ≤1 scroll) continua válida com os painéis visuais.

## Corner cases

- [ ] **Item sem dimensões resolvíveis** (tpl modado/malformado): a célula usa tamanho **1×1 default** e não quebra; o tooltip mostra "—" no tamanho.
- [ ] **ItemSpec de preset:** o ícone e o tamanho são os do **item-raiz** resolvido (preset > tpl, igual ao `ClassViewItemSpec`); preset não resolvível usa o tpl/nome cru e cai no comportamento de "sem ícone".
- [ ] **Linha de stash sem categoria** (tpl fora do handbook): cai no grupo **"Other"** — nunca some do painel.
- [ ] **Equipado vazio / Stash vazio:** mantém as mensagens "No equipped items." / "No loose stash items." do 033 (não renderiza grid vazio).
- [ ] **Stash com linha sem preço** (`MissingPrice`): a célula mostra o badge ⚠ (no tooltip e/ou na célula) e conta 0 no subtotal — igual ao 033, nunca silencioso.
- [ ] **Filtro do editor sem resultados:** mostra uma mensagem "Nenhum item corresponde ao filtro" em vez de aba vazia; limpar o filtro restaura todos os grupos.
- [ ] **Categoria do handbook com nome só em en (sem pt):** usa o fallback en→id que `GetCategories` já faz; nunca mostra id cru se houver nome en.
- [ ] **Tooltip em item com qty grande / nome longo:** o tooltip não trunca (mostra nome completo); a célula trunca o nome curto com ellipsis.
- [ ] **Preset/arma como linha de stash:** o `LoadoutCostEntry` já vem expandido por tpl pelo CostService — o `StashPanel` agrupa por categoria do tpl de cada entry (não re-expande presets).

## Fora de escopo

- [ ] Redesign do editor de slots/linhas (`ItemSpecEditor`) — fica como está; só ganha agrupamento/filtro ao redor (kickoff).
- [ ] Header do `ClassDetail` (ícone da classe, nome colorido, badges) — território do **036**.
- [ ] Coluna esquerda do dashboard (skills/hideout/outfit) — fora do território.
- [ ] Layout fixo de 2 linhas de slot estilo viewer (HEADWEAR|ARMOR|RIG|BACKPACK / ON BACK|HOLSTER|SHEATH) — o `Equipped` é um dicionário esparso; renderiza só os slots presentes (ver premissa).
- [ ] Headers de grupo colapsáveis no editor — agrupamento + filtro já resolvem o scroll cego; colapsar fica para depois (ver premissa).
- [ ] Drag-and-drop ou edição visual no grid — read-only no detalhe; edição continua via forms.
- [ ] Cache/índice novo no CatalogService além de expor dimensão/categoria — performance é território do 037 (os getters novos reusam os índices existentes ou são consultas baratas por tpl).

## Premissas autônomas (usuário ausente)

- **PA-034-01 — Slots vazios:** o `Equipped` é um dicionário esparso (classe só declara o que sobrescreve). Renderiza **apenas os slots presentes**, não um grid fixo de 7 slots com vazios esmaecidos. Mais fiel ao modelo de dados e evita decidir uma taxonomia de slots fixa. O CSS prevê uma classe de slot vazio (`cc-gear-slot--empty`) caso uma iteração futura queira o grid fixo, mas não é usada na v1.
- **PA-034-02 — Headers não colapsáveis no editor:** agrupamento + filtro por nome resolvem o problema de "scroll cego" do review #12 sem introduzir estado de colapso por grupo (que complica o `@key`/recompute). Colapsar é melhoria futura.
- **PA-034-03 — Tooltip via MudTooltip:** reusa o componente do MudBlazor (já usado no 033/`ClassViewItemSpec`) em vez de portar o popover custom JS do viewer (`profiles.js:340-380`) — menos superfície, sem JS interop, comportamento de posicionamento já resolvido pela lib.
- **PA-034-04 — Dimensão e categoria via novos getters no CatalogService:** `GetItemDimensions(tpl)` (lê `_props.Width/Height`, default 1×1) e `GetCategoryName(tpl)` / resolução tpl→categoria reusam `_handbookIndex` e `GetCategories` já existentes (037). Sem novo índice; consultas O(1)/O(cats) por tpl, baratas no render read-only.
- **PA-034-05 — Reuso oportunista do tooltip nos pickers:** não será feito nesta entrega salvo se trivial; não é critério de aceite.

## Referências

- Kickoff: [034-loadout-visual-00-kickoff.md](./034-loadout-visual-00-kickoff.md)
- Dashboard base + pontos de extensão: `Web/Pages/ClassDetail.razor:216-280`
- Catálogo: `CatalogService.cs` (nome/preço/categoria; dimensão a expor)
- Viewer (port de layout): `tools/tarkov-itemdb/viewer/profiles.css:256-495`, `profiles.js:243-320`

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec funcional criada via `/create-spec` (autônoma, a partir do kickoff de 2026-06-10) |
