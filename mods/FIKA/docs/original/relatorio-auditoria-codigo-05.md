---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 05: Raid Lifecycle & World Interactivity)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 05: Raid Lifecycle & World Interactivity)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 5 (Ciclo de Vida de Raid, GameMode, Sincronização de Mundo, Portas, Lâmpadas, Airdrops, BTR, Transits e Reconexão)** do código original do mod **FIKA**, inspecionando ~7.200 linhas de código C# distribuídas nos módulos `Fika.Core/Main/GameMode/`, `HostClasses/`, `ClientClasses/`, `Components/` e `Networking/Packets/World/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 2 | Falta de descarte e limpeza do `GameController` em `CoopGame.Dispose()` (retendo bots, loot e granadas) e vazamento de inscrições de eventos (`+=` sem `-=`) em `FikaExfilManager` e `ItemPositionSyncer`. |
| 🟠 **Alto** | 1 | Polling contínuo em `FikaExfilManager.Update()` executando LINQ `.Any()` a cada frame em pontos de contagem regressiva de extração. |
| 🟡 **Médio** | 2 | Picos de GC por conversão JSON + Zlib no pacote de reconexão (`ReconnectPacket`) e falta de limpeza de `_playersInTransitZone` em `FikaHostTransitController`. |
| 💡 **Otimização** | 1 | Otimização de checagem de física repousada (*sleeping rigidbody*) em `BTRViewSynchronizer` e `CorpsePositionSyncer`. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-05-01` | 🔴 Crítico | [`CoopGame.cs:L648-668`](../../original/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L648-L668) | Memory Leak | `GameController` nunca é descartado no `CoopGame.Dispose()`, retendo listas de `Bots`, `LootItems`, `ThrownGrenades` e `CoopHandler`. |
| `AUD-05-02` | 🔴 Crítico | [`FikaExfilManager.cs:L90-93`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaExfilManager.cs#L90-L93) | Event Leak | Inscrições de eventos em `ExfiltrationPoint` (`OnStartExtraction`, `OnStatusChanged`) nunca são desinscritas no teardown. |
| `AUD-05-03` | 🟠 Alto | [`FikaExfilManager.cs:L72-76`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaExfilManager.cs#L72-L76) | GC / CPU | Uso contínuo de LINQ `!exfiltrationPoint.UnmetRequirements(player).Any()` no loop de `Update()`. |
| `AUD-05-04` | 🟡 Médio | [`ReconnectPacket.cs:L51, L91`](../../original/Fika-Plugin/Fika.Core/Networking/Packets/World/ReconnectPacket.cs#L51) | GC Pressure | Serialização JSON (`ToJson()`) e compressão Zlib em tempo real no envio UDP de dados de saúde na reconexão. |
| `AUD-05-05` | 🟡 Médio | [`FikaHostTransitController.cs:L28-30`](../../original/Fika-Plugin/Fika.Core/Main/HostClasses/FikaHostTransitController.cs#L28-L30) | AP-01 (Teardown) | Dicionários `_playersInTransitZone` e listas de jogadores em trânsito não são limpos ao final da raid. |
| `AUD-05-06` | 💡 Otimização | [`BTRViewSynchronizer.cs:L15`](../../original/Fika-Plugin/Fika.Core/Main/Components/BTRViewSynchronizer.cs#L15), [`CorpsePositionSyncer.cs:L20`](../../original/Fika-Plugin/Fika.Core/Main/Components/CorpsePositionSyncer.cs#L20) | Desempenho | Polling contínuo de coordenadas em `Update()` que pode ser pausado quando os corpos/veículos entram em repouso físico. |

---

## 3. Detalhamento dos Achados

### AUD-05-01 · Retenção de `GameController` e Coleções em `CoopGame.Dispose`
- **Severidade:** 🔴 Crítico
- **Localização:** [`CoopGame.cs:L648-668`](../../original/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L648-L668)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** No encerramento da raid em `CoopGame.Dispose()`, os jogadores de `dictionary_0` são descartados, mas a propriedade `GameController` (`HostGameController` ou `ClientGameController`) não recebe nenhuma chamada de limpeza ou descarte.
- **Impacto Técnico Real:** `GameController.Bots` (`Dictionary<string, Player>`), `GameController.LootItems`, `GameController.ThrownGrenades`, `GameController.ExfilManager` e o `CoopHandler` continuam vivos na memória Heap da aplicação, acumulando gigabytes de dados após várias partidas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Otimizada:*
    ```csharp
    public override void Dispose()
    {
        ClientHearingTable.Instance = null;

        if (GameController != null)
        {
            GameController.Bots?.Clear();
            GameController.LootItems?.Clear();
            GameController.ThrownGrenades?.Clear();
            GameController.CoopHandler?.Players?.Clear();
            GameController = null;
        }

        foreach (var player in dictionary_0.Values)
        {
            try
            {
                if (player != null)
                {
                    player.Dispose();
                    AssetPoolObject.ReturnToPool(player.gameObject, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }
        dictionary_0.Clear();
        base.Dispose();
    }
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-05-02 · Vazamento de Inscrições de Eventos em `FikaExfilManager` e `ItemPositionSyncer`
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaExfilManager.cs:L90-93`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaExfilManager.cs#L90-L93) e [`ItemPositionSyncer.cs:L70`](../../original/Fika-Plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs#L70)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** `FikaExfilManager` faz `+=` nos eventos dos pontos de extração (`OnStartExtraction`, `OnCancelExtraction`, `OnStatusChanged`) mas não possui `OnDestroy()` para desinscrever com `-=`. Da mesma forma, `ItemPositionSyncer` assina `ItemOwner.RemoveItemEvent` sem remoção defensiva em caso de destruição prematura do componente.
- **Impacto Técnico Real:** Referências de delegates aos componentes do Fika permanecem atreladas aos GameObjects estáticos da cena do Tarkov.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Implementar `OnDestroy()` em `FikaExfilManager` e `ItemPositionSyncer` desinscrevendo rigorosamente todos os delegates registrados.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-05-03 · Polling com LINQ em `FikaExfilManager.Update`
- **Severidade:** 🟠 Alto
- **Localização:** [`FikaExfilManager.cs:L72-76`](../../original/Fika-Plugin/Fika.Core/Main/Components/FikaExfilManager.cs#L72-L76)
- **Causa Raiz:** `_countdownPoints` no método `Update()` executa `!exfiltrationPoint.UnmetRequirements(player).Any()` para cada jogador no ponto de extração a cada frame.
- **Impacto Técnico Real:** Alocações contínuas de enumeradores LINQ no Garbage Collector enquanto jogadores estão extraindo.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir `!exfiltrationPoint.UnmetRequirements(player).Any()` por uma verificação direta ou iteração manual sem delegates.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-05-04 · Churn de GC por JSON e Zlib em `ReconnectPacket`
- **Severidade:** 🟡 Médio
- **Localização:** [`ReconnectPacket.cs:L51, L91`](../../original/Fika-Plugin/Fika.Core/Networking/Packets/World/ReconnectPacket.cs#L51)
- **Causa Raiz:** Serialização via `SimpleZlib.CompressToBytes(ProfileHealthClass.ToJson(), 4)` cria strings JSON completas e buffers compactados no Heap a cada envio de estado para clientes reconectando.
- **Impacto Técnico Real:** Picos evitáveis de alocação de memória e processamento de CPU durante o processo de sincronização de reconexão.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Serializar a estrutura binária dos membros de saúde diretamente no `NetDataWriter` sem passar por JSON intermediário.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-05-05 · Retenção de Coleções em `FikaHostTransitController`
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaHostTransitController.cs:L28-30`](../../original/Fika-Plugin/Fika.Core/Main/HostClasses/FikaHostTransitController.cs#L28-L30)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O controlador de trânsito retém `_playersInTransitZone` (`Dictionary<Player, int>`) e `_transittedPlayers` (`List<int>`) sem rotina explícita de limpeza quando a raid termina ou o jogador decide não transitar.
- **Impacto Técnico Real:** Retenção de instâncias de `Player` no controlador de trânsito.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar método de limpeza explícito chamado no encerramento da partida.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-05-06 · Otimização de Sincronização em `BTRViewSynchronizer` e `CorpsePositionSyncer`
- **Severidade:** 💡 Otimização
- **Localização:** [`BTRViewSynchronizer.cs:L15`](../../original/Fika-Plugin/Fika.Core/Main/Components/BTRViewSynchronizer.cs#L15), [`CorpsePositionSyncer.cs:L20`](../../original/Fika-Plugin/Fika.Core/Main/Components/CorpsePositionSyncer.cs#L20)
- **Causa Raiz:** Execução contínua em `Update()` verificando posição de veículos e corpos, mesmo quando estes já atingiram o estado de repouso físico (`rigidbody.IsSleeping()`).
- **Impacto Técnico Real:** Ciclos desnecessários de CPU no thread principal da Unity.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Interromper o polling ou reduzir a frequência de checagem quando o Rigidbody estiver em repouso (*sleeping*).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Questing Bots*, *Transit mods*):

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `IFikaGame` | *Fika.Core*, *Modding APIs* | Preservar propriedades `GameController`, `ExitStatus`, `ExitLocation`. |
| `CoopGame.GameController` | *External Scripts*, *UI Mods* | Preservar visibilidade pública e tipo `BaseGameController`. |
| `FikaHostGameWorld` / `FikaClientGameWorld` | *Transit mods*, *Map mods* | Preservar herança e métodos de sincronização de mundo. |
| `FikaExfilManager` | *Dynamic Maps*, *Extraction UI* | Preservar métodos públicos de extração e contagem. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-05.md
```
