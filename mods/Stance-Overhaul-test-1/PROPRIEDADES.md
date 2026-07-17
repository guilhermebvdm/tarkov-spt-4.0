# Propriedades F12 — Fontaine-StanceOverhaul

> **Plugin:** `com.fontaine.stanceoverhaul` · Fontaine-StanceOverhaul · v1.0.0
> **Fonte:** [original/src/PluginConfig.cs](original/src/PluginConfig.cs) (método `InitConfigBindings`)
> **Dependência:** RealismCommonLib (`BepInDependency` obrigatória — o plugin não carrega sem ela)
> **Nota:** itens marcados **(Avançado)** (coluna "Avançado" = ✓) só aparecem no F12 com a opção "Advanced settings" do ConfigurationManager ligada.
> **Total:** 10 seções · 180 opções

As propriedades de cada seção estão ordenadas por `Order` decrescente (maior `Order` aparece primeiro no F12).

---

### 0. Dev.

20 knobs de desenvolvimento sem descrição — condensados numa linha:

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| test 1 … test 20 | teste 1 … teste 20 | float | 1.0 | -5000–5000 | ✓ | — (knobs de desenvolvimento sem descrição; Order de 170 a -10, decrescendo de 10 em 10) |

### 1. Weapon Stances And Position.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| De-Jank EFT Animations | Suavizar animações do EFT | bool | true | — | — | Tenta deixar certas animações do EFT menos travadas, como as animações de inventário e de portas. |
| Override Collision | Substituir sistema de colisão | bool | true | — | — | Se o FOV Fix estiver instalado, substitui completamente o sistema de colisão da BSG para funcionar bem com stances e posições alternativas de arma. Se não estiver instalado, modifica o sistema de colisão da BSG. |
| Use Realism Mounting System | Usar sistema de apoio (mounting) do Realism | bool | true | — | — | Substitui o sistema de mounting da BSG pelo do Realism (que foi implementado primeiro). As mecânicas de recuo, stance e balanço (sway) foram todas construídas em torno do mounting do Realism e não funcionarão corretamente com o da BSG. |
| Enable Extra Weapon Position/Rotation Effects | Habilitar efeitos extras de posição/rotação da arma | bool | true | — | — | A arma fica levemente inclinada com base na ergonomia. ADS com máscara de gás/protetor facial fica inclinado. A inclinação da arma aumenta ao agachar, e ela se aproxima de você. Outros efeitos sutis. |
| Remember Stance After Using Item | Lembrar stance após usar item | bool | true | — | — | Lembra a stance após ações (uso de itens). |
| Remember Stance After Firing | Lembrar stance após atirar | bool | true | — | — | Lembra a stance após atirar, se o jogador estava mirando. |
| Block Shooting While In Stance | Bloquear disparo em stance | bool | false | — | — | Bloqueia o disparo enquanto estiver em uma stance; cancela a stance ao tentar atirar. |
| Enable Tactical Sprint Animation | Habilitar animação de sprint tático | bool | true | — | — | Habilita o uso da animação de sprint tático ao correr a partir da posição High Ready. |
| Tactical Sprint Sprint Speed Bonus | Bônus de velocidade do sprint tático | float | 1.15 | 0.1–5 | — | Bônus de velocidade de sprint ao correr a partir da posição High Ready. |
| Tactical Sprint Sprint Acceleration Bonus | Bônus de aceleração do sprint tático | float | 1.37 | 0.1–5 | — | Bônus de aceleração de sprint ao correr a partir da posição High Ready. |
| Enable Alternative Pistol Position And ADS | Habilitar posição e ADS alternativos de pistola | bool | true | — | — | A pistola será segurada centralizada e em postura comprimida. O ADS é animado. Se o FOV Fix estiver em uso, a arma se move até a câmera para um ADS mais suave. |
| Enable Alternative Rifle Position And ADS | Habilitar posição e ADS alternativos de fuzil | bool | true | — | — | A posição do fuzil fica mais centralizada. Se o FOV Fix estiver em uso, a arma se move até a câmera para um ADS mais suave. |
| Enable Alternative Rifle Recoil Override | Habilitar override de recuo do fuzil alternativo | bool | true | — | — | Ao usar o fuzil alternativo, permite que ele sobrescreva o recuo. Resulta em sensação de recuo diferente, mas com transição mais suave do estado de disparo para o ADS sem disparo. |
| Enable Idle Arm Stamina Drain | Habilitar drenagem de stamina dos braços em idle | bool | true | — | — | A stamina dos braços drena quando não se está em uma stance (High e Low Ready, Short-Stocking). |
| Idle Stam Drain Modifer | Modificador de drenagem de stamina em idle | float | 0.1 | 0–5 | — | — |
| Allow Reload From Active Aim | Permitir recarga no Active Aim | bool | false | — | — | Permite recarregar o carregador durante o Active Aim, com bônus de velocidade. |
| Enable Stance Stamina And Movement Effects | Habilitar efeitos de stamina e movimento das stances | bool | true | — | — | Habilita que stances e mounting afetem stamina e velocidade de movimento. A drenagem de stamina pode não funcionar corretamente se desabilitado. High + Low Ready, Short-Stocking e pistola em idle regeneram stamina mais rápido e, opcionalmente, ficar em idle com fuzis drena stamina. High Ready tem velocidade e aceleração de sprint maiores; Low Ready tem aceleração de sprint maior. A stamina dos braços não drena a stamina normal se chegar a 0. |
| Enable Mounting UI | Habilitar UI de mounting | bool | true | — | — | Se habilitado, um ícone na tela indica se o jogador está apoiado (bracing), montado (mounting) e de que lado da cobertura está. |
| Stance Sfx Volume Modifier | Modificador de volume dos SFX de stance | float | 2 | 0.1–20 | — | Modificador do volume do barulho de equipamento ao fazer ações relacionadas a stance. |
| Rifle Position Offset | Deslocamento da posição do fuzil | Vector3 | (-0.04, -0.015, 0) | — | ✓ | Requer a opção 'alt rifle'. Ajusta a posição inicial do fuzil na tela. |
| Stance Blend Speed | Velocidade de blend das stances | float | 18 | 0.1–100 | ✓ | Velocidade das transições de blending de stance. |
| Global Stance Speed | Velocidade global das stances | float | 1 | 0.1–10 | ✓ | Multiplicador global para todas as velocidades de stance. |
| Enable NVG Aim Block | Habilitar bloqueio de mira com NVG | bool | true | — | — | Ópticas com magnificação bloqueiam o ADS ao usar NVGs. |
| Enable Thermal Aim Block | Habilitar bloqueio de mira com térmico | bool | true | — | — | Não é possível mirar pelas miras ao usar óculos térmicos. |
| Enable Faceshield Aim Block | Habilitar bloqueio de mira com protetor facial | bool | true | — | — | Protetores faciais bloqueiam o ADS, a menos que a combinação específica de coronha/arma/protetor facial permita. |

