# ORBIT — Propriedades de Configuração (F12 / BepInEx ConfigurationManager)

> **Plugin:** `com.chazut.orbit` — ORBIT v1.2.1  
> **Fontes:** [original/Orbit/Plugin.cs](original/Orbit/Plugin.cs) e [original/Orbit/Looting/LootConfig.cs](original/Orbit/Looting/LootConfig.cs)  
> **Nota:** As configurações marcadas com **(Avançado)** ficam ocultas por padrão no menu F12 e só são exibidas quando a opção **"Advanced settings"** estiver ativada no ConfigurationManager do BepInEx.

---

## 01. Essentials

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Enable looting` | Habilitar Coleta (Looting) | `LootingFaction` | `Pmc, Scav, PlayerScav` | Flags: `None, Pmc, Scav, PlayerScav, Raider, Rogue, Boss, Follower, All` | Quais facções saqueiam contêineres, itens soltos e corpos. Apenas as facções listadas são afetadas. | Não |
| `Squad rally` | Reagrupamento de Esquadrão | `bool` | `true` | - | LIGADO (padrão): quando um colega de esquadrão sofre disparos, os outros se desvencilham e convergem para dar suporte (o ORBIT os roteia, o SAIN combate). DESLIGADO: cada um luta sua própria batalha. | Não |
| `Emergency extract when wounded` | Extração de Emergência ao ser Ferido | `bool` | `true` | - | LIGADO (padrão): um PMC / PlayerScav gravemente ferido e sem suprimentos médicos utilizáveis se separa e extrai sozinho em vez de morrer onde está, reagrupando-se ao esquadrão caso se cure. DESLIGADO: os membros só saem pelos gatilhos de saque/tempo/esquadrão. | Não |
| `Player convergence` | Convergência com o Jogador | `bool` | `false` | - | LIGADO: o mundo suavemente se desloca em direção ao(s) jogador(es) humano(s), trazendo mais ação até você. DESLIGADO (padrão): sem atração. (Força e alcance ficam em Advection nas configurações Avançadas). | Não |
| `Degraded tickrate for off-screen squads` | Taxa de Decisão Reduzida para Esquadrões Fora de Tela | `bool` | `false` | - | LIGADO: esquadrões distantes de qualquer jogador tomam decisões com muito menos frequência para economizar CPU. Movimentação/mira/portas continuam rodando, apenas adiam NOVAS decisões. NOTA: o ganho é modesto — o ORBIT é uma fração leve do custo de um bot (combate, visão e caminhos do SAIN/EFT dominam), portanto deixar DESLIGADO (padrão) é adequado para a maioria dos setups. | Não |
| `Quiet logging` | Registro Silencioso (Quiet Logging) | `bool` | `true` | - | LIGADO (padrão): log limpo — apenas avisos e erros, independentemente dos níveis de log abaixo. DESLIGUE para usar os níveis de log (ex.: marcar Debug antes de enviar um relatório de bug). | Não |
| `Log levels` | Níveis de Registro (Log Levels) | `OrbitLogLevel` | `Info, Warning, Error` | Flags: `Debug, Info, Warning, Error` | Quais níveis de mensagem o ORBIT grava (usado quando Quiet logging está DESLIGADO). Padrão: tudo exceto Debug. Marque Debug para um relatório detalhado de bugs — agora funciona na versão Release, não apenas em builds de debug. | Não |
| `Performance logging` | Registro de Desempenho | `bool` | `false` | - | LIGADO: grava uma linha de resumo 'PERF' (fps, engasgos, GC, contadores de atividade do ORBIT) no log a cada 30s, independentemente das outras configurações de log. Ative antes de gravar uma raid para gerar um relatório de desempenho. | **Sim** |

---

## 02. Factions

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Vanilla scavs (RESTART)` | Scavs Vanilla / Desativar ORBIT em Scavs | `bool` | `false` | - | LIGADO: scavs bots usam a inteligência vanilla da BSG em vez do ORBIT (fazendo com que 'Roaming Scavs' não tenha efeito). DESLIGADO (padrão): ORBIT os controla. PlayerScavs sempre permanecem no ORBIT. | Não |
| `Vanilla goons (RESTART)` | Goons Vanilla / Desativar ORBIT em Goons | `bool` | `false` | - | LIGADO: Goons (Knight, Big Pipe, Bird Eye) usam a inteligência vanilla da BSG. DESLIGADO (padrão): ORBIT os controla. | Não |
| `Vanilla cultists (RESTART)` | Cultistas Vanilla / Desativar ORBIT em Cultistas | `bool` | `false` | - | LIGADO: Cultistas usam a inteligência vanilla da BSG. DESLIGADO (padrão): ORBIT os controla. | Não |
| `Vanilla raiders (RESTART)` | Raiders Vanilla / Desativar ORBIT em Raiders | `bool` | `true` | - | LIGADO (padrão): Raiders (Reserve / Labs) e Rogues (Lighthouse) usam a inteligência vanilla da BSG. DESLIGADO: ORBIT os controla. | Não |
| `Vanilla bloodhounds (RESTART)` | Bloodhounds Vanilla / Desativar ORBIT em Bloodhounds | `bool` | `false` | - | LIGADO: Bloodhounds (Smugglers / arena spawns) usam a inteligência vanilla da BSG. DESLIGADO (padrão): ORBIT os controla. | Não |
| `Roaming Scavs` | Scavs Vagueando Livremente | `bool` | `false` | - | DESLIGADO (padrão): scavs ficam perto de suas áreas de spawn. LIGADO: vagueiam pelo mapa inteiro como PMCs. | Não |
| `Roaming Goons` | Goons Vagueando Livremente | `bool` | `true` | - | LIGADO (padrão): Goons vagueiam livremente por todo o mapa. DESLIGADO: ORBIT os puxa gradualmente em direção à área de spawn sempre que não estiverem em combate — mas ainda cobrem muito terreno, pois escutam e enxergam de muito longe. | Não |
| `Roaming Bloodhounds` | Bloodhounds Vagueando Livremente | `bool` | `true` | - | DESLIGADO: Bloodhounds ficam perto do spawn. LIGADO (padrão): vagueiam pelo mapa inteiro. | Não |
| `Take over UNTAR bots` | Assumir Controle de Bots UNTAR | `bool` | `false` | - | LIGADO: ORBIT roteia bots da UNTAR como PMCs. DESLIGADO (padrão): rodam em seu comportamento próprio 'Go Home'. | **Sim** |
| `Take over RUAF bots` | Assumir Controle de Bots RUAF | `bool` | `false` | - | LIGADO: ORBIT roteia bots da RUAF como PMCs. DESLIGADO (padrão): rodam em seu comportamento próprio 'Come Home'. | **Sim** |
| `Take over Black Division bots` | Assumir Controle de Bots Black Division | `bool` | `false` | - | LIGADO: ORBIT roteia bots da Black Division como PMCs. DESLIGADO (padrão): rodam em seu comportamento próprio. | **Sim** |
| `Take over ISB bots` | Assumir Controle de Bots ISB | `bool` | `true` | - | LIGADO (padrão): ORBIT roteia bots da ISB como PMCs. DESLIGADO: rodam em seu comportamento próprio. | **Sim** |
| `Take over Combine Soldiers bots` | Assumir Controle de Bots Combine Soldiers | `bool` | `false` | - | LIGADO: ORBIT roteia os Combine Soldiers do Manimal como PMCs. DESLIGADO (padrão): rodam em seu comportamento próprio. | **Sim** |

