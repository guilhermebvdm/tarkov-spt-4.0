# 003 — labs-exclusivo-pmc

**Mod:** TRL-DynamicSpawn
**Status:** 🟢 Entregue
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

O mapa Laboratory (Labs) no Escape From Tarkov não possui spawns de Scavs comuns no jogo base. O mod TRL-DynamicSpawn deve respeitar essa regra, alocando 100% da cota do MaxBot para PMCs (BEAR e USEC) e deixando a geração de Raiders a cargo do jogo Vanilla.

## Comportamento atual

Atualmente o mod pode tentar spawnar Scavs (`assault`) no mapa Labs, ocupando vagas do MaxBot que deveriam ser dedicadas a PMCs.

## Comportamento desejado

1. Quando o mapa ativo for `laboratory` / `lab`, o spawner do mod bloqueia inteiramente a escolha do perfil `assault` (Scavs).
2. Toda a cota definida em `MaxBot` para o mapa Labs é dividida dinamicamente apenas entre PMCs (`sptBear` e `sptUsec`).
3. Spawns nativos de Raiders (`pmcBot`) continuam sendo gerenciados pelas ondas originais do jogo (Vanilla) como bots extras.

## Critérios de aceite

- [ ] Nenhum Scav comum (`assault`) é gerado pelo TRL-DynamicSpawn no mapa `laboratory`.
- [ ] O limite de `MaxBot` configurado para Labs é preenchido exclusivamente por PMCs (`BEAR` e `USEC`).
- [ ] Raiders nativos do jogo/alarmes de exfil funcionam normalmente sem interferência.
