# 007 — ACK de operação de inventário bloqueado por spawn de bots no mesmo canal de rede

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-11

## Visão geral

Toda operação de inventário do FIKA (pedido de swap/move e o ACK de confirmação do servidor) e todo broadcast de spawn/estado de personagem (bot ou jogador) trafegam pelo mesmo par `(canal 0, DeliveryMethod.ReliableOrdered)` do LiteNetLib — mesmo havendo um segundo canal já provisionado (`ChannelsCount = 2`) e nunca usado. `ReliableOrdered` garante entrega ao app **na ordem de envio dentro do mesmo par (canal, delivery method)**; uma rajada de pacotes de spawn de bot enfileirada antes do ACK de uma operação de inventário atrasa esse ACK mesmo que o servidor não esteja de fato sobrecarregado. Combinado com o watchdog de timeout do item `002` (5s), isso produz um falso `EOperationStatus.Failed` para uma operação que na verdade já tinha sido confirmada (ou seria, se não tivesse sido drenada antes). Relatado por usuário em raid real: troca de carregador por swap direto (arma já municiada → outro carregador) disparou a animação de recarga corretamente, mas o item ficou piscando indefinidamente no inventário — sintoma idêntico ao descrito na spec do item `002` — até fechar/reabrir o inventário manualmente. O log da raid mostrou a sequência completa: `OperationCallback` (id 42) expirado pelo watchdog (`"Network operation timed out"`) bem no meio de uma rajada de `PuppetMaster setup complete` (spawn de vários bots), seguido do erro de "status mismatch" entre servidor (`Failed`, forçado pelo watchdog) e cliente (`Succeeded`, resultado real da execução local).

## Comportamento atual

- `ChannelsCount = 2` é configurado em ambos os lados (`FikaClient.cs:146`, `FikaServer.cs:161`), mas nenhuma chamada de envio no projeto passa `channelNumber` explícito — todas usam o default (`channelNumber = 0`, `LiteNetLib/LiteNetManager.cs:361`). O segundo canal nunca é usado na prática.
- `SendGenericPacket`/`SendGenericPacketToPeer` — o transporte usado tanto pelo cliente quanto pelo servidor para `EGenericSubPacketType.InventoryOperation`, `.OperationCallback` e `.SendCharacter` (entre outros) — estão hardcoded em `DeliveryMethod.ReliableOrdered`:
  - `FikaClient.cs:392-398` (`SendGenericPacket`)
  - `FikaServer.cs:621-635` (`SendGenericPacket` e `SendGenericPacketToPeer`)
- `InventoryOperationHandler.cs:51,61` envia o ACK (`OperationCallback`) de volta ao peer por esse mesmo caminho. `FikaServer.Callbacks.cs:856,866,869,881` idem. `FikaServer.Callbacks.cs:629` envia `SendCharacter` (dados de spawn de bot/jogador) pelo mesmo `SendGenericPacketToPeer`.
- <!-- review: gap — o levantamento de código encontrou bem mais tipos usando `SendGenericPacket`/`SendGenericPacketToPeer` do que só `SendCharacter`: `UpdateBackendData` (`FikaServer.cs:777,880`), `ClientConnected`/`ClientDisconnected` (`FikaServer.cs:875`, `FikaServer.Callbacks.cs:387`), sincronização de stashes (`FikaServer.cs:762`, `FikaServer.Callbacks.cs:655`), quest sync (`FikaServer.Callbacks.cs:673`), pacotes de lâmpada/janela/interativo/arremessável (`FikaServer.Callbacks.cs:534-595`) e resync (`FikaServer.Callbacks.cs:860,885`). Todos competem pelo mesmo canal 0. Este item cita só `SendCharacter` como exemplo por ser o que coincidiu no log real, não porque seja o único candidato — a spec técnica precisa decidir o corte exato entre "crítico para gameplay" (fica isolado) e "resto" (pode continuar no canal 0), não só mover `SendCharacter`. -->
- Resultado: pedido de swap de inventário, ACK de confirmação e broadcast de spawn de personagem (e o resto do tráfego genérico listado acima) competem pela mesma fila ordenada. Numa rajada de spawns (vários bots entrando na raid ao mesmo tempo), o ACK de uma operação de inventário enviada nesse intervalo pode ficar enfileirado atrás dos pacotes de spawn, mesmo que o servidor já tenha processado a operação.
- O watchdog do item `002` (`FikaPlayer.cs:85-192`, `CallbackTimeoutSeconds = 5.0f`) não distingue "servidor realmente não respondeu" de "resposta está na fila atrás de outro tráfego" — aos 5s, drena o callback e força `EOperationStatus.Failed` (`FikaPlayer.cs:123-131`).
- Em `ClientInventoryOperationHandler.HandleResult` (`:87-132`), quando o `serverStatus` forçado pelo watchdog (`Failed`) diverge do `localStatus` real da operação (`Succeeded`, já executada localmente antes do timeout), o resultado final entregue ao callback é sobrescrito para o `Failed` do watchdog (`:90-93`), mesmo a operação já tendo sido aplicada com sucesso no cliente.
- `ReceiveStatusFromServer` (`:41-76`) só dispara o refresh visual de recuperação (`RaiseRefreshEvent`) quando `Operation is MoveOperationClass` (`:56-68`). Não há tratamento equivalente para `SwapOperationClass` — hipótese ainda não confirmada contra o assembly descompilado (não gerado nesta máquina) para explicar por que o item citado pelo usuário ficou "piscando" até um refresh manual do inventário, em vez de se recuperar sozinho como o `MoveOperationClass` faz.

