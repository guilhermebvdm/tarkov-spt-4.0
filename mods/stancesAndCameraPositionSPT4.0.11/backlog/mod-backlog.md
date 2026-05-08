# Backlog — stancesAndCameraPositionSPT4.0.11

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Stamina e velocidade por postura | Adiciona controle de drain de stamina e multiplicador de velocidade (50–100%) por postura, com props no F12. | [001-stamina-e-velocidade/](./001-stamina-e-velocidade/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
