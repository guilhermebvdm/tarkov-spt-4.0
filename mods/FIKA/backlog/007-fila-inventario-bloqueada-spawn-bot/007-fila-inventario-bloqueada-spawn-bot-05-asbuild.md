# 007 — ACK de Operação de Inventário Bloqueado por Spawn de Bots · As-Built

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Versão Entregue:** `2.4.4`  
**Spec Funcional:** [007-fila-inventario-bloqueada-spawn-bot-01-spec.md](007-fila-inventario-bloqueada-spawn-bot-01-spec.md)  
**Spec Técnica:** [007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md](007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md)  
**Review Técnica:** [007-fila-inventario-bloqueada-spawn-bot-03-spec-tech-review-01.md](007-fila-inventario-bloqueada-spawn-bot-03-spec-tech-review-01.md)  
**Code Review:** [007-fila-inventario-bloqueada-spawn-bot-04-code-review-01.md](007-fila-inventario-bloqueada-spawn-bot-04-code-review-01.md)  
**Data:** 2026-09-15  
**Status:** 🟢 Concluído e Validado em Build  

---

## 1. Resumo Executivo da Entrega

Este item resolveu a ocorrência de itens "piscando" na interface ou sofrendo falso timeout de inventário (5 segundos no Watchdog) durante ondas intensas de spawn de bots e PMCs no FIKA Coop.

A causa raiz era o fenômeno de **Head-of-Line Blocking** na camada de transporte: os pacotes volumosos de sincronização de bots (`SendCharacter`, contendo perfis completos, peças modulares de armas e armaduras) eram enfileirados no mesmo canal ordenado (`ReliableOrdered`, Canal 0) que as mensagens de `InventoryOperation` e `OperationCallback`.

A solução implementada ativou o **Canal 1** do LiteNetLib (já alocado por `ChannelsCount = 2`), criando uma faixa expressa de alta prioridade e baixa latência (QoS) para o tráfego de inventário.

---

## 2. Componentes e Arquivos Alterados

### 2.1 [NetworkUtils.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/NetworkUtils.cs)
- Adicionada a classe estática `NetworkChannels` com constantes:
  - `ChannelGeneral = 0`: canal padrão para tráfego geral (spawn de personagens, VOIP packets secundários, clima, stashes).
  - `ChannelInventory = 1`: canal prioritário exclusivo para inventário.
- Implementado `GetChannelForSubPacket(EGenericSubPacketType type)` com `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:
  - Mapeia `EGenericSubPacketType.InventoryOperation` e `EGenericSubPacketType.OperationCallback` para `ChannelInventory` (Canal 1).
  - Mapeia todos os demais tipos para `ChannelGeneral` (Canal 0).

### 2.2 [FikaClient.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaClient.cs)
- Atualizado `SendGenericPacket`:
  - Obtém o canal via `NetworkChannels.GetChannelForSubPacket(type)`.
  - Encaminha para `SendNetReusable` com o `channelNumber` correspondente.
- Adicionada sobrecarga de `SendNetReusable` aceitando `byte channelNumber`, invocando `_netClient.SendToAll(_dataWriter, channelNumber, deliveryMethod)`.
- Mantida sobrecarga padrão `SendNetReusable` com 4 parâmetros redirecionando para `channelNumber = 0`.

### 2.3 [FikaServer.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs)
- Atualizado `SendGenericPacket` e `SendGenericPacketToPeer`:
  - Ambos consultam `NetworkChannels.GetChannelForSubPacket(type)`.
  - Direcionam os callbacks autoritativos de inventário para o Canal 1.
- Adicionadas sobrecargas em `SendNetReusable` e `SendNetReusableToPeer` aceitando `byte channelNumber`, invocando:
  - `_netServer.SendToAll(_dataWriter, channelNumber, deliveryMethod, peerToIgnore)`
  - `peer.Send(_dataWriter, channelNumber, deliveryMethod)`
- Mantidas as sobrecargas legadas apontando para Canal 0.

### 2.4 Controle de Versão (SemVer)
- [FikaPlugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Plugin/FikaPlugin.cs): atualizado para `public const string FikaVersion = "2.4.4";`.
- [Fika.Core.csproj](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Fika.Core.csproj): atualizado para `<Version>2.4.4</Version>`.

---

## 3. Verificação e Build

- **Comando:** `dotnet build -c Release`
- **Diretório:** `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/`
- **Resultado:** 0 Erros, build isolado em `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`.
- **Integridade de Sistema:** Nenhuma DLL foi copiada para o caminho de instalação do SPT (`D:/SPT`), cumprindo estritamente as diretrizes de isolamento de build.
