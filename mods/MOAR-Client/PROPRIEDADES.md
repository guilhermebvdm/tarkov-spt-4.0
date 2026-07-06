# MOAR v3.1.2

Arquivo original: [original/Helpers/Settings.cs](original/Helpers/Settings.cs)

**Nota:** Os itens marcados com **(Avançado)** só aparecem no F12 se a opção "Advanced settings" estiver ativada.

## 1. Main Settings

| Nome original | Tradução (pt-BR) | Tipo | Padrão | Faixa/Opções | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Announce Key | Tecla de Anúncio | KeyCode | End | - | Anuncia o preset atual. |
| Preset Announce On/Off | Anúncio de Preset | bool | true | - | Habilita/Desabilita o anúncio do preset ao iniciar a raid. |
| Moar Preset | Preset Moar | string | Random | Lista | Preset a ser usado. A opção 'Random' puxa um preset aleatório baseado em pesos do PresetWeights.json ao fim de cada raid. |
| Scav difficulty | Dificuldade Scav | float | - | 0 - 1.5 | Funciona com SAIN ou SPT para decidir o preset de 'dificuldade' do bot (EASY: 0, easy-MEDIUM: 0.4, easy-MEDIUM-hard: 0.6, medium-hard: 0.85, HARD-impossible: 1, etc..). |
| Pmc difficulty | Dificuldade Pmc | float | - | 0 - 1.5 | Funciona com SAIN ou SPT para decidir o preset de 'dificuldade' do bot (EASY: 0, easy-MEDIUM: 0.4, easy-MEDIUM-hard: 0.6, medium-hard: 0.85, HARD-impossible: 1, etc..). |
| Starting PMCS On/Off | PMCs Iniciais | bool | - | - | Impacto na Performance: Faz com que todos os PMCs spawnem nos primeiros minutos do jogo (intensivo na performance). |
| spawnSmoothing On/Off | Suavização de Spawn | bool | - | - | Melhora a performance: Garante espaçamento de spawn entre as ondas. (não muda a quantidade, nem o tempo geral, apenas evita picos de spawns próximos). |
| randomSpawns On/Off | Spawns Aleatórios | bool | - | - | Desliga o novo sistema de spawn em cascata e faz com que scavs/pmcs spawnem aleatoriamente pelo mapa. |
| Faction Based Aggression On/Off | Agressão Baseada em Facção | bool | false | - | Ativar este recurso faz com que PMCs controlados por IA da mesma facção não entrem em combate entre si. Sabe, como contratados militares reais. (Isto é apenas lado cliente). |

## 2. Custom game Settings

