# 031 — Notificação de sync: mensagem mentirosa + estado que não se limpa + i18n

**Mod:** Launcher4.0-v2
**Status:** Backlog
**Criado:** 2026-08-02

## Visão geral

As mensagens que o launcher mostra durante e depois de sincronizar os arquivos estão erradas em várias frentes: **toda** ação aparece como "Baixando" (mesmo quando o arquivo está sendo *removido* ou *arquivado*), a mensagem **final** de sucesso é montada em português dentro do código (então no idioma inglês o jogador vê texto em português + jargão interno como "movidos p/ disabled"), o processo **não fecha o ciclo** (a barra fica pendurada na última linha de progresso, sem uma mensagem de conclusão clara), e o link do relatório da verificação **anterior** não é limpo ao começar uma nova. Este item revê todas essas notificações — o que aparece durante, o que aparece no fim, e o estado que sobra — deixando cada mensagem fiel à ação, traduzida, e fechando o ciclo. A **velocidade de download** (a taxa em MB/s) é o item irmão **032**, feito em conjunto mas com spec própria.

## Comportamento atual

- **Toda ação de I/O vira "Baixando".** O texto de progresso do apply é um só ([ProfileViewModel.cs:682](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L682) usa sempre `update_downloading`), porque o motor só reporta a fase genérica `"applying"` ([SyncEngine.cs:85](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L85)), sem dizer qual ação é. Resultado: **remover** um extra, **mover um mod para a quarentena** (o move de pasta do item 034), **semear**, **forçar config** e **aplicar config opcional** — todos aparecem como "Baixando: X". Foi o que o jogador viu ao desligar o TRL-PvpMode: a pasta estava sendo *arquivada*, mas a tela dizia "Downloading: TRL-PvpMode". Existe até uma string "Removendo" (`update_deleting`) que **nenhum código usa** (órfã).
- **A mensagem final não é traduzida.** A mensagem de sucesso mostra `result.Summary`, e o `Summary` é montado com texto **fixo em português dentro do código** ([SyncResult.cs:66-75](../../project/SPT.Launcher.Base/Sync/SyncResult.cs#L66-L75)): `"3 atualizados · 2 preservados · 1 movidos p/ disabled"`. No idioma inglês, o jogador vê essa frase em português, com o jargão interno `"movidos p/ disabled"` exposto.
- **O ciclo não fecha.** A barra de progresso (`IsUpdateVisible`) nunca volta a ficar oculta no caminho de sucesso — fica pendurada na tela. E a mensagem final de conclusão pode ser **sobrescrita** pela última linha de progresso que chega atrasada (o progresso é postado de forma assíncrona; um report em voo pousa depois da mensagem final). No print do jogador, o processo terminou preso em "Downloading …", sem mensagem de conclusão.
- **O estado do run anterior não é limpo.** Ao clicar "Verificar arquivos", o placar/link `"{0} arquivo(s) foram atualizados, ver detalhes"` do run **passado** continua na tela ([reset em :453-459](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L453) zera status/progresso/taxa mas **não** o link), e o link abre um relatório obsoleto. Pior: o link só aparece se `Updated > 0` — num run que só **moveu/removeu** arquivos (`Updated = 0`), o link **some** justamente quando houve mudança relevante.
- **Dois fluxos divergentes.** A tela logada (`ProfileViewModel`) e a tela de setup (`ModUpdateViewModel`) usam mensagens de sucesso **diferentes** (`update_completed_success` × `update_completed`), com estilos e conteúdos distintos.

## Comportamento desejado

**1. Mensagem de progresso por tipo de ação.** Cada ação de I/O mostra um texto fiel ao que está acontecendo — baixando, removendo, **arquivando** (mod saiu do servidor → quarentena), instalando padrão, aplicando config obrigatória, aplicando config opcional. O vocabulário é de jogador: nada de "plugins-disabled" ou "movidos p/ disabled" na tela; a ação de quarentena é explicada como "o mod saiu do servidor — foi arquivado".

**2. Mensagem final traduzida e legível.** O resumo final para de ser montado em português no código e passa a ser composto a partir de textos traduzidos (PT+EN), formando uma frase clara de conclusão (ex.: "Concluído: 3 baixados, 1 arquivado, 2 mantidos.") — sem jargão, no idioma ativo.

**3. Fechar o ciclo.** Ao terminar (sucesso, erro ou cancelamento), a mensagem final **sempre** é a última exibida — uma linha de progresso atrasada nunca a sobrescreve. A **barra de progresso** (a barrinha que enche) se **oculta** ao concluir; a **conclusão** permanece visível como a linha de status final **+** o link do relatório (que não some sozinho). Assim o jogador nunca fica olhando uma barra pendurada em "Baixando", e ao terminar vê um resumo estático com o link para os detalhes — que só é substituído no próximo run.

**4. Reset único no início do run.** Ao começar qualquer verificação (botão "Verificar arquivos" **ou** o auto-check do login), um único ponto limpa **tudo** que é do run anterior: texto de status, progresso, taxa **e** o link/placar do relatório. Durante a nova verificação, nada obsoleto do run passado aparece na tela.

**5. Link do relatório por total de ações.** O link "ver detalhes" fica visível quando **qualquer** ação relevante aconteceu (baixou, removeu, moveu para quarentena, forçou config…), não só quando baixou algo. E sempre aponta para o relatório **deste** run.

**6. Consolidar e traduzir.** As mensagens dos dois fluxos de sync (tela logada e tela de setup) são unificadas (mesmo texto/comportamento de sucesso); as strings órfãs são removidas; e todas as strings novas existem em **PT e EN** com paridade exata.

## Critérios de aceite

- [ ] **CA-031.1 (progresso por ação):** ao mover um mod para a quarentena, a barra mostra um texto de **arquivamento** ("o mod saiu do servidor — arquivado"), não "Baixando". Cada tipo de ação (baixar, remover, arquivar, semear, forçar config, config opcional) tem seu próprio texto. Verificável: desligar um mod-pasta e observar o texto durante o apply.
- [ ] **CA-031.2 (final traduzido):** com o launcher no idioma **inglês**, a mensagem final de conclusão aparece **100% em inglês** — nenhuma palavra em português (`atualizados`/`preservados`/`movidos p/ disabled`). Idem invertido para PT.
- [ ] **CA-031.3 (sem jargão):** nem a mensagem de progresso nem a final expõem termos internos (`plugins-disabled`, `-disabled`, `MoveToDisabled`) ao jogador — a quarentena é descrita em linguagem de produto.
- [ ] **CA-031.4 (ciclo fecha):** ao terminar sem erro, a **última** mensagem visível é a de conclusão (nunca uma linha de "Baixando …" pendurada), e a barra deixa o estado "em andamento" (some ou vira resumo estático). Verificável: rodar um sync com pelo menos uma ação e confirmar que a tela termina numa mensagem de conclusão, não de progresso.
- [ ] **CA-031.5 (reset limpa o run anterior):** clicar "Verificar arquivos" limpa o placar/link do run passado **antes** de começar; durante a nova verificação não aparece nenhum número/link do run anterior; o link, ao fim, abre o relatório do run atual.
- [ ] **CA-031.6 (link por total de ações):** num run cujo único evento foi mover/remover arquivos (`Updated = 0`), o link "ver detalhes" **permanece visível** e abre o relatório deste run.
- [ ] **CA-031.7 (dois fluxos consistentes):** a mensagem de sucesso da tela logada e a da tela de setup são a **mesma** (mesmo texto/estilo), no idioma ativo.
- [ ] **Fika/multiplayer:** `N/A` — é a camada de notificação da UI do launcher (pré-jogo), não roda no cliente durante o raid nem troca pacote. As ações descritas (baixar/arquivar/aplicar) são locais; a fidelidade da mensagem não muda o comportamento de coop. (Se um mod coop-essencial foi preservado pelo guard coop-safe, a mensagem pode dizer "mantido", não "arquivado" — coberto por CA-031.1.)
- [ ] **Estado entre raids:** `N/A` — acontece no fluxo de login/pré-jogo do launcher; nenhum estado de raid é criado ou alterado.

## Corner cases

- [ ] **CC-1 (run cancelado):** ao cancelar no meio, a última mensagem é a de cancelamento (estado parcial), não uma linha de progresso; a barra fecha o ciclo.
- [ ] **CC-2 (run com erro):** havendo erro por-arquivo, a mensagem final informa o erro de forma clara e traduzida (quantos ok / quantos falharam), e o link aponta para o relatório.
- [ ] **CC-3 (plano sem I/O — só preservados):** quando nada foi baixado/movido (tudo já up-to-date, ou só preservações), a mensagem final é "tudo atualizado" (com "(N preservados)" quando houver), sem falsa impressão de que algo aconteceu.
- [ ] **CC-4 (sync do item 030):** o sync disparado pelo `PendingApply` logo após a tela "Mods e Configs" mostra mensagens coerentes com o que o jogador acabou de escolher (mods ligados baixados, desligados arquivados).
- [ ] **CC-5 (move-de-pasta do 034):** um mod-pasta arquivado aparece como **uma** ação/entrada (a pasta), não N por-arquivo; o resumo final conta a pasta como 1 e o link abre o relatório com a entrada agregada.
- [ ] **CC-6 (Dev Mode):** em Dev Mode nada é movido; a mensagem final reflete "preservado por Dev Mode" quando aplicável, sem sugerir remoção.
- [ ] **CC-7 (corrida do progresso):** uma linha de progresso postada atrasada **não** sobrescreve a mensagem final — a mensagem de conclusão sempre é a última.
- [ ] **CC-8 (manifesto falhou no começo):** se a verificação aborta logo no início (servidor sem manifesto), o texto/placar limpo pelo reset **não** ressuscita o do run anterior; a tela mostra o estado de "não foi possível verificar", não um resumo velho.
- [ ] **CC-9 (idioma trocado no meio):** trocar de idioma com uma mensagem de conclusão na tela — decidir se ela re-renderiza no novo idioma ou permanece (aceitável permanecer até o próximo run, já que é reativo por chave).

## Fora de escopo

- [ ] **Velocidade de download (taxa MB/s)** — é o item irmão **032** (medição intra-arquivo/streaming + rebinding da UI). Feito em conjunto com este, mas com spec própria. Este item cobre o **reset** da taxa no início do run (mecanismo 4), não a medição.
- [ ] Redesenho visual da barra de update além do texto e do estado "em andamento/concluído" (cores, animação, layout).
- [ ] Mudar o formato do arquivo de relatório (`last-update.json`) — só a visibilidade/limpeza do link para ele.

## Referências

- [031 — Kickoff](./031-notificacao-sync-mensagem-final-00-kickoff.md) (diagnóstico original D-031.1/2/3)
- [Item 034 — Quarentena move a pasta](../034-quarentena-mover-pasta-do-mod/) (a ação `moved-to-disabled` que precisa de texto próprio; CA-034.7 = 1 entrada agregada)
- [Item 032 — Velocidade de download](../032-velocidade-download-nunca-funcionou/) (irmão; coordenar o reset da taxa e o rebinding)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Spec funcional criada via `/create-spec`, ampliando o kickoff. Origem: relato do usuário (print do TRL-PvpMode arquivado aparecendo como "Downloading" + sem mensagem final). Review desta sessão adicionou os achados B (mensagem final não traduzida — `Summary` em PT hardcoded) e F (dois fluxos de sync divergentes) além dos 3 defeitos do kickoff. Velocidade separada no item 032 (feito em conjunto). |
| 2026-08-02 | `/review-spec` — 2 decisões firmadas: pós-estado da barra (mec. 3 = barra de progresso oculta ao concluir, resumo+link ficam estáticos até o próximo run); flicker de arquivos pequenos coordenado com o 032 (média móvel + ticker, sem piso). |
