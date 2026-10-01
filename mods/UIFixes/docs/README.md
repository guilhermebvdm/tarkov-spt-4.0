# UIFixes — Documentação Técnica

> Visão geral e documentação arquitetural do mod **UIFixes**, a mais abrangente suíte de melhorias de interface, usabilidade, atalhos, seleção múltipla e qualidade de vida para o SPT e Escape From Tarkov.

---

## Sumário de Documentos

| # | Documento | Tema Principal | Status |
|---|---|---|---|
| 01 | [01-visao-geral-e-arquitetura.md](01-visao-geral-e-arquitetura.md) | Arquitetura geral, pontos de entrada, patches Harmony e integração com o ecossistema FIKA. | 🟢 Vivo |
| 02 | [02-sistema-de-multiselecao-e-stash.md](02-sistema-de-multiselecao-e-stash.md) | Sistema de seleção múltipla retangular, manipulação em massa e interoperabilidade. | 🟢 Vivo |
| 03 | [03-gameplay-e-controles-hibridos.md](03-gameplay-e-controles-hibridos.md) | Controles híbridos (Toggle/Hold), modificação de armas equipadas e comportamento em raid. | 🟢 Vivo |
| 04 | [04-sistemas-de-inventario-troca-e-empilhamento.md](04-sistemas-de-inventario-troca-e-empilhamento.md) | Troca direta de itens no mesmo slot (Item Swapping), auto-empilhamento e contêineres. | 🟢 Vivo |
| 05 | [05-mercado-comerciantes-e-janelas.md](05-mercado-comerciantes-e-janelas.md) | Otimizações no Flea Market, entrega automática de missões e inspeção 3D de armas. | 🟢 Vivo |

---

## Arquivos de Código Mapeados

- Entrypoint: [`../original/src/Plugin.cs`](../original/src/Plugin.cs)
- Configurações e Seções F12: [`../original/src/Settings/Settings.cs`](../original/src/Settings/Settings.cs) e [`../PROPRIEDADES.md`](../PROPRIEDADES.md)
- Integração e Sincronização FIKA: [`../original/src/Fika/Sync.cs`](../original/src/Fika/Sync.cs)
- Núcleo de Multiselect: [`../original/src/Multiselect/MultiSelect.cs`](../original/src/Multiselect/MultiSelect.cs)
- Item Swapping: [`../original/src/Patches/SwapPatches.cs`](../original/src/Patches/SwapPatches.cs)
