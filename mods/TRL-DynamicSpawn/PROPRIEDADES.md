# TRL Dynamic Spawn — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `TRLDynamicSpawn.settings` — TRLDynamicSpawn v3.2.9 (versão corrente; o `/compile-mod` do item 009 aplica bump minor)<br>
> **Fonte:** [Client/Helpers/Settings.cs](Client/Helpers/Settings.cs)<br>

## Host Performance Caps

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Factory Max Bots | Máximo de Bots em Factory | `int` | `15` | `5`–`50` | Número máximo de bots ativos permitidos em Factory. |
| Customs Max Bots | Máximo de Bots em Customs | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Customs. |
| Woods Max Bots | Máximo de Bots em Woods | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Woods. |
| Shoreline Max Bots | Máximo de Bots em Shoreline | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Shoreline. |
| Interchange Max Bots | Máximo de Bots em Interchange | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Interchange. |
| Reserve Max Bots | Máximo de Bots em Reserve | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Reserve. |
| Lighthouse Max Bots | Máximo de Bots em Lighthouse | `int` | `25` | `5`–`50` | Número máximo de bots ativos permitidos em Lighthouse. |
| Streets Max Bots | Máximo de Bots em Streets | `int` | `30` | `5`–`60` | Número máximo de bots ativos permitidos em Streets. |
| Ground Zero Max Bots | Máximo de Bots em Ground Zero | `int` | `20` | `5`–`50` | Número máximo de bots ativos permitidos em Ground Zero. |
| Laboratory Max Bots | Máximo de Bots em Laboratory | `int` | `20` | `5`–`50` | Número máximo de bots ativos permitidos em Laboratory. |

## Spawn Culling (Line of Sight)

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable LoS Culling | Ativar Culling LoS | `bool` | `true` | — | Impede o spawn de bots no campo de visão direto dos jogadores. |
| LoS Culling Max Distance | Distância Máxima LoS (m) | `float` | `150.0` | `10.0`–`500.0` | Distância máxima para checar linha de visão do jogador antes do spawn. |

## Smooth Spawning

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Smooth Spawning | Ativar Spawn Suave | `bool` | `true` | — | Escalona os spawns de bots para evitar stutters/travamentos durante ondas. |
| Smooth Spawning Delay | Atraso no Spawn Suave (s) | `float` | `1.5` | `0.0`–`10.0` | Atraso em segundos entre grupos de bots gerados. |

## Despawn Settings

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Enable Despawn System | Ativar Sistema de Despawn | `bool` | `true` | Chave mestre para ativar ou desativar o despespawn de bots distantes. |
| Replace Despawned Bots | Substituir Bots Despawnados | `bool` | `true` | Quando um bot faz despawn, gera um novo bot equivalente próximo ao raio ativo. |

## Spawn Bubble Settings

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Enable Spawn Bubble | Ativar Bolha de Spawn | `bool` | `true` | Força Scavs e PMCs a spawnarem apenas dentro do raio ativo do jogador. |

## Map Overlay (SPT-DynamicMaps)

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Enable Map Overlay | Ativar Overlay no Mapa | `bool` | `true` | Exibe os círculos visuais e cone de visão no mapa do SPT-DynamicMaps. |
| Show Safe Zone Circle | Exibir Círculo de Zona Segura | `bool` | `true` | Exibe o círculo vermelho de Zona Segura ao redor do jogador. |
| Show Spawn Bubble Circle | Exibir Círculo da Bolha | `bool` | `true` | Exibe o círculo ciano da Bolha de Spawn no mapa. |
| Show LoS / FOV Cone | Exibir Cone de Visão (FOV) | `bool` | `true` | Exibe o cone amarelo do campo de visão no mapa. |

## Server Config

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Reload Server Config | Recarregar Configuração do Servidor | `bool` | `false` | Marque para recarregar agora a configuração do painel web (aplica as edições feitas durante a raid). Desmarca sozinho após recarregar. |

> Funciona como um **botão**: a configuração do painel web é buscada **uma vez por raid** (item 009 / AUD-01-01). Edições feitas no painel **durante** a raid só entram na próxima raid — ou imediatamente, marcando esta opção.
