# TRL-WeatherSync (v1.1.0)

Propriedades expostas no F12 (BepInEx Configuration Manager).

## Seção: Networking

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Enable Weather Sync | Ativar Sincronização de Clima | bool | `true` | - | Ativa a sincronização contínua de clima entre Host e Clientes em raids FIKA. Sem efeito fora de raids coop. IMPORTANTE: este mod precisa estar instalado em TODOS os participantes da raid (Host e Clientes) — um peer sem o mod pode causar problemas de rede para todo mundo na raid. |
| Sync Interval Seconds | Intervalo de Sincronização (segundos) | float | `10.0` | 5 a 30 | Intervalo, em segundos, entre cada pacote de sincronização de clima enviado pela autoridade de clima da raid. Valores menores deixam o clima mais preciso, mas aumentam o tráfego de rede. |

## Seção: Storm

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Storm Check Cooldown Seconds | Cooldown de Verificação de Tempestade (segundos) | float | `300.0` | 60 a 1800 | Intervalo mínimo, em segundos, entre tentativas de iniciar uma tempestade sincronizada. A cada ciclo, a autoridade de clima da raid sorteia contra a probabilidade nativa de raio/trovão do jogo (baseada na nebulosidade); se o sorteio ganhar, a tempestade começa sincronizada para todos. O FIM da tempestade ainda não é sincronizado nesta versão — cada jogador sai dela pelo tempo nativo do próprio jogo. |

## Item 002 — Gerenciador Ciclo Natural Estações

Este item **não adiciona nenhuma `ConfigEntry` nova** — decisão da spec funcional: a duração de cada estação, o modo de progressão e os pesos de probabilidade de clima por estação vêm de `modded/Server/Config/season-cycle.json`, um arquivo lido pelo SPT Server (lado servidor), não do painel F12 do client. Ver `README.md` § "Requisito de instalação" para a exigência de manter esse `.json` idêntico entre todas as instalações FIKA da raid.
