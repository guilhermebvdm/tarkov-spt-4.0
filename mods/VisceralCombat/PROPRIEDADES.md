# Visceral Combat — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `com.servph.VisceralCombat` — Visceral Combat v3.7.0<br>
> **Fonte:** [original/VisceralCombat/VisceralCombat/VisceralEntry.cs](original/VisceralCombat/VisceralCombat/VisceralEntry.cs)<br>

## Dismemberment

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Dismemberment Enabled | Habilitar Desmembramento | `bool` | `true` | Desativa literalmente TUDO relacionado a desmembramento. |

## Blood

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Blood Effects Enabled | Habilitar Efeitos de Sangue | `bool` | `true` | Desativa literalmente TUDO relacionado a efeitos de sangue. |

## Blood \| Splatters

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Blood Splatter Size | Tamanho dos Respingos | `float` | `1.0` | Tamanho do respingo de sangue no ambiente. |
| Use Old Blood Decals | Usar Decalques Antigos | `bool` | `false` | Oculta os decalques antigos de sangue no chão mantendo o efeito visual. |

## Blood \| Trails & Flows

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Arterial Spraying | Jorro Arterial | `bool` | `true` | Habilita sangramento arterial jorrando. |
| Arterial Spray Minimum Time (Seconds) | Tempo Mínimo de Jorro (s) | `float` | `8.0` | Tempo mínimo de duração do jorro arterial. |
| Arterial Spray Maxmimum Time (Seconds) | Tempo Máximo de Jorro (s) | `float` | `2.0` | Tempo máximo de duração do jorro arterial. |

## Blood \| Spurts

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Bleed Maxmimum Time (Seconds) | Tempo Máximo de Sangramento (s) | `float` | `2.0` | Duração máxima dos espasmos/borrifos de sangue. |

## Blood \| Performance

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Maximum Ground Decals | Máximo de Decalques no Chão | `int` | `2048` | Quantidade máxima de decalques no chão antes de remover os mais antigos. |

## Ragdolls \| Ragdoll Phsyical Properties

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Bullet Intensity | Intensidade do Impacto de Tiro | `float` | `85.0` | Força aplicada aos corpos por tiros (depende do calibre). |
| Grenade Intensity | Intensidade de Explosão de Granadas | `float` | `190.0` | Força aplicada por explosões de granada. |
| Player Body Collision | Colisão Física com Corpos | `bool` | `false` | Permite pisar e colidir com corpos mortos. |

## Ragdolls \| Character Properties

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Shoot off Helmets | Arrancar Capacete com Tiros | `bool` | `true` | Permite soltar capacetes com impactos na cabeça. |
| Helmet Knock Off Chance | Chance de Arrancar Capacete (%) | `float` | `15.0` | Probabilidade percentual do capacete voar ao levar tiro. |
| Duration for anim swap | Duração da Troca de Animação | `float` | `1.0` | Duração do blend da animação de morte. |
| Duration for Mapping Weight swap | Duração do Peso do Mapeamento | `float` | `1.0` | Transição do peso de ragdoll ativo. |
| Use Active Ragdolls | Usar Ragdolls Ativos | `bool` | `true` | Ativa física de ragdoll ativo com animações corporais. |

## Ragdolls \| Performance

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Disable Active Ragdolls After Animation | Desativar Ragdolls Após Animação | `bool` | `true` | Converte ragdoll ativo para estático após término da animação. |
| Allow AI to Activate Ragdolls | Permitir IA Ativar Ragdolls | `bool` | `true` (original `false`) | Se bots IAs acionam ragdolls ativos nos alvos. |
| Max Distance the Ragdolls can Activate at | Distância Máxima de Ativação (m) | `int` | `50` | Distância limite para ativação de ragdoll ativo. |
| Ragdoll Sleep Time | Tempo de Repouso do Ragdoll (s) | `int` | `60` | Tempo até colocar o corpo em repouso físico. |

## Combat \| Visuals

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Infinite Shell Casing Lifetime | Tempo de Vida Infinito de Cartuchos | `bool` | `false` | Desativa remoção automática de cápsulas no chão. |

## Physics \| Item Physical Properties

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Item Physics | Física Avançada em Itens Dropados | `bool` | `false` | Aplica física interativa a itens largados. |
| Item Force Intensity | Intensidade de Força em Itens | `float` | `0.3` | Multiplicador de força aplicada aos itens físicos. |
