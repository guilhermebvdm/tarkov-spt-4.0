# 013 — Roteamento de Canal QoS por Dono do Inventário · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [013-canal-qos-dono-inventario-01-spec.md](013-canal-qos-dono-inventario-01-spec.md)
**Criado:** 2026-09-23
**Revisado:** 2026-09-23 — conforme decisão em [013-canal-qos-dono-inventario-03-spec-tech-review-01.md](013-canal-qos-dono-inventario-03-spec-tech-review-01.md) (PA-01-01, PA-01-02, PA-01-03)

> Fonte primária: código do FIKA no próprio fork (`mods/FIKA/modded-V2/`) — este item não patcheia o Assembly do EFT, modifica diretamente uma função de roteamento de rede que já é código nosso (introduzida pelo item `007`). Toda referência de linha abaixo foi conferida em `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/` nesta sessão (2026-09-23).

## 1. Estratégia

**Revisão de abordagem (review 01, PA-01-01/PA-01-02):** a primeira versão desta spec técnica decidia o canal comparando o `NetId` do "dono" do inventário-alvo contra o `NetId` de quem enviou o pacote (`actor == target ⇒ canal rápido`). A review 01 encontrou dois casos reais, confirmados no código, em que essa comparação dá o resultado errado:

- O relay do servidor pros demais peers (`FikaServer.SendGenericPacket`) **nunca é um envio 1:1** — confirmado em [`FikaServer.cs:640-649`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L640-L649), o parâmetro `broadcast` nem é lido no corpo de `SendNetReusable`; quem decide os destinatários é só `peerToIgnore` passado pro `_netServer.SendToAll(..., peerToIgnore)` — ou seja, é sempre "todos os peers, menos no máximo um". Se o alvo da operação é o próprio ator que a originou (ex.: jogador A recarregando a própria arma), `actor == target` mandava esse relay pro canal rápido — mas o canal seguro pra cada destinatário depende de **ele já ter processado o `SendCharacter` daquela entidade**, não de quem a operou. Um peer que acabou de invadir a raid (item `010`) e ainda não recebeu o `SendCharacter` de A pode receber essa operação adiantada — o mesmo bug relatado, agora em jogador vivo.
- O callback de volta pro peer que pediu a operação (`SendGenericPacketToPeer`) **é sempre 1:1**, mas carrega o `NetId` do alvo original (que pode ser um terceiro, ex.: um corpo sendo lootado) — `actor == target` dava falso nesse caso e mandava o ACK pro canal lento, mesmo esse envio sendo seguro (o peer só conseguiu pedir a operação porque já tinha aquele inventário carregado localmente). Isso reabriria o sintoma original do item `007` (mãos travadas esperando ACK) pra qualquer operação em inventário de terceiro.

**Abordagem revisada:** o canal correto depende da **topologia do envio**, não de quem é o dono do inventário-alvo:

- **Envio 1:1** (cliente → servidor via `FikaClient.SendGenericPacket`; servidor → o peer que originou o pedido via `FikaServer.SendGenericPacketToPeer`) — sempre seguro pro canal rápido. O servidor é sempre autoritativo/completo (não tem "descoberta" pendente de nenhuma entidade); o peer que originou um pedido só conseguiu fazê-lo porque já tinha aquele inventário — próprio ou de terceiro — carregado localmente.
- **Envio pra múltiplos destinatários** (`FikaServer.SendGenericPacket`, broadcast pra todos os peers menos no máximo um) — sempre canal geral (0), incondicionalmente, pra `InventoryOperation`/`OperationCallback`. Qualquer um dos destinatários pode não ter processado ainda o `SendCharacter` da entidade-alvo (item `010`, catch-up de raid em andamento) — preserva a ordem relativa com esse `SendCharacter`, fechando a corrida relatada tanto pra corpos/bots quanto pra jogadores vivos.

Isso elimina a necessidade de extrair/comparar `NetId` de ator e alvo (a alternativa (a) descartada na versão anterior desta spec — carregar um campo "é meu?" no pacote — segue descartada pelo mesmo motivo: mudaria o formato do pacote). A decisão agora é só "este envio é 1:1 ou é pra múltiplos peers?", informação que cada método já sabe estruturalmente (não precisa ser calculada por chamada):

