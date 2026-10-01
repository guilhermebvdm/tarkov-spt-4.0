---
title: "Relatório de Auditoria de Código: UIFixes"
date: 2026-09-05
status: 🟢 Vivo
authors: [AI Assistant]
---

# Relatório de Auditoria de Código: UIFixes

## 1. Resumo Executivo

Este documento apresenta a auditoria técnica de código estática e arquitetural do mod **UIFixes** (`mods/UIFixes/modded`), em conformidade com as diretrizes do SPT 4.0.13, EFT 0.16.9 e a referência local modded do **FIKA** (`mods/FIKA/modded`).

### Estatísticas da Auditoria
- **Arquivos Auditados:** 101 arquivos C# (`mods/UIFixes/modded/src/`)
- **Linhas de Código Analisadas:** ~22.000 linhas
- **Total de Achados:** 9
  - 🔴 **Críticos:** 2
  - 🟠 **Altos:** 4
  - 🟡 **Médios:** 2
  - 💡 **Baixos / Sugestões:** 1

### Classificação de Risco
- **Risco Geral do Mod:** 🟠 **Alto** (devido a riscos de travamento permanente de lógica de inventário via flags estáticas sem bloco `finally`, exceções em stack concorrente do `NetworkTransactionWatcher`, vazamentos de subscrições em delegados no `InventoryController` e `Fika.Sync`, além de dezenas de `Update()` desnecessários em centenas de células do Stash).

### Conclusão de Prontidão
O mod oferece excelentes melhorias de usabilidade para o Tarkov, mas possui vícios arquiteturais graves de performance per-frame e retenção de estado estático que degradam sessões longas de jogo e podem quebrar transferências de itens e sincronização multiplayer Fika em partidas consecutivas ou transições de mapa (`ExitStatus.Transit`).

---

## 2. Metodologia e Escopo

### Escopo Auditado
O escopo envolveu a totalidade do código fonte em `mods/UIFixes/modded/src`, abrangendo:
- Integrações e sincronização de pacotes Fika (`src/Fika/`).
- Gerenciamento de operações assíncronas e transações de rede (`src/NetworkTransactionWatcher.cs`).
- Módulos de seleção múltipla e renderização de pré-visualização (`src/Multiselect/`).
- Patches de interface, inventário, troca e arraste (`src/Patches/`).
- Camada de abstração de reflexão e wrappers de UI (`src/R.cs` e `src/GlobalUsings.cs`).

### Fontes Canônicas de Referência
1. 🥇 **Assembly Descompilado EFT:** `references/eft-decompiled/` (EFT 0.16.9.0.36411).
2. 🥇 **FIKA Modded (Referência Obrigatória):** `mods/FIKA/modded/Fika-Plugin/Fika.Core/` (v2.3.13).
3. 🥇 **Servidor SPT:** `references/spt-source/` (SPT 4.0.13).
4. 🥈 **Antipadrões SPT:** `docs/technical/spt-antipatterns.md` (AP-01 a AP-09).

---

## 3. Tabela Consolidada de Achados

