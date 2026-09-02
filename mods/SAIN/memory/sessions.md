# Memória de Sessões — SAIN

## Estado atual

> **Delta 2026-09-02 (Sessão 1):** SAIN em **v4.5.1** compilado com 0 Erros e 0 Warnings em [`mods/SAIN/builds/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/builds/). Concluída a maratona de 6 rodadas de auditoria técnica estática profunda (30 relatórios técnicos em `docs/original/` e `docs/modded/` cobrindo 100% dos subsistemas do mod) e implementação de 9 Ondas de saneamento de código com formalização de Code Reviews (0 bloqueadores). Principais entregas: (1) eliminação definitiva de vazamentos de memória entre raids com descarte determinístico de delegates e coleções em `PlayerSpawnTracker`, `Squad`, `BotManagerComponent`, `ExtractFinderComponent` e `DoorHandler`; (2) otimização de GC Alloc por frame em loops de profile de bot e throttling em `findLocation`; (3) blindagem completa contra NREs em combate, sentidos, movimentação e editor F6; (4) 100% de compatibilidade binária (ABI/API) preservada com mods dependentes (*ORBIT*, *TRL-ImmersiveCombatMedicine*, *TRL-DynamicSpawn*, *TRL-Fixes*); (5) definição do [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/docs/ROADMAP.md) para o futuro Addon desacoplado de Imersão Militar Tática (MilSim CQB & Esquadrão).

- **Auditoria integral concluída:** 36 relatórios temáticos e de código (`docs/original/` e `docs/modded/01` a `30`) validados contra fontes canônicas (`references/eft-decompiled` EFT 0.16.9, `references/fika-plugin` FIKA 2.3.4, `references/spt-source` SPT 4.0.13).
- **Ciclo de Vida e Teardown:** Desinscrição defensiva de eventos de motor (`OnPersonAdd`, `OnDispose`, `OnPresetUpdated`, `OnMemberRemove`) e nulificação de delegates no encerramento da raid.
- **Isolamento de Build:** Binários compilados localmente em `mods/SAIN/builds/` (`SAIN.dll` .NET Standard 2.1 e `SAINServerMod.dll` .NET 9.0).
- **Roadmap MilSim:** Arquitetura desacoplada via BigBrain e API pública do SAIN registrada para desenvolvimento futuro.

## Pendências

- [P-1.1] (aberta 2026-09-02) **VALIDAR IN-GAME a build consolidada v4.5.1 do SAIN** — Cenários a testar: **(1)** Raids consecutivas no SPT/FIKA monitorando memória/GC para certificar ausência de OOM entre raids; **(2)** Combates em ambientes fechados (CQB) e abertos para certificar ausência de NREs nos logs BepInEx; **(3)** Abertura e salvamento de presets no editor in-game F6; **(4)** Interoperabilidade com o mod de VOIP e resposta a gritos do jogador. 🟡 Validação in-game.
- [P-1.2] (aberta 2026-09-02) **Planejamento e especificação do Addon MilSim (TRL-MilSimAI)** — Estruturar a camada desacoplada de BigBrain para *Breach & Clear* (granadas/flashbangs em portas) e *Bounding Overwatch* em esquadrão conforme [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/docs/ROADMAP.md). 🟢 Ideia.

---

## 2026-09-02 00:04 (GMT-3) — Sessão 1: Auditoria Técnica Integral (30 Relatórios), Saneamento das 9 Ondas, Roadmap MilSim e Build v4.5.1

**Tema central:** Auditoria profunda do código-fonte original e modificado do SAIN, implementação e validação das correções de memory leaks, GC e NREs (Ondas 1 a 9), criação do Roadmap MilSim desacoplado e compilação do mod para v4.5.1.

**Decisões-chave:**
- [Auditoria Integral de 30 Relatórios]: Varredura estática de 100% das 400+ classes do SAIN em 6 rodadas contínuas, gerando relatórios de 01 a 30 em `mods/SAIN/docs/modded/` e atualizando o catálogo mestre em `mods/SAIN/docs/README.md`.
- [Estancamento de Memory Leaks entre Raids]: Descarte unificado em `PlayerSpawnTracker.cs`, `Squad.cs`, `BotManagerComponent.cs`, `ExtractFinderComponent.cs` e `DoorHandler.cs`, eliminando referências circulares e delegates presos em singletons.
- [Saneamento de Performance e GC]: Throttling em `findLocation` (`LocationClass.cs`), eliminação de alocações de string por frame em profiling e prevenção de divisões por zero na interface gráfica F6 (`BotSelectionClass.cs`).
- [Compatibilidade Estrita de API (0 Quebras)]: Validação automatizada por script AST garantindo que nenhum tipo, método ou propriedade pública sofreu alteração de assinatura que quebrasse outros mods.
- [Roadmap MilSim como Addon Desacoplado]: Decisão estratégica de manter a base do SAIN enxuta e estável, delegando as mecânicas avançadas de CQB com granadas (*Breach & Clear* com máquina de estados de espera) e *Bounding Overwatch* para um mod Addon separado acoplado via BigBrain.
- [Compilação e Bump SemVer 4.5.1]: Correção de duplicidades sintáticas de pré-build em `SquadDecisionClass.cs`, `Squad.cs` e `PlayerSpawnTracker.cs`, gerando build limpa em `mods/SAIN/builds/SAIN.dll`.

**Lições / hipóteses descartadas:**
- *Modificar o core do SAIN para táticas MilSim:* Descartado em favor de um Addon desacoplado via BigBrain. Modificar o core criaria atrito com os detectores de unstuck e reações de perigo do SAIN e dificultaria manutenções futuras.
- *Alteração de vozes/taunts:* Descartada para proteger a mecânica do mod customizado de VOIP do usuário.

**Atividade cronológica:**
1. Realização das 6 rodadas de auditoria técnica estática cobrindo Ciclo de Vida, Visão, Decisão, Movimentação, Supressão e Editor F6.
2. Implementação progressiva das Ondas de refatoração 1 a 9 com comentários de rastreabilidade `// ref: AUD-NN-MM`.
3. Execução dos Code Reviews formais com 0 bloqueadores reportados.
4. Resolução das pendências de pré-compilação e execução do `dotnet build` com saída em `mods/SAIN/builds/`.
5. Criação e validação do documento `mods/SAIN/docs/ROADMAP.md`.
6. Criação da memória de sessões em `mods/SAIN/memory/sessions.md`.

**Pendências abertas nesta sessão:**
- [P-1.1] (aberta 2026-09-02) **VALIDAR IN-GAME a build consolidada v4.5.1 do SAIN**. Categoria: 🟡 validação in-game.
- [P-1.2] (aberta 2026-09-02) **Planejamento e especificação do Addon MilSim (TRL-MilSimAI)**. Categoria: 🟢 ideia.
