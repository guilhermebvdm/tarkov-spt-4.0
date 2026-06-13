# 035 — Densidade global + redução de cliques · Spec Funcional

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-12
**Épico:** UX do editor (030–037) · **Wave:** UX-W4 (fechamento polish)

## Visão geral

Passada **final de polimento** sobre o editor web de classes já entregue nas waves 030–036. Não cria telas novas: aperta a **densidade visual** de todos os componentes MudBlazor, **corta cliques** das tarefas frequentes (editar, comparar skills entre classes, navegar da matriz pro edit) e **persiste preferências de UI** (estado do drawer, última vista/aba, ordenação da lista, toggles da matriz) em `localStorage` para que cada navegação não volte ao default.

Tudo aqui é UI: zero mudança em validação, custo, schema de classe ou comportamento de save/hot-apply. As páginas afetadas (`Classes.razor`, `ClassEdit.razor`, `SkillsMatrix.razor`, `ClassDetail.razor`, `NavMenu.razor`, `BaseLayout.razor`, pickers e diálogos) já existem e estão commitadas — este item **reutiliza e refina**, não duplica.

> **Premissa de escopo (PA-035-00):** a "passada de regressão visual Chrome MCP + re-medição dos tempos do 037" citada no kickoff §Escopo/§DoD **NÃO** é responsabilidade deste item. O orquestrador roda essa bateria na validação final, com o server real e screenshots de evidência. Este item entrega **código + docs**. As métricas de clique do DoD abaixo são alvos de design verificáveis na validação final, não medições produzidas aqui.

## Comportamento atual

- **Densidade:** a lista de classes (`Classes.razor`) já usa `MudTable Dense`, mas o **edit** (`ClassEdit.razor`) tem `MudTabs`/`MudGrid` com espaçamento default (airy) e campos com `Margin.Dense` apenas em parte dos `MudTextField`/`MudSelect`; os diálogos de lifecycle e os pickers misturam densidades. Resultado: poucas linhas por tela, scroll desnecessário.
- **Lista de classes:** colunas **não ordenáveis** (ordem fixa = ordem de arquivo); o ícone da classe **já renderiza** na primeira coluna (`Classes.razor:57-63` — o kickoff diz "hoje não renderiza", mas o código atual já o faz: registrar como premissa); a única ação rápida por linha é Duplicate/Delete — **não há "Edit" direto** (a edição exige abrir o detalhe e clicar Edit lá).
- **Edição:** trocar de classe pela sidebar (`NavMenu.NavigateToClass`) já preserva a **vista** (edit→edit), mas **reseta a aba ativa** para General — comparar a aba Skills entre duas classes custa 2 cliques extras por troca. Não há atalho de teclado para salvar (`Ctrl+S`). O resultado do save já virou snackbar (037/030 — não empurra mais o layout).
- **Matriz (`SkillsMatrix.razor`):** clicar numa célula/header navega para o **detalhe** read-only da classe (`NavigateTo` → `/customclasses/classes/{bare}`), não para o edit na aba Skills.
- **Preferências:** **nenhuma é persistida**. Cada reload/navegação reseta: o filtro da sidebar, os dois toggles da matriz ("Mostrar desabilitadas" default on, "Multiplicadores XP" default off), a ordenação da lista (não existe ainda) e a aba ativa do edit.
- **Pickers:** o `ItemPicker` seleciona por clique; um resultado único ainda exige o clique (sem atalho Enter). *(O kickoff lista "resultado único → Enter seleciona" — ver §Fora de escopo.)*

## Comportamento desejado

### a) Densidade global

- Todos os `MudTable`, `MudSelect`, `MudTextField`, `MudNumericField`, `MudTabs` e diálogos do editor adotam a densidade compacta (`Dense="true"` onde o componente expõe `Dense`; `Margin="Margin.Dense"` nos campos de texto/numéricos; `MudTabs` com `PanelClass` de padding reduzido). Meta de design: **~2× mais linhas/campos por tela** na lista e nas abas de edição, sem sobreposição nem corte de conteúdo.
- A folha de densidade do 033 (`wwwroot/css/customclasses.css`, classe `.cc-dense` e paddings de tabela) continua sendo a fonte de verdade para padding de tabelas customizadas; o item só garante que os componentes Mud que ainda estavam airy passem a `Dense`/`Margin.Dense`.

### b) Lista de classes — ordenação + Edit direto

- **Colunas ordenáveis** (`MudTable` com `SortLabel`/`SortBy`): no mínimo **Class (nome)**, **Skill cost** e **Loadout ₽**. Clique no header alterna asc/desc; a coluna+direção ativas são **persistidas** (§d).
- Cada linha ganha uma ação rápida **"Edit"** (ícone lápis), ao lado de Duplicate/Delete, que abre `/customclasses/classes/{bare}/edit` direto — sem passar pelo detalhe. Desabilitada quando a classe não tem `Definition` parseável (mesmo critério do Duplicate hoje).
- O ícone da classe continua renderizando na primeira coluna (já funciona — manter).
- A sidebar (`NavMenu`) ganha a mesma ação "Edit" acessível por linha (ex.: ícone que aparece no hover do item), levando direto ao edit da classe — atende "ação Edit direto da lista/sidebar" do kickoff.

