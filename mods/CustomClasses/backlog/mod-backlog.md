# Backlog — CustomClasses

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões. Roadmap derivado do plano aprovado em 2026-06-07.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Scaffold + 1 classe (walking skeleton) | Projeto server C# que registra 1 classe como edition no launcher (`DescriptionLocaleKey` + locale `en`) com só skills estáticas. Prova o mecanismo de injeção de edition ponta a ponta. | [001-walking-skeleton/](./001-walking-skeleton/) | 🟢 |
| 002 | Schema de classe + loader multi-classe | **Um arquivo JSON por classe** em `Server/config/classes/`, lido **dinamicamente** → injeta N editions (soltar um `.json` = nova classe). Schema inicial (id, base, skills); **cresce a cada item** (itens/equip/skins/mults nos 003-008) até configurar *tudo* por classe. Porta recipe/anchor/balance do RZ. | [002-class-schema-loader/](./002-class-schema-loader/) | 🟢 |
| 003 | Itens + hideout + 10 classes reais | Capacidade de itens (stash/equipado/compostos via `ParentId/SlotId` + validador) **e hideout** (estações iniciais). **Conteúdo:** as 10 classes do RZ recriadas com **mesmos itens+skills** (agora equipados/compostos) + estação de hideout. Migração trazida do 007. | [003-starting-items/](./003-starting-items/) | 🟡 |
| 004 | Outfits por classe | `Customization` (Head/Body/Feet/Hands) + `Suits` via `databaseService.GetCustomization()`. | [004-outfits/](./004-outfits/) | 🟡 |
| 005 | Multiplicadores de skill por classe (client) | Multiplicadores por skill (buff/debuff) escalando o ganho de XP (sem distribuição dinâmica). Server serve config por rota; client BepInEx escala em runtime. **+ Exibir o multiplicador na tela de Skills** (no tooltip e/ou na listagem de cada skill, **mesma UX/layout** já usada lá): prefixo `+` se >1, `-` se <1; para `1` pular ou deixar bem sutil. Reusar padrão dos patches de UI de skill dos mods de referência (`SkillTooltipPatch`/`SkillPanelPatch`). | [005-skill-multipliers/](./005-skill-multipliers/) | 🟡 |
| 006 | Compat opcional com Skills-Extended | Soft-detect; multiplicadores p/ skills do SE só quando ele existe; no-op + aviso se ausente. (SE revive `ESkillId` vanilla → mecanismo já suporta.) Exemplo: Médico (FirstAid/FieldMedicine). | [006-skills-extended-compat/](./006-skills-extended-compat/) | 🟢 |
| 007 | Coexistência → aposentar RZCustomProfiles | (Migração das 10 classes movida p/ o 003.) Resolver o **clobber** do RZ (reconstrói os templates e some com nossa edition) + coexistência: saves guardam o `Edition` string do RZ; remover o RZ não pode quebrá-los. Definir transição e aposentar. | `007-migrate-rzcustomprofiles/` | ⚪ |
| 008 | i18n (multilíngue pt-BR/en) | Descrição de edition por idioma (segue a língua do servidor, fallback en; `description` string OU `{en,pt}`) + seletor de língua no F12 (default English) p/ os textos in-game. | [008-i18n/](./008-i18n/) | 🟢 |
| 009 | Ocultar edições vanilla no launcher | Config JSON p/ esconder edições da criação de perfil. Default: ocultar Standard/Left Behind/Prepare To Escape/Edge Of Darkness/Unheard/Tournament/SPT Easy start; manter SPT Developer + SPT Zero to hero + as classes do mod. (`CreateNewProfileTypesBlacklist`; v1-only) | [009-ocultar-edicoes-vanilla/](./009-ocultar-edicoes-vanilla/) | 🟢 |
| 010 | UI dos multiplicadores de skill | Refino visual do 005: borda colorida no ícone da skill (verde=buff/vermelho=debuff), seta+`±X%` à direita do nome (no lugar da seta azul) e tooltip dedicado "…devido à Classe **\<Nome\>**". Requer expor o nome da classe/edition ao client. | [010-ui-multiplicadores-skill/](./010-ui-multiplicadores-skill/) | 🟢 |

> **Pré-requisito de infra (000 — fora do sandbox do mod):** ✅ **feito em 2026-06-07.** [.agents/scripts/compile-mod.sh](../../../.agents/scripts/compile-mod.sh) agora detecta vários `.csproj`, classifica cada um (client/server/lib), builda os entry projects e instala **só as DLLs próprias** do mod (filtra SPTarkov/Unity/BepInEx/NuGet) nos 2 destinos (server → `SPT/user/mods/`, client → `BepInEx/plugins/`). Verificado: syntax + classificação contra SkillDistribution/Skills-Extended. Build dotnet end-to-end pendente até existirem projetos (item 001).

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly/SPT
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
7. `/compile-mod <ref>` → build + instala no SPT
8. `/code-review <ref>` → revisão do build
