# 006 — otimizacao-fila-spawn-warmup

**Mod:** TRL-DynamicSpawn
**Status:** 🟢 Entregue
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

Investigar o motivo pelo qual o spawner em fase de Warmup sofre atrito extremo para atingir a cota `MaxBot`, ficando preso em loops repetitivos de tentativa. Investigar se há bots pendentes na fila de geração do jogo (EFT `BotsController` / `BotCreator`) que travam o processamento dos spawns subsequentes.

## Comportamento atual

Durante a partida (especialmente no warmup em mapas como Lighthouse), o spawner roda o loop repetidas vezes tentando atingir a cota `MaxBot`. Às vezes a fila fica "bloqueada" até um evento como a morte de um bot ou comando de depuração ser disparado.

## Comportamento desejado

1. Investigar como o SPT/EFT gerencia a fila assíncrona de criação de bots (`IBotCreator.ActivateBot` / `CreateBot`).
2. Verificar se a limpeza de fila de spawn (`ClearQueue` / `Cancel`) abrange todas as categorias (PMCs, Scavs, Bosses, Rogues, Raiders).
3. Otimizar a velocidade de resposta do warmup para atingir a cota `MaxBot` em poucos ciclos limpos e liberar a contagem regressiva para a próxima onda.

## Critérios de aceite

- [ ] Identificação da causa do atraso/bloqueio no preenchimento do MaxBot durante a fase de warmup.
- [ ] A limpeza de fila engloba 100% dos perfis de bot cadastrados no mod.
- [ ] O warmup atinge a cota de bots em tempo ágil sem ficar travado em loops infinitos.
