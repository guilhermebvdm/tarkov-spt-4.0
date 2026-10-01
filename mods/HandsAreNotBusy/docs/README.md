# HandsAreNotBusy — Documentação Técnica

> Visão geral e documentação arquitetural do mod **HandsAreNotBusy (HANB)**, responsável por fornecer um mecanismo manual de recuperação quando as mãos do jogador entram no estado de trava de controle (*Hands are busy*) no SPT / Escape From Tarkov.

---

## Sumário de Documentos

| # | Documento | Tema Principal | Status |
|---|---|---|---|
| 01 | [01-visao-geral-e-arquitetura.md](01-visao-geral-e-arquitetura.md) | Arquitetura do plugin, ponto de injeção Harmony e ciclo de vida do componente. | 🟢 Vivo |
| 02 | [02-mecanismo-de-recuperacao-de-maos.md](02-mecanismo-de-recuperacao-de-maos.md) | Análise detalhada do pipeline `FixHandsController`, purge de `List_0` e reinicialização de controladores. | 🟢 Vivo |
| 03 | [03-impacto-em-coop-multiplayer-fika.md](03-impacto-em-coop-multiplayer-fika.md) | Diagnóstico de limitações em rede: descompasso cliente-servidor e deadlocks com FIKA Coop. | 🟢 Vivo |

---

## Arquivos de Código Mapeados

- Entrypoint: [`../original/HANB_Plugin.cs`](../original/HANB_Plugin.cs)
- Injeção de Componente: [`../original/HANB_Patch.cs`](../original/HANB_Patch.cs)
- Lógica de Recuperação: [`../original/HANB_Component.cs`](../original/HANB_Component.cs)
- Catálogo F12: [`../PROPRIEDADES.md`](../PROPRIEDADES.md)
