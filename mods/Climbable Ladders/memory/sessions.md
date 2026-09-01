# Memory — Climbable Ladders

Memória cronológica de sessões de desenvolvimento, auditoria e manutenção técnica do mod **Climbable Ladders** (SPT 4.0 / EFT 0.16.9 / Fika).

---

## Estado Atual (Snapshot)

- **Versão:** 
  - `tarkin.ladders.shared.dll` $\rightarrow$ **1.0.4**
  - `tarkin.ladders.bep.dll` $\rightarrow$ **1.0.5**
  - `tarkin.ladders.fika.dll` $\rightarrow$ **1.1.1** (unificado nativamente com o `TRL-FikaSync-ClimbableLadders`)
- **Arquitetura:** Interceptação da física do jogador via `Patch_Physical` com isolamento de instância (`MainPlayer.Physical`), controle de estado via `PlayerLadderController` com restauração assíncrona defensiva de armas (`RestoreWeaponWhenReady` aguardando `HandsIsEmpty` e término de vaulting), rig procedural de mãos com FinalIK (`ProceduralGrip`), sincronização em rede Fika em 3 camadas por `NetId` (`ObservedPlayerLadderController` / `FikaHandler`) e integração nativa com o sistema de Vaulting da BSG (`Patch_VaultingComponent`).
- **Documentação:** Modular completa em `docs/` (6 artigos técnicos + README + Relatório de Auditoria 01 com 9 achados 100% sanados).

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
