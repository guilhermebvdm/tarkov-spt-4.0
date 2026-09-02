# SAIN (Solarint's AI Modifications) — Documentação Técnica e Funcional

Bem-vindo à documentação técnica e arquitetural completa do **SAIN (Solarint's AI Modifications)** para Escape From Tarkov / SPT 4.0.

A base de conhecimento está organizada entre a versão em produção otimizada (**v4.5.0 / `modded`**), a versão base (**v4.4.3 / `original`**) e os relatórios técnicos de auditoria de código.

---

## 🚀 Documentação da Versão em Produção (v4.5.0 — Modded)

Documentação técnica atualizada cobrindo todas as melhorias balísticas, otimizações multithread sem alocações de GC, prevenção de fogo amigo e robustez defensiva:

| Módulo | Subsistema / Foco Temático | Status |
|---|---|---|
| [**01. Visão Geral e Arquitetura**](./modded/01-visao-geral-e-arquitetura.md) | Topologia Client-Server, Ciclo de Vida em Raid, Árvore de Componentes (`GameWorldComponent`, `BotManagerComponent`, `BotComponent`), BigBrain Layers e Suporte Cooperativo Fika sem Reflection. | 🟢 Vivo |
| [**02. Máquinas de Estado e Tomada de Decisão**](./modded/02-maquinas-de-estado-e-tomada-de-decisao.md) | Motor de Decisão (`BotDecisionManager`), Enums `ECombatDecision`, `ESquadDecision`, `ESelfActionType`, DogFight O(N) e Fix Melee Tagilla. | 🟢 Vivo |
| [**03. Sistema Sensorial: Visão, Audição e Memória**](./modded/03-sistema-sensorial-visao-audicao-e-memoria.md) | Percepção Visual (iluminação, neblina, vegetação, Dazzle otimizado), Audição Espacial (discriminação de recarga vs consumíveis, teardown seguro) e Memória de Rastreamento (`SAINEnemy`). | 🟢 Vivo |
| [**04. Sistema de Combate: Mira, Tiro e Recoil**](./modded/04-sistema-de-combate-mira-tiro-e-recoil.md) | Balística Preditiva, Suavização Inercial Contínua de Visada, Correção Matemática de Recoil (`Mathf.Abs`), Zero-Alloc Friendly Fire (`SphereCastNonAlloc`) e Gestão de Granadas. | 🟢 Vivo |
| [**05. Sistema de Cobertura: CoverFinder e Posicionamento**](./modded/05-sistema-de-cobertura-coverfinder-e-posicionamento.md) | Scanner de Cobertura Volumétrico por Raycast (`CoverFinderComponent`), Projeção Vetorial Corrigida, Triagem de Distância ao Quadrado e Lean sem Alocações LINQ. | 🟢 Vivo |
| [**06. Personalidades e Sistema de Presets**](./modded/06-personalidades-e-sistema-de-presets.md) | Arquétipos de Personalidade (`EPersonality`), Personalidades de Esquadrão (`ESquadPersonality`), Reset Dinâmico (`.Clear()`), Serialização JSON e Editor Gráfico In-Game Desacoplado (F6). | 🟢 Vivo |
| [**07. Táticas de Esquadrão, Comunicação e Interoperabilidade**](./modded/07-taticas-de-esquadrao-comunicacao-e-interop.md) | Hierarquia de Esquadrão, Teardown Seguro de Grupos (`BotSquads.Dispose()`), Templates de Frases Estáticos sem Reflexão e Matriz de Interoperabilidade (Fika, QuestingBots, LootingBots, Realism). | 🟢 Vivo |
| [**08. Sistemas Auxiliares: Portas, Médico, Extração e Patches**](./modded/08-sistemas-auxiliares-portas-medico-extracao-e-patches.md) | Manipulação e Chute de Portas (`DoorOpener`), Medicina de Campo, Extração com Limites Quadráticos Corrigidos, Unity Jobs e Catálogo Completo de Patches Harmony sem Reflexão. | 🟢 Vivo |

---

## 🏛️ Documentação da Versão Base (Original — v4.4.3)

Documentação de referência da arquitetura upstream do mod:

| Módulo | Subsistema / Foco Temático | Status |
|---|---|---|
| [**01. Visão Geral e Arquitetura**](./original/01-visao-geral-e-arquitetura.md) | Arquitetura base, camadas BigBrain e inicialização original. | 🟢 Vivo |
| [**02. Máquinas de Estado e Tomada de Decisão**](./original/02-maquinas-de-estado-e-tomada-de-decisao.md) | Máquinas de estado originais e catálogo de decisões. | 🟢 Vivo |
| [**03. Sistema Sensorial: Visão, Audição e Memória**](./original/03-sistema-sensorial-visao-audicao-e-memoria.md) | Pipeline sensorial base e modelo de visão/audição. | 🟢 Vivo |
| [**04. Sistema de Combate: Mira, Tiro e Recoil**](./original/04-sistema-de-combate-mira-tiro-e-recoil.md) | Modos de disparo, recuo e mira originais. | 🟢 Vivo |
| [**05. Sistema de Cobertura: CoverFinder e Posicionamento**](./original/05-sistema-de-cobertura-coverfinder-e-posicionamento.md) | Algoritmo original de busca de coberturas. | 🟢 Vivo |
| [**06. Personalidades e Sistema de Presets**](./original/06-personalidades-e-sistema-de-presets.md) | Arquétipos de personalidade e editor original. | 🟢 Vivo |
| [**07. Táticas de Esquadrão, Comunicação e Interoperabilidade**](./original/07-taticas-de-esquadrao-comunicacao-e-interop.md) | Gestão de esquadrões e interoperabilidade básica. | 🟢 Vivo |
| [**08. Sistemas Auxiliares: Portas, Médico, Extração e Patches**](./original/08-sistemas-auxiliares-portas-medico-extracao-e-patches.md) | Sistemas auxiliares e catálogo inicial de patches. | 🟢 Vivo |

---

## 🔍 Relatórios de Auditoria Técnica de Código (6ª Rodada — Base v4.5.0 Refatorada)

Sexta rodada de verificação profunda sobre a base em produção após aplicação das Ondas 1 a 8:

| Relatório | Escopo / Domínio Auditado | Novos Achados | Status |
|---|---|---|---|
| [**Relatório de Auditoria 25**](./modded/relatorio-auditoria-codigo-25.md) | **Parte 1:** Ciclo de Vida de Raid, Gestão de Memória / Leaks, Patches Globais e Interoperabilidade Client-Server. | 2 identificados (`AUD-25-01` a `25-02`) | 🟢 Concluído |
| [**Relatório de Auditoria 26**](./modded/relatorio-auditoria-codigo-26.md) | **Parte 2:** Sensores, Percepção Visual, Audição Espacial, Dazzle e Fogo Amigo. | 2 identificados (`AUD-26-01` a `26-02`) | 🟢 Concluído |
| [**Relatório de Auditoria 27**](./modded/relatorio-auditoria-codigo-27.md) | **Parte 3:** Máquina de Decisão (`BotDecisionManager`), Camadas BigBrain, Esquadrões e Comunicação. | 2 identificados (`AUD-27-01` a `27-02`) | 🟢 Concluído |
| [**Relatório de Auditoria 28**](./modded/relatorio-auditoria-codigo-28.md) | **Parte 4:** Cobertura (`CoverFinder`, `CoverAnalyzer`), Movimentação, Steering, Portas e Extração. | 2 identificados (`AUD-28-01` a `28-02`) | 🟢 Concluído |
| [**Relatório de Auditoria 29**](./modded/relatorio-auditoria-codigo-29.md) | **Parte 5:** Combate, Balística, Mira Preditiva, Recoil e Patches de Disparo. | 2 identificados (`AUD-29-01` a `29-02`) | 🟢 Concluído |
| [**Relatório de Auditoria 30**](./modded/relatorio-auditoria-codigo-30.md) | **Parte 6:** Presets, Serialização JSON, Modelos e Editor Gráfico In-Game (F6). | 2 identificados (`AUD-30-01` a `30-02`) | 🟢 Concluído |

---

### 📜 Histórico de Auditorias Anteriores (1ª a 5ª Rodadas)

- **5ª Rodada (Onda 8 — 12 achados resolvidos):**
  - [Relatório 19](./modded/relatorio-auditoria-codigo-19.md) · [Relatório 20](./modded/relatorio-auditoria-codigo-20.md) · [Relatório 21](./modded/relatorio-auditoria-codigo-21.md) · [Relatório 22](./modded/relatorio-auditoria-codigo-22.md) · [Relatório 23](./modded/relatorio-auditoria-codigo-23.md) · [Relatório 24](./modded/relatorio-auditoria-codigo-24.md)

- **4ª Rodada (Onda 7 — 14 achados resolvidos):**
  - [Relatório 13](./modded/relatorio-auditoria-codigo-13.md) · [Relatório 14](./modded/relatorio-auditoria-codigo-14.md) · [Relatório 15](./modded/relatorio-auditoria-codigo-15.md) · [Relatório 16](./modded/relatorio-auditoria-codigo-16.md) · [Relatório 17](./modded/relatorio-auditoria-codigo-17.md) · [Relatório 18](./modded/relatorio-auditoria-codigo-18.md)

- **3ª Rodada (Onda 6 — 15 achados resolvidos):**
  - [Parte 1 (v4.5.0)](./modded/relatorio-auditoria-codigo-07.md) · [Parte 2 (v4.5.0)](./modded/relatorio-auditoria-codigo-08.md) · [Parte 3 (v4.5.0)](./modded/relatorio-auditoria-codigo-09.md)
  - [Parte 4 (v4.5.0)](./modded/relatorio-auditoria-codigo-10.md) · [Parte 5 (v4.5.0)](./modded/relatorio-auditoria-codigo-11.md) · [Parte 6 (v4.5.0)](./modded/relatorio-auditoria-codigo-12.md)
- **2ª Rodada (Onda 5 — 18 achados resolvidos):**
  - [Parte 1 (v4.5.0)](./modded/relatorio-auditoria-codigo-01.md) · [Parte 2 (v4.5.0)](./modded/relatorio-auditoria-codigo-02.md) · [Parte 3 (v4.5.0)](./modded/relatorio-auditoria-codigo-03.md)
  - [Parte 4 (v4.5.0)](./modded/relatorio-auditoria-codigo-04.md) · [Parte 5 (v4.5.0)](./modded/relatorio-auditoria-codigo-05.md) · [Parte 6 (v4.5.0)](./modded/relatorio-auditoria-codigo-06.md)
- **1ª Rodada (Ondas 1 a 4 — 25 achados resolvidos):**
  - [Parte 1 (Original)](./original/relatorio-auditoria-codigo-01.md) · [Parte 2 (Original)](./original/relatorio-auditoria-codigo-02.md) · [Parte 3 (Original)](./original/relatorio-auditoria-codigo-03.md)
  - [Parte 4 (Original)](./original/relatorio-auditoria-codigo-04.md) · [Parte 5 (Original)](./original/relatorio-auditoria-codigo-05.md) · [Parte 6 (Original)](./original/relatorio-auditoria-codigo-06.md)

---

## 🛠️ Catálogo de Propriedades e Configurações

Para consultar a listagem exaustiva de todas as opções de configuração, multiplicadores numéricos, categorias globais e parâmetros de personalidades disponíveis no menu F6 e nos arquivos de preset JSON, consulte:
👉 [**PROPRIEDADES.md (Catálogo de Configurações do SAIN)**](../PROPRIEDADES.md)

---

## 📂 Mapeamento Estrutural do Código-Fonte

### Client Mod (C# / BepInEx) — [`mods/SAIN/modded/SAIN/`](../modded/SAIN/)
- **Entry Points:** [`SAINPlugin.cs`](../modded/SAIN/SAINPlugin.cs), [`BigBrainHandler.cs`](../modded/SAIN/Plugin/BigBrainHandler.cs), [`PatchManager.cs`](../modded/SAIN/Plugin/PatchManager.cs), [`ModDetection.cs`](../modded/SAIN/Plugin/ModDetection.cs), [`PresetHandler.cs`](../modded/SAIN/Preset/PresetHandler.cs)
- **Componentes Centrais:** [`GameWorldComponent.cs`](../modded/SAIN/Components/GameWorldComponent.cs), [`BotManagerComponent.cs`](../modded/SAIN/Components/BotManagerComponent.cs), [`BotComponent.cs`](../modded/SAIN/Components/BotComponent.cs), [`PlayerComponent.cs`](../modded/SAIN/Components/PlayerComponent.cs), [`CoverFinderComponent.cs`](../modded/SAIN/Components/CoverFinderComponent.cs)
- **Motor de Decisão:** [`SAINDecisionClass.cs`](../modded/SAIN/Classes/Bot/Decision/SAINDecisionClass.cs), [`BotDecisionManager.cs`](../modded/SAIN/Classes/Bot/Decision/BotDecisionManager.cs), [`EnemyDecisionClass.cs`](../modded/SAIN/Classes/Bot/Decision/EnemyDecisionClass.cs), [`SquadDecisionClass.cs`](../modded/SAIN/Classes/Bot/Decision/SquadDecisionClass.cs), [`SelfActionDecisionClass.cs`](../modded/SAIN/Classes/Bot/Decision/SelfActionDecisionClass.cs)
- **Sensores e Rastreamento:** [`SAINVisionClass.cs`](../modded/SAIN/Classes/Bot/Sense/SAINVisionClass.cs), [`EnemyVisionClass.cs`](../modded/SAIN/Classes/Bot/EnemyClasses/Vision/EnemyVisionClass.cs), [`SAINHearingSensorClass.cs`](../modded/SAIN/Classes/Bot/Sense/Hearing/SAINHearingSensorClass.cs), [`SAINMemoryClass.cs`](../modded/SAIN/Classes/Bot/Memory/SAINMemoryClass.cs), [`Enemy.cs`](../modded/SAIN/Classes/Bot/EnemyClasses/Enemy.cs)
- **Combate e Armas:** [`AimClass.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/AimClass.cs), [`AimDownSightsController.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/AimDownSightsController.cs), [`Recoil.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/Recoil.cs), [`Firerate.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/Firerate.cs), [`SAINBotSuppressClass.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/SAINBotSuppressClass.cs), [`BotGrenadeManager.cs`](../modded/SAIN/Classes/Bot/WeaponFunction/Grenades/BotGrenadeManager.cs)
- **Cobertura e Movimento:** [`CoverAnalyzer.cs`](../modded/SAIN/Classes/Coverfinder/CoverAnalyzer.cs), [`CoverPoint.cs`](../modded/SAIN/Classes/Coverfinder/CoverPoint.cs), [`SAINCoverClass.cs`](../modded/SAIN/Classes/Bot/Mover/SAINCoverClass.cs), [`SAINSteeringClass.cs`](../modded/SAIN/Classes/Bot/Steering/SAINSteeringClass.cs), [`SAINMoverClass.cs`](../modded/SAIN/Classes/Bot/Mover/SAINMoverClass.cs), [`DoorOpener.cs`](../modded/SAIN/Classes/Bot/Doors/DoorOpener.cs)
- **Esquadrões e Comunicação:** [`BotSquads.cs`](../modded/SAIN/Classes/BotManager/BotSquads.cs), [`Squad.cs`](../modded/SAIN/Classes/BotManager/Squad.cs), [`SAINBotTalkClass.cs`](../modded/SAIN/Classes/Bot/Talk/SAINBotTalkClass.cs), [`GroupTalk.cs`](../modded/SAIN/Classes/Bot/Talk/GroupTalk.cs)
- **Personalidades e Editor:** [`PersonalityManagerClass.cs`](../modded/SAIN/Preset/Personalities/BasePersonality/PersonalityManagerClass.cs), [`PersonalityDefaultsClass.cs`](../modded/SAIN/Preset/Personalities/BasePersonality/PersonalityDefaultsClass.cs), [`SAINEditor.cs`](../modded/SAIN/Preset/Editor/SAINEditor.cs), [`SAINPresetClass.cs`](../modded/SAIN/Preset/SAINPresetClass.cs)
- **Patches Harmony:** [`Patches/`](../modded/SAIN/Patches/) (`GameWorld/`, `Aim/`, `Shoot/`, `BotHearing/`, `VisionPatches.cs`, `MovementPatches.cs`, `TalkPatches.cs`)

### Server Mod (C# / SPT 4.0 Server) — [`mods/SAIN/modded/SAINServerMod/`](../modded/SAINServerMod/)
- **Metadados e Entry Point:** [`SAINServermodMetadata.cs`](../modded/SAINServerMod/SAINServermodMetadata.cs), [`PreSptLoad.cs`](../modded/SAINServerMod/OnLoad/PreSptLoad.cs)
- **Roteador e Serviços:** [`SAINStaticRouter.cs`](../modded/SAINServerMod/Routers/Static/SAINStaticRouter.cs), [`ConfigService.cs`](../modded/SAINServerMod/Services/ConfigService.cs)
- **Base de Dados:** [`NicknamePersonalities.json`](../modded/SAINServerMod/Data/NicknamePersonalities.json)