### c) Edição — aba preservada + Ctrl+S

- Trocar de classe pela sidebar enquanto se está no edit **preserva a aba ativa**: se o usuário está comparando a aba Skills da classe A, ao clicar na classe B na sidebar ele cai no edit da B **já na aba Skills**. A aba ativa é persistida por índice (§d) e relida ao montar o `ClassEdit`.
  - Restrição: a aba alvo só é preservada se existir e for válida para a classe destino (todas as 7 abas existem sempre — General/Skills/Multipliers/Hideout/Outfit/Equipped/Stash; índice fora do range cai em General).
  - O guard de mudanças não salvas (`BaseLayout` `NavigationLock`) continua intermediando a troca quando o form está sujo — este item não altera o guard.
- **`Ctrl+S`** (e `Cmd+S` no mac) na página de edit dispara o **Save** (mesma rota do botão Save), prevenindo o "salvar página" nativo do browser. Quando `_saving` está em andamento, o atalho é no-op (não enfileira saves).

### d) Preferências persistidas (`localStorage`)

Estado de UI persistido por chave estável, lido na montagem e gravado na mudança, **escopo por-browser** (single-user, premissa do item 021):

| Preferência | Origem | Chave sugerida |
|---|---|---|
| Pin do drawer (Mini ↔ Persistent) — *ver nota PA-R1-01 na tech* | `BaseLayout` `MudDrawer` `Variant` | `cc.ui.drawerPinned` |
| ~~Última vista (detail vs edit)~~ — *removida no v1 (PA-R1-08): a vista já vem da URL na troca pela sidebar; sem consumidor* | — | — |
| Aba ativa do edit | `ClassEdit` `ActivePanelIndex` | `cc.ui.editTab` |
| Ordenação da lista (coluna + direção) | `Classes` `MudTable` SortLabel/dir | `cc.ui.listSort` |
| Toggles da matriz (showDisabled, showMultipliers) | `SkillsMatrix` | `cc.ui.matrixToggles` |
| Filtro da sidebar (opcional) | `NavMenu` `_filter` | `cc.ui.sidebarFilter` |

