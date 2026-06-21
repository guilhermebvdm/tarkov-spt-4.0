# Propriedades do F12 — stancesAndCameraPositionSPT4.0.11

Plugin BepInEx: `shwng.camerarotation` — *shwng.FpsCameraStances v1.1.0*

Lista das 90 propriedades expostas no menu do BepInEx ConfigurationManager (F12), traduzidas para pt-BR. Itens marcados com **(Avançado)** só aparecem quando "Advanced settings" está habilitado no F12. A coluna **Tooltip (pt-BR)** é a tradução fiel do `ConfigDescription` que aparece ao passar o mouse sobre a propriedade.

Fonte: [modded/Plugin.cs](modded/Plugin.cs) (63 originais + 16 do backlog `001-stamina-e-velocidade` + **11 novas do backlog `002-ciclo-linear-hotkeys-snap-fogo` − 1 removida** = `Use Only Stances` substituída por `Include Stance 0 - Vanilla in Cycle` + **1 nova do code-review 002 round 01** = `Snap Stale Timeout (s)`).

> ⚠️ **Breaking change (backlog 002):** as seções de stance no F12 foram renomeadas para refletir os eixos reais — `Stance 1 - Ready Up` → `Stance 1 - High Ready`, `Stance 2 - Ready Down` → `Stance 2 - Custom`, `Stance 3 - Custom` → `Stance 3 - Low Ready`. BepInEx casa entradas por `(section, key)`, então **valores customizados em `BepInEx/config/shwng.camerarotation.cfg` serão recriados com defaults**. Para preservar configurações antigas: copiar manualmente os valores das seções antigas para as novas no `.cfg` antes do primeiro boot pós-update.
>
> ⚠️ **Breaking change adicional (002-06-fix-01):** Stance 2 e Stance 3 trocaram de papel — agora **Stance 2 = Low Ready** (Pitch +30, cano desce), **Stance 3 = Custom** (Yaw -30, lateral). Valores no `.cfg` para seções `Stance 2 - Custom` e `Stance 3 - Low Ready` (nomes anteriores) ficam órfãos após boot; entries em `Stance 2 - Low Ready` e `Stance 3 - Custom` são recriadas com defaults. Migração manual no `.cfg` se quiser preservar customizações.

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
| Include Stance 0 - Vanilla in Cycle | Incluir Postura 0 - Vanilla no Ciclo | bool | `false` | — | Quando habilitado, a Postura 0 - Vanilla é incluída no ciclo. Substitui a antiga `Use Only Stances` (lógica invertida, nome mais claro). Afeta sempre a tecla V; afeta o scroll só em modo Cycle. |
| Enable Stance 1 - High Ready in Cycle | Incluir Postura 1 - High Ready no Ciclo | bool | `true` | — | Quando habilitado, a Postura 1 é incluída no ciclo de posturas. Quando desabilitado, é pulada. |
| Enable Stance 2 - Custom in Cycle | Incluir Postura 2 - Custom no Ciclo | bool | `true` | — | Quando habilitado, a Postura 2 é incluída no ciclo de posturas. Quando desabilitado, é pulada. |
| Enable Stance 3 - Low Ready in Cycle | Incluir Postura 3 - Low Ready no Ciclo | bool | `true` | — | Quando habilitado, a Postura 3 é incluída no ciclo de posturas. Quando desabilitado, é pulada. |
| Stance Toggle Hotkey | Tecla para Trocar Postura | KeyCode | `V` | — | Pressione esta tecla para ciclar pelas posturas habilitadas: Padrão → Postura 1 → Postura 2 → Postura 3 → Padrão |
| Enable Mouse Wheel Stance Cycle | Habilitar Ciclo via Roda do Mouse | bool | `false` | — | Quando habilitado, segure a tecla modificadora e role a roda do mouse para ciclar entre posturas |
| Mouse Wheel Modifier Key | Tecla Modificadora da Roda | KeyCode | `LeftAlt` | — | Segure esta tecla enquanto rola a roda do mouse para ciclar entre posturas (quando o ciclo via roda está habilitado) |
| Mouse Wheel Scroll Mode | Modo de Scroll da Roda | enum | `Linear` | Cycle / Linear | **Cycle**: ciclo circular respeitando os toggles de stance. **Linear**: eixo fixo Stance 1 (topo) ↔ Stance 0 (centro) ↔ Stance 2 (fundo); Stance 3 fica off-axis (só via tecla dedicada). Visível apenas com `Enable Mouse Wheel Stance Cycle` ativo. |
| Stance Transition Speed | Velocidade de Transição de Postura | float | `1` | 0.5 a 20 | Quão rápido as mãos transicionam entre Padrão e Postura. Maior = mais rápido/abrupto, Menor = mais lento/suave. Recomendado: 3-10 |
| ADS Transition Speed | Velocidade de Transição da Mira | float | `1` | 0.5 a 5 | Quão rápido as mãos transicionam entre as posições de postura e mira. 1 = lento, 2 = normal, 3+ = rápido/abrupto. |
| Stance Change Sound Volume | Volume do Som ao Trocar Postura | float | `1` | 0 a 2 | Multiplicador de volume do som de chacoalho ao trocar de postura. 0 = mudo, 1 = normal, 2 = mais alto. |
| Stance 0 - Vanilla Hotkey | Tecla Dedicada — Postura 0 | KeyCode | `None` | — | Tecla dedicada para retornar à Postura 0 - Vanilla. Pressionar quando já em Stance 0 não faz nada. Bloqueada durante sprint, ignorada em ADS. |
| Stance 1 - High Ready Hotkey | Tecla Dedicada — Postura 1 | KeyCode | `None` | — | Tecla dedicada para ativar Stance 1 - High Ready. Toggle: pressionar quando já ativa retorna a Stance 0. Bloqueada durante sprint, ignorada em ADS. |
| Stance 2 - Low Ready Hotkey | Tecla Dedicada — Postura 2 | KeyCode | `None` | — | Tecla dedicada para ativar Stance 2 - Low Ready. Toggle: pressionar quando já ativa retorna a Stance 0. Bloqueada durante sprint, ignorada em ADS. |
| Stance 3 - Custom Hotkey | Tecla Dedicada — Postura 3 | KeyCode | `O` | — | Tecla dedicada para ativar Stance 3 - Custom. Toggle: pressionar quando já ativa retorna a Stance 0. Bloqueada durante sprint, ignorada em ADS. |
| Snap Fire Threshold (ms) | Limiar de Snap-Fire (ms) | int | `200` | 50 a 500 (Avançado) | Tempo máximo (ms) entre apertar e soltar o gatilho para classificar como clique único. Clique único = snap para Stance 0 sem disparo. Pressão maior = snap + 1 tiro natural. |
| Snap Stale Timeout (s) | Timeout do Snap Stale (s) | float | `2` | 0.5 a 10 (Avançado) | Tempo máximo (segundos) que o intercept do snap permanece ativo sem button-up antes de ser limpo automaticamente. Valores menores reduzem risco de estado stale em weapon swap durante hold. Default 2s é seguro. |
| Start In Low Ready On Raid Begin | Iniciar em Low Ready ao Entrar em Raid | bool | `true` | — | Quando habilitado, o jogador inicia toda raid já em Stance 3 - Low Ready, sem animação de transição. Aplica mesmo se Stance 3 estiver fora do ciclo. |

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
| Stance 0 Stamina Multiplier | Multiplicador de Stamina — Postura 0 | float | `0.5` | 0.0 a 10.0 | Controla o comportamento da stamina. < 1.0 = drain (ex: 0.5 = drena na metade da taxa de mira). 1.0 = vanilla, sem efeito. > 1.0 = recovery acelerado (ex: 2.0 = recupera na taxa de drain de mira). |
| Stance 0 Modifies Movement Speed | Modifica Velocidade — Postura 0 | bool | `true` | — | Quando habilitado, esta stance aplica um redutor à velocidade de movimentação. |
| Stance 0 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 0 | int (%) | `90` | 50 a 100 (Avançado) | Redutor de velocidade em %. 50 = metade da velocidade · 75 = um pouco mais lento · 100 = sem redução. Apenas redução (limitação do sistema de speed limits do EFT). |
| Stance 0 Apply When Prone | Aplicar em Prone — Postura 0 | bool | `false` | — (Avançado) | Aplicar esta stance (offsets, drain/recovery e redutor de velocidade) também quando o personagem está deitado. Desligado por padrão porque pode conflitar com as animações nativas de prone. |

