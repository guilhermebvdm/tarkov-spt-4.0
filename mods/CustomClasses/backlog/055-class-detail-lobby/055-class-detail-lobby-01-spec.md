# 055 — UI: detalhe da classe no lobby/loading da raid

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-02

## Visão geral

Segundo ponto de entrada para o **detalhe da classe** (perks 🔧 / drawbacks 🔻), irmão do **053/059** (aba CLASS na
tela de Skills): na **tela de carregamento da raid do FIKA**, ao interagir com o **seu** player, abre um painel com o
detalhe da **sua** classe — o mesmo conteúdo e layout de 2 colunas do 059, reusando o `PerksCatalog`. **Escopo (decisão
do usuário 2026-07-02):** só a **classe local** (client-side, dados do próprio perfil como o 053) — **não** resolve a
classe de outros players (isso é o item **057**, ainda backlog). Só exibição; não toca mecânica.

## Comportamento atual

- O detalhe de perks/drawbacks da classe só existe na **aba CLASS** da tela de Skills (053/059): header (brasão + nome)
  + 2 colunas (perks à esquerda, drawbacks à direita), cards por grupo com efeitos em linhas.
- Na **tela de carregamento da raid do FIKA**, cada player aparece como uma **linha** (`LoadingScreenPlayer`: apelido +
  barra de progresso), instanciada por `LoadingScreenUI.AddPlayer(netId, nickname)`. Não há detalhe de classe ali.
- O item **015** (`PlayerNamePanelPatch`) já mostra **identidade** do player local (nome colorido + ícone + tooltip) no
  `MatchMakerPlayerPreview`/`PlayerNamePanel` (base EFT), mas **não** o detalhe completo (perks/drawbacks).

## Comportamento desejado

Na tela de carregamento da raid (FIKA), interagir (hover/click) com a **linha do seu player** abre um **painel de
detalhe da classe local** — perks à esquerda, drawbacks à direita, **idêntico** aos cards da aba CLASS (059),
reusando o `PerksCatalog` (mesmos grupos, cores derivadas do multiplicador, "em breve" nos deferidos). Bilíngue
(EN/pt-br, segue `GameLocale`). Client-side, só a sua classe.

## Critérios de aceite

- [ ] Na tela de carregamento da raid (FIKA), interagir com a **linha do player local** abre o painel de detalhe da
      classe local com **2 colunas** (perks/drawbacks), com **paridade visual e de conteúdo** com a aba CLASS (059).
      O **gatilho definitivo** (hover · click · auto-visível) é decidido na spec-tech e confirmado no gate; o critério
      verificável é: *o painel de detalhe aparece a partir da linha do player local e some quando a interação cessa/a
      tela de carregamento fecha*.
- [ ] O painel lista os **mesmos grupos/linhas** do `PerksCatalog` da classe ativa (cores derivadas, chips de valor,
      sufixo "em breve" nos deferidos) — sem reautorar dados (reusa o catálogo).
- [ ] **Bilíngue**: o painel respeita o idioma do EFT (`GameLocale`), como o 053.
- [ ] **Só a classe local**: interagir com linhas de **outros players** não abre detalhe (nenhuma tentativa de resolver
      a classe alheia — evita dependência do 057).
- [ ] **Degradação graciosa**: sem FIKA (solo) ou classe vanilla, nada quebra e nada aparece; a aba CLASS continua o
      acesso ao detalhe. Erros são logados e engolidos (não travam a tela de carregamento).
- [ ] **Fika/multiplayer:** cada client vê o detalhe da **sua própria** classe no seu próprio loading screen (dados
      locais, **sem** sincronização entre clients). Classes de outros players ficam fora de escopo (057).
- [ ] **Estado entre raids:** o painel é reconstruído a cada tela de carregamento (o `LoadingScreenUI` é recriado por
      raid); sem estado persistente. raid1 → exit → raid2 reflete a classe **atual**, sem resíduo do anterior.

## Corner cases

- [ ] **Loading muito curto** (mapa carrega rápido / SSD): a janela de interação pode ser pequena. Avaliar trigger que
      não exija precisão (hover na própria linha, ou painel auto-visível). Nota: o **lobby** (`MatchMakerUIScript`/
      `ListPlayer`, mais persistente) fica como host alternativo se o loading se provar curto demais — decidir no gate.
- [ ] **Player local não localizado** no loading (host headless, `netId` ausente, prefab não instanciado ainda): não
      anexar nada, sem exceção.
- [ ] **Múltiplas raids na mesma sessão** / re-init do loading screen: não duplicar handler nem painel (idempotência).
- [ ] **Classe trocada entre raids**: o painel relê `SkillMultipliers` a cada exibição → reflete a classe atual.
- [ ] **Solo sem FIKA**: o tipo `LoadingScreenUI` não existe → o patch (soft-detect) simplesmente não atua.
- [ ] **Coop — linha de outro player**: interagir não deve nem tentar resolver classe alheia (nenhuma chamada que
      dependa de registry per-player).
- [ ] **Coexistência com o 015** (`PlayerNamePanelPatch`, identidade no `MatchMakerPlayerPreview`): pontos de entrada
      distintos (confirmation vs loading) — não deve haver dupla renderização nem conflito de patch entre os dois.

## Fora de escopo

- [x] Detalhe da classe de **outros players** (per-player em coop) → **item 057** (registry per-player + rota server).
- [x] **Lobby** (`MatchMakerUIScript`) como host — só entra se o loading se provar inviável (nota de corner case).
- [x] Alterar mecânica de perks/drawbacks — este item é **só exibição** (reusa o `PerksCatalog`).
- [x] Refatorar a mecânica de classe / identidade (015/057).

## Referências

- [053-perks-ui-tab](../053-perks-ui-tab/) — aba CLASS na tela de Skills (mesmo conteúdo, outro ponto de entrada).
- [059-catalogo-propriedades-atomicas](../059-catalogo-propriedades-atomicas/) — modelo atômico + display 2 colunas (a reusar).
- [class-design.md](../../docs/class-design.md) — perks/drawbacks por classe.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Item criado via `/create-spec` (escopo = classe local no loading FIKA; per-player fica no 057) |
| 2026-07-02 | `/review-spec` — critério do gatilho tornado verificável + corner case de coexistência com o 015 |