## Comportamento desejado

- Tráfego de inventário crítico para gameplay (`InventoryOperation`, `OperationCallback`) deixa de competir com broadcast de spawn/estado de personagem (`SendCharacter` e afins) pela mesma fila `ReliableOrdered`, usando o segundo canal já provisionado (`channelNumber = 1`) ou outra separação equivalente a definir na spec técnica.
- Uma operação de inventário cujo ACK estava apenas atrasado na fila (não realmente perdido) não é mais tratada como falha pelo watchdog quando a resposta real chega — ou, no mínimo, o desalinhamento visual resultante (item "piscando" indefinidamente) é corrigido sem exigir fechar/reabrir o inventário manualmente.
- Nenhuma mudança de formato/hash de pacote é introduzida (separar por canal é uma propriedade de transporte, não de conteúdo) — compatibilidade com peers de outras versões do mod é preservada.
- <!-- review: decisão humana — a autoridade do servidor sobre o resultado final da operação (`ClientInventoryOperationHandler.HandleResult`, `:90-93`: um `Failed` que chega depois de um `Succeeded` local sobrescreve o resultado) NÃO deve ser enfraquecida por este item. `Strict Inventory Sync` (`PROPRIEDADES.md` §9) existe justamente para forçar confirmação do servidor antes de liberar ações — suprimir esse override pra "confiar no resultado local" evitaria o sintoma, mas abriria desync real (cliente mostra item trocado, servidor não confirmou). A separação de canal ataca a CAUSA do atraso, não a lógica de autoridade — confirmar que a spec técnica não tenta "consertar" o mismatch mudando `HandleResult`. -->

## Critérios de aceite

- [ ] Um pedido de swap de inventário (ex: troca de carregador arma-com-carregador ↔ colete) enviado durante uma rajada de spawn de bots recebe o ACK do servidor sem ser drenado pelo watchdog por atraso de fila — validado com instrumentação temporária de latência real do ACK nesse cenário.
- [ ] Quando o watchdog ainda assim expira um callback (timeout genuíno, ex: desconexão real), o item afetado não fica "piscando" indefinidamente no inventário — reflete o estado final corretamente sem exigir fechar/reabrir o inventário.
- [ ] `SendCharacter` (ou o tráfego de spawn que for movido) continua chegando de forma ordenada e completa em todos os peers — mover para outro canal não pode quebrar a própria sincronização de spawn.
- [ ] `node scripts/check-packet-hashes.js` continua reportando 0 colisões após a mudança (nenhum tipo/nome de pacote novo introduzido — mudança é de canal de transporte, não de conteúdo).
- [ ] **Fika/multiplayer:** validado especificamente em raid Headless com spawn de múltiplos bots em sequência (cenário que reproduziu o bug original) e com 2+ jogadores reais trocando itens de inventário simultaneamente — nenhuma regressão de ordenação entre canais diferentes.
- [ ] **Estado entre raids:** N/A — separação de canal é configuração de transporte por sessão de rede, não introduz estado persistente entre raids.

## Corner cases

