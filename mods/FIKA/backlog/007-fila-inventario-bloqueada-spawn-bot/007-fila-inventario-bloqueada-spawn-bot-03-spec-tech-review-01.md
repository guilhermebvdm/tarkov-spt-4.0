# 007 — ACK de Operação de Inventário Bloqueado por Spawn de Bots · Review Técnica 01

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Funcional:** [007-fila-inventario-bloqueada-spawn-bot-01-spec.md](007-fila-inventario-bloqueada-spawn-bot-01-spec.md)  
**Spec Técnica:** [007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md](007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md)  
**Data da Review:** 2026-09-15  
**Parecer Geral:** 🟢 APROVADO COM PONTOS DE ATENÇÃO  

---

## 1. Avaliação Crítica de Arquitetura & Riscos Mapeados

### ⚠️ RT-01: Dependência de Causalidade entre Spawn de Entidades (Canal 0) e Interação de Inventário (Canal 1)
- **Cenário:** Se um bot spawna no Canal 0 e um jogador tenta interagir com os itens do bot via Canal 1, existe risco de uma operação de inventário chegar no servidor antes que o cliente tenha conhecimento do bot?
- **Análise:** No EFT, a interface de saque (*Loot UI*) só pode ser aberta pelo cliente após o personagem/corpo existir fisicamente na cena da Unity e o raycast de interação ser executado localmente. Isso significa que o pacote `SendCharacter` obrigatoriamente já chegou e foi instanciado pelo cliente antes que qualquer ação de inventário naquele item possa ser emitida. Logo, a separação de canais não quebra a ordem causal do jogo.

### ⚠️ RT-02: Varredura de Chamadas Diretas sem `SendGenericPacket`
- **Ponto de Atenção:** A revisão deve garantir que nenhuma operação de inventário seja enviada por outros métodos (ex: `SendDataToPeer` ou serializações ad-hoc).
- **Verificação:** Todas as operações de inventário mapeadas no FIKA passam por `EGenericSubPacketType.InventoryOperation` e `EGenericSubPacketType.OperationCallback`. Centralizar a seleção de canal no `NetworkChannels.GetChannelForSubPacket(type)` dentro de `SendGenericPacket` e `SendGenericPacketToPeer` cobre 100% dos fluxos identificados.

### ⚠️ RT-03: Manutenção do `ChannelsCount = 2`
- **Ponto de Atenção:** O LiteNetLib lançará `ArgumentOutOfRangeException` se `channelNumber >= ChannelsCount`.
- **Verificação:** Ambos `FikaClient.cs` e `FikaServer.cs` já definem `ChannelsCount = 2` em suas rotinas de inicialização (`Init`). A spec utiliza apenas os canais `0` e `1`, mantendo-se estritamente dentro do limite configurado.

---

## 2. Conclusão da Review

A segregação do Canal 1 para inventário é a solução ideal e canônica para eliminar o *Head-of-Line Blocking* no LiteNetLib, sem introduzir custos de CPU nem alterar a estrutura dos pacotes.
