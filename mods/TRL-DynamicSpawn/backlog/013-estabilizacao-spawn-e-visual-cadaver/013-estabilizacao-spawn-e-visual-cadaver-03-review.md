# 013 — Revisão Técnica Crítica: Estabilização de Spawn e Visual de Cadáver

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:04:00-03:00  
**Ref:** `013-estabilizacao-spawn-e-visual-cadaver`  

---

## 1. Avaliação Crítica da Abordagem

### 1.1. Sobre o Snap de Spawn (`OnBotCreatedSafetySnap`)
* **Ponto Forte:** A abordagem anterior tentava adivinhar a posição do chão através de um raycast descendente de 3 metros, ignorando que em Customs pontes e passarelas possuem geometrias suspensas. A nova restrição (|ΔY| < 0.35m e janela curta de 0.5m) protege pontes e mezaninos contra soterramento.
* **Crítica / Trade-off:**
  - Se um bot spawnar em uma encosta íngreme de montanha (ex.: Woods/Shoreline/Lighthouse), o ponto original da BSG pode ter os pés levemente inseridos na malha da terra (~0.10m). A restrição de 0.35m permite corrigir esse pequeno afundamento sem correr o risco de atravessar pontes ou telhados.
  - O código precisa garantir que `RaycastHit` valide camadas específicas de terreno/estático (`LayerMaskClass.HighPolyWithTerrainMask`) e descarte colisores de outros bots ou veículos dinâmicos.

### 1.2. Sobre o Desacoplamento da Mochila e Interação de Loot
* **Ponto Forte:** Desacoplar a mochila da hierarquia de `corpsePlayer` elimina completamente o acoplamento com os `Joints` e `Rigidbodies` do ragdoll, erradicando a fonte física da ejeção vertical ao céu.
* **Crítica / Risco:**
  - No EFT, a busca por objetos interagíveis (`GameWorld.FindInteractable`, `Player.cs:29625-29640`) procura na hierarquia do colisor atingido por `GetComponentInParent<InteractableObject>()`.
  - Se a mochila for um GameObject raiz separado e tiver apenas um `BoxCollider` sem ligação com o `Corpse`, o jogador verá a mochila mas **não conseguirá interagir**, pois o raycast atingirá a mochila e não encontrará o `Corpse` do bot!
  - **Requisito Crucial:** A mochila precisa obrigatoriamente conter o componente `InteractiveProxy` do EFT apontando para o `Corpse` do bot (`proxy.Link = corpsePlayer.Corpse`), ou a mochila precisa ser limpa de rigidbodies e seus colisores marcados com parâmetros que não colidam com o ragdoll (ex.: ignorar colisão entre os colliders do bot e o collider da mochila via `Physics.IgnoreCollision`).

### 1.3. Sobre o Ciclo de Vida da Mochila
* **Risco de Vazamento de Memória:** Se a mochila for desacoplada do `corpsePlayer.gameObject`, quando a raid terminar ou o cadáver for destruído/limpo, quem destrói o GameObject da mochila?
* **Mitigação Obrigatória:** A classe `TrackedCorpse` já possui o campo `SpawnedBackpackVisual`. Devemos garantir que o método `ClearStaticState()` e qualquer despawn explícito invoque `UnityEngine.Object.Destroy(tracked.SpawnedBackpackVisual)` para evitar vazamento de memória e mochilas órfãs.

---

## 2. Decisões Técnicas Consolidadas

1. **Snap Seguro:**
   - Reduzir a altura inicial do Raycast para `currentPos + Vector3.up * 0.5f`.
   - Limitar o comprimento do Raycast para `1.0f`.
   - Só executar `Teleport` se `Mathf.Abs(currentPos.y - rayHit.point.y) <= 0.35f`. Caso contrário, abortar e manter a posição nativa da BSG intacta.
2. **Preload do Bundle:**
   - Criar método assíncrono `PreloadMbssBackpackBundle()` executado durante o `RaidLifecycle.OnRaidStart`.
3. **Mochila Desacoplada e Interativa:**
   - Configurar `InteractiveProxy` na mochila apontando para o `corpsePlayer.Corpse`.
   - Remover qualquer `Rigidbody` do prefab da mochila de forma imediata antes de instanciar ou antes do primeiro tick de física.
   - Utilizar `Physics.IgnoreCollision` contra todos os colliders do ragdoll do bot como camada extra de segurança física.