- `FikaClient.SendGenericPacket` ([Networking/FikaClient.cs:392](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L392)) é sempre 1:1 (cliente só tem um peer: o servidor).
- `FikaServer.SendGenericPacketToPeer` ([Networking/FikaServer.cs:631](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L631)) é sempre 1:1 por definição (usa `SendNetReusableToPeer`, envia só pro `peer` recebido).
- `FikaServer.SendGenericPacket` ([Networking/FikaServer.cs:622](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L622)) é sempre potencialmente-múltiplo (usa `SendNetReusable` → `_netServer.SendToAll`).

**Efeito colateral positivo:** como a decisão não depende mais de `NetId`/ator, `HostInventoryController.cs:168`, `ClientInventoryController.cs:165` e `BotInventoryController.cs:70` (os 3 call sites adicionais de `SendGenericPacket` auditados nesta revisão, todos chamando via `PacketSender.NetworkManager.SendGenericPacket` — a mesma interface `IFikaNetworkManager` implementada por `FikaClient`/`FikaServer`) **não precisam de nenhuma alteração** — o roteamento correto já emerge de qual método (`FikaClient` vs `FikaServer`) eles chamam, sem exigir que cada call site resolva ou passe um `actorNetId`.

## 2. Pontos de patch

Não há alvo no Assembly do EFT — a tabela abaixo lista os pontos de modificação no código do próprio FIKA (fork `modded-V2`).

