---
title: "009 — As-Built: Catálogo Dinâmico de Malhas e Indexação Espacial O(1)"
date: "2026-09-17"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 009 — As-Built: Catálogo Dinâmico de Malhas e Indexação Espacial O(1)

## 1. O Que Foi Entregue

1. **Catálogo Espacial de Malhas ([VegetationSpatialIndex.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/VegetationSpatialIndex.cs)):**
   - Varredura na inicialização da raid (`IndexSceneVegetation()`) que indexa todos os prefabs 3D de vegetação sem colisor (`grass02_LOD0`, `fern01_LOD0`, `plant_wolf01_LOD0`, etc.).
   - Particionamento em grade espacial 2D (células de 16m × 16m) com suporte multi-célula contra falhas de borda.
   - Consulta instantânea `IsPointCoveredByVegetation(worldPos, isProne, poseLevel, out plantTopY)`.

2. **Integração Zonal e GPUInstancer ([NaturalConcealmentManager.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/NaturalConcealmentManager.cs)):**
   - Extração dinâmica de matrizes de densidade do `GPUInstancerDetailManager` nativo de EFT para leitura do capim alto procedural do terreno.
   - Fallback de inicialização assíncrona via `cachedDensityMapForInstance` dos protótipos de vegetação.
   - Amostragem com kernel 3x3 para evitar falhas de fronteira em tufos de vegetação.
   - Checagem anatômica por membro (cabeça, peito, cintura, perna esquerda, perna direita).
   - Altura de corte configurada dinamicamente: em pé (> 0.90m), agachado (> 0.60m) e deitado (> 0.25m).
   - Remoção do fallback genérico de solo, garantindo que terreno liso de terra/gramado rasteiro não conceda camuflagem.
   - Limpeza total de memória no encerramento da raid.

3. **Depurador Visual Holográfico ([ConcealmentVisualizer.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/ConcealmentVisualizer.cs)):**
   - Substituição das primitivas geométricas gigantescas por cópias holográficas fiéis dos `SkinnedMeshRenderer` do próprio soldado e equipamento (vermelho 50% translúcido acendendo apenas nas partes cobertas).

4. **Inspetor de Vegetação por Mira — Tecla '[' ([AimVegetationInspector.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/AimVegetationInspector.cs)):**
   - Disparo de raycast instantâneo da visão do jogador para inspecionar a folhagem/capim 2D e 3D exatamente no ponto mirado (não apenas o terreno).
   - Extração do catálogo de GPUInstancer: nome da textura PNG, nome do prefab, dimensões físicas reais (`detailScale` com min/max altura e largura), densidade e classificação de tamanho.
   - Painel OnGUI translúcido em tempo real na tela por 12 segundos com catálogo completo das gramas do mapa.
   - Notificação nativa EFT e exportação automática de texturas PNG e relatório detalhado em `BepInEx/plugins/TRL-CoreSight/Dump/AimInspector/`.

5. **Blindagem contra Concorrência de Threads do GPUInstancer & Isolamento de Falhas:**
   - Correção de exceção `IndexOutOfRangeException` em `GPUInstancerDetailManager.GetDetailLayer(p)` durante a inicialização assíncrona do mapa.
   - Envolvimento em blocos `try / catch` com fallback garantido para o array estático `cachedDensityMapForInstance`.
   - Isolamento individual de todos os submódulos em `PerformanceManager.Initialize`.

6. **Calibração Cirúrgica com Dados Reais da Mira (v0.4.8):**
   - **Correção de Terreno Nulo:** Fallback para `Terrain.activeTerrain` quando `mgr.terrain` vier nulo no EFT.
   - **Chave Única de Setor (`UniqueKey`):** Indexação composta `$"{mgr.GetInstanceID()}_{p}_{proto.name}"` para cobrir todos os setores sem conflito.
   - **Filtro Estrito Anti-Pedras:** Descarte automático de rochas e entulho (`!vertexlit_rock`).

