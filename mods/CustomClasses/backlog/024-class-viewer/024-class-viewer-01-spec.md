# 024 — Viewer de classes (lista + detalhe read-only) — Spec

**Mod:** CustomClasses
**Status:** Implementado (build integrado + smoke no browser pendentes — ver as-built)
**Criado:** 2026-06-10
**Origem:** [024-class-viewer-00-kickoff.md](./024-class-viewer-00-kickoff.md)

## Visão geral

Primeira entrega visível do editor web: lista de classes + página de detalhe **read-only** exibindo TUDO de cada classe. Fonte de dados são os **arquivos** (`ClassEditorService.ListClassFiles`, item 021) — nunca os registries, pois classe `disabled`/inválida só existe em disco. Custos e nomes vêm do item 022 (`CostService`/`CatalogService`).

## Comportamento desejado

- **Lista** (`/customclasses/classes`): uma linha por arquivo de `config/classes/` com ícone (estático do `wwwroot`), nome na `nameColor` da classe, displayName (en), status (Registered/Disabled/Invalid/Not registered — Invalid com tooltip dos diagnostics), nº de skills, custo ponderado de skills (chip verde dentro do budget 28–32, laranja fora, neutro p/ classe sem skills) e total do loadout em ₽. Linha clicável → detalhe. Arquivo com parse error ainda aparece (linha com chip de erro), sem quebrar a página.
- **Detalhe** (`/customclasses/classes/{file}` — nome SEM extensão na rota): diagnostics do dry-run no topo (MudAlert por severidade) e seções: Geral (name, displayName/description en+pt, baseEdition, enabled, swatch da nameColor, preview do iconFile), Skills (nível, peso, origem do peso, custo por skill + total + warnings de budget), Multiplicadores (verde >1 / vermelho <1, badge "Skills-Extended" nas 4 skills do SE + aviso se SE ausente), Hideout, Outfit (nomes resolvidos por facção), Equipado (árvore recursiva por slot: preset resolvido c/ nome e nº de partes, chip premium, ammo, ícones loadedMag/chambered, mods e contents indentados), Stash (linhas precificadas, badge "⚠ no price" p/ `missingPrice`) e Resumo de custo (dois totais + breakdown completo + warnings).
- **Navegação:** NavMenu ganha link "Classes" real; Home ganha card/botão de entrada pra lista.
- **Sem edição nenhuma** — read-only.

## Critérios de aceite

- [ ] 11 classes visíveis na lista com custos idênticos aos do 022 (smoke no browser — orquestrador).
- [ ] Classe inválida plantada à mão aparece com diagnóstico legível e some ao corrigir (smoke — orquestrador).
- [x] Lista lê dos arquivos (ListClassFiles), nunca dos registries.
- [x] Diagnostics vêm do dry-run do 021 — nenhuma validação duplicada na UI.
- [x] Item sem preço nunca é 0 silencioso (badge + warning do 022 exibidos).
- [x] Parse error não quebra a lista nem o detalhe.

## Fora de escopo

- Qualquer edição/salvamento (itens 025+).
- Rotas HTTP/JSON (a UI consome os serviços por DI, padrão Blazor interativo do host).
