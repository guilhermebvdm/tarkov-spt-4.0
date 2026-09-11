---
title: "TRL-SpeakFromTarkov — Investigação: Canal LiteNetLib Dedicado para VOIP"
date: 2026-09-09
status: 🟢 Vivo
authors: Claude
---

# Investigação: vale a pena um canal LiteNetLib dedicado para o VOIP?

Resposta dirigida ao Passo 3 do pedido de auditoria: reconfirmação com evidência própria dos 8 pontos levantados numa sessão anterior, mais achados novos, sobre usar o canal 1 (ocioso) do LiteNetLib do FIKA para separar o tráfego de voz do canal 0 compartilhado.

**Resposta curta: não vale a pena.** Ver seção "Recomendação" no final.

---

## (a) Reconfirmação dos 8 pontos originais

Todos os 8 pontos foram reconfirmados diretamente no código-fonte atual de `references/fika-plugin/` e `modded-V4/Network/SftNetwork.cs` (idêntico a `modded-V3-audit/`). Nenhum mudou desde a sessão anterior.

1. ✅ `IFikaNetworkManager.SendData<T>(ref T, DeliveryMethod, bool broadcast)` não tem parâmetro de canal — confirmado em `references/fika-plugin/Fika.Core/Networking/IFikaNetworkManager.cs:77`.
2. ✅ `FikaClient.SendData`/`FikaServer.SendData` chamam `peer.Send(data, deliveryMethod)` (2 args, sem canal) — `FikaClient.cs:375`, `FikaServer.cs:612`.
3. ✅ FIKA sobe o LiteNetLib com `ChannelsCount = 2` — `FikaClient.cs:145`, `FikaServer.cs:160`. Canal 1 existe, alocado, ocioso.
4. ✅ `NetPeer.Send(data, byte channelNumber, DeliveryMethod)` é um overload público que aceita canal explícito — `references/fika-plugin/Fika.Core/Networking/LiteNetLib/NetPeer.cs:184`.
5. ✅ `FikaClient.ServerConnection` (o `NetPeer` cru) é público — `FikaClient.cs:56`.
6. ✅ `FikaClient.OnNetworkReceive`/`FikaServer.OnNetworkReceive` recebem `channelNumber` mas **não o usam** na dispatch — decidem só por `EPacketType` lido do início dos bytes (`FikaClient.cs:494-530`, `FikaServer.cs:929-961`). Mandar no canal 1 não quebraria a leitura no destino.
7. ✅ `_dataWriter`/`_packetProcessor` são campos `private` em ambas as classes — confirmado (`FikaServer.cs:124,129`). Reaproveitar o canal exigiria reimplementar a serialização manualmente.
8. ✅ Em `SftNetwork.cs:242-243`, todo frame de áudio é enviado com `broadcast: true`, sempre, sem filtro de distância no envio. O filtro por distância (`HandleVoipPacket`, `SftNetwork.cs:414-424`) roda **depois** do pacote já ter atravessado a rede — só economiza CPU/decodificação no destino, não banda.

---

## Achados novos (além dos 8 pontos originais)

### N1 — FIKA tem uma API de voz dedicada (`SendVOIPData`/`EPacketType.VOIP`), mas é um beco sem saída
`IFikaNetworkManager.SendVOIPData(ArraySegment<byte> data, DeliveryMethod, NetPeer peer)` existe (`IFikaNetworkManager.cs:122`) e usa um `EPacketType.VOIP` próprio, separado de `EPacketType.Serializable`. Parecia promissor — mas no lado de recepção, `EPacketType.VOIP` é **hardcoded** para alimentar exclusivamente `VOIPClient.NetworkReceivedPacket()`/`VOIPServer.NetworkReceivedPacket()` (`FikaClient.cs:523-528`, `FikaServer.cs:953-958`), que são os objetos do Dissonance (motor de voz nativo do FIKA). O próprio TRL-SpeakFromTarkov já **desliga esse pipeline de propósito**: `GameSessionPatcher.cs:106-142` (`FikaClientInitializeVoipPatch`/`FikaServerInitializeVoipPatch`) faz `InitializeVOIP()` retornar `Task.CompletedTask` sem nunca instanciar `VOIPClient`/`VOIPServer`. Ou seja: com o mod ativo, `VOIPClient`/`VOIPServer` são sempre `null`, e esse canal nativo de voz não tem como ser reaproveitado sem também patchear o `OnNetworkReceive` privado do FIKA — mais internals, não menos.