### 2. Weapon Stances Keybinds.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Keybind To Use With Mouse Wheel | Tecla para usar com a roda do mouse | KeyboardShortcut | LeftAlt | — | — | Tecla usada em combinação com a roda do mouse, se habilitado. |
| Require Key + Mouse Wheel | Exigir tecla + roda do mouse | bool | true | — | — | Exige tecla configurada + roda do mouse para trocar de stance. |
| Enable Mouse Wheel Stance Switching | Habilitar troca de stance pela roda do mouse | bool | true | — | — | Alterna entre High e Low Ready pela roda do mouse. |
| Melee Keybind | Tecla de ataque corpo a corpo | KeyboardShortcut | None | — | — | Golpeia com o cano ou a baioneta da arma equipada. |

### 3. Device Bonuses.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| NVG IR Laser Bonus | Bônus de laser IR com NVG | float | 0.5 | 0.1–2 | — | Multiplicador de imprecisão do hipfire quando o NVG está ativo com laser IR ou laser visível. |
| NVG IR Light With Laser Bonus | Bônus de lanterna IR + laser com NVG | float | 0.4 | 0.1–2 | — | Multiplicador de imprecisão do hipfire quando o NVG está ativo com lanterna IR e laser. |
| NVG IR Light Bonus | Bônus de lanterna IR com NVG | float | 0.6 | 0.1–2 | — | Multiplicador de imprecisão do hipfire quando o NVG está ativo somente com lanterna IR. |
| NVG White Light Bonus | Bônus de luz branca com NVG | float | 0.95 | 0.1–2 | — | Multiplicador de imprecisão do hipfire quando o NVG está ativo com luz branca. |
| Thermal Goggles Debuff | Penalidade de óculos térmicos | float | 1.15 | 0.1–2 | — | Multiplicador de imprecisão do hipfire quando a visão térmica está ativa. |
| Normal Visible Laser Bonus | Bônus de laser visível (modo normal) | float | 0.5 | 0.1–2 | — | Multiplicador de imprecisão do hipfire com laser visível no modo normal (sem NVG/térmico). |
| Normal White Light With Laser Bonus | Bônus de luz branca + laser (modo normal) | float | 0.4 | 0.1–2 | — | Multiplicador de imprecisão do hipfire com luz branca e laser no modo normal. |
| Normal White Light Bonus | Bônus de luz branca (modo normal) | float | 0.6 | 0.1–2 | — | Multiplicador de imprecisão do hipfire somente com luz branca no modo normal. |

