# 014 — Spatial Culling Host-Side · As-Built

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [014-spatial-culling-host-side-01-spec.md](014-spatial-culling-host-side-01-spec.md)
**Spec técnica:** [014-spatial-culling-host-side-02-spec-tech.md](014-spatial-culling-host-side-02-spec-tech.md)
**Última review técnica:** [014-spatial-culling-host-side-03-spec-tech-review-01.md](014-spatial-culling-host-side-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod` em `modded-V4/`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Network/SftNetwork.cs` | `DrainSendQueue()` bifurcado por canal + papel (host relay direto / convidado `broadcast:false` / canais 1-2 inalterados); novo método `RelayVoiceToNearbyPeers` (itera `GameWorld.RegisteredPlayers`, `SendDataToPeer` por jogador dentro do alcance); `OnReceiveVoipDataV2` chama o relay quando o host recebe voz de um convidado. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica, já resolvidos na spec técnica antes deste build.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 Bloqueador | `RelayVoiceToNearbyPeers` usa `GameWorld.RegisteredPlayers` (não `AllAlivePlayersList`) — implementado corretamente, preservando a escuta dupla do espectador. |
| PA-01-02 | A — Gap · 🟢 Menor | `ObservedPlayer : FikaPlayer` confirmado — o cast `candidate is FikaPlayer` no código real funciona sem ressalvas. |

## Detalhe além da spec técnica

O stub original da spec técnica tinha o `try/catch` de `RelayVoiceToNearbyPeers` envolvendo o laço inteiro — implementado com o `try/catch` **dentro** do laço, só em volta da chamada `SendDataToPeer`, pra que uma falha de envio isolada num peer não aborte o relay pros demais peers dentro do alcance.

## Mudanças posteriores

### 2026-09-12 — Regressão real encontrada no primeiro teste em jogo (3 jogadores)

**Sintoma reportado pelo usuário:** com host (A) + 2 convidados (B, C) numa raid: A ouvia B e C normalmente; B não ouvia ninguém; C ouvia B; nem B nem C ouviam A.

**Causa raiz:** `RelayVoiceToNearbyPeers` resolvia o `NetPeer` de cada convidado via `manager.GetPeerById(fikaPlayer.NetId)`. `FikaPlayer.NetId` é um **id de jogo** atribuído pelo FIKA (host sempre `1`, convidados `2`, `3`, ... — `Fika.Core/Networking/FikaServer.cs:207-208`), enquanto `GetPeerById(int id)` do LiteNetLib indexa o array interno de **conexões de transporte**, numerado pela ordem em que os sockets conectaram (`LiteNetLib/LiteNetManager.HashSet.cs:119-122`). Os dois espaços de números não têm nenhuma relação — o lookup ora "acertava" um peer errado (ou nenhum) por pura coincidência de quem conectou em que ordem, explicando o padrão assimétrico observado. Isso não foi pego na review técnica porque nenhuma das fontes consultadas (Assembly EFT, FIKA) documentava essa diferença explicitamente; só ficou evidente com múltiplos clientes reais conectados simultaneamente.

**Correção aplicada:** `SftAudioPacketV2` passou a ser registrado com o overload `RegisterPacket<T, NetPeer>` (`IFikaNetworkManager.cs:137`), que entrega o `NetPeer` real de quem enviou cada pacote direto pro handler. `OnReceiveVoipDataV2` cacheia esse `NetPeer` num novo `Dictionary<string, NetPeer> _profileIdToPeer` (chave = ProfileId), populado a cada pacote de voz recebido no host. `RelayVoiceToNearbyPeers` passou a resolver o peer por esse dicionário em vez de `GetPeerById(NetId)`. Dicionário limpo em `StopSession()` (higiene entre raids, AP-01). Bônus: `candidate == senderPlayer` (comparação `IPlayer`/`Player`, gerava warning `CS0252`) trocado por `ReferenceEquals` — 0 avisos no build.

**Limitação conhecida:** o cache só é populado quando o convidado já enviou pelo menos 1 pacote de voz nesta raid. Se o HOST falar antes de qualquer convidado ter falado, essa primeira fala do host pode não alcançar quem ainda não abriu o microfone. Na prática irrelevante (voz é contínua, o cache populariza no primeiro segundo de uso real), mas registrado como gap conhecido — não reaberto sem evidência de impacto real.

**Versão:** `1.6.0 → 1.6.1` (patch — correção de regressão, não feature nova).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo (com pelo menos 2 clientes) e compilação (`/compile-mod`) ainda pendentes. |
| 2026-09-12 | Build local re-executado (`dotnet build` direto, sem `/compile-mod` — usuário instala manualmente) após correção da regressão de resolução de `NetPeer` descrita acima. Versão `1.6.1`. Novo teste com múltiplos convidados ainda pendente. |
