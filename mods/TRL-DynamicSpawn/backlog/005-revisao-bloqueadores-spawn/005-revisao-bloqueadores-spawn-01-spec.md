# 005 — revisao-bloqueadores-spawn

**Mod:** TRL-DynamicSpawn
**Status:** 🟢 Entregue
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

Revisão abrangente de todas as regras e checagens de validação de pontos de spawn (distância mínima do jogador, campo de visão / Line of Sight, Zona Segura, colisão com geometria) para identificar se há conflitos, bloqueios mútuos ou falsos negativos que impeçam o spawn legítimo de bots.

## Comportamento atual

Existem múltiplos bloqueadores (Safe Zone distance, Spawn Bubble distance, LoS culling, min spawn distance) atuando em paralelo. Em mapas densos ou com muitos obstáculos, essas regras podem estar entrando em conflito e anulando praticamente todos os pontos de spawn válidos da zona.

## Comportamento desejado

1. Mapear a ordem de execução de todas as verificações em `DynamicSpawnManager.cs` e `Methods.cs`.
2. Identificar regras redundantes ou contraditórias.
3. Simplificar o pipeline de validação garantindo que o spawn não seja impedido injustificadamente.

## Critérios de aceite

- [ ] Relatório completo de auditoria do fluxo de validação de spawn.
- [ ] Eliminação de redundâncias e conflitos entre SafeZone, LoS e SpawnBubble.
- [ ] Pontos válidos de spawn são aproveitados com maior taxa de sucesso sem violar o antirush/safe zone.
