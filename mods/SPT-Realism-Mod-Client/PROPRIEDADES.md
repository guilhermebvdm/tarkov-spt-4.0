# SPT-Realism-Mod-Client — Propriedades F12

**Plugin:** `RealismMod` — versão `1.6.3`  
**Código:** [original/Plugin.cs](original/Plugin.cs) | [original/PluginConfig.cs](original/PluginConfig.cs)  
> Itens **(Avançado)** só aparecem com "Advanced settings" ligado no F12.

## Índice

| # | Seção |
| --- | ------- |
| 0 | [Testing](#0-testing) |
| 1 | [Misc. Settings.](#1-misc-settings) |
| 2 | [Ballistics Settings.](#2-ballistics-settings) |
| 3 | [Recoil Settings.](#3-recoil-settings) |
| 4 | [Advanced Recoil Settings.](#4-advanced-recoil-settings) |
| 5 | [Stat Display Settings.](#5-stat-display-settings) |
| 6 | [Weapon Settings.](#6-weapon-settings) |
| 7 | [Health and Meds Settings.](#7-health-and-meds-settings) |
| 8 | [Hazard Zone Settings.](#8-hazard-zone-settings) |
| 9 | [Movement Settings.](#9-movement-settings) |
| 10 | [Deafening and Audio.](#10-deafening-and-audio) |
| 11 | [Weapon Speed Modifiers.](#11-weapon-speed-modifiers) |
| 12 | [Weapon Stances And Position.](#12-weapon-stances-and-position) |
| 13 | [Weapon Stances Keybinds.](#13-weapon-stances-keybinds) |
| 14 | [Active Aim.](#14-active-aim) |
| 15 | [High Ready.](#15-high-ready) |
| 16 | [Low Ready.](#16-low-ready) |
| 17 | [Pistol Position And Stance.](#17-pistol-position-and-stance) |
| 18 | [Short-Stocking.](#18-short-stocking) |
| 19 | [Third Person Animations.](#19-third-person-animations) |

---

## .0. Testing

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| test 1 | Teste 1 | float | 1 | -5000 – 5000 | ✓ | — |
| test 2 | Teste 2 | float | 1 | -5000 – 5000 | ✓ | — |
| test 3 | Teste 3 | float | 1 | -5000 – 5000 | ✓ | — |
| test 4 | Teste 4 | float | 1 | -5000 – 5000 | ✓ | — |
| test 5 | Teste 5 | float | 1 | -5000 – 5000 | ✓ | — |
| test 6 | Teste 6 | float | 1 | -5000 – 5000 | ✓ | — |
| test 7 | Teste 7 | float | 1 | -5000 – 5000 | ✓ | — |
| test 8 | Teste 8 | float | 1 | -5000 – 5000 | ✓ | — |
| test 9 | Teste 9 | float | 1 | -5000 – 5000 | ✓ | — |
| test 10 | Teste 10 | float | 1 | -5000 – 5000 | ✓ | — |
| Create Debug Zone | Criar Zona de Debug | KeyboardShortcut | None | — | ✓ | — |
| TargetZone | Zona Alvo | string | "" | — | ✓ | Zona de Debug |
| Effect Type | Tipo de Efeito | string | "" | — | ✓ | HeavyBleeding, LightBleeding, Fracture, removeHP, addHP. |
| Body Part Index | Índice de Parte do Corpo | int | 1 | — | ✓ | Head = 0, Chest = 1, Stomach = 2, Left Arm, Right Arm, Left Leg, Right Leg, Common (corpo todo) |
| Add Effect Keybind | Tecla de Adicionar Efeito | KeyboardShortcut | None | — | ✓ | — |
| Enable Zone Debug | Ativar Debug de Zona | bool | false | — | ✓ | — |
| Enable Dev Mode | Ativar Modo Dev | bool | false | — | ✓ | — |
| Enable Ballistics Logging | Ativar Log de Balística | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |
| Enable General Logging | Ativar Log Geral | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |
| Enable Medical Logging | Ativar Log Médico | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |
| Enable Reload Logging | Ativar Log de Recarga | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |
| Enable PWA Logging | Ativar Log PWA | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |
| Enable Recoil Logging | Ativar Log de Recuo | bool | false | — | ✓ | Ativa log para depuração e desenvolvimento |

---

## .1. Misc. Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Enable NVG/Thermal ADS Patch | Ativar Correção de Mira com NVG/Térmico | bool | (server) | — | — | Ópticas com magnificação bloqueiam a mira com NVGs. Não é possível mirar com miras ao usar óculos térmicos. |
| Enable Faceshield Patch | Ativar Correção de Viseira | bool | (server) | — | — | Viseiras bloqueiam a mira (ADS) a menos que o estoque/arma/viseira específica permita. |
| Enable Weight Mouse Sensitivity Penalty | Ativar Penalidade de Sensibilidade do Mouse por Peso | bool | (server) | — | — | Em vez de usar as estatísticas de penalidade de mouse do equipamento, é calculado com base no peso do equipamento + conteúdo, modificado pela estatística de conforto. |
| Enable Zero Shift | Ativar Desvio de Zero | bool | (server) | — | — | Miras simulam perda de zeragem ao atirar. A retícula tem chance de desviar do alvo. A chance é determinada pela precisão do escopo e sua montagem, e pelo recuo da arma. Miras de alta qualidade não perdem a zeragem. O SCAR-H tem desvio de zero pior. |

---

## .2. Ballistics Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Display Ammo Stats | Exibir Estatísticas de Munição | bool | (server) | — | — | Requer reinício. |
| Armor Durability Loss Modifier | Modificador de Perda de Durabilidade da Armadura | float | 1.25 | 0.25 – 2 | ✓ | Modifica a perda de durabilidade da armadura por tiro. |
| Ballistic Coefficient Modifier | Modificador de Coeficiente Balístico | float | 1.25 | 0.5 – 5 | ✓ | Determina a quantidade de arrasto nos projéteis. Maior = tempo de voo mais lento e mais queda. |
| Global Damage Modifier | Modificador Global de Dano | float | 1.0 | 0.1 – 2 | ✓ | Menor = menos dano recebido (exceto cabeça) por bots e jogador. |
| Enable Armor Plate Hitbox Changes | Ativar Mudanças na Hitbox da Placa de Armadura | bool | (server) | — | — | Reduz o tamanho das hitboxes das placas de armadura para mais próximo do real e de como foram implementadas originalmente. |
| Enable Body Hit Zones | Ativar Zonas de Impacto no Corpo | bool | (server) | — | — | Divide o corpo em zonas A, C e D como em alvos IPSC. Inclui braço superior, antebraço, coxa, panturrilha, pescoço, coluna e coração. Cada zona modifica dano e chance de sangramento. |
| Enable Hit Sounds | Ativar Sons de Impacto | bool | (server) | — | — | Ativa sons adicionais ao acertar as novas zonas do corpo e sons de impacto em armadura por material. |
| Flesh Hit Sound Multi | Multi. Volume de Sons de Impacto em Carne | float | 1.0 | 0 – 5 | ✓ | Aumenta/reduz o volume dos novos sons de impacto. |
| Close Armor Hit Sound Multi | Multi. Volume de Sons de Impacto Próximo em Armadura | float | 1.0 | 0 – 5 | ✓ | Aumenta/reduz o volume dos novos sons de impacto. |
| Distant Armor Hit Sound Mutli | Multi. Volume de Sons de Impacto Distante em Armadura | float | 1.0 | 0 – 5 | ✓ | Aumenta/reduz o volume dos novos sons de impacto. |
| Enable Ragdoll Fix (Experimental) | Ativar Correção de Ragdoll (Experimental) | bool | (server) | — | — | Requer reinício. Ativa correção para ragdolls voando para a estratosfera. |
| Ragdoll Force Modifier | Modificador de Força do Ragdoll | float | 0.01 | 0 – 10 | ✓ | Requer a Correção de Ragdoll ativada. |
| Disarm Base Chance. | Chance Base de Desarmamento | float | 1.0 | 0 – 100 | ✓ | Chance base de ser desarmado. 1 = 1% de chance. Aumentada pela energia cinética do projétil, reduzida pela armadura, e dobrada se o antebraço for atingido. |
| Fall Base Chance | Chance Base de Queda | float | 20.0 | 0 – 100 | ✓ | Chance base de ficar na posição deitada se atingido na perna. 1 = 1%. Aumentada pela energia cinética do projétil e dobrada se a panturrilha for atingida. |
| Enable Bot Knockdown | Ativar Derrubada de Bot | bool | (server) | — | — | Se atingido na perna com HP zero, há chance de o bot ficar deitado. Modificada pela energia cinética e dobrada se panturrilha atingida. |
| Enable Player Knockdown | Ativar Derrubada de Jogador | bool | (server) | — | — | Se atingido na perna com HP zero, há chance de o jogador ficar deitado. Modificada pela energia cinética e dobrada se panturrilha atingida. |
| Can Disarm Bot. | Pode Desarmar Bot | bool | false | — | — | Se atingido nos braços, há chance de a arma equipada cair. Chance modificada pela energia cinética, reduzida pela armadura do braço, e dobrada se o antebraço for atingido. AVISO: Bots desarmados ficam passivos e não atacam o jogador, por isso está desativado por padrão. |
| Can Disarm Player | Pode Desarmar Jogador | bool | (server) | — | — | Se atingido nos braços, há chance de a arma equipada cair. Chance modificada pela energia cinética, reduzida pela armadura do braço, e dobrada se o antebraço for atingido. |

---

## .3. Recoil Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Recoil Intensity | Intensidade do Recuo | float | 1.75 | 0 – 5 | — | Altera a intensidade geral do recuo. Aumenta/diminui recuo horizontal, dispersão e recuo vertical. Não afeta muito o recuo acumulado — afeta principalmente dispersão e visual. |
| Rifle Vertical Recoil Multi. | Multi. Recuo Vertical Rifle | float | 1.05 | 0 – 5 | — | Cima/Baixo. Também aumenta o recuo acumulado. |
| Pistol Vertical Recoil Multi | Multi. Recuo Vertical Pistola | float | 3.0 | 0 – 5 | — | Cima/Baixo. Também aumenta o recuo acumulado. |
| Horizontal Recoil Multi | Multi. Recuo Horizontal | float | 1.0 | 0 – 5 | — | Para frente/para trás. Também aumenta o tremor da arma ao atirar. |
| Rifle Dispersion Recoil Multi | Multi. Dispersão Rifle | float | 1.0 | 0 – 5 | — | Dispersão. Também aumenta o tamanho do padrão em S. |
| Pistol Dispersion Recoil Multi | Multi. Dispersão Pistola | float | 0.4 | 0 – 5 | — | Dispersão. Também aumenta o tamanho do padrão em S. |
| Rifle Camera Recoil Multi. | Multi. Recuo de Câmera Rifle | float | 1.0 | 0 – 5 | — | Recuo visual de câmera. |
| Pistol Camera Recoil Multi | Multi. Recuo de Câmera Pistola | float | 0.4 | 0 – 5 | — | Recuo visual de câmera. |
| Enable Recoil Angle | Ativar Ângulo de Recuo | bool | (server) | — | — | As armas recuam em ângulos diferentes; mais peso na frente torna o ângulo mais acentuado. Se desativado, todo recuo será a 90 graus. |
| Recoil Angle Multi | Multi. Ângulo de Recuo | float | 1.0 | 0.8 – 1.2 | — | Multiplicador para o ângulo de recuo; menor = ângulo mais acentuado. |
| Rifle Convergence Multi | Multi. Convergência Rifle | float | 0.6 | 0 – 40 | — | Também chamado de auto-compensação. Maior = recuo mais rápido, reset mais veloz e padrão de recuo mais apertado. |
| Pistol Convergence Multi | Multi. Convergência Pistola | float | 1.3 | 0 – 40 | — | Também chamado de auto-compensação. Maior = recuo mais rápido, reset mais veloz e padrão de recuo mais apertado. |

---

## .4. Advanced Recoil Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Use FPS Recoil Factor | Usar Fator de FPS no Recuo | bool | (server) | — | — | Considera o FPS atual para manter o recuo acumulado consistente. |
| Reset Recoil Randomness Multi | Multi. Aleatoriedade de Reset do Recuo | float | 1.15 | 0 – 10 | — | Maior = mais desvio do ponto de mira após atirar. |
| Recoil Randomness | Aleatoriedade de Recuo | float | 2.8 | 0 – 10 | — | Maior = recuo mais errático e padrão mais imprevisível. |
| Camera Recoil Speed | Velocidade do Recuo de Câmera | float | 0.07 | 0 – 0.5 | — | Maior = recuo de câmera mais rápido. |
| Camera Recoil Wiggle | Tremor de Câmera | float | 0.82 | 0 – 0.9 | — | Maior = mais tremor de câmera. |
| Enable Additional Visual Recoil | Ativar Recuo Visual Adicional | bool | (server) | — | — | Ativa elementos de recuo visual adicionais. A arma se move mais em novas direções ao atirar; não tem efeito significativo na dispersão. |
| BSG Visual Recoil Multi | Multi. Recuo Visual BSG | float | 1.0 | 0 – 5 | — | Multi. para todos os elementos de recuo visual da BSG; faz a arma vibrar mais ao atirar. É afetado pelas estatísticas de arma do Realism. |
| Realism Visual Recoil Multi | Multi. Recuo Visual Realism | float | 1.0 | 0 – 5 | — | Multi. para todos os elementos de recuo visual do mod; faz a arma vibrar mais ao atirar. É afetado pelas estatísticas de arma. |
| Recoil Climb Speed Multi | Multi. Velocidade de Recuo Acumulado | float | 4.0 | 0.1 – 20 | — | Quão rápido o recuo acumula; pode torná-lo mais suave ou mais brusco. |
| Recoil Climb Multi | Multi. Recuo Acumulado | float | 4.0 | 0 – 50 | — | Multiplicador de quanto as armas (exceto pistolas) sobem verticalmente por tiro. A estatística de recuo vertical da arma aumenta isso. |
| Pistol Recoil Climb Multi. | Multi. Recuo Acumulado Pistola | float | 0.4 | 0 – 50 | — | Multiplicador de quanto as pistolas sobem verticalmente por tiro. A estatística de recuo vertical da arma aumenta isso. |
| S-Pattern Multi. | Multi. Padrão em S | float | 1.2 | 0 – 50 | — | Aumenta o tamanho do clássico padrão em S. A estatística de dispersão da arma aumenta isso. |
| S-Pattern Speed Multi | Multi. Velocidade do Padrão em S | float | 3.0 | 0 – 100 | — | Aumenta a velocidade com que o recuo faz o padrão em S. |
| Reset Delay | Atraso de Reset | float | 0.14 | -0.1 – 0.5 | ✓ | Tempo em segundos que deve passar antes de o disparo ser considerado encerrado; o recuo não fará reset antes disso. |
| Rearward Recoil | Recuo para Trás | bool | (server) | — | ✓ | Faz o recuo ir em direção ao ombro do jogador em vez de para frente. |
| Rearward Recoil Wiggle Multi | Multi. Tremor de Recuo para Trás | float | 1.0 | 0.1 – 1.5 | — | Quantidade de tremor para trás após atirar. |
| Rifle Vertical Recoil Wiggle Multi | Multi. Tremor Vertical Rifle | float | 1.0 | 0.1 – 1.5 | — | Quantidade de tremor vertical após atirar. |
| Pistol Vertical Recoil Wiggle Multi | Multi. Tremor Vertical Pistola | float | 0.7 | 0.1 – 1.5 | — | Quantidade de tremor vertical após atirar. |

---

## .5. Stat Display Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Show Balance Stat | Exibir Estatística de Balanço | bool | (server) | — | — | Requer reinício. Aviso: exibir muitas estatísticas em armas com muitos slots dificulta o uso do menu de inspeção. |
| Show Camera Recoil Stat | Exibir Estatística de Recuo de Câmera | bool | false | — | — | Requer reinício. Aviso: exibir muitas estatísticas em armas com muitos slots dificulta o uso do menu de inspeção. |
| Show Dispersion Stat | Exibir Estatística de Dispersão | bool | false | — | — | Requer reinício. Aviso: exibir muitas estatísticas em armas com muitos slots dificulta o uso do menu de inspeção. |
| Show Recoil Angle Stat | Exibir Estatística de Ângulo de Recuo | bool | (server) | — | — | Requer reinício. Aviso: exibir muitas estatísticas em armas com muitos slots dificulta o uso do menu de inspeção. |
| Show Semi Auto ROF Stat | Exibir Estatística de Cadência Semi-Auto | bool | (server) | — | — | Requer reinício. Aviso: exibir muitas estatísticas em armas com muitos slots dificulta o uso do menu de inspeção. |

---

## .6. Weapon Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Enable Muzzle Effects. | Ativar Efeitos de Boca de Cano | bool | (server) | — | — | Ativa alterações no flash de boca, fumaça etc. e torna sua intensidade dependente do calibre, condição da arma, acessórios, etc. |
| Aim Punch Intensity. | Intensidade do Aim Punch | float | 0.8 | 0 – 5 | — | Quantidade de aim punch. |
| Sway Intensity. | Intensidade do Balanço | float | 1.0 | 0 – 5 | — | Altera a intensidade do balanço de mira. |
| Procedural Intensity. | Intensidade Procedural | float | 1.0 | 0 – 3 | — | Altera a intensidade das animações procedurais, incluindo balanço, movimento da arma e inércia. |
| Malfunction Reduction Durability Threshold. | Limiar de Durabilidade para Redução de Falha | float | 90.0 | 1 – 100 | — | A chance de falha baseada em durabilidade é reduzida até este limiar de durabilidade ser excedido. |
| Malfunction Durability Threshold | Limiar de Durabilidade para Falha | float | 98.0 | 1 – 100 | — | A chance de falha é quase 0 até este limiar ser atingido, exceto se critérios específicos forem atendidos (calor, contagem de tiros em rajada, chance de falha do pente, munição, modificações, subsônico sem suporte, etc.). |
| Malfunction Multi | Multi. de Falha | float | 0.75 | 0 – 5 | — | Multiplicador de chance de falha. |
| Enable Increased Inaccuracy | Ativar Inacurácia Aumentada | bool | (server) | — | — | Requer reinício. Aumenta a imprecisão de todas as armas para que MOA/Precisão seja uma estatística mais importante. |

---

## .7. Health and Meds Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Medical Notifications | Notificações Médicas | bool | (server) | — | — | Ativa notificações para efeitos de status médico, cura, etc. |
| Enable Hydration/Energy Loss Rate Changes | Ativar Mudanças de Taxa de Perda de Hidratação/Energia | bool | (server) | — | — | Ativa alterações no cálculo das taxas de perda de hidratação e energia. Aumentadas por ferimentos, uso de drogas, corrida e peso. |
| Enable Passive Regen | Ativar Regeneração Passiva | bool | (server) | — | — | Ativa regeneração em certas condições e se o jogador não tiver levado dano por algum tempo. |
| Hydration Drain Rate Multi. | Multi. Taxa de Perda de Hidratação | float | 0.5 | 0.1 – 1.5 | — | Menor = menos perda. |
| Heartbeat SFX Volume | Volume do SFX de Batimento Cardíaco | float | 0.4 | 0 – 2 | — | Modificador de volume do SFX de batimento cardíaco (usado para Adrenalina). |
| Energy Drain Rate Multi. | Multi. Taxa de Perda de Energia | float | 0.3 | 0.1 – 1.5 | — | Menor = menos perda. |
| Enable Tourniquet Effect | Ativar Efeito do Torniquete | bool | (server) | — | — | O torniquete drena HP do membro ao qual é aplicado. |
| Gear Blocks Consumption | Equipamento Bloqueia Consumo | bool | (server) | — | — | Equipamento bloqueia comer e beber. Inclui algumas máscaras, óculos de visão noturna e viseiras ativadas. |
| Gear Blocks Healing | Equipamento Bloqueia Cura | bool | false | — | — | Equipamento bloqueia o uso de medicamentos se o ferimento estiver coberto por ele. |
| Adrenaline | Adrenalina | bool | (server) | — | — | Se o jogador for atingido ou alvo de tiros, recebe efeito de analgésico, visão em túnel e tremores. Duração e intensidade determinadas pela habilidade de resistência ao estresse. |
| Remove Gear Keybind (Double Press) | Tecla de Remover Equipamento (Pressão Dupla) | KeyboardShortcut | P | — | — | Remove qualquer equipamento que esteja bloqueando a cura de um ferimento. Pressão dupla, como a tecla de mochila. |

---

## .8. Hazard Zone Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Toggle Equip Gas Mask | Alternar Máscara de Gás | KeyboardShortcut | Ctrl+Z | — | — | Equipa a máscara de gás do colete tático, braçadeira ou bolsos/slots especiais. Ao desequipar, tenta retornar ao slot original. |
| Mute Gas Analysed Key | Tecla de Silenciar Analisador de Gás | KeyboardShortcut | Ctrl+M | — | — | — |
| Mute Geiger Key | Tecla de Silenciar Geiger | KeyboardShortcut | Alt+M | — | — | — |
| Display True Hazard Rates | Exibir Taxas Reais de Perigo | bool | false | — | — | Exibe a taxa de perigo "real", sem considerar medicamentos ou máscara de gás. |
| Visualize Radiation | Visualizar Radiação | bool | (server) | — | — | A radiação causa ruído visual; a intensidade depende da taxa atual e do envenenamento total. |
| Visualize Gas | Visualizar Gás | bool | (server) | — | — | O gás se torna visível. |
| Gas Mask Breath Volume | Volume de Respiração com Máscara de Gás | float | 0.4 | 0 – 2 | — | Modificador de volume do SFX da máscara de gás. |
| Device Volume | Volume dos Dispositivos | float | 0.45 | 0 – 2 | — | Modificador de volume do Geiger e do Analisador de Gás. |

---

## .9. Movement Settings

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Enable Ground Material Speed Modifier | Ativar Modificador de Velocidade por Material do Chão | bool | (server) | — | — | Ativa a velocidade de movimento sendo afetada pelo material do chão (concreto, grama, metal, vidro etc.). |
| Enable Ground Slope Speed Modifier | Ativar Modificador de Velocidade por Inclinação | bool | false | — | — | Ativa rampas desacelerando o movimento. Pode causar desacelerações aleatórias em pequenos pontos devido à geometria ruim dos mapas da BSG. |
| Enable Sprint Aim Penalties | Ativar Penalidades de Mira após Corrida | bool | (server) | — | — | A mira após corrida tem um breve atraso, velocidade de mira reduzida e balanço aumentado. Quanto mais tempo você correr, maior a penalidade. |

---

## .10. Deafening and Audio

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Reduce Gain Keybind | Tecla de Reduzir Ganho | KeyboardShortcut | KeypadMinus | — | — | — |
| Increase Gain Keybind | Tecla de Aumentar Ganho | KeyboardShortcut | KeypadPlus | — | — | — |
| Headset Impulse Noise Reduction | Redução de Ruído Impulsivo do Headset | int | 0 | -10 – 15 | — | Para qual nível de amplificação o headset reduz quando há tiros ou explosões. Está restrito por código a não exceder o ganho atual do headset. |
| Headset Gain | Ganho do Headset | int | 2 | -5 – 15 | — | ATENÇÃO: CUIDADO AO AUMENTAR MUITO! PODE PREJUDICAR SUA AUDIÇÃO! Ajusta o ganho dos headsets equipados em tempo real, como o controle de volume de protetores auriculares reais. |
| Gunshot Volume | Volume de Disparos | float | 0.7 | 0 – 2 | — | Multiplicador para o volume de disparos de jogador e NPCs. Maior = mais alto. |
| Enable Ambient Audio Changes | Ativar Alterações de Áudio Ambiente | bool | false | — | — | Ativa o uso dos multiplicadores de áudio ambiente. Pode causar falhas de áudio ao transitar de interior para exterior. |
| Outdoor Ambient Audio Offset | Offset de Áudio Ambiente Externo | float | 0.0 | -60 – 50 | — | Ajusta o volume ambiente com e sem headsets. Maior = mais alto. |
| Indoor Ambient Audio Offset | Offset de Áudio Ambiente Interno | float | -20.0 | -60 – 50 | — | Ajusta o volume ambiente com e sem headsets. Maior = mais alto. |
| Shared Movement Volume Multi | Multi. de Volume de Movimento Compartilhado | float | 1.0 | 0 – 5 | — | Multiplicador para o volume de corrida de jogador + NPC. Compartilhado devido a limitações da BSG. |
| NPC Movement Volume Multi | Multi. de Volume de Movimento de NPC | float | 1.0 | 0 – 5 | — | Multiplicador para o volume de movimento de NPC. Inclui caminhada e barulho de equipamento. |
| Player Movement Volume Multi | Multi. de Volume de Movimento do Jogador | float | 1.0 | 0 – 5 | — | Multiplicador para o volume de movimento do jogador. Inclui caminhada e barulho de equipamento. |
| ADS Volume Multi | Multi. de Volume de Mira | float | 2.5 | 0 – 10 | — | Volume da mira (ADS). Maior = mais alto. |
| Deafen Reset Delay | Atraso de Reset do Ensurdecimento | float | 1.0 | 0 – 10 | — | Atraso antes de o ensurdecimento e a visão em túnel começarem a resetar. |

---

## 11. Weapon Speed Modifiers

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Pistol Aim Speed Multi. | Multi. Velocidade de Mira Pistola | float | 1.0 | 0.1 – 10 | — | — |
| Aim Speed Multi. | Multi. Velocidade de Mira | float | 1.0 | 0.1 – 10 | — | — |
| Magazine Reload Speed Multi | Multi. Velocidade de Recarga de Pente | float | 1.0 | 0.1 – 10 | — | — |
| Malfunction Fix Speed Multi | Multi. Velocidade de Correção de Falha | float | 1.0 | 0.1 – 10 | — | — |
| UBGL Reload Speed Multi | Multi. Velocidade de Recarga de UBGL | float | 1.0 | 0.1 – 10 | ✓ | — |
| Pistol Rechamber Speed Multi | Multi. Velocidade de Re-engatilhamento Pistola | float | 1.0 | 0.1 – 10 | ✓ | — |
| Rechamber Speed Multi | Multi. Velocidade de Re-engatilhamento | float | 1.0 | 0.1 – 10 | — | — |
| Bolt Speed Multi | Multi. Velocidade do Ferrolho | float | 1.0 | 0.1 – 10 | — | — |
| Shotgun Rack Speed Multi | Multi. Velocidade de Armação de Escopeta | float | 1.0 | 0.1 – 10 | — | — |
| Chamber Check Speed Multi | Multi. Velocidade de Verificação da Câmara | float | 1.25 | 0.1 – 10 | — | — |
| Shotgun Chamber Check Speed Multi | Multi. Velocidade de Verificação da Câmara de Escopeta | float | 1.25 | 0.1 – 10 | ✓ | — |
| Pistol Chamber Check Speed Multi | Multi. Velocidade de Verificação da Câmara de Pistola | float | 1.25 | 0.1 – 10 | ✓ | — |
| Pistol Check Ammo Multi | Multi. de Verificação de Munição de Pistola | float | 1.25 | 0.1 – 10 | ✓ | — |
| Check Ammo Multi. | Multi. de Verificação de Munição | float | 1.3 | 0.1 – 10 | — | — |
| Quick Reload Multi | Multi. de Recarga Rápida | float | 1.45 | 0.1 – 10 | — | — |
| Internal Magazine Reload | Recarga de Pente Interno | float | 1.15 | 0.1 – 10 | — | — |

---

## 12. Weapon Stances And Position

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| De-Jank EFT Animations | Reduzir Travamentos das Animações EFT | bool | (server) | — | — | Tenta tornar certas animações do EFT menos travadas, como animações de inventário e de portas. |
| Modify BSG Collision | Modificar Colisão BSG | bool | (server) | — | — | Se 'Override Collision' estiver ativo, esta opção é ignorada. Ajusta o tratamento de colisão de arma da BSG; torna mais lento e menos travado. |
| Override Collision | Substituir Colisão | bool | (server) | — | — | Se ativo, 'Modify BSG Collision' é ignorado. Substitui completamente o tratamento de colisão da BSG por solução personalizada. Requer o mod FOV Fix para funcionar corretamente. |
| Use Realism Mounting System | Usar Sistema de Apoio do Realism | bool | (server) | — | — | Substitui o sistema de apoio da BSG pelo do Realism (implementado primeiro). Mecânicas de recuo, postura e balanço são construídas em torno do apoio do Realism e não funcionarão corretamente com o da BSG. |
| Enable Extra Weapon Position/Rotation Effects | Ativar Efeitos Extras de Posição/Rotação da Arma | bool | (server) | — | — | A arma tem leve inclinação baseada em ergonomia. Mira com máscara de gás/viseira é inclinada. A inclinação aumenta ao agachar e a arma se aproxima. Outros efeitos sutis. |
| Remember Stance After Firing | Lembrar Postura após Atirar | bool | (server) | — | — | Lembra a postura após atirar se o jogador estava mirando. |
| Remember Stance After Using Item | Lembrar Postura após Usar Item | bool | (server) | — | — | Lembra a postura após ações (usar itens). |
| Block Shooting While In Stance | Bloquear Tiro na Postura | bool | false | — | — | Bloqueia o disparo enquanto em postura; cancela a postura ao tentar atirar. |
| Allow Reload From Active Aim | Permitir Recarga da Mira Ativa | bool | false | — | — | Permite recarga de pente no Active Aim com bônus de velocidade. |
| Enable High Ready Sprint Animation | Ativar Animação de Corrida no High Ready | bool | (server) | — | — | Ativa a animação de corrida no High Ready ao correr na posição de High Ready. |
| Enable Alternative Pistol Position And ADS | Ativar Posição Alternativa de Pistola e Mira | bool | (server) | — | — | A pistola será segurada centralizada e em postura comprimida. A mira é animada. Com FOV Fix, a arma se move para a câmera para mira mais suave. |
| Enable Alternative Rifle Position And ADS | Ativar Posição Alternativa de Rifle e Mira | bool | (server) | — | — | A posição do rifle será mais centralizada. Com FOV Fix, a arma se move para a câmera para mira mais suave. |
| Enable Alternative Rifle Recoil Override | Ativar Substituição de Recuo de Rifle Alternativo | bool | (server) | — | — | Ao usar rifle alternativo, permite substituir o recuo. Resulta em sensação de recuo diferente mas transição mais suave do estado de disparo para o de mira sem disparo. |
| Enable Idle Arm Stamina Drain | Ativar Dreno de Stamina dos Braços em Repouso | bool | (server) | — | — | A stamina dos braços drena quando não em postura (High Ready, Low Ready, Short-Stocking). |
| Idle Stam Drain Modifer | Modificador de Dreno de Stamina em Repouso | float | 1.0 | 0.1 – 5 | — | — |
| Enable Stance Stamina And Movement Effects | Ativar Efeitos de Stamina e Movimento por Postura | bool | (server) | — | — | Posturas e apoio afetam stamina e velocidade de movimento. High Ready, Low Ready, Short-Stocking e pistola regeneram stamina mais rápido. High Ready tem corrida mais rápida; Low Ready tem aceleração de corrida mais rápida. Stamina dos braços não drena stamina normal ao chegar a 0. |
| Enable Mounting UI | Ativar UI de Apoio | bool | (server) | — | — | Se ativo, um ícone na tela indica se o jogador está apoiado, montado e em qual lado da cobertura está. |
| Left Shoulder Offset | Offset do Ombro Esquerdo | float | -0.13 | -0.2 – 0.1 | — | — |
| Stance Sfx Volume Modifier | Modificador de Volume SFX de Postura | float | 2.0 | 0.1 – 20 | — | Modificador de volume do barulho do equipamento ao fazer movimentos de postura. |
| Rifle Position Offset | Offset de Posição do Rifle | Vector3 | (-0.04, -0.015, 0) | — | ✓ | A opção 'alt rifle' é necessária. Ajusta a posição inicial do rifle na tela. |
| Stance Rotation Speed Multi | Multi. de Velocidade de Rotação de Postura | float | 1.0 | 0.1 – 10 | ✓ | Ajusta a velocidade das mudanças de rotação de postura. |
| Stance Transition Speed. | Velocidade de Transição de Postura | float | 15.0 | 1 – 35 | ✓ | Ajusta a velocidade de mudança de posição entre posturas. |

---

## 13. Weapon Stances Keybinds

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Keybind To Use With Mouse Wheel | Tecla para Usar com Roda do Mouse | KeyboardShortcut | LeftControl | — | — | Tecla usada em combinação com a roda do mouse, se ativado. |
| Require Key + Mouse Wheel | Exigir Tecla + Roda do Mouse | bool | (server) | — | — | Exige a tecla + roda do mouse para mudar de postura. |
| Enable Mouse Wheel Stance Switching | Ativar Troca de Postura pela Roda do Mouse | bool | (server) | — | — | Alterna entre High Ready e Low Ready pela roda do mouse. |
| Patrol/Neutral Stance Keybind | Tecla de Postura de Patrulha/Neutra | KeyboardShortcut | K | — | — | Coloca a arma em posição neutra, melhorando a regen de stamina dos braços e a velocidade de caminhada. Para o máximo de larping. |
| Melee Keybind | Tecla de Corpo a Corpo | KeyboardShortcut | None | — | — | Atacar com o cano ou baioneta da arma equipada. |
| Short-Stock Keybind | Tecla de Short-Stocking | KeyboardShortcut | J | — | — | Tuca o estoque da arma sob o braço, encurtando o comprimento total para evitar que o cano seja empurrado para longe do alvo. |
| Low Ready Keybind | Tecla de Low Ready | KeyboardShortcut | Ctrl+Mouse3 | — | — | — |
| High Ready Keybind | Tecla de High Ready | KeyboardShortcut | Alt+Mouse3 | — | — | — |
| Use Toggle For Active Aim | Usar Alternância para Active Aim | bool | false | — | — | — |
| Active Aim Keybind | Tecla de Active Aim | KeyboardShortcut | Mouse4 | — | — | Inclina a arma para o lado, melhorando a precisão de quadril. |
| Cycle Stances Keybind | Tecla de Ciclar Posturas | KeyboardShortcut | None | — | — | Cicla entre High Ready, Low Ready e Short-Stocking. Clique duplo retorna ao repouso. |

---

## 14. Active Aim

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Active Aim Additonal Rotation Speed Multi. | Multi. de Velocidade de Rotação Adicional (Active Aim) | float | 2.0 | 0 – 10 | ✓ | — |
| Active Aim Reset Rotation Speed Multi. | Multi. de Velocidade de Reset de Rotação (Active Aim) | float | 3.5 | 0 – 10 | ✓ | — |
| Active Aim Rotation Speed Multi. | Multi. de Velocidade de Rotação (Active Aim) | float | 2.0 | 0 – 10 | ✓ | — |
| Active Aim Speed Multi | Multi. de Velocidade (Active Aim) | float | 15.0 | 1 – 100 | ✓ | — |
| Active Aim Reset Speed Multi | Multi. de Velocidade de Reset (Active Aim) | float | 6.0 | 1 – 100 | ✓ | — |
| Active Aim Position | Posição do Active Aim | Vector3 | (-0.02, 0.008, 0) | — | ✓ | Posição da arma na postura. |
| Active Aim Rotation | Rotação do Active Aim | Vector3 | (0, -35, 0) | — | ✓ | Rotação da arma na postura. |
| Active Aiming Additional Rotation | Rotação Adicional do Active Aim | Vector3 | (0, -35, 0) | — | ✓ | Rotação adicional separada da arma ao entrar na postura. |
| Active Aiming Reset Rotation | Rotação de Reset do Active Aim | Vector3 | (-0.5, 20.5, -2) | — | ✓ | Rotação da arma ao sair da postura. |

---

## 15. High Ready

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| High Ready Additonal Rotation Speed Multi. | Multi. de Velocidade de Rotação Adicional (High Ready) | float | 0.1 | 0 – 100 | ✓ | Quão rápido a arma rota ao sair da postura. |
| High Ready Reset Rotation Speed Multi. | Multi. de Velocidade de Reset de Rotação (High Ready) | float | 1.5 | 0 – 100 | ✓ | Quão rápido a arma rota ao sair da postura. |
| High Ready Rotation Speed Multi. | Multi. de Velocidade de Rotação (High Ready) | float | 2.0 | 1 – 100 | ✓ | Quão rápido a arma rota ao entrar na postura. |
| High Ready Reset Speed Multi | Multi. de Velocidade de Reset (High Ready) | float | 6.5 | 0 – 100 | ✓ | Quão rápido a arma se move ao sair da postura. |
| High Ready Speed Multi | Multi. de Velocidade (High Ready) | float | 6.0 | 0 – 100 | ✓ | Quão rápido a arma se move ao entrar na postura. |
| High Ready Position | Posição do High Ready | Vector3 | (0.005, 0.035, -0.04) | — | ✓ | Posição da arma na postura. |
| High Ready Rotation | Rotação do High Ready | Vector3 | (-8, -20, 0) | — | ✓ | Rotação da arma na postura. |
| High Ready Additional Rotation | Rotação Adicional do High Ready | Vector3 | (-50, -25, -5) | — | ✓ | Rotação adicional separada da arma ao entrar na postura. |
| High Ready Reset Rotation | Rotação de Reset do High Ready | Vector3 | (0, 2, 0) | — | ✓ | Rotação da arma ao sair da postura. |

---

## 16. Low Ready

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Low Ready Additonal Rotation Speed Multi | Multi. de Velocidade de Rotação Adicional (Low Ready) | float | 0.75 | 0 – 100 | ✓ | Quão rápido a arma rota. |
| Low Ready Reset Rotation Speed Multi | Multi. de Velocidade de Reset de Rotação (Low Ready) | float | 2.25 | 0 – 100 | ✓ | Quão rápido a arma rota. |
| Low Ready Rotation Speed Multi | Multi. de Velocidade de Rotação (Low Ready) | float | 1.5 | 0 – 100 | ✓ | Quão rápido a arma rota. |
| Low Ready Speed Multi. | Multi. de Velocidade (Low Ready) | float | 14.0 | 0 – 100 | ✓ | — |
| Low Ready Reset Speed Multi | Multi. de Velocidade de Reset (Low Ready) | float | 8.7 | 0 – 100 | ✓ | — |
| Low Ready Position | Posição do Low Ready | Vector3 | (0, -0.01, 0) | — | ✓ | Posição da arma na postura. |
| Low Ready Rotation | Rotação do Low Ready | Vector3 | (8, -5, -1) | — | ✓ | Rotação da arma na postura. |
| Low Ready Additional Rotation | Rotação Adicional do Low Ready | Vector3 | (12, -1, 0) | — | ✓ | Rotação adicional separada da arma ao entrar na postura. |
| Low Ready Reset Rotation | Rotação de Reset do Low Ready | Vector3 | (-1, 0, 0) | — | ✓ | Rotação da arma ao sair da postura. |

---

## 17. Pistol Position And Stance

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Pistol Additional Rotation Speed Multi | Multi. de Velocidade de Rotação Adicional (Pistola) | float | 0.1 | 0 – 20 | ✓ | Quão rápido a arma rota. |
| Pistol Reset Rotation Speed Multi | Multi. de Velocidade de Reset de Rotação (Pistola) | float | 0.5 | 0 – 20 | ✓ | Quão rápido a arma rota. |
| Pistol Rotation Speed Multi | Multi. de Velocidade de Rotação (Pistola) | float | 1.0 | 0 – 20 | ✓ | Quão rápido a arma rota. |
| Pistol Position Speed Multi | Multi. de Velocidade de Posição (Pistola) | float | 6.0 | 1 – 100 | ✓ | — |
| Pistol Position Reset Speed Multi | Multi. de Velocidade de Reset de Posição (Pistola) | float | 8.0 | 1 – 100 | ✓ | — |
| Pistol Position | Posição da Pistola | Vector3 | (0, 0.04, -0.015) | — | ✓ | Posição da arma na postura. |
| Pistol Rotation | Rotação da Pistola | Vector3 | (0, -5, 0) | — | ✓ | Rotação da arma na postura. |
| Pistol Ready Additional Rotation | Rotação Adicional da Pistola Pronta | Vector3 | (0, 0, 0) | — | ✓ | Rotação adicional separada da arma ao entrar na postura. |
| Pistol Ready Reset Rotation | Rotação de Reset da Pistola Pronta | Vector3 | (-5, 0, 0) | — | ✓ | Rotação da arma ao sair da postura. |

---

## 18. Short-Stocking

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Short-Stock Additional Rotation Speed Multi | Multi. de Velocidade de Rotação Adicional (Short-Stock) | float | 1.5 | 0.1 – 5 | ✓ | Quão rápido a arma rota. |
| Short-Stock Reset Rotation Speed Multi | Multi. de Velocidade de Reset de Rotação (Short-Stock) | float | 1.0 | 0.1 – 5 | ✓ | Quão rápido a arma rota. |
| Short-Stock Rotation Speed Multi | Multi. de Velocidade de Rotação (Short-Stock) | float | 2.0 | 0.1 – 5 | ✓ | Quão rápido a arma rota. |
| Short-Stock Position Speed Multi. | Multi. de Velocidade de Posição (Short-Stock) | float | 4.0 | 1 – 100 | ✓ | — |
| Short-Stock Position Reset Speed Mult | Multi. de Velocidade de Reset de Posição (Short-Stock) | float | 3.8 | 1 – 100 | ✓ | — |
| Short-Stock Position | Posição do Short-Stock | Vector3 | (0.02, 0.1, -0.025) | — | ✓ | Posição da arma na postura. |
| Short-Stock Rotation | Rotação do Short-Stock | Vector3 | (0, -15, 0) | — | ✓ | Rotação da arma na postura. |
| Short-Stock Ready Additional Rotation | Rotação Adicional do Short-Stock Pronto | Vector3 | (-3, -15, 1) | — | ✓ | Rotação adicional separada da arma ao entrar na postura. |
| Short-Stock Ready Reset Rotation | Rotação de Reset do Short-Stock Pronto | Vector3 | (-1.5, 2, 0) | — | ✓ | Rotação da arma ao sair da postura. |

---

## 19. Third Person Animations

| Nome (EN) | Nome (PT-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (PT-BR) |
|-----------|-------------|------|--------|-------|----------|-----------------|
| Third Person Position Speed Multi | Multi. de Velocidade de Posição em Terceira Pessoa | float | 1.0 | 0.1 – 20 | ✓ | Velocidade de mudança de posição de postura em terceira pessoa. |
| Third Person Rotation Speed Multi | Multi. de Velocidade de Rotação em Terceira Pessoa | float | 1.5 | 0.1 – 20 | ✓ | Velocidade de mudança de rotação de postura em terceira pessoa. |
| Pistol Third Person Position | Posição da Pistola em Terceira Pessoa | Vector3 | (-0.03, 0.04, -0.05) | — | ✓ | — |
| Pistol Third Person Rotation | Rotação da Pistola em Terceira Pessoa | Vector3 | (0, 15, 0) | — | ✓ | — |
| Short-Stock Third Person Position | Posição do Short-Stock em Terceira Pessoa | Vector3 | (0.03, 0.065, -0.075) | — | ✓ | — |
| Short-Stock Third Person Rotation | Rotação do Short-Stock em Terceira Pessoa | Vector3 | (0, -15, 0) | — | ✓ | — |
| Active Aim Third Person Position | Posição do Active Aim em Terceira Pessoa | Vector3 | (-0.02, -0.02, 0.02) | — | ✓ | — |
| Active Aim Third Person Rotation | Rotação do Active Aim em Terceira Pessoa | Vector3 | (0, -35, 0) | — | ✓ | — |
| High Ready Third Person Position | Posição do High Ready em Terceira Pessoa | Vector3 | (0.02, 0.05, -0.045) | — | ✓ | — |
| High Ready Third Person Rotation | Rotação do High Ready em Terceira Pessoa | Vector3 | (-8, -25, 0) | — | ✓ | — |
| Low Ready Third Person Position | Posição do Low Ready em Terceira Pessoa | Vector3 | (0.01, -0.025, 0) | — | ✓ | — |
| Low Ready Third Person Rotation | Rotação do Low Ready em Terceira Pessoa | Vector3 | (24, 10, -1) | — | ✓ | — |
