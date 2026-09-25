---
title: "008 — Code Review 01: Camuflagem Zonal Anatômica e Visualizador Debug"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 008 — Code Review 01: Camuflagem Zonal Anatômica e Visualizador Debug

## 1. Arquivos Modificados e Criados

| Arquivo | Estado | Responsabilidade |
|---|---|---|
| `Core/VegetationAssetDumper.cs` | Excluído | Removida ferramenta antiga de dump de texturas PNG. |
| `Core/ConcealmentVisualizer.cs` | Novo | Renderiza primitivas 3D translúcidas na cor vermelha (50% transparente) nas zonas protegidas. |
| `Core/NaturalConcealmentManager.cs` | Refatorado | Orquestra as 5 zonas de oclusão anatômica com contorno de +30 cm e amostragem volumétrica. |
| `Patches/BotVisionPatches.cs` | Novo | `BotVisionSpeedPatch` modula tempo de spotting e `BotAimOffsetPatch` dispersa a mira dos bots. |
| `Configuration/ModConfig.cs` | Modificado | Opções no menu F12 (`ShowConcealmentVisualizer`, `VisualizerShortcutKey`, `ConcealmentOffsetMeters`). |
| `Core/PerformanceManager.cs` | Modificado | Alternância in-game via tecla F9 com notificação nativa na tela. |
| `TRL-CoreSight.csproj` / `Plugin.cs` / `mod.json` | Modificados | Incremento SemVer para v0.4.0 e registro dos novos patches. |

## 2. Análise de Conformidade e Riscos

1. **Risco de Falso-Positivo:** Eliminado completamente. A checagem cega de `Terrain` foi extirpada; agora cada zona avalia colisão física de folhagem/arbustos ou proximidade vertical de tufos de vegetação.
2. **Performance:** Zero alocações no hot path; `OverlapSphereNonAlloc` reutiliza buffer compartilhado de 16 elementos; visualizador só atualiza matrizes de transform quando ativado.
3. **Isolamento de Build:** Respeitado estritamente (`builds/TRL-CoreSight.dll`).

## 3. Conclusão
Aprovado para validação em raid.
