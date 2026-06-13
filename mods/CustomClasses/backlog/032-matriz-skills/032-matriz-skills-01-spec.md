# 032 — Matriz de skills (classes × skills, heatmap) — Spec

**Mod:** CustomClasses
**Status:** Especificado
**Criado:** 2026-06-12
**Origem:** [032-matriz-skills-00-kickoff.md](./032-matriz-skills-00-kickoff.md)

## Visão geral

Uma página nova `/customclasses/skills` (`SkillsMatrix.razor`) traz de volta a única visão **comparativa entre TODAS as classes de uma vez** que o viewer antigo (`profiles-skills.html`) tinha e que o editor atual não tem. É uma matriz **skills × classes**:

- **Linhas = skills na ORDEM CANÔNICA** (Physical → Mental → Combat → Practical → Special Elite), reusando `SkillMaster.Entries` do item 031 — a página **não redefine** ordem nem lista; cada categoria abre com um separador rotulado na cor da categoria.
- **Colunas = todas as classes**, com header **vertical/rotacionado** pintado na `nameColor` da classe (+ ícone). Classes **desabilitadas** entram como coluna **esmaecida**, controladas por um toggle "Mostrar desabilitadas" (default ligado, esmaecido) — comparação não pode ficar cega para o que está fora do ar.
- **Células = nível da skill** daquela classe, com fundo **heatmap por intensidade** (tier baixo/médio/alto + vazio quando nível 0), portando as faixas e cores de `profiles-skills.css:25-127`.

Extras que o viewer antigo não tinha, baratos com dados vivos: **rodapé com o custo de skill ponderado por classe** (reusando `CostService.ComputeSkillCost`), **toggle de multiplicadores de XP** (chip ±% por célula) e **célula clicável** que navega para o detalhe da classe. Objetivo de DoD: comparar as skills de todas as classes em **0 cliques** depois de abrir a página.

A fonte de dados é a **view de cache do item 037** (`ClassEditorService.GetCachedEntries()`) — a matriz **nunca** dispara dry-run por render. O custo de skill é derivado na UI com uma chamada de `CostService.ComputeSkillCost` por classe, **uma vez por navegação** (em `OnInitialized`), nunca dentro do loop de render — exatamente o padrão já consolidado no `NavMenu.razor` (sidebar 030) e em `Classes.razor`.

## Comportamento atual

- Não existe nenhuma visão comparativa entre classes no editor. A sidebar (030) lista classes com custo total e dot de status; o detalhe (ClassDetail) e a edição (ClassEdit) usam a lista canônica de skills (031), mas **uma classe por vez**.
- Para comparar duas classes, o usuário precisa abrir cada uma, memorizar níveis e cruzar de cabeça — impossível além de 2–3 skills.
- O viewer antigo `profiles-skills.html` fazia exatamente isso (matriz heatmap), mas é estático, baseado num JSON dumpado (`profiles-meta.json`) e está fora do editor.

## Comportamento desejado

- **Matriz completa numa tela:** linhas = todas as skills canônicas do 031 (com separadores de categoria coloridos), colunas = todas as classes. Header de coluna vertical com o nome na `nameColor` e o ícone da classe.
- **Heatmap por tier:** célula com nível > 0 recebe fundo por faixa de intensidade — porta de `profiles-skills.js:64` (`lvl <= 3 ? low : lvl <= 6 ? mid : high`) e cores de `profiles-skills.css:99-121`. Nível 0 → célula vazia (transparente). O número do nível aparece dentro da célula colorida.
- **Classes desabilitadas:** entram como coluna esmaecida (opacidade reduzida no header e nas células) e somem quando o toggle "Mostrar desabilitadas" é desligado. Default: ligado, esmaecido.
- **Rodapé de custo por classe:** uma linha final mostra o **custo de skill ponderado total** por classe (`SkillCostBreakdown.Total`), com destaque visual quando dentro do budget (`WithinBudget` — faixa 28–32). Classes com custo 0 / sem definição parseável mostram "—" (neutro, não "fora do budget").
- **Toggle de multiplicadores de XP:** quando ligado, cada célula ganha um chip ±% derivado de `def.SkillMultipliers` (fator 1 → sem chip), reusando a mesma convenção do chip de `SkillCanonicalList.razor:374-393`.
- **Célula clicável → detalhe da classe:** clicar em qualquer célula (ou no header da coluna) navega para `/customclasses/classes/{bareName}` (detalhe read-only), permitindo agir no que se viu em 1 clique.
- **Hover na linha** destaca a skill inteira (porta de `profiles-skills.css:76-78`).
- **Skills fora da canônica:** se alguma classe define uma skill que não está em `SkillMaster.Entries` (enum morto / desconhecido), ela entra numa seção de overflow no fim das linhas (espelhando a seção "Outside canonical" do componente 031), nunca é descartada silenciosamente.