| ID | Severidade | Dimensão | Localização | Descrição Resumida |
|---|---|---|---|---|
| **AUD-01-01** | 🔴 Crítico | Dimensão 5 | `FixTraderControllerSimulateFalsePatch.cs:80-86` | Flag estática `BlockPartialTransfers` sem `finally` trava merge/transfer se houver exceção |
| **AUD-01-02** | 🔴 Crítico | Dimensão 5 & 6 | `NetworkTransactionWatcher.cs:49-58` | Stack LIFO estática lança `InvalidOperationException` em descarte/concorrência assíncrona |
| **AUD-01-03** | 🟠 Alto | Dimensão 1 & 3 | `Fika/Sync.cs:58, 88-96` | Vazamento de handlers de rede Fika e retenção de subscrição em transição de mapa |
| **AUD-01-04** | 🟠 Alto | Dimensão 2 | `QuickMovePreview.cs:62`, `EmptySlotMenuTrigger.cs:23` | Centenas de `Update()` ativos per-frame em itens de inventário sem uso de `enabled = false` |
| **AUD-01-05** | 🟠 Alto | Dimensão 3 | `MultiSelect.cs:99, 103`, `SwapPatches.cs:38, 672` | Retenção estática de UI e vazamento cumulativo de delegados anônimos no `Item.Owner` |
| **AUD-01-06** | 🟠 Alto | Dimensão 3 | `DrawMultiSelect.cs:125`, `MultiSelectPatches.cs:1709` | Alocação contínua de memória (GC pressure) e ausência de pooling em previews de arraste |
| **AUD-01-07** | 🟡 Médio | Dimensão 5 | `GlobalUsings.cs:7-106`, `R.cs:189` | Violação de AP-09 com 106 aliases `GClass` hardcoded e métodos voláteis do compilador |
| **AUD-01-08** | 🟡 Médio | Dimensão 2 | `SliderPatch.cs:58-73` | `SliderMouseListener.Update()` captura eventos de scroll globalmente sem checar hover |
| **AUD-01-09** | 💡 Baixo | Dimensão 4 | `LoadMultipleMagazinesPatches.cs:138`, `Plugin.cs:128` | Expressão lógica duplicada, comentários TODO e código inativo deixado no assembly |

---

## 4. Achados Detalhados

### AUD-01-01: Flag estática `BlockPartialTransfers` sem `finally` trava operações de inventário
- **Severidade:** 🔴 Crítico
- **Dimensão:** 5 (Antipadrões SPT - AP-02: Estado compartilhado não protegido)
- **Evidência:** `mods/UIFixes/modded/src/Patches/FixTraderControllerSimulateFalsePatch.cs:78-87`
- **Causa Raiz:** A flag estática `BlockPartialTransfers = true` é atribuída antes de chamar `__instance.method_23(applicable, ref error, ref opStruct);` e restaurada para `false` logo em seguida. Se o método original do jogo lançar qualquer exceção (como `NullReferenceException` ou erro de validação de slot), a restauração nunca é executada.
- **Impacto:** A partir do momento do erro, a classe estática `FixTraderControllerPatches` mantém `BlockPartialTransfers == true` permanentemente durante toda a sessão do jogo. Isso altera globalmente o método `InteractionsHandlerClass.TransferOrMerge`, forçando apenas `Merge` e bloqueando qualquer divisão/transferência de pilhas de itens (munição, dinheiro, consumíveis) no jogo.
- **Código Problemático:**
```csharp
if (!splitAvailable)
{
    BlockPartialTransfers = true;
}

var operation = __instance.method_23(applicable, ref error, ref opStruct);

// Restore default behavior
BlockPartialTransfers = false;
```
- **Proposta de Correção:**
```csharp
if (!splitAvailable)
{
    BlockPartialTransfers = true;
}
try
{
    var operation = __instance.method_23(applicable, ref error, ref opStruct);
    // processamento do resultado...
}
finally
{
    BlockPartialTransfers = false;
}
```

---

### AUD-01-02: Fragilidade na stack de `NetworkTransactionWatcher` com `InvalidOperationException`
- **Severidade:** 🔴 Crítico
- **Dimensão:** 5 & 6 (Antipadrões SPT & Threading/Segurança Unity)
- **Evidência:** `mods/UIFixes/modded/src/NetworkTransactionWatcher.cs:47-59`
- **Causa Raiz:** O gerenciador de transações de rede assume que chamadas a `NetworkTransactionWatcher.WatchNext()` e seu subsequente `Dispose()` ocorrem em ordem estrita de pilha (LIFO). Na linha 51, `Watchers.Pop()` é chamado e, se `watcher != this`, o código lança deliberadamente `new InvalidOperationException("NetworkTransactionWatcher disposed out of order")`. Além disso, o Harmony Prefix em `NetworkTransactionPatch` esvazia a pilha chamando `Watchers.Pop()`.
- **Impacto:** Em operações assíncronas do EFT que finalizam fora de ordem ou em caso de timeout/cancelamento de rede, o bloco `using` dispara uma exceção fatal não tratada, abortando o fluxo da UI e desestabilizando transações de traders/stash.
- **Código Problemático:**
```csharp
public void Dispose()
{
    if (Watchers.Count > 0)
    {
        var watcher = Watchers.Pop();
        if (watcher != this)
        {
            throw new InvalidOperationException("NetworkTransactionWatcher disposed out of order");
        }

        _source.TrySetCanceled();
    }
}
```
- **Proposta de Correção:** Substituir a `Stack` por uma coleção baseada em identificação de instância (ou `LinkedList`/`HashSet`) e descarte seguro sem lançar exceção:
```csharp
private static readonly HashSet<NetworkTransactionWatcher> ActiveWatchers = [];

public void Dispose()
{
    lock (ActiveWatchers)
    {
        if (ActiveWatchers.Remove(this))
        {
            _source.TrySetCanceled();
        }
    }
}
```