### 4. Active Aim.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Active Aim Keybind | Tecla do Active Aim | KeyboardShortcut | Mouse4 | — | — | Inclina a arma lateralmente, melhorando a precisão do hipfire. |
| Use Toggle For Active Aim | Usar modo alternável (toggle) para o Active Aim | bool | false | — | — | — |
| Active Aim Walk Speed Modifier | Modificador de velocidade de caminhada no Active Aim | float | 0.9 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante o Active Aim. |
| Active Aim Sprint Accel Modifier | Modificador de aceleração de sprint no Active Aim | float | 1.1 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante o Active Aim. |
| Active Aim Stamina Rate | Taxa de stamina do Active Aim | float | 0.075 | 0–20 | — | Taxa de drenagem da stamina dos braços durante o Active Aim. |
| Active Aim Hipfire Bonus | Bônus de hipfire do Active Aim | float | 0.7 | 0–1 | — | Multiplicador aplicado à imprecisão do hipfire durante o Active Aim. |
| Active Aim Transition From: Idle | Transição do Active Aim a partir de: Idle | float | 3 | 0–50 | — | Velocidade de entrada no Active Aim a partir do estado ocioso (sem stance anterior). |
| Active Aim Blend Threshold: Low Ready | Limiar de blend do Active Aim: Low Ready | float | 0.15 | 0–1 | ✓ | — |
| Active Aim Blend Threshold: High Ready | Limiar de blend do Active Aim: High Ready | float | 0 | 0–1 | ✓ | — |
| Active Aim Blend Threshold: Left Shoulder | Limiar de blend do Active Aim: Left Shoulder | float | 0 | 0–1 | ✓ | — |
| Active Aim Blend Threshold: Patrol | Limiar de blend do Active Aim: Patrol | float | 1 | 0–1 | ✓ | — |
| Active Aim Blend Threshold: Short-Stock | Limiar de blend do Active Aim: Short-Stock | float | 0 | 0–1 | ✓ | — |
| Active Aim Transition From: Low Ready | Transição do Active Aim a partir de: Low Ready | float | 3.85 | 0–50 | ✓ | — |
| Active Aim Transition From: High Ready | Transição do Active Aim a partir de: High Ready | float | 3 | 0–50 | ✓ | — |
| Active Aim Transition From: Left Shoulder | Transição do Active Aim a partir de: Left Shoulder | float | 3 | 0–50 | ✓ | — |
| Active Aim Transition From: Patrol | Transição do Active Aim a partir de: Patrol | float | 1 | 0–50 | ✓ | — |
| Active Aim Transition From: Short-Stock | Transição do Active Aim a partir de: Short-Stock | float | 2.25 | 0–50 | ✓ | — |
| Active Aim Transition To Speed: Low Ready | Velocidade de transição do Active Aim para: Low Ready | float | 0.75 | 0–50 | ✓ | — |
| Active Aim Transition To Speed: High Ready | Velocidade de transição do Active Aim para: High Ready | float | 1 | 0–50 | ✓ | — |
| Active Aim Transition To Speed: Left Shoulder | Velocidade de transição do Active Aim para: Left Shoulder | float | 0.1 | 0–50 | ✓ | — |
| Active Aim Transition To Speed: Patrol | Velocidade de transição do Active Aim para: Patrol | float | 1 | 0–50 | ✓ | — |
| Active Aim Transition To Speed: Short-Stock | Velocidade de transição do Active Aim para: Short-Stock | float | 0.2 | 0–50 | ✓ | — |

