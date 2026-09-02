# SAIN — Catálogo de Propriedades de Configuração (F6 / JSON Presets)

> **Plugin:** `me.sol.sain` — SAIN (Solarint's AI Modifications)  
> **Fontes:** [`mods/SAIN/modded/SAIN/Preset/GlobalSettings/`](./modded/SAIN/Preset/GlobalSettings/) e [`mods/SAIN/modded/SAIN/Preset/Personalities/`](./modded/SAIN/Preset/Personalities/)  
> **Interface:** Acessível in-game via tecla **F6** (Editor GUI) ou editável via arquivos JSON em `BepInEx/plugins/SAIN/Presets/<PresetName>/`.

---

## 01. Configurações Gerais e Otimização (`General`)

| Propriedade / Categoria | Tradução (pt-BR) | Tipo | Descrição e Impacto no Jogo |
|---|---|---|---|
| `AILimit.LimitAIvsAIGlobal` | Limitar IA vs IA Globalmente | `bool` | Reduz a frequência de atualização e distância máxima de visão entre bots distantes do jogador para economizar CPU. |
| `AILimit.MaxVisionRanges` | Distâncias Máximas de Visão por Limite | `Dictionary` | Define os limites de metros de visão para bots nos estados `Far` (150m), `VeryFar` (100m) e `Narnia` (50m). |
| `Performance.PerformanceMode` | Modo de Alta Performance | `bool` | Simplifica cálculos volumétricos de cobertura e reduz taxa de raycasts de busca. |
| `DoorSettings.DisableDoorOpening` | Desativar Abertura de Portas por Bots | `bool` | Impede que bots abram ou arrombem portas fechadas no cenário. |
| `Extract.SAIN_EXTRACT_TOGGLE` | Habilitar Extração de Bots | `bool` | Permite que PMCs e PlayerScavs naveguem para os pontos de extração e saiam da raid. |
| `Extract.MinExtractTimeFraction` | Fração Mínima de Tempo para Extrair | `float` | Percentual mínimo do tempo de raid transcorrido antes de sortear exfiltração (padrão: 10%). |
| `Extract.ExtractLootThreshold` | Limiar de Valor de Loot para Extração (₽) | `float` | Valor em rublos no inventário que motiva a fuga do bot (padrão: 200.000 ₽). |
| `VanillaBots.VanillaScavs` | Manter Scavs na IA Vanilla BSG | `bool` | Desativa o SAIN em scavs comuns, mantendo a inteligência padrão da BSG. |
| `VanillaBots.VanillaBosses` | Manter Bosses na IA Vanilla BSG | `bool` | Desativa o SAIN nos chefes de mapa (Killa, Glukhar, Reshala, etc.). |
| `VanillaBots.VanillaGoons` | Manter Goons na IA Vanilla BSG | `bool` | Desativa o SAIN nos Goons (Knight, Big Pipe, Bird Eye). |

---

## 02. Percepção Visual e Iluminação (`Look`)

| Propriedade | Tradução (pt-BR) | Tipo | Descrição e Impacto no Jogo |
|---|---|---|---|
| `VisionSpeed.VisionSpeedModifier` | Multiplicador de Velocidade de Visão | `float` | Escala geral do tempo de reação visual dos bots (menor = bots mais rápidos; maior = bots mais humanos). |
| `VisionDistance.VisionDistanceModifier` | Multiplicador de Distância de Visão | `float` | Escala do alcance máximo em metros que os bots conseguem enxergar. |
| `Time.VISION_WEATHER_MIN_COEF` | Coeficiente Mínimo de Visão em Mau Clima | `float` | Limite inferior de redução de visão causada por chuva torrencial ou neblina densa. |
| `Time.VISION_WEATHER_MIN_DIST_METERS` | Distância Mínima de Visão em Mau Clima | `float` | Garantia de visibilidade mínima mesmo na pior condição climática (padrão: 20m). |
| `LightNVG.NightVisionDistanceModifier` | Eficiência de Visão com NVG | `float` | Multiplicador de visão para bots equipados com óculos de visão noturna durante a noite. |
| `LightNVG.FlashlightGainSightModifier` | Modificador de Ganho de Visada com Lanterna | `float` | Aceleração de detecção de alvos quando o bot utiliza lanterna tática em ambientes escuros. |
| `NotLooking.NotLookingAngle` | Ângulo de Visão Periférica Não Focada | `float` | Ângulo a partir do qual alvos são tratados como visão periférica, aumentando o tempo de reação. |

---

## 03. Percepção Auditiva (`Hearing`)

| Propriedade | Tradução (pt-BR) | Tipo | Descrição e Impacto no Jogo |
|---|---|---|---|
| `Hearing.HearingDistanceModifier` | Multiplicador de Distância de Audição | `float` | Escala global do alcance com que sons são percebidos pelos bots. |
| `Hearing.GunshotDistanceModifier` | Multiplicador de Alcance de Tiros | `float` | Ajusta a distância em que disparos não silenciados alertam os bots. |
| `Hearing.FootstepDistanceModifier` | Multiplicador de Alcance de Passos | `float` | Ajusta a distância em que passos e corridas são escutados em piso comum. |
| `Hearing.SuppressedShotDistanceModifier` | Multiplicador de Tiros Silenciados | `float` | Alcance efetivo de detecção de disparos com silenciador (supressor). |
| `Hearing.HearingDispersionModifier` | Multiplicador de Dispersão Acústica | `float` | Quantidade de imprecisão/erro angular na localização de sons pelos bots. |
| `Hearing.MaxUnderFireDistance` | Distância Máxima de Projétil para Sob Fogo | `float` | Distância máxima de passagem de projétil (*fly-by*) para acionar reação de *Under Fire* (padrão: 10m). |

---

## 04. Mecânicas de Mira e Disparo (`Aim` & `Shoot`)

| Propriedade | Tradução (pt-BR) | Tipo | Descrição e Impacto no Jogo |
|---|---|---|---|
| `Aim.GlobalAimModifier` | Modificador Global de Precisão de Mira | `float` | Ajusta a precisão de tiro dos bots (maior = mais dispersão/erros; menor = tiros mais certeiros). |
| `Aim.AimDownSightsTimeModifier` | Multiplicador de Tempo de Mirada (ADS) | `float` | Tempo necessário para o bot elevar a arma até a visada de mira/luneta. |
| `Shoot.RecoilMultiplier` | Multiplicador de Recoil dos Bots | `float` | Intensidade do recuo vertical/horizontal aplicado aos disparos dos bots. |
| `Shoot.SemiAutoFirerateModifier` | Cadência em Semiautomático | `float` | Intervalo de tempo entre disparos únicos com pistolas, rifles de precisão e DMRs. |
| `Shoot.BurstFirerateModifier` | Cadência de Rajadas Curtas | `float` | Duração e espaçamento entre rajadas de fuzis de assalto a média distância. |
| `Shoot.FullAutoDistanceThreshold` | Distância Máxima para Automático | `float` | Distância máxima (em metros) em que os bots optam por disparar em modo automático contínuo. |

---

## 05. Cobertura e Navegação (`Cover` & `Steering`)

| Propriedade | Tradução (pt-BR) | Tipo | Descrição e Impacto no Jogo |
|---|---|---|---|
| `Cover.CoverMinHeight` | Altura Mínima de Cobertura | `float` | Altura mínima do colisor para que seja classificado como ponto de cobertura válido (padrão: 0.5m). |
| `Cover.DebugCoverFinder` | Gizmos Visuais de Cobertura | `bool` | Renderiza linhas e esferas coloridas no jogo exibindo todos os `CoverPoints` ativos. |
| `Steering.SteerSpeed` | Velocidade de Rotação de Mira/Olhar | `float` | Velocidade angular máxima com que o bot gira o tronco e a cabeça em resposta a ameaças. |
| `Move.SprintToCoverThreshold` | Distância Mínima para Correr até Cobertura | `float` | Distância de abrigo a partir da qual o bot ativa sprint em vez de andar (padrão: 4m). |

---

## 06. Configurações de Personalidade (`Personalities`)

Cada arquétipo (`GigaChad`, `Chad`, `Rat`, `Timmy`, `Coward`, `SnappingTurtle`, `Wreckless`, `Normal`) possui sua própria subseção:

| Propriedade de Personalidade | Tradução (pt-BR) | Descrição |
|---|---|---|
| `Assignment.CanBeRandomlyAssigned` | Sorteio Aleatório | Permite que a personalidade seja atribuída aleatoriamente a bots comuns. |
| `Assignment.PowerLevelMin / Max` | Faixa de Power Level | Pontuação de equipamento exigida para que o bot possa receber este perfil. |
| `Behavior.General.KickOpenAllDoors` | Chutar Todas as Portas | Se verdadeiro, o bot sempre arromba portas com chute em vez de abrir com a maçaneta. |
| `Behavior.General.AggressionMultiplier` | Multiplicador de Agressividade | Influencia a preferência por avançar (*push*) contra recuar para cobertura. |
| `Behavior.Talk.TauntChance` | Chance de Provocação Vocal | Probabilidade percentual de gritar frases provocativas durante o combate. |
| `Behavior.Search.WillChaseDistantGunshots` | Perseguir Tiros Distantes | Se verdadeiro, o bot se desloca até a origem de disparos escutados longe no mapa. |
| `Behavior.Search.SprintWhileSearchChance` | Chance de Correr na Busca | Probabilidade de utilizar sprint durante a varredura e checagem de cantos. |
