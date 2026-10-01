# Memory — Climbable Ladders

Memória cronológica de sessões de desenvolvimento, auditoria e manutenção técnica do mod **Climbable Ladders** (SPT 4.0 / EFT 0.16.9 / Fika).

---

## Estado Atual (Snapshot)

- **Versão:** 
  - `tarkin.ladders.shared.dll` $\rightarrow$ **1.0.4**
  - `tarkin.ladders.bep.dll` $\rightarrow$ **1.0.7**
  - `tarkin.ladders.fika.dll` $\rightarrow$ **1.1.3** (compatibilidade nativa com FIKA modded v2.3.11)
- **Arquitetura:** Interceptação da física do jogador via `Patch_Physical` com isolamento de instância (`MainPlayer.Physical`), controle de estado via `PlayerLadderController` com restauração assíncrona defensiva de armas (`RestoreWeaponWhenReady`), ciclo de vida de cenas Release corrigido (`LaddersLoader`), rig procedural de mãos com FinalIK (`ProceduralGrip`) com alocação zero no hot path, sincronização em rede Fika nativa sem reflection (`IFikaNetworkManager.UnregisterPacket<T>()`), contenção de flags de vaulting com `try/finally` e registro estático com teardown entre raids (`Ladder.ClearRegistry()`).
- **Documentação:** Modular completa em `docs/` (6 artigos técnicos + README + Relatórios de Auditoria 01 e 02 + Relatório de Code Review 02).

---

## 2026-09-03 (GMT-3) — Sessão 4: Resolução dos Achados AUD-02-01 a AUD-02-07 e Code Review 02

- **Implementação dos Achados da Auditoria Técnica 02 (`/code-mod`)**:
  - **`AUD-02-01` & `AUD-02-06` (Cenas e Registry)**:
    - Em `LaddersLoader.cs`, substituído `#elif RELEASE` por `#else` garantindo descarregamento assíncrono de cenas aditivas em Release (`SceneManager.UnloadSceneAsync`).
    - Adicionado método estático `Ladder.ClearRegistry()` invocado em `LaddersLoader.Unload()`.
  - **`AUD-02-02` (Transform)**:
    - Em `ProxyTransformModifierByPath.cs`, gravado `localPosition` e `localRotation` na captura para garantir simetria exata na restauração de objetos de cena.
  - **`AUD-02-03` (Zero-Alloc Hot Path)**:
    - Em `ProceduralGrip.cs`, `SetCurl` modificado para aplicar rotações diretamente em `finger.Base`, `Mid` e `Tip`, eliminando 10 alocações de `IEnumerator` por frame no `LateUpdate()`.
  - **`AUD-02-04` & `AUD-02-05` (Vaulting e CPU)**:
    - Em `PlayerLadderController.cs`, `OverrideVaultObstacleDistance` isolado com `try / finally` exclusivamente dentro de `TryVaultingFakeForwardInput()`, removendo ativação contínua em `Init()` e `OnDestroy()`.
    - Cache do fator de peso `cachedInventoryWeightFactor` no `Init()`, eliminando chamadas recursivas a `InventoryController.TotalWeight()` a cada frame em `Update()`.
  - **`AUD-02-07` & Modernização FIKA Modded**:
    - Em `FikaHandler.cs`, proteção contra `InvalidOperationException` em `AllPlayersEverExisted` via `try / catch`.
    - Substituída Reflection sobre `_packetProcessor` pelo método oficial `manager.UnregisterPacket<T>()` nativo do FIKA modded v2.3.11.
  - **Versionamento SemVer (Gemini)**:
    - `tarkin.ladders.bep`: `1.0.6` $\rightarrow$ `1.0.7` (`Plugin.cs` e `.csproj`).
    - `tarkin.ladders.fika`: `1.1.2` $\rightarrow$ `1.1.3` (`Plugin.cs`, `.csproj` e dependência `com.tarkin.ladders` atualizada para `"1.0.7"`).
  - **Build & Compilação**:
    - Compilação da Solution em `Release` com **0 erros e 0 avisos** (isolamento estrito preservado).
- **Code Review Crítico (`/code-review`)**:
  - Relatório formal gerado em `docs/relatorio-code-review-02.md`: **0 Bloqueadores**, **0 Fortes**, **0 Médios**, **0 Menores** — **Aprovado para Produção**.

---

## 2026-09-03 (GMT-3) — Sessão 3: Fix do Bug de "Busy Hands" na Primeira Subida da Raid (CR-01-01 a CR-01-03)

