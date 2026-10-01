---
title: "009 — Catálogo Dinâmico de Malhas 3D de Vegetação e Indexação Espacial O(1)"
date: "2026-09-17"
status: "🟡 Em progresso"
authors: ["TRL Team", "Antigravity"]
---

# 009 — Catálogo Dinâmico de Malhas 3D de Vegetação e Indexação Espacial O(1)

## 1. Visão Geral e Problema

No Escape From Tarkov, a grande maioria da vegetação de ocultação tática do solo (capim alto `grass02_LOD0`, samambaias `fern01_LOD0`, mato denso `plant_wolf01_LOD0`, juncos `reed` e ervas `burdock`) é composta por **modelos de malha 3D avulsos** instanciados na cena, e **NÃO** por texturas/camadas pintadas no terreno (`Terrain Detail Layers`), as quais são inclusive desativadas pela Battlestate Games no cliente compilado.

Esses modelos 3D de vegetação apresentam uma assimetria severa:
1. **Ausência de Colisores Físicos:** Para economizar computação de física (PhysX), o jogo não coloca `Collider` nessas plantas.
2. **Visão Transpassável da IA:** O raycast de visão dos bots passa 100% liso pelas malhas sem colisor, enxergando o jogador mesmo quando este está visualmente 100% coberto atrás de uma touceira densa ou samambaia.
3. **Falha da Camuflagem Zonal Anterior:** O sistema do item 008 dependia de `TerrainData` ou raycast de chão genérico, deixando essas plantas avulsas invisíveis para os cálculos de cobertura zonal.

## 2. Objetivos e Requisitos Funcionais

1. **Varredura Dinâmica e Indexação por Mapa (Raid Start):**
   - Ao iniciar a raid (`OnRaidStarted`), o mod varre os renderers da cena ativa buscando objetos de vegetação de solo sem colisor físico.
   - Filtra palavras-chave de vegetação (`grass`, `fern`, `plant_`, `reed`, `weed`, `burdock`, `flower`) e layers (`Foliage`, `Grass`, `Default`).
   - Deduplica instâncias com múltiplos LODs (`LODGroup`), considerando apenas o volume principal do prefab.
   - Descarta superfícies de terreno gigantescos (> 25 metros de extensão, como sobreposições de bunker).

2. **Estrutura de Indexação Espacial O(1) (Spatial Hash Grid):**
   - As malhas catalogadas são particionadas em uma grade espacial 2D (células de 16m × 16m).
   - Elimina a necessidade de iterar sobre centenas de plantas a cada tick: a consulta de qualquer ponto no espaço 3D busca unicamente os elementos contidos na célula correspondente à coordenada `(X, Z)`.
   - Custo computacional de consulta por zona anatômica inferior a 0,005 ms (0% de stutter e impacto imperceptível de FPS).

3. **Classificação Física por Altura e Volume:**
   - **Capim Alto / Mato Denso (Altura >= 0,60m):** Cobre o corpo inteiro deitado ou agachado; cobre pernas/cintura em pé.
   - **Mato Médio / Touceiras (0,30m <= Altura < 0,60m):** Cobre o corpo apenas quando deitado de bruços (`IsInPronePose`).
   - **Grama Rasteira (< 0,30m):** Ignorada para camuflagem.

4. **Integração com a Camuflagem Zonal Anatômica:**
   - O [NaturalConcealmentManager](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/NaturalConcealmentManager.cs) consulta o índice espacial para cada uma das 5 zonas do soldado.
   - Se os ossos da zona estiverem dentro do volume horizontal e vertical de uma malha catalogada, a zona ativa seu escudo correspondente no layer `Foliage`.
   - Permite que o jogador exponha partes específicas (ex.: apenas a cabeça fora da touceira enquanto o corpo permanece invisível aos bots).

5. **Feedback Visual F9:**
   - O visualizador vermelho translúcido 50% reflete com exatidão matemática quais membros estão imersos dentro das malhas catalogadas.

## 3. Critérios de Aceite

* [ ] Ao deitar dentro de uma touceira de `grass02_LOD0` ou samambaia `fern01_LOD0`, o escudo de camuflagem ativa nas partes imersas.
* [ ] Se o jogador colocar apenas a cabeça para fora do capim alto, peito e pernas permanecem camuflados e apenas a cabeça fica desprotegida.
* [ ] Terreno liso ou gramado rasteiro (< 0,30m) não ativa camuflagem, mesmo ao ar livre.
* [ ] Tempo de varredura no carregamento do mapa inferior a 25ms.
* [ ] Custo de avaliação por tick (a cada 0,2s) inferior a 0,02ms no `OnUpdate`.
* [ ] Compatível com qualquer mapa (Woods, Customs, Shoreline, Lighthouse, etc.) sem listas manuais fixas de coordenadas.
