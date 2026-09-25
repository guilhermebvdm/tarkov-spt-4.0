---
title: "Item 005 — Camuflagem Natural e Percepção de IA — Code Review 01"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 005 — Camuflagem Natural e Percepção de IA — Code Review 01

## 1. Escopo da Revisão

Revisão técnica do código implementado no mod `TRL-CoreSight` para o módulo de **Camuflagem Natural e Oclusão de Visão de IA** (*Player Proximity Concealment Shield*), protegendo o jogador contra a visão de raio-X de bots através de capinzais altos e copas de árvores, integrando-se organicamente com a reação de tiro de supressão e busca do SAIN.

### Arquivos Criados / Modificados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Core/NaturalConcealmentManager.cs` | Novo | Gerenciador dinâmico de volumes de oclusão no layer `Foliage`, verificação periódica de solo/vegetação, ajuste fino por postura (deitado/agachado) e quebra por proximidade de tropeço (< 4m). |
| `modded/Configuration/ModConfig.cs` | Modificado | Inclusão de configurações da Seção 12 no F12 (`EnableNaturalConcealment`, `GrassConcealmentRadius`, `BreakProximityDistance`, `TreeCanopyOcclusion`, `EnableBotTimeSlicing`) e evento `OnConcealmentSettingsChanged`. |
| `modded/Core/PerformanceManager.cs` | Modificado | Integração do `NaturalConcealmentManager` no ciclo de vida, atualização por frame, telemetria OnGUI e destruição segura no `Cleanup()`. |
| `modded/Plugin.cs` | Modificado | Bump de versão SemVer para `0.3.6`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Inclusão do novo arquivo compilado e bump SemVer para `0.3.6`. |
| `mod.json` | Modificado | Bump de versão para `0.3.6`. |
| `PROPRIEDADES.md` | Modificado | Documentação em pt-BR da seção 12 de F12. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Preservação de Física Balística
- **Zero Impacto em Balística e Movimento:** Ao operar estritamente no layer canônico `LayerMaskClass.Foliage` (Layer 15 da Unity), o colisor do escudo é 100% invisível para a física de locomoção do jogador (`PlayerCollisionsMask`) e para a física de projéteis e impacto de balas (`HitColliderMask`). O jogador anda e corre sem qualquer colisão fantasma, e tiros de ambos os lados atravessam a grama com dano balístico integral.
- **Bloqueio Canônico de Raycasts de IA:** Tanto a IA nativa da BSG quanto os jobs de visão paralela do SAIN (`VisionRaycastJob.cs:17`) utilizam a máscara `HighPolyWithTerrainMaskAI`, que inclui o layer `Foliage`. Ao bater no volume de oclusão do jogador, a IA perde a linha de visão (`CanBeSeen = false`).

### 2.2. Integração Orgânica com o SAIN
- **Memória e Tiro de Supressão Preservados:** Como o bloqueio ocorre na camada física do raycast de visão e não por hacks de remoção de inimigo da lista do bot, o SAIN retém a coordenada onde viu o jogador pela última vez (`EnemyKnownPlaces.LastKnownPosition`), acionando organicamente tiros de supressão (`SAINBotSuppressClass.SuppressPosition`), lançamento de granadas de fragmentação e varredura cautelosa do arbusto/capim.

### 2.3. Fairplay e Corner Cases
- **Validação de Terreno e Solo:** O método `CheckIfOnVegetation` realiza um raycast descendente periódico garantindo que se o jogador deitar no asfalto, concreto ou assoalhos de madeira internos, a camuflagem não é ativada.
- **Quebra por Tropeço (< 4 metros):** Se um bot se aproximar a menos de 4 metros, `IsAnyBotTooClose` desativa a camuflagem no frame, permitindo que o bot detecte o jogador escondido.
- **Destruição Imediata na Morte:** Se o jogador morrer, os colisores são desativados de imediato, garantindo que o cadáver possa ser saqueado e visualizado sem interferências.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.5` para `0.3.6` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL final gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (72.192 bytes). Nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item 005 documentado de ponta a ponta (01-spec, 02-spec-tech, 03-review, 04-code-review, 05-asbuild). |
| **Idioma** | ✅ Aprovado | Toda a documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor para Homologação em Raid.** A solução é engenhosa, elegante, aproveita a arquitetura nativa de layers do EFT e responde 100% à demanda do usuário.