### 5. High Ready.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| High Ready Keybind | Tecla do High Ready | KeyboardShortcut | UpArrow | — | — | — |
| High Ready Walk Speed Modifier | Modificador de velocidade de caminhada no High Ready | float | 1.035 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante o High Ready. |
| High Ready Sprint Accel Modifier | Modificador de aceleração de sprint no High Ready | float | 1.2 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante o High Ready. |
| High Ready Stamina Rate | Taxa de stamina do High Ready | float | 1.85 | 0–20 | — | Taxa de regeneração da stamina dos braços durante o High Ready. |
| High Ready Transition From: Idle | Transição do High Ready a partir de: Idle | float | 3 | 0–50 | — | Velocidade de entrada no High Ready a partir do estado ocioso (sem stance anterior). |
| High Ready Blend Threshold: Active Aim | Limiar de blend do High Ready: Active Aim | float | 0.2 | 0–1 | ✓ | — |
| High Ready Blend Threshold: Low Ready | Limiar de blend do High Ready: Low Ready | float | 0.05 | 0–1 | ✓ | — |
| High Ready Blend Threshold: Left Shoulder | Limiar de blend do High Ready: Left Shoulder | float | 0.2 | 0–1 | ✓ | — |
| High Ready Blend Threshold: Patrol | Limiar de blend do High Ready: Patrol | float | 0 | 0–1 | ✓ | — |
| High Ready Blend Threshold: Short-Stock | Limiar de blend do High Ready: Short-Stock | float | 0 | 0–1 | ✓ | — |
| High Ready Transition From: Active Aim | Transição do High Ready a partir de: Active Aim | float | 2.5 | 0–50 | ✓ | — |
| High Ready Transition From: Low Ready | Transição do High Ready a partir de: Low Ready | float | 3 | 0–50 | ✓ | — |
| High Ready Transition From: Left Shoulder | Transição do High Ready a partir de: Left Shoulder | float | 1.85 | 0–50 | ✓ | — |
| High Ready Transition From: Patrol | Transição do High Ready a partir de: Patrol | float | 1.25 | 0–50 | ✓ | — |
| High Ready Transition From: Short-Stock | Transição do High Ready a partir de: Short-Stock | float | 0.75 | 0–50 | ✓ | — |
| High Ready Transition To Speed: Active Aim | Velocidade de transição do High Ready para: Active Aim | float | 1 | 0–50 | ✓ | — |
| High Ready Transition To Speed: Low Ready | Velocidade de transição do High Ready para: Low Ready | float | 0.6 | 0–50 | ✓ | — |
| High Ready Transition To Speed: Left Shoulder | Velocidade de transição do High Ready para: Left Shoulder | float | 1.8 | 0–50 | ✓ | — |
| High Ready Transition To Speed: Patrol | Velocidade de transição do High Ready para: Patrol | float | 1 | 0–50 | ✓ | — |
| High Ready Transition To Speed: Short-Stock | Velocidade de transição do High Ready para: Short-Stock | float | 0.8 | 0–50 | ✓ | — |

