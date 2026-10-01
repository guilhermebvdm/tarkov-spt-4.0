# Backlog — ORBIT-2.1

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 002 | Pacotes Fika do addon no padrão AP-11 | Envelope de comprimento, leitura com `TryGet*`, flag `Valid` e fim do `UnregisterPacket` em `OrbitDoorPacket` e `OrbitGhostFightPacket`. Herdado do upstream; muda o formato dos pacotes. | [002-pacotes-fika-ap11/](./002-pacotes-fika-ap11/) | ⚪ |
| 001 | Downgrade SPT 4.1 → 4.0 | Portar o ORBIT 2.1.0 (cliente, addon Fika e servidor), escrito para SPT 4.1, para rodar no SPT 4.0.13 / EFT 0.16.9 usando o de-para de classes lido ao contrário. | [001-downgrade-spt41-spt40/](./001-downgrade-spt41-spt40/) | 🟡 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
