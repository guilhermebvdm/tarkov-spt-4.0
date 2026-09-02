---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 01: Networking Core & LiteNetLib Transport)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 01: Networking Core & LiteNetLib Transport)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático detalhado e minucioso da **Partição 1 (Mecanismos de Rede, LiteNetLib, Sockets UDP, Pooling e NAT Punching)** do código original do mod **FIKA**, inspecionando ~10.000 linhas de código C# distribuídas no módulo `Fika.Core/Networking/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 2 | Memory leaks por falta de teardown em `FikaServer.OnDestroy()` e `FikaClient.OnDestroy()`, retendo instâncias de `Player`, `Profile`, `GameWorld` e Tasks de background entre raids. |
| 🟠 **Alto** | 1 | Vazamento de handlers de inventário por falta de descarte/limpeza do pool `InventoryOperationHandlerPool`. |
| 🟡 **Médio** | 3 | Acessos não defensivos a Singletons em callbacks UDP (AP-02), churn de GC em `SerializeHealthInfo` (`MemoryStream`/`BinaryWriter`) e alocações de LINQ. |
| 🔵 **Baixo** | 1 | Código morto e classes órfãs sem uso (`ArraySegmentPooling`, `_bufferWriter`, `WriterPoolManager`). |
| 💡 **Otimização** | 1 | Otimização de cadência de atualização de estado em `Update()` atrelada ao `SendRate` configurado. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-01-01` | 🔴 Crítico | [`FikaServer.cs:L585-605`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L585-L605) | Memory Leak | Falta de limpeza de listas de `ObservedPlayers`, dicionários de perfis e cancelamento de `CancellationTokenSource` no teardown do Host. |
| `AUD-01-02` | 🔴 Crítico | [`FikaClient.cs:L350-363`](../../original/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L350-L363) | Memory Leak | Falta de limpeza de `ObservedPlayers` e fila de `_inventoryOperations` no teardown do Cliente Peer. |
| `AUD-01-03` | 🟠 Alto | [`InventoryOperationHandlerPool.cs:L5`](../../original/Fika-Plugin/Fika.Core/Networking/Pooling/InventoryOperationHandlerPool.cs#L5) | Memory Leak | Pool de handlers de operação de inventário implementa `IDisposable` mas nunca é liberado nem limpo. |
| `AUD-01-04` | 🟡 Médio | [`FikaServer.Callbacks.cs:L258`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L258) | AP-02 (Defensiva) | Acessos a `Singleton<GameWorld>.Instance` em múltiplos callbacks de pacotes sem checagem de nulo prévia. |
| `AUD-01-05` | 🟡 Médio | [`FikaSerializationExtensions.cs:L544`](../../original/Fika-Plugin/Fika.Core/Networking/FikaSerializationExtensions.cs#L544) | GC Pressure | `SerializeHealthInfo` instancia `MemoryStream`, `BinaryWriter` e `ToArray()` no Heap a cada serialização de vida. |
| `AUD-01-06` | 🟡 Médio | [`FikaNotificationManager.cs:L203`](../../original/Fika-Plugin/Fika.Core/Networking/Websocket/FikaNotificationManager.cs#L203) | GC Pressure | Alocações repetidas de `.ToList()` e instanciação de `new Random()` em manipuladores de notificação e eventos. |
| `AUD-01-07` | 🔵 Baixo | [`ArraySegmentPooling.cs:L11`](../../original/Fika-Plugin/Fika.Core/Networking/Pooling/ArraySegmentPooling.cs#L11) | Código Morto | Classe utilitária `ArraySegmentPooling` e buffers estáticos órfãos sem nenhum chamador no grafo do mod. |
| `AUD-01-08` | 💡 Otimização | [`FikaServer.cs:L546-570`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L546-L570) | Desempenho | Iteração frame-a-frame de `ObservedPlayers.ManualStateUpdate()` no `Update()` em vez de sincronizada ao `SendRate`. |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Memory Leak no Teardown do Host (`FikaServer.OnDestroy`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaServer.cs:L585-605`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L585-L605)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `OnDestroy()` encerra o `_netServer` e limpa `_genericPacket`, mas **não limpa** as listas `ObservedPlayers`, `_visualProfiles`, `_cachedConnections`, e **não cancela** o `_cts` (`CancellationTokenSource`) disparado na inicialização para o `NatIntroduceTask`.
- **Impacto Técnico Real:** Todas as instâncias de `ObservedPlayer` (com seus `PlayerBones`, `HealthController`, componentes Unity e referências a `Profile` e `GameWorld`) permanecem retidas na memória RAM após a saída da raid. A task de background do NAT continua viva indefinidamente.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:*
    ```csharp
    private void OnDestroy()
    {
        _netServer?.Stop();
        _genericPacket.Clear();
        PoolUtils.ReleaseAll();
        if (_fikaChat != null) Destroy(_fikaChat);
        if (_raidAdminUIScript != null) Destroy(_raidAdminUIScript);
        BotInventoryOperationHandlerPool.Clear();
        FikaEventDispatcher.DispatchEvent(new FikaNetworkManagerDestroyedEvent(this));
    }
    ```
  - *Abordagem Otimizada:*
    ```csharp
    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _netServer?.Stop();
        _genericPacket.Clear();

        ObservedPlayers?.Clear();
        _visualProfiles?.Clear();
        _cachedConnections?.Clear();
        _inventoryOperationHandlerPool?.Dispose();

        PoolUtils.ReleaseAll();

        if (_fikaChat != null) Destroy(_fikaChat);
        if (_raidAdminUIScript != null) Destroy(_raidAdminUIScript);

        BotInventoryOperationHandlerPool.Clear();
        FikaEventDispatcher.DispatchEvent(new FikaNetworkManagerDestroyedEvent(this));
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · Memory Leak no Teardown do Cliente Peer (`FikaClient.OnDestroy`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaClient.cs:L350-363`](../../original/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L350-L363)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** No fechamento da sessão do cliente, `ObservedPlayers` não é limpo, e a fila `_inventoryOperations` (`Queue<BaseInventoryOperationClass>`) mantém objetos de operação com referências ao modelo de inventário do EFT.
- **Impacto Técnico Real:** Acúmulo de instâncias de operadores remotos e descritores de inventário não coletados pelo Garbage Collector a cada raid jogada como cliente.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:*
    ```csharp
    private void OnDestroy()
    {
        _netClient?.Stop();
        _genericPacket.Clear();

        ObservedPlayers?.Clear();
        _inventoryOperations?.Clear();
        _missingIds?.Clear();

        PoolUtils.ReleaseAll();

        if (_fikaChat != null) Destroy(_fikaChat);
        FikaEventDispatcher.DispatchEvent(new FikaNetworkManagerDestroyedEvent(this));
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · Falta de Descarte do Pool `InventoryOperationHandlerPool`
- **Severidade:** 🟠 Alto
- **Localização:** [`InventoryOperationHandlerPool.cs:L5`](../../original/Fika-Plugin/Fika.Core/Networking/Pooling/InventoryOperationHandlerPool.cs#L5) e [`FikaServer.cs:L174`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L174)
- **Causa Raiz:** A classe `InventoryOperationHandlerPool` herda de `PacketPool<InventoryOperationHandler>`, que por sua vez implementa `IDisposable`. No entanto, quando `FikaServer` é destruído, o pool não é descartado, mantendo a stack com 8 instâncias de handlers que possuem referências para `NetPeer` e buffers de rede.
- **Impacto Técnico Real:** Retenção de objetos de pooling no Heap com ponteiros obsoletos para sockets e instâncias de servidor desativadas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Inserir chamada de `_inventoryOperationHandlerPool?.Dispose()` dentro de `FikaServer.OnDestroy()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-04 · Acessos Não Defensivos a Singletons em Callbacks UDP
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaServer.Callbacks.cs:L164, L258, L419, L429, L443`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L258) e [`FikaClient.Callbacks.cs:L151, L178, L188, L433, L442`](../../original/Fika-Plugin/Fika.Core/Networking/FikaClient.Callbacks.cs#L151)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-02`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** Múltiplos métodos de recebimento de pacotes invocam `Singleton<GameWorld>.Instance.TransitController`, `Singleton<GameWorld>.Instance.BtrController` e `Singleton<GameWorld>.Instance.RunddansController` diretamente, sem validar se `Singleton<GameWorld>.Instantiated` é verdadeiro ou se a propriedade `Instance` retornou nula.
- **Impacto Técnico Real:** Se pacotes UDP forem entregues logo após a extração ou durante a transição de carregamento da cena (quando o `GameWorld` é destruído), a rotina dispara `NullReferenceException` não tratada, poluindo o console e podendo travar a thread de rede.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar guarda defensiva `if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance is { } gameWorld)` antes de acessar os subsistemas.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-05 · Churn de GC por Alocação em `SerializeHealthInfo`
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaSerializationExtensions.cs:L544-586`](../../original/Fika-Plugin/Fika.Core/Networking/FikaSerializationExtensions.cs#L544-L586)
- **Causa Raiz:** O método `SerializeHealthInfo` cria `new MemoryStream()`, `new BinaryWriter(stream)` e retorna `stream.ToArray()` a cada chamada de sincronização de vida de jogadores.
- **Impacto Técnico Real:** Aloca 3 objetos temporários no Heap por execução em múltiplos pontos de sincronização de rede (`RequestSubPackets.cs:414`, `FikaServer.Callbacks.cs:612`, `CoopHandler.cs:430`).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Serializar diretamente no `NetDataWriter` pré-alocado ou reutilizar um buffer binário estático via `BinaryWriter` estático reciclado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-06 · Alocações de LINQ e `new Random()` em Handlers
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaNotificationManager.cs:L203`](../../original/Fika-Plugin/Fika.Core/Networking/Websocket/FikaNotificationManager.cs#L203), [`MineEvent.cs:L35`](../../original/Fika-Plugin/Fika.Core/Networking/Packets/Generic/SubPackets/MineEvent.cs#L35)
- **Causa Raiz:** Em `FikaNotificationManager.cs`, `DevelopersList.ToList()[new System.Random().Next(...)].Key` aloca uma nova `List` e uma nova instância de `Random` a cada notificação. Em `MineEvent.cs`, `.FirstOrDefault()` gera delegates de closure.
- **Impacto Técnico Real:** Pressão evitável no Garbage Collector da Unity.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Pré-armazenar a lista de desenvolvedores em um array estático indexável e utilizar uma instância estática única de `Random` ou `UnityEngine.Random.Range`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-07 · Código Morto e Estruturas Órfãs em `Pooling/` e `Networking/`
- **Severidade:** 🔵 Baixo
- **Localização:** [`ArraySegmentPooling.cs:L11`](../../original/Fika-Plugin/Fika.Core/Networking/Pooling/ArraySegmentPooling.cs#L11), [`WriterPoolManager.cs:L11`](../../original/Fika-Plugin/Fika.Core/Networking/Pooling/WriterPoolManager.cs#L11), [`FikaSerializationExtensions.cs:L26-27`](../../original/Fika-Plugin/Fika.Core/Networking/FikaSerializationExtensions.cs#L26-L27)
- **Causa Raiz:**
  1. `ArraySegmentPooling` (96 linhas) não possui nenhum chamador no código.
  2. `WriterPoolManager` está marcada `[Obsolete("Use EFTSerializationExtensions instead", true)]` com pré-alocação estática no construtor.
  3. `_bufferWriter` e `_bufferReader` em `FikaSerializationExtensions.cs` são campos estáticos com buffers de 1024 bytes instanciados e nunca utilizados.
- **Impacto Técnico Real:** Poluição da base de código e desperdício de memória estática não recolhida.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Remover campos estáticos órfãos e classes obsoletas que não são consumidas por mods terceiros.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-08 · Otimização de Cadência de `ObservedPlayers.ManualStateUpdate()`
- **Severidade:** 💡 Otimização
- **Localização:** [`FikaServer.cs:L546-570`](../../original/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L546-L570) e [`FikaClient.cs:L330-348`](../../original/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L330-L348)
- **Causa Raiz:** `Update()` itera sobre toda a lista de `ObservedPlayers` a cada frame renderizado (60 a 144 vezes por segundo), quando a taxa real de atualização de rede é limitada pelo `SendRate` (10 a 30 Hz).
- **Impacto Técnico Real:** Ciclos de CPU gastos em interpolações redundantes de micro-frações de segundo sem novas amostras de pacotes.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Aplicar interpolação suave apenas quando houver delta temporal significativo ou acoplado à cadência configurada de rede, economizando ciclos de processamento no thread principal da Unity.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, etc.), as seguintes interfaces e classes públicas foram validadas e **devem permanecer intactas** em qualquer refatoração:

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `IFikaNetworkManager` | *Speak From Tarkov*, *Dynamic Maps* | Manter propriedades `IsServer`, `IsClient`, `ConnectedPeers`, `CoopHandler` e métodos `SendData` inalterados. |
| `FikaServer` / `FikaClient` | *Fika.Core*, *Speak From Tarkov* | Manter nomes de métodos, herança `MonoBehaviour` e visibilidade pública. |
| `FikaEventDispatcher` | *SAIN*, *TRL-PvpMode*, *Custom UI* | Preservar todos os tipos de eventos (`PeerConnectedEvent`, `FikaRaidStartedEvent`, `FikaNetworkManagerDestroyedEvent`). |
| `FikaVOIPClient` / `FikaVOIPServer` | *Speak From Tarkov* (interceptação de áudio) | Preservar assinaturas de pacotes e callbacks de áudio. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-01.md
```
