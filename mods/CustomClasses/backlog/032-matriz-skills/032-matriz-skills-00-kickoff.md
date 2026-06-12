# 032 — Matriz de skills (classes × skills, heatmap) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** comparação de UX com `tools/tarkov-itemdb/viewer/profiles-skills.html`
**Épico:** UX do editor (030–035) · **Wave:** UX-W2 (paralelo ao 033) · **Deps:** 031 (ordem canônica), 030 (link no sidebar)

> Brief de kickoff — insumo para `/create-spec 032`. Não é a spec.

## Problema (UX)

A página `profiles-skills.html` do viewer antigo era a ferramenta de **comparação entre classes**: tabela skills (linhas, ordem canônica) × classes (colunas, headers verticais), células como **heatmap** por tier (1-3 fraco / 4-6 médio / 7-10 forte), vazio = nível 0. Leitura horizontal = quem tem a skill; vertical = perfil da classe. O editor atual **não tem nenhuma visão comparativa**.

## Escopo

- **Página `/customclasses/skills`** (`Web/Pages/SkillsMatrix.razor`): matriz com linhas = skills na ordem canônica do 031 (separadores de categoria) e colunas = todas as classes (header vertical/rotacionado com nome na `nameColor` + ícone). **Classes desabilitadas entram como coluna esmaecida** com toggle "show disabled" (default on, esmaecido) — comparação não pode ficar cega pro que está fora do ar (review #8). Célula: nível com fundo heatmap por tier (port das cores de `profiles-skills.css:25-127`); vazia quando 0. Hover na linha destaca.
- **Extras que o viewer antigo não tinha** (barato com dados vivos): linha de rodapé com **custo ponderado total** por classe (verde no budget 28–32); toggle "mostrar multiplicadores XP" (célula ganha chip ±% colorido); célula clicável → vai pro detalhe/edit da classe (1 clique pra agir no que viu).
- Link "Skills matrix" no sidebar (030).
- Fonte: `ListClassSummaries`/`ListClassFiles` (servidos pelo cache do 037 — a matriz NÃO pode disparar dry-runs) + `CostService` — sem nova API.

## Refs

- `tools/tarkov-itemdb/viewer/profiles-skills.js:20-93` (renderMatrix), `profiles-skills.css:25-127` (heatmap/tiers/headers verticais)
- `SkillMaster.cs` (031), `CostService.cs`
- Território: `Web/Pages/SkillsMatrix.razor` (novo) + 1 linha no NavMenu (combinar com o que o 030 deixou)

## DoD (resumo)

- Comparar skills de TODAS as classes = **0 cliques** depois de abrir a página (tudo numa tela).
- Heatmap legível; rodapé com custos; clique na célula navega pra classe.
