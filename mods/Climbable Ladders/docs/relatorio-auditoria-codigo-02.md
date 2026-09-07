---
title: "Relatório de Auditoria Técnica de Código — Climbable Ladders (Review 02)"
date: 2026-09-03
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — Climbable Ladders (Review 02)

Este documento apresenta a **segunda auditoria técnica estática profunda e minuciosa** do código-fonte do mod **Climbable Ladders** (v1.0.6 / Fika v1.1.2), cobrindo os módulos client-side BepInEx (`ladders.bep`), componentes compartilhados de cena (`ladders.shared`) e sincronização cooperativa Fika (`ladders.fika`).

A análise inclui validação cruzada com o Assembly EFT 0.16.9 (`Assembly-CSharp`), servidor SPT 4.0, framework multiplayer `Fika.Core` e a verificação estrita das 6 Dimensões Técnicas definidas no repositório.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 1 | Vazamento permanente de cenas aditivas em Release por diretiva pré-processador órfã (`#elif RELEASE`) |
| 🟠 **Alto** | 1 | Corrupção de coordenadas de restauração em `ProxyTransformModifierByPath` (posição de mundo em variável local) |
| 🟡 **Médio** | 3 | Pressão de GC por alocação de `IEnumerator` em hot path, efeito colateral global em vaulting e polling de peso |
| 🔵 **Baixo** | 2 | Falta de `.Clear()` explícito em `Ladder.registry` no fim de raid e loop `foreach` instável em Fika fallback |
| 💡 **Otimização** | 2 | Cache de peso de inventário durante escalada e eliminação de alocações em rig procedural de dedos |

---