### 6. Low Ready.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Low Ready Keybind | Tecla do Low Ready | KeyboardShortcut | DownArrow | — | — | — |
| Low Ready Walk Speed Modifier | Modificador de velocidade de caminhada no Low Ready | float | 1.055 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante o Low Ready. |
| Low Ready Sprint Accel Modifier | Modificador de aceleração de sprint no Low Ready | float | 1.25 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante o Low Ready. |
| Low Ready Stamina Rate | Taxa de stamina do Low Ready | float | 2.4 | 0–20 | — | Taxa de regeneração da stamina dos braços durante o Low Ready. |
| Low Ready Transition From: Idle | Transição do Low Ready a partir de: Idle | float | 3 | 0–50 | — | Velocidade de entrada no Low Ready a partir do estado ocioso (sem stance anterior). |
| Low Ready Blend Threshold: Active Aim | Limiar de blend do Low Ready: Active Aim | float | 0 | 0–1 | ✓ | — |
| Low Ready Blend Threshold: High Ready | Limiar de blend do Low Ready: High Ready | float | 0.25 | 0–1 | ✓ | — |
| Low Ready Blend Threshold: Left Shoulder | Limiar de blend do Low Ready: Left Shoulder | float | 0.5 | 0–1 | ✓ | — |
| Low Ready Blend Threshold: Patrol | Limiar de blend do Low Ready: Patrol | float | 0 | 0–1 | ✓ | — |
| Low Ready Blend Threshold: Short-Stock | Limiar de blend do Low Ready: Short-Stock | float | 0 | 0–1 | ✓ | — |
| Low Ready Transition From: Active Aim | Transição do Low Ready a partir de: Active Aim | float | 3 | 0–50 | ✓ | — |
| Low Ready Transition From: High Ready | Transição do Low Ready a partir de: High Ready | float | 3 | 0–50 | ✓ | — |
| Low Ready Transition From: Left Shoulder | Transição do Low Ready a partir de: Left Shoulder | float | 3 | 0–50 | ✓ | — |
| Low Ready Transition From: Patrol | Transição do Low Ready a partir de: Patrol | float | 2 | 0–50 | ✓ | — |
| Low Ready Transition From: Short-Stock | Transição do Low Ready a partir de: Short-Stock | float | 2 | 0–50 | ✓ | — |
| Low Ready Transition To Speed: Active Aim | Velocidade de transição do Low Ready para: Active Aim | float | 2 | 0–50 | ✓ | — |
| Low Ready Transition To Speed: High Ready | Velocidade de transição do Low Ready para: High Ready | float | 1.25 | 0–50 | ✓ | — |
| Low Ready Transition To Speed: Left Shoulder | Velocidade de transição do Low Ready para: Left Shoulder | float | 1.8 | 0–50 | ✓ | — |
| Low Ready Transition To Speed: Patrol | Velocidade de transição do Low Ready para: Patrol | float | 0.15 | 0–50 | ✓ | — |
| Low Ready Transition To Speed: Short-Stock | Velocidade de transição do Low Ready para: Short-Stock | float | 1 | 0–50 | ✓ | — |

