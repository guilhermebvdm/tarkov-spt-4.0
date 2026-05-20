# Backlog — RZCustomProfiles

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
| --- | --- | --- | --- | --- |
| 001 | Perfis customizados temáticos | 10 perfis de classe (médico, caçador, fuzileiro, etc.) com skills pré-elevadas (budget ponderado 28-32), 1 estação de hideout temática e loadout inicial ~1.7M ₽ no stash. | [001-custom-profiles/](./001-custom-profiles/) | 🟢 |
| 002 | Redesign de skills com budget por categoria | Remove 20 skills mortas no SPT 4.0.13 (FirstAid, Sniping, NightOps, etc.) e redistribui skills por categoria (Ph/M/C/P) com orçamento por classe. Renomeia Op. Noturno → Op. Furtivo. | [002-custom-profiles/](./002-custom-profiles/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