### N2 — O relay do host SEMPRE força canal 0, independente do canal de origem
Este é o achado que mais pesa contra a ideia do canal 1. VOIP é enviado com `broadcast: true`, o que no FIKA significa: cliente manda pro host, host repassa pra todos os outros clientes (topologia estrela, não é mesh). Confirmei que esse repasse do host é feito por `FikaServer.OnNetworkReceive` (`FikaServer.cs:932-936`):
```csharp
if (reader.GetByte() == 1) // byte de broadcast
{
    _netServer.SendToAll(reader.GetRemainingBytesSpan(), deliveryMethod, peer);
}
```
E `_netServer.SendToAll(ReadOnlySpan<byte>, DeliveryMethod, LiteNetPeer)` é o overload **sem** canal, herdado de `LiteNetManager` (`LiteNetManager.cs:1228-1234`), cujo próprio doc-comment diz literalmente **"(channel - 0)"** — internamente chama `netPeer.Send(data, options)`, o mesmo overload de 2 argumentos que também hardcoda canal 0 (`LiteNetPeer.cs:632-633`).

**Conclusão prática:** mesmo que um cliente mandasse a própria voz pro host usando o canal 1 de verdade (via `NetPeer.Send(data, 1, deliveryMethod)`, chamada direta e pública), o **host repassaria essa voz pros outros jogadores sempre no canal 0** — porque a lógica de relay do FIKA (privada, não pode ser trocada sem Harmony patch) não preserva nem lê o canal de origem. Pra canal 1 funcionar de ponta a ponta (cliente→host→outros clientes), seria necessário Harmony-patchear a dispatch de recepção **e** a lógica de relay do `FikaServer`/`FikaClient` — uma dependência de internals não documentados maior do que o ponto 7 original já indicava sozinho.

