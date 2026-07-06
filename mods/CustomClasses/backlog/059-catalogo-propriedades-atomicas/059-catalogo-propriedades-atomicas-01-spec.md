# 059 — Catálogo de propriedades atômicas + fix da aba CLASS

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-02

## Visão geral

A aba **CLASS** na tela de Skills lista os perks/drawbacks da classe ativa. Este item tem duas frentes:
(1) **corrigir o botão da aba** — hoje o rótulo não aparece e a posição está deslocada; e (2) **reestruturar
o catálogo de perks** de "um perk = um bloco de texto" para **propriedades atômicas** (cada efeito numa linha
própria), onde a natureza **perk ou drawback** e o **valor exibido** são **derivados do multiplicador** do
efeito + a direção "boa" da propriedade (maior-é-melhor vs menor-é-melhor). O painel passa a exibir os perks e
os drawbacks em **duas colunas** (perks à esquerda, drawbacks à direita). Escopo: **só a exibição** (client-side);
a mecânica dos perks (que altera recuo/velocidade/etc.) **não muda**.

## Comportamento atual

- **Aba CLASS:** existe como 1ª sub-aba (CLASS · SKILLS · MASTERING), mas o **rótulo textual não renderiza** no
  botão (aparece só um ícone) e a **posição** ficou deslocada pro canto; em algumas montagens a aba empurrava
  as outras e sobrepunha a caixa de busca da MASTERING.
- **Painel:** mostra os perks/drawbacks da classe como cards, agrupados em duas seções empilhadas verticalmente
  ("PERKS" em cima, "DRAWBACKS" embaixo). Cada card = um perk nomeado com **todos os seus efeitos numa única
  linha de texto** (ex.: "Heavy Frame — −10% speed, +30% hunger/thirst"). A classificação perk/drawback é
  **fixada à mão** em cada entrada.
- **Notificação de início de raid:** exibe a lista de perks/drawbacks da classe (uma linha por perk).
- Efeitos ainda não implementados no jogo já são marcados como "em breve".

## Comportamento desejado

- **Aba CLASS:** o botão mostra **ícone da classe + o rótulo "CLASS"** (genérico; em pt-BR "CLASSE"), **legível
  nos estados selecionado e não-selecionado**, alinhado à esquerda; permanece como **1ª aba**, sem deslocar nem
  sobrepor SKILLS/MASTERING (a caixa de busca da MASTERING continua intacta).
- **Painel em 2 colunas:** **perks à esquerda, drawbacks à direita**. Cada perk/drawback nomeado continua sendo
  um **card** (com seu ícone + nome), mas **cada efeito individual aparece numa linha própria** dentro do card
  (ex.: o card "Heavy Frame" tem duas linhas: uma para a velocidade, outra para fome/sede).
- **Classificação automática:** para cada efeito, se é **benéfico (perk, verde)** ou **prejudicial (drawback,
  vermelho)** e o **valor exibido** (ex.: "−15%", "+30%", "×0.85") são **derivados do multiplicador** do efeito
  e da direção da propriedade — não escritos à mão. Efeitos qualitativos (sem número, ex.: "braço não cansa")
  são marcados explicitamente como benéficos/prejudiciais.
- **Deferidos:** efeitos ainda não implementados exibem "em breve" (na própria linha, ou o card inteiro quando
  todos os efeitos daquele perk estão deferidos).
- **Notificação de raid:** permanece **compacta** — uma linha por perk/drawback nomeado (nome colorido),
  cabendo no toast.

## Critérios de aceite

- [ ] A 1ª aba da tela de Skills exibe **ícone + o texto "CLASS"** (pt-BR "CLASSE") **por completo — não
  truncado nem vazio** — tanto **selecionada** quanto **não selecionada**, alinhada à esquerda; ao clicar nela
  abre o painel de perks/drawbacks da classe.
- [ ] Selecionar SKILLS ou MASTERING funciona normalmente e a **caixa de busca de armas da MASTERING não é
  sobreposta** pela aba CLASS.
