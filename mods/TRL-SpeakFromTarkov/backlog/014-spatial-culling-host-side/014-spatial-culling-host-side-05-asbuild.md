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

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo (com pelo menos 2 clientes) e compilação (`/compile-mod`) ainda pendentes. |