### N3 — O "ganho" do item 01 do backlog (Opus DTX/VBR) já está, na prática, conquistado por outro caminho
Verificado em `modded-V4/Audio/VoipProcessor.cs:45`: `encoder.UseVBR = true` já está ativo, com o comentário explícito "sem DTX que quebra o Concentus C#" — ou seja, `UseDTX` nativo foi **deliberadamente rejeitado** por um bug de compatibilidade conhecido com a biblioteca Opus usada aqui (Concentus, implementação 100% C#), não por falta de implementação.

Mais importante: o objetivo de banda do DTX (não gastar rede durante silêncio) **já é 100% alcançado** por outro mecanismo — `UpdateTransmittingState()` (`VoipProcessor.cs:94-159`) só chama `Transmit()` quando `IsTransmitting == true` (gate por VAD/PTT/Open). Durante silêncio, **nenhum pacote é enviado**, não só um pacote menor (que é tudo que o DTX nativo faria). Isso é estritamente melhor do que o DTX do Opus teria entregado. **O ganho real de "implementar o item 01" é próximo de zero.**

---

## (b) Vale a pena implementar o canal 1 real?

**Não.** Considerando N2, o trade-off original (banda vs. fragilidade) já era desfavorável — agora sabemos que o "preço" descrito no ponto 7 (reimplementar só a serialização de envio) estava subestimado: seria necessário reescrever também a lógica de recepção **e** de relay do host, ambas privadas e não documentadas, em ambas as pontas (`FikaClient` e `FikaServer`). Isso significa:
- Harmony-patchear (ou reimplementar via Reflection) `FikaServer.OnNetworkReceive` para reconhecer e re-relay corretamente pacotes de canal 1.
- Reimplementar manualmente o formato de bytes de **duas direções diferentes** (cliente→servidor inclui um byte de broadcast antes do `EPacketType`; servidor→cliente não inclui) — confirmado nos dois métodos `SendData` de `FikaClient.cs:365-377` vs `FikaServer.cs:606-613`.
- Qualquer patch/versão nova do FIKA pode quebrar isso silenciosamente, sem aviso, já que nada disso é API pública.

O ganho de banda também é questionável: separar em canal 1 não reduz a **quantidade** de bytes trafegados — só evita que o VOIP fique na mesma fila de ordenação/entrega que outros pacotes custom do canal 0. Dado que VOIP já usa `DeliveryMethod.Unreliable` (não bloqueia/não é bloqueado por pacotes `ReliableOrdered` de outros sistemas do jogo da mesma forma que um canal `ReliableOrdered` compartilhado seria), o cenário de "congestionamento" que motivou a pergunta original é mais teórico do que comprovado no código.

## (c) Comparação com os itens 01 (DTX/VBR) e 06 (Spatial Culling Host-Side) do backlog

| Item | Estado real | Ganho de banda | Risco | Depende de internals do FIKA? |
|---|---|---|---|---|
| Canal 1 LiteNetLib | Não implementado | Nenhum (não reduz bytes, só isolamento de fila) | Alto — precisa patchear recepção e relay privados, nas duas direções | Sim, pesado |
| Item 01 (DTX/VBR) | **VBR já ativo; DTX já é desnecessário** (gate de silêncio já corta 100% do envio) | Próximo de zero (objetivo já alcançado por outro caminho) | N/A | Não |
| Item 06 (Spatial Culling Host-Side) | **Não implementado** — filtro de distância só roda no destino, depois do pacote já ter sido enviado a todos | **Real e proporcional** — hoje o host retransmite a voz de cada jogador pra TODOS os outros, sempre; limitar a retransmissão a quem está dentro do alcance de audição corta banda de uplink do host proporcionalmente à fração de jogadores fora de alcance (ex.: numa raid de 10, se em média 5-6 estão fora do alcance de 60m a qualquer momento, o host deixa de retransmitir mais da metade do tráfego de voz) | Baixo — usa `SendDataToPeer` (API pública documentada, mesma usada em outros pontos do próprio mod) | Não |

**Confirmação da sua leitura:** sim, os itens 01 e 06 têm ganho maior e risco muito menor que o canal 1 — mas por um motivo ainda mais forte do que o esperado: o item 01 não é mais necessário (objetivo já alcançado), e o item 06 é o único dos três com ganho de banda real ainda não capturado.

## (d) Recomendação final

- **Canal LiteNetLib dedicado: fechar o assunto, não implementar.** Ganho de banda nulo, risco alto (dependência de internals privados do FIKA em 2 métodos, 2 direções). Documentado aqui como decisão tomada — não reabrir sem uma mudança de arquitetura do FIKA que exponha essas APIs publicamente.
- **Item 01 (DTX/VBR):** fechar como "já resolvido por design" — o backlog pode ser atualizado para refletir que o gate de transmissão (VAD/PTT/Open) já entrega o resultado que o DTX daria, e que `UseDTX` nativo continua desabilitado por incompatibilidade conhecida com o Concentus.
- **Item 06 (Spatial Culling Host-Side): é o único que vale a pena perseguir.** Baixo risco, ganho de banda real e mensurável, usa API pública (`SendDataToPeer`). Requer rodar essa lógica apenas no host (checar `IFikaNetworkManager` é servidor vs cliente) e reescrever o envio de `SftNetwork.DrainSendQueue()` para, no host, iterar peers e decidir por distância antes de repassar — ao invés do atual `broadcast: true` único. Este é o único item de rede desta investigação que deveria virar um item de backlog formal (`/add-backlog-item` → `/create-spec` → `/create-technical-spec` → `/review-technical-spec` antes de qualquer código, por ser mudança em rede).
