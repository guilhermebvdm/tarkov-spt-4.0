# 022 — Catálogo de itens + custo (port RZ) — Spec

**Mod:** CustomClasses
**Status:** Implementado (build integrado pendente — ver as-built)
**Criado:** 2026-06-10
**Origem:** [022-catalog-e-custo-00-kickoff.md](./022-catalog-e-custo-00-kickoff.md)

## Visão geral

Camada de dados do editor web de classes (futuro item 023): catálogo read-only de itens/preços/presets/roupas do **DB vivo** do server e serviço de **custo de classe** com a fórmula do RZCustomProfiles. Só C#, sem UI, sem rotas (as rotas vêm com o editor).

## Comportamento desejado

- **Catálogo** (`CatalogService`): busca de itens por nome (en/pt), shortname, nome interno do template ou tpl exato, com filtro opcional por categoria do handbook (incluindo descendentes); nome localizado por item; preço com fonte; árvore de categorias do handbook; presets de uma arma (flag de qual é o default e qual o "premium" que o `InventoryBuilder` escolheria); roupas upper/lower por facção (mesmas regras de aceitação do `OutfitBuilder`); lista de edition keys registradas. Itens de outros mods entram automaticamente (leitura on-demand do DB pós-load).
- **Custo de skills** (`CostService.ComputeSkillCost`): port fiel do RZ — Σ nível×peso, pesos da tabela `SkillWeights` (31 skills explícitas + 4 derivadas do Skills-Extended + fallback por categoria), budget alvo **[28, 32]**, regras informativas (≥1 ponto por categoria Ph/M/C/P, máx 6 skills com pontos, teto sugerido 10) como **warnings não-bloqueantes**. Multiplicadores de XP ficam **fora** do custo (decisão do usuário — só exibidos).
- **Custo de loadout** (`CostService.ComputeLoadoutCost`): total ₽ caminhando equipped + stash, expandindo presets (paridade com a auto-completação do `InventoryBuilder`), árvores manuais de mods, contents de contêiner e munição (carregador cheio + câmara). Por linha: tpl, nome, qty, preço unitário, fonte do preço, subtotal. Item sem preço = 0 com flag `missingPrice` — **nunca 0 silencioso**. Moeda = valor facial.
- **Preço ₽:** flea efetivo do server primeiro, fallback handbook (decisão registrada na spec-tech).

## Critérios de aceite

- [x] Paridade do custo ponderado de skills com a fórmula RZ nas 11 classes atuais (Peladão = 0) — `scripts/check-skill-costs.mjs` verde.
- [x] Pesos do script de paridade idênticos aos de `SkillWeights.cs`.
- [x] Tabela de pesos estendida (4 skills do SE) com racional documentado por skill.
- [x] Fallback por categoria documentado; skill fora de qualquer mapa → 1.00 + flag (nunca 0 silencioso).
- [x] Catálogo resolve nome/preço/preset/customization/editions sem editar arquivos de outros itens em andamento (020/021).
- [ ] Sanidade ₽ in-game (sem itens a 0 inesperados) — depende do build integrado (pendência no as-built).

## Fora de escopo

- Rotas HTTP e UI do editor (item 023).
- Persistência/escrita de classes (item 021 — ClassEditorService).
