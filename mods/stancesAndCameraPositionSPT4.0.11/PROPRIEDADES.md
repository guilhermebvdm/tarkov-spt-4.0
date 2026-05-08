# Propriedades do F12 — stancesAndCameraPositionSPT4.0.11

Plugin BepInEx: `shwng.camerarotation` — *shwng.FpsCameraStances v1.1.0*

Lista das 83 propriedades expostas no menu do BepInEx ConfigurationManager (F12), traduzidas para pt-BR. Itens marcados com **(Avançado)** só aparecem quando "Advanced settings" está habilitado no F12. A coluna **Tooltip (pt-BR)** é a tradução fiel do `ConfigDescription` que aparece ao passar o mouse sobre a propriedade.

Fonte: [modded/Plugin.cs](modded/Plugin.cs) (63 originais + 20 novas em backlog `001-stamina-e-velocidade`)

---

## Posições (`Positions`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Camera Position | Habilitar Posição da Câmera | bool | `true` | — | Habilita ou desabilita os offsets de posição da câmera |
| Forward/Backward Offset | Deslocamento Frente/Trás | float | `0` | -0.5 a 0.5 | Posição da câmera para frente/trás (positivo = para frente) |
| Up/Down Offset | Deslocamento Cima/Baixo | float | `0.02` | -0.5 a 0.5 | Posição da câmera para cima/baixo (positivo = para cima) |
| Sideways Offset | Deslocamento Lateral | float | `0` | -0.5 a 0.5 | Posição da câmera para esquerda/direita (positivo = direita) |

## Configurações (`Settings`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 1 in Cycle | Incluir Postura 1 no Ciclo | bool | `true` | — | Quando habilitado, a Postura 1 é incluída no ciclo de posturas. Quando desabilitado, a Postura 1 é pulada. |
| Enable Stance 2 in Cycle | Incluir Postura 2 no Ciclo | bool | `true` | — | Quando habilitado, a Postura 2 é incluída no ciclo de posturas. Quando desabilitado, a Postura 2 é pulada. |
| Enable Stance 3 in Cycle | Incluir Postura 3 no Ciclo | bool | `true` | — | Quando habilitado, a Postura 3 é incluída no ciclo de posturas. Quando desabilitado, a Postura 3 é pulada. |
| Stance Toggle Hotkey | Tecla para Trocar Postura | KeyCode | `V` | — | Pressione esta tecla para ciclar pelas posturas habilitadas: Padrão → Postura 1 → Postura 2 → Postura 3 → Padrão |
| Enable Mouse Wheel Stance Cycle | Habilitar Ciclo via Roda do Mouse | bool | `false` | — | Quando habilitado, segure a tecla modificadora e role a roda do mouse para ciclar entre posturas |
| Mouse Wheel Modifier Key | Tecla Modificadora da Roda | KeyCode | `LeftAlt` | — | Segure esta tecla enquanto rola a roda do mouse para ciclar entre posturas (quando o ciclo via roda está habilitado) |
| Use Only Stances | Usar Apenas Posturas | bool | `true` | — | Quando habilitado, o ciclo pula a posição Padrão (não-postura) e cicla apenas pelas posturas habilitadas |
| Stance Transition Speed | Velocidade de Transição de Postura | float | `1` | 0.5 a 20 | Quão rápido as mãos transicionam entre Padrão e Postura. Maior = mais rápido/abrupto, Menor = mais lento/suave. Recomendado: 3-10 |
| ADS Transition Speed | Velocidade de Transição da Mira | float | `1` | 0.5 a 5 | Quão rápido as mãos transicionam entre as posições de postura e mira. 1 = lento, 2 = normal, 3+ = rápido/abrupto. |
| Stance Change Sound Volume | Volume do Som ao Trocar Postura | float | `1` | 0 a 2 | Multiplicador de volume do som de chacoalho ao trocar de postura. 0 = mudo, 1 = normal, 2 = mais alto. |