- [ ] O painel exibe **duas colunas**: todos os efeitos **benéficos à esquerda** e os **prejudiciais à direita**;
  cada efeito individual de um perk aparece em **sua própria linha** com seu valor.
- [ ] Um efeito "menor-é-melhor" com multiplicador < 1 (ex.: dano recebido ×0.85) aparece como **benéfico
  (verde, "−15%")** na coluna da esquerda; um "menor-é-melhor" com multiplicador > 1 (ex.: recuo ×1.25) aparece
  como **prejudicial (vermelho, "×1.25")** na coluna da direita.
- [ ] Efeitos ainda não implementados aparecem marcados com **"em breve"** (linha ou card).
- [ ] A **notificação de início de raid** lista os perks/drawbacks de forma **compacta** (uma linha por perk
  nomeado, colorida), sem estourar o toast.
- [ ] **Fika/multiplayer:** em coop, **cada cliente** mostra a aba/painel **e a notificação de início de raid
  da SUA própria classe** (menu e notificação **locais**); nenhum estado é sincronizado nem depende de outro
  jogador. Verificável abrindo a tela em 2 clientes com classes diferentes (cada um vê a sua). *(Não é `N/A`
  vazio: a notificação dispara no início da raid, mas é estritamente local por cliente.)*
- [ ] **Estado entre raids:** ao reabrir a tela de Skills (raid1 → extração/morte/MIA → raid2, e após alt-F4),
  a aba CLASS **não duplica**, o rótulo e a posição se mantêm, e o painel se reconstrói para a classe atual
  sem resíduo do estado anterior.

## Corner cases

- [ ] **Classe vanilla (não-mod) / sem perks:** o painel mostra uma mensagem "sem perks/drawbacks" ocupando a
  largura total (fora das 2 colunas), **sem retângulos brancos** (ícone/marca d'água sem sprite) e sem quebrar
  o layout de colunas.
- [ ] **Reabrir a tela de Skills repetidas vezes** (screen pooled) sem trocar de classe: a aba CLASS não é
  recriada/duplicada e o painel não pisca nem reconstrói à toa.
- [ ] **Efeito qualitativo sem multiplicador** (ex.: "lança-granadas sem penalidade de ergo", "braço não cansa"):
  aparece como linha **sem chip de valor**, classificado explicitamente como benéfico.
- [ ] **Colunas desbalanceadas** (ex.: classe com 3 perks e 1 drawback): o layout permanece correto, sem
  esticar/espalhar os cards.
- [ ] **Perk com efeitos deferidos parciais** (ex.: alguns efeitos ativos + 1 "em breve"): só a linha deferida
  recebe "em breve"; as ativas seguem normais.
- [ ] **Perk compartilhado entre classes** (ex.: o mesmo "+30% de carga" em duas classes): aparece idêntico nas
  duas, sem divergência de texto.
- [ ] **Classe vanilla no início da raid:** sem perks a listar → a **notificação de início de raid não aparece**
  (nada a mostrar), sem erro/toast vazio.

## Fora de escopo

- [ ] **Mecânica dos perks** — os patches/configuração que efetivamente alteram recuo/velocidade/etc. **não
  mudam**; este item é só exibição.
- [ ] **Editor web / seletor de propriedades** (visão futura) — a estrutura fica pronta para isso, mas o seletor
  em si não entra.
- [ ] **Sincronização dos efeitos entre jogadores no coop** — pertence ao item 057.
- [ ] **Detalhe da classe no lobby/loading** — pertence ao item 055.

## Referências

- Plano de design aprovado: `~/.claude/plans/shimmying-dreaming-wilkes.md`
- Design das classes (magnitudes/efeitos por classe): [class-design.md](../../docs/class-design.md)
- Item base da UI: [053-perks-ui-tab/](../053-perks-ui-tab/)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Item criado via `/add-backlog-item` |
| 2026-07-02 | Spec funcional criada via `/create-spec` (deriva do plano aprovado) |
| 2026-07-02 | Revisão `/review-spec` — critério "legível" → verificável; Fika de `N/A` → comportamento verificável (local por cliente + notificação); +1 corner case (notificação em classe vanilla) |