## 2. Tabela Consolidada de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-02-01` | 🔴 Crítico | [LaddersLoader.cs:L73](../modded/ladders.bep/LaddersLoader.cs#L73) | Memory Leak / AP-01 | Cenas aditivas de escadas nunca são descarregadas em builds Release por diretiva `#elif RELEASE` inexistente. |
| `AUD-02-02` | 🟠 Alto | [ProxyTransformModifierByPath.cs:L77](../modded/ladders.shared/ProxyTransformModifierByPath.cs#L77) | Bug / Posição de Cena | Coordenada de mundo (`position`) armazenada em variável de coordenada local, distorcendo o objeto na restauração. |
| `AUD-02-03` | 🟡 Médio | [ProceduralGrip.cs:L32](../modded/ladders.bep/ProceduralGrip.cs#L32) | GC Pressure / Hot Path | Propriedade `Joints` com `yield return` aloca iterador no Heap a cada frame para 10 dedos (600–1440 allocs/s). |
| `AUD-02-04` | 🟡 Médio | [Patch_VaultingComponent.cs:L45](../modded/ladders.bep/Patch_VaultingComponent.cs#L45) | Concorrência / AP-03 | Flag estática `OverrideVaultObstacleDistance` ativa na escalada inteira afeta IA e outros jogadores sem isolamento. |
| `AUD-02-05` | 🟡 Médio | [PlayerLadderController.cs:L340](../modded/ladders.bep/PlayerLadderController.cs#L340) | Update / CPU Waste | Chamada de `TotalWeight()` a cada frame em `Update()` para peso invariante durante a subida. |
| `AUD-02-06` | 🔵 Baixo | [Ladder.cs:L19](../modded/ladders.shared/Ladder.cs#L19) | Teardown / AP-01 | Dicionário estático `Ladder.registry` sem rotina de limpeza no descarregamento de raid. |
| `AUD-02-07` | 🔵 Baixo | [FikaHandler.cs:L96](../modded/ladders.fika/FikaHandler.cs#L96) | Concorrência | Iteração via `foreach` em `AllPlayersEverExisted` vulnerável a `InvalidOperationException` durante spawn de bots. |

---

## 3. Detalhamento dos Achados

### AUD-02-01 · Vazamento Permanente de Cenas Aditivas em Release por Diretiva `#elif RELEASE` Inexistente
- **Severidade:** 🔴 Crítico
- **Evidência:** Forte (confirmado por análise léxica do compilador Roslyn e inspeção dos arquivos `.csproj`)
- **Localização no Mod:** [LaddersLoader.cs:L71-L76](../modded/ladders.bep/LaddersLoader.cs#L71-L76)
- **Referência Cruzada:** [tarkin.ladders.bep.csproj:L9](../modded/ladders.bep/tarkin.ladders.bep.csproj#L9) e [AP-01](../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:**
  No método `Unload()` de `LaddersLoader.cs`, o código faz a seguinte verificação condicional:
  ```csharp
  if (scene.isLoaded)
  {
  #if DEBUG
      SceneManager.UnloadScene(scene);
  #elif RELEASE
      SceneManager.UnloadSceneAsync(scene);
  #endif
  }
  ```
  No .NET SDK padrão, compilações Release definem apenas que `DEBUG` não existe; a constante `RELEASE` **nunca é definida por padrão** a menos que seja explicitamente adicionada em `<DefineConstants>`. Como `tarkin.ladders.bep.csproj` não define `RELEASE`, o compilador C# ignora silenciosamente o bloco `#elif RELEASE`.
- **Impacto Técnico Real:**
  Em todas as compilações Release (`dotnet build -c Release`), a chamada `SceneManager.UnloadSceneAsync(scene)` **não é compilada no binário**. Ao término de cada partida, a cena aditiva contendo todas as escadas do mapa permanece em memória no Unity. Em raids subsequentes, novas cenas são sobrepostas sobre as anteriores, gerando acúmulo infinito de memória física (RAM Leak) e colisores duplicados.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Substituir a diretiva condicional por `#if DEBUG ... #else ... #endif` ou executar sempre `SceneManager.UnloadSceneAsync`:
  ```csharp
  if (scene.isLoaded)
  {
  #if DEBUG
      SceneManager.UnloadScene(scene);
  #else
      SceneManager.UnloadSceneAsync(scene);
  #endif
  }
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-02 · Corrupção de Coordenadas em `ProxyTransformModifierByPath`
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (análise matemática e estrutural de `Transform`)
- **Localização no Mod:** [ProxyTransformModifierByPath.cs:L77-L80](../modded/ladders.shared/ProxyTransformModifierByPath.cs#L77-L80) e [ProxyTransformModifierByPath.cs:L39-L41](../modded/ladders.shared/ProxyTransformModifierByPath.cs#L39-L41)
- **Causa Raiz:**
  No momento de capturar o estado original (`ApplyTransform`):
  ```csharp
  originalStates.Add(target, new TransformData
  {
      localPos = target.transform.position, // Posição de MUNDO
      localRot = target.transform.rotation, // Rotação de MUNDO
      localScale = target.transform.localScale
  });
  ```
  Ao restaurar (`OnDestroy`):
  ```csharp
  kvp.Key.transform.localPosition = kvp.Value.localPos; // Aplicado como LOCAL
  kvp.Key.transform.localRotation = kvp.Value.localRot; // Aplicado como LOCAL
  ```
- **Impacto Técnico Real:**
  Para qualquer objeto aninhado em uma hierarquia (com `parent != null`), atribuir as coordenadas mundiais em `localPosition` e `localRotation` desloca o objeto para uma posição matemática completamente errada no mapa ao final do ciclo de vida do modificador.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Gravar explicitamente `target.transform.localPosition` e `target.transform.localRotation`:
  ```csharp
  originalStates.Add(target, new TransformData
  {
      localPos = target.transform.localPosition,
      localRot = target.transform.localRotation,
      localScale = target.transform.localScale
  });
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-03 · Pressão de GC por Alocação de `IEnumerator` em Hot Path de Rigging (`Finger.Joints`)
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (alocação de closure/state machine em `yield return`)
- **Localização no Mod:** [ProceduralGrip.cs:L32-L41](../modded/ladders.bep/ProceduralGrip.cs#L32-L41) e [ProceduralGrip.cs:L204](../modded/ladders.bep/ProceduralGrip.cs#L204)
- **Causa Raiz:**
  A propriedade `Finger.Joints` utiliza iterador com `yield return`:
  ```csharp
  public IEnumerable<FingerJoint> Joints
  {
      get
      {
          if (Base != null) yield return Base;
          if (Mid != null) yield return Mid;
          if (Tip != null) yield return Tip;
      }
  }
  ```
  O método `SetCurl(float t)` itera sobre essa coleção a cada frame (`foreach (FingerJoint joint in finger.Joints)`). Como `SetCurl` roda no `LateUpdate()` para os 5 dedos de ambas as mãos (10 dedos), o Unity aloca 10 instâncias de `IEnumerator<FingerJoint>` por frame no Heap gerenciado.
- **Impacto Técnico Real:**
  Em 60–144 FPS, isso representa **600 a 1440 alocações por segundo** em um caminho quente de animação procedural, acelerando coletas de lixo do Mono GC e gerando micro-travamentos (*stutters*) perceptíveis enquanto o operador escala.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Substituir o `IEnumerable` com `yield` por iteração direta sobre os campos `Base`, `Mid`, `Tip` ou armazenar um array fixo pré-alocado `FingerJoint[]`:
  ```csharp
  public readonly FingerJoint[] JointsArray; // Inicializado uma única vez no construtor do Finger
  ```
  Ou diretamente no loop de `SetCurl`:
  ```csharp
  if (finger.Base != null) ApplyJointCurl(finger.Base, curlAngle);
  if (finger.Mid != null) ApplyJointCurl(finger.Mid, curlAngle);
  if (finger.Tip != null) ApplyJointCurl(finger.Tip, curlAngle);
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-04 · Efeito Colateral Global em Vaulting de Terceiros por Flag Estática Contínua
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (análise de escopo estático no Harmony Postfix)
- **Localização no Mod:** [Patch_VaultingComponent.cs:L45-L52](../modded/ladders.bep/Patch_VaultingComponent.cs#L45-L52) e [PlayerLadderController.cs:L98](../modded/ladders.bep/PlayerLadderController.cs#L98)
- **Referência Cruzada:** [Patch_Physical.cs:L24-L28](../modded/ladders.bep/Patch_Physical.cs#L24-L28) (onde o `AUD-01-03` realizou o isolamento de instância)
- **Causa Raiz:**
  `OverrideVaultObstacleDistance` é uma flag estática setada como `true` durante todo o tempo de escalada (`Init()` até `OnDestroy()`). O patch `Patch_ObstacleCalculatorModel_DistanceToMainObstacle` sobrescreve `DistanceToMainObstacle` para qualquer instância de `ObstacleCalculatorModel`:
  ```csharp
  [PatchPostfix]
  private static void PatchPostfix(ObstacleCalculatorModel __instance, ref float __result)
  {
      if (!OverrideVaultObstacleDistance) return;
      __result = Mathf.Min(__result, 0.499f);
  }
  ```
  Diferente do `Patch_Physical` (que foi corrigido no `AUD-01-03` para checar `__instance == mainPlayer.Physical`), este patch não verifica se o modelo de vaulting pertence ao jogador local na escada. Além disso, ele só é necessário durante a chamada de `TryVaultingFakeForwardInput()`, mas fica ativado por minutos enquanto o jogador sobe/espera na escada.
- **Impacto Técnico Real:**
  No Fika coop ou para bots de IA próximos, se um bot ou colega de equipe tentar transpor um obstáculo enquanto o jogador local estiver na escada, a distância calculada será truncada indevidamente para `0.499f`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Ativar a flag exclusivamente dentro do escopo de `TryVaultingFakeForwardInput()` e desativar imediatamente:
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
  E remover a ativação contínua no `Init()` e `OnDestroy()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-05 · Recálculo Redundante de `InventoryController.TotalWeight()` a Cada Frame
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (leitura do loop `Update()`)
- **Localização no Mod:** [PlayerLadderController.cs:L340](../modded/ladders.bep/PlayerLadderController.cs#L340)
- **Causa Raiz:**
  No `Update()`, a cada frame executa-se:
  ```csharp
  float inventoryWeightFactor = Mathf.Clamp01(Mathf.InverseLerp(20, 60, player.InventoryController.TotalWeight()));
  ```
  Durante a escalada na escada, os slots de mãos estão bloqueados e a interface de inventário não pode ser aberta pelo jogador. O peso total do equipamento permanece estritamente constante do momento em que o operador sobe até o momento em que desce.
- **Impacto Técnico Real:**
  Execução repetitiva e desnecessária de consultas na árvore de inventário a 144 FPS.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Calcular `cachedInventoryWeightFactor` uma única vez no `Init()` e armazenar como campo privado do `PlayerLadderController`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-06 · Falta de Teardown Explícito de `Ladder.registry` no Fim de Raid
- **Severidade:** 🔵 Baixo
- **Evidência:** Suspeita (prevenção de vazamento estático entre transições)
- **Localização no Mod:** [Ladder.cs:L19](../modded/ladders.shared/Ladder.cs#L19)
- **Referência Cruzada:** [Patch_GameWorld_Dispose.cs:L20](../modded/ladders.bep/Patch_GameWorld_Dispose.cs#L20) e [AP-01](../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:**
  `Ladder.registry` armazena instâncias de `Ladder` por NetId. Embora cada componente remova a si mesmo no `OnDestroy()`, um descarregamento assíncrono de cena ou encerramento de raid que não invoque `OnDestroy` em componentes inativos pode reter referências a `GameObject`s no dicionário estático entre partidas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Disponibilizar `public static void ClearRegistry() => registry.Clear();` e invocá-lo no `LaddersLoader.Unload()` ou no teardown de raid (`Patch_GameWorld_Dispose`).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-07 · Iteração `foreach` Não Thread-Safe sobre `AllPlayersEverExisted` em Fika Fallback
- **Severidade:** 🔵 Baixo
- **Evidência:** Média (concorrência de coleção durante spawn)
- **Localização no Mod:** [FikaHandler.cs:L96](../modded/ladders.fika/FikaHandler.cs#L96)
- **Causa Raiz:**
  No método `ResolvePlayerByNetId`, a varredura do fallback 3 utiliza `foreach`:
  ```csharp
  var allPlayers = gameWorld.AllPlayersEverExisted;
  if (allPlayers != null)
  {
      foreach (var p in allPlayers)
      {
          if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
              return fp;
      }
  }
  ```
  Se uma nova entidade entrar na lista durante a execução do pacote de rede, o enumerador do `foreach` lança `InvalidOperationException`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Substituir por loop indexado com proteção de bounds, idêntico ao fallback 2:
  ```csharp
  var allPlayers = gameWorld.AllPlayersEverExisted;
  if (allPlayers != null)
  {
      for (int i = 0; i < allPlayers.Count; i++)
      {
          var p = allPlayers[i];
          if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
              return fp;
      }
  }
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

## 4. Plano de Ação e Recomendações

1. **Prioridade Imediata (Bloqueador de Produção):**
   - Corrigir a diretiva pré-processador em [LaddersLoader.cs:L73](../modded/ladders.bep/LaddersLoader.cs#L73) (`AUD-02-01`) substituindo `#elif RELEASE` por `#else`, garantindo que cenas aditivas sejam sempre descarregadas em builds de distribuição.
2. **Prioridade Alta:**
   - Corrigir a captura de coordenadas em [ProxyTransformModifierByPath.cs](../modded/ladders.shared/ProxyTransformModifierByPath.cs) (`AUD-02-02`) para usar `localPosition` e `localRotation`.
3. **Melhorias de Performance e Zero-Alloc:**
   - Eliminar as alocações contínuas de `IEnumerator` em `ProceduralGrip.cs` (`AUD-02-03`).
   - Escopar a flag estática `OverrideVaultObstacleDistance` exclusivamente à chamada de `TryVaulting` (`AUD-02-04`).
   - Realizar o cache de `inventoryWeightFactor` no `Init()` (`AUD-02-05`).
