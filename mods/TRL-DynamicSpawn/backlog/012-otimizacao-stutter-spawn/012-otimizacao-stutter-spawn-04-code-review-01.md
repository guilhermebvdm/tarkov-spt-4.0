# 012 — Code Review (01)

**Item:** 012-otimizacao-stutter-spawn  
**Revisor:** Antigravity  
**Data:** 2026-09-15  
**Parecer:** 🟢 Aprovado  

---

## 1. Inventário de Arquivos e Diffs

| Arquivo | Tipo de Alteração | Rastreabilidade |
| :--- | :---: | :---: |
| `modded/Client/Helpers/LoSCache.cs` | CRIADO | `CR-012-05` |
| `modded/Client/Components/DynamicSpawnManager.cs` | MODIFICADO | `CR-012-01`, `CR-012-02`, `CR-012-03`, `CR-012-04`, `CR-012-05` |
| `modded/Client/Patches/Patches.cs` | MODIFICADO | `CR-012-05` |
| `modded/Client/Plugin.cs` | MODIFICADO | `CR-012-06` (Bump SemVer 3.7.7) |
| `modded/Client/TRL-DynamicSpawn-Client.csproj` | MODIFICADO | `CR-012-06` (Bump SemVer 3.7.7) |
| `modded/Server/TRL-DynamicSpawn-Server.csproj` | MODIFICADO | `CR-012-06` (Bump SemVer 3.7.7) |
| `modded/Server/ModMetadata.cs` | MODIFICADO | `CR-012-06` (Bump SemVer 3.7.7) |
| `mod.json` | MODIFICADO | `CR-012-06` (Bump SemVer 3.7.7) |

---

## 2. Auditoria Detalhada por Componente

### 2.1. Desassociação e Correção de IDs de Rastreabilidade (`DynamicSpawnManager.cs`)
- **Antes:** Linhas 1181 e 1214 usavam indevidamente `ref: AUD-02-01` e `ref: AUD-02-02` (que colidiam com achados de injeção fracionada e nós de patrulha do `BotDespawnManager`).
- **Depois:**
  - `DynamicSpawnManager.cs:1181`: Atualizado para `// ref: CR-012-01 — Geração atômica de esquadrão em uma única Task assíncrona`.
  - `DynamicSpawnManager.cs:1214`: Atualizado para `// ref: CR-012-02 — Pré-Carregamento Assíncrono de Bundles (Pre-warming)`.
  - Colisão formalmente eliminada.

### 2.2. Pré-Carregamento de Bundles (`DynamicSpawnManager.cs:1214-1247`)
- Chamada via `Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools`:
  - Categoria: `PoolsCategory.Raid`.
  - AssemblyType: `AssemblyType.Local`.
  - Prioridade: `JobPriorityClass.General`.
  - Token de Cancelamento: `PoolManagerClass.DefaultCancellationToken`.
- Bloqueio síncrono da main thread substituído por espera de corrotina (`while (!prewarmTask.IsCompleted) yield return null;`), garantindo que o carregamento ocorra com frames fluindo.

### 2.3. Spawning Escalonado e Sucessão de Liderança (`DynamicSpawnManager.cs:1265-1400`)
- **Isolamento de Membros:** O spawner divide a instanciação física em fatias temporais mantendo a raiz `_profileData` idêntica.
- **Temporização Segura:** O intervalo intra-esquadrão aplica `Mathf.Clamp(Settings.smoothSpawningDelay.Value, 0.8f, 1.8f)`, protegendo o esquadrão contra dilatações extremas caso o slider do F12 esteja em valores altos.
- **Sucessão de Liderança:** Adicionado bloco que checa `leaderBot.HealthController.IsAlive`. Caso o líder tombe prematuramente, o seguidor subsequente é promovido a líder via `followerBot.Boss.SetBoss(...)`, prevenindo seguidores órfãos.

### 2.4. Utilitário `LoSCache.cs`
- **Filtro Estrito:** `diff.sqrMagnitude > 150f * 150f` aborta o cálculo de LoS imediatamente.
- **Quantização:** Grade de 1.5m reduz redundância de checagens em micro-movimentações.
- **Teto Bounded:** `MAX_CACHE_ENTRIES = 1000` com purga automática a cada 5 segundos impede memory leaks em raids longas.

---

## 3. Conclusão da Auditoria

O código está limpo, bem documentado, livre de regressões e atende integralmente a todos os critérios de aceite estabelecidos no item 012.