7. **Classificador Interativo de Vegetação in-game (v0.4.9):**
   - **Teclas `[` e `]` na Mira:**
     - **`[` (LeftBracket):** Registra a folhagem mirada como **CAMUFLAGEM ATIVA** (Whitelist) e exibe notificação verde.
     - **`]` (RightBracket):** Registra a folhagem mirada como **NÃO-CAMUFLAGEM** (Blacklist / Ignorada) e exibe notificação vermelha.
   - **Persistência JSON em Tempo Real ([VegetationClassifier.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/VegetationClassifier.cs)):**
     - Criação automática e persistência em `BepInEx/plugins/TRL-CoreSight/vegetation_classification.json`.
     - Permite calibrar qualquer folhagem de qualquer mapa dinamicamente, mantendo as configurações salvas entre raids.
   - **Reatividade Instantânea no [NaturalConcealmentManager.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/NaturalConcealmentManager.cs):**
     - O gerenciador avalia `VegetationClassifier.IsBlocked()` dinamicamente a cada 0.2s. Ao apertar `]`, os escudos daquela folhagem se apagam no mesmo segundo; ao apertar `[`, ativam instantaneamente.
   - **Correção de Raycast da Mira:**
     - Ignora colisores do corpo do jogador (`_player.gameObject.transform` e `HitCollider`), impedindo que o raio colida na mandíbula/queixo (`Jaw`) ao olhar para o chão.
   - Incremento para a versão **`v0.4.9`** em `Plugin.cs` e `TRL-CoreSight.csproj`.
   - Binário compilado exclusivamente em [`mods/TRL-CoreSight/builds/TRL-CoreSight.dll`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/builds/TRL-CoreSight.dll).

8. **Calibração de Densidade e Postura Específica (v0.4.10):**
   - **Isolamento de `Field_grass_D`:** Bloqueado permanentemente da concessão de camuflagem (grama rasteira curta mesmo com densidade 6).
   - **Regra para `Grass_new_1_D`:** Ativação exclusiva para **Prone (`isProne`)** quando a densidade no solo for **≥ 6** (no teste in-game deu densidade 8).
   - **Regra para `Grass6_D`:** Ativação para **Prone** (5 zonas) e **PoseLevel(0f)** (`poseLevel <= 0.15f`, cobrindo pernas, pelve e peito) quando a densidade for **≥ 4** (no teste in-game deu densidade 4).
   - **Filtro Estrito Inicial:** Por solicitação do usuário, o sistema de GPUInstancer atende exclusivamente a essas duas folhagens validadas, mantendo qualquer outra vegetação desligada até futuro mapeamento.
   - Incremento para a versão **`v0.4.10`** em `Plugin.cs` e `TRL-CoreSight.csproj`.
   - Binário compilado exclusivamente em [`mods/TRL-CoreSight/builds/TRL-CoreSight.dll`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/builds/TRL-CoreSight.dll).

9. **Resolução de Tufos Misturados, Descarte de Scan e PoseLevel Seguro (v0.4.11):**
   - **Independência Real de Camadas:** Confirmado que o descarte de `Field_grass_D` opera via `continue;` no laço de camadas, nunca anulando `Grass6_D` ou `Grass_new_1_D`.
   - **Remoção de Descarte Prematuro no Scan:** O `ScanGPUInstancerGrass()` não exclui protótipos na inicialização se estiverem na lista de bloqueados, garantindo que todas as folhagens existam em memória para avaliação em tempo real.
   - **Salvaguarda na Mira (`]`):** Em tufos misturados contendo múltiplas espécies de folhagem, o comando `]` não bloqueia folhagens calibradas ou permitidas (`Grass6_D`, `Grass_new_1_D`), bloqueando apenas a rasteira indesejada (`Field_grass_D`).
   - **Margem Segura de Agachamento:** Ajuste de `poseLevel <= 0.15f` para `poseLevel <= 0.25f` para suportar tanto o agachamento de scroll completo quanto o agachamento padrão da tecla `C`.
   - **Fallback Anatômico para Todas as Zonas:** Pernas, pelve e peito recorrem à posição de base dos pés se a inclinação do corpo agachado deslocar levemente o osso para fora do pico de densidade, garantindo a cobertura de 4 zonas corporais e acendimento imediato do HUD holográfico.
   - Incremento para a versão **`v0.4.11`** em `Plugin.cs` e `TRL-CoreSight.csproj`.
   - Binário compilado exclusivamente em [`mods/TRL-CoreSight/builds/TRL-CoreSight.dll`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/builds/TRL-CoreSight.dll).