- Persistência via **JS interop** (`localStorage`) — o mod ainda não tem JS próprio; este item introduz um pequeno `wwwroot/js/customclasses.js` (lido/escrito por um helper C# `IJSRuntime`) servido pelo mesmo mount `/CustomClasses-Server/` que já serve css/icons. O mesmo arquivo hospeda o handler de `Ctrl+S` (§c).
- **Defaults na ausência da chave** = exatamente os defaults de hoje (drawer Mini/não-pinado, aba General, ordem de arquivo, toggles on/off). Chave corrompida/ausente → fallback silencioso ao default (nunca quebra a página).
- **Prerender:** a leitura de `localStorage` só pode ocorrer **depois** do circuito interativo conectar (JS indisponível no prerender estático — ver §030 BaseLayout UI-03). A persistência aplica no `OnAfterRenderAsync(firstRender)`, não no `OnInitialized`.

### e) Matriz → edit direto

- Clicar numa **célula** da matriz (`SkillsMatrix`) navega para o **edit da classe na aba Skills** (`/customclasses/classes/{bare}/edit`, com a aba Skills selecionada via a preferência persistida `cc.ui.editTab` ou um parâmetro de rota/query). Header de coluna continua um destino de navegação (decisão: header → edit também, mantendo o ato "1 clique para editar a partir da matriz").
- Atende a métrica do DoD "editar a partir de qualquer vista ≤ 2 cliques" (matriz: 1 clique).

## Critérios de aceite

- [ ] Lista, abas de edição, pickers e diálogos usam densidade compacta (`Dense`/`Margin.Dense`); nenhuma tabela/campo do editor permanece em densidade default airy. Visualmente, a lista e cada aba de edição mostram aproximadamente o dobro de linhas/campos por viewport em relação ao default.
- [ ] As colunas **Class**, **Skill cost** e **Loadout ₽** da lista são ordenáveis (clique no header alterna asc/desc); a ordenação ativa sobrevive a um reload da página (persistida em `localStorage`).
- [ ] Cada linha da lista (e cada item da sidebar) tem uma ação **Edit** que abre o form de edição da classe em ≤ 1 clique, sem passar pelo detalhe; desabilitada para classes sem definição parseável.
- [ ] Estando no edit da classe A na aba Skills, clicar na classe B na sidebar abre o edit da B **na aba Skills** (aba preservada). Índice de aba inválido para a classe destino cai em General sem erro.
- [ ] `Ctrl+S` (e `Cmd+S`) na página de edit dispara o Save e **impede** o diálogo nativo "salvar página" do browser; durante um save em curso o atalho não enfileira um segundo save.
- [ ] Clicar numa célula da matriz abre o **edit** da classe correspondente já na aba **Skills** (1 clique).
- [ ] Estado do drawer, última vista, aba ativa do edit, ordenação da lista e os dois toggles da matriz são lidos do `localStorage` na montagem (após o circuito conectar) e regravados ao mudar; na primeira visita (chave ausente) o comportamento é idêntico ao de hoje.
- [ ] Nenhuma mudança de validação, custo, schema ou save/hot-apply: salvar produz o mesmo `.jsonc`, os mesmos diagnósticos e o mesmo audit log que antes do item.
- [ ] `docs/class-editor.md` atualizado com os fluxos/rotas das waves 030–036 (sidebar, matriz, dashboard, comparação) e com os novos atalhos/persistência deste item, **preservando o frontmatter/cabeçalho** e adicionando linha no Histórico de Alterações.

## Corner cases

- [ ] **Prerender sem JS:** ler `localStorage` no prerender estático lança/retorna vazio. A leitura/aplicação das preferências só roda em `OnAfterRenderAsync(firstRender: true)`; antes disso a UI mostra os defaults — nunca um erro de "JS interop não disponível durante prerender".
- [ ] **Chave de `localStorage` ausente/corrompida:** parse falho (ex.: índice de aba não numérico, JSON inválido nos toggles) → fallback silencioso ao default, nunca exceção que derrube o circuito.
- [ ] **Aba persistida fora do range na classe destino:** a aba alvo preservada na troca de classe pode não existir (índice antigo > nº de abas). Cai em General (índice 0), sem erro.
- [ ] **Ctrl+S com form inválido / save bloqueado:** o atalho chama o mesmo `SaveAsync` do botão — a validação de form (UI-01) e o bloqueio por Error continuam valendo; o atalho não pula a validação.
- [ ] **Ctrl+S fora da página de edit:** o handler de teclado só é armado na página de edit (ou checa a rota antes de agir); em lista/detalhe/matriz o `Ctrl+S` mantém o comportamento nativo do browser (não sequestrar globalmente).
- [ ] **Ordenação por coluna numérica com valor ausente:** Skill cost / Loadout podem ser `—` (classe sem definição / parse error). A ordenação trata o ausente de forma estável (ex.: empurra para o fim em ambas as direções) sem `NullReferenceException`.
- [ ] **Edit direto de classe inválida:** o botão Edit na lista/sidebar fica desabilitado quando a classe não tem `Definition`; clicar numa célula da matriz de uma coluna inválida cai no detalhe (fallback, igual ao comportamento atual da sidebar para classes sem definição) em vez de abrir um edit que não monta.
- [ ] **Densidade não pode quebrar componentes ilegais:** algumas props `Dense` não existem em certos componentes (ex.: `MudTextField` usa `Margin.Dense`, não `Dense`; ver ref code-review CR-01-02 do 034 que removeu um `Dense` ilegal). A passada de densidade respeita a prop válida de cada componente (não reintroduzir `MUD0002`).
- [ ] **Concorrência de circuitos (Blazor Server):** múltiplas abas do browser compartilham o mesmo `localStorage`; a última escrita vence. Aceitável (single-user) — registrar, sem coordenação cross-tab.

## Fora de escopo

- [ ] **Regressão visual Chrome MCP + re-medição dos tempos do 037** (PA-035-00) — responsabilidade do orquestrador na validação final.
- [ ] **Picker "resultado único → Enter seleciona"** — o kickoff lista, mas é um refinamento de baixo valor e exige interceptar Enter no `MudTextField` do picker (que já tem debounce); **adiado** para não inflar o item. Registrado como divergência consciente; pode virar item próprio se houver demanda. (Decisão autônoma — usuário ausente.)
- [ ] Mudança de qualquer comportamento de validação, custo, schema, save/hot-apply ou layout estrutural das telas (essas vieram nas waves 030–036).
- [ ] Persistência server-side / por-perfil das preferências (escopo é `localStorage` por-browser).

## Referências

- Kickoff: [035-densidade-cliques-00-kickoff.md](./035-densidade-cliques-00-kickoff.md)
- Performance/contrato de cache (consumido pela lista/sidebar/matriz): [037-performance-cache-01-spec.md](../037-performance-cache/037-performance-cache-01-spec.md)
- Guia de uso do editor: [class-editor.md](../../docs/class-editor.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec funcional criada via `/create-spec` (autônoma — usuário ausente). Premissas PA-035-00 (regressão fora de escopo) e divergências do kickoff (ícone já renderiza; Enter-no-picker adiado) registradas. |