## Postura 1 — High Ready (`Stance 1 - High Ready`)

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 1 Sprint Animation | Habilitar Animação de Sprint da Postura 1 | bool | `true` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 1 (estilo tac sprint) |
| Stance 1 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 1 | float | `-15` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 1 em graus (inclinação cima/baixo) |
| Stance 1 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 1 | float | `-15` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 1 em graus (giro esquerda/direita) |
| Stance 1 Hands Roll (Z-Axis) | Roll das Mãos — Postura 1 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 1 em graus (inclinação/cant da arma) |
| Stance 1 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 1 | float | `-0.15` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para frente/trás (positivo = frente) |
| Stance 1 Hands Up/Down Offset | Offset Cima/Baixo — Postura 1 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para cima/baixo (positivo = cima) |
| Stance 1 Hands Sideways Offset | Offset Lateral — Postura 1 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 1 para esquerda/direita (positivo = direita) |
| Stance 1 Stamina Multiplier | Multiplicador de Stamina — Postura 1 | float | `1.5` | 0.0 a 10.0 | Controla o comportamento da stamina. < 1.0 = drain. 1.0 = vanilla, sem efeito. > 1.0 = recovery acelerado (ex: 1.5 = recupera na metade da taxa de drain de mira). |
| Stance 1 Modifies Movement Speed | Modifica Velocidade — Postura 1 | bool | `true` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 1 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 1 | int (%) | `95` | 50 a 100 (Avançado) | Redutor de velocidade em %. 100 = sem redução. |
| Stance 1 Apply When Prone | Aplicar em Prone — Postura 1 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |
| Stance 1 Snap to Stance 0 on Fire | Snap para Postura 0 ao Atirar — Postura 1 | bool | `true` | — | Quando habilitado, atirar em Stance 1 faz snap automático para Stance 0. Clique único (< limiar) = sem tiro. Hold (>= limiar) = snap + 1 tiro. Não atua em ADS nem com arma branca/granada. |

