# 018 — Doc canônica do schema de classe · As-Built

**Mod:** CustomClasses · **Build:** 2026-06-10 · **Kickoff:** [00-kickoff](018-schema-doc-canonica-00-kickoff.md) · **Spec:** [01-spec](018-schema-doc-canonica-01-spec.md)

> Item de documentação (sem código). Entrega principal: `mods/CustomClasses/docs/class-schema.md`.

## Arquivos

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `docs/class-schema.md` (270 linhas) | Referência canônica: carga/loader (§1), tabela completa dos campos do `ClassDefinition` (§2, incl. `displayName` e i18n §2.1), `loadout`/`ItemSpec`/`ModSpec` com exemplos de cada forma (§3), builders — equipado/preset/stash-GridPacker/hideout/outfit (§4), validações por símbolo/método (§5), limites Skills-Extended (§6). Cabeçalho/rodapé no padrão docs do repo. |
| CRIADO | `backlog/018-.../018-...-01-spec.md` | Spec funcional enxuta (critérios de aceite do kickoff). |
| CORRIGIDO | `config/classes/_docs/exampleClass.jsonc` | **Divergência encontrada:** faltava `displayName` (campo existe no DTO `ClassDefinition.DisplayName` desde o item 011, usado pelo registry visual). Adicionado bloco comentado + exemplo `{en,pt}`. |

## Verificação

- Doc cobre 100% dos campos do DTO (conferido contra `ClassDefinition.cs`); refs de validação por símbolo (`CustomClassesMod.RegisterClass`, `ApplySkills`, `InventoryBuilder.Apply`, `OutfitBuilder.Apply`, `HideoutBuilder.Apply`), zero refs por linha.
- `exampleClass.jsonc` consistente com o DTO (única divergência era o `displayName`, corrigida).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | As-built. Doc criada na sessão de 2026-06-09 (agente interrompido por limite); reconciliação do exampleClass + as-built concluídos em 2026-06-10. |
