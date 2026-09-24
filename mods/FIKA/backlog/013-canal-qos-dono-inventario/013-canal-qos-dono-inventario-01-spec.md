# 013 — Roteamento de Canal QoS por Dono do Inventário

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-23

## Visão geral

O item `007` moveu `InventoryOperation`/`OperationCallback` para um segundo canal de rede (`channelNumber = 1`) mais rápido, isolando-os do tráfego de spawn/estado de personagem (`SendCharacter`, que continua no canal `0`), para resolver ACKs de inventário do próprio jogador atrasados atrás de rajadas de spawn de bot. A separação por **tipo de pacote** não distingue **de quem** é o inventário sendo operado: uma operação em inventário de uma entidade recém-descoberta (bot morto/corpo lootável) pode, pelo canal rápido, chegar ao cliente **antes** do `SendCharacter` (canal lento) que estabelece a existência e o estado inicial daquele inventário. Relatado por usuário em raid real de 2 jogadores após invasão de raid em andamento (item `010`): jogador A descarrega um carregador de uma arma dentro do rig (colete) de um corpo; o jogador B fica com aquele item e aquele rig especificamente bloqueados/intransponíveis para loot, enquanto outros itens no mesmo grid do rig continuam looteáveis normalmente por ambos.

## Comportamento atual

- `NetworkChannels.GetChannelForSubPacket` (`Networking/NetworkUtils.cs:191-198`) roteia por **tipo de sub-pacote**, sem nenhuma noção de dono: `InventoryOperation` e `OperationCallback` sempre vão para `ChannelInventory` (canal `1`); todo o resto (incluindo `SendCharacter`) cai no `default` para `ChannelGeneral` (canal `0`).
- `FikaClient.cs` (`SendGenericPacket`, chamando `GetChannelForSubPacket` em `:397`) e `FikaServer.cs` (`SendGenericPacket`/`SendGenericPacketToPeer`, `:627` e `:636`) aplicam essa mesma função de roteamento independentemente de qual `InventoryController`/entidade é o alvo da operação.
- `RequestSubPackets.cs:387-428` (`RequestCharactersPacket.HandleRequest`) é o caminho que dispara em rajada vários `SendCharacter` (canal `0`) quando um cliente pede "os personagens/corpos que me faltam" — cenário que ocorre tipicamente ao entrar numa raid já em andamento (item `010`), onde múltiplos corpos com loot completo (armas, rigs, munição) podem estar todos "em trânsito" no canal `0` ao mesmo tempo.
- `ReliableOrdered` garante ordem **dentro do mesmo canal**, mas os canais `0` e `1` são filas independentes — não há garantia de ordem relativa entre um `SendCharacter` (canal `0`) que ainda está na fila e um `InventoryOperation`/`OperationCallback` (canal `1`) referente a um item **daquele mesmo corpo**, disparado por outro jogador, que chega antes.
- Resultado observado: uma operação em inventário de terceiro (bot/corpo) que "vence a corrida" contra o `SendCharacter` correspondente deixa esse item especificamente com o estado de bloqueio ("sendo modificado") pendurado para os demais peers, sem nunca receber a liberação correspondente — outros itens do mesmo container, que nunca tiveram operação concorrente, não são afetados.

## Comportamento desejado

- O roteamento de canal passa a considerar **de quem é o inventário** sendo operado, não apenas o tipo do pacote: operações no inventário do **próprio jogador que está enviando o pacote** (o caso original que o item `007` resolveu — arma/carregador na mão ou no corpo do próprio jogador) continuam indo pelo canal rápido (`1`).
- Operações em inventário de **qualquer outra entidade** (bot, corpo lootável, outro jogador) passam a usar o mesmo canal (`0`) que o `SendCharacter`/pacote equivalente que estabeleceu aquele inventário — preservando a ordem relativa entre "esse inventário existe com este conteúdo" e "esta operação aconteceu nele".
- O fix original do item `007` (recarga/swap do próprio jogador não trava mais atrás de rajada de spawn de bot) continua válido e testável da mesma forma.
- A trava de item/rig entre dois jogadores operando o mesmo corpo deixa de ocorrer.
- Nenhuma mudança de formato/hash de pacote é introduzida — o roteamento por dono é decidido no momento do envio, olhando o alvo da operação já disponível no pacote, sem adicionar campos novos ao payload.