## Postura 2 — Low Ready (`Stance 2 - Low Ready`)

> 06-fix-01: Stance 2 trocou de papel — agora é **Low Ready** (Pitch +30°, cano desce). Posição relaxada / pré-mira. Era "Custom" no item 002 original.

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 2 Sprint Animation | Habilitar Animação de Sprint da Postura 2 | bool | `false` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 2 (estilo tac sprint) |
| Stance 2 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 2 | float | `30` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 2 em graus (Low Ready: cano desce). |
| Stance 2 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 2 | float | `0` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 2 em graus (giro esquerda/direita) |
| Stance 2 Hands Roll (Z-Axis) | Roll das Mãos — Postura 2 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 2 em graus (inclinação/cant da arma) |
| Stance 2 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 2 | float | `0.03` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para frente/trás (positivo = frente). Low Ready: leve push forward. |
| Stance 2 Hands Up/Down Offset | Offset Cima/Baixo — Postura 2 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para cima/baixo (positivo = cima) |
| Stance 2 Hands Sideways Offset | Offset Lateral — Postura 2 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 2 para esquerda/direita (positivo = direita) |
| Stance 2 Stamina Multiplier | Multiplicador de Stamina — Postura 2 | float | `1.0` | 0.0 a 10.0 | Controla o comportamento da stamina. Low Ready: padrão vanilla (1.0 = sem efeito; ajuste para drain ou recovery). |
| Stance 2 Modifies Movement Speed | Modifica Velocidade — Postura 2 | bool | `true` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 2 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 2 | int (%) | `90` | 50 a 100 (Avançado) | Redutor de velocidade em %. 90 = leve redução (Low Ready). |
| Stance 2 Apply When Prone | Aplicar em Prone — Postura 2 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |
| Stance 2 Snap to Stance 0 on Fire | Snap para Postura 0 ao Atirar — Postura 2 | bool | `false` | — | Quando habilitado, atirar em Stance 2 faz snap automático para Stance 0. Padrão **false** porque Low Ready é tipicamente usado para pré-mira (não convém quebrar). Clique único (< limiar) = sem tiro. Hold (>= limiar) = snap + 1 tiro. |

## Postura 3 — Custom (`Stance 3 - Custom`)

