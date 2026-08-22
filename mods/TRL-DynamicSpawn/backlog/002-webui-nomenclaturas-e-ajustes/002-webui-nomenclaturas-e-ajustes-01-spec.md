# 002 — webui-nomenclaturas-e-ajustes

**Mod:** TRL-DynamicSpawn
**Status:** 🟢 Entregue
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

Ajustes de interface no Painel Web do mod TRL-DynamicSpawn para melhorar a clareza dos termos e refinar os limites do slider de espera inicial da primeira onda.

## Comportamento atual

1. A subaba de configurações por mapa no painel Web chama-se "Configuração de mapas".
2. A subaba de bosses no painel Web chama-se "Configuração de Bosses".
3. O slider "Espera inicial da primeira onda (segundos)" possui o intervalo de 0 a 3600 segundos (até 1 hora).

## Comportamento desejado

1. Alterar o rótulo da subaba para **"Ondas"**.
2. Alterar o rótulo da subaba para **"Bots"**.
3. Reduzir o intervalo do slider para **0 a 120 segundos** (no máximo 2 minutos).

## Critérios de aceite

- [ ] A aba/subaba de configuração de mapas no painel Web exibe o nome "Ondas".
- [ ] A aba/subaba de configuração de bosses no painel Web exibe o nome "Bots".
- [ ] O slider de espera inicial da primeira onda aceita valores de no máximo 120 segundos.
- [ ] As traduções I18N (`pt` e `en`) refletem as novas nomenclaturas.
