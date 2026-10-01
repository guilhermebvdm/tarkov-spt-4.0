# Backlog — TRL-CoreSight

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | fundacao-e-culling-arquitetura | Estruturação base do mod, orquestrador de ciclo de vida em raid, gerenciador de sombras em interiores e culling de animação de bots | [001-fundacao-e-culling-arquitetura/](./001-fundacao-e-culling-arquitetura/) | 🔵 |
| 002 | culling-loot-dinamico-ads | Culling de loose loot à distância por tamanho de inventário com bypass imediato de visibilidade ao mirar (ADS) | [002-culling-loot-dinamico-ads/](./002-culling-loot-dinamico-ads/) | 🔵 |
| 003 | culling-sombras-luzes-locais | Desativação seletiva de projeção de sombras em lâmpadas pequenas e secundárias em corredores de interiores | [003-culling-sombras-luzes-locais/](./003-culling-sombras-luzes-locais/) | 🔵 |
| 004 | gerenciador-gc-anti-stutter | Supressão de Garbage Collection da Unity em combate/ADS e coletas preventivas controladas em momentos seguros | [004-gerenciador-gc-anti-stutter/](./004-gerenciador-gc-anti-stutter/) | 🔵 |
| 005 | camuflagem-natural-e-percepcao-ia | Camuflagem de folhagem/árvores (10m) e capim alto (2m), reação orgânica no SAIN e time-slicing suave de bots | [005-time-slicing-ia-seguro/](./005-time-slicing-ia-seguro/) | 🔵 |
| 006 | culling-capsulas-balas | Otimização da PhysX e cancelamento de spawn no nascimento de cápsulas de balas ejetadas além de 25m | [006-culling-capsulas-balas/](./006-culling-capsulas-balas/) | 🔵 |
| 007 | culling-audio-ambiente-inaudivel | Culling de fontes de áudio ambiente contínuas inaudíveis da cena (geradores, lâmpadas, motores) poupando CPU de mixagem | [007-culling-audio-ambiente-inaudivel/](./007-culling-audio-ambiente-inaudivel/) | 🔵 |
| 008 | camuflagem-zonal-e-visualizador-escudo | Camuflagem segmentada em 5 zonas anatômicas com contorno de 30cm, visualizador debug vermelho 50% e reação balística de IA | [008-camuflagem-zonal-e-visualizador-escudo/](./008-camuflagem-zonal-e-visualizador-escudo/) | 🔵 |
| 009 | catalogo-dinamico-vegetacao-malhas | Catálogo dinâmico de malhas 3D de vegetação sem colisor com indexação espacial O(1) para camuflagem zonal | [009-catalogo-dinamico-vegetacao-malhas/](./009-catalogo-dinamico-vegetacao-malhas/) | 🔵 |
| 010 | afinidade-cpu-topologia-threads | Gerenciador universal de afinidade de CPU e fixação de threads para eliminar micro-stutters e latência inter-CCX/E-Cores | [010-afinidade-cpu-topologia-threads/](./010-afinidade-cpu-topologia-threads/) | 🔵 |
| 011 | remocao-otimizador-lunetas-smart-pip | Remoção completa do Smart PiP (PiPExternalLODBias, PiPShadowDistance, StripOpticVolumetrics e patch) para eliminar bugs visuais | [011-remocao-otimizador-lunetas-smart-pip/](./011-remocao-otimizador-lunetas-smart-pip/) | 🔵 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🔵 Em andamento (aguardando validação em raid) · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