---

## 03. PlayerScav

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Enabled (goal system)` | Habilitado (Sistema de Metas) | `bool` | `true` | - | DESLIGADO: esquadrões de PlayerScav ignoram o sistema de metas Quest / Kills / LootValue. | Não |
| `Main count min` | Mínimo de Objetivos Principais | `int` | `1` | `1..20` | Quantidade mínima de objetivos sorteados para um esquadrão de PlayerScav. | Não |
| `Main count max` | Máximo de Objetivos Principais | `int` | `5` | `1..20` | Quantidade máxima de objetivos sorteados para um esquadrão de PlayerScav. | Não |
| `Main mix — Quest %` | Proporção de Missão (Quest %) | `float` | `0.10` (10%) | `0..1` (0%..100%) | Parcela de objetivos de Missão (Quest) no mix de PlayerScav (normalizada com Kills + LootValue). | Não |
| `Main mix — Kills %` | Proporção de Eliminações (Kills %) | `float` | `0.30` (30%) | `0..1` (0%..100%) | Parcela de objetivos de Eliminações (Kills) no mix de PlayerScav. | Não |
| `Main mix — LootValue %` | Proporção de Valor de Loot (LootValue %) | `float` | `0.60` (60%) | `0..1` (0%..100%) | Parcela de objetivos de Valor de Loot (LootValue) no mix de PlayerScav. | Não |
| `Time extract window (%)` | Janela de Tempo para Extração (%) | `Vector2` | `(10, 30)` | - | Janela aleatória (como % do tempo restante de raid) em que um esquadrão de PlayerScav decide extrair. | Não |
| `Extract at loot value (₽)` | Extrair por Valor de Loot (₽) | `float` | `200000` | - | Quando um esquadrão de PlayerScav atinge este valor coletado (₽ entre membros vivos), ele se dirige ao ponto de extração mais próximo. 0 = nunca. | Não |

---

## 04. Looting

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Detect loot distance (m)` | Distância de Detecção de Loot (m) | `float` | `80` | - | Distância máxima do líder do esquadrão para que um local de saque (contêiner / item / corpo) seja considerado. 0 = sem limite. | Não |
| `Corpse requires LoS or squad kill` | Corpo Requer Linha de Visão ou Abate pelo Esquadrão | `bool` | `true` | - | LIGADO (padrão): um corpo só é saqueado se o esquadrão o avistou ou foi responsável pelo abate, evitando que bots saibam magicamente de corpos através do mapa. | Não |
| `Extract allowed for` | Extração Permitida Para | `ExtractFaction` | `Pmc, PlayerScav` | Flags: `None, Pmc, PlayerScav, All` | Quais facções o ORBIT guia para extração. Controla TODAS as extrações — gatilhos de saque/tempo do esquadrão e extrações solo de emergência (feridos) ou por valor de saque. None = ninguém extrai. Apenas PMC e PlayerScav possuem lógica de extração. | Não |
| `Solo extract on own loot threshold (%)` | Chance de Extração Solo por Limite Próprio de Loot (%) | `int` | `50` | `0..100` | Quando o saque PRÓPRIO de um PMC/PlayerScav ultrapassa seu limite individual, é a chance de sair SOZINHO enquanto o resto do esquadrão continua (sorteado uma vez por membro). 0 = permanece com o esquadrão, 100 = sai sempre. | Não |
| `Scav: per-item loot chance (%)` | Scav: Chance de Coleta por Item (%) | `int` | `30` | `0..100` | Scavs bots (não PlayerScavs) ignoram o filtro de valor e rolam esta chance por item, como scavs oportunistas vanilla. PMCs/PlayerScavs não são afetados. | Não |

