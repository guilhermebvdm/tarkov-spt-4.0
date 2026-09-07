# 003 — Fix rejeição de swap de magazine in-place no Headless (GClass1561)

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-06

## Visão geral

Existe uma funcionalidade (provida por outro mod, UIFixes) de troca 1-para-1 de carregador: o jogador arrasta o carregador do colete/rig diretamente para a arma empunhada (ou para o slot de magazine na janela de Inspecionar), trocando de lugar com o carregador atual da arma, sem que nenhum dos dois caia no chão e sem exigir vaga extra no colete. Esse fluxo funciona corretamente em singleplayer, mas em partidas coop conectadas a um servidor FIKA Headless dedicado, o servidor recusa a troca alegando que o item já está sendo modificado, deixando o carregador do rig piscando indefinidamente e o jogador com o gatilho/mãos travados até um mecanismo de recuperação por timeout (já implementado em outra correção deste mod) liberar o travamento.

## Comportamento atual

- O cliente aplica a troca visualmente de forma otimista e envia a operação para validação do servidor (Host ou Headless dedicado).
- Em partidas com Headless dedicado, essa validação às vezes recusa a troca alegando conflito de modificação simultânea no item, mesmo quando não há nenhuma outra ação de fato concorrendo pelo mesmo carregador/arma naquele instante.
- Quando a recusa acontece, o cliente fica com o estado visual e o estado real dessincronizados: o carregador antigo permanece "preso" visualmente, e a ação de rede pendente do jogador não é liberada dentro do tempo normal de round-trip — só se recupera quando o timeout do mecanismo de watchdog (já existente) drena o pendente.
- Uma tentativa anterior de correção, feita do lado do mod que provê a funcionalidade de troca (UIFixes), tentou suprimir esse erro do lado do jogador que inicia a ação, mas não resolveu o problema em coop: a validação que efetivamente recusa a troca acontece do lado do servidor dedicado (um processo separado da máquina/instância do jogador), e o ajuste feito do lado do cliente não alcança esse processo.
- Uma investigação já conduzida nesta sessão (a ser registrada em `mods/FIKA/memory/sessions.md` via `/update-memory`) determinou que a checagem causadora da recusa **não é uma peculiaridade do FIKA nem algo introduzido por nós** — é uma proteção que já existe no motor do jogo (comportamento idêntico entre o caminho local e o caminho usado pelo servidor dedicado), destinada a impedir que um item seja mutado enquanto uma transição de "entrar/sair das mãos" relacionada a ele ainda está em andamento. A hipótese mais provável (a ser confirmada na spec técnica) é uma corrida entre a confirmação dessa transição de mãos e a chegada da troca de carregador ao servidor — não uma colisão real de duas ações concorrentes.

## Comportamento desejado

- A troca 1-para-1 de carregador em arma empunhada deve ser aceita pelo servidor (Host e Headless dedicado) sem recusa, preservando a animação canônica de recarga, no caso comum em que não há nenhuma outra ação de fato concorrendo pelo mesmo item.
- A proteção original do motor do jogo contra mutação concorrente de um item durante uma transição de mãos genuinamente incompleta deve continuar funcionando sem enfraquecimento — a correção deve resolver a causa da recusa indevida (ver hipótese acima), não desativar de forma ampla a checagem por tipo de item.
- A correção deve ser genérica o suficiente para proteger qualquer ação legítima de troca de carregador em arma empunhada validada pelo servidor dedicado — não deve ser acoplada especificamente ao mod que hoje dispara esse fluxo (UIFixes), já que a validação recusada acontece numa camada compartilhada por qualquer mod que gere esse tipo de operação.
- O jogador nunca deve ficar com a ação de rede pendente além do tempo normal de round-trip como consequência deste fluxo específico, sem depender do mecanismo de timeout existente para se recuperar.

## Critérios de aceite

- [ ] Arrastar um carregador do rig sobre a arma empunhada (nos três slots de arma do personagem) executa a troca 1-para-1 com sucesso numa partida coop conectada a um Headless dedicado, sem que o servidor recuse a operação por conflito de modificação simultânea.
- [ ] A mesma troca, feita no slot de magazine da janela de Inspecionar da arma, também é aceita sem recusa.
- [ ] Após a troca, nenhum carregador cai no chão e nenhuma vaga extra no colete é exigida — o carregador antigo ocupa exatamente o slot de onde saiu o novo.
- [ ] A ação de rede do jogador nunca fica pendente além do tempo normal de round-trip como resultado deste fluxo, sem depender do timeout do mecanismo de watchdog existente para se recuperar.
- [ ] **Fika/multiplayer:** a troca funciona igualmente como Host (outro jogador observando a ação) e como cliente conectado a um Headless dedicado. Um segundo jogador tentando mutar o mesmo item (mesma arma/mesmo carregador) enquanto a operação do primeiro ainda está em andamento continua sendo corretamente recusado — a correção não deve eliminar essa proteção contra concorrência real.
- [ ] **Estado entre raids:** o comportamento é consistente entre raids sucessivos (raid1 → sair → raid2) e não depende de estado acumulado de uma raid anterior; a morte, o status "desaparecido em combate" ou o fechamento abrupto do jogo por um jogador durante uma troca em andamento não deixam o servidor num estado que bloqueie trocas futuras de outro jogador.

## Corner cases

- [ ] Dois jogadores diferentes tentando trocar carregador no mesmo item ao mesmo tempo (ex: item compartilhado via troca/loot) — deve continuar sendo recusado.
- [ ] Troca disparada logo após o jogador sacar a arma (uma transição de equipar/desequipar anterior ainda em confirmação) — a correção não deve liberar indevidamente uma colisão real com essa transição diferente.
- [ ] Arma sem carregador algum no momento da troca (slot de magazine vazio) — deve cair no fluxo normal de carregar um carregador novo, não neste fluxo de troca 1-para-1.
- [ ] Latência alta ou perda de pacote entre cliente e servidor durante a troca — deve degradar para uma recusa limpa e recuperável rapidamente, nunca para uma aplicação parcial/inconsistente do estado do item.
- [ ] Interação com outros mods que também participam do fluxo de recarga em coop (ex: mods de interrupção de recarga, mods de animação de carregamento de munição) — não deve haver regressão nessas interações.
- [ ] Bots controlados pelo servidor dedicado — a correção não deve alterar o comportamento de recarga de bots.

## Fora de escopo

- [ ] Revisão/limpeza do patch atual do lado do UIFixes que tentava suprimir a recusa no cliente (hoje ineficaz para coop e com escopo amplo demais) — tratada como item de acompanhamento separado no próprio UIFixes, depois de validada esta correção.
- [ ] A definir (demais itens, se surgirem durante a spec técnica)

## Referências

- Precedente de correção cirúrgica análoga a este tipo de rejeição em jogador observado pelo servidor dedicado: item [001-drop-backpack-sync-fix](../001-drop-backpack-sync-fix/).
- Mecanismo de recuperação por timeout já existente, mencionado no "Comportamento atual": item [002-inventory-desync-watchdog](../002-inventory-desync-watchdog/).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-06 | Item criado via `/add-backlog-item` |
| 2026-09-06 | Revisão `/review-spec` — 1 gap crítico (spec continha nomes de classe/método do EFT e `arquivo:linha`, proibidos no estágio funcional) + 2 gaps de escopo (fix genérico não acoplado ao UIFixes; fronteira com a Trilha B do UIFixes) + 1 corner case adicionado (arma sem carregador) corrigidos |
