# 013 — Spec Técnica: Estabilização de Spawn e Visual de Cadáver

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:03:00-03:00  
**Ref:** `013-estabilizacao-spawn-e-visual-cadaver`  

---

## 1. Arquitetura e Análise de Causa-Raiz

### 1.1. Queda no Limbo (Assembly EFT & Física de Spawn)
* **Local:** `DynamicSpawnManager.cs:1862` (`OnBotCreatedSafetySnap`) e `BotDespawnManager.cs:763` (`AttemptToTeleportGroup`).
* **Mecanismo da Falha:**
  - O ponto de spawn fornecido pela cena da BSG (`ISpawnPoint.Position`) é confiável na maioria absoluta dos casos.
  - Ao executar `NavMesh.SamplePosition(currentPos, out hit, 2.5f)` e em seguida `Physics.Raycast(targetPos + Vector3.up * 1.5f, Vector3.down, 3.0f, ...)`, geometrias complexas de múltiplos níveis (pontes suspensas, passarelas de ferro perfuradas, dormitórios em Customs) provocam leituras ambíguas:
    - O raycast não detecta a malha perfurada ou atinge um piso inferior a metros de distância.
    - Se o raycast falha, `targetPos.y` herda o NavMesh que pode pertencer ao subsolo.
    - O teleporte imediato (`bot.GetPlayer.Teleport(targetPos, true)`) insere o `CharacterController` do jogador em colisão com a laje ou abaixo dela, e a rotina do PhysX repele o bot para baixo da colisão (queda no limbo).
* **Solução Técnica:**
  - Definir uma janela de segurança estrita:
    - O raycast deve descer de no máximo `0.5m` acima do ponto original (`currentPos + Vector3.up * 0.5f`) com comprimento de `1.0m`.
    - Se o raycast não atingir nada sólido OU se a altura detectada divergir mais de `0.35m` da altura original de spawn (`Mathf.Abs(currentPos.y - rayHit.point.y) > 0.35f`), **nenhum teleporte é executado**. A posição original da BSG é preservada intacta.
    - Não alterar a posição vertical baseado apenas em `NavMeshHit.position.y` se o `Physics.Raycast` não confirmar um solo sólido na mesma cota.

### 1.2. Cadáver Invisível por Falha de Bundle
* **Local:** `CorpseCleanupManager.cs:300-321` e `388-417` (`GetMbssBackpackPrefab`).
* **Mecanismo da Falha:**
  - O mod invoca `forceRenderingOff = true` em todos os renderers do corpo do bot antes de ter a certeza de que o prefab da mochila está carregado.
  - `Singleton<IEasyAssets>.Instance.GetAsset<GameObject>` retorna `null` se o bundle `assets/content/items/equipment/backpack_flyye_mbss/item_equipment_backpack_flyye_mbss.bundle` não fez parte dos pools da raid atual.
  - O prefab resultante é nulo, abortando a criação da mochila enquanto o corpo permanece invisível.
* **Solução Técnica:**
  - **Pre-warming:** Na inicialização da raid (`RaidLifecycle.OnRaidStart`), solicitar o carregamento assíncrono do prefab/bundle da Flyye MBSS (`544a5cde4bdc2d39388b456b`) através de `PoolManagerClass.Instance.LoadBundlesAndCreatePools` ou criação de item template.
  - **Fallback Atômico:** Em `ConvertToBackpack`, a ocultação dos renderers do corpo só é executada **APÓS** a instanciação bem-sucedida do visual da mochila. Se `GetMbssBackpackPrefab()` retornar nulo, o método emite log de warning e aborta sem ocultar nenhuma malha.

### 1.3. Ejeção da Mochila ao Céu (Efeito Catapulta do PhysX)
* **Local:** `CorpseCleanupManager.cs:341-375` (`ConvertToBackpack`).
* **Mecanismo da Falha:**
  - O prefab da mochila é instanciado e tornado filho direto de `corpsePlayer.gameObject.transform`.
  - O mod adiciona um `BoxCollider` não-trigger (`isTrigger = false`).
  - O `corpsePlayer` é o pai de todo o esqueleto do ragdoll (`PlayerBones`), cujos rigidbodies e colisores ainda estão na cena.
  - O método tenta remover rigidbodies do prefab usando `UnityEngine.Object.Destroy(vrb)`. O `Destroy()` na Unity é diferido para o final do quadro; durante o passo de física corrente, os colliders do prefab e do ragdoll sofrem despenetração violenta (depenetration force) contra o solo, catapultando o objeto para cima.
  - Quando os rigidbodies do ragdoll são colocados em `isKinematic = true`, a velocidade é congelada no topo da trajetória (mochila presa no céu).
* **Solução Técnica:**
  - **Desacoplamento Estrutural:** Não usar `SetParent(corpsePlayer.gameObject.transform)`!
  - Instanciar a mochila como um GameObject independente no mundo (`parent = null`).
  - Posicioná-la com base na posição da coluna (`Spine3`) ou pelve do bot com snap suave no chão via raycast.
  - Não adicionar `Rigidbody` na mochila.
  - O `BoxCollider` é adicionado como trigger ou com layer `DeadbodyLayer` configurada para colidir exclusivamente com raycasts de interação (`LootRaycast`), e não contra a malha física do ragdoll.
  - Anexar à mochila um script proxy simples (`ProxyTransportee` ou componente de vínculo) apontando para o `corpsePlayer.Corpse`, de modo que `GameWorld.FindInteractable` encontre o `LootItem` pai diretamente.

---

## 2. Alterações de Código Previstas

```
TRL-DynamicSpawn/modded/Client/
├── Components/
│   ├── DynamicSpawnManager.cs   --> OnBotCreatedSafetySnap restrito e seguro
│   ├── BotDespawnManager.cs     --> AttemptToTeleportGroup com trava idêntica
│   └── CorpseCleanupManager.cs  --> Preload MBSS, desacoplamento físico e fallback
├── Helpers/
│   └── RaidLifecycle.cs         --> Chamada de Preload no início da raid
└── Plugin.cs / .csproj          --> Bump de versão para 3.7.8
```

---

## 3. Riscos Técnicos e Mitigações

1. **Risco:** Desacoplar a mochila do GameObject do bot pode quebrar a referência de `GetComponentInParent<InteractableObject>()` durante o raycast do jogador.
   - **Mitigação:** Anexar o script `InteractiveProxy` nativo do EFT (`InteractiveProxy.Link = corpsePlayer.Corpse`), que o próprio EFT suporta nativamente em `Player.cs:29632-29637`.
2. **Risco:** Pre-load assíncrono de bundle no início da raid causar pequeno atraso.
   - **Mitigação:** O bundle da mochila MBSS possui apenas ~2 MB e carrega em menos de 15ms em SSD através de `ItemFactoryClass` / `PoolManagerClass`.