> 06-fix-01: Stance 3 trocou de papel — agora é **Custom** (Yaw -30°, lateral, off-axis no scroll Linear). Era "Low Ready" no item 002 original.

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Stance 3 Sprint Animation | Habilitar Animação de Sprint da Postura 3 | bool | `false` | — | Quando habilitado, usa uma animação compacta de sprint ao correr na Postura 3 (estilo tac sprint) |
| Stance 3 Hands Pitch (X-Axis) | Pitch das Mãos — Postura 3 | float | `0` | -45 a 45 | Rotação Pitch de mãos/braços da Postura 3 em graus (Custom: padrão sem pitch). |
| Stance 3 Hands Yaw (Y-Axis) | Yaw das Mãos — Postura 3 | float | `-30` | -45 a 45 | Rotação Yaw de mãos/braços da Postura 3 em graus (Custom: lateral à esquerda). |
| Stance 3 Hands Roll (Z-Axis) | Roll das Mãos — Postura 3 | float | `0` | -45 a 45 | Rotação Roll de mãos/braços da Postura 3 em graus (inclinação/cant da arma) |
| Stance 3 Hands Forward/Backward Offset | Offset Frente/Trás — Postura 3 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para frente/trás (positivo = frente). Custom: padrão sem deslocamento. |
| Stance 3 Hands Up/Down Offset | Offset Cima/Baixo — Postura 3 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para cima/baixo (positivo = cima) |
| Stance 3 Hands Sideways Offset | Offset Lateral — Postura 3 | float | `0` | -0.5 a 0.5 | Posição de mãos/arma da Postura 3 para esquerda/direita (positivo = direita) |
| Stance 3 Stamina Multiplier | Multiplicador de Stamina — Postura 3 | float | `2.0` | 0.0 a 10.0 | Controla o comportamento da stamina. Custom: padrão `2.0` (recovery acelerado — taxa de drain de mira). |
| Stance 3 Modifies Movement Speed | Modifica Velocidade — Postura 3 | bool | `true` | — | Quando habilitado, aplica um redutor à velocidade de movimentação. |
| Stance 3 Movement Speed Multiplier | Multiplicador de Velocidade — Postura 3 | int (%) | `100` | 50 a 100 (Avançado) | Redutor de velocidade em %. 100 = sem redução (Custom: sem cap). |
| Stance 3 Apply When Prone | Aplicar em Prone — Postura 3 | bool | `false` | — (Avançado) | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |
| Stance 3 Snap to Stance 0 on Fire | Snap para Postura 0 ao Atirar — Postura 3 | bool | `true` | — | Quando habilitado, atirar em Stance 3 faz snap automático para Stance 0. Clique único (< limiar) = sem tiro. Hold (>= limiar) = snap + 1 tiro. Não atua em ADS nem com arma branca/granada. |

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

## Apoio Passivo de Arma (`Weapon Mount (Passive)`) — Item 011

| Propriedade (EN) | Tradução | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Passive Mount | Habilitar Apoio Passivo | bool | `true` | — | Liga o apoio passivo: ao encostar a arma numa superfície (sem a tecla de mount) você ganha um benefício leve de estabilidade. Desligado = só o mount nativo do jogo. |
| Passive Recoil Multiplier | Multiplicador de Recuo (Passivo) | float | `0.7` | 0.1 a 1.0 | Multiplicador de recuo enquanto apoiado (passivo). 0.7 = 30% menos recuo. Deve ser MAIOR que o do mount ativo (vanilla) — o passivo é mais fraco. |
| Passive Sway Multiplier | Multiplicador de Sway (Passivo) | float | `0.65` | 0.0 a 1.0 | Multiplicador de sway (respiração) enquanto apoiado. 0.65 = 35% menos sway. |
| Passive Stamina Save | Economia de Estamina (Passivo) | bool | `true` | — | Enquanto apoiado, pausa/reduz o drain de stamina de braço (mais fraco que o mount nativo). |
| Active Mount Stamina Regen | Recuperação de Estamina (Ativo) | float | `5.0` | 0 a 20 | Taxa de recuperação de stamina de braço no mount ATIVO (vanilla), em hipfire. Em ADS usa a taxa do passivo (recupera leve). 0 = não recupera. |
| Passive Mount Stamina Regen | Recuperação de Estamina (Passivo) | float | `2.5` | 0 a 20 | Taxa de recuperação no mount PASSIVO em hipfire (e no ativo em ADS). No passivo + ADS a stamina fica parada (segura, sem recuperar). Deve ser MENOR que a do ativo. |
| Show Mount Icon | Mostrar Ícone de Apoio | bool | `true` | — | Mostra o ícone direcional (esquerda/direita/baixo) no canto inferior direito quando o apoio passivo está ativo. |