---

## 05. Performance

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Full-rate distance (m)` | Distância em Taxa Total (m) | `float` | `200` | `0..1000` | Esquadrões dentro desta distância de qualquer jogador sempre rodam em taxa máxima. Além dessa distância, o limitador de intervalo entra em ação. | **Sim** |
| `Far decision interval (s)` | Intervalo de Decisão Distante (s) | `float` | `6` | `0.5..30` | Com que frequência um esquadrão distante/fora de visão reavalia decisões. Maior = mais economia de CPU, reações mais lentas à distância. 5–10s é uma boa faixa. | **Sim** |

---

## 06. POI guard (RESTART)

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Base guard duration (s, min..max)` | Duração Base de Guarda (s, mín..máx) | `Vector2` | `(60, 180)` | - | Quanto tempo um esquadrão mantém uma posição de missão/cobertura antes de escolher um novo objetivo. Maior = mapa mais estático. | **Sim** |
| `Synthetic POI guard duration (s, min..max)` | Duração de Guarda em POI Sintético (s, mín..máx) | `Vector2` | `(3.5, 6.5)` | - | Igual ao anterior, mas para pontos virtuais de patrulha (sem loot/missão real). Mantido curto para manter bots em movimento constante. | **Sim** |
| `Loot/Quest guard duration cut (×, min..max)` | Redução da Guarda em Loot/Missão (×, mín..máx) | `Vector2` | `(0.2, 0.5)` | - | Uma vez que todo o esquadrão chegou ao ponto de loot/missão, o tempo de guarda é reduzido a esta fração (ex.: 0.2–0.5 = 20–50% da base). | **Sim** |

---