- [ ] Timeout genuíno (perda real de conexão, não atraso de fila) — o watchdog deve continuar funcionando como rede de segurança; a mudança não pode mascarar uma desconexão real como "só atrasado".
- [ ] Duas ou mais operações de inventário do mesmo jogador em sequência rápida (ex: troca de carregador seguida de outra ação de inventário) — ordenação relativa entre elas deve ser preservada mesmo isoladas do tráfego de spawn.
- [ ] Sessão sem bots (ex: raid PvP-only ou `No AI` ativo no F12) — comportamento não deve regressão; a separação de canal deve ser neutra quando não há rajada de spawn.
- [ ] Peer com versão antiga do FIKA (sem a separação de canal) conectando a um Host com a mudança aplicada, ou vice-versa — confirmar que não há suposição implícita de canal do lado que não foi atualizado (avaliar se isso é sequer um cenário suportado pelo mod, dado o guia de compatibilidade em `docs/technical/fika-packet-desync-prevention-plan.md`).
- [ ] Rajada de spawn tão grande que também sature o canal separado — a mudança reduz a chance de colisão, não a elimina estruturalmente; documentar isso explicitamente para não ser vendido como fix definitivo do watchdog.
- [ ] Raça exata entre o watchdog expirando um callback e o `OperationCallback` real chegando no mesmo frame/janela — confirmar qual dos dois vence de forma determinística e que o perdedor não deixa `OperationCallbacks`/`_operationCallbackTimestamps` (`FikaPlayer.cs`) num estado inconsistente (entrada removida por um lado, timestamp ainda presente pelo outro, ou vice-versa).
- [ ] Jogador extrai, morre ou MIA (fim de raid) com uma operação de inventário ainda pendente no watchdog no momento da transição — `IFikaNetworkManager` é destruído e recriado entre sessões (`fika-packet-desync-prevention-plan.md` §1); confirmar que nenhum callback/timestamp pendente vaza para a próxima raid nem lança exceção durante o teardown.
- [ ] Sessão sem `IFikaNetworkManager` instanciado (singleplayer/hideout, fora de coop) — a separação de canal não deve ser acionada nem quebrar esses contextos, já que não há rede FIKA ativa.

## Fora de escopo

- [x] Recalibrar `CallbackTimeoutSeconds` (hoje `5.0f`) — achado relacionado já registrado como `CR-01-03` (item `002`, débito técnico não aplicado). Este item ataca a causa de fila compartilhada, não o valor do timeout; a spec técnica pode decidir se faz sentido combinar os dois, mas não presumir isso aqui.
- [x] Confirmar contra o assembly descompilado se `SwapOperationClass` recebe (ou deveria receber) o mesmo tratamento de `RaiseRefreshEvent` que `MoveOperationClass` em `ClientInventoryOperationHandler.cs:56-68` — decompile não gerado nesta máquina (`bash scripts/decompile-eft.sh`); necessário antes da spec técnica para confirmar essa hipótese como causa do "piscando", não deste item de spec funcional.
- [ ] A definir: qual subconjunto do tráfego genérico migra para o canal separado. Além de `InventoryOperation`/`OperationCallback`/`SendCharacter`, o levantamento encontrou `UpdateBackendData`, `ClientConnected`/`ClientDisconnected`, sync de stashes, quest sync, pacotes de lâmpada/janela/interativo/arremessável e resync — todos no mesmo canal 0 hoje (ver `<!-- review -->` em Comportamento atual). Decisão de desenho para a spec técnica, não deste item de spec funcional.

## Referências

- [002-inventory-desync-watchdog/](../002-inventory-desync-watchdog/) — watchdog de timeout de 5s cujo comportamento de auto-drenar interage diretamente com o achado deste item; `CR-01-03` (canal/timeout configurável) é achado relacionado não aplicado.
- [003-magazine-swap-inplace-fix/](../003-magazine-swap-inplace-fix/) — fix anterior de rejeição de swap de carregador in-place; mesmo tipo de operação (`SwapOperationClass`) que reproduziu o bug relatado aqui.
- [docs/technical/fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) — guia canônico de rede FIKA; cobre `ParseException`/derrubada de fila de frame (causas 1-6), mas não documenta hoje o comportamento de `channelNumber`/head-of-line blocking dentro do mesmo canal — candidato a atualização quando este item fechar.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-11 | Item criado via `/add-backlog-item`, a partir de bug relatado em raid real (item de inventário piscando indefinidamente após swap de carregador) e investigação de código que encontrou o ACK de operação de inventário compartilhando o canal 0 `ReliableOrdered` com broadcast de spawn de bot/jogador, apesar de um segundo canal já provisionado e nunca usado |
| 2026-09-11 | Revisão `/review-spec` — 1 gap de escopo (lista real de tipos que competem pelo canal 0 é maior que só `SendCharacter`) + 4 corner cases adicionados (raça watchdog-vs-ACK real, fim de raid com operação pendente, sessão sem `IFikaNetworkManager`, e o gap de escopo refletido em "Fora de escopo") + 1 trecho marcado `<!-- review -->` pedindo decisão humana (preservar autoridade do servidor em `HandleResult`, não tentar resolver o mismatch mudando essa lógica) |
