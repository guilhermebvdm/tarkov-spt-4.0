# 013 — As-Built: Estabilização de Spawn e Visual de Cadáver

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento (Aguardando validação em raid real)  
**Entregue:** 2026-09-15T09:30:00-03:00  
**Versão:** 3.7.8  
**Ref:** `013-estabilizacao-spawn-e-visual-cadaver`  

---

## 1. Resumo da Entrega

Foram eliminadas as causas físicas e visuais dos comportamentos anômalos relatados em Customs:
1. **Eliminação de Queda no Limbo:** Redefinição estrita da janela de raycast vertical de `OnBotCreatedSafetySnap` e `BotDespawnManager.AttemptToTeleportGroup`, impedindo teletransportes descendentes para o subsolo ou através de pontes/passarelas perfuradas em Customs.
2. **Eliminação de Cadáveres Invisíveis:**
   - Adicionado pré-carregamento assíncrono do bundle da mochila Flyye MBSS (`544a5cde4bdc2d39388b456b`) em `RaidLifecycle.OnRaidStart` via `PoolManagerClass.Instance.LoadBundlesAndCreatePools`.
   - Adicionado fallback atômico em `ConvertToBackpack`: se o prefab da mochila for nulo, o método aborta sem ocultar nenhuma malha humana do bot.
3. **Eliminação do Efeito Catapulta da Mochila ao Céu:**
   - Destruição imediata (`DestroyImmediate`) de rigidbodies e colliders residuais do prefab.
   - Aplicação de `Physics.IgnoreCollision` mútuo entre o colisor da mochila e todos os membros do ragdoll do cadáver.
   - Anexação segura sob `corpsePlayer.gameObject` garantindo que o `BoxCollider` na layer `Deadbody` permita acesso imediato ao inventário de loot via `GetComponentInParent<InteractableObject>()`.

---

## 2. Arquivos Modificados

| Arquivo | Mudanças Principais |
| :--- | :--- |
| `Components/DynamicSpawnManager.cs` | Snap de spawn estrito (|ΔY| <= 0.35m, raycast curto de 1.0m, sem soterramento). |
| `Components/BotDespawnManager.cs` | Replicação da mesma segurança de snap vertical no teleporte de grupos fora da bolha. |
| `Components/CorpseCleanupManager.cs` | `PreloadMbssBackpackBundle`, fallback atômico, `DestroyImmediate`, `IgnoreCollision` e restauração de interação. |
| `Helpers/RaidLifecycle.cs` | Chamada ao `PreloadMbssBackpackBundle` em `OnRaidStart`. |
| `Plugin.cs`, `.csproj`, `ModMetadata.cs` | Incremento de versão SemVer para 3.7.8. |

---

## 3. Próximo Passo

- Validação pelo usuário em raid real em Customs:
  1. Verificar ausência de bots caindo no limbo próximo a pontes e dormitórios.
  2. Verificar se cadáveres convertidos viram a mochila apoiada no chão sem voar para o céu e com prompt "Search" funcional.
- Após a validação em raid, o status deste item poderá ser promovido para 🟢 Entregue, e o Item 014 (Sincronização FIKA) poderá ser implementado.
