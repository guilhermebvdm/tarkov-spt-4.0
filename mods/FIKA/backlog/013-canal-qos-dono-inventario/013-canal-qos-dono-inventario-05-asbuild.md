# 013 — Roteamento de Canal QoS por Dono do Inventário · As-Built

**Mod:** FIKA
**Spec funcional:** [013-canal-qos-dono-inventario-01-spec.md](013-canal-qos-dono-inventario-01-spec.md)
**Spec técnica:** [013-canal-qos-dono-inventario-02-spec-tech.md](013-canal-qos-dono-inventario-02-spec-tech.md)
**Última review técnica:** [013-canal-qos-dono-inventario-03-spec-tech-review-01.md](013-canal-qos-dono-inventario-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-23

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/NetworkUtils.cs` | `NetworkChannels.GetChannelForSubPacket` ganha parâmetro `isSinglePeerSend`; `InventoryOperation`/`OperationCallback` só vão pro canal rápido (1) quando o envio é 1:1 — canal geral (0) incondicional pra broadcast. |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs` | `SendGenericPacket` passa `isSinglePeerSend: true` (cliente→servidor é sempre 1:1). |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs` | `SendGenericPacket` passa `isSinglePeerSend: false` (broadcast sempre potencialmente-múltiplo); `SendGenericPacketToPeer` passa `isSinglePeerSend: true` (sempre 1:1 por definição). |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Bump de versão `FikaVersion` 2.4.4 → 2.4.5 (patch, fix de bug). |
| MODIFICADO | `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Fika.Core.csproj` | `<Version>` acompanha o bump acima (2.4.5). |

`HostInventoryController.cs`, `ClientInventoryController.cs` e `BotInventoryController.cs` foram auditados (3 call sites adicionais de `SendGenericPacket` via `PacketSender.NetworkManager`) e **não precisaram de alteração** — o roteamento correto emerge de qual método (`FikaClient`/`FikaServer`) eles chamam, sem exigir que cada call site resolva um `actorNetId`.

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | `FikaServer.SendGenericPacket` (broadcast) deixou de comparar `NetId` de ator/alvo — agora usa `isSinglePeerSend: false` incondicional pra `InventoryOperation`/`OperationCallback`, sempre canal geral. Fecha a corrida pra jogador vivo em invasão de raid (item 010). |
| PA-01-02 | C — Erro de Lógica · 🔴 | `FikaServer.SendGenericPacketToPeer` deixou de comparar `NetId` — agora usa `isSinglePeerSend: true` incondicional, sempre canal rápido. O ACK de uma operação em inventário de terceiro (ex.: loot de corpo) não fica mais preso atrás de rajada de spawn de bots. |
| PA-01-03 | A — Gap · 🟢 | Dependência do item 011 fechada na própria spec técnica (§7) com evidência de código (`SendPlayerState` é caminho separado) — nenhuma mudança de código adicional necessária; a nova abordagem também não lê mais estado mutável (`MyPlayer`/dicionário de peers) pra decidir canal, tornando a questão ainda mais moot. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-23 | Build concluído via `/code-mod` — `Fika.Core` v2.4.5 (fork `modded-V2`) |