---

### AUD-01-03: Falta de desregistro de pacotes e subscrições pendentes no Fika
- **Severidade:** 🟠 Alto
- **Dimensão:** 1 & 3 (Validação Cruzada com FIKA modded & Memory Leaks)
- **Evidência:**
  - `mods/UIFixes/modded/src/Fika/Sync.cs:58`
  - `mods/UIFixes/modded/src/Fika/Sync.cs:88-96`
  - Referência FIKA canônica: `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs:183`
- **Causa Raiz:**
  1. No evento `FikaNetworkManagerCreatedEvent`, `client.RegisterPacket(new Action<ConfigPacket>(HandlePacketClient));` é registrado, mas nunca é chamado `client.UnregisterPacket<ConfigPacket>()` quando o cliente de rede é destruído (`FikaNetworkManagerDestroyedEvent`).
  2. Em `OnGameEnded`, se o jogador transiciona entre mapas (`ev.ExitStatus == ExitStatus.Transit`), o método executa `return;` antecipado. Com isso, a subscrição `Plugin.Instance.Config.SettingChanged += OnServerSettingChanged;` permanece ativa e é acumulada novamente no próximo raid/transit.
- **Impacto:** Conexões consecutivas e partidas em coop acumulam múltiplos disparos para cada mudança de configuração e mantêm referências ativas a instâncias antigas de rede do Fika.
- **Proposta de Correção:**
  1. Assinar `FikaNetworkManagerDestroyedEvent` e executar `manager.UnregisterPacket<ConfigPacket>()`.
  2. Gerenciar a subscrição de `SettingChanged` com idempotência ou garantir que transições de mapa não dupliquem o listener.

---

### AUD-01-04: Polling per-frame em centenas de `ItemView` sem controle de `enabled`
- **Severidade:** 🟠 Alto
- **Dimensão:** 2 (`Update()` vs Arquitetura Reativa)
- **Evidência:**
  - `mods/UIFixes/modded/src/QuickMovePreview.cs:62-86`
  - `mods/UIFixes/modded/src/ContextMenus/EmptySlotMenuTrigger.cs:23-39`
- **Causa Raiz:** O componente `QuickMovePreview` é anexado a cada visualizador de item (`ItemView`) gerado no Stash e inventário (frequentemente centenas de instâncias simultâneas). O método `Update()` roda a cada frame em todas as instâncias apenas para verificar:
  ```csharp
  if (!_hovered) return;
  ```
  Na engine Unity, qualquer MonoBehaviour ativo com método `Update()` declarado força a transição de contexto C++ para C# (overhead nativo/gerenciado) em todas as frames, mesmo que a primeira linha seja um `return`.
- **Impacto:** Em stashes grandes (com 500 a 1000 itens visíveis/carregados), a engine executa 500 a 1000 chamadas inúteis de `Update()` por frame, provocando perda mensurável de taxa de quadros (FPS) na UI do inventário.
- **Proposta de Correção:** Manter o componente desabilitado (`enabled = false`) por padrão, ativando-o exclusivamente quando o ponteiro entra no item e desativando-o na saída:
```csharp
public void Awake()
{
    enabled = false;
}

public void OnPointerEnter(PointerEventData eventData)
{
    _hovered = true;
    enabled = true;
    // ...
}

public void OnPointerExit(PointerEventData eventData)
{
    _hovered = false;
    HideHighlight();
    enabled = false;
}
```

