---
title: "Relatório de Code Review Técnico — Climbable Ladders (Review 02)"
date: 2026-09-03
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Code Review Técnico — Climbable Ladders (Review 02)

**Mod:** `Climbable Ladders`  
**Versões Analisadas:** `tarkin.ladders.bep` v1.0.7 / `tarkin.ladders.fika` v1.1.3 / `tarkin.ladders.shared` v1.0.4  
**Referência de Multiplayer:** [FIKA modded](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded) v2.3.11  
**Auditoria Prévia de Referência:** [relatorio-auditoria-codigo-02.md](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/Climbable%20Ladders/docs/relatorio-auditoria-codigo-02.md)  
**Data da Revisão:** 2026-09-03  

> Análise crítica e verificação estática minuciosa do código implementado no mod Climbable Ladders para sanar os achados `AUD-02-01` até `AUD-02-07` e modernizar a camada de rede com a API pública do FIKA modded.

---

## 1. Resumo Executivo da Revisão

> 🔴 **Bloqueadores:** 0 · 🟠 **Fortes:** 0 · 🟡 **Médios:** 0 · 🟢 **Menores:** 0 · ✅ **Aprovados e Resolvidos:** 7 · **Total:** 7

| Categoria da Análise | Avaliação | Parecer |
|---|---|---|
| **A — Crítico** (Bugs graves, crashes, memory leaks) | 🟢 Limpo | `AUD-02-01` totalmente sanado; sem vazamento de cenas aditivas em Release. |
| **B — Bug latente** (Cenários de borda e concorrência) | 🟢 Limpo | `AUD-02-04` e `AUD-02-07` eliminam riscos de colisão com bots e colegas coop. |
| **C — Gap vs. Spec** (Critérios de conformidade) | 🟢 Limpo | Todas as propostas do plano de implementação foram cumpridas com fidelidade. |
| **D — Arquitetura** (Padrões de projeto, reuso e reflection) | 🟢 Limpo | Reflection de `_packetProcessor` removida em prol da API oficial do FIKA modded. |
| **E — Legibilidade e Manutenção** | 🟢 Limpo | Código enxuto, comentários explicativos e salvaguardas contra nulo. |
| **F — Otimização de Performance** | 🟢 Limpo | `ProceduralGrip` zero-alloc e cache de peso em `Update()` eliminam overhead desnecessário. |

---

## 2. Tabela Detalhada de Verificação dos Itens Implementados