## Premissas registradas (decisões autônomas)

- **P1 — Fonte de dados:** classes vêm de `ClassEditorService.GetCachedEntries()` (view de cache 037), projetadas **uma vez** em `OnInitialized`. Nada de dry-run por render; nada de API nova no servidor. A página é puramente de leitura.
- **P2 — Custo:** apenas o **custo de skill** (`ComputeSkillCost`) entra no rodapé. O **custo de loadout** (`ComputeLoadoutCost`) e o **dry-run de stash** (`CheckStashCapacity`) ficam **fora** — são caros (rebuild de loadout / packing) e não pertencem a uma comparação de skills. Decisão alinhada ao kickoff ("a matriz NÃO pode disparar dry-runs").
- **P3 — Ordem das linhas:** 100% delegada a `SkillMaster.Entries`. A página não tem nenhuma lista de skills própria — se o 031 mudar a ordem, a matriz acompanha.
- **P4 — Multiplicadores no custo:** o toggle de multiplicadores afeta **apenas o display** (chip ±%); ele **não** altera o custo de skill no rodapé (multiplicador de XP nunca entrou no modelo de custo — decisão de produto registrada em `CostService.cs:88`). Premissa explícita para evitar confusão de UX.
- **P5 — Ordem das colunas:** mesma ordem determinística da sidebar (030) e da lista (024): `ListClassFiles` ordena por nome de arquivo (`ClassEditorService.cs:128`). A matriz herda essa ordem; classes desabilitadas **não** são reordenadas (ficam na posição natural, só esmaecidas) para preservar a posição visual entre toggles.
- **P6 — Identificação de "desabilitada":** usa `ClassFileEntry.Enabled` (não `Registered`). Uma classe com erro de parse (`Definition == null`) é tratada como coluna sem dados — header com nome do arquivo, todas as células vazias, custo "—".
- **P7 — Rota da célula clicável:** navega para o **detalhe** (não a edição), espelhando o destino default da sidebar (030) quando não se está editando. O segmento da rota é o nome de arquivo sem extensão (convenção 024).
- **P8 — Layout vs. MudBlazor:** o heatmap é uma `<table>` com classes CSS scoped portadas do viewer, dentro de uma página com `@layout BaseLayout` (mesma do resto do editor). Não se força tudo em `MudTable` — a matriz é densa e rotacionada, e o controle fino de CSS do viewer é mais fiel que adaptar um `MudTable`. Os toggles usam `MudSwitch`/`MudSwitch<bool>` para consistência visual com o restante do editor.

## Critérios de aceite

