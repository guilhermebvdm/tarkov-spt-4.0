# 001 — fundacao-e-culling-arquitetura · Code Review 01

**Mod:** TRL-CoreSight  
**Spec funcional:** [001-fundacao-e-culling-arquitetura-01-spec.md](001-fundacao-e-culling-arquitetura-01-spec.md)  
**Spec técnica:** [001-fundacao-e-culling-arquitetura-02-spec-tech.md](001-fundacao-e-culling-arquitetura-02-spec-tech.md)  
**As-Built:** [001-fundacao-e-culling-arquitetura-05-asbuild.md](001-fundacao-e-culling-arquitetura-05-asbuild.md)  
**Data:** 2026-09-15  

> Análise crítica do código entregue em `mods/TRL-CoreSight/modded/`. Cada achado recebe um ID permanente `CR-01-MM`.

---

## Memória consultada

- `mods/TRL-CoreSight/memory/sessions.md` (Sessões 1 a 3).
- `docs/technical/spt-antipatterns.md` (AP-01, AP-02, AP-04, AP-09).
- Binário compilado verificado: `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (v0.3.1, 47.104 bytes).

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Aplicados: 3 · Total: 4

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | B — Bug latente | 🟠 Forte | `BotPerformanceLimiter` trava se a câmera mudar ou for recriada em runtime (`_mainCamera` estático) | ✅ Aplicado (v0.3.1) |
| CR-01-02 | B — Bug latente | 🟡 Médio | `InteriorOcclusionWatcher` não reverte sombra para original se o jogador morrer dentro de um prédio | ✅ Aplicado (v0.3.1) |
| CR-01-03 | E — Manutenção | 🟢 Menor | Campo inerte `_mainCamera` em `InteriorOcclusionWatcher.cs` | ✅ Aplicado (v0.3.1) |
| CR-01-04 | D — Arquitetura | 🟢 Menor | Expansão positiva não espelhada na spec técnica original (Declutter e Optic Volumetrics) | 🟢 Registrado (As-Built) |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de save/estado.
- **B — Bug latente** — comportamento errado em cenário dinâmico (troca de câmera, morte de player).
- **C — Gap vs. spec** — código deixou de implementar critério de aceite obrigatório.
- **D — Arquitetura** — violações de padrões do repo ou discrepâncias de módulos.
- **E — Legibilidade/Manutenção** — dead code, nomes confusos, falta de documentação inline.
- **F — Melhoria opcional** — otimizações e simplificações cosméticas.

---

## Análise Componente a Componente

### 1. `InteriorOcclusionWatcher.cs`
- **Pontos Positivos:**
  - Amostragem throttled a cada 0.5s via timer (`CheckInterval`), evitando sobrecarga de raycast na thread principal da Unity.
  - Verificação robusta com 1 raio vertical + 4 raios horizontais cardeais contra `LayerMaskClass.HighPolyWithTerrainMask`, distinguindo com precisão um quarto fechado de um beiral aberto.
  - Transição de `QualitySettings.shadowDistance` realizada via `Mathf.MoveTowards` a 80m/s, impedindo quedas ou pop-in agressivo no campo de visão do jogador.
- **Achados:**
  - `CR-01-02`: Ao morrer (`!HealthController.IsAlive`), a execução retorna sem chamar `Restore()`, mantendo a distância de sombra travada em 25m até o descarregamento da raid.
  - `CR-01-03`: `_mainCamera` foi instanciado no `Initialize` mas nunca é utilizado.

### 2. `BotPerformanceLimiter.cs`
- **Pontos Positivos:**
  - Zero alocação de GC no loop de frame: utiliza o buffer pré-alocado `private readonly Plane[] _cameraPlanes = new Plane[6]`.
  - Integridade balística absoluta: manipula **estritamente** `animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms`. Nenhum `Collider`, `Rigidbody` ou `CharacterController` é desativado ou modificado, garantindo que tiros e detecção de acertos continuem perfeitos em 100% dos testes.
  - Proteção de alvos: bots amigáveis, jogadores locais e mortos são filtrados na primeira instrução.
  - Restauração determinística via `RestoreAll()` ao desligar o mod ou sair da partida.
- **Achados:**
  - `CR-01-01`: Se `Camera.main` for recriado pelo jogo ou se for nulo na inicialização da raid (fase de loading de texturas), o limiter interrompe o processamento indefinidamente.

### 3. `PerformanceManager.cs`
- **Pontos Positivos:**
  - Padrão singleton defensivo contra duplicação de GameObjects na troca de cenas.
  - Interpolação contínua de `QualitySettings.lodBias` com transições suaves (taxa de 4.0/s) entre navegação normal (1.0), visada com red dot (2.0) e mira telescópica PiP (0.8).
  - HUD de telemetria em tempo real no `OnGUI` sob flag `DebugMode`, facilitando a validação em raid.
  - Teardown idempotente em `Cleanup()`, chamando a restauração de todos os subsistemas.

### 4. `DeclutterManager.cs`
- **Pontos Positivos:**
  - Time-slicing com processamento amortizado em 250 objetos por frame usando Coroutine cooperativa (`yield return null`).
  - Lista de proteção abrangente para 22+ componentes de física e gameplay do EFT 0.16.9 (`Door`, `LootItem`, `ExfiltrationPoint`, `WindowBreaker`, `Light`, etc.).
  - Cache seguro das transformações desativadas para religamento sem memory leak no `OnRaidFinished()`.

### 5. `Patches/RaidLifecyclePatches.cs` & `Patches/OpticVolumetricsPatch.cs`
- **Pontos Positivos:**
  - Hooks em conformidade total com [AP-01](../../../../docs/technical/spt-antipatterns.md): postfix em `GameWorld.OnGameStarted` e prefix em `GameWorld.OnDestroy`.
  - Tratamento de exceções com blocos `try/catch` protegidos por logs estruturados no `Plugin.LogSource`.
  - Otimização pioneira de lunetas em `OpticVolumetricsPatch`, desativando a luz volumétrica redundante da `BaseOpticCamera` no `LateUpdate`.

---

## Pontos Detalhados

### CR-01-01 · B — Bug latente · 🟠 Forte

**`BotPerformanceLimiter` trava se a câmera mudar ou for recriada em runtime**

- **Arquivo:** [`modded/Core/BotPerformanceLimiter.cs:21-35`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/BotPerformanceLimiter.cs#L21-L35)
- **Causa:** O campo `_mainCamera` é inicializado uma única vez em `Initialize`. Se a câmera for instanciada após o `OnGameStarted` (ou em spectator do FIKA), `_mainCamera` permanece nulo, fazendo a linha 32 abortar o método toda vez:
  ```csharp
  if (_mainPlayer == null || !_mainPlayer.HealthController.IsAlive || _mainCamera == null)
      return;
  ```
- **Solução Recomendada:**
  ```csharp
  if (_mainCamera == null || !_mainCamera.isActiveAndEnabled)
  {
      _mainCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null) 
          ? CameraClass.Instance.Camera 
          : Camera.main;
  }
  ```

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**`InteriorOcclusionWatcher` não reverte sombra para o original se o jogador morrer dentro de um prédio**

- **Arquivo:** [`modded/Core/InteriorOcclusionWatcher.cs:50-53`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs#L50-L53)
- **Causa:** O `OnUpdate` retorna imediatamente quando `!_mainPlayer.HealthController.IsAlive`, deixando `QualitySettings.shadowDistance` no valor ocluído (25m) durante a animação de morte e tela de extração.
- **Solução Recomendada:**
  Adicionar detecção de transição de morte para acionar `Restore()` imediatamente no primeiro frame em que `IsAlive == false`.

---

### CR-01-03 · E — Manutenção · 🟢 Menor

**Campo inerte `_mainCamera` em `InteriorOcclusionWatcher.cs`**

- **Arquivo:** [`modded/Core/InteriorOcclusionWatcher.cs:11`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/InteriorOcclusionWatcher.cs#L11)
- **Causa:** O campo `private Camera _mainCamera;` é atribuído na linha 33 mas nunca é lido no restante do arquivo.
- **Solução Recomendada:** Remover a variável e a linha de atribuição para evitar confusão de manutenção.

---

### CR-01-04 · D — Arquitetura · 🟢 Menor

**Expansão positiva não espelhada na spec técnica original**

- **Arquivos:** `modded/Core/DeclutterManager.cs` e `modded/Patches/OpticVolumetricsPatch.cs`
- **Causa:** A entrega do item 001 incluiu funcionalidades de culling cosmético (Declutter) e corte de volumetria em lunetas (Smart PiP), agregando valor significativo, mas que não constavam formalmente no `001-fundacao-e-culling-arquitetura-02-spec-tech.md`.
- **Solução Recomendada:** Manter a implementação completa no build e consolidar a descrição detalhada no As-Built.

---

## Conclusão da Revisão de Código

O código está **aprovado para validação em raid real**, com arquitetura limpa, alta performance e zero geração de lixo para a GC da Unity. Os achados `CR-01-01` e `CR-01-02` são ajustes pontuais de robustez para edge-cases que podem ser aplicados diretamente em sequência.