## Transições Avançadas de Mira (`Advanced ADS Transitions`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Advanced ADS Transitions | Transições Avançadas de Mira | bool | `false` | — | — | Quando habilitado, a arma é arremessada para frente e empurrada de volta ao mirar, simulando o encaixe no ombro |
| Affect Stance Transition Too | Aplicar Também na Transição de Postura | bool | `true` | — | **(Avançado)** | Quando habilitado (requer 'Transições Avançadas de Mira'), aplica o mesmo efeito de encaixe ao trocar entre posturas |
| ADS Shoulder Throw Intensity | Intensidade do Encaixe ao Mirar | float | `1` | 0 a 2 | **(Avançado)** | Intensidade geral do efeito de arremesso ao mirar. Multiplica os valores de Frente, Cima, Yaw, Pitch, Roll. 0 = sem arremesso, 1 = usa valores do config, 2 = efeito dobrado. |
| Stance Shoulder Throw Intensity | Intensidade do Encaixe ao Trocar Postura | float | `0.75` | 0 a 2 | **(Avançado)** | Intensidade geral do efeito de arremesso ao trocar postura. Multiplica os valores de Frente, Cima, Yaw, Pitch, Roll. 0 = sem arremesso, 1 = usa valores do config, 2 = efeito dobrado. |
| Scale by Weapon Stats | Escalar por Stats da Arma | bool | `true` | — | **(Avançado)** | Quando habilitado, a velocidade/duração/quantidade do encaixe escala com o peso e a ergonomia da arma (usa o cálculo de AimingSpeed do EFT). Pesada/baixa-ergo = lenta e dramática. Leve/alta-ergo = rápida e sutil. |
| Advanced ADS Transition Stat Intensity | Intensidade dos Stats na Transição de Mira | float | `1` | 0 a 2 | **(Avançado)** | Quão fortemente os stats da arma afetam o encaixe ao mirar. 0 = sem escala (todas as armas iguais), 1 = normal, 2 = diferença exagerada entre armas leves/pesadas. |
| Advanced Stance Transition Stat Intensity | Intensidade dos Stats na Transição de Postura | float | `1` | 0.01 a 2 | **(Avançado)** | Quão fortemente o peso/ergonomia afeta a velocidade da transição de postura e o encaixe. 0.01 = efeito mínimo, 1 = normal, 2 = exagerado. Funciona quando 'Aplicar Também na Transição de Postura' está habilitado. |
| Shoulder Throw Forward Amount | Distância do Empurrão Frontal | float | `0.02` | 0 a 0.3 | **(Avançado)** | Distância base do arremesso para frente. Com 'Escalar por Stats da Arma' habilitado, é multiplicada pelo inverso de AimingSpeed (armas pesadas arremessam mais). |
| Shoulder Throw Up Amount | Deslocamento Vertical do Encaixe | float | `-0.015` | -0.15 a 0.15 | **(Avançado)** | Offset vertical base durante o arremesso. Negativo = para baixo. Com 'Escalar por Stats da Arma', escala com o inverso de AimingSpeed. |
| Shoulder Throw Yaw | Yaw do Encaixe | float | `6` | -15 a 15 | **(Avançado)** | Rotação Yaw durante a fase de arremesso (graus). Positivo = girar à direita. Aplicado tanto às transições de mira quanto às de postura. |
| Shoulder Throw Pitch | Pitch do Encaixe | float | `-3` | -15 a 15 | **(Avançado)** | Rotação Pitch durante a fase de arremesso (graus). Positivo = girar para cima. Aplicado tanto às transições de mira quanto às de postura. |
| Shoulder Throw Roll | Roll do Encaixe | float | `-1.5` | -15 a 15 | **(Avançado)** | Rotação Roll durante a fase de arremesso (graus). Positivo = inclinar à direita. Aplicado tanto às transições de mira quanto às de postura. |
| Shoulder Throw Speed | Velocidade do Empurrão | float | `2` | 0.5 a 5 | **(Avançado)** | Velocidade base do movimento de arremesso. Com 'Escalar por Stats da Arma', multiplicada por AimingSpeed (armas leves = mais rápido). |
| Shoulder Settle Speed | Velocidade de Acomodação | float | `1.5` | 0.5 a 5 | **(Avançado)** | Velocidade base de acomodação na mira. Com 'Escalar por Stats da Arma', multiplicada por AimingSpeed. |
| Shoulder Throw Duration | Duração do Empurrão | float | `0.15` | 0.01 a 0.5 | **(Avançado)** | Duração base da fase de arremesso (segundos). Com 'Escalar por Stats da Arma', armas pesadas têm duração mais longa. |

