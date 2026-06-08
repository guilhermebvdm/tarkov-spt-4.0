# 001 — Scaffold + 1 classe (walking skeleton)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Primeiro item do mod: provar, ponta a ponta, o mecanismo central de "classe = edition selecionável no launcher". Entregar a estrutura mínima do mod (lado servidor) que adiciona **uma** classe nova à tela de criação de personagem do launcher, com **apenas skills iniciais estáticas** definidas. Nenhuma outra capacidade (itens, outfits, multiplicadores, multi-classe, i18n) entra aqui — o objetivo é validar o caminho de injeção e a criação de perfil sem erros. Este item entrega **apenas o componente servidor** — sem plugin client/BepInEx, sem F12, sem patches.

> **Decisão (2026-06-07):** o walking skeleton usa uma **classe de teste mínima** ("Test Class") com 1-2 skills iniciais, só para provar o mecanismo. As classes reais entram no item 002 (multi-classe) / 007 (migração).


## Comportamento atual

Hoje o mod não existe. A tela de criação de personagem do launcher mostra apenas as edições nativas do SPT (Standard, Left Behind, Edge Of Darkness, Unheard, e as variантes SPT). Não há nenhuma "classe" customizada, e nenhuma forma de o jogador iniciar um personagem com skills pré-definidas por papel.

## Comportamento desejado

Com o componente servidor do mod instalado, a tela de criação de personagem do launcher passa a listar **uma** nova edição (a classe), ao lado das nativas, com um nome e uma descrição própria. Ao criar um personagem escolhendo essa edição, o perfil é criado normalmente e o personagem **nasce com um conjunto pré-definido de skills em níveis iniciais** (definidos pela classe). Tudo o mais (inventário, aparência, traders, hideout, progressão) permanece igual ao de um início padrão. Desinstalar o mod remove a edição, sem deixar resíduo nem quebrar o launcher.

## Critérios de aceite

- [ ] Com o servidor do mod instalado, a tela de criação de personagem do launcher lista a nova edição da classe junto às edições nativas.
- [ ] A edição exibe uma descrição não-vazia em inglês na tela de seleção.
- [ ] No boot, o servidor loga uma confirmação única de que a classe foi registrada; criar um personagem com essa edição conclui sem erros (sem exceção; perfil criado e carregável).
- [ ] O personagem criado começa com exatamente os níveis de skill definidos pela classe (verificável na tela de Skills in-game).
- [ ] Skills não definidas pela classe começam no valor padrão do jogo (a classe só altera as que lista).
- [ ] Remover/desabilitar o mod faz o launcher voltar a mostrar apenas as edições nativas, sem edição órfã nem erro.

## Corner cases

- [ ] **Os dois lados (USEC e BEAR):** escolher a classe com qualquer das duas facções deve produzir as mesmas skills iniciais da classe (a definição cobre ambos os lados).
- [ ] **Wipe / recriação:** se já existe um perfil e o jogador faz wipe e recria escolhendo a classe, as skills da classe aplicam no perfil novo sem herdar estado antigo.
- [ ] **Descrição sem tradução:** se o idioma ativo do launcher não tiver entrada para a descrição da classe, deve cair para o inglês — nunca exibir uma chave crua nem quebrar.
- [ ] **Valores de skill fora do intervalo:** nível configurado acima do máximo do jogo ou negativo deve ser tratado/limitado, sem gerar perfil inválido.
- [ ] **Colisão de identificador de edição:** o identificador da classe não pode sobrescrever silenciosamente uma edição nativa ou de outro mod — colisão deve ser evitada/detectada.
- [ ] **Ordem de carregamento:** a edição só é adicionada depois que o banco de perfis do servidor está pronto, sem corrida com a inicialização do servidor.
- [ ] **Restart do servidor (idempotência):** reiniciar o servidor não duplica a edição nem corrompe o banco de perfis — o registro é idempotente entre boots.
- [ ] **Não-regressão das nativas:** após a injeção, as edições nativas do SPT (e de outros mods) continuam listadas e funcionais.
- [ ] **Definição malformada/ausente:** se a definição da classe vier de arquivo e estiver malformada ou faltando, o servidor loga erro claro e degrada (não registra a classe) — **sem** derrubar o servidor.

## Fora de escopo

- Itens iniciais (stash, equipados, compostos) — item 003.
- Outfits/aparência — item 004.
- Multiplicadores de evolução de skill — item 005.
- Compatibilidade com Skills-Extended — item 006.
- Múltiplas classes / loader de N JSONs — item 002 (aqui é **uma** classe, pode ser hardcoded ou um único JSON).
- Tradução pt-BR e seletor de língua no F12 — item 008 (aqui basta a descrição em inglês).
- Migração das 10 classes do RZCustomProfiles — item 007.
- Componente client/BepInEx (F12, patches, plugin Unity) — 001 é **só servidor**; o client entra a partir do 005/008.

## Referências

- Plano aprovado da sessão 2026-06-07 (`~/.claude/plans/`).
- [README do mod](../../README.md) · [backlog](../mod-backlog.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` |
| 2026-06-07 | Revisão `/review-spec` — escopo client/server clarificado, +1 critério (log de confirmação), +3 corner cases (idempotência, não-regressão, definição malformada), 1 decisão marcada (qual classe) |
| 2026-06-07 | Decisão: walking skeleton usa classe de teste mínima ("Test Class") |
