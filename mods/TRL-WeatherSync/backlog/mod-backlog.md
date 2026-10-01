# Backlog — TRL-WeatherSync

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Sincronização Contínua de Clima em Raid | Pacote de rede (`TrlWeatherSyncPacket`) que sincroniza clima continuamente entre Host e Clientes no FIKA, com evento explícito de tempestade para evitar decisões locais divergentes. | [001-sincronizacao-continua-clima-raid/](./001-sincronizacao-continua-clima-raid/) | 🟢 |
| 002 | Gerenciador Ciclo Natural Estações | Controla a progressão semanal e configurável das estações do ano, com sub-fases nativas do EFT e pesos de clima por estação adaptados dos dados reais do jogo (inverno mais chuvoso/nevado). | [002-gerenciador-ciclo-natural-estacoes/](./002-gerenciador-ciclo-natural-estacoes/) | 🟢 |
| 003 | Chuva Fraca Mais Visível | Permite configurar um piso mínimo para a escala/espessura das gotas de chuva fraca via F12, ajustável ao vivo em raid sem reiniciar. | [003-chuva-fraca-visivel/](./003-chuva-fraca-visivel/) | 🟢 |
| 004 | Gotas na Lente Reativas ao Ângulo da Câmera | Calibra dinamicamente as gotas de chuva na tela/câmera: intensifica ao olhar para o céu, reforça a presença ao olhar para a frente e zera/seca ao olhar para o chão. | [004-gotas-lente-reativas-angulo-camera/](./004-gotas-lente-reativas-angulo-camera/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