## Valores Padrão da Mira (`ADS Default Values`) — todos **(Avançado)**

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Reset Positions When Aiming | Resetar Posições ao Mirar | bool | `true` | — | Quando habilitado, transiciona suavemente todas as posições de volta aos padrões ao mirar |
| ADS Hands Pitch Rotation | Rotação Pitch das Mãos ao Mirar | float | `0` | -45 a 45 | Rotação Pitch das mãos (eixo X) ao mirar com 'Resetar ao Mirar' habilitado. 0 = posição padrão do jogo |
| ADS Hands Yaw Rotation | Rotação Yaw das Mãos ao Mirar | float | `0` | -45 a 45 | Rotação Yaw das mãos (eixo Y) ao mirar com 'Resetar ao Mirar' habilitado. 0 = posição padrão do jogo |
| ADS Hands Roll Rotation | Rotação Roll das Mãos ao Mirar | float | `0` | -45 a 45 | Rotação Roll das mãos (eixo Z) ao mirar com 'Resetar ao Mirar' habilitado. 0 = posição padrão do jogo |
| ADS Hands Forward/Backward Offset | Offset Frente/Trás das Mãos ao Mirar | float | `0` | -0.5 a 0.5 | Posição das mãos para frente/trás (eixo Z) ao mirar. Padrão é 0.04 |
| ADS Hands Up/Down Offset | Offset Cima/Baixo das Mãos ao Mirar | float | `0` | -0.5 a 0.5 | Posição das mãos para cima/baixo (eixo Y) ao mirar. Padrão é 0.04 |
| ADS Hands Sideways Offset | Offset Lateral das Mãos ao Mirar | float | `0` | -0.5 a 0.5 | Posição das mãos para esquerda/direita (eixo X) ao mirar. Padrão é 0.04 |

## Posições Padrão de Mãos/Braços (`Default Hands/Arms Positions`) — todos **(Avançado)**

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Default Hands/Arms Position | Habilitar Posição Padrão de Mãos/Braços | bool | `false` | — | Habilita ou desabilita os offsets padrão de posição de mãos/braços quando NÃO estiver em postura |
| Default Hands Forward/Backward Offset | Offset Frente/Trás Padrão | float | `0` | -0.5 a 0.5 | Posição padrão das mãos/arma para frente/trás (positivo = frente). Esta é sua posição normal de hip-fire. |
| Default Hands Up/Down Offset | Offset Cima/Baixo Padrão | float | `0` | -0.5 a 0.5 | Posição padrão das mãos/arma para cima/baixo (positivo = cima). Esta é sua posição normal de hip-fire. |
| Default Hands Sideways Offset | Offset Lateral Padrão | float | `0` | -0.5 a 0.5 | Posição padrão das mãos/arma para esquerda/direita (positivo = direita). Esta é sua posição normal de hip-fire. |

## Postura 0 — Pronto de Tiro (`Stance 0`)