| ID | Item / Arquivo | Categoria | Impacto Original | Status Pós-Implementação | Verificação Técnica |
|---|---|---|---|---|---|
| `CR-02-01` | [LaddersLoader.cs:L73](../../modded/ladders.bep/LaddersLoader.cs#L73) | A — Crítico | 🔴 Bloqueador | ✅ **Aprovado** | `#else` garante compilação de `SceneManager.UnloadSceneAsync` em `-c Release`. |
| `CR-02-02` | [ProxyTransformModifierByPath.cs:L77](../../modded/ladders.shared/ProxyTransformModifierByPath.cs#L77) | B — Bug latente | 🟠 Forte | ✅ **Aprovado** | `localPosition` e `localRotation` salvos e restaurados com simetria hierárquica. |
| `CR-02-03` | [ProceduralGrip.cs:L195](../../modded/ladders.bep/ProceduralGrip.cs#L195) | F — Otimização | 🟡 Médio | ✅ **Aprovado** | `SetCurl` atualiza `Base`, `Mid`, `Tip` diretamente; zero alocações de `IEnumerator`. |
| `CR-02-04` | [PlayerLadderController.cs:L389](../../modded/ladders.bep/PlayerLadderController.cs#L389) | B — Bug latente | 🟡 Médio | ✅ **Aprovado** | `OverrideVaultObstacleDistance` delimitado por `try / finally` estritamente em `TryVaulting`. |
| `CR-02-05` | [PlayerLadderController.cs:L55](../../modded/ladders.bep/PlayerLadderController.cs#L55) | F — Otimização | 🟡 Médio | ✅ **Aprovado** | `cachedInventoryWeightFactor` calculado no `Init()` e lido em `Update()`. |
| `CR-02-06` | [Ladder.cs:L19](../../modded/ladders.shared/Ladder.cs#L19) & [LaddersLoader.cs:L80](../../modded/ladders.bep/LaddersLoader.cs#L80) | D — Arquitetura | 🔵 Baixo | ✅ **Aprovado** | `Ladder.ClearRegistry()` executado no descarregamento da cena entre raids. |
| `CR-02-07` | [FikaHandler.cs:L92](../../modded/ladders.fika/FikaHandler.cs#L92) | B — Bug latente | 🔵 Baixo | ✅ **Aprovado** | `try / catch (InvalidOperationException)` blinda fallback de jogadores contra spawn assíncrono. |
| `CR-02-08` | [FikaHandler.cs:L164](../../modded/ladders.fika/FikaHandler.cs#L164) | D — Arquitetura | 💡 Otimização | ✅ **Aprovado** | Invocação nativa de `manager.UnregisterPacket<T>()` sem Reflection. |

---

## 3. Análise Crítica por Arquivo e Trecho Modificado

### 3.1. `LaddersLoader.cs` — Descarregamento Assíncrono e Limpeza de Registry
* **Trecho Auditado:**
  ```csharp
  public void Unload()
  {
      if (scene.isLoaded)
      {
  #if DEBUG
          SceneManager.UnloadScene(scene); // unsafe apparently (unity docs say so), but required for hot reload
  #else
          SceneManager.UnloadSceneAsync(scene);
  #endif
      }
      if (sceneBundle != null)
          sceneBundle.Unload(false);

      Ladder.ClearRegistry();
  }
  ```
* **Análise:** 
  A substituição de `#elif RELEASE` por `#else` é gramaticalmente irrefutável no C# e garante que o método assíncrono de descarregamento do Unity seja emitido pelo compilador Roslyn. A adição de `Ladder.ClearRegistry()` após o descarregamento garante higiene do dicionário estático entre partidas consecutivas sem reiniciar o jogo.

---

### 3.2. `ProxyTransformModifierByPath.cs` — Coordenadas Locais vs Globais
* **Trecho Auditado:**
  ```csharp
  originalStates.Add(target, new TransformData
  {
      localPos = target.transform.localPosition,
      localRot = target.transform.localRotation,
      localScale = target.transform.localScale
  });
  ```
* **Análise:**
  Anteriormente, o código lia `target.transform.position` (coordenada de mundo) e gravava no campo `localPos`, que era posteriormente atribuído a `target.transform.localPosition` no `OnDestroy()`. Ao capturar `localPosition` e `localRotation`, preserva-se estritamente a posição relativa ao nó pai da hierarquia do mapa, sanando qualquer desalinhamento espacial de objetos móveis.

---

### 3.3. `ProceduralGrip.cs` — Zero-Alloc no Hot Path de Animação Procedural
* **Trecho Auditado:**
  ```csharp
  void SetCurl(float t)
  {
      currentCurl = t;

      for (int i = 0; i < _fingers.Count; i++)
      {
          Finger finger = _fingers[i];
          float curlAngle = Mathf.Lerp(finger.MinCurl, finger.MaxCurl, t);
          Quaternion curlRotation = Quaternion.AngleAxis(curlAngle, _bendAxis);

          if (finger.Base != null) _fingerRotations[finger.Base.Index] = finger.Base.RestRotation * curlRotation;
          if (finger.Mid != null) _fingerRotations[finger.Mid.Index] = finger.Mid.RestRotation * curlRotation;
          if (finger.Tip != null) _fingerRotations[finger.Tip.Index] = finger.Tip.RestRotation * curlRotation;
      }
  }
  ```
* **Análise:**
  A eliminação da propriedade iteradora `Finger.Joints` com `yield return` evita que o Mono Runtime aloque 10 instâncias de `IEnumerator<FingerJoint>` a cada chamada de `LateUpdate()`. O cálculo de `Quaternion.AngleAxis` foi hoisted para fora do acesso às juntas (executado uma vez por dedo em vez de três). Além disso, as proteções `if (finger.Base != null)` impedem exceções de ponteiro nulo em rigs assimétricos.

---

### 3.4. `PlayerLadderController.cs` — Isolamento de Vaulting e Cache de Peso
* **Trechos Auditados:**
  1. **Vaulting com escopo estrito:**
     ```csharp
     bool TryVaultingFakeForwardInput()
     {
         player.InputDirection = new Vector2(0, 1f);
         player.MovementContext.MovementDirection_1 = new Vector2(0, 1f);

         try
         {
             Patch_ObstacleCalculatorModel_DistanceToMainObstacle.OverrideVaultObstacleDistance = true;
             return player.MovementContext.TryVaulting();
         }
         finally
         {
             Patch_ObstacleCalculatorModel_DistanceToMainObstacle.OverrideVaultObstacleDistance = false;
         }
     }
     ```
  2. **Cache de peso no `Init()`:**
     ```csharp
     cachedInventoryWeightFactor = Mathf.Clamp01(Mathf.InverseLerp(20, 60, player.InventoryController?.TotalWeight() ?? 0f));
     ```
* **Análise:**
  1. A remoção da flag nos métodos `Init()` e `OnDestroy()` e sua contenção em `try / finally` garante que bots e colegas de equipe em cooperação no FIKA nunca tenham suas distâncias de obstáculo truncadas enquanto o jogador local escala.
  2. A leitura de `cachedInventoryWeightFactor` em `Update()` elimina chamadas repetidas a `InventoryController.TotalWeight()`, que percorria recursivamente todos os slots e contêineres de equipamento a cada frame.

---

### 3.5. `FikaHandler.cs` — Desregistro Nativo e Concorrência de Spawn
* **Trechos Auditados:**
  1. **Desregistro nativo limpo:**
     ```csharp
     void UnregisterPackets(IFikaNetworkManager manager)
     {
         if (manager == null) return;
         try
         {
             manager.UnregisterPacket<LadderStatePacket>();
             manager.UnregisterPacket<BarAnglePacket>();
         }
         catch (Exception ex)
         {
             Plugin.Logger.LogWarning($"[FikaHandler] Erro ao desregistrar pacotes: {ex.Message}");
         }
     }
     ```
  2. **Proteção contra concorrência em `AllPlayersEverExisted`:**
     ```csharp
     var allPlayers = gameWorld.AllPlayersEverExisted;
     if (allPlayers != null)
     {
         try
         {
             foreach (var p in allPlayers)
             {
                 if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                 {
                     return fp;
                 }
             }
         }
         catch (InvalidOperationException)
         {
             // Proteção contra modificação da coleção durante spawn concorrente
         }
     }
     ```
* **Análise:**
  1. Como `IFikaNetworkManager` no FIKA modded v2.3.11 provê nativamente `void UnregisterPacket<T>() where T : INetSerializable`, foram eliminados o campo estático `_cachedPacketProcessorField` e o método `GetPacketProcessor()`, removendo qualquer uso de Reflection frágil.
  2. A coleção `gameWorld.AllPlayersEverExisted` é exposta como `IEnumerable<Player>` (internamente `Dictionary.ValueCollection`). O encapsulamento com `try / catch (InvalidOperationException)` blinda o handler contra alterações de coleção caso ocorra um spawn de bot no exato momento da iteração de rede.

---

### 3.6. Versionamento SemVer e Integridade de Build
* `tarkin.ladders.bep`: bumped para **v1.0.7** (`Plugin.cs` e `tarkin.ladders.bep.csproj`).
* `tarkin.ladders.fika`: bumped para **v1.1.3** (`Plugin.cs`, `tarkin.ladders.fika.csproj` e `[BepInDependency("com.tarkin.ladders", "1.0.7")]`).
* Compilação com `dotnet build "mods/Climbable Ladders/modded/ladders.sln" -c Release -p:SPTPath="E:/Tarkov Red Line"`:
  * **0 Erros**
  * **0 Avisos**
  * **Isolamento de build 100% mantido:** Nenhuma DLL foi copiada para a pasta do jogo instalada; os artefatos permanecem estritamente dentro da árvore do mod.

---

## 4. Veredito Final

> ✅ **STATUS: APROVADO PARA PRODUÇÃO E MERGE**  
> Todos os 7 achados da Auditoria Técnica 02 foram plenamente sanados. O código está limpo, resiliente, otimizado para alto FPS (zero alocações em hot path) e perfeitamente compatível com o FIKA modded v2.3.11.
