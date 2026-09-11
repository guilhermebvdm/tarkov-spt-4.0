# 001 — Sincronização Contínua de Clima em Raid

**Mod:** TRL-WeatherSync
**Status:** Backlog
**Criado:** 2026-09-10

## Visão geral

Em raids cooperativas (FIKA), o clima (chuva, vento, neblina, nuvens, temperatura) é decidido apenas uma vez, na tela de carregamento, e depois cada jogador passa a evoluí-lo por conta própria — o que inclui a decisão de iniciar uma tempestade. Este item entrega um mecanismo de sincronização contínua, autoritativo pelo Host, que mantém o clima consistente entre todos os jogadores durante a raid inteira, incluindo o início/fim de tempestades como evento explícito e compartilhado.

## Comportamento atual

Hoje, ao entrar numa raid FIKA, cada jogador troca informação de clima com o Host **uma única vez**, durante o carregamento. A partir do momento em que a raid começa, não existe mais nenhuma troca — cada instalação do jogo (Host e cada Cliente) segue evoluindo o clima de forma independente. Isso inclui a decisão de entrar em tempestade, que não é comunicada entre jogadores: é possível que um jogador veja céu limpo enquanto outro, na mesma raid, veja tempestade. Um jogador que reconecta no meio da raid também só recebe o clima do momento em que reconecta, sem garantia de que reflita exatamente o que os demais jogadores estão vendo.

## Comportamento desejado

Durante toda a raid, todos os jogadores (Host e Clientes) observam o mesmo clima, com pequenas variações apenas toleráveis por atraso de rede — chuva, vento, neblina, nuvens e temperatura evoluem de forma sincronizada e suave (sem "saltos" visuais perceptíveis quando um valor muda). O início e o fim de uma tempestade passam a ser uma decisão única, tomada pelo Host e comunicada a todos os Clientes, de forma que todo mundo entra e sai da tempestade dentro de uma janela curta de tempo entre si — não mais uma decisão que cada jogo toma sozinho. Um jogador que reconecta no meio da raid recebe o estado de clima atual, incluindo se há uma tempestade em andamento.

## Critérios de aceite

- [ ] Durante uma raid FIKA com 2+ jogadores, em qualquer momento observado ao longo de toda a raid (não só no início), nenhum jogador vê céu limpo enquanto outro vê chuva pesada ou neblina densa na mesma raid, sob condições normais de rede (latência típica de coop).
- [ ] Mudanças de intensidade de clima (chuva aumentando/diminuindo, neblina surgindo/dissipando) ocorrem de forma suave, sem salto perceptível na tela do jogador, mesmo quando causadas por uma atualização de sincronização vinda da rede.
- [ ] Quando uma tempestade começa durante a raid, todos os jogadores da mesma raid entram na tempestade dentro de poucos segundos entre si (nunca minutos de diferença), sob condições normais de rede. <!-- review: tolerância exata em segundos fica pra spec técnica (depende do intervalo de broadcast escolhido) -->
- [ ] O fim de uma tempestade também é sincronizado entre todos os jogadores da mesma forma.
- [ ] **Fika/multiplayer:** este item só se aplica a raids em coop via FIKA. Em raid solo (sem FIKA), o mod não deve alterar o comportamento nativo de clima do jogo nem introduzir tráfego de rede.
- [ ] **Estado entre raids:** a sincronização de clima é reiniciada a cada raid nova; nenhum estado de clima ou de tempestade "vaza" de uma raid pra outra — raid1 → exit → raid2 começa do zero, com handshake de clima limpo, igual ao comportamento nativo do FIKA hoje.

## Corner cases

- [ ] Jogador desconecta e reconecta no meio de uma tempestade — ao reentrar, precisa já receber o estado "em tempestade", não céu limpo.
- [ ] Pacotes de sincronização perdidos ocasionalmente por instabilidade de rede — o clima não deve travar nem regredir de forma abrupta quando a próxima atualização chegar; a transição seguinte deve absorver a lacuna suavemente.
- [ ] Mapa em que o próprio FIKA hoje pula a geração de clima customizado (ex: Laboratory, que fixa clima de verão sem chuva) — o mod não deve tentar sincronizar clima onde ele não é gerado.
- [ ] Raid sem outros jogadores conectados (host sozinho) — não deve haver overhead de rede perceptível nem comportamento diferente do nativo.
- [ ] Host tem o mod instalado mas algum Cliente não (ou vice-versa) — a raid não pode quebrar; definir o que acontece nesse cenário de instalação assimétrica.
- [ ] Uma tempestade começa muito perto do fim da raid ou de uma extração — não deve haver efeito colateral perceptível na sequência de extração/fim de raid.
- [ ] O FIKA recria sua camada de rede a cada nova raid (não é uma conexão contínua entre raids) — a sincronização de clima precisa voltar a funcionar em toda raid nova, não só na primeira raid da sessão de jogo. <!-- review: comportamento consistente com o critério "Estado entre raids" acima, mas vale destacar como corner case porque é uma falha silenciosa fácil de passar despercebida em teste rápido -->
- [ ] Um pacote de clima corrompido ou malformado (ex: perda parcial de dados na rede) não pode travar, atrasar ou quebrar a sincronização de outros sistemas da raid (posição de outros jogadores, inventário, etc.) — o erro deve ficar contido ao próprio clima.
- [ ] Ao longo de uma raid longa, pequenas diferenças de relógio interno entre Host e Cliente não devem causar divergência perceptível de clima — a sincronização periódica deve corrigir esse desvio antes que fique perceptível.
- [ ] Presença de outro mod que também declare pacotes de rede customizados no FIKA — a sincronização de clima deste mod não deve colidir nem interferir na sincronização de rede desse outro mod.

## Fora de escopo

- [ ] Ciclo natural de estações do ano (Primavera/Verão/Outono/Inverno) controlado por calendário real — item de backlog separado.
- [ ] Otimizações de desempenho de partículas de neve/chuva (redução de partículas, culling em miras óticas) — item de backlog separado.
- [ ] Integração com IA (SAIN) reagindo a clima (visão/audição de bots) — item de backlog separado.
- [ ] Painel de configuração BepInEx (F12) completo — cobrir apenas o necessário para este item funcionar; opções de estações/performance ficam para os itens correspondentes.

## Referências

- [ROADMAP.md — §5 (Motor de Sincronização Contínua) e §6 (Handshake Pré-Raid)](../../ROADMAP.md)
- [docs/investigacao-fika-eft-2026-09-10.md — investigação de código real (handshake FIKA, API de clima do EFT, causa raiz da divergência de tempestade)](../../docs/investigacao-fika-eft-2026-09-10.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Item criado via `/add-backlog-item` |
| 2026-09-10 | Revisão `/review-spec` — 2 gaps + 4 corner cases corrigidos |