---

### AUD-01-05: Retenção estática de UI e vazamento cumulativo de delegados no `Item.Owner`
- **Severidade:** 🟠 Alto
- **Dimensão:** 3 (Memory Leaks e GC Pressure)
- **Evidência:**
  - `mods/UIFixes/modded/src/Multiselect/MultiSelect.cs:99, 103`
  - `mods/UIFixes/modded/src/Patches/SwapPatches.cs:38, 672`
- **Causa Raiz:**
  1. Em `MultiSelect.Select`, o código subscreve `windowContext.OnClose += () => Deselect(itemContext);` gerando closures anônimos que não podem ser desinscritos com `-=`.
  2. Além disso, `itemContext.Item.Owner.AddItemEvent += OnItemAdded;` é registrado no dono do item (o `InventoryController` do jogador), mas **nunca é desinscrito** quando o item é desselecionado em `Deselect()`.
  3. `MultiSelect.Clear()` itera e limpa apenas `SelectedItems`, deixando qualquer item presente em `SecondaryItems` retido na memória.
  4. Em `SwapPatches.cs:672`, o campo `private static GridItemView LastHoveredGridItemView;` grava a referência do último item com hover e nunca é anulado, retendo GameObjects, texturas e referências de inventário entre telas e raids.
- **Impacto:** Acúmulo descontrolado de listeners fantasmas disparados a cada adição de item no inventário e retenção de instâncias inteiras de UI de partidas anteriores no Heap gerenciado.
- **Proposta de Correção:**
  - Implementar método nomeado ou registrar o delegate para permitir `-= OnItemAdded` em `Deselect()`.
  - Limpar `SecondaryItems` em `MultiSelect.Clear()`.
  - Anular `LastHoveredGridItemView = null;` no fechamento de telas ou ao desinstanciar o `ItemUiContext`.

---

### AUD-01-06: Alocação excessiva per-frame e falta de pooling em pré-visualizações
- **Severidade:** 🟠 Alto
- **Dimensão:** 3 (GC Pressure & Performance)
- **Evidência:**
  - `mods/UIFixes/modded/src/Multiselect/DrawMultiSelect.cs:125`
  - `mods/UIFixes/modded/src/Patches/MultiSelectPatches.cs:1709, 1731, 1744`
- **Causa Raiz:**
  1. Em `DrawMultiSelect.Update()`, enquanto o retângulo de seleção está ativo, a linha 125 executa a cada frame:
     ```csharp
     transform.root.GetComponentsInChildren<GridItemView>()
         .Concat(Singleton<PreloaderUI>.Instance.GetComponentsInChildren<GridItemView>())
     ```
     Isso percorre toda a árvore da cena do Unity, alocando novos arrays gerenciados e enumeradores LINQ a cada frame de desenho.
  2. Em `MultiSelectPatches.ShowPreview()`, a cada alteração de célula hoverada durante o arraste de seleção múltipla, imagens de pré-visualização são criadas via `GameObject.Instantiate()` e destruídas via `GameObject.Destroy()`.
- **Impacto:** Alocação de dezenas de megabytes no Garbage Collector da Unity durante o uso de caixa de seleção, gerando micro-travamentos (stutters) perceptíveis enquanto o usuário arrasta grupos de itens.
- **Proposta de Correção:**
  - Em `DrawMultiSelect`: utilizar método sem alocação `GetComponentsInChildren(true, cachedList)` e restringir a busca ao container do Stash ativo.
  - Em `MultiSelectPatches`: implementar um pool de `Image` / `GameObject` para reciclagem imediata dos retângulos de highlight em vez de `Instantiate`/`Destroy`.

---

### AUD-01-07: Violação de AP-09 com 106 aliases `GClass` hardcoded e métodos voláteis
- **Severidade:** 🟡 Médio
- **Dimensão:** 5 (Antipadrões SPT - AP-09: Hardcoding de tipos obfuscados)
- **Evidência:**
  - `mods/UIFixes/modded/src/GlobalUsings.cs:7-106`
  - `mods/UIFixes/modded/src/R.cs:189`
  - `mods/UIFixes/modded/src/Patches/OpenSortingTablePatches.cs:58, 62`
  - `mods/UIFixes/modded/src/Patches/LoadMultipleMagazinesPatches.cs:78, 99, 127`