### 7. Short-Stocking.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Short-Stock Keybind | Tecla do Short-Stock | KeyboardShortcut | J | — | — | Prende a coronha da arma sob o braço do jogador, encurtando o comprimento total da arma para evitar que o cano seja empurrado para longe do alvo. |
| Short-Stock Walk Speed Modifier | Modificador de velocidade de caminhada no Short-Stock | float | 0.9 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante o Short-Stocking. |
| Short-Stock Sprint Accel Modifier | Modificador de aceleração de sprint no Short-Stock | float | 0.9 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante o Short-Stocking. |
| Short-Stock Stamina Rate | Taxa de stamina do Short-Stock | float | 1.3 | 0–20 | — | Taxa de regeneração da stamina dos braços durante o Short-Stocking. |
| Short-Stock Hipfire Bonus | Bônus de hipfire do Short-Stock | float | 1.35 | 0–5 | — | Multiplicador aplicado à precisão do hipfire durante o Short-Stocking. |
| Short-Stock Transition From: Idle | Transição do Short-Stock a partir de: Idle | float | 2.25 | 0–50 | — | Velocidade de entrada no Short-Stock a partir do estado ocioso (sem stance anterior). |
| Short-Stock Blend Threshold: Active Aim | Limiar de blend do Short-Stock: Active Aim | float | 0 | 0–1 | ✓ | — |
| Short-Stock Blend Threshold: High Ready | Limiar de blend do Short-Stock: High Ready | float | 0 | 0–1 | ✓ | — |
| Short-Stock Blend Threshold: Low Ready | Limiar de blend do Short-Stock: Low Ready | float | 0 | 0–1 | ✓ | — |
| Short-Stock Blend Threshold: Patrol | Limiar de blend do Short-Stock: Patrol | float | 0 | 0–1 | ✓ | — |
| Short-Stock Blend Threshold: Left Shoulder | Limiar de blend do Short-Stock: Left Shoulder | float | 0 | 0–1 | ✓ | — |
| Short-Stock Transition From: Active Aim | Transição do Short-Stock a partir de: Active Aim | float | 2.25 | 0–50 | ✓ | — |
| Short-Stock Transition From: High Ready | Transição do Short-Stock a partir de: High Ready | float | 0.7 | 0–50 | ✓ | — |
| Short-Stock Transition From: Low Ready | Transição do Short-Stock a partir de: Low Ready | float | 1 | 0–50 | ✓ | — |
| Short-Stock Transition From: Patrol | Transição do Short-Stock a partir de: Patrol | float | 0.5 | 0–50 | ✓ | — |
| Short-Stock Transition From: Left Shoulder | Transição do Short-Stock a partir de: Left Shoulder | float | 1.55 | 0–50 | ✓ | — |
| Short-Stock Transition To Speed: Active Aim | Velocidade de transição do Short-Stock para: Active Aim | float | 0.1 | 0–50 | ✓ | — |
| Short-Stock Transition To Speed: High Ready | Velocidade de transição do Short-Stock para: High Ready | float | 1 | 0–50 | ✓ | — |
| Short-Stock Transition To Speed: Low Ready | Velocidade de transição do Short-Stock para: Low Ready | float | 0.8 | 0–50 | ✓ | — |
| Short-Stock Transition To Speed: Patrol | Velocidade de transição do Short-Stock para: Patrol | float | 1 | 0–50 | ✓ | — |
| Short-Stock Transition To Speed: Left Shoulder | Velocidade de transição do Short-Stock para: Left Shoulder | float | 0.5 | 0–50 | ✓ | — |

