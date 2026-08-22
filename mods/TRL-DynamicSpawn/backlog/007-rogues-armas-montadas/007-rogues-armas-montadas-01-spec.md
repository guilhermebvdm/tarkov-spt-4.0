# 007 — rogues-armas-montadas

**Mod:** TRL-DynamicSpawn
**Status:** ⚪ Backlog
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

Investigar e corrigir a inteligência artificial / lógica de comportamento dos Rogues (`exUsec`) no mapa Lighthouse para usarem armas montadas (metralhadoras estacionárias e lançadores de granadas AGS).

## Comportamento atual

Os Rogues gerados caminham até o ponto da arma montada no cenário e se posicionam ao lado dela, porém nunca assumem o controle nem disparam o armamento estacionário, ficando vulneráveis e passivos na posição.

## Comportamento desejado

1. Investigar nos assemblies descompilados (`references/eft-decompiled`), no código-fonte do SPT (`references/spt-source`), no mod `ORBIT` e no SAIN como a IA nativa do jogo vincula um bot a um `StationaryWeapon` / `MountedWeapon`.
2. Verificar se o método de injeção ou atribuição de zona/papel (`BotRole.exUsec`) no TRL-DynamicSpawn está omitindo a inicialização do nó de comportamento `StationaryWeaponOwner` ou `StationaryStation`.
3. Garantir que os Rogues assumam e operem as metralhadoras e AGS normalmente.

## Critérios de aceite

- [ ] Causa técnica identificada no código de IA ou de injeção de spawn.
- [ ] Os Rogues spawnados voltam a interagir, montar e disparar com o armamento estacionário.