## 07. Advection & convergence

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Zone radius scale` | Escala de Raio de Zonas | `float` | `1.0` | `0..10` | Multiplicador no raio das zonas de advecção por mapa. 1.0 = padrões do autor. | **Sim** |
| `Zone force scale` | Escala de Força de Zonas | `float` | `1.0` | `-10..10` | Multiplicador na força de advecção. Valores negativos invertem atratores↔repulsores; 0 desativa a advecção. | **Sim** |
| `Zone falloff scale` | Escala de Decaimento de Zonas | `float` | `1.0` | `0..5` | Quão rápido a força de uma zona se dissipa com a distância. Maior = mais concentrado próximo à zona. | **Sim** |
| `Convergence radius scale` | Escala do Raio de Convergência | `float` | `1.0` | `0..10` | Multiplicador de quão longe o deslocamento em direção aos jogadores alcança. 1.0 = padrão do autor. | **Sim** |
| `Convergence force scale` | Escala da Força de Convergência | `float` | `1.0` | `-10..10` | Multiplicador de quão fortemente os bots são atraídos em direção aos jogadores. Negativo os EMPURRA PARA LONGE; 0 desativa. | **Sim** |

---

## 08. Main objectives

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Enabled` | Habilitado | `bool` | `true` | - | Interruptor mestre do sistema de metas (objetivos Quest / Kills / LootValue). DESLIGADO = despacho simples. | **Sim** |
| `Enabled for PMC` | Habilitado para PMCs | `bool` | `true` | - | DESLIGADO: esquadrões de PMC ignoram o sistema de metas. | **Sim** |
| `Extract when all mains done` | Extrair ao Concluir Todos os Objetivos | `bool` | `true` | - | LIGADO (padrão): o esquadrão segue para extração assim que todos os seus objetivos forem concluídos. | **Sim** |
| `Main pull strength` | Força de Atração do Objetivo Principal | `float` | `4.0` | `0..20` | Quão fortemente os esquadrões se comprometem com seus objetivos. Maior = mais focado na meta. | **Sim** |
| `Kills roam pull strength` | Força de Atração na Área de Eliminações | `float` | `3.0` | `0..20` | Atração em direção a um objetivo de Kills enquanto caça na região. | **Sim** |
| `LootValue timeout (s)` | Limite de Tempo para Valor de Loot (s) | `float` | `300` | `30..1800` | Margem de segurança: um objetivo de LootValue é finalizado automaticamente após esta quantidade de segundos engajado. | **Sim** |
| `Combat caller grace (s)` | Tolerância de Chamada de Combate (s) | `float` | `5` | `0..60` | Quanto tempo o impulso de reagrupamento para combate permanece ativo após os últimos disparos. | **Sim** |
| `Roam splinter radius (m)` | Raio de Dispersão ao Vaguear (m) | `float` | `50` | `10..200` | Quão longe cada membro se dispersa para escolher seu próprio ponto ao patrulhar um objetivo de Kills / LootValue. | **Sim** |
| `Same-floor sweep tolerance (m)` | Tolerância de Varredura no Mesmo Andar (m) | `float` | `2.5` | `0..10` | Loot dentro desta distância vertical conta como 'mesmo andar' e é priorizado sobre itens em outros andares (evita efeito io-iô em escadas). 0 = ignora andares. | **Sim** |
| `Cross-floor splinter chance` | Chance de Dispersão Entre Andares | `float` | `0.10` (10%) | `0..1` (0%..100%) | Chance de um bot escolher loot em outro andar em vez de terminar o atual, permitindo transições naturais entre pisos. 0% = limpa andar antes de mudar; 100% = ignora andares. | **Sim** |
| `Time extract window — PMC (%)` | Janela de Tempo para Extração — PMC (%) | `Vector2` | `(10, 30)` | - | Janela aleatória (em % do tempo restante de raid) em que um esquadrão PMC decide sair. Sorteado uma vez por esquadrão. | **Sim** |
| `PMC loot cell cooldown (s)` | Cooldown de Célula de Loot PMC (s) | `float` | `600` | `0..3600` | Após saquear uma célula, quanto tempo antes do mesmo esquadrão PMC retornar a ela. Evita efeito bumerangue. 0 = sem cooldown. | **Sim** |
| `Synthetic POI cooldown (s)` | Cooldown de POI Sintético (s) | `float` | `180` | `0..1800` | Após finalizar um ponto de patrulha, quanto tempo antes de revisitar. 0 = sem cooldown. | **Sim** |
| `Opportunistic corpse scan (s)` | Varredura Oportunista de Corpos (s) | `float` | `2.5` | `0.5..10` | Frequência com que o esquadrão busca corpos recentes nas proximidades. Menor = saque mais ágil, porém consome mais CPU com muitos bots. | **Sim** |

