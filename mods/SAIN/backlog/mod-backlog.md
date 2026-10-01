# Backlog — SAIN

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | perf-limitador-ia-lod | Elimina busca de distância duplicada e oscilação de tick-rate no throttle de IA por distância (AI Limiter + LOD), sem mudar comportamento perceptível. | [001-perf-limitador-ia-lod/](./001-perf-limitador-ia-lod/) | 🟢 |
| 002 | bots-atiram-sem-linha-visao | Esquadrão compartilha posição de inimigo sem checar alcance/visão; checagem de tiro limpo desativada e nunca testa terreno; bots ficam travados mirando um no outro sem nunca se verem. Bug pré-existente do SAIN vanilla, exposto pela otimização multithread. | [002-bots-atiram-sem-linha-visao/](./002-bots-atiram-sem-linha-visao/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`

> **Nota:** o item 001 nasceu do fluxo auxiliar `/optimize-mod-performance` (não do `/create-spec` genérico) — sua `01-spec.md` segue o perfil de **não-regressão** (comportamento atual preservado + metas medíveis), não o perfil padrão de feature nova.