### 8. Patrol Stance.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Patrol/Neutral Stance Keybind | Tecla da stance Patrol/neutra | KeyboardShortcut | H | — | — | Coloca a arma em posição neutra, melhorando a regeneração da stamina dos braços e a velocidade de caminhada. Para o máximo de "larp". |
| Patrol Walk Speed Modifier | Modificador de velocidade de caminhada no Patrol | float | 1.1 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante a stance Patrol. |
| Patrol Sprint Accel Modifier | Modificador de aceleração de sprint no Patrol | float | 1.45 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante a stance Patrol. |
| Patrol Stamina Rate | Taxa de stamina do Patrol | float | 4.0 | 0–20 | — | Taxa de regeneração da stamina dos braços durante a stance Patrol. |
| Patrol Transition From: Idle | Transição do Patrol a partir de: Idle | float | 1.65 | 0–50 | — | Velocidade de entrada na stance Patrol a partir do estado ocioso (sem stance anterior). |
| Patrol Blend Threshold: Active Aim | Limiar de blend do Patrol: Active Aim | float | 0 | 0–1 | ✓ | — |
| Patrol Blend Threshold: High Ready | Limiar de blend do Patrol: High Ready | float | 0 | 0–1 | ✓ | — |
| Patrol Blend Threshold: Low Ready | Limiar de blend do Patrol: Low Ready | float | 0 | 0–1 | ✓ | — |
| Patrol Blend Threshold: Left Shoulder | Limiar de blend do Patrol: Left Shoulder | float | 0.5 | 0–1 | ✓ | — |
| Patrol Blend Threshold: Short-Stock | Limiar de blend do Patrol: Short-Stock | float | 0 | 0–1 | ✓ | — |
| Patrol Transition From: Active Aim | Transição do Patrol a partir de: Active Aim | float | 1 | 0–50 | ✓ | — |
| Patrol Transition From: High Ready | Transição do Patrol a partir de: High Ready | float | 1.1 | 0–50 | ✓ | — |
| Patrol Transition From: Low Ready | Transição do Patrol a partir de: Low Ready | float | 1.9 | 0–50 | ✓ | — |
| Patrol Transition From: Left Shoulder | Transição do Patrol a partir de: Left Shoulder | float | 2 | 0–50 | ✓ | — |
| Patrol Transition From: Short-Stock | Transição do Patrol a partir de: Short-Stock | float | 1 | 0–50 | ✓ | — |
| Patrol Transition To Speed: Active Aim | Velocidade de transição do Patrol para: Active Aim | float | 1 | 0–50 | ✓ | — |
| Patrol Transition To Speed: High Ready | Velocidade de transição do Patrol para: High Ready | float | 1.15 | 0–50 | ✓ | — |
| Patrol Transition To Speed: Low Ready | Velocidade de transição do Patrol para: Low Ready | float | 0.65 | 0–50 | ✓ | — |
| Patrol Transition To Speed: Left Shoulder | Velocidade de transição do Patrol para: Left Shoulder | float | 0.85 | 0–50 | ✓ | — |
| Patrol Transition To Speed: Short-Stock | Velocidade de transição do Patrol para: Short-Stock | float | 1 | 0–50 | ✓ | — |

### 9. Left Shoulder.

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| Left Shoulder Walk Speed Modifier | Modificador de velocidade de caminhada no Left Shoulder | float | 0.88 | 0.1–5 | — | Multiplicador aplicado à velocidade de caminhada durante a stance Left Shoulder. |
| Left Shoulder Sprint Accel Modifier | Modificador de aceleração de sprint no Left Shoulder | float | 0.9 | 0.1–5 | — | Multiplicador aplicado à aceleração de sprint durante a stance Left Shoulder. |
| Left Shoulder Stamina Rate | Taxa de stamina do Left Shoulder | float | 0 | 0–20 | — | Taxa de variação da stamina dos braços durante a stance Left Shoulder. |
| Left Shoulder Hipfire Bonus | Bônus de hipfire do Left Shoulder | float | 1.35 | 0–5 | — | Multiplicador aplicado à precisão do hipfire durante a stance Left Shoulder. |
| Left Shoulder Transition From: Idle | Transição do Left Shoulder a partir de: Idle | float | 2 | 0–50 | — | Velocidade de entrada no Left Shoulder a partir do estado ocioso (sem stance anterior). |
| Left Shoulder Blend Threshold: Active Aim | Limiar de blend do Left Shoulder: Active Aim | float | 0 | 0–1 | ✓ | — |
| Left Shoulder Blend Threshold: High Ready | Limiar de blend do Left Shoulder: High Ready | float | 0 | 0–1 | ✓ | — |
| Left Shoulder Blend Threshold: Low Ready | Limiar de blend do Left Shoulder: Low Ready | float | 0.55 | 0–1 | ✓ | — |
| Left Shoulder Blend Threshold: Patrol | Limiar de blend do Left Shoulder: Patrol | float | 0 | 0–1 | ✓ | — |
| Left Shoulder Blend Threshold: Short-Stock | Limiar de blend do Left Shoulder: Short-Stock | float | 0 | 0–1 | ✓ | — |
| Left Shoulder Transition From: Active Aim | Transição do Left Shoulder a partir de: Active Aim | float | 2 | 0–50 | ✓ | — |
| Left Shoulder Transition From: High Ready | Transição do Left Shoulder a partir de: High Ready | float | 1.3 | 0–50 | ✓ | — |
| Left Shoulder Transition From: Low Ready | Transição do Left Shoulder a partir de: Low Ready | float | 3.1 | 0–50 | ✓ | — |
| Left Shoulder Transition From: Patrol | Transição do Left Shoulder a partir de: Patrol | float | 1.55 | 0–50 | ✓ | — |
| Left Shoulder Transition From: Short-Stock | Transição do Left Shoulder a partir de: Short-Stock | float | 1.5 | 0–50 | ✓ | — |
| Left Shoulder Transition To Speed: Active Aim | Velocidade de transição do Left Shoulder para: Active Aim | float | 1 | 0–50 | ✓ | — |
| Left Shoulder Transition To Speed: High Ready | Velocidade de transição do Left Shoulder para: High Ready | float | 1.15 | 0–50 | ✓ | — |
| Left Shoulder Transition To Speed: Low Ready | Velocidade de transição do Left Shoulder para: Low Ready | float | 1.45 | 0–50 | ✓ | — |
| Left Shoulder Transition To Speed: Patrol | Velocidade de transição do Left Shoulder para: Patrol | float | 0.18 | 0–50 | ✓ | — |
| Left Shoulder Transition To Speed: Short-Stock | Velocidade de transição do Left Shoulder para: Short-Stock | float | 0.9 | 0–50 | ✓ | — |