---

## 09.0 SAIN personality - General

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|---|
| `Enable SAIN personality` | Habilitar Personalidade SAIN | `bool` | `true` | - | LIGADO (padrão): esquadrões de PMC usam os valores por arquétipo das seções 09.1–09.5 (correspondentes ao cérebro SAIN). Scavs e PlayerScavs sempre usam as configurações padrão/fallback. | Não |
| `Brain names → Timmy (RESTART)` | Nomes de Cérebro → Timmy (REINÍCIO) | `string` | `"Timmy"` | - | Nomes de cérebro SAIN (separados por vírgula, sem distinção de maiúsculas) tratados como Timmy (aleatório/errático). | **Sim** |
| `Brain names → Cautious (RESTART)` | Nomes de Cérebro → Cautious (REINÍCIO) | `string` | `"Rat, Coward, SnappingTurtle"` | - | Nomes de cérebro SAIN tratados como Cauteloso (baixo risco, foco em loot). | **Sim** |
| `Brain names → Average (RESTART)` | Nomes de Cérebro → Average (REINÍCIO) | `string` | `"Normal"` | - | Nomes de cérebro SAIN tratados como Médio (balanceado). Cérebros não listados nas 5 categorias usam Average como padrão. | **Sim** |
| `Brain names → Aggressive (RESTART)` | Nomes de Cérebro → Aggressive (REINÍCIO) | `string` | `"Wreckless, Chad"` | - | Nomes de cérebro SAIN tratados como Agressivo. | **Sim** |
| `Brain names → Very aggressive (RESTART)` | Nomes de Cérebro → Very Aggressive (REINÍCIO) | `string` | `"GigaChad"` | - | Nomes de cérebro SAIN tratados como Muito Agressivo. | **Sim** |
| `Timmy: erratic extras` | Timmy: Comportamentos Erráticos Extras | `bool` | `true` | - | LIGADO: esquadrões Timmy têm 20% de chance de escolher a célula errada e 5% de chance de ignorar a blacklist de células além dos valores de 09.1. | **Sim** |

---

## 09.1 SAIN personality - Timmy

