# 004 — dificuldade-bots-sain-integration

**Mod:** TRL-DynamicSpawn
**Status:** 🟢 Entregue
**Criado:** 2026-08-04T21:35:00-03:00

## Visão geral

Ajustar e validar a aplicação da dificuldade dos bots configurada no TRL-DynamicSpawn. Além disso, se o mod SAIN (Solarint's AI modifications) estiver instalado no cliente/servidor, o TRL-DynamicSpawn deve delegar o gerenciamento de IA/dificuldade ao SAIN, evitando conflitos de atributos.

## Comportamento atual

A dificuldade configurada no mod parece não surtir efeito ou pode sobrescrever os perfis de IA do SAIN de forma indesejada.

## Comportamento desejado

1. Validar a atribuição de dificuldade no método de spawn do C# (`ChooseProfile` / `BotDifficulty`).
2. Implementar detecção do SAIN (via BepInEx plugin search ou BSF/chain).
3. Se o SAIN estiver presente, ignorar a sobrescrita de dificuldade do TRL-DynamicSpawn, permitindo que a IA do SAIN assuma o controle total da agressividade e precisão.

## Critérios de aceite

- [ ] A dificuldade configurada no mod funciona quando o SAIN não está instalado.
- [ ] O mod detecta a presença do SAIN e ignora/desativa a alteração forçada de dificuldade.
