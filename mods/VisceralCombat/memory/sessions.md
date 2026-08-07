# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.7.1 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação limpa em C# 12 (0 erros, 26 avisos) alcançada para `nexus-bundleloader.dll`, `VolumetricBloodFX.dll` e `VisceralCombat.dll`. Todos os 5 achados do Code Review 01 resolvidos.
- **Pendências:** 🟢 Nenhuma pendência blocker ou alta aberta. Todos os achados do Code Review 01 marcados como resolvidos e aplicados em `modded/`.

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- **Análise:** Realizado code-review minucioso identificando gargalos de FPS, vazamentos de RAM, corrotinas descontroladas, thread-safety bugs (`async void`) e 15+ propriedades fantasma no F12.
- **Entregável:** Criado o roadmap detalhado de refatoração em `docs/refactor-roadmap.md`.
- **Regra:** Todas as correções serão realizadas em `modded/` sem alterar a pasta `original/`.

## Sessão 2026-08-07 — Execução do Refactor, Build Clean 3.7.1 e Aplicação do Code Review 01
- **Refatoração:** Concluída a resolução dos erros de descompilação de C# em `modded/`, refatorados `PlayerInitPatch` (eliminação de `async void`), `ShellCasingPatch` (janela deslizante de 50 cápsulas), `PhysicalItemsPatch` (repouso de física) e `KillPatch` (sistema de repouso de ragdolls com `PuppetMaster.Mode.Kinematic`).
- **Resolução do Code Review 01:**
  - **CR-01-01 (Bloqueador):** Protegidos os callbacks de `GClass855.WaitSeconds` contra encerramento de raid.
  - **CR-01-02 (Forte):** Cacheado `_supportRigidbodyMethod` estático em `RagdollClassPatch.cs`.
  - **CR-01-03 (Forte):** Adicionada destruição de `AnimatorOverrideController` anterior no `KillPatch.cs`.
  - **CR-01-04 (Médio):** Conectado `GoreObjectPool.Instance.Spawn` e `Recycle` em `KillPatch.cs` e `BleedPatch.cs`.
  - **CR-01-05 (Menor):** Substituído `val.loop` por `main.loop = false`.
- **Build & Sincronização:** Re-compilação executada com 0 erros. Binários atualizados no SPT (`SPT-4.0/BepInEx/plugins/VisceralCombat`).