> Todos os itens desta seção são **(Avançado)**.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Main mix — Quest %` | Proporção de Missão % | `float` | `0.29` (29%) | `0..1` (0%..100%) | Parcela de missões principais para este arquétipo (auto-normalizada com Kills + LootValue). |
| `Main mix — Kills %` | Proporção de Eliminações % | `float` | `0.29` (29%) | `0..1` (0%..100%) | Parcela de objetivos de eliminações para este arquétipo. |
| `Main mix — LootValue %` | Proporção de Valor de Loot % | `float` | `0.42` (42%) | `0..1` (0%..100%) | Parcela de objetivos de valor de loot para este arquétipo. |
| `Main count (min, max)` | Contagem de Objetivos (Mín, Máx) | `Vector2` | `(1, 2)` | - | Quantidade de objetivos principais sorteados por esquadrão [mín..máx]. |
| `Extract loot threshold (₽, min..max)` | Limite de Loot para Extração (₽, mín..máx) | `Vector2` | `(100000, 300000)` | - | Valor em rublos que ativa o pedido de extração. Sorteado uma vez por esquadrão. |
| `Loot coverage % (min..max)` | Cobertura de Coleta % (mín..máx) | `Vector2` | `(0.30, 0.50)` | - | Probabilidade de coleta por POI (célula LootValue + varredura). 1.0 = aspira tudo, 0.3 = pula a maioria. |
| `Sprint propensity (0..1)` | Propensão a Correr / Sprint (0..1) | `float` | `0.0` | - | Propensão ao sprint: 0 = nunca corre, 1 = sempre corre. |
| `Locked door unlock %` | Chance de Destrancar Portas Fechadas % | `float` | `0.10` (10%) | - | Probabilidade de forçar a abertura de uma porta trancada em POIs intermediários. POIs de âncora principal sempre rolam 100%. |
| `Mini-loot threshold (₽)` | Limite Mínimo de Item de Loot (₽) | `int` | `0` | - | Valor mínimo em rublos de um item para o bot se dar ao trabalho de pegar. |
| `Scavenge sweep radius (m)` | Raio de Varredura de Loot (m) | `float` | `10` | - | Após saquear, raio para encadear a coleta no contêiner/corpo/item solto mais próximo. |
| `Follower splinter radius (m)` | Raio de Dispersão dos Membros (m) | `float` | `30` | - | Membros não-líderes se espalham para POIs secundários dentro deste raio do líder. |
| `Kills roam duration (s, min..max)` | Duração de Patrulha de Kills (s, mín..máx) | `Vector2` | `(30, 150)` | - | Tempo em segundos que o esquadrão passa patrulhando a âncora de Kills antes de concluir. |
| `Top loot cells max` | Máximo de Melhores Células de Loot | `int` | `10` | - | Sorteios de LootValue são restritos às N células mais ricas do mapa. Menor = mais concentração e combate. |

---

## 09.2 SAIN personality - Cautious

> Todos os itens desta seção são **(Avançado)**.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Main mix — Quest %` | Proporção de Missão % | `float` | `0.23` (23%) | `0..1` (0%..100%) | Parcela de missões principais para este arquétipo (auto-normalizada com Kills + LootValue). |
| `Main mix — Kills %` | Proporção de Eliminações % | `float` | `0.06` (6%) | `0..1` (0%..100%) | Parcela de objetivos de eliminações para este arquétipo. |
| `Main mix — LootValue %` | Proporção de Valor de Loot % | `float` | `0.71` (71%) | `0..1` (0%..100%) | Parcela de objetivos de valor de loot para este arquétipo. |
| `Main count (min, max)` | Contagem de Objetivos (Mín, Máx) | `Vector2` | `(2, 4)` | - | Quantidade de objetivos principais sorteados por esquadrão [mín..máx]. |
| `Extract loot threshold (₽, min..max)` | Limite de Loot para Extração (₽, mín..máx) | `Vector2` | `(200000, 500000)` | - | Valor em rublos que ativa o pedido de extração. Sorteado uma vez por esquadrão. |
| `Loot coverage % (min..max)` | Cobertura de Coleta % (mín..máx) | `Vector2` | `(0.85, 0.95)` | - | Probabilidade de coleta por POI (célula LootValue + varredura). 1.0 = aspira tudo, 0.3 = pula a maioria. |
| `Sprint propensity (0..1)` | Propensão a Correr / Sprint (0..1) | `float` | `0.2` | - | Propensão ao sprint: 0 = nunca corre, 1 = sempre corre. |
| `Locked door unlock %` | Chance de Destrancar Portas Fechadas % | `float` | `0.10` (10%) | - | Probabilidade de forçar a abertura de uma porta trancada em POIs intermediários. POIs de âncora principal sempre rolam 100%. |
| `Mini-loot threshold (₽)` | Limite Mínimo de Item de Loot (₽) | `int` | `5000` | - | Valor mínimo em rublos de um item para o bot se dar ao trabalho de pegar. |
| `Scavenge sweep radius (m)` | Raio de Varredura de Loot (m) | `float` | `15` | - | Após saquear, raio para encadear a coleta no contêiner/corpo/item solto mais próximo. |
| `Follower splinter radius (m)` | Raio de Dispersão dos Membros (m) | `float` | `18` | - | Membros não-líderes se espalham para POIs secundários dentro deste raio do líder. |
| `Kills roam duration (s, min..max)` | Duração de Patrulha de Kills (s, mín..máx) | `Vector2` | `(30, 150)` | - | Tempo em segundos que o esquadrão passa patrulhando a âncora de Kills antes de concluir. |
| `Top loot cells max` | Máximo de Melhores Células de Loot | `int` | `10` | - | Sorteios de LootValue são restritos às N células mais ricas do mapa. Menor = mais concentração e combate. |

---

## 09.3 SAIN personality - Average

