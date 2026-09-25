# Propriedades de Configuração — TRL-CoreSight

> Configurações expostas no menu F12 do BepInEx (`ConfigurationManager`).
> Organizadas por ordem de relevância e impacto no desempenho.

**Plugin:** `com.trl.coresight`  
**Versão:** `0.4.18`  
**Arquivo de Origem:** [`modded/Configuration/ModConfig.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-CoreSight/modded/Configuration/ModConfig.cs)

---

### 0. Afinidade de CPU & Threads (Hardware Topology)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `CpuAffinityMode` | Modo de Afinidade de CPU | `enum` | `PhysicalCoresOnly` | `PhysicalCoresOnly, Auto, PerformanceCores, PrimaryCluster, Disabled` | Define a estratégia: PhysicalCoresOnly (recomendado: desativa SMT mantendo todos os núcleos físicos), Auto (detecta P-Cores na Intel ou aplica núcleos físicos), PerformanceCores (apenas P-Cores na Intel híbrida), PrimaryCluster (modo avançado: fixa apenas a Main Thread no cluster L3 primário, sem restringir o processo inteiro — reduz o total de núcleos disponíveis pras demais threads do jogo; testar com o benchmark de frametime antes de manter ativado) ou Disabled. | Não |
| `ProcessPriorityHigh` | Prioridade de Processo Alta | `bool` | `true` | - | Eleva a prioridade do processo do jogo no Windows para Alta (High), reduzindo interrupções de outros programas em segundo plano. | Não |
| `EnableFrametimeBenchmark` | Gravador de Benchmark de Frametime | `bool` | `false` | - | Grava um arquivo CSV (`benchmark_frametimes.csv`) ao fim da raid com frametime médio e 1% low, associados ao modo de afinidade usado, para comparar o impacto real de cada configuração. | Não |

---

### 1. Geral (General)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `ModEnabled` | Ativar CoreSight | `bool` | `true` | - | Ativa ou desativa todas as funcionalidades de otimização do CoreSight. | Não |
| `DebugMode` | Modo de Diagnóstico | `bool` | `false` | - | Exibe métricas de FPS, contagem de bots ocluídos e estado de oclusão na tela/console. | Sim |

---

### 2. Otimização de Sombras (Shadows)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableInteriorShadowCulling` | Culling de Sombras em Interiores | `bool` | `true` | - | Reduz dinamicamente a distância de sombras quando o jogador está dentro de quartos ou prédios fechados. | Não |
| `InteriorShadowDistance` | Distância de Sombra Interna | `float` | `25.0` | `10.0 - 50.0` | Distância máxima (em metros) para cálculo de sombras em ambientes fechados (padrão EFT: 100-150m). | Não |
| `ExteriorShadowDistance` | Distância de Sombra Externa | `float` | `100.0` | `50.0 - 200.0` | Distância máxima de sombras restaurada ao ar livre. | Não |

---

### 3. Otimização de Bots (Bot CPU Optimization)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableBotAnimationLOD` | LOD de Animação de Bots | `bool` | `true` | - | Reduz a taxa de atualização do esqueleto de bots que estão fora de visão ou atrás de paredes sólidas. | Não |
| `BotOcclusionCheckInterval` | Intervalo de Checagem (s) | `float` | `0.2` | `0.05 - 1.0` | Intervalo em segundos entre as verificações de visibilidade dos bots (evita sobrecarga de CPU). | Sim |
| `DisableCosmeticsWhenOccluded` | Desativar Roupas Ocluídas | `bool` | `false` | - | Desativa temporariamente os renderers de equipamentos de bots a mais de 100m atrás de paredes. | Sim |

---

### 4. LOD Dinâmico (Dynamic LOD)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableDynamicLODBias` | LOD Bias Dinâmico | `bool` | `true` | - | Ajusta o lodBias automaticamente: reduz em áreas fechadas/correndo e aumenta ao mirar (ADS). | Não |
| `BaseLODBias` | LOD Bias Base | `float` | `1.0` | `0.5 - 2.0` | Valor base de qualidade geométrica do cenário durante movimentação normal. | Não |
| `AimLODBias` | LOD Bias ao Mirar (ADS) | `float` | `2.0` | `1.5 - 4.0` | Valor aumentado de qualidade geométrica acionado ao apontar a mira da arma. | Não |

---

### 5. Declutter (Limpeza de Detritos Cosméticos)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableDeclutter` | Ativar Declutter | `bool` | `true` | - | Remove detritos estáticos inúteis (papéis, latas, cacos, poças e decalques) para diminuir draw calls e aliviar a CPU. | Não |
| `DeclutterMode` | Modo de Ocultação | `enum` | `RendererOnly` | `RendererOnly, FullGameObject` | `RendererOnly` desabilita apenas renderizadores visuais (seguro). `FullGameObject` desativa o objeto por completo. | Não |
| `DeclutterGarbage` | Lixo e Papéis Soltos | `bool` | `true` | - | Remove caixas de papelão vazias, sacos plásticos, papéis e folhetos no chão. | Não |
| `DeclutterHeaps` | Montes de Entulho e Concreto | `bool` | `true` | - | Remove pequenos montes de pedras e concreto cosméticos no chão com altura <= limite. | Não |
| `DeclutterCartridges` | Estojos de Munição do Chão | `bool` | `true` | - | Remove estojos e cápsulas gastas pré-posicionadas pelo mapa. | Não |
| `DeclutterFakeFood` | Comida Estática Cenográfica | `bool` | `true` | - | Remove latas amassadas, garrafas vazias e pacotes de ração decorativos. | Não |
| `DeclutterDecals` | Decalques de Chão e Sujeira | `bool` | `true` | - | Remove sangue estático antigo de cenário, manchas de lama e grafites decorativos. | Não |
| `DeclutterPuddles` | Poças D'água Decorativas | `bool` | `true` | - | Remove poças refletivas decorativas de chão que consomem draw calls extras. | Não |
| `DeclutterShards` | Cacos de Vidro e Azulejos | `bool` | `true` | - | Remove cacos de vidro quebrados e fragmentos decorativos no chão. | Não |
| `DeclutterScaleLimit` | Altura Limite (m) | `float` | `1.5` | `0.5 - 2.5` | Altura máxima vertical para elegibilidade. Objetos maiores são mantidos para servirem de cobertura tática. | Sim |

---

### 6. Declutter por Mapa (Map Whitelist)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|:---:|
| `DeclutterFactory` | Factory | `bool` | `true` | Ativa Declutter na Factory (dia e noite). | Não |
| `DeclutterCustoms` | Customs | `bool` | `true` | Ativa Declutter na Customs (bigmap). | Não |
| `DeclutterWoods` | Woods | `bool` | `true` | Ativa Declutter em Woods. | Não |
| `DeclutterShoreline` | Shoreline | `bool` | `true` | Ativa Declutter em Shoreline. | Não |
| `DeclutterInterchange` | Interchange | `bool` | `true` | Ativa Declutter no shopping Interchange. | Não |
| `DeclutterReserve` | Reserve | `bool` | `true` | Ativa Declutter na base militar Reserve. | Não |
| `DeclutterLighthouse` | Lighthouse | `bool` | `true` | Ativa Declutter no Farol (Lighthouse). | Não |
| `DeclutterStreets` | Streets of Tarkov | `bool` | `true` | Ativa Declutter em Streets of Tarkov (maior ganho de FPS). | Não |
| `DeclutterGroundZero` | Ground Zero | `bool` | `true` | Ativa Declutter em Ground Zero (Sandbox). | Não |
| `DeclutterTheLab` | The Lab | `bool` | `false` | Ativa Declutter no Laboratório TerraGroup (desativado por padrão). | Não |
| `DeclutterTheLabyrinth` | The Labyrinth | `bool` | `false` | EXPERIMENTAL: Mapa fechado de evento com portas/armadilhas dinâmicas (TrapSyncable). Mantenha desativado para não quebrar a lógica procedural do evento. | Sim |

---

### 8. Otimização de Física (Cápsulas de Bala)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableShellCulling` | Culling de Cápsulas | `bool` | `true` | - | Suprime a criação e física de quique de cápsulas de balas ejetadas por armas médias e distantes. | Não |
| `ShellCullingDistance` | Distância Limite de Cápsulas (m) | `float` | `25.0` | `10.0 - 60.0` | Distância máxima em metros em relação à câmera para renderizar e simular o quique de cartuchos ejetados. | Não |

---

### 9. Otimização de Loot (Loose Loot Culling)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableLootCulling` | Culling de Loot Solto | `bool` | `true` | - | Oculta a renderização de pequenos itens soltos no chão a média/longa distância para economizar draw calls. | Não |
| `SmallLootDistance` | Alcance de Loot Pequeno (m) | `float` | `25.0` | `15.0 - 40.0` | Distância máxima em metros para renderizar itens pequenos de 1x1 e 1x2 (balas, parafusos, chaves). | Não |
| `MediumLootDistance` | Alcance de Loot Médio (m) | `float` | `45.0` | `25.0 - 70.0` | Distância máxima em metros para renderizar itens médios de 2x2 e 2x3 (medkits, capacetes, comida). | Não |
| `EnableADSLootBypass` | Visibilidade Total no ADS | `bool` | `true` | - | Restaura imediatamente a visibilidade de itens dentro do cone de visão ao mirar com a arma (ADS / Luneta). | Não |

---

### 10. Otimização de Iluminação (Sombras Secundárias)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableLocalLightShadowCulling` | Otimização de Sombras Locais | `bool` | `true` | - | Otimiza o cálculo de sombras de lâmpadas secundárias em corredores e interiores para aliviar a GPU. | Não |
| `ShadowOptimizationMode` | Modo de Otimização | `enum` | `DisableWeakShadowsOnly` | `DisableWeakShadowsOnly`, `DowngradeSoftToHard`, `DisableAllSecondaryShadows` | Modo de otimização de sombras (WeakShadows protege contra vazamento de luz através de paredes). | Não |
| `WeakLightIntensityThreshold` | Limiar de Intensidade Fraca | `float` | `1.2` | `0.2 - 5.0` | Intensidade máxima da luz para ser considerada fraca no modo DisableWeakShadowsOnly. | Não |
| `WeakLightRangeThreshold` | Limiar de Alcance Fraco (m) | `float` | `6.0` | `1.0 - 20.0` | Alcance máximo em metros da luz para ser considerada secundária no modo DisableWeakShadowsOnly. | Não |

---

### 11. Anti-Stutter (Garbage Collection)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableGCOptimizer` | Inibição de GC em Combate/ADS | `bool` | `true` | - | Inibe a coleta de lixo da Unity durante combates e mira (ADS) para eliminar stutters, mantendo o GC nativo fora de risco. | Não |
| `CombatGracePeriod` | Janela de Combate (s) | `float` | `5.0` | `2.0 - 15.0` | Tempo em segundos após o último disparo de arma para manter a inibição de GC ativa. | Não |
| `CriticalMemoryLimitMB` | Limite de Segurança de RAM (MB) | `float` | `3500.0` | `2000.0 - 6000.0` | Limite de segurança de memória alocada para forçar a liberação do GC nativo e evitar crash por falta de RAM (OOM). | Sim |

---

### 12. Camuflagem Natural & Visão de IA

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableNaturalConcealment` | Camuflagem Zonal Anatômica | `bool` | `true` | - | Ativa o sistema de camuflagem por 5 zonas anatômicas independentes através de capim alto e copas de árvores. | Não |
| `ConcealmentOffsetMeters` | Contorno de Segurança (m) | `float` | `0.30` | `0.15 - 0.50` | Contorno de segurança do escudo além do corpo e mochila do soldado (metros). | Não |
| `BreakProximityDistance` | Distância de Tropeço (m) | `float` | `4.0` | `2.0 - 8.0` | Distância mínima em metros para um bot detectar o jogador camuflado no capim (tropeço). | Não |
| `EnableAimOffsetNerf` | Desvio de Mira dos Bots | `bool` | `true` | - | Aplica dispersão e erro de mira aos bots que atirarem em jogadores parcialmente camuflados. | Não |
| `AimOffsetNerfIntensity` | Intensidade do Desvio | `float` | `1.0` | `0.2 - 3.0` | Intensidade do desvio de mira dos bots contra alvos encobertos por vegetação. | Sim |
| `TreeCanopyOcclusion` | Oclusão por Copas de Árvores | `bool` | `true` | - | Bloqueia tiros milagrosos de bots através das folhas e copas de árvores a longa distância. | Não |
| `EnableBotTimeSlicing` | Time-Slicing de Bots Distantes | `bool` | `true` | - | Amortiza checagens de visão de bots pacíficos a mais de 100m para aliviar o uso de CPU. | Não |

---

### 13. Otimização de Áudio (Culling de Ambiente)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `EnableAmbientAudioCulling` | Culling de Áudio Ambiente | `bool` | `true` | - | Pausa fontes de áudio ambiente contínuas inaudíveis à distância para poupar CPU de mixagem. | Não |
| `AudioCullingMargin` | Margem de Histerese (m) | `float` | `10.0` | `2.0 - 30.0` | Margem de segurança em metros além do maxDistance para pausar o áudio sem cortes sonoros. | Não |

---

### 14. Visualizador Debug (Escudos de Camuflagem)

| Propriedade | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Avançado |
|---|---|---|---|---|---|:---:|
| `ShowConcealmentVisualizer` | Visualizar Escudos (Debug) | `bool` | `false` | - | Renderiza contorno 3D translúcido em vermelho (50% transparente) nas partes do corpo onde o escudo de camuflagem estiver ativo. | Sim |
| `VisualizerShortcutKey` | Tecla do Visualizador | `shortcut` | `F9` | - | Tecla de atalho para alternar a visualização dos escudos de camuflagem vermelhos durante a raid. | Sim |