> Stance vanilla (arma à frente). Não tem offsets de mãos próprios — só configurações de stamina/velocidade do backlog 001.

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Stance 0 Stamina Mode | Modo de Stamina — Postura 0 | enum (`None`/`Drain`/`Recovery`) | `Drain` | — | Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire. |
| Stance 0 Stamina Intensity | Intensidade de Stamina — Postura 0 | float | `0.50` | 0.0 a 2.0 (Avançado) | Multiplicador de intensidade do efeito (drain ou recovery). 0.25=muito baixo · 0.50=baixo · 1.00=normal · 1.50=alto · 2.00=muito alto. Sem efeito se Mode = None. |
| Stance 0 Modifies Movement Speed | Modifica Velocidade — Postura 0 | bool | `true` | — | Quando habilitado, esta stance aplica um redutor à velocidade de movimentação. |
| Stance 0 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 0 | int (%) | `90` | 50 a 100 (Avançado) | Redutor de velocidade em %. 50 = metade da velocidade · 75 = um pouco mais lento · 100 = sem redução. Apenas redução (limitação do sistema de speed limits do EFT). |
| Stance 0 Apply When Prone | Aplicar em Prone — Postura 0 | bool | `false` | — (Avançado) | Aplicar esta stance (offsets, drain/recovery e redutor de velocidade) também quando o personagem está deitado. Desligado por padrão porque pode conflitar com as animações nativas de prone. |

## Postura 1 (`Stance 1`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 1 Sprint Animation | Habilitar Animação de Sprint da Postura 1 | bool | `true` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 1 (estilo tac sprint) |
| Stance 1 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 1 | float | `-15` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 1 em graus (inclinação cima/baixo) |
| Stance 1 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 1 | float | `-15` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 1 em graus (giro esquerda/direita) |
| Stance 1 Hands Roll (Z-Axis) | Roll das Mãos — Postura 1 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 1 em graus (inclinação/cant da arma) |
| Stance 1 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 1 | float | `-0.15` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para frente/trás (positivo = frente) |
| Stance 1 Hands Up/Down Offset | Offset Cima/Baixo — Postura 1 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para cima/baixo (positivo = cima) |
| Stance 1 Hands Sideways Offset | Offset Lateral — Postura 1 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para esquerda/direita (positivo = direita) |
| Stance 1 Stamina Mode | Modo de Stamina — Postura 1 | enum (`None`/`Drain`/`Recovery`) | `Recovery` | — | Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire. |
| Stance 1 Stamina Intensity | Intensidade de Stamina — Postura 1 | float | `2.00` | 0.0 a 2.0 (Avançado) | Multiplicador de intensidade do efeito. 0.25=muito baixo · 0.50=baixo · 1.00=normal · 1.50=alto · 2.00=muito alto. Sem efeito se Mode = None. |
| Stance 1 Modifies Movement Speed | Modifica Velocidade — Postura 1 | bool | `true` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 1 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 1 | int (%) | `100` | 50 a 100 (Avançado) | Redutor de velocidade em %. 100 = sem redução. |
| Stance 1 Apply When Prone | Aplicar em Prone — Postura 1 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |

## Postura 2 (`Stance 2`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 2 Sprint Animation | Habilitar Animação de Sprint da Postura 2 | bool | `false` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 2 (estilo tac sprint) |
| Stance 2 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 2 | float | `0` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 2 em graus (inclinação cima/baixo) |
| Stance 2 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 2 | float | `-30` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 2 em graus (giro esquerda/direita) |
| Stance 2 Hands Roll (Z-Axis) | Roll das Mãos — Postura 2 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 2 em graus (inclinação/cant da arma) |
| Stance 2 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 2 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para frente/trás (positivo = frente) |
| Stance 2 Hands Up/Down Offset | Offset Cima/Baixo — Postura 2 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para cima/baixo (positivo = cima) |
| Stance 2 Hands Sideways Offset | Offset Lateral — Postura 2 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para esquerda/direita (positivo = direita) |
| Stance 2 Stamina Mode | Modo de Stamina — Postura 2 | enum (`None`/`Drain`/`Recovery`) | `None` | — | Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire. |
| Stance 2 Stamina Intensity | Intensidade de Stamina — Postura 2 | float | `1.00` | 0.0 a 2.0 (Avançado) | Multiplicador de intensidade do efeito. Sem efeito se Mode = None. |
| Stance 2 Modifies Movement Speed | Modifica Velocidade — Postura 2 | bool | `false` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 2 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 2 | int (%) | `100` | 50 a 100 (Avançado) | Redutor de velocidade em %. 100 = sem redução. |
| Stance 2 Apply When Prone | Aplicar em Prone — Postura 2 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |

