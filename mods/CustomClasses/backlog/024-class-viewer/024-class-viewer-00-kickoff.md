# 024 — Viewer de classes (lista + detalhe read-only) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 023→024)
**Wave:** W2 (paralelo ao 023 — `Web/Pages/` × `Web/Shared/`) · **Deps:** 020, 021, 022

> Brief de kickoff — insumo para `/create-spec 024`. Não é a spec.

## Objetivo

Primeira entrega visível do editor: ler e exibir TUDO de cada classe corretamente (Etapa 3 do plano do usuário). Sem edição.

## Escopo

- **Página lista** (`/customclasses`): classes lidas dos **arquivos** via `ClassEditorService` (021) — NÃO dos registries (classe `disabled` não aparece em registry nenhum, e os registries nem enumeram). Por classe: nome/cor/ícone, status (registrada / disabled / inválida com diagnósticos do dry-run), custo ponderado de skills e `loadoutTotalRub`.
- **Página detalhe read-only:** todos os campos do schema — displayName/description en+pt, nameColor, baseEdition, skills, skillMultipliers (badge nas 4 skills do SE + aviso se SE ausente), hideout, outfit (nomes resolvidos), loadout equipado (árvore composta: preset/mods/ammo/contents) e stash, com nomes/preços resolvidos pelo `CatalogService`; painel de custo com breakdown (por skill e por item) e aviso de budget 28–32.

## Riscos / atenção

- Substitui a home placeholder do 020 como entrada principal.
- Diagnósticos de classe inválida vêm do dry-run do 021 (`Validate/Build`) — não duplicar validação.

## Refs

- `ClassEditorService` + `ClassRegistrar.Validate` (021), `CatalogService`/`CostService` (022)
- [modded/Server/config/classes/](../../modded/Server/config/classes/) — as 11 classes reais
- Doc do 018 (`docs/class-schema.md`)

## DoD (resumo)

- 11 classes visíveis com custos idênticos aos do 022.
- Classe inválida plantada à mão aparece com diagnóstico legível (e some ao corrigir).
