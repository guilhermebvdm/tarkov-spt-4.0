# Disable Vanilla Waves

O usuário relatou que se colocar as opções de bots no modo offline como "Nenhum", nenhum bot spawna, nem mesmo o do nosso mod. Isso ocorre porque o Tarkov desabilita toda a IA (`BotsController.Init` passa `botEnabled = false`) quando essa opção é selecionada.

Para contornar isso e permitir que o nosso mod tenha controle 100% absoluto sem interferência do Tarkov, nós precisamos dizer ao usuário para jogar como "AsOnline" (Como no Online) e bloquear a geração de waves vanilla via código.

## Proposed Changes

### [MODIFY] Client/Patches/Patches.cs

Nós vamos adicionar dois novos patches na classe `Patches.cs`:

1. `DisableVanillaWavesPatch`:
   - Alvo: `BotsController.ActivateBotsByWave(BotWaveDataClass wave)`
   - Ação: Retornar `false` no `[PatchPrefix]`, bloqueando completamente o agendamento e o spawn das ondas normais do jogo base.

2. `DisableVanillaBossWavesPatch`:
   - Alvo: `BotsController.ActivateBotsByWave(BossLocationSpawn wave)`
   - Ação: Retornar `false` no `[PatchPrefix]`, bloqueando completamente os bosses e snipers estáticos do jogo base de spawnarem de forma descontrolada.
   
*(Nota: O nosso mod já possui lógica própria para spawnar Bosses, então não perderemos os chefes, apenas assumiremos o controle deles).*

### [MODIFY] Client/Plugin.cs

- Habilitar esses dois novos patches no `Plugin.cs` dentro do método `Awake()`.

## Verification Plan
1. Recompilar o Client `TRL-DynamicSpawn.dll`.
2. Pedir para o usuário testar jogando com "AsOnline" e verificar se o número de bots bate estritamente com o `maxCap` e sem spawns avulsos gerados pelo SPT.