- [ ] **CA1 — Cobertura de linhas:** a matriz renderiza exatamente as linhas de `SkillMaster.Entries` (mesma ordem e contagem), com separadores de categoria coloridos; verificável comparando a contagem de linhas de skill com `SkillMaster.Entries.Count` (sem hardcode na página).
- [ ] **CA2 — Cobertura de colunas:** com o toggle "Mostrar desabilitadas" ligado, há uma coluna por arquivo de classe em `GetCachedEntries()`; desligar o toggle remove exatamente as colunas com `Enabled == false` e mantém a ordem das demais.
- [ ] **CA3 — Heatmap correto:** uma célula com nível N>0 recebe a faixa `low` (1–3), `mid` (4–6) ou `high` (7–10) conforme `profiles-skills.js:64`; nível 0 → célula vazia/transparente; o número N aparece na célula. Verificável visualmente contra uma classe de níveis conhecidos.
- [ ] **CA4 — Rodapé de custo:** o rodapé mostra, por coluna, `ComputeSkillCost(def).Total` arredondado; classes dentro do budget (`WithinBudget`) têm destaque distinto das fora; classes sem definição/custo 0 mostram "—". Os valores batem com os exibidos na sidebar (030) para as mesmas classes.
- [ ] **CA5 — Navegação 1 clique:** clicar numa célula (ou header de coluna) leva a `/customclasses/classes/{bareName}` da classe daquela coluna.
- [ ] **CA6 — Toggle de multiplicadores:** com o toggle ligado, células de skills cujo `def.SkillMultipliers` tem fator ≠ 1 exibem chip ±% com o sinal/percentual corretos; fator 1 ou ausente → sem chip; desligar o toggle remove todos os chips. O custo do rodapé **não muda** com o toggle (P4).
- [ ] **CA7 — Zero dry-run:** abrir a página e alternar qualquer toggle não dispara nenhum `ValidateAndBuild`/`ComputeLoadoutCost`/`CheckStashCapacity`; o custo de skill é computado uma vez por navegação. Verificável pelo log de perf do 037 (`ListClassFiles: N hot / 0 cold`) e ausência de logs de builder.
- [ ] **CA8 — Link no sidebar:** o `NavMenu.razor` (030) ganha um link "Skills matrix" para `/customclasses/skills`, sem alterar o restante da sidebar.

## Corner cases

1. **Nenhuma classe / nenhum arquivo:** `GetCachedEntries()` vazio → a página mostra uma mensagem "No class files found" (alinhada ao vazio da sidebar/lista) em vez de uma tabela só com a coluna de skills.
2. **Classe sem nenhuma skill (ex.: Peladão, item 016):** coluna inteira de células vazias; custo de rodapé 0 → "—" (não conta como "fora do budget"). A coluna ainda aparece (header com nome/ícone).
3. **Classe com erro de parse (`Definition == null`):** coluna presente com header do nome de arquivo, todas as células vazias e custo "—" (P6); nunca quebra a matriz nem some silenciosamente.
4. **Skill fora da canônica** (enum morto/desconhecido definido por alguma classe): vai para a seção de overflow no fim das linhas (espelha "Outside canonical" do 031), nunca é descartada; o nível dessa classe aparece, as demais classes ficam vazias nessa linha.
5. **`nameColor` ausente ou inválida:** header da coluna usa a cor de texto default (sem `color:` inline), igual ao tratamento da sidebar (`NavMenu.razor:287-288`).
6. **Nível acima de 10** (skills podem ir até 51, ver `MudNumericField Max="51"` em 031): satura no tier `high` (a faixa `> 6` cobre tudo acima); o número exibido é o valor real (ex.: "42"), não clampado.
7. **Muitas classes (overflow horizontal):** a matriz rola horizontalmente (`overflow-x:auto`, porta de `profiles-skills.css:14-16/26-28`); a coluna de nomes de skill permanece legível à esquerda (não precisa ficar sticky neste item — ver fora de escopo).
8. **Multiplicador definido para skill com nível 0:** com o toggle ligado, o chip ±% aparece mesmo na célula vazia daquela classe (o multiplicador é independente do nível inicial) — comportamento consistente com o chip do 031, que deriva do fator e não do nível.

## Fora de escopo

- Edição inline na matriz (a matriz é read-only; editar é 1 clique até o detalhe/edição).
- Coluna de skills **sticky**/congelada no scroll horizontal (melhoria de densidade — fica para o item 035).
- Comparação A×B dedicada com deltas (é o item 036; a matriz já cobre comparação N-classes por leitura).
- Custo de loadout e dry-run de stash no rodapé (P2 — caros, não pertencem à visão de skills).
- Filtro/busca de skills ou classes na matriz, ordenação por coluna, exportação (não pedidos; densidade fica no 035).
- Qualquer API nova no servidor — a página consome apenas serviços existentes (037 cache + CostService).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Criação da spec funcional (visão, comportamento atual/desejado, 8 premissas, 8 critérios, 8 corner cases, fora de escopo). |
