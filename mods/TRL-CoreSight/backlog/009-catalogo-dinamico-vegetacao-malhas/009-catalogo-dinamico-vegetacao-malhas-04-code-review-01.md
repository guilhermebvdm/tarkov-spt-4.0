---
title: "009 — Code Review: Catálogo Dinâmico de Malhas e Indexação Espacial (Round 01)"
date: "2026-09-17"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 009 — Code Review: Catálogo Dinâmico de Malhas e Indexação Espacial (Round 01)

## 1. Escopo Auditado
* [`VegetationSpatialIndex.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/VegetationSpatialIndex.cs) (Novo arquivo: grade espacial O(1) multi-célula).
* [`NaturalConcealmentManager.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Core/NaturalConcealmentManager.cs) (Integração na inicialização, checagem zonal e limpeza).
* [`Plugin.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Plugin.cs) e [`TRL-CoreSight.csproj`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/TRL-CoreSight.csproj) (Bump para v0.4.4).

## 2. Achados de Auditoria Crítica

### 🟢 CR-009-01: Resolução de Falhas de Borda (Multi-Cell Insertion)
* **Status:** Conforme e verificado.
* **Detalhes:** O laço de inserção calcula `minCX..maxCX` e `minCZ..maxCZ` com base nos bounds físicos da planta + 15cm. Qualquer planta que toque na fronteira de uma célula de 16m é registrada nas células adjacentes, prevenindo qualquer "buraco negro" de camuflagem na transição de coordenadas.

### 🟢 CR-009-02: Zero Alocação de Heap no Loop de Update
* **Status:** Conforme e verificado.
* **Detalhes:** A consulta `IsPointCoveredByVegetation` recebe coordenadas por valor (`Vector3`), usa `struct VegetationMeshEntry` e itera sobre a `List<VegetationMeshEntry>` da célula sem gerar nenhuma alocação de objeto (`0 B/tick` no GC).

### 🟡 CR-009-03: Remoção do Fallback Amplo de Terreno
* **Status:** Mitigado e aprovado.
* **Detalhes:** Foi removido o fallback que atribuía 0,75m de capim a qualquer chão natural. Agora, o chão liso de terra/gramado rasteiro não ativa camuflagem, transferindo a concessão de cobertura estritamente para plantas reais e detail layers legítimas.

### 🟢 CR-009-05: Cobertura Integral de Terreno via GPUInstancer (Capim Alto Real)
* **Status:** Conforme e verificado.
* **Detalhes:** Identificado que o EFT zera `terrain.detailObjectDistance = 0f` na Unity nativa e delega todo o capim procedural ao `GPUInstancer.GPUInstancerDetailManager`. Implementada leitura das matrizes de densidade de cada protótipo com fallback imediato para `cachedDensityMapForInstance`, amostragem de kernel 3x3 e filtro de altura mínima de 0,70m.

### 🟢 CR-009-06: Correção de Atribuição CS0165 em Curto-Circuito
* **Status:** Corrigido e validado.
* **Detalhes:** Variável de saída `zoneGrassHeight` refatorada com controle de fluxo explícito, eliminando o erro de compilação CS0165 e garantindo que o valor correto de altura seja repassado para o cálculo vertical da zona.

## 3. Veredito Final do Code Review
* **Aprovado para validação em raid (v0.4.5).** Compilação 100% limpa (0 avisos, 0 erros). Release compilado e isolado em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll`.