| Nome original | Tradução (pt-BR) | Tipo | Padrão | Faixa/Opções | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| PmcWaveQuantity | Quantidade de Ondas PMC | float | - | 0 - 10 | Multiplica a contagem de ondas vista no mapConfig.json do servidor por este número. |
| ScavWaveQuantity | Quantidade de Ondas Scav | float | - | 0 - 10 | Multiplica a contagem de ondas vista no mapConfig.json do servidor por este número. |
| PmcWaveDistribution | Distribuição de Ondas PMC | float | - | 0.1 - 1.9 | Determina o peso dos spawns. (0.1) as ondas spawnarão a maioria no começo, (1) Padrão: as ondas são espalhadas igualmente, ou (1.9) os spawns aparecerão perto do fim da raid. |
| ScavWaveDistribution | Distribuição de Ondas Scav | float | - | 0.1 - 1.9 | Determina o peso dos spawns. (0.1) as ondas spawnarão a maioria no começo, (1) Padrão: as ondas são espalhadas igualmente, ou (1.9) os spawns aparecerão perto do fim da raid. |
| zombiesEnabled On/Off | Zumbis Habilitados | bool | - | - | Permite que zumbis spawnem. |
| ZombieWaveDistribution | Distribuição de Ondas de Zumbi | float | - | 0.1 - 1.9 | Determina o peso dos spawns de zumbi. |
| ZombieWaveQuantity | Quantidade de Ondas de Zumbi | float | - | 0 - 10 | Multiplica a contagem de ondas vista no mapConfig.json do servidor por este número. |
| ZombieHealth | Vida do Zumbi | float | - | 0 - 3 | Controla a vida dos zumbis. |
| MaxBotCap | Limite Máx de Bots | int | - | 0 - 50 | Máximo de bots vivos ao mesmo tempo. |
| MaxBotPerZone | Máx de Bots por Zona | int | - | 0 - 15 | Máximo de bots permitidos em qualquer zona de spawn, recomendado não alterar. |
| sniperGroupChance Percentage | Chance de Grupo Sniper | float | - | 0 - 1 | Controla a chance de spawnar em grupo vs solo, quantidade máxima configurada pelo maxGroup. |
| scavGroupChance Percentage | Chance de Grupo Scav | float | - | 0 - 1 | Controla a chance de spawnar em grupo vs solo, quantidade máxima configurada pelo maxGroup. |
| pmcGroupChance Percentage | Chance de Grupo PMC | float | - | 0 - 1 | Controla a chance de spawnar em grupo vs solo, quantidade máxima configurada pelo maxGroup. |
| pmcMaxGroupSize | Tamanho Máx de Grupo PMC | int | - | 0 - 10 | Tamanho máximo do grupo de PMC. |
| scavMaxGroupSize | Tamanho Máx de Grupo Scav | int | - | 0 - 10 | Tamanho máximo do grupo de scavs. |
| sniperMaxGroupSize | Tamanho Máx de Grupo Sniper | float | - | 0 - 5 | Tamanho máximo do grupo de snipers. |
| bossOpenZones On/Off | Zonas Abertas de Bosses | bool | - | - | Experimental: Faz com que os bosses principais possam spawnar em qualquer lugar. |
| randomRaiderGroup On/Off | Grupo de Raiders Aleatório | bool | - | - | Experimental: Faz com que um grupo de raiders aleatório possa spawnar em qualquer lugar. |
| randomRaiderGroupChance | Chance de Grupo Raider | int | - | 0 - 100% | Chance de spawnar um grupo de raiders. |
| randomRogueGroup On/Off | Grupo de Rogues Aleatório | bool | - | - | Experimental: Faz com que um grupo de rogues aleatório possa spawnar em qualquer lugar. |
| randomRogueGroupChance | Chance de Grupo Rogue | int | - | 0 - 100% | Chance de spawnar um grupo de rogues. |
| disableBosses On/Off | Desabilitar Bosses | bool | - | - | Desabilita todos os bosses, bom para debugging. |
| mainBossChanceBuff | Buff de Chance do Boss Principal | int | - | 0 - 100% | Aumenta a chance de spawn do boss 'principal' único de cada mapa por esta porcentagem. |
| bossInvasion On/Off | Invasão de Bosses | bool | - | - | Permite que bosses principais (não knights, rogues, raiders) invadam outros mapas com uma comitiva reduzida, por padrão eles spawnarão nas localizações nativas de boss. |
| bossInvasionSpawnChance | Chance de Spawn de Invasão de Boss | int | - | 0 - 100% | Porcentagem de chance de cada boss invasor spawnar. |
| gradualBossInvasion On/Off | Invasão Gradual de Bosses | bool | - | - | Faz com que bosses invasores não spawnem todos no começo (recomendado para performance). |

## 3. Debug

| Nome original | Tradução (pt-BR) | Tipo | Padrão | Faixa/Opções | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| debug On/Off | Debug | bool | - | - | Serve para debugar saídas do servidor. Deixe desligado se não souber o que está fazendo. |

## 4. Advanced
**Todas as configurações desta seção são (Avançado)**

| Nome original | Tradução (pt-BR) | Tipo | Padrão | Faixa/Opções | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Delete a sniper spawn | Deletar spawn sniper | KeyCode | Nenhum | - | Atalho para deletar o spawn de sniper mais próximo. |
| Delete a player spawn | Deletar spawn player | KeyCode | Nenhum | - | Atalho para deletar o spawn inicial de player mais próximo. |
| Delete a pmc spawn | Deletar spawn pmc | KeyCode | Nenhum | - | Atalho para deletar o spawn de pmc mais próximo. |
| Delete a scav spawn | Deletar spawn scav | KeyCode | Nenhum | - | Atalho para deletar o spawn de scav mais próximo. |
| Add a sniper spawn | Adicionar spawn sniper | KeyCode | Nenhum | - | Atalho para adicionar um spawn de sniper. |
| Add a player spawn | Adicionar spawn player | KeyCode | Nenhum | - | Atalho para adicionar um spawn inicial de player. |
| Add a pmc spawn | Adicionar spawn pmc | KeyCode | Nenhum | - | Atalho para adicionar um spawn de pmc. |
| Add a scav spawn | Adicionar spawn scav | KeyCode | Nenhum | - | Atalho para adicionar um spawn de scav. |
| Spawnpoint overlay On/Off | Overlay de spawnpoint | bool | false | - | Valor de desenvolvedor - Liga/Desliga ferramenta dev pointOverlay (Requer reiniciar). |