| Arquivo (FIKA, `modded-V2`) | Tipo | Motivo |
|---|---|---|
| [`Networking/NetworkUtils.cs:187-198`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/NetworkUtils.cs#L187-L198) | Modificação direta | `GetChannelForSubPacket` passa a receber `isSinglePeerSend` (bool) em vez de tipo só; decide por topologia do envio, não por dono do inventário. |
| [`Networking/FikaClient.cs:397`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L397) | Modificação direta | `SendGenericPacket` passa `isSinglePeerSend: true` (cliente→servidor é sempre 1:1). |
| [`Networking/FikaServer.cs:627`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L627) | Modificação direta | `SendGenericPacket` passa `isSinglePeerSend: false` (sempre potencialmente-múltiplo — ver §1). |
| [`Networking/FikaServer.cs:636`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L636) | Modificação direta | `SendGenericPacketToPeer` passa `isSinglePeerSend: true` (sempre 1:1 por definição). |

`HostInventoryController.cs`, `ClientInventoryController.cs` e `BotInventoryController.cs` — auditados nesta revisão, **sem alteração** (ver §1, "Efeito colateral positivo").

## 3. Novas propriedades F12 (BepInEx)

N/A — este item não introduz `ConfigEntry` nova. A decisão de canal é sempre automática por topologia de envio; não há trade-off que o usuário precise escolher — é estritamente uma correção de bug do roteamento existente do item `007`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Fika-Plugin/Fika.Core/Networking/NetworkUtils.cs` | MODIFICAR | `GetChannelForSubPacket` ganha parâmetro `isSinglePeerSend`; `InventoryOperation`/`OperationCallback` vão pro canal rápido só quando `true`. |
| `Fika-Plugin/Fika.Core/Networking/FikaClient.cs` | MODIFICAR | `SendGenericPacket` passa `isSinglePeerSend: true`. |
| `Fika-Plugin/Fika.Core/Networking/FikaServer.cs` | MODIFICAR | `SendGenericPacket` passa `isSinglePeerSend: false`; `SendGenericPacketToPeer` passa `isSinglePeerSend: true`. |
| `Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Bump de versão (SemVer patch, é fix de bug — ver critério "manter fix do item 007"). |
| `Fika-Plugin/Fika.Core/Fika.Core.csproj` | MODIFICAR | `<Version>` acompanha o bump acima. |

## 5. Stubs de código

```csharp
// Networking/NetworkUtils.cs
using System.Runtime.CompilerServices;

namespace Fika.Core.Networking;

public static class NetworkChannels
{
    public const byte ChannelGeneral = 0;
    public const byte ChannelInventory = 1;

    // Canal rápido (1) só em envio 1:1 (cliente→servidor, ou servidor→o peer que
    // originou o pedido) — nesses casos o destinatário já tem contexto garantido
    // sobre a entidade envolvida (é autoritativo, ou já carregou aquele inventário
    // localmente pra poder pedir a operação). Envio pra múltiplos peers (broadcast
    // do servidor) sempre cai no canal geral (0) — a mesma fila do SendCharacter
    // que estabelece cada entidade — porque qualquer um dos destinatários pode
    // ainda não ter processado o SendCharacter daquele alvo (item 010, catch-up de
    // raid em andamento). Ver review 01, PA-01-01/PA-01-02, pra evidência de código
    // de por que "dono do inventário" (comparação de NetId) não é o critério certo.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetChannelForSubPacket(EGenericSubPacketType type, bool isSinglePeerSend)
    {
        switch (type)
        {
            case EGenericSubPacketType.InventoryOperation:
            case EGenericSubPacketType.OperationCallback:
                return isSinglePeerSend ? ChannelInventory : ChannelGeneral;
            default:
                return ChannelGeneral;
        }
    }
}
```

```csharp
// Networking/FikaClient.cs — trecho de SendGenericPacket (assinatura/corpo já existentes, só o cálculo do canal muda)
public void SendGenericPacket(EGenericSubPacketType type, IPoolSubPacket subpacket, bool broadcast = false, NetPeer peerToIgnore = null)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;

    // Cliente só tem um peer (o servidor) — todo envio daqui é 1:1 por definição.
    var channel = NetworkChannels.GetChannelForSubPacket(type, isSinglePeerSend: true);

    SendNetReusable(ref packet, DeliveryMethod.ReliableOrdered, channel, broadcast, peerToIgnore);
}
```

```csharp
// Networking/FikaServer.cs — trechos de SendGenericPacket/SendGenericPacketToPeer
public void SendGenericPacket(EGenericSubPacketType type, IPoolSubPacket subpacket, bool broadcast = false, NetPeer peerToIgnore = null)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;

    // ref: Networking/FikaServer.cs:640-649 — SendNetReusable ignora o parâmetro
    // `broadcast` e envia sempre via SendToAll(peerToIgnore); ou seja, este método
    // é sempre "todos os peers, menos no máximo um" — nunca um envio 1:1 real.
    var channel = NetworkChannels.GetChannelForSubPacket(type, isSinglePeerSend: false);

    SendNetReusable(ref packet, DeliveryMethod.ReliableOrdered, channel, broadcast, peerToIgnore);
}

public void SendGenericPacketToPeer(EGenericSubPacketType type, IPoolSubPacket subpacket, NetPeer peer)
{
    var packet = _genericPacket;
    packet.Type = type;
    packet.SubPacket = subpacket;

    // Sempre 1:1 por definição (SendNetReusableToPeer manda só pro `peer` recebido)
    // — o peer só chega a pedir a operação porque já tinha aquele inventário
    // (próprio ou de terceiro) carregado localmente, então devolver rápido não
    // tem risco de corrida de descoberta (ver review 01, PA-01-02).
    var channel = NetworkChannels.GetChannelForSubPacket(type, isSinglePeerSend: true);

    SendNetReusableToPeer(ref packet, DeliveryMethod.ReliableOrdered, channel, peer);
}
```

## 6. Fluxo de dados

```
Cenário A (jogador recarrega a própria arma — deve continuar no canal rápido pro ACK):
[A] ClientInventoryController → FikaClient.SendGenericPacket(InventoryOperation)
  → isSinglePeerSend=true → ChannelInventory (1) — sem concorrência com SendCharacter
    de bots (canal 0) no trecho cliente→servidor.
[Servidor] OnInventoryPacketReceived → SendGenericPacketToPeer(OperationCallback, peer=A)
  → isSinglePeerSend=true → ChannelInventory (1) — ACK de A chega rápido, watchdog
    (item 002/007) não drena a operação. Fix do item 007 preservado.

Cenário B (jogador A descarrega carregador do rig do corpo netId=42 — bug relatado):
[A] → FikaClient.SendGenericPacket(InventoryOperation{NetId=42}) → isSinglePeerSend=true
  → ChannelInventory (1) — só o trecho cliente→servidor, sem risco (servidor é autoritativo).
[Servidor] SendGenericPacketToPeer(OperationCallback{NetId=42}, peer=A) → isSinglePeerSend=true
  → ChannelInventory (1) — ACK de volta pra A também rápido (A já tinha o corpo 42
    carregado localmente pra poder pedir a operação — sem risco de corrida).
[Servidor] SendGenericPacket(InventoryOperation{NetId=42}, peerToIgnore=peerA) — broadcast
  pros demais peers (B, C, ...) → isSinglePeerSend=false → ChannelGeneral (0) — mesma
  fila ordenada do SendCharacter{NetId=42} que estabelece o corpo pra cada um deles →
  sem corrida, mesmo se B ou C ainda não tiverem processado esse SendCharacter.

Cenário C (jogador A recarrega a própria arma; jogador C acabou de invadir a raid — item 010):
[Servidor] SendGenericPacket(InventoryOperation{NetId=A}, peerToIgnore=peerA) — broadcast
  pros demais peers → isSinglePeerSend=false → ChannelGeneral (0), independente de o
  alvo ser o próprio A — protege C, que pode ainda não ter processado o
  SendCharacter{NetId=A} da rajada de catch-up (item 010). Corrige PA-01-01.
```

## 7. Riscos e dependências

- **Item `007` (patches existentes):** `NetworkChannels.GetChannelForSubPacket` é a mesma função que o item 007 criou — mudança de assinatura, não de arquivo novo. Os 3 call sites (`FikaClient.SendGenericPacket`, `FikaServer.SendGenericPacket`, `FikaServer.SendGenericPacketToPeer`) precisam ser atualizados junto; um esquecido quebra a compilação (assinatura muda), não falha silenciosa.
- **Item `012` (Manodavis):** `ClientInventoryOperationHandler.cs`/`Move`/`SplitOperationDescriptorPatch.cs` não são tocados por este item — o roteamento de canal é ortogonal à lógica de rollback/reconciliação desses patches. Nenhuma dependência direta, mas os critérios de aceite da spec funcional exigem re-teste do cenário original do item 012 como regressão.
- **Item `010` (Invasão):** cenário onde o bug relatado se manifesta com maior probabilidade (rajada de `SendCharacter` durante catch-up de múltiplos corpos/jogadores). Nenhuma mudança de código no item 010; apenas cenário de teste — e, com a revisão desta spec (§1, Cenário C), agora também cobre explicitamente jogadores vivos, não só corpos.
- **Item `011` (AoI/threading) — dependência FECHADA nesta revisão (ver [review 01, PA-01-03](013-canal-qos-dono-inventario-03-spec-tech-review-01.md)):** confirmado por leitura direta do código que o despacho de `PlayerStateData` do item 011 usa `IFikaNetworkManager.SendPlayerState` ([`IFikaNetworkManager.cs:114`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs#L114), implementado em [`FikaClient.cs:381-389`](../../modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L381-L389) e `FikaServer.cs:681`) — um método estruturalmente separado que nunca chama `GetChannelForSubPacket`/`SendGenericPacket`. Os chamadores (`BotPacketSender.cs:61`, `ClientPacketSender.cs:98`, `ServerPacketSender.cs:109`) confirmam isso. Não há mais dependência de thread-safety a verificar pra este item — a nova abordagem (§1) também não usa mais `TryGetPlayerByPeer`/`MyPlayer` pra decidir canal, então mesmo que houvesse sobreposição, não haveria mais estado compartilhado a proteger.
- **`fika-packet-desync-prevention-plan.md` §1/causa 6:** `_dataWriter` compartilhado de `FikaClient`/`FikaServer` já exige envio só na main thread — este item não introduz uma nova violação (a decisão de canal agora é ainda mais simples — só checa `type` e um `bool` fixo por método — sem leitura de estado mutável compartilhado).
- **Compatibilidade com peers de versão antiga:** `GetChannelForSubPacket` é uma função interna, chamada só no processo local que está enviando — a mudança de assinatura não afeta o formato do pacote no fio (mesmo argumento do item 007, §4 da spec-tech original). Um peer com o binário antigo (sem este fix) continua roteando por tipo apenas — não quebra compatibilidade, só não recebe a correção do lado dele.

## 8. Checklist de implementação

- [x] Alterar `NetworkChannels.GetChannelForSubPacket` em `NetworkUtils.cs` para a assinatura `(type, isSinglePeerSend)` com a lógica de topologia de envio.
- [x] Atualizar `FikaClient.SendGenericPacket` para passar `isSinglePeerSend: true`.
- [x] Atualizar `FikaServer.SendGenericPacket` para passar `isSinglePeerSend: false`.
- [x] Atualizar `FikaServer.SendGenericPacketToPeer` para passar `isSinglePeerSend: true`.
- [x] Confirmar que não sobrou nenhum outro call site de `GetChannelForSubPacket` além dos 3 listados (grep no fork inteiro).
- [x] Confirmar que `HostInventoryController.cs`, `ClientInventoryController.cs` e `BotInventoryController.cs` **não precisam de alteração** (nenhum deles chama `GetChannelForSubPacket` diretamente — só `SendGenericPacket` via `PacketSender.NetworkManager`).
- [x] Bump de versão (patch) em `FikaPlugin.cs` e `Fika.Core.csproj` — 2.4.4 → 2.4.5.
- [ ] Build Release (0 erros) isolado no fork `modded-V2` — pendente `/compile-mod`.
- [ ] Validar em raid real: cenário do item 007 (recarga do próprio jogador durante spawn de bot) sem regressão.
- [ ] Validar em raid real: cenário do bug relatado (jogador A descarrega carregador do rig de um corpo; jogador B loota o mesmo rig/item logo depois) sem trava.
- [ ] Validar em raid real: invasão de raid em andamento (item 010) com múltiplos corpos E jogadores vivos existentes, operação de terceiro durante catch-up (cenário C, §6).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Não há estado de lifecycle a gerenciar — a decisão de canal é recalculada a cada envio, sem hook de raid-start/stop próprio. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | O roteamento distingue explicitamente "envio 1:1 (sempre seguro)" de "broadcast pra múltiplos peers (sempre canal geral)" — critério revisado na review 01 (PA-01-01/PA-01-02) após a versão anterior (comparação de `NetId`) falhar exatamente nesse filtro pra broadcasts e callbacks de terceiro (§1, §6). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Não há patch sobre método virtual/ofuscado do EFT; `GetChannelForSubPacket` é função concreta própria do FIKA, chamada diretamente (sem dispatch virtual). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Não muta estado de jogo/inventário do EFT — só decide o `channelNumber` de transporte de um pacote já existente. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | A nova versão não lê nenhum campo de instância (`MyPlayer`, dicionário de peers) pra decidir canal — `isSinglePeerSend` é fixo por método, não depende de estado de raid nenhum. Mais simples que a versão anterior nesse quesito. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Não é patch Harmony; `GetChannelForSubPacket` não se re-invoca. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | Não há cache — `isSinglePeerSend` é uma constante por call site, não um valor lido de contexto mutável. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Todas as refs (`FikaServer.cs:622/631/640-649`, `FikaClient.cs:381-392`, `IFikaNetworkManager.cs:114`, `HostInventoryController.cs:168`, `ClientInventoryController.cs:165`, `BotInventoryController.cs:70`) foram lidas diretamente em `mods/FIKA/modded-V2/` nesta sessão, incluindo na review 01. |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]`) — AP-10 | N/A | Não usa skill do EFT como mecanismo. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + `Valid` + campos resetados + envio só main thread + registro por instância + zero `UnregisterPacket` + airbag com throttle — AP-11 | ✅ | `InventoryPacket`/`OperationCallbackPacket` não têm o formato/serialização alterado (mesmo `Serialize`/`Deserialize`, mesmo nome de tipo/hash) — só o `channelNumber` do transporte muda. A ressalva de "envio só main thread" da versão anterior está **fechada**: confirmado que o item 011 usa `SendPlayerState`, caminho estruturalmente separado de `GetChannelForSubPacket`/`SendGenericPacket` (ver §7, review 01 PA-01-03). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Spec técnica criada via `/create-technical-spec` (versão original: roteamento por comparação de `NetId` ator/alvo) |
| 2026-09-23 | Revisão `/review-technical-spec` 01 encontrou 2 bloqueadores na comparação de `NetId` (PA-01-01: broadcast do servidor fast-pathed incorretamente; PA-01-02: callback de terceiro slow-pathed incorretamente) + 1 gap fechado (PA-01-03: dependência do item 011). Usuário aceitou a sugestão (roteamento por topologia de envio, não por dono do inventário) — spec técnica reescrita (§1, §2, §4, §5, §6, §7, §9) pra refletir a nova abordagem. |
