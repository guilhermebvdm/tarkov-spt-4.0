# 036 — Modo comparação A×B no dashboard · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** review do épico UX (achado #9 — complemento direto do pedido original "comparar facilmente a diferença de skills entre classes")
**Épico:** UX do editor (030–036) · **Wave:** UX-W3 (paralelo ao 034 — territórios disjuntos) · **Deps:** 031 (SkillCanonicalList), 033 (header do dashboard)

> Brief de kickoff — insumo para `/create-spec 036`. Não é a spec.

## Problema (UX)

A matriz (032) compara **todas** as classes em largura (visão panorâmica); falta a comparação **profunda de duas**: ao decidir balanceamento ("o Caçador está mais forte que o Batedor?"), o usuário quer deltas skill a skill e de custo — hoje teria que abrir duas abas do browser lado a lado.

## Escopo

- **Picker "Compare with…"** no header do `ClassDetail` (dropdown das demais classes, com cor/ícone). Selecionada a classe B:
  - **`SkillCanonicalList` ganha modo comparação:** coluna fantasma com o nível de B + delta por skill (▲ verde quando A > B, ▼ vermelho quando A < B, vazio quando iguais/ambas 0) — a ordem canônica fixa (031) é o que torna a leitura instantânea.
  - **Deltas de resumo no header:** custo ponderado A vs B (com budget), loadout ₽ A vs B, nº de skills.
  - **Multiplicadores XP:** chips ±% das duas classes lado a lado na mesma linha.
  - Hideout/outfit: comparação simples em 2 colunas compactas (sem visual rico).
- Comparação é **read-only e efêmera** (não persiste no JSON); limpar = voltar ao dashboard normal. Persistir a última escolha em `localStorage` fica pro 035.
- Deep-link: `?compare=<classe>` na URL do detail (compartilhável; a matriz do 032 pode linkar pra cá no futuro).
- **Território:** header do `ClassDetail` + `SkillCanonicalList.razor` (modo novo). O 034 (paralelo) mexe em `GearPanel`/`StashPanel`/coluna direita — sem interseção; combinar apenas o ponto de montagem no `ClassDetail` (header vs coluna direita).

## Refs

- `SkillCanonicalList.razor` (031), `CostService` (022), layout do dashboard (033)

## DoD (resumo)

- Comparar 2 classes skill a skill (deltas coloridos) + custos = **2 cliques** a partir do detail (abrir picker + escolher).
- Trocar a classe A pelo sidebar mantém a comparação com B ativa (B fixa, A navega).