- **Causa Raiz:** O arquivo `GlobalUsings.cs` define 106 mapeamentos literais do tipo `global using GridItemAddress = GClass3393;`, `MoveOperation = GClass3411;`, etc. Adicionalmente, chamadas diretas a métodos obfuscados como `smethod_1` e `method_8` são invocadas por reflexão estática.
- **Impacto:** Extrema fragilidade a qualquer atualização pontual do EFT ou SPT (ex: 4.0.14 ou patches intermediários), que reorganiza os identificadores numéricos das `GClass`, quebrando a compilação e execução imediata do mod.
- **Proposta de Correção:** Adotar resolução dinâmica por tipo canônico ou estrutural (conforme convenção do SPT e utilitários de reflexão de tipos do `SPT.Reflection.Utils.PatchConstants.EftTypes`).

---

### AUD-01-08: `SliderMouseListener` escuta scroll globalmente sem validação de hover
- **Severidade:** 🟡 Médio
- **Dimensão:** 2 (`Update()` vs Arquitetura Reativa)
- **Evidência:** `mods/UIFixes/modded/src/Patches/SliderPatch.cs:58-73`
- **Causa Raiz:** `SliderMouseListener` implementa `Update()` e lê `Input.mouseScrollDelta.y` diretamente, modificando `_slider.value` sem checar se o ponteiro do mouse está sobre o slider ou seu container.
- **Impacto:** Se o usuário estiver rolando uma lista suspensa ou tela de inventário enquanto uma janela de diálogo com Slider estiver aberta em segundo plano, a rolagem acidentalmente altera os valores do slider (ex: alterando quantidade de compra ou divisão de pilhas).
- **Proposta de Correção:** Implementar `IPointerEnterHandler` e `IPointerExitHandler` no componente, habilitando a leitura de scroll exclusivamente durante o hover.

---

### AUD-01-09: Código órfão, duplicado e comentários TODO abandonados
- **Severidade:** 💡 Baixo
- **Dimensão:** 4 (Código Órfão e Morto)
- **Evidência:**
  - `mods/UIFixes/modded/src/Patches/LoadMultipleMagazinesPatches.cs:138`
  - `mods/UIFixes/modded/src/Plugin.cs:128-130`
  - `mods/UIFixes/modded/src/Multiselect/MultiSelect.cs:74-78`
- **Causa Raiz:**
  1. Em `LoadMultipleMagazinesPatches.cs:138`:
     ```csharp
     if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift) || ...)
     ```
     O teste de `KeyCode.LeftShift` é repetido duas vezes (provavelmente pretendia checar `RightShift`).
  2. Métodos e blocos inteiros comentados (`CheckForOldInterop()`, `LoadUnloadSerializer.Cancel()`).
- **Impacto:** Confusão de manutenção e comportamento inesperado com a tecla Shift direita.
- **Proposta de Correção:** Corrigir a checagem para `KeyCode.RightShift` e remover trechos de código morto e comentários obsoletos.

---

## 5. Análise por Dimensão

### Dimensão 1: Validação Cruzada com Referências Canônicas
- **Conformidade Geral:** 🟡 Parcial.
- **Análise:** A auditoria utilizando a versão local modded do FIKA (`mods/FIKA/modded`) revelou que a API de rede do Fika fornece ciclo de vida completo via `IFikaNetworkManager` (`RegisterPacket` e `UnregisterPacket`, além de eventos como `FikaNetworkManagerDestroyedEvent`). O UIFixes utiliza apenas o registro na inicialização, negligenciando a limpeza necessária ao término das sessões de jogo.

### Dimensão 2: `Update()` vs Arquitetura Reativa
- **Conformidade Geral:** 🔴 Insatisfatória.
- **Análise:** Vários componentes utilizam `Update()` desnecessariamente para polling contínuo de input e estados locais (`QuickMovePreview`, `EmptySlotMenuTrigger`, `SliderMouseListener`, `SearchKeyListener`, `FocusFleaOfferNumberPatches`). O pior caso é o `QuickMovePreview`, que executa dezenas de milhares de ciclos de chamada nativa C++/C# desperdiçados ao longo de uma sessão no Stash.

