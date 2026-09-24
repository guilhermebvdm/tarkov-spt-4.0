# 013 — Code Review: Estabilização de Spawn e Visual de Cadáver

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:15:00-03:00  
**Ref:** `013-estabilizacao-spawn-e-visual-cadaver`  

---

## 1. Escopo das Alterações Analisadas

1. `DynamicSpawnManager.cs:1862` (`OnBotCreatedSafetySnap`):
   - Redução do raio de raycast de 3.0m para 1.0m partindo de 0.5m acima do ponto original.
   - Verificação estrita de divergência vertical (|ΔY| <= 0.35m) e validação de NavMesh concomitante.
   - Abortagem segura preservando a posição nativa da BSG em geometrias suspensas ou perfuradas (Customs).
2. `BotDespawnManager.cs:760` (`AttemptToTeleportGroup`):
   - Replicação da mesma trava de segurança de raycast vertical (|ΔY| <= 0.35m) no reposicionamento de bots.
3. `CorpseCleanupManager.cs`:
   - `PreloadMbssBackpackBundle()`: pré-aquecimento assíncrono do bundle da mochila Flyye MBSS (`544a5cde4bdc2d39388b456b`).
   - `ConvertToBackpack`:
     - Fallback atômico: se `mbssPrefab == null`, aborta a conversão sem desativar renderers do corpo (elimina corpos invisíveis).
     - Destruição imediata (`DestroyImmediate`) de Rigidbodies e Colliders residuais do prefab.
     - `Physics.IgnoreCollision` mútuo contra todos os colliders do ragdoll do bot morto.
     - `boxCollider` sólido na layer `Deadbody` configurado como filho do `corpsePlayer` de modo que `GetComponentInParent<InteractableObject>()` encontre nativamente o `Corpse` do bot para saque.
4. `RaidLifecycle.cs`:
   - Acionamento em segundo plano de `PreloadMbssBackpackBundle()` no `OnRaidStart`.
5. Version Bump:
   - 3.7.7 -> 3.7.8 em `Plugin.cs`, `TRL-DynamicSpawn-Client.csproj`, `TRL-DynamicSpawn-Server.csproj` e `ModMetadata.cs`.

---

## 2. Parecer Crítico e Verificações

### 2.1. Análise de Físicas e Depenetration
- **Achado:** Ao substituir `Destroy()` diferido por `DestroyImmediate()` nos rigidbodies residuais do prefab da mochila, removemos qualquer possibilidade de o PhysX calcular forças dinâmicas com o prefab no primeiro frame.
- **Achado:** A invocação de `Physics.IgnoreCollision(boxCollider, cc, true)` para cada membro do ragdoll do cadáver garante que, mesmo que a pelve ou coluna sofram pequenas acomodações no chão, nenhuma força de repulsão seja aplicada entre a mochila e os ossos. Isso erradica o vetor vertical de catapultamento.

### 2.2. Acesso à Interação de Saque
- **Achado:** Como a mochila agora é anexada com `transform.SetParent(corpsePlayer.gameObject.transform, true)` após a desativação de forças e possui `BoxCollider` na layer `Deadbody`, o raycast do jogador (`GameWorld.FindInteractable`) atinge o colisor da mochila e executa `gameObject.GetComponentInParent<InteractableObject>()`, localizando o componente `Corpse` no pai sem necessidade de proxies externos instáveis.

### 2.3. Não-Regressão e Estabilidade
- **Achado:** A compilação do Client e do Server completou com zero erros.
- **Ressalva:** A validação final em raid real é mandatória para certificar o comportamento em pontos críticos de Customs (ponte do rio, mezanino de dormitórios e caçambas).

---

## 3. Conclusão

Código limpo, seguro, defensivo e aderente aos requisitos funcionais. Aprovado para testes em raid real.
