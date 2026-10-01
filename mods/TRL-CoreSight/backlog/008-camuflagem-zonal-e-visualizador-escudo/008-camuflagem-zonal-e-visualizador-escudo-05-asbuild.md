---
title: "008 — As-Built: Camuflagem Zonal Anatômica e Visualizador Debug"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 008 — As-Built: Camuflagem Zonal Anatômica e Visualizador Debug

## 1. O que foi Entregue

1. **Remoção do Dumper de Texturas Antigo:**
   - Excluído `VegetationAssetDumper.cs` e limpas todas as chamadas de menu F12 e atalhos.
2. **Sistema de Camuflagem em 5 Zonas Anatômicas:**
   - Cada soldado agora possui 5 zonas de oclusão atreladas aos ossos reais de `player.PlayerBones`:
     1. Cabeça / Visão (`Head`)
     2. Peito / Mochila (`Ribcage`)
     3. Pelve / Centro (`Pelvis`)
     4. Perna / Pé Esquerdo (`LeftThigh1`)
     5. Perna / Pé Direito (`RightThigh1`)
3. **Envelope de Contorno de +30 cm:**
   - As cápsulas de colisão no layer `Foliage` possuem raio de proteção estendido em +0,30 m ao redor da anatomia e da bolsa do jogador.
4. **Visualizador Debug In-Game em Vermelho Translúcido (50% Transparente):**
   - Implementado no `ConcealmentVisualizer.cs`.
   - Pode ser alternado pressionando a tecla **`F9`** ou no menu F12 (`ShowConcealmentVisualizer`).
   - Mostra no jogo exatamente as partes do corpo que estão cobertas e a área de 30 cm que envolve o corpo.
5. **Mecânicas Inspiradas no Ombarella:**
   - `BotVisionSpeedPatch` em `EnemyInfo.method_9`: reduz a velocidade de spotting dos bots proporcionalmente à quantidade de zonas encobertas.
   - `BotAimOffsetPatch` em `BotAimingClass.method_13`: introduz erro e espalhamento de tiro quando o alvo está parcialmente camuflado.
6. **Compilação e Isolamento:**
   - Versão incrementada para `v0.4.0`.
   - DLL compilada e isolada em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.

## 2. Validação Manual Recomendada

1. Copiar `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` para `E:\Tarkov Red Line - SERVER TEST\BepInEx\plugins\TRL-CoreSight\`.
2. Em raid, pressionar `F9`:
   - Em pé na trilha: nenhum escudo vermelho ativo.
   - Deitado com a cabeça em tufo de capim alto: apenas a cápsula da cabeça acende em vermelho translúcido.
   - Deitado inteiramente dentro de capim alto ou moita: as 5 cápsulas acendem em vermelho 50% transparente.
