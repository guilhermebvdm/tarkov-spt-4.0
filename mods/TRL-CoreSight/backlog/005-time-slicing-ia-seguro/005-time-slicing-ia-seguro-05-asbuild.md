---
title: "Item 005 — Camuflagem Natural e Percepção de IA — As-Built"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 005 — Camuflagem Natural e Percepção de IA — As-Built

## 1. Resumo Executivo

O Item 005 implementou no mod **TRL-CoreSight** o módulo de **Camuflagem Natural e Oclusão de Visão de IA** (*Player Proximity Concealment Shield*).

O sistema resolve definitivamente duas das maiores deficiências de física e balanceamento de inteligência artificial do Escape from Tarkov:
1. Bots enxergando o jogador através da copa e folhas de árvores densas a 150–200 metros de distância (devido à falta de colisor nas copas por design da BSG).
2. Bots cravando tiros na cabeça de jogadores deitados em capinzais e grama alta (pelo fato de a grama do GPUInstancer ser apenas cosmética sem presença física no mundo).

A tecnologia cria volumes invisíveis e dinâmicos no layer canônico `Foliage` (Layer 15) estritamente no raio imediato do jogador, bloqueando o raycast de visão dos bots sem interferir na passagem de balas e sem travar a movimentação do jogador. O sistema integra-se com perfeição ao SAIN, permitindo que a IA inimiga mantenha a memória da última posição vista, dispare tiros de supressão no mato e arremesse granadas táticas.

---

## 2. Componentes e Funcionalidades Implementadas

### 2.1. Escudo de Camuflagem Dinâmico (`Core/NaturalConcealmentManager.cs`)
- **Camada Física Foliage (Layer 15):**
  - O volume de oclusão opera no layer nativo `Foliage`, componente oficial do `LayerMaskClass.HighPolyWithTerrainMaskAI` utilizado por EFT e SAIN.
  - Balas atravessam livremente (`HitColliderMask` ignora `Foliage`).
  - Movimentação do jogador 100% desobstruída (`PlayerCollisionsMask` ignora `Foliage`).
- **Camuflagem de Grama Alta por Postura:**
  - **Deitado (`IsInPronePose == true`):** Projeta uma cápsula de 2.0 metros de raio e 0.85m de altura cobrindo completamente o corpo do jogador.
  - **Agachado (`PoseLevel < 0.6f`):** Projeta cápsula de 0.5m de altura protegendo pernas e abdômen; a cabeça permanece vulnerável a tiros por ângulos elevados.
  - **Em Pé:** Camuflagem rasteira desativada.
- **Oclusão por Copas de Árvores (`TreeCanopyOcclusion`):**
  - Detecta se o jogador está sob a projeção da copa de uma árvore e projeta esfera de oclusão superior contra bots a longa distância.
- **Validação de Terreno e Solo:**
  - Raycast descendente periódico a cada 0.5s garante que o jogador só ganha camuflagem de grama se estiver sobre terreno natural (`Terrain`, `dirt`, `grass`, `mud`). Deitar em pisos de madeira, galpões de concreto ou asfalto mantém o escudo desativado.
- **Mecânica de Tropeço (< 4.0 metros):**
  - Se qualquer bot inimigo se aproximar a menos de 4 metros no mato, a camuflagem é temporariamente cancelada, simulando que o bot tropeçou ou avistou fisicamente o jogador escondido.
- **Destruição Imediata na Morte:**
  - Desativa instantaneamente os volumes ao morrer ou ao término da partida (`Cleanup()`).

### 2.2. Menu BepInEx F12 (`Configuration/ModConfig.cs` e `PROPRIEDADES.md`)
Criada a seção **`12. Camuflagem Natural & Visão de IA`**:
- `EnableNaturalConcealment` (`bool`, padrão `true`): Ativa ou desativa a camuflagem no mato e sob árvores.
- `GrassConcealmentRadius` (`float`, padrão `2.0m`, faixa `1.0m` a `4.0m`): Raio do escudo ao redor do jogador deitado.
- `BreakProximityDistance` (`float`, padrão `4.0m`, faixa `2.0m` a `8.0m`): Distância de tropeço para detecção por aproximação.
- `TreeCanopyOcclusion` (`bool`, padrão `true`): Oclusão superior de copas de árvores.
- `EnableBotTimeSlicing` (`bool`, padrão `true`): Alívio de checagens visuais de bots distantes fora de combate.

### 2.3. Telemetria On-Screen (`Core/PerformanceManager.cs`)
- Adicionada a linha de telemetria no overlay de debug (F11):
  `Camuflagem: ATIVA / COPA / NÃO`.

---

## 3. Arquivos Criados e Modificados

| Arquivo | Ação | Descrição |
|---|---|---|
| `modded/Core/NaturalConcealmentManager.cs` | CRIAR | Componente de gerenciamento do escudo físico no layer Foliage, verificação de solo e proximidade de bots. |
| `modded/Configuration/ModConfig.cs` | MODIFICAR | Inclusão das propriedades da seção 12 e evento de configuração. |
| `modded/Core/PerformanceManager.cs` | MODIFICAR | Ciclo de vida, atualização por frame e telemetria do NaturalConcealmentManager. |
| `modded/Plugin.cs` | MODIFICAR | Bump de versão SemVer para `0.3.6`. |
| `modded/TRL-CoreSight.csproj` | MODIFICAR | Inclusão do novo arquivo compilado e versão `0.3.6`. |
| `mod.json` | MODIFICAR | Bump de versão para `0.3.6`. |
| `PROPRIEDADES.md` | MODIFICAR | Documentação em pt-BR da seção 12 de F12. |

---

## 4. Verificação e Compilação

- **Compilação:** `dotnet build mods/TRL-CoreSight/modded/TRL-CoreSight.csproj -c Release`
- **Resultado:** 0 Erros, 0 Avisos. Tempo decorrido: 1.29s.
- **Binário isolado gerado:** `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (72.192 bytes).