---

## Observações

- **Nenhuma propriedade fantasma:** todas as 180 `ConfigEntry` declaradas no topo da classe são bindadas em `InitConfigBindings` (auditoria 1:1). Contagem por seção: Dev 20 · Weapon Stances And Position 25 · Keybinds 4 · Device Bonuses 8 · Active Aim 22 · High Ready 20 · Low Ready 20 · Short-Stocking 21 · Patrol 20 · Left Shoulder 20.
- **Left Shoulder não tem keybind próprio** — é a única stance sem tecla configurável na sua seção (as demais têm keybind com Order máximo da seção). A ativação vem de outro mecanismo do mod, fora deste arquivo de config.
- **Empates de `Order`** (ordem relativa no F12 indefinida entre si): "Remember Stance After Using Item"/"After Firing" (260); trio do Tactical Sprint (230); trio Alt Pistol/Rifle/Recoil (229); "Enable NVG"/"Thermal Aim Block" (5); "Hipfire Bonus" × "Transition From: Idle" no Active Aim (105), Short-Stock (6) e Left Shoulder (32); "test 19" × "test 20" (-10).
- **Tooltips de hipfire inconsistentes:** Active Aim diz "hipfire *inaccuracy*" (default 0.7 = melhora), mas Short-Stock e Left Shoulder dizem "hipfire *accuracy*" com default 1.35 — pelo padrão dos valores, todos parecem multiplicar a **imprecisão** (1.35 = hipfire pior), então o termo em SS/LS provavelmente está trocado no original.
- **Descrições vazias fora da seção Dev:** "Use Toggle For Active Aim", "Idle Stam Drain Modifer", "High Ready Keybind" e "Low Ready Keybind" têm tooltip vazio no código.
- **Typos no texto original** (mantidos no EN, corrigidos na tradução): "Wweapon", "Sublte", "Modifer", "Specfic", "WIth".
- **Curiosidades de código:** 11 declarações do Left Shoulder usam `ConfigEntry<float>?` (nullable) sem efeito prático; "Left Shoulder Stamina Rate" default 0 é o único neutro (nem drena nem regenera); a seção 1 concentra 25 opções heterogêneas (animações, colisão, mounting, stamina, sprint tático, aim blocks) enquanto as seções de stance são uniformes com 15 knobs avançados de blend/transição cada.
