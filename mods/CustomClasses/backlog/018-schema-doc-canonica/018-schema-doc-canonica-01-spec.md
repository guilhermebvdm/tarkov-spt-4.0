# 018 — Doc canônica do schema de classe

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-09
**Kickoff:** [018-schema-doc-canonica-00-kickoff.md](018-schema-doc-canonica-00-kickoff.md)

## Visão geral

Criar **`mods/CustomClasses/docs/class-schema.md`**: a referência canônica do schema JSON de classe (`config/classes/*.json[c]`). É um item de **documentação** — não altera código. A doc é a fonte de verdade para as specs do editor web (019–029) e para a validação do pipeline (021); a verdade primária é o código (`ClassDefinition.cs` + loader + builders), e a doc deve refleti-lo fielmente.

## Critérios de aceite

- [ ] A doc cobre **100% dos campos** de `ClassDefinition`: `name`, `enabled`, `baseEdition`, `displayName {en,pt}`, `description` (string legada OU `{en,pt}`), `iconFile`, `nameColor`, `skills`, `skillMultipliers`, `hideout`, `outfit {usec,bear}.{upper,lower}`, `loadout {equipped, stash}` — com nome exato no JSON, tipo, obrigatório/opcional, default e o que controla.
- [ ] `ItemSpec` (`tpl|preset`, `premium`, `count`, `ammo`, `loadedMag`, `chambered`, `contents[]`, `mods[]`) e `ModSpec` (`slotId`, `tpl`, `mods[]` recursivo) documentados **com exemplo JSON de cada forma** (tpl simples, preset, preset premium, árvore manual, contêiner com contents, mag carregado + câmara).
- [ ] Semântica dos builders descrita: remoção do ocupante do slot, resolução de preset (default vs premium vs stash), stash = lista plana **sem posição** (GridPacker em runtime: first-fit + rotação, stack-aware), hideout (estação ativa, não "em construção"), outfit (vanilla vs aparência direta, validação de facção).
- [ ] Toda regra de validação do loader referenciada **por símbolo/método** (ex.: `CustomClassesMod.RegisterClass`, `InventoryBuilder.Apply`) — **nunca por número de linha** (o item 021 refatora esses arquivos).
- [ ] Limites conhecidos registrados: 4 skills dependem do Skills-Extended (soft-detect; multiplicador inócuo sem SE), classe inválida é pulada sem derrubar as demais, comportamento de `enabled:false`.
- [ ] Cabeçalho no padrão docs (blockquote Data/Status/Responsáveis/Referências) + rodapé `## Histórico de Alterações`.
- [ ] `config/classes/_docs/exampleClass.jsonc` **consistente com a doc** — divergências corrigidas (preservando o estilo de comentários).

## Fora de escopo

- Qualquer mudança de código (loader/builders) — a doc descreve o que existe.
- Validação automatizada do schema (JSON Schema/pipeline) — item 021.
- Specs do editor web — itens 019–029 (consomem esta doc).

## Referências

- Kickoff: [018-schema-doc-canonica-00-kickoff.md](018-schema-doc-canonica-00-kickoff.md)
- Código (verdade): [ClassDefinition.cs](../../modded/Server/ClassDefinition.cs), [CustomClassesMod.cs](../../modded/Server/CustomClassesMod.cs), [InventoryBuilder.cs](../../modded/Server/InventoryBuilder.cs), [GridPacker.cs](../../modded/Server/GridPacker.cs), [HideoutBuilder.cs](../../modded/Server/HideoutBuilder.cs), [OutfitBuilder.cs](../../modded/Server/OutfitBuilder.cs)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Spec funcional criada (item de documentação — enxuta) |
