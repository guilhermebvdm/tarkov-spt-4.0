---
title: "008 — Revisão Técnica: Camuflagem Zonal Anatômica e Visualizador Debug"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 008 — Revisão Técnica 01: Camuflagem Zonal Anatômica e Visualizador Debug

## 1. Avaliação Crítica

### 1.1. Trade-offs da Segmentação Zonal
- **Prós:** Elimina 100% o exploit do "Efeito Avestruz" (esconder a cabeça e deixar pernas expostas) e o falso-positivo em terreno raso.
- **Contras / Riscos:** Se os sensores de membros inferiores tiverem raio muito pequeno, o jogador poderia perder camuflagem ao mover levemente a perna durante animação de rastejo (*prone crawl*).
- **Mitigação:** O envelope de +30cm garante que o colisor acomode pequenas oscilações de animação esquelética sem causar cintilação (*flicker*) de camuflagem.

### 1.2. Renderização do Visualizador Debug
- **Risco:** Emissão de erros no log caso shaders não sejam encontrados na cena da raid.
- **Mitigação:** Usar cascata de fallback:
  ```csharp
  Shader shader = Shader.Find("Transparent/Diffuse") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
  ```
  E remover qualquer colisor das primitivas para não colidir com o próprio jogador.

## 2. Decisão
Spec aprovada para implementação.
