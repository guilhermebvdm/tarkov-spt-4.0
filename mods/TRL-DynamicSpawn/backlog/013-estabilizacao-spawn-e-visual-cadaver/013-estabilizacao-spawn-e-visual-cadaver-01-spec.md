# 013 — estabilizacao-spawn-e-visual-cadaver

**Mod:** TRL-DynamicSpawn  
**Status:** 🔵 Em andamento  
**Criado:** 2026-09-15T09:02:00-03:00  

> **Perfil desta spec:** Correção Funcional de Spawn e Física de Cadáver. Eliminação de queda de bots no limbo em geometrias verticais e correção dos quatro defeitos visuais/físicos do subsistema de cadáveres (`CorpseCleanupManager`): corpos invisíveis sem mochila, mochilas catapultadas ao céu pelo PhysX, perda de interação de saque e travamento de física de ragdoll.

---

## 1. Visão Geral

Durante testes em raid no mapa **Customs**, dois grupos de problemas severos de física e renderização foram identificados:
1. **Bots caindo no limbo:** Bots recém-spawnados sofriam teletransporte forçado para debaixo do piso em pontes, passarelas elevadas e armazéns industriais, caindo infinitamente no vazio.
2. **Defeitos no subsistema de cadáveres (`CorpseCleanupManager`):**
   - Corpos de bots mortos tornavam-se totalmente invisíveis sem transformar em mochila.
   - Mochilas transformadas sofriam aceleração vertical instantânea e ficavam presas imóvel no céu a dezenas de metros de altura.
   - O prompt e a interação de saque ("Search") com o cadáver deixavam de funcionar.
   - Conflito entre a destruição assíncrona de colisores do prefab da mochila e a resolução de despenetração contínua do PhysX com o ragdoll do bot.

---

## 2. Comportamento Desejado

### 2.1. Snap de Spawn Seguro e Não-Destrutivo
- O método `OnBotCreatedSafetySnap` em `DynamicSpawnManager.cs` (e o alinhamento equivalente em `BotDespawnManager.AttemptToTeleportGroup`) só deve teletransportar o bot se o teste vertical de piso encontrar uma superfície sólida contínua com discrepância estritamente plausível (|ΔY| < 0.45m).
- Caso o raycast vertical atinja o vazio, caia em subsolos distantes ou atinja tetos/lajes de outros andares, o ponto original do `ISpawnPoint` da BSG deve ser rigorosamente preservado, sem teletransporte descendente que fure o chão.

### 2.2. Pré-Carregamento do Asset da Mochila MBSS
- No início da raid (`RaidLifecycle.OnRaidStart`), o mod deve pré-carregar de forma segura o asset bundle da mochila Flyye MBSS (`544a5cde4bdc2d39388b456b`).
- Isso assegura que no momento da conversão de cadáver, `GetMbssBackpackPrefab()` nunca retorne nulo por ausência do asset na memória da raid.

### 2.3. Fallback Visual Obrigatório
- Em `ConvertToBackpack`, caso por qualquer motivo anômalo o prefab da mochila retorne nulo, o mod não deve ocultar as malhas do bot morto; se já as tiver ocultado, deve imediatamente restaurá-las (`forceRenderingOff = false`), impedindo que o bot se torne um fantasma invisível.

### 2.4. Desacoplamento Físico da Mochila (Fim do Efeito Catapulta)
- A mochila visual instanciada deve ser completamente desacoplada da hierarquia de ossos e rigidbodies do `corpsePlayer`.
- Em vez de depender de `Destroy()` diferido de Rigidbodies e Colliders de um prefab dinâmico que colide com o esqueleto do ragdoll no mesmo frame, a mochila é gerada no mundo estático com um `BoxCollider` na layer `Deadbody`, perfeitamente apoiada no chão, sem aplicar forças de repulsão no ragdoll.

---

## 3. Critérios de Aceite

### Não-Regressão
- [ ] **NR-1:** A capacidade de saquear armas, coletes e itens do bot morto permanece 100% funcional.
- [ ] **NR-2:** Spawns em terreno aberto comum (grama, terra batida) mantêm a estabilização contra afundamento leve de pés.
- [ ] **NR-3:** O desempenho de CPU/GPU de cadáveres congelados mantém o ganho obtido pelo modo `Backpack Convert` (0 draw calls para malhas humanas após a conversão).

### Metas Mensuráveis
- [ ] **AC-M1:** 0 ocorrências de bots caindo no limbo ao spawnar ou serem reposicionados em pontes, passarelas ou edifícios de múltiplos andares em Customs.
- [ ] **AC-M2:** 100% dos corpos convertidos no modo `Backpack Convert` exibem a mochila visual apoiada no chão (0 corpos invisíveis).
- [ ] **AC-M3:** 0 mochilas ejetadas para o céu (velocidade vertical da mochila = 0 em todos os frames).
- [ ] **AC-M4:** O ponto de interação de loot permanece sobre a mochila e responsivo ao raycast do jogador a ~1.5m de distância.
