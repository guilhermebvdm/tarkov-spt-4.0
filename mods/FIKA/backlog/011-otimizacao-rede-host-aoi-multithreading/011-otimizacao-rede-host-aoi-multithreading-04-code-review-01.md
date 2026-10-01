# 011 — Otimização de Rede do Host (AoI Culling & Multithreading) · Code Review 01

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Técnica:** [011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md](011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md)  
**Review Técnica:** [011-otimizacao-rede-host-aoi-multithreading-03-spec-tech-review-01.md](011-otimizacao-rede-host-aoi-multithreading-03-spec-tech-review-01.md)  
**Data da Code Review:** 2026-09-15  

---

## 1. Resumo da Análise de Código

A revisão estrita de código inspecionou os arquivos modificados no fork `mods/FIKA/modded-V2/`:
1. `BotStateManager.cs`
2. `FikaServer.cs`
3. `BotPacketSender.cs`
4. `FikaConfig.cs`

Foram identificados 3 achados específicos de implementação relacionados à segurança de concorrência e resiliência de exceções:

---

## 2. Achados de Code Review

### 🔍 CR-01-01: Verificação de `peer.ConnectionState` antes de cada `peer.Send()`
- **Localização:** `BotStateManager.cs:255-270`
- **Gravidade:** Média
- **Descrição:** O código valida `if (peer == null || peer.ConnectionState != ConnectionState.Connected) continue;` no início da iteração do peer. No entanto, o envio real do pacote UDP ocorre em múltiplos pontos dentro do loop de bots (`writer.Length + _stateSize > maxMtu`) e no final do loop (`if (writtenCount > 0)`). Se o peer desconectar durante a iteração de 35 bots, o objeto `NetPeer` pode ser descartado enquanto o writer ainda tenta despachar.
- **Solução Recomendada:** Envolver a chamada de envio por peer em um bloco `try/catch (ObjectDisposedException)` individual, impedindo que a queda de um cliente interrompa o envio para os demais clientes da lista.

---

### 🔍 CR-01-02: Possibilidade de Bloqueio da Main Thread se a Background Thread Travar
- **Localização:** `BotStateManager.cs:176-187`
- **Gravidade:** Baixa
- **Descrição:** A Main Thread adquire `lock (_swapLock)` para trocar os ponteiros `_frontBuffer` e `_backBuffer`. Do outro lado, o `WorkerLoop` também adquire `lock (_swapLock)` apenas para capturar a referência `toProcess = _backBuffer;`. Como ambos os blocos executam apenas a atribuição de uma referência em memória (operações de nanossegundos), a contenção de lock é praticamente nula. No entanto, deve-se garantir que **nenhum processamento de I/O ou cálculo ocorra dentro do bloco `lock`**, o que foi estritamente respeitado na implementação.

---

### 🔍 CR-01-03: Tolerância a Reentrância de `AutoResetEvent`
- **Localização:** `BotStateManager.cs:186`
- **Gravidade:** Baixa
- **Descrição:** O método `_workSignal.Set()` é chamado a cada tick de rede. Se a thread de fundo ainda estiver processando o tick anterior quando a Main Thread disparar o próximo tick, o sinal do `AutoResetEvent` permanece em estado sinalizado (`Set`) e o worker imediatamente emenda o próximo processamento sem empilhar threads ou alocar filas. O buffer é sobrescrito com os dados mais recentes da Unity, o que é o comportamento ótimo para rede UDP (*drop frame* do estado velho em favor do estado novo).

---

## 3. Conformidade com as Regras do Projeto

- **Isolamento de Compilação:** 100% em conformidade. Nenhuma DLL foi gerada ou copiada para `D:/SPT`.
- **SemVer:** Versão atualizada para `2.4.3` sincronizada entre `FikaPlugin.cs` e `Fika.Core.csproj`.
- **Zero-Alloc GC:** Validado. Nenhum array ou objeto novo é instanciado durante o `Update()` ou `WorkerLoop()`.