### Dimensão 3: Memory Leaks e GC Pressure
- **Conformidade Geral:** 🔴 Insatisfatória.
- **Análise:** A presença de closures anônimos em eventos estáticos (`windowContext.OnClose`, `Item.Owner.AddItemEvent`), retenção de objetos Unity em variáveis estáticas (`LastHoveredGridItemView`) e alocação dinâmica em massa per-frame (`GetComponentsInChildren` repetitivo e `Instantiate`/`Destroy` sem pooling) impõem alta sobrecarga ao GC da Unity, sendo uma das principais causas de lentidão acumulada da UI.

### Dimensão 4: Código Órfão e Morto
- **Conformidade Geral:** 🟢 Aceitável com ressalvas.
- **Análise:** A base de código está em sua maior parte ativa e articulada, com pequenos trechos comentados (`CheckForOldInterop`) e um erro de digitação lógico em `LoadMultipleMagazinesPatches.cs:138`.

### Dimensão 5: Antipadrões SPT (AP-01 a AP-09)
- **Conformidade Geral:** 🟠 Problemática.
- **Análise:**
  - **AP-02:** Flag estática sem `finally` em `FixTraderControllerPatches` põe em risco a estabilidade de inventário.
  - **AP-09:** Extensiva dependência de 106 aliases `GClass` hardcoded em `GlobalUsings.cs` e chamadas diretas a métodos obfuscados por reflexão.

### Dimensão 6: Threading e Segurança Unity
- **Conformidade Geral:** 🟡 Regular.
- **Análise:** `NetworkTransactionWatcher` implementa uma pilha estática frágil que não tolera execução assíncrona concorrente ou descarte de tarefas fora de ordem estrita, podendo lançar exceções não tratadas durante operações de inventário.

---

## 6. Recomendações e Próximos Passos

### Plano de Ação Prioritário

1. **Fase 1 (Segurança e Estabilidade - Crítico):**
   - [ ] Envolver a alteração de `BlockPartialTransfers` em bloco `try ... finally` no `FixTraderControllerSimulateFalsePatch.cs` (AUD-01-01).
   - [ ] Refatorar o `NetworkTransactionWatcher` para coleção segura sem lançar `InvalidOperationException` em descarte fora de ordem (AUD-01-02).
   - [ ] Corrigir o ciclo de registro/desregistro e limpeza de subscrição de pacotes no `src/Fika/Sync.cs` (AUD-01-03).

2. **Fase 2 (Otimização de Performance e GC - Alto):**
   - [ ] Adicionar controle de `enabled = false` / `enabled = true` em `QuickMovePreview.cs` e `EmptySlotMenuTrigger.cs` (AUD-01-04).
   - [ ] Implementar desinscrição de `AddItemEvent` e limpeza de `SecondaryItems` em `MultiSelect.cs` e anular referências estáticas em `SwapPatches.cs` (AUD-01-05).
   - [ ] Eliminar chamadas de `GetComponentsInChildren` per-frame em `DrawMultiSelect.cs` e criar pooling para os retângulos de highlight em `MultiSelectPatches.cs` (AUD-01-06).

3. **Fase 3 (Refinamento e Robustez - Médio/Baixo):**
   - [ ] Restringir o listener de scroll em `SliderPatch.cs` para ativação sob hover (AUD-01-08).
   - [ ] Corrigir verificação de `KeyCode.RightShift` em `LoadMultipleMagazinesPatches.cs:138` (AUD-01-09).
   - [ ] Planejar a migração progressiva dos 106 `GClass` em `GlobalUsings.cs` para nomes canônicos e reflexão desacoplada (AUD-01-07).

### Sugestão de Backlog
Recomenda-se a criação do item de backlog para aplicação controlada dessas melhorias:
- **Sugestão:** `mods/UIFixes/backlog/001-audit-fixes/` (ou aplicação direta sob comando do desenvolvedor).