## Critérios de aceite

- [ ] Um pedido de swap/reload no inventário do **próprio jogador** enviado durante uma rajada de spawn de bots continua recebendo o ACK sem ser drenado pelo watchdog por atraso de fila (regressão do fix do item `007`).
- [ ] Jogador A realiza uma operação (ex.: descarregar carregador) num item dentro do rig de um corpo; Jogador B consegue lootear esse mesmo rig e esse mesmo item imediatamente depois, sem bloqueio pendurado.
- [ ] Durante e depois da operação do critério anterior, outros itens no mesmo grid do rig permanecem looteáveis por ambos os jogadores (comportamento que já funcionava e não pode regredir).
- [ ] Ao invadir uma raid em andamento (item `010`) com múltiplos corpos existentes, uma operação de outro jogador em qualquer um desses corpos — mesmo enquanto a rajada de `SendCharacter` de catch-up ainda está em trânsito — não deixa item bloqueado permanentemente.
- [ ] **Fika/multiplayer:** validado com 2+ jogadores reais operando simultaneamente itens diferentes dentro do mesmo corpo (bot morto), e com o cenário de invasão de raid em andamento do item `010` — sem trava de item nem regressão do fix de recarga do item `007`.
- [ ] **Estado entre raids:** N/A — roteamento de canal é decidido por pacote/sessão de rede, não introduz estado persistente entre raids.

## Corner cases

- [ ] Dois jogadores operam, ao mesmo tempo, dois itens **diferentes** dentro do mesmo corpo (ambas operações em inventário de terceiro, ambas no canal `0`) — não deve haver trava além do comportamento padrão já existente do EFT para modificação concorrente do mesmo container.
- [ ] Não é possível determinar com segurança, no momento de montar o pacote, a quem pertence o inventário-alvo (ex.: referência nula/estado transitório) — o roteamento deve cair para o canal seguro (`0`), nunca para o canal rápido por omissão, para não reabrir a janela de corrida.
- [ ] Jogador realiza uma operação no **próprio** inventário (canal rápido) no exato instante em que também está em andamento uma operação sua em inventário de **terceiro** (canal lento) — confirmar que não há inversão de ordem indevida entre as duas quando ambas partem do mesmo jogador.
- [ ] Corpo cujo `SendCharacter` nunca chegou a ser solicitado pelo jogador local (ex.: fora do raio de interesse/AoI do item `011`) recebe uma operação de outro jogador — comportamento deve degradar de forma segura (sem exceção, sem trava permanente), não necessariamente "funcionar" sem o corpo ser conhecido localmente.
- [ ] Contêiner sem jogador/bot associado (baú, caixa estacionária, contêiner de mapa) — não tem "dono" no sentido de `InventoryController` de uma entidade viva, mas ainda assim não é o inventário do jogador local; deve cair na mesma regra do canal lento (`0`), não ser tratado como um terceiro caso à parte nem cair no canal rápido por não bater no critério "é de outra entidade".
- [ ] Jogador extrai, morre ou fica MIA (fim de raid) com uma operação de inventário em terceiro (canal `0`) ainda em trânsito no momento da transição — `IFikaNetworkManager` é destruído e recriado entre sessões (ver `docs/technical/fika-packet-desync-prevention-plan.md` §1 e o corner case equivalente do item `007`); confirmar que a mudança de roteamento não introduz um caminho novo de vazamento de callback/timestamp pendente para a próxima raid.
- [ ] Interação com outro mod do mesmo escopo que também dispara operações de inventário via swap direto (`UIFixes`, mesmo mod que expôs o desync do item `012`, caso "Manodavis") — confirmar que a troca rápida de item continua sendo roteada corretamente conforme o dono (próprio jogador → canal `1`) sem reabrir esse desync.