> Todos os itens desta seção são **(Avançado)**.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Main mix — Quest %` | Proporção de Missão % | `float` | `0.34` (34%) | `0..1` (0%..100%) | Parcela de missões principais para este arquétipo (auto-normalizada com Kills + LootValue). |
| `Main mix — Kills %` | Proporção de Eliminações % | `float` | `0.33` (33%) | `0..1` (0%..100%) | Parcela de objetivos de eliminações para este arquétipo. |
| `Main mix — LootValue %` | Proporção de Valor de Loot % | `float` | `0.33` (33%) | `0..1` (0%..100%) | Parcela de objetivos de valor de loot para este arquétipo. |
| `Main count (min, max)` | Contagem de Objetivos (Mín, Máx) | `Vector2` | `(1, 5)` | - | Quantidade de objetivos principais sorteados por esquadrão [mín..máx]. |
| `Extract loot threshold (₽, min..max)` | Limite de Loot para Extração (₽, mín..máx) | `Vector2` | `(500000, 1000000)` | - | Valor em rublos que ativa o pedido de extração. Sorteado uma vez por esquadrão. |
| `Loot coverage % (min..max)` | Cobertura de Coleta % (mín..máx) | `Vector2` | `(0.65, 0.75)` | - | Probabilidade de coleta por POI (célula LootValue + varredura). 1.0 = aspira tudo, 0.3 = pula a maioria. |
| `Sprint propensity (0..1)` | Propensão a Correr / Sprint (0..1) | `float` | `0.5` | - | Propensão ao sprint: 0 = nunca corre, 1 = sempre corre. |
| `Locked door unlock %` | Chance de Destrancar Portas Fechadas % | `float` | `0.30` (30%) | - | Probabilidade de forçar a abertura de uma porta trancada em POIs intermediários. POIs de âncora principal sempre rolam 100%. |
| `Mini-loot threshold (₽)` | Limite Mínimo de Item de Loot (₽) | `int` | `10000` | - | Valor mínimo em rublos de um item para o bot se dar ao trabalho de pegar. |
| `Scavenge sweep radius (m)` | Raio de Varredura de Loot (m) | `float` | `10` | - | Após saquear, raio para encadear a coleta no contêiner/corpo/item solto mais próximo. |
| `Follower splinter radius (m)` | Raio de Dispersão dos Membros (m) | `float` | `30` | - | Membros não-líderes se espalham para POIs secundários dentro deste raio do líder. |
| `Kills roam duration (s, min..max)` | Duração de Patrulha de Kills (s, mín..máx) | `Vector2` | `(60, 300)` | - | Tempo em segundos que o esquadrão passa patrulhando a âncora de Kills antes de concluir. |
| `Top loot cells max` | Máximo de Melhores Células de Loot | `int` | `10` | - | Sorteios de LootValue são restritos às N células mais ricas do mapa. Menor = mais concentração e combate. |

---

## 09.4 SAIN personality - Aggressive

> Todos os itens desta seção são **(Avançado)**.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Main mix — Quest %` | Proporção de Missão % | `float` | `0.18` (18%) | `0..1` (0%..100%) | Parcela de missões principais para este arquétipo (auto-normalizada com Kills + LootValue). |
| `Main mix — Kills %` | Proporção de Eliminações % | `float` | `0.64` (64%) | `0..1` (0%..100%) | Parcela de objetivos de eliminações para este arquétipo. |
| `Main mix — LootValue %` | Proporção de Valor de Loot % | `float` | `0.18` (18%) | `0..1` (0%..100%) | Parcela de objetivos de valor de loot para este arquétipo. |
| `Main count (min, max)` | Contagem de Objetivos (Mín, Máx) | `Vector2` | `(2, 4)` | - | Quantidade de objetivos principais sorteados por esquadrão [mín..máx]. |
| `Extract loot threshold (₽, min..max)` | Limite de Loot para Extração (₽, mín..máx) | `Vector2` | `(1000000, 1500000)` | - | Valor em rublos que ativa o pedido de extração. Sorteado uma vez por esquadrão. |
| `Loot coverage % (min..max)` | Cobertura de Coleta % (mín..máx) | `Vector2` | `(0.50, 0.60)` | - | Probabilidade de coleta por POI (célula LootValue + varredura). 1.0 = aspira tudo, 0.3 = pula a maioria. |
| `Sprint propensity (0..1)` | Propensão a Correr / Sprint (0..1) | `float` | `0.8` | - | Propensão ao sprint: 0 = nunca corre, 1 = sempre corre. |
| `Locked door unlock %` | Chance de Destrancar Portas Fechadas % | `float` | `0.45` (45%) | - | Probabilidade de forçar a abertura de uma porta trancada em POIs intermediários. POIs de âncora principal sempre rolam 100%. |
| `Mini-loot threshold (₽)` | Limite Mínimo de Item de Loot (₽) | `int` | `15000` | - | Valor mínimo em rublos de um item para o bot se dar ao trabalho de pegar. |
| `Scavenge sweep radius (m)` | Raio de Varredura de Loot (m) | `float` | `8` | - | Após saquear, raio para encadear a coleta no contêiner/corpo/item solto mais próximo. |
| `Follower splinter radius (m)` | Raio de Dispersão dos Membros (m) | `float` | `39` | - | Membros não-líderes se espalham para POIs secundários dentro deste raio do líder. |
| `Kills roam duration (s, min..max)` | Duração de Patrulha de Kills (s, mín..máx) | `Vector2` | `(90, 450)` | - | Tempo em segundos que o esquadrão passa patrulhando a âncora de Kills antes de concluir. |
| `Top loot cells max` | Máximo de Melhores Células de Loot | `int` | `5` | - | Sorteios de LootValue são restritos às N células mais ricas do mapa. Menor = mais concentração e combate. |

