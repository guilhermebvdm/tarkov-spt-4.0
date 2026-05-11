# Backlog — stancesAndCameraPositionSPT4.0.11

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
| --- | --- | --- | --- | --- |
| 001 | Stamina e velocidade por postura | Adiciona controle de drain de stamina e multiplicador de velocidade (50–100%) por postura, com props no F12. | [001-stamina-e-velocidade/](./001-stamina-e-velocidade/) | 🟢 |
| 003 | Stamina Multiplier — faixa até 10 | Amplia o teto de `Stance X Stamina Multiplier` de 3.0 para 10.0 nas 4 stances. | [003-stamina-multiplier-faixa-10/](./003-stamina-multiplier-faixa-10/) | 🟢 |
| 002 | Ciclo linear, hotkeys e snap fogo | Ciclo de scroll não-circular, teclas dedicadas por stance, e snap automático para Stance 0 ao atirar nas Stances 1/2/3. | [002-ciclo-linear-hotkeys-snap-fogo/](./002-ciclo-linear-hotkeys-snap-fogo/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