- **Correção da Dessincronização da FSM de Mãos (`/code-mod`)**:
  - **`CR-01-01` (Prevenção no Frame Zero & Transição)**:
    - No `Init()`, solta gatilho residual (`SetTriggerPressed(false)`), executa `player.HideWeapon()` e dispara imediatamente `player.FastForwardCurrentOperations()`.
    - No `Transition()`, garante avanço prévio antes de aplicar o `ApproachState` no `MovementContext`, prevenindo que a sobreposição de animators engula o evento de animação de coldre da arma na primeira subida (cold cache).
    - Watchdogs com teto de 0.5s nos loops assíncronos `while (player.HandsController.IsInInteraction())` e `while (!player.HandsIsEmpty)` com fallback via `FastForwardCurrentOperations()`.
  - **`CR-01-02` (Restauração Resiliente)**:
    - Em `RestoreWeaponWhenReady`, caso o controlador ainda acuse interação ou não seja `EmptyHandsController` após a saída da escada, força `FastForwardCurrentOperations()` antes de chamar `TrySetLastEquippedWeapon()`.
    - Callback assíncrono com checagem de sobrevivência (`player.HealthController.IsAlive`) e retry defensivo.
  - **`CR-01-03` (Higiene de Logs)**:
    - Logs informativos e de depuração migrados para `Plugin.Logger.LogDebug` e `LogWarning`.
  - **Versionamento SemVer (Gemini)**:
    - `tarkin.ladders.bep`: `1.0.5` $\rightarrow$ `1.0.6` (`Plugin.cs` e `.csproj`).
    - `tarkin.ladders.fika`: `1.1.1` $\rightarrow$ `1.1.2` (`Plugin.cs`, `.csproj` e BepInDependency).
  - **Build & Compilação**:
    - Build da solução `ladders.sln` em `Release` com **0 erros e 0 avisos** (isolamento preservado em `bin/Release/`).

---

## 2026-08-31 (GMT-3) — Sessão 2: Unificação do FikaSync, Resolução dos Achados AUD-01 a AUD-09 e Fix Definitivo de Hands Busy

- **Unificação FikaSync no `ladders.fika` & Resolução da Auditoria (`/code-mod`)**:
  - Incorporado todo o código do mod externo `TRL-FikaSync-ClimbableLadders` diretamente no módulo `ladders.fika`, tornando o mod externo desnecessário.
  - **`AUD-01-01` (Memory Leak)**: `MainPlayerLadderControllerTracker` descartado síncrono em `OnProceduralBodyDestroy` e auto-removido da lista de trackers com lock thread-safe.
  - **`AUD-01-02` (Glitch 3ª Pessoa)**: `ObservedPlayerLadderController` atualizado com `player.HideWeapon()` no `Init()`, `player.RevealWeapon()` condicional no `OnDestroy()`, `[DefaultExecutionOrder(100)]` e interpolação rápida `SmoothTime = 0.08f`.
  - **`AUD-01-03` (Concorrência)**: `Patch_Physical` isolado exclusivamente para a instância `MainPlayer.Physical`.
  - **`AUD-01-04` (Heap Estático)**: `Ladder.TryIdentifySurfaceSound` com bloco `finally { overlapCols[0] = null; }`.
  - **`AUD-01-05` (Reflection)**: `_packetProcessor` em cache estático com lazy init em `FikaHandler`.
  - **`AUD-01-06` (PhysX Hot Path)**: Gating de altura `currentHeight < 1.0f` no raycast de descida em `PlayerLadderController.TryExit`.
  - **`AUD-01-07` & `AUD-01-08` (Limpeza)**: Removido `return;` inalcançável em `Patch_VaultingComponent` e método órfão `TestSinAnimation` em `ProceduralGrip`.
  - **`AUD-01-09` & Diagnóstico Forense de Hands Busy**: Investigado o erro `Default Inventory is currently being modified` do Fika Server. Blindada a coroutine `RestoreWeaponWhenReady` para aguardar a confirmação do desarmamento no servidor (`HandsIsEmpty == true`), término de vaulting, liberação de interações e 1 frame de ACK antes de chamar `TrySetLastEquippedWeapon()`.
  - **Identificação de Rede**: Pacotes `LadderStatePacket` e `BarAnglePacket` migrados para `FikaPlayer.NetId` e resolução em 3 camadas (`CoopHandler.Players` $\rightarrow$ `AllAlivePlayersList` $\rightarrow$ `AllPlayersEverExisted`).
- **Revisão Crítica de Código (`/code-review`)**:
  - Aprovado com **0 bloqueadores** e **0 riscos fortes**.
  - Todos os 3 projetos compilados em `Release` com **0 erros e 0 avisos**.

---

## 2026-08-30 (GMT-3) — Sessão 1: Documentação Modular e Auditoria Técnica

- **Documentação Modular (`/document-mod`)**:
  - Criada a suíte documental completa em `docs/`:
    - `01-visao-geral-e-arquitetura.md` — Visão geral da arquitetura de escadas escaláveis e pipeline do mod.
    - `02-controlador-de-jogador-e-maquina-de-estados.md` — `PlayerLadderController`, máquina de estados (Mounting, Climbing, Sliding, Dismounting) e custos de stamina/fadiga.
    - `03-cinematica-inversa-e-animacao-procedural.md` — Algoritmos de IK (FinalIK), posicionamento de pés e mãos em degraus (`ProceduralGrip`).
    - `04-infraestrutura-de-cenas-e-ferramentas-de-edicao.md` — Detecção de escadas por colisor/tag, ferramentas de debug e spawn de escadas em mapas.
    - `05-patches-harmony-e-integracao-com-eft.md` — Patches de física (`MovementContext`), transição de vaulting e bloqueio de disparo durante escalada.
    - `06-suporte-multiplayer-coop-fika.md` — Sincronização em rede Fika, replicação de estado de subida e animação de clones observados (`ObservedPlayerLadderController`).
    - `README.md` — Índice central dos artigos técnicos.
- **Auditoria de Código (`/audit-mod-code`)**:
  - Relatório registrado em `docs/relatorio-auditoria-codigo-01.md`.
  - Mapeadas melhorias de zero-allocation em hot paths e segurança de cancelamento de subida.
