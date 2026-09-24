# Visceral Combat — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `com.servph.VisceralCombat` — Visceral Combat v3.7.0<br>
> **Fonte:** [original/VisceralCombat/VisceralCombat/VisceralEntry.cs](original/VisceralCombat/VisceralCombat/VisceralEntry.cs)<br>

## General

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Visceral Combat Enabled | Habilitar Visceral Combat | `bool` | `true` | Liga/desliga todas as features visuais e de gameplay pesadas do mod (desmembramento, sangue, ragdoll customizado, efeitos de item físico) de uma vez. A queda de arma e a queda de capacete/óculos continuam funcionando mesmo desligado, pra sincronizar corretamente com outros jogadores do raid que estejam com o mod ativo. |

Desligar este toggle mestre desativa, de uma vez, as 6 categorias abaixo: Dismemberment, Blood, Ragdolls \| Ragdoll Physical Properties, Ragdolls \| Character Properties (exceto "Drop Weapon On Death", ver nota), Combat \| Visuals e Physics \| Item Physical Properties. **"Drop Weapon On Death" e "Drop Headwear/Eyewear On Head Dismemberment" têm toggle próprio e continuam ativos mesmo com o mestre desligado** — não é preferência de UX, é sincronização de dados: se outro jogador do raid tem o mod ligado e vê a arma/capacete/óculos/máscara/fone cair (Fika sincroniza a morte pra todos os peers), quem está com o mestre desligado precisa dropar os mesmos itens também, senão eles ficam duplicados (chão + inventário do morto) pra quem tem o mod ativo — mesma classe de bug já corrigida por `CR-NET-LOCK-01` (item 002). "Helmet Knock Off Chance" (abaixo) é uma feature separada, sem relação de sincronização — segue o toggle mestre normalmente.

## Dismemberment

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Dismemberment Enabled | Habilitar Desmembramento | `bool` | `true` | Desativa literalmente TUDO relacionado a desmembramento. |
| Drop Headwear/Eyewear On Head Dismemberment | Derrubar Capacete/Óculos no Desmembramento de Cabeça | `bool` | `true` | Derruba capacete, óculos, máscara (FaceCover) e fone (Earpiece) com 100% de chance quando a cabeça é efetivamente desmembrada — os 4 itens presos na cabeça, apesar do nome da opção (mantido por compatibilidade com o valor já salvo dos jogadores). Cobre tanto o desmembramento na hora da morte quanto o post-mortem (tiro num cadáver já morto). Gatilho distinto da chance configurável de "Helmet Knock Off Chance" — ver nota nessa propriedade sobre o alcance atual dela. |

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
| Grenade Intensity | Intensidade de Explosão de Granadas | `float` | `40.0` | Força aplicada por explosões de granada. |
| Player Body Collision | Colisão Física com Corpos | `bool` | `true` | Permite pisar e colidir com corpos mortos. Também controla se atropelar um corpo já acomodado o empurra, proporcional à velocidade do jogador. |

## Ragdolls \| Ragdoll Physical Properties

> Seção com grafia diferente da anterior ("Physical" vs. "Phsyical") — existente no código desde antes desta sessão, não corrigida aqui pra não quebrar valores já salvos pelo usuário (seção faz parte da chave `Config.Bind`).

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Head Impulse Intensity | Intensidade de Impacto na Cabeça | `float` | `1.0` | Multiplicador de força pra tiro na cabeça. |
| Torso Impulse Intensity | Intensidade de Impacto no Torso | `float` | `1.0` | Multiplicador de força pra tiro no torso. |
| Arms Impulse Intensity | Intensidade de Impacto nos Braços | `float` | `1.0` | Multiplicador de força pra tiro nos braços. |
| Legs Impulse Intensity | Intensidade de Impacto nas Pernas | `float` | `1.0` | Multiplicador de força pra tiro nas pernas. |
| Corpse Kick Intensity | Intensidade de Chute em Corpos | `float` | `0.5` | Multiplicador de força ao atropelar/chutar um corpo (contato físico, sem tiro). Independente dos multiplicadores de impacto de bala por parte do corpo. |

## Ragdolls \| Character Properties

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Shoot off Helmets | Arrancar Capacete com Tiros | `bool` | `true` | Permite soltar capacetes com impactos na cabeça. |
| Drop Weapon On Death | Derrubar Arma na Morte | `bool` | `true` | Solta a arma em mãos (exceto faca) como item avulso ao morrer, em vez de deixá-la presa ao cadáver. |
| Helmet Knock Off Chance | Chance de Arrancar Capacete (%) | `float` | `15.0` | Probabilidade percentual do capacete voar ao levar tiro **não-fatal** na cabeça. ref: CR-04-01 — não dispara em tiros de cabeça fatais (o guard de `IsAlive`, `CR-02-02`, evita duplicar o item quando o bot morre; se a cabeça também não for desmembrada, o capacete permanece no cadáver como loot em vez de ter uma chance de cair). |
| Duration for anim swap | Duração da Troca de Animação | `float` | `1.0` | Duração do blend da animação de morte. |
| Duration for Mapping Weight swap | Duração do Peso do Mapeamento | `float` | `1.0` | Transição do peso de ragdoll ativo. |
| Use Active Ragdolls | Usar Ragdolls Ativos | `bool` | `true` | Ativa física de ragdoll ativo com animações corporais. |

## Ragdolls \| Performance

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Disable Active Ragdolls After Animation | Desativar Ragdolls Após Animação | `bool` | `true` | Converte ragdoll ativo para estático após término da animação. |
| Allow AI to Activate Ragdolls | Permitir IA Ativar Ragdolls | `bool` | `true` (original `false`) | Se bots IAs acionam ragdolls ativos nos alvos. |
| Max Distance the Ragdolls can Activate at | Distância Máxima de Ativação (m) | `int` | `50` | Distância limite para ativação de ragdoll ativo. |
| Ragdoll Sleep Time | Tempo de Repouso do Ragdoll (s) | `int` | `15` | Tempo até colocar o corpo em repouso físico (faixa de 1 a 15). |

## Combat \| Visuals

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Infinite Shell Casing Lifetime | Tempo de Vida Infinito de Cartuchos | `bool` | `false` | Desativa remoção automática de cápsulas no chão. |

## Physics \| Item Physical Properties

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Item Physics | Física Avançada em Itens Dropados | `bool` | `true` | Aplica física interativa a itens largados. Também controla se atropelar um item largado o empurra, proporcional à velocidade do jogador. |
| Item Force Intensity | Intensidade de Força em Itens | `float` | `10.0` | Multiplicador de força aplicada aos itens físicos (impacto de bala). |
| Item Kick Intensity | Intensidade de Chute em Itens | `float` | `1.5` | Multiplicador de força ao atropelar/chutar um item largado (contato físico, sem tiro). Independente de "Item Force Intensity". |