10. **Calibração Específica de Folhagens e Densidade > 8 (v0.4.12):**
    - **Folhagens 100% Ignoradas:** `Field_grass_D`, `Grass_new_1_D`, `Grass_new_2_D` e `Grass_new_3_D` totalmente excluídas da concessão de camuflagem mesmo em pico de densidade.
    - **Folhagens Ativas com Densidade ≥ 8:** `Grass2_D` (2.00m), `Grass5_512_D` (1.30m) e `Grass6_D` (1.62m) ativam a camuflagem holográfica exclusivamente quando o tufo atingir densidade **≥ 8**:
      - Em **Prone (`isProne`)**: Cobertura de 100% das 5 zonas corporais.
      - Em **Agachamento Baixo (`poseLevel <= 0.25f`)**: Cobertura de 4 zonas corporais (pernas, pelve e peito).
      - Em **Pé ou com densidade < 8**: Desligado.
    - **Sincronização de Defaults e Persistência JSON:** Atualização de `vegetation_classification.json` e das listas padrão em `VegetationClassifier.cs`.
    - **Proteção Bidirecional na Mira:** `[` ignora plantas excluídas e `]` protege folhagens ativas em tufos mistos.
    - Incremento para a versão **`v0.4.12`** em `Plugin.cs` e `TRL-CoreSight.csproj`.
    - Binário compilado exclusivamente em [`mods/TRL-CoreSight/builds/TRL-CoreSight.dll`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/builds/TRL-CoreSight.dll).

11. **Controles de Ativação no Menu F12 (v0.4.13):**
    - **Toggle da Camuflagem Natural:** Opção `EnableNaturalConcealment` na seção `12. Camuflagem & Visão IA` (com prioridade no topo do menu). Ao desativar, desativa imediatamente os 5 colidentes corporais, os patches de visão de IA e os escudos visuais vermelhos do `F9`.
    - **Toggle da Ferramenta de Vegetação:** Nova opção `EnableVegetationInspector` na seção `16. Ferramenta de Vegetação`. Ao desativar, neutraliza as teclas `[` e `]`, fecha imediatamente o painel OnGUI e cessa qualquer processamento de raycast da ferramenta.
    - Incremento para a versão **`v0.4.13`** em `Plugin.cs` e `TRL-CoreSight.csproj`.
    - Binário compilado exclusivamente em [`mods/TRL-CoreSight/builds/TRL-CoreSight.dll`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/builds/TRL-CoreSight.dll).

## 2. Instruções de Teste em Jogo
1. O binário já foi atualizado em `E:\Tarkov Red Line - SERVER TEST\BepInEx\plugins\TRL-CoreSight\TRL-CoreSight.dll` (cópia mestre mantida em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`).
2. Pressione **`F12`** in-game:
   - Seção **`12. Camuflagem & Visão IA`**: ative ou desative `EnableNaturalConcealment` para ligar/desligar todo o sistema de camuflagem na hora.
   - Seção **`16. Ferramenta de Vegetação`**: ative ou desative `EnableVegetationInspector` para ligar/desligar o scanner de mira das teclas `[` e `]`.
