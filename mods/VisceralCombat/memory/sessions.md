# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.7.1 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação limpa em C# 12 (0 erros, 29 avisos) alcançada para `nexus-bundleloader.dll`, `VolumetricBloodFX.dll` e `VisceralCombat.dll`. Ragdoll Sleep System, otimizações de pooling/raycasting e correções de threading aplicadas em `modded/`.
- **Pendências:** 🔴 1 Bloqueador (CR-01-01: Callback assíncrono em `WaitSeconds`), 🟠 2 Fortes (CR-01-02: Reflection sem cache no `RagdollClassPatch`, CR-01-03: `AnimatorOverrideController` sem `Destroy`), 🟡 1 Médio (CR-01-04: Conexão completa do `GoreObjectPool`), 🟢 1 Menor (CR-01-05: `ParticleSystem.loop` obsoleto).

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- **Análise:** Realizado code-review minucioso identificando gargalos de FPS, vazamentos de RAM, corrotinas descontroladas, thread-safety bugs (`async void`) e 15+ propriedades fantasma no F12.
- **Entregável:** Criado o roadmap detalhado de refatoração em `docs/refactor-roadmap.md`.
- **Regra:** Todas as correções serão realizadas em `modded/` sem alterar a pasta `original/`.

## Sessão 2026-08-07 — Execução do Refactor, Build Clean 3.7.1 e Code Review
- **Refatoração:** Concluída a resolução dos erros de descompilação de C# em `modded/`, refatorados `PlayerInitPatch` (eliminação de `async void`), `ShellCasingPatch` (janela deslizante de 50 cápsulas), `PhysicalItemsPatch` (repouso de física) e `KillPatch` (sistema de repouso de ragdolls com `PuppetMaster.Mode.Kinematic`).
- **Build & Sincronização:** Atualizado o pipeline `compile-mod.sh` para resolver referências cruzadas entre os projetos da solução. Build concluído com sucesso e arquivos `.dll` instalados automaticamente na pasta `SPT-4.0/BepInEx/plugins/VisceralCombat`.
- **Code Review:** Gerado o relatório de Code Review `docs/001-refactor-visceralcombat-04-code-review-01.md` com 5 achados mapeados (1 🔴 Bloqueador, 2 🟠 Fortes, 1 🟡 Médio, 1 🟢 Menor).
