# Backlog — TRL-DynamicSpawn

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 009 | perf-config-cache-raid | Rodada 1 de performance (AUD-01-01/02/03): config do servidor cacheada por raid, poller de despawn só em raid e backoff em falha de fetch | [009-perf-config-cache-raid/](./009-perf-config-cache-raid/) | 🟢 |
| 001 | portabilidade-spt-4 | Portar e corrigir a inicialização do mod TRL-DynamicSpawn para a versão SPT 4.0 | [001-portabilidade-spt-4/](./001-portabilidade-spt-4/) | 🟢 |
| 002 | webui-nomenclaturas-e-ajustes | Ajustar nomenclaturas (Ondas, Bots) e reduzir slider de espera inicial para 0-120s no painel Web | [002-webui-nomenclaturas-e-ajustes/](./002-webui-nomenclaturas-e-ajustes/) | 🟢 |
| 003 | labs-exclusivo-pmc | Remover spawns de SCAVs em Labs e distribuir a cota do MaxBot exclusivamente para PMCs (BEAR/USEC) | [003-labs-exclusivo-pmc/](./003-labs-exclusivo-pmc/) | 🟢 |
| 004 | dificuldade-bots-sain-integration | Corrigir aplicação da dificuldade de bots e ignorar configuração do mod caso o SAIN esteja ativo | [004-dificuldade-bots-sain-integration/](./004-dificuldade-bots-sain-integration/) | 🟢 |
| 005 | revisao-bloqueadores-spawn | Revisar regras de bloqueio de spawn (visão, distância, safezone, colisão) eliminando conflitos | [005-revisao-bloqueadores-spawn/](./005-revisao-bloqueadores-spawn/) | 🟢 |
| 006 | otimizacao-fila-spawn-warmup | Investigar atrito no atingo do MaxBot durante warmup e garantir limpeza completa da fila de spawn | [006-otimizacao-fila-spawn-warmup/](./006-otimizacao-fila-spawn-warmup/) | 🟢 |
| 007 | rogues-armas-montadas | Investigar e corrigir motivo dos Rogues se posicionarem em armas montadas sem usá-las | [007-rogues-armas-montadas/](./007-rogues-armas-montadas/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
