# TarkovIRL — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `com.trl.tarkovirl` — TarkovIRL v4.0.0-beta<br>
> **Fonte:** [PrimeMover.cs](PrimeMover.cs)<br>

## a - Mod Status

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Mod | Ativar Mod | `bool` | `true` | — | Chave mestre para ativar ou desativar todos os recursos do mod. |
| Toggle Mod Key | Atalho de Ativação | `KeyboardShortcut` | `F10` (Key 285) | — | Tecla de atalho para ligar/desligar o mod rapidamente. |
| Master Sensitivity Multiplier | Multiplicador Geral de Sensibilidade | `float` | `1.0` | `0.1`–`5.0` | Escala a sensibilidade de todos os efeitos de mouse (Free Aim, Sway) simultaneamente. |

## a - Toggle base features

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| Enable weapon deadzone | Ativar Deadzone da Arma | `bool` | `true` | Área morta de mira desacoplada da câmera. |
| Enable efficiency indicator | Ativar Indicador de Eficiência | `bool` | `true` | Exibe dois pontos na parte inferior da tela cuja distância indica a eficiência atual. |
| Enable custom weapon sway | Ativar Balanço Customizado | `bool` | `true` | Balanço de arma totalmente reconstruído do zero. |
| Enable breathing effect | Ativar Efeito de Respiração | `bool` | `true` | Adiciona oscilação visual à arma dependente do nível de stamina. |
| Enable stance-dependent weapon position | Posicionar Arma por Postura | `bool` | `true` | Ao agachar, puxa a posição da arma para mais perto do personagem. |
| Enable stance transition effect | Efeito de Transição de Postura | `bool` | `true` | Adiciona uma inclinação suave na mira durante a troca de agachamento. |
| Enable extra arm stam shake | Tremor Adicional por Stamina de Braço | `bool` | `true` | Aumenta o tremor do braço conforme a stamina de braço diminui. |
| Enable small visual effects | Ativar Detalhes Visuais | `bool` | `true` | Ativa detalhes menores: recuo em rotação, lançamento de granadas e inclinação. |
| Enable footstep effect | Ativar Efeito de Passos | `bool` | `true` | Faz a arma balançar levemente a cada passo do personagem. |
| Enable aiming misalignment feature | Ativar Paralaxe/Desalinhamento | `bool` | `true` | Faz a arma girar na mão desacoplando a mira de ferro/red dot em rotação. |
| Enable directional sway feature | Ativar Balanço Direcional | `bool` | `true` | Camada extra de balanço causada por movimentação WASD. |
| Enable ADS head tilt | Inclinação de Cabeça em ADS | `bool` | `true` | Inclina levemente a cabeça ao mirar com armas que possuem coronha. |
| Enable Enhanced Weapon Transitions | Transições Aprimoradas de Arma | `bool` | `true` | Animações customizadas para transição de ombro/bandoleira. |
| Enable Shot Parallax | Paralaxe de Disparo | `bool` | `false` | Adiciona torção/tremor à arma durante o disparo com base no peso e calibre. |

## b - Adjust main feature values

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Deadzone multiplier | Multiplicador de Deadzone | `float` | `0.3` | `0.0`–`5.0` | Ajusta a intensidade geral da área morta. |
| Sway multiplier | Multiplicador de Balanço | `float` | `0.5` | `0.0`–`2.0` | Ajusta a força geral do balanço da arma. |
| Aiming misalignment multiplier | Multiplicador de Paralaxe | `float` | `16.0` | `1.0`–`100.0` | Ajusta a força do efeito de desalinhamento de mira. |
| Directional Sway Final Modifier | Modificador de Balanço Direcional | `float` | `0.12` | `0.0`–`5.0` | Ajusta o efeito de balanço por movimento WASD. |
| Weapon transition speed multiplier | Vel. de Transição de Arma | `float` | `1.3` | `0.1`–`5.0` | Multiplicador da velocidade de transição de arma. |
| Main hand smoothing layer | Suavização Principal da Mão | `float` | `1.0` | `1.0`–`20.0` | Nível de suavização aplicada aos movimentos da mão. |
| Fast Turn Threshold | Limite de Rotação Rápida | `float` | `150.0` | `0.1`–`500.0` | Velocidade em graus/s que atenua Sway e Free Aim. |
| Fast Turn Attenuation | Atenuação em Giro Rápido | `float` | `0.8` | `0.0`–`1.0` | Quanto o Sway/Free Aim são reduzidos em giros rápidos. |
| Efficiency Indicator Position | Posição do Indicador de Eficiência | `float` | `550.0` | `400`–`600` | Ajuste da posição vertical do indicador de eficiência. |

## c - Sway Values & Multipliers

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Minimum Weapon Sway | Balanço Mínimo da Arma | `float` | `0.3` | `0.0`–`2.0` | Balanço mínimo garantido mesmo para armas leves. |
| Invert Sway Direction in Vanilla | Inverter Balanço Vanilla | `bool` | `false` | — | Faz a arma liderar a câmera no modo Vanilla (Bodycam). |
| Sway Pistol Multiplier | Multiplicador de Balanço em Pistolas | `float` | `2.0` | `1.0`–`10.0` | Exagera o balanço visual para pistolas compensando o tamanho. |
| Sway Fixed Weight | Peso Fixo para Balanço | `float` | `4.0` | `0.0`–`20.0` | Peso fixo sobrescrevendo o peso dinâmico da arma. |
| Sway Fixed Ergo Norm | Ergonomia Fixa para Balanço | `float` | `0.5` | `0.0`–`1.0` | Ergonomia fixa (0 a 1) sobrescrevendo a ergonomia da arma. |
| Sway return to centre speed | Vel. de Retorno ao Centro | `float` | `15.0` | `-5`–`50` | Velocidade de autocorreção do balanço. |
| Sway Slide (Pivot) Multiplier | Multiplicador de Deslize do Pivô | `float` | `1.0` | `0.0`–`5.0` | Multiplicador de balanço horizontal de posição. |

## z - Free Aim

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable True Free Aim | Ativar True Free Aim | `bool` | `true` | — | Ativa a mira desacoplada da câmera no modo hipfire. |
| Bounds Horizontal | Limite Horizontal (Graus) | `float` | `15.0` | `0.0`–`25.0` | Ângulo horizontal máximo da área morta. |
| Bounds Vertical | Limite Vertical (Graus) | `float` | `10.0` | `0.0`–`25.0` | Ângulo vertical máximo da área morta. |
| Return Speed | Velocidade de Retorno | `float` | `5.0` | `0.1`–`20.0` | Velocidade para a arma voltar ao centro da tela. |
| Free Aim Movement Speed | Sensibilidade do Free Aim | `float` | `0.5` | `0.0`–`1.0` | Movimento relativo da arma com a rotação do mouse. |
| Enable Camera Auto-Center | Ativar Auto-Centralização de Câmera | `bool` | `false` | — | A câmera gira automaticamente para acompanhar a arma. |

## z - Free Aim ADS

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Free Aim (ADS) | Ativar Free Aim em ADS | `bool` | `false` | — | Área morta desacoplada enquanto mira (ADS). |
| Bounds Horizontal | Limite Horizontal ADS (Graus) | `float` | `5.0` | `0.0`–`25.0` | Ângulo horizontal máximo da área morta em ADS. |
| Bounds Vertical | Limite Vertical ADS (Graus) | `float` | `3.0` | `0.0`–`25.0` | Ângulo vertical máximo da área morta em ADS. |
| Return Speed | Velocidade de Retorno ADS | `float` | `10.0` | `0.1`–`20.0` | Velocidade de retorno ao centro em ADS. |