---

## 09.5 SAIN personality - Very aggressive

> Todos os itens desta seção são **(Avançado)**.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Main mix — Quest %` | Proporção de Missão % | `float` | `0.06` (6%) | `0..1` (0%..100%) | Parcela de missões principais para este arquétipo (auto-normalizada com Kills + LootValue). |
| `Main mix — Kills %` | Proporção de Eliminações % | `float` | `0.83` (83%) | `0..1` (0%..100%) | Parcela de objetivos de eliminações para este arquétipo. |
| `Main mix — LootValue %` | Proporção de Valor de Loot % | `float` | `0.11` (11%) | `0..1` (0%..100%) | Parcela de objetivos de valor de loot para este arquétipo. |
| `Main count (min, max)` | Contagem de Objetivos (Mín, Máx) | `Vector2` | `(2, 5)` | - | Quantidade de objetivos principais sorteados por esquadrão [mín..máx]. |
| `Extract loot threshold (₽, min..max)` | Limite de Loot para Extração (₽, mín..máx) | `Vector2` | `(1500000, 3000000)` | - | Valor em rublos que ativa o pedido de extração. Sorteado uma vez por esquadrão. |
| `Loot coverage % (min..max)` | Cobertura de Coleta % (mín..máx) | `Vector2` | `(0.30, 0.45)` | - | Probabilidade de coleta por POI (célula LootValue + varredura). 1.0 = aspira tudo, 0.3 = pula a maioria. |
| `Sprint propensity (0..1)` | Propensão a Correr / Sprint (0..1) | `float` | `1.0` | - | Propensão ao sprint: 0 = nunca corre, 1 = sempre corre. |
| `Locked door unlock %` | Chance de Destrancar Portas Fechadas % | `float` | `0.60` (60%) | - | Probabilidade de forçar a abertura de uma porta trancada em POIs intermediários. POIs de âncora principal sempre rolam 100%. |
| `Mini-loot threshold (₽)` | Limite Mínimo de Item de Loot (₽) | `int` | `20000` | - | Valor mínimo em rublos de um item para o bot se dar ao trabalho de pegar. |
| `Scavenge sweep radius (m)` | Raio de Varredura de Loot (m) | `float` | `5` | - | Após saquear, raio para encadear a coleta no contêiner/corpo/item solto mais próximo. |
| `Follower splinter radius (m)` | Raio de Dispersão dos Membros (m) | `float` | `45` | - | Membros não-líderes se espalham para POIs secundários dentro deste raio do líder. |
| `Kills roam duration (s, min..max)` | Duração de Patrulha de Kills (s, mín..máx) | `Vector2` | `(150, 750)` | - | Tempo em segundos que o esquadrão passa patrulhando a âncora de Kills antes de concluir. |
| `Top loot cells max` | Máximo de Melhores Células de Loot | `int` | `3` | - | Sorteios de LootValue são restritos às N células mais ricas do mapa. Menor = mais concentração e combate. |
