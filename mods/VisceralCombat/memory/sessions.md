# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.1 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Desmembramento pós-morte (braços, pernas e cabeça) totalmente funcional e testado em jogo sem regredição. Todos os achados dos Code Reviews 01 e 02 resolvidos e aplicados em `modded/`.
- **Origem dos Bugs de Gigantismo/Inflamento:** Registrado que todos os episódios de "gigantismo" e "corpo inflando" foram causados por código experimental no `modded/` durante a refatoração de agonia/músculos e foram 100% corrigidos por nós. **Não vieram do mod original.**
- **Pendências:** 🟢 Nenhuma pendência blocker ou alta aberta.

---

## Sessão 2026-08-10 — Desmembramento Pós-Morte & Correções de Agonia e Estabilidade
- **Esclarecimento sobre Gigantismo & Inflamento:**
  - Identificado e registrado que os comportamentos de "corpo inflando" e "gigantismo" surgiram devido a inconsistências nas primeiras alterações do `modded/` (resets de escala de músculos pelo `PuppetMaster.State.Dead`, transições de camada de agonia e desacoplamento do `Animator`).
  - **Não foram defeitos herdados do mod original.** Todos foram corrigidos no `modded/`.
- **Desmembramento Pós-Morte (Cadáveres no Chão):**
  - Implementada estratégia dupla em `LimbKillPatch.cs`: `BodyPartColliderType` para bots vivos e matching por nome de osso físico (`Base Human[L/R/Head]`) para cadáveres.
  - Habilitado desmembramento de **cabeça** pós-morte, confirmado via SPY que `Base HumanHead` é filho de `Base HumanNeck` e seguro contra colapso de malha.
  - Corrigido o bug no cálculo de chance por calibre onde o parâmetro `out chance` zerava a variável (`0.0f`) para munições fora do dicionário, restaurando o default de 50%.
- **Resolução do Code Review 02:**
  - Removidos 100% dos logs e comentários de debug `SPY` / `SPY-HEAD`.
  - Protegido o callback assíncrono do `PuppetMaster` em `InterruptAgony` com `(UnityEngine.Object)pm != null`.
  - Limpos os dicionários `deadPlayers` e `dismemberedPlayers` no encerramento/início de raid.

---

## Sessão 2026-08-07 — Execução do Refactor, Build Clean 3.7.1 e Aplicação do Code Review 01
- **Refatoração:** Concluída a resolução dos erros de descompilação de C# em `modded/`, refatorados `PlayerInitPatch` (eliminação de `async void`), `ShellCasingPatch` (janela deslizante de 50 cápsulas), `PhysicalItemsPatch` (repouso de física) e `KillPatch` (sistema de repouso de ragdolls com `PuppetMaster.Mode.Kinematic`).
- **Resolução do Code Review 01:**
  - **CR-01-01 (Bloqueador):** Protegidos os callbacks de `GClass855.WaitSeconds` contra encerramento de raid.
  - **CR-01-02 (Forte):** Cacheado `_supportRigidbodyMethod` estático em `RagdollClassPatch.cs`.
  - **CR-01-03 (Forte):** Adicionada destruição de `AnimatorOverrideController` anterior no `KillPatch.cs`.
  - **CR-01-04 (Médio):** Conectado `GoreObjectPool.Instance.Spawn` e `Recycle` em `KillPatch.cs` e `BleedPatch.cs`.
  - **CR-01-05 (Menor):** Substituído `val.loop` por `main.loop = false`.
- **Build & Sincronização:** Re-compilação executada com 0 erros. Binários atualizados no SPT (`SPT-4.0/BepInEx/plugins/VisceralCombat`).

---

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- **Análise:** Realizado code-review minucioso identificando gargalos de FPS, vazamentos de RAM, corrotinas descontroladas, thread-safety bugs (`async void`) e 15+ propriedades fantasma no F12.
- **Entregável:** Criado o roadmap detalhado de refatoração em `docs/refactor-roadmap.md`.
- **Regra:** Todas as correções serão realizadas em `modded/` sem alterar a pasta `original/`.