## Postura 3 (`Stance 3`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 3 Sprint Animation | Habilitar Animação de Sprint da Postura 3 | bool | `false` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 3 (estilo tac sprint) |
| Stance 3 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 3 | float | `30` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 3 em graus (inclinação cima/baixo) |
| Stance 3 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 3 | float | `0` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 3 em graus (giro esquerda/direita) |
| Stance 3 Hands Roll (Z-Axis) | Roll das Mãos — Postura 3 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 3 em graus (inclinação/cant da arma) |
| Stance 3 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 3 | float | `0.03` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para frente/trás (positivo = frente) |
| Stance 3 Hands Up/Down Offset | Offset Cima/Baixo — Postura 3 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para cima/baixo (positivo = cima) |
| Stance 3 Hands Sideways Offset | Offset Lateral — Postura 3 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para esquerda/direita (positivo = direita) |
| Stance 3 Stamina Mode | Modo de Stamina — Postura 3 | enum (`None`/`Drain`/`Recovery`) | `Recovery` | — | Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire. |
| Stance 3 Stamina Intensity | Intensidade de Stamina — Postura 3 | float | `1.50` | 0.0 a 2.0 (Avançado) | Multiplicador de intensidade do efeito. Sem efeito se Mode = None. |
| Stance 3 Modifies Movement Speed | Modifica Velocidade — Postura 3 | bool | `true` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 3 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 3 | int (%) | `95` | 50 a 100 (Avançado) | Redutor de velocidade em %. 95 = leve redução. |
| Stance 3 Apply When Prone | Aplicar em Prone — Postura 3 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |

## Configurações de Tac Sprint (`Tac Sprint Settings`) — todos **(Avançado)**

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Tac Sprint Weight Limit | Peso Máximo p/ Tac Sprint | float | `5.1` | 1 a 15 | Peso máximo da arma (kg) para permitir a animação de tac sprint. Padrão: 5.1kg |
| Tac Sprint Weight Limit (Bullpup) | Peso Máximo Bullpup | float | `5.75` | 1 a 15 | Peso máximo da arma (kg) para armas bullpup permitirem tac sprint. Bullpups recebem um limite mais alto. Padrão: 5.75kg |
| Tac Sprint Length Limit | Tamanho Máximo p/ Tac Sprint | int | `6` | 1 a 10 | Tamanho máximo da arma (células de inventário) para permitir a animação de tac sprint. Padrão: 6 células |
| Tac Sprint Ergo Limit | Ergonomia Mínima p/ Tac Sprint | float | `35` | 0 a 100 | Ergonomia mínima da arma para permitir a animação de tac sprint. Padrão: 35 |
| Tac Sprint Reset Delay | Atraso para Resetar Tac Sprint | float | `0.35` | 0 a 1 | Atraso (segundos) após o sprint terminar antes da arma voltar ao tamanho normal. 0 = imediato. Previne o snap-back abrupto. |

## Campo de Visão (`Field of View`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Expanded FOV Range | Habilitar FOV Estendido | bool | `false` | — | Permite estender o slider de FOV além da faixa padrão de 50-75 |
| Minimum FOV | FOV Mínimo | int | `20` | 1 a 50 | Valor mínimo de FOV. O mínimo padrão do jogo é 50 |
| Maximum FOV | FOV Máximo | int | `150` | 75 a 170 | Valor máximo de FOV. O máximo padrão do jogo é 75 |