<!-- review: decisão/verificação técnica necessária antes do /create-technical-spec — o item `011` descarrega o despacho de `PlayerStateData` (posição/estado de bots) para uma Background Worker Thread, e a própria doc do item 011 afirma que esse caminho NÃO passa por serialização em `NetDataWriter` (portanto, a princípio, não usa `SendGenericPacket`/`GetChannelForSubPacket` nem o `_dataWriter` compartilhado descrito em `spt-mod-best-practices` §9). Essa afirmação vem da spec do item 011, não foi reconfirmada linha a linha nesta sessão. A spec técnica deste item PRECISA verificar isso contra o código atual antes de assumir que a extração de "dono do inventário" só roda na Main Thread — se o worker thread do item 011 alguma vez chamar esse caminho, a extração de dono (e o `_dataWriter` compartilhado) precisa ser thread-safe. -->

<!-- review: ✅ FECHADO em 2026-09-23 via [013-canal-qos-dono-inventario-03-spec-tech-review-01.md](013-canal-qos-dono-inventario-03-spec-tech-review-01.md) PA-01-03 — confirmado por leitura direta do código que `PlayerStateData` do item 011 usa `IFikaNetworkManager.SendPlayerState` (`FikaClient.cs:381-389`, `FikaServer.cs:681`), método estruturalmente separado que nunca chama `GetChannelForSubPacket`/`SendGenericPacket`. Sem dependência de thread-safety a verificar; a spec técnica revisada também deixou de depender de estado mutável (`MyPlayer`/dicionário de peers) pra decidir canal, o que torna a questão ainda mais moot. -->

- [ ] Interação com o item `011` (AoI/multithreading): confirmar que a extração de "quem é o dono deste inventário" não depende de dado só disponível na `Main Thread` de um jeito que force marshaling adicional ou quebre o caminho existente de despacho em background.

## Fora de escopo

- [ ] Resolver o bug, ainda sob investigação, de itens do slot `TacticalVest` de um corpo aparentemente se acumulando com os de outro corpo ao looter vários corpos em sequência (suspeita atual: buffer estático compartilhado de serialização de perfil em `FikaSerializationExtensions.cs`, não relacionado a roteamento de canal — instrumentação de diagnóstico já em `modded-V2`, aguardando teste em raid real). Item de backlog separado quando confirmado.
- [ ] Mudanças no item `010` (Join In Progress / Invasão) ou no item `012` (fix "Manodavis" de recarga) além do necessário para não regredi-los.
- [ ] A definir: revisitar se `ItemPacket` (mencionado apenas na spec técnica original do item `007` como candidato, mas não implementado como tal no roteamento atual) deveria ter uma regra de dono equivalente — decisão de escopo para a spec técnica deste item.

## Referências

- [007-fila-inventario-bloqueada-spawn-bot/](../007-fila-inventario-bloqueada-spawn-bot/) — item que introduziu a segregação de canais; este item refina a regra de roteamento sem reverter o ganho original.
- [012-desync-recarga-swap-uifixes-auto-cura/](../012-desync-recarga-swap-uifixes-auto-cura/) — não deve ser afetado por este item.
- [010-join-in-progress-senha-lobby-headless/](../010-join-in-progress-senha-lobby-headless/) — cenário (invasão de raid) em que o bug foi reportado, expõe a janela de corrida com maior probabilidade por trazer rajadas maiores de `SendCharacter`.
- [docs/technical/fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) — guia canônico de rede FIKA; candidato a atualização quando este item fechar, documentando a regra de roteamento por dono.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Item criado via `/add-backlog-item`, a partir de bug relatado em raid real de 2 jogadores (item/rig de corpo bloqueado para um jogador após o outro operar um item dentro dele) e investigação de código que identificou o roteamento de canal do item `007` como causa — separa por tipo de pacote, não por dono do inventário-alvo |
| 2026-09-23 | Revisão `/review-spec` — 3 corner cases adicionados (fim de raid com operação de terceiro em trânsito, contêiner sem dono-entidade, interação com UIFixes/item 012) + 1 trecho marcado `<!-- review: --> ` pedindo verificação técnica (se o worker thread do item 011 usa o mesmo caminho de envio que este item precisa tornar dono-aware) |
