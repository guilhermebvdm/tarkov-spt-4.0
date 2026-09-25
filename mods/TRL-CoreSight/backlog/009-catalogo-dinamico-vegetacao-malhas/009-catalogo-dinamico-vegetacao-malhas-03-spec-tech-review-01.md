---
title: "009 — Revisão Técnica Crítica da Especificação Técnica (Round 01)"
date: "2026-09-17"
status: "🟡 Em progresso"
authors: ["TRL Team", "Antigravity"]
---

# 009 — Revisão Técnica Crítica da Especificação Técnica (Round 01)

## 1. Análise Crítica e Riscos Identificados

### 🔴 Risco 1: Fronteiras de Célula (Edge Border Misses)
* **Problema:** Se uma touceira grande de capim estiver centrada perto da borda de uma célula (ex.: X = 15.8m em uma célula de 16m), suas folhas e bounds ultrapassam para a célula adjacente (X = 16.2m). Se a consulta pesquisar estritamente a célula do ponto do soldado, o ponto pode estar na célula adjacente e não encontrar a planta registrada na célula vizinha!
* **Solução Obrigatória:** O registro da planta no grid deve ser inserido em **todas as células 2D que o seu `bounds` sobrepor** (minCellX..maxCellX, minCellZ..maxCellZ). Como o raio da maioria das plantas é de 0,5m a 2m, uma planta ocupará no máximo 1 a 4 células adjacentes. Isso garante 100% de detecção sem falhas de borda.

### 🟡 Risco 2: Amostragem Inicial no `OnRaidStarted` vs Carregamento Tardio de Chunks
* **Problema:** Em alguns mapas do Tarkov (especialmente Streets of Tarkov e Lighthouse), objetos distantes usam streaming ou são ativados tardiamente pela BSG.
* **Mitigação:** Na inicialização, varremos a cena ativa. Se o jogador transitar para áreas novas ou o mapa tiver streaming, podemos permitir um rescan amortizado ou verificar renderers já carregados na hierarquia de cena raiz. Para a maioria dos mapas abertos (Woods, Customs, Shoreline), os prefabs de vegetação já são instanciados no carregamento da cena.

### 🟡 Risco 3: Falsos Positivos com Objetos Decorativos Internos
* **Problema:** Um vaso de flor ou samambaia decorativa dentro de uma sala de dormitório (`Flower_5_LOD0`) poderia teoricamente conceder camuflagem.
* **Mitigação:** Como a planta decorativa é pequena (altura < 0,35m), ela só concederia camuflagem se deitado em cima da mesa. Além disso, se a sala for fechada (interior), o sistema de interior já monitora se o jogador está em ambiente confinado. Ainda assim, manter o filtro de altura mínima e descarte de folhagens estritamente ornamentais de interior.

### 🟢 Trade-Off Aceito: Spatial Grid vs Octree / BVH
* Uma Spatial Hash Grid 2D possui alocação estática simples, indexação O(1) e não gera lixo no GC durante a partida, sendo superior a árvores BVH dinâmicas que exigiriam percorrimento recursivo na Unity.

## 2. Veredito da Revisão
* Aprovado com a inclusão mandatória do **registro multi-célula para plantas que cruzam as bordas da grade de 16m** (Risco 1).
