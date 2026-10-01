---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 03: AI & Bot Spawning)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 03: AI & Bot Spawning)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 3 (Sincronização de Bots, IA Autoritativa no Host, Spawns, Dynamic AI e Culling)** do código original do mod **FIKA**, inspecionando ~1.400 linhas de código C# distribuídas nos módulos `Fika.Core/Main/Players/FikaBot.cs`, `Fika.Core/Main/BotClasses/`, `Fika.Core/Main/Components/BotStateManager.cs` e `Fika.Core/Main/Patches/AI/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 1 | Memory leak e risco de exceção não tratada no encerramento de bots (`FikaBot.OnDestroy()`) impedindo limpeza do `BotsController`. |
| 🟠 **Alto** | 1 | Acesso não defensivo a `Singleton<AbstractGame>.Instance` em `BotPlayerBridge` (AP-02). |
| 🟡 **Médio** | 2 | Churn de tarefas assíncronas no Heap em `BotInventoryController.HandleOperation` (`Task.Yield`) e alocações de boxing com `Enum.GetValues` em `BotCacher_Patch`. |
| 🔵 **Baixo** | 1 | Chamada direta ao método obfuscado `_botsController.method_0()` sem anotação conceitual (AP-09). |
| 💡 **Otimização** | 1 | Validação defensiva de nulidade no loop de envio em lote de estados de IA (`BotStateManager.SendBatchStates`). |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-03-01` | 🔴 Crítico | [`FikaBot.cs:L359-385`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaBot.cs#L359-L385) | Memory Leak | Falta de limpeza de coleções de armadura e risco de NRE ao consultar `fikaGame.GameController.GameInstance.Status` no teardown. |
| `AUD-03-02` | 🟠 Alto | [`BotPlayerBridge.cs:L24`](../../original/Fika-Plugin/Fika.Core/Main/BotClasses/BotPlayerBridge.cs#L24) | AP-02 (Defensiva) | Acesso a `Singleton<AbstractGame>.Instance.LastServerTimeStamp` sem checagem de nulo. |
| `AUD-03-03` | 🟡 Médio | [`BotInventoryController.cs:L90-97`](../../original/Fika-Plugin/Fika.Core/Main/BotClasses/BotInventoryController.cs#L90-L97) | GC Pressure | `HandleOperation` instancia uma máquina de estados assíncrona (`Task.Yield`) a cada ação de inventário de cada bot. |
| `AUD-03-04` | 🟡 Médio | [`BotCacher_Patch.cs:L36, L38`](../../original/Fika-Plugin/Fika.Core/Main/Patches/BotCacher_Patch.cs#L36) | GC Pressure | Uso de `Enum.GetValues` em loop aninhado gerando alocações de arrays e boxing de enums. |
| `AUD-03-05` | 🔵 Baixo | [`BotStateManager.cs:L71`](../../original/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L71) | AP-09 (Ofuscação) | Invocação direta de `_botsController.method_0()` sem documentação semântica do conceito EFT 4.1. |
| `AUD-03-06` | 💡 Otimização | [`BotStateManager.cs:L85-98`](../../original/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L85-L98) | Robustez | Adição de checagem defensiva de nulo contra bots destruídos durante a iteração de escrita de pacotes UDP. |

---

## 3. Detalhamento dos Achados

### AUD-03-01 · Memory Leak e Risco de NRE no Teardown de Bots (`FikaBot.OnDestroy`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaBot.cs:L359-385`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaBot.cs#L359-L385)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** No método `OnDestroy()`, a consulta `fikaGame.GameController.GameInstance.Status` não é defensiva; se a partida estiver sendo encerrada pelo menu ou por queda do servidor, `GameController` ou `GameInstance` podem ser nulos, gerando `NullReferenceException` que interrompe a execução antes da chamada a `base.OnDestroy()`. Além disso, bindings de armadura (`_armorUnsubcribes`) e coleções `_preAllocatedArmorComponents` herdadas de `FikaPlayer` não são liberadas caso o bot seja destruído vivo.
- **Impacto Técnico Real:** Instâncias de bots vivos destruídos na transição de saída da raid retêm referências a `Profile`, `Inventory` e `PlayerBones` no Heap.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:*
    ```csharp
    public override void OnDestroy()
    {
        if (Singleton<FikaServer>.Instantiated)
        {
            var fikaGame = Singleton<IFikaGame>.Instance;
            if (fikaGame?.GameController?.GameInstance != null && fikaGame.GameController.GameInstance.Status == GameStatus.Started)
            {
                var server = Singleton<FikaServer>.Instance;
                BotStatePacket packet = new()
                {
                    NetId = NetId,
                    Type = BotStatePacket.EStateType.DisposeBot
                };

                server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
                fikaGame.GameController.Bots.Remove(ProfileId);
            }
        }

        if (CoopHandler.TryGetCoopHandler(out var coopHandler))
        {
            coopHandler.Players.Remove(NetId);
        }

        base.OnDestroy();
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-02 · Acesso Não Defensivo a `Singleton<AbstractGame>.Instance` em `BotPlayerBridge`
- **Severidade:** 🟠 Alto
- **Localização:** [`BotPlayerBridge.cs:L24`](../../original/Fika-Plugin/Fika.Core/Main/BotClasses/BotPlayerBridge.cs#L24)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-02`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** A propriedade `WorldTime` consulta `Singleton<AbstractGame>.Instance.LastServerTimeStamp` sem checar se o singleton está instanciado.
- **Impacto Técnico Real:** Disparos balísticos ou registro de acertos ocorrendo em momentos de transição de cena disparam `NullReferenceException`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por:
    ```csharp
    public float WorldTime
    {
        get
        {
            return Singleton<AbstractGame>.Instantiated && Singleton<AbstractGame>.Instance != null
                ? Singleton<AbstractGame>.Instance.LastServerTimeStamp
                : Time.time;
        }
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-03 · Churn de Tarefas Assíncronas em Operações de Inventário de Bots
- **Severidade:** 🟡 Médio
- **Localização:** [`BotInventoryController.cs:L90-97`](../../original/Fika-Plugin/Fika.Core/Main/BotClasses/BotInventoryController.cs#L90-L97)
- **Causa Raiz:** `HandleOperation` executa `await Task.Yield()` para toda operação de inventário de bots (troca de armas, carregamento de cartuchos, checagem de carregador), alocando uma máquina de estados assíncrona no Heap.
- **Impacto Técnico Real:** Com dezenas de bots atuando simultaneamente, centenas de micro-tarefas assíncronas são geradas por segundo, aumentando o churn do Garbage Collector.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Executar `RunBotOperation(operation, callback)` diretamente quando a operação não exigir adiamento de frame, ou despachar via coroutine / callback síncrono.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-04 · Alocações Desnecessárias de Array e Boxing em `BotCacher_Patch`
- **Severidade:** 🟡 Médio
- **Localização:** [`BotCacher_Patch.cs:L36, L38`](../../original/Fika-Plugin/Fika.Core/Main/Patches/BotCacher_Patch.cs#L36)
- **Causa Raiz:** `Enum.GetValues(typeof(WildSpawnType))` e `Enum.GetValues(typeof(BotDifficulty))` são executados em loop duplo a cada inicialização de raid, alocando múltiplos arrays de enums no Heap.
- **Impacto Técnico Real:** Alocações transitórias evitáveis durante o carregamento da raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Pré-armazenar os arrays de enums em campos `static readonly WildSpawnType[]` e `static readonly BotDifficulty[]`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-05 · Invocação Direta de Método Obfuscado `_botsController.method_0()` (AP-09)
- **Severidade:** 🔵 Baixo
- **Localização:** [`BotStateManager.cs:L71`](../../original/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L71)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-09`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `_botsController?.method_0()` é chamado sem comentário semântico explicitando o conceito mapeado (otimização de ondas / culling de IA do EFT).
- **Impacto Técnico Real:** Dívida de legibilidade e risco em migrações de versão.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Inserir comentário explicativo `// EFT 4.1 Concept: BotWavesOptimization / Culling Update Step`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-03-06 · Validação Defensiva no Loop de Batching de Bots
- **Severidade:** 💡 Otimização
- **Localização:** [`BotStateManager.cs:L85-98`](../../original/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L85-L98)
- **Causa Raiz:** `SendBatchStates` itera sobre `_bots[i]` sem checagem de nulo ou validação de `BotPacketSender`.
- **Impacto Técnico Real:** Se um bot sofrer destruição inesperada no mesmo frame, a rotina pode disparar `NullReferenceException` e interromper o pacote de lote de todos os outros bots.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Validar `if (_bots[i] != null && _bots[i].BotPacketSender != null)` antes de gravar o estado no `NetDataWriter`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para garantir que mods de terceiros de IA e spawn (*SAIN*, *BigBrain*, *Questing Bots*, *Looting Bots*, *Speak From Tarkov*) operem com 100% de estabilidade:

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `FikaBot.BotPacketSender` | *Fika.Core*, *Networking* | Preservar visibilidade e assinatura. |
| `FikaBot.CreateBot` | *HostGameController*, *AI Spawners* | Preservar todos os parâmetros de instanciação de bots. |
| `FikaPlayer.IsAI` / `IsObservedAI` | *SAIN*, *Speak From Tarkov*, *Dynamic Maps* | Preservar propriedades de identificação. |
| `BotPlayerBridge` | *EFT Ballistics*, *BodyPartCollider* | Preservar implementação da interface `IPlayerBridge`. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-03.md
```
