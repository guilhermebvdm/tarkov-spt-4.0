# Backlog — SPT-Foldables

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Auto-desdobrar fone só no próprio jogador | `InventoryScreenShowPatch` desdobra automaticamente o fone de ouvido do jogador mesmo quando há um container externo aberto junto (corpo, bot, outro jogador), causando reentrância em `ItemsPanel.Show()` — painel de loot duplica/corrompe. Restringe a ação a quando nenhum container externo (`lootItem`) está sendo mostrado. `Foldables.dll` v1.0.4. | [001-desdobrar-fone-proprio-jogador/](./001-desdobrar-fone-proprio-jogador/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
