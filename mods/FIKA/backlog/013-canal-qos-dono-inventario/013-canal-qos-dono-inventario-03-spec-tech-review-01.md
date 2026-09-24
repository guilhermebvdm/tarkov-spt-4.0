# 013 — Roteamento de Canal QoS por Dono do Inventário · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [013-canal-qos-dono-inventario-02-spec-tech.md](013-canal-qos-dono-inventario-02-spec-tech.md)
**Data:** 2026-09-23

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.
>
> **Memória consultada:** `mods/FIKA/memory/sessions.md` — snapshot desatualizado (última entrada 2026-09-14, Sessão 10); não cobre os itens `007`/`011`/`012` (2026-09-15) nem este item `013` (criado nesta mesma sessão, 2026-09-23). Nenhuma pendência registrada especificamente sobre este item. Pendências que afetam: nenhuma.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-01-01](#pa-01-01--c--erro-de-lógica--🔴-bloqueador) | C | 🔴 | Broadcast do host fast-pathed por `actor==target` reabre a corrida para jogador vivo | ✅ Resolvido 2026-09-23 |
| [PA-01-02](#pa-01-02--c--erro-de-lógica--🔴-bloqueador) | C | 🔴 | Callback de operação em inventário de terceiro roteado pro canal lento — regride o item 007 | ✅ Resolvido 2026-09-23 |
| [PA-01-03](#pa-01-03--a--gap--🟢-menor) | A | 🟢 | Dependência do item 011 (`<!-- review -->` da spec funcional) — resolvida com evidência nova | ✅ Resolvido 2026-09-23 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · ✅ Resolvido em 2026-09-23

**Broadcast do host fast-pathed por `actor==target` reabre a corrida para jogador vivo (não só corpo/bot)**

**Problema:** O stub de `GetChannelForSubPacket` (`013-canal-qos-dono-inventario-02-spec-tech.md:75-99`) decide o canal comparando `targetNetId == actorNetId`, e esse mesmo cálculo é usado tanto pra sends 1:1 quanto pro broadcast do servidor (`FikaServer.SendGenericPacket`, stub em `013-canal-qos-dono-inventario-02-spec-tech.md:121-133`). Conferi o call site real do relay em [`Networking/FikaServer.Callbacks.cs:865-871`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L865-L871):

```csharp
var handler = _inventoryOperationHandlerPool.Get();
handler.Set(result, packet.CallbackId, packet.NetId, peer, this);
SendGenericPacketToPeer(EGenericSubPacketType.OperationCallback,
        OperationCallbackPacket.FromValue(packet.NetId, packet.CallbackId, EOperationStatus.Started), peer);

SendGenericPacket(EGenericSubPacketType.InventoryOperation, packet,
    true, peer);
```

`peer` aqui é sempre quem originou a operação (ex.: jogador A). Pelo design da spec técnica, `actorNetId = TryGetPlayerByPeer(peerToIgnore=peer) → NetId(A)`. Se A executa uma operação no **próprio** inventário (`packet.NetId == NetId(A)`), a comparação dá `targetNetId == actorNetId` → `ChannelInventory` (canal rápido) — só que esse `SendGenericPacket` aqui **não é um envio 1:1 pra A**, é o broadcast pra **todos os outros peers** (B, C, ...), excluindo A. Também bati o mesmo padrão em [`HostInventoryController.cs:168-169`](../../modded-V2/Fika-Plugin/Fika.Core/Main/HostClasses/HostInventoryController.cs#L168-L169) e [`BotInventoryController.cs:70-71`](../../modded-V2/Fika-Plugin/Fika.Core/Main/BotClasses/BotInventoryController.cs#L70-L71) — ambos chamam `SendGenericPacket(..., broadcast: true)` sem `peerToIgnore`, e confirmei em [`FikaServer.cs:640-649`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L640-L649) que o parâmetro `broadcast` do `SendNetReusable` do servidor **nem é lido no corpo do método** — quem decide os destinatários é só `peerToIgnore` passado pro `_netServer.SendToAll(..., peerToIgnore)`. Ou seja: **todo `FikaServer.SendGenericPacket` é, por construção, um envio pra N peers** (N ou N-1), nunca um 1:1.

O canal seguro pra um destinatário depende de **ele já ter processado o `SendCharacter` daquela entidade**, não de quem executou a operação. "Ser o próprio ator" é uma propriedade de quem *enviou*, irrelevante pra quem *recebe* num broadcast.

**Por que importa:** Reproduz a corrida exata que este item existe pra fechar, mas para **qualquer jogador vivo**, não só corpos/bots. Cenário concreto: jogador A recarrega a própria arma; o host relay do `InventoryOperation` (linha 870-871 acima) vai pro canal rápido (1) porque `target(A) == actor(A)`; se o jogador C acabou de invadir a raid em andamento (item `010`) e ainda não processou o `SendCharacter{NetId=A}` (canal 0, pode estar atrás de uma rajada de spawn de bots), C recebe a operação de A antes de saber que A existe — o mesmo bug relatado (item travado/bloqueado), agora batendo em jogador vivo, não em corpo. O corner case do item 010 na spec funcional já reconhece esse risco para corpos; o mesmo mecanismo se aplica a jogadores.

**Sugestão:** Trocar o critério de "dono do inventário" (`actor == target`) por **topologia do envio**, que é o que realmente determina segurança de reordenação:

- `FikaClient.SendGenericPacket` (cliente → servidor): sempre canal rápido, sem comparação nenhuma — o servidor já é autoritativo e completo sobre todas as entidades, não existe corrida de "descoberta" nesse sentido.
- `FikaServer.SendGenericPacketToPeer` (servidor → o peer que originou o pedido): sempre canal rápido, sem comparação — esse peer só conseguiu disparar a operação porque já tinha aquele inventário carregado localmente (própria UI ou loot já aberto), então não há risco de corrida de descoberta pra ele.
- `FikaServer.SendGenericPacket` com broadcast pra múltiplos peers (`InventoryOperation`/`OperationCallback`): **sempre canal geral (0)**, incondicionalmente — não interessa quem foi o ator, o que importa é que qualquer um dos N destinatários pode não ter processado o `SendCharacter` daquele alvo ainda.

Isso elimina a necessidade de `actorNetId`/comparação de `NetId` inteiramente no caminho de broadcast — `NetworkChannels.GetChannelForSubPacket` não precisa mais receber `subpacket`/`actorNetId`; basta o `FikaServer.SendGenericPacket` (broadcast) passar sempre `ChannelGeneral` pra `InventoryOperation`/`OperationCallback`, e `FikaClient.SendGenericPacket`/`FikaServer.SendGenericPacketToPeer` sempre `ChannelInventory` pra esses tipos. Isso também torna PA-01-02 resolvido pelo mesmo caminho (ver abaixo) e remove a fragilidade de `TryGetPlayerByPeer(peerToIgnore)`/`MyPlayer` como fonte de verdade pro roteamento — o `peerToIgnore=null` de `HostInventoryController.cs:168`/`BotInventoryController.cs:70` deixa de ser um caso especial, porque o broadcast já é sempre canal geral independente do ator.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicada em `013-canal-qos-dono-inventario-02-spec-tech.md` (§1, §5, §6 Cenário C) — `GetChannelForSubPacket` passa a decidir por topologia do envio (`isSinglePeerSend`), não por comparação de `NetId`. `FikaServer.SendGenericPacket` (sempre potencialmente-múltiplo, ver PA-01-01) passa `isSinglePeerSend: false` incondicionalmente para `InventoryOperation`/`OperationCallback`.

---

### PA-01-02 · C — Erro de Lógica · ✅ Resolvido em 2026-09-23

**Callback de operação em inventário de terceiro (ex.: loot de corpo) seria roteado pro canal lento — regride o item 007 pra esse caso**

**Problema:** O stub de `SendGenericPacketToPeer` (`013-canal-qos-dono-inventario-02-spec-tech.md:135-147`) resolve `actorNetId = TryGetPlayerByPeer(peer, out var actor) ? actor.NetId : -1`, onde `peer` é sempre **quem pediu** a operação (confirmado em [`FikaServer.Callbacks.cs:857-858` e `:867-868`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L857-L868): `OperationCallbackPacket.FromValue(packet.NetId, ...)` enviado de volta pro mesmo `peer`). Mas `OperationCallbackPacket.NetId` carrega o **alvo** da operação original (`packet.NetId`), que pode ser um corpo/terceiro (ex.: `NetId=42`), não o `NetId` de quem pediu (`NetId(A)=5`). Como `targetNetId(42) != actorNetId(5)`, a comparação do stub roteia esse callback — que é um envio **1:1 direto pro peer que pediu** — pro `ChannelGeneral` (canal lento), mesmo sendo tecnicamente seguro (A já tinha acesso à UI daquele corpo pra disparar a operação).

**Por que importa:** Confirmei em [`Main/Players/FikaPlayer.cs:65-77`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L65-L77) que `WaitingForCallback` (o gate que trava `HandsController.CanRemove()`, linha `:1449`) depende só de `OperationCallbacks.Count > 0` — um dicionário genérico por `id`, sem distinção de alvo. Ou seja, o watchdog do item `002`/`007` trava as mãos do jogador **pra qualquer operação pendente**, incluindo looting de terceiro. Se o callback de "descarreguei o carregador do corpo 42" for roteado pro canal lento e ficar atrás de uma rajada de `SendCharacter` de spawn de bots, o jogador fica travado (`WaitingForCallback=true`) até o timeout (`CallbackTimeoutSeconds`) auto-drenar — reproduzindo o sintoma original do item 007, só que disparado por loot de terceiro em vez de recarga própria.

**Sugestão:** Mesma correção de topologia da PA-01-01: `SendGenericPacketToPeer` é sempre 1:1 pro peer que originou o pedido — não precisa de comparação de `NetId` nenhuma, sempre `ChannelInventory` pra `InventoryOperation`/`OperationCallback`. O peer só chega a pedir uma operação sobre uma entidade que ele já tem localmente (própria arma ou container já aberto), então devolver a confirmação rápido não tem risco de corrida de descoberta, seja o alvo o próprio jogador ou um terceiro.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicada em `013-canal-qos-dono-inventario-02-spec-tech.md` (§1, §5, §6 Cenário B) — `FikaServer.SendGenericPacketToPeer` passa `isSinglePeerSend: true` incondicionalmente, sem comparação de `NetId`.

---

### PA-01-03 · A — Gap · ✅ Resolvido em 2026-09-23

**Dependência do item 011 (`<!-- review -->` da spec funcional) — resolvida com evidência nova**

**Problema:** A spec funcional (`01-spec.md:46`) e a spec técnica §7 (`02-spec-tech.md:173`) deixam em aberto se o worker thread do item `011` chama `SendGenericPacket`/`GetChannelForSubPacket` fora da main thread — o que exigiria tornar a resolução de `actorNetId` thread-safe. Verifiquei agora contra o código atual: o despacho de `PlayerStateData` usa um método **inteiramente separado**, `SendPlayerState`, declarado em [`IFikaNetworkManager.cs:114`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs#L114) e implementado em [`FikaClient.cs:381-389`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L381-L389) e `FikaServer.cs:681`. O corpo desse método escreve `EPacketType.PlayerState` direto no `_dataWriter` e envia via `DeliveryMethod.Unreliable` **sem nunca chamar `NetworkChannels.GetChannelForSubPacket` nem `SendGenericPacket`**. Os chamadores confirmados — [`Main/PacketHandlers/BotPacketSender.cs:61`](../../modded-V2/Fika-Plugin/Fika.Core/Main/PacketHandlers/BotPacketSender.cs#L61), `ClientPacketSender.cs:98`, `ServerPacketSender.cs:109` — todos chamam `NetworkManager.SendPlayerState(ref state)`, não `SendGenericPacket`.

**Por que importa:** Isso fecha, com evidência de código (não só a afirmação da doc do item 011), a dependência que a spec técnica §7 e §9-check-11 marcavam como "ressalva" não fechada. O caminho que este item `013` modifica (`GetChannelForSubPacket`/`SendGenericPacket`) é estruturalmente isolado do caminho de `PlayerStateData` do item 011 — não há necessidade de tornar a resolução de ator thread-safe pra cobrir esse caso.

**Sugestão:** Atualizar `02-spec-tech.md` §7 e §9-check-11 substituindo a ressalva "não está fechada" por essa evidência (`SendPlayerState` é método distinto, sem interseção com `GetChannelForSubPacket`), e remover/fechar o `<!-- review -->` de `01-spec.md:46` citando esta review. Não bloqueia `/code-mod` — é um fechamento, não uma nova pendência.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** `02-spec-tech.md` §7 e §9-check-11 atualizados citando esta review; marcador `<!-- review: -->` em `01-spec.md:46` fechado com pointer pra `PA-01-03`.
