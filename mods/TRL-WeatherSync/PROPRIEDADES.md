# TRL-WeatherSync (v1.3.0)

Propriedades expostas no F12 (BepInEx Configuration Manager).

## Seção: Networking

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Enable Weather Sync | Ativar Sincronização de Clima | bool | `true` | - | Ativa a sincronização contínua de clima entre Host e Clientes em raids FIKA. Sem efeito fora de raids coop. IMPORTANTE: este mod precisa estar instalado em TODOS os participantes da raid (Host e Clientes) — um peer sem o mod pode causar problemas de rede para todo mundo na raid. |
| Sync Interval Seconds | Intervalo de Sincronização (segundos) | float | `10.0` | 5 a 30 | Intervalo, em segundos, entre cada pacote de sincronização de clima enviado pela autoridade de clima da raid. Valores menores deixam o clima mais preciso, mas aumentam o tráfego de rede. |

## Seção: Rain

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Min Rain Drop Size | Tamanho Mínimo da Gota de Chuva | float | `0.08` | 0.0 a 0.30 | Define o tamanho mínimo visível da gota de chuva (útil para enxergar chuvas fracas de frente). Ajusta ao vivo sem precisar reiniciar a raid. Não altera chuvas fortes caso já ultrapassem este tamanho. Defina 0 para desativar. |

## Seção: Lens Drops

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Enable Lens Drops Tuning | Ativar Calibragem de Gotas na Lente | bool | `true` | - | Ativa a calibragem dinâmica de gotas de chuva na lente da câmera reativas ao ângulo de visão. Se desativado, o comportamento retorna 100% ao original do jogo. |
| Look Up Multiplier | Multiplicador ao Olhar para Cima | float | `2.5` | 1.0 a 5.0 | Multiplicador de quantidade e frequência de gotas ao olhar para cima (para o céu). |
| Forward Rate Multiplier | Multiplicador de Taxa Frontal | float | `1.5` | 0.5 a 3.0 | Multiplicador base da taxa de gotas ao olhar para a frente/horizonte (reforça a presença de pingos em chuvas fracas). |
| Look Down Drain Effect | Efeito de Drenagem ao Olhar para Baixo | bool | `true` | - | Faz as gotas da lente secarem/escorrerem rapidamente ao inclinar a cabeça para o chão (simula a aba do capacete/boné protegendo o rosto). |
| Max Drop Lifetime Seconds | Tempo Máximo de Vida da Gota (segundos) | float | `8.0` | 2.0 a 25.0 | Tempo máximo de vida de cada gota na lente em segundos. Valores menores deixam o ciclo de gotas mais contínuo e orgânico (o padrão do jogo é 25s). |

## Seção: Storm — removida (06-fix-01, 2026-09-12)

A `ConfigEntry` "Storm Check Cooldown Seconds" e toda a feature de "tempestade sincronizada" (CR-01-02) foram removidas — causavam nevasca de inverno incorreta em vez de tempestade de verão (ver `06-fix-01.md` do item 001). O clima de tempestade real (chuva/vento/nuvem altos) continua funcionando normalmente via sincronização contínua (`Networking` acima), sem precisar de nenhuma troca de estado especial.

## Item 002 — Gerenciador Ciclo Natural Estações

Este item **não adiciona nenhuma `ConfigEntry` nova** — decisão da spec funcional: a duração de cada estação, o modo de progressão e os pesos de probabilidade de clima por estação vêm de `modded/Server/Config/season-cycle.json`, um arquivo lido pelo SPT Server (lado servidor), não do painel F12 do client. Ver `README.md` § "Requisito de instalação" para a exigência de manter esse `.json` idêntico entre todas as instalações FIKA da raid.
