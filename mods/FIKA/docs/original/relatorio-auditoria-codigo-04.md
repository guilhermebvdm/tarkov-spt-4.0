---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 04: Strict Inventory Sync & Ballistics)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 04: Strict Inventory Sync & Ballistics)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 4 (Sincronização Estrita de Inventário, Transações RPC de Itens, Pipeline Balístico, Armaduras e Registro de Dano)** do código original do mod **FIKA**, inspecionando ~2.700 linhas de código C# distribuídas nos módulos `Fika.Core/Main/BaseClasses/BaseInventoryController.cs`, `HostClasses/HostInventoryController.cs`, `ClientClasses/ClientInventoryController.cs`, `ObservedClasses/ObservedInventoryController.cs` e rotinas balísticas de `FikaPlayer.cs`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 1 | Memory leak por falta de descarte de pools de handlers (`HostInventoryOperationHandlerPool` e `ClientInventoryOperationHandlerPool`) no teardown `OnDestroy()` de `FikaPlayer`. |
| 🟠 **Alto** | 1 | Churn contínuo de tarefas assíncronas no Heap (`Task.Yield`) para toda e qualquer transação de inventário em `HostInventoryController` e `ClientInventoryController`. |
| 🟡 **Médio** | 2 | Acessos não defensivos a Singletons de configuração (`BackendConfigSettingsClass` e `SharedGameSettingsClass` — AP-02) e alocações de LINQ/closures no descarregador rápido de carregadores. |
| 💡 **Otimização** | 1 | Alocações transitórias de `ShotInfoClass` durante rajadas de tiro de alta cadência em `SimulatedApplyShot`. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-04-01` | 🔴 Crítico | [`FikaPlayer.cs:L1586-1595`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1586-L1595) | Memory Leak | Pools de handlers de inventário do Host e Cliente só são limpos em `Dispose()`, sendo ignorados no `OnDestroy()` da Unity. |
| `AUD-04-02` | 🟠 Alto | [`HostInventoryController.cs:L79`](../../original/Fika-Plugin/Fika.Core/Main/HostClasses/HostInventoryController.cs#L79), [`ClientInventoryController.cs:L87`](../../original/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryController.cs#L87) | GC Pressure | `HandleOperation` dispara `await Task.Yield()` gerando alocações de máquinas de estado assíncronas para cada ação de item. |
| `AUD-04-03` | 🟡 Médio | [`BaseInventoryController.cs:L72, L119`](../../original/Fika-Plugin/Fika.Core/Main/BaseClasses/BaseInventoryController.cs#L72) | AP-02 (Defensiva) | Acessos a `Singleton<BackendConfigSettingsClass>.Instance` e `SharedGameSettingsClass` sem checagem de nulo prévia. |
| `AUD-04-04` | 🟡 Médio | [`BaseInventoryController.cs:L357, L373`](../../original/Fika-Plugin/Fika.Core/Main/BaseClasses/BaseInventoryController.cs#L357) | GC Pressure | Uso de LINQ `.Sum(i => i.StackObjectsCount)` e closures em `CustomAmmoUnloader` alocando delegates no Heap. |
| `AUD-04-05` | 💡 Otimização | [`FikaPlayer.cs:L816`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L816) | Desempenho | Alocação de `new ShotInfoClass()` em `SimulatedApplyShot` a cada projétil de IA e minas recebido. |

---

## 3. Detalhamento dos Achados

### AUD-04-01 · Falta de Limpeza de Pools de Inventário no Teardown (`OnDestroy`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`FikaPlayer.cs:L1586-1595`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1586-L1595) vs [`L1982-2013`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1982-L2013)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `ClearPool()` de `HostInventoryController` e `ClientInventoryController` só é chamado dentro do método `FikaPlayer.Dispose()`. Se o GameObject do jogador for destruído pelo Unity durante descarregamento de cena ou morte sem invocação explícita de `Dispose()`, os pools `_hostInventoryOperationHandlerPool` e `_clientInventoryOperationHandlerPool` não são descartados.
- **Impacto Técnico Real:** Instâncias de handlers contendo referências para `NetPeer`, delegates de callback e descritores de itens de inventário permanecem retidas no Heap entre partidas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Garantir a invocação de `ClearPool()` diretamente no `FikaPlayer.OnDestroy()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-04-02 · Churn de Tarefas Assíncronas em Transações de Inventário (`Task.Yield`)
- **Severidade:** 🟠 Alto
- **Localização:** [`HostInventoryController.cs:L79-86`](../../original/Fika-Plugin/Fika.Core/Main/HostClasses/HostInventoryController.cs#L79-L86) e [`ClientInventoryController.cs:L87-94`](../../original/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryController.cs#L87-L94)
- **Causa Raiz:** Ambos os controladores implementam `HandleOperation` executando `if (_player.HealthController.IsAlive) await Task.Yield();`, alocando objetos assíncronos no Heap a cada movimentação de item, recarga ou interação de contêiner.
- **Impacto Técnico Real:** Picos contínuos de alocação de memória no Garbage Collector durante momentos de saque ou combate rápido com manipulação de itens.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Executar as operações de forma síncrona diretamente no frame corrente quando a integridade do inventário permitir, eliminando a criação de `Task` no Heap.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-04-03 · Acessos Não Defensivos a Singletons de Configuração (AP-02)
- **Severidade:** 🟡 Médio
- **Localização:** [`BaseInventoryController.cs:L72, L119`](../../original/Fika-Plugin/Fika.Core/Main/BaseClasses/BaseInventoryController.cs#L72) e [`ClientInventoryController.cs:L70`](../../original/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryController.cs#L70)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-02`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** Acessos a `Singleton<BackendConfigSettingsClass>.Instance` e `Singleton<SharedGameSettingsClass>.Instance` ocorrem sem validação prévia de instanciação (`.Instantiated`).
- **Impacto Técnico Real:** Risco de `NullReferenceException` durante transições de carregamento ou inicialização de itens antes da disponibilização completa dos singletons do SPT.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Inserir guarda defensiva com valores padrão de fallback seguros.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-04-04 · Alocações de LINQ no Descarregamento Rápido de Carregadores
- **Severidade:** 🟡 Médio
- **Localização:** [`BaseInventoryController.cs:L357, L373`](../../original/Fika-Plugin/Fika.Core/Main/BaseClasses/BaseInventoryController.cs#L357)
- **Causa Raiz:** `_totalAmmoCount = magazine.Cartridges.Items.Sum(i => i.StackObjectsCount);` aloca closures e delegates a cada início de descarregamento rápido.
- **Impacto Técnico Real:** Alocação desnecessária no Heap em um loop utilitário simples.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir o `.Sum(...)` por um loop `foreach` simples iterando sobre os cartuchos sem alocações.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-04-05 · Otimização de Alocações em `SimulatedApplyShot`
- **Severidade:** 💡 Otimização
- **Localização:** [`FikaPlayer.cs:L816`](../../original/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L816)
- **Causa Raiz:** Instanciação repetida de `new ShotInfoClass()` no processamento de tiros de IA e minas.
- **Impacto Técnico Real:** Pressão evitável no Garbage Collector em situações de tiroteio com múltiplos bots atirando simultaneamente.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Reutilizar uma instância local reciclada de `ShotInfoClass` para registro de impactos.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Realism*, *Item/Weapon mods*):

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `BaseInventoryController.StrictSync` | *Network Controllers*, *HUD mods* | Preservar propriedade booleana e visibilidade. |
| `BaseInventoryController.LoadMagazine` / `UnloadMagazine` | *EFT Inventory Logic* | Preservar assinaturas de métodos e retornos de `Task<IResult>`. |
| `FikaPlayer.ApplyDamageInfo` / `ApplyShot` | *SAIN*, *EFT Ballistics* | Preservar assinaturas e contratos de física balística. |
| `HostInventoryController` / `ClientInventoryController` | *Fika.Core*, *Networking* | Preservar hierarquia de herança de `BaseInventoryController`. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-04.md
```
