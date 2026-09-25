---
title: "008 — Camuflagem Zonal Anatômica e Visualizador Debug Translúcido"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 008 — Camuflagem Zonal Anatômica e Visualizador Debug Translúcido

## 1. Visão Geral e Problema

O sistema anterior de camuflagem do mod utilizava um raio único de solo para detectar terreno e erguia uma cápsula monolítica de 2 metros ao redor do centro do jogador. Isso gerava o "Efeito Avestruz" e falsos-positivos severos:
1. **Falso-positivo em grama rasteira:** Ao deitar em gramados aparados ou chão de terra, o mod ativava invisibilidade total para a IA, mesmo com o jogador 100% exposto no horizonte.
2. **Falta de granularidade por membro:** Se o jogador colocasse apenas a cabeça dentro de um arbusto e deixasse as pernas e a mochila expostas na terra batida, o bot não via as pernas.

## 2. Objetivos e Requisitos Funcionais

1. **Segmentação em 5 Zonas Anatômicas Independentes:**
   - **Zona 1 (Cabeça / Olhos):** Ancorada em `player.PlayerBones.Head`.
   - **Zona 2 (Peito / Mochila):** Ancorada em `player.PlayerBones.Ribcage`.
   - **Zona 3 (Pelve / Centro):** Ancorada em `player.PlayerBones.Pelvis`.
   - **Zona 4 (Perna / Pé Esquerdo):** Ancorada em `LeftThigh` + `LeftFoot`.
   - **Zona 5 (Perna / Pé Direito):** Ancorada em `RightThigh` + `RightFoot`.
2. **Contorno Afastado de 30 cm:**
   - Cada colisor de zona possui um envelope expandido em +30 cm (+0,30 m) além da anatomia e mochila do soldado.
3. **Sensores Volumétricos Locais:**
   - A cada 0,2s, cada zona testa presença real de vegetação (layer `Foliage`, triggers de `TreeInteractive` / `IsInTree` e capim alto denso).
   - Apenas zonas cobertas ativam seu respectivo escudo no layer `Foliage`. Zonas no ar livre permanecem desprovidas de escudo, permitindo que a IA enxergue aquele membro.
4. **Visualizador Debug In-Game (Vermelho 50% Transparente):**
   - Toggle no menu F12 (`ShowConcealmentVisualizer`) e atalho configurável no teclado (padrão **`F9`**).
   - Renderiza em tempo real primitivas translúcidas com cor vermelha 50% transparente (`RGBA(1.0, 0.0, 0.0, 0.5)`) exatamente nas zonas com camuflagem ativa.
5. **Modulação de Visão e Mira da IA (Inspirado no Ombarella):**
   - Reduz a taxa de percepção em `EnemyInfo.method_9` proporcionalmente ao `CoverageRatio`.
   - Aplica dispersão na mira em `BotAimingClass.method_13` quando o jogador estiver parcialmente coberto.
6. **Remoção Completa do Dumper Antigo:**
   - Exclusão do código de dump de texturas PNG (`VegetationAssetDumper.cs`).

## 3. Critérios de Aceite

* [ ] Se o jogador deitar em gramado liso, nenhuma zona ativa camuflagem (0% cobertura).
* [ ] Se o jogador colocar apenas a cabeça no capim, apenas a Zona 1 acende o escudo e fica vermelha no visualizador.
* [ ] Bots detectam e atiram nas partes expostas (ex.: pernas fora do mato).
* [ ] Pressionar F9 liga/desliga o visualizador vermelho translúcido in-game com 0 stutter.
* [ ] Compilação limpa com 0 avisos e 0 erros.
