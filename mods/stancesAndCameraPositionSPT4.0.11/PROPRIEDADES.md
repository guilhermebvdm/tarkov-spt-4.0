# Propriedades F12 — stancesAndCameraPositionSPT4.0.11

> Todas as opções do menu **F12** (BepInEx ConfigurationManager). **23 seções · 143 opções.**
> Regenerado de `modded/Plugin.cs` em 2026-07-09 (fonte de verdade). Tooltips reproduzidos do código (alguns em inglês, como aparecem no jogo).
>
> **Ordem no menu F12:** o ConfigurationManager ordena as seções por **ordem de descoberta** (primeira `Config.Bind`), não por nome. Aqui elas estão agrupadas **por tema** para leitura; a ordem real no jogo está na tabela ["Ordem no F12"](#ordem-no-menu-f12) ao fim.

## Índice por tema

| Tema | Seções |
|---|---|
| [A. Troca de stances e câmera](#a--troca-de-stances-e-câmera) | Settings · General · Positions · Field of View |
| [B. Poses das stances e mira (ADS)](#b--poses-das-stances-e-mira-ads) | Stance 0/1/2/3 · Wiggle · Advanced ADS Transitions · ADS Default Values · Default Hands/Arms Positions |
| [C. Apoio de arma (mount) e stamina](#c--apoio-de-arma-mount-e-stamina) | Weapon Mount (Active) · Weapon Mount (Passive) · Stamina Management |
| [D. Movimento e velocidade](#d--movimento-e-velocidade) | Movement & Inertia · Tac Sprint · Animations & Transitions |
| [E. Mecânicas de arma](#e--mecânicas-de-arma) | Manual Chambering · Action Stances |
| [F. Respiração, UI e debug](#f--respiração-ui-e-debug) | 9. Respiração · 10. Barra de Oxigênio · Debug |

> ⚠️ **Rótulos legados:** por retrocompatibilidade de config salva, alguns keys mantêm nomes antigos: "Enable Stance 2 - **Custom** in Cycle" e "Enable Stance 3 - **Low Ready** in Cycle" (hoje Stance 2 = Low Ready, Stance 3 = Custom); e nos blocos de Stance, os keys "Roll (Tombar Arma)" e "Yaw (Apontar Esq/Dir)" estão trocados em relação ao eixo interno (o tooltip descreve o eixo real). Não renomeados para não descartar valores salvos (BepInEx casa por `(seção, key)`).

---

## A — Troca de stances e câmera

### Settings

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Include Stance 0 - Vanilla in Cycle | bool | `false` | — | Inclui a Stance 0 (Vanilla) no ciclo. Sempre afeta o ciclo da tecla V; afeta o scroll só quando o modo de scroll = Cycle. |
| Enable Stance 1 - High Ready in Cycle | bool | `true` | — | Inclui a Stance 1 no ciclo; desligado = pulada. |
| Enable Stance 2 - Custom in Cycle | bool | `true` | — | Inclui a Stance 2 no ciclo (rótulo legado — hoje Stance 2 = Low Ready). |
| Enable Stance 3 - Low Ready in Cycle | bool | `true` | — | Inclui a Stance 3 no ciclo (rótulo legado — hoje Stance 3 = Custom). |
| Stance Toggle Hotkey | KeyCode | `V` | — | Tecla para ciclar as stances habilitadas: Default → 1 → 2 → 3 → Default. |
| Enable Mouse Wheel Stance Cycle | bool | `false` | — | Segurar o modificador + scroll do mouse cicla as stances. |
| Mouse Wheel Modifier Key | KeyCode | `LeftAlt` | — | Tecla a segurar durante o scroll para ciclar (quando o scroll está ligado). |
| Mouse Wheel Scroll Mode | enum | `Linear` | — | Cycle = circular, respeita os toggles por-stance. Linear = eixo fixo: Stance 1 (topo) ↔ 0 (centro) ↔ 2 (base); Stance 3 fica fora do eixo (só via hotkey). |
| ADS Transition Speed | float | `1` | 0.5 – 5 | Velocidade de transição das mãos entre stance e ADS. 1 = lento, 2 = normal, 3+ = rápido. |
| Stance Change Sound Volume | float | `1` | 0 – 2 | Volume do som de "rattle" ao trocar de stance. 0 = mudo. |
| Stance 0 - Vanilla Hotkey | KeyCode | `None` | — | Tecla dedicada para voltar à Stance 0. Sem toggle. Bloqueada no sprint e ignorada em ADS. |
| Stance 1 - High Ready Hotkey | KeyCode | `None` | — | Tecla dedicada para Stance 1. Toggle: apertar em Stance 1 volta à 0. Bloqueada no sprint, ignorada em ADS. |
| Stance 2 - Low Ready Hotkey | KeyCode | `None` | — | Tecla dedicada para Stance 2. Toggle. Bloqueada no sprint, ignorada em ADS. |
| Stance 3 - Custom Hotkey | KeyCode | `O` | — | Tecla dedicada para Stance 3. Toggle. Bloqueada no sprint, ignorada em ADS. |
| Snap Fire Threshold (ms) | int | `600` | 50 – 1000 | Tempo máx. de clique (ms) classificado como clique único. Clique = snap p/ Stance 0 sem atirar; segurar = snap + 1 tiro natural. |
| Snap Stale Timeout (s) | float | `2` | 0.5 – 10 | Tempo máx. (s) que o intercept de snap fica ativo sem button-up antes de auto-limpar. Reduz risco de estado preso em troca de arma durante o hold. |
| Start In Low Ready On Raid Begin | bool | `true` | — | Começa toda raid já em Stance 3 - Low Ready, sem animação. Vale mesmo se a Stance 3 estiver fora do ciclo. |

### General

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Stance Transition Speed | float | `1.0` | 0.1 – 5.0 | Multiplicador de velocidade da transição entre stances e a vista padrão. |
| Stance Kick Intensity (Contra o Peito) | float | `-0.05` | -0.3 – 0.3 | Quanto a arma "chuta" contra o peito ao trocar de stance/ADS. Negativo puxa para você. |
| ADS Kick Delay (In) | float | `0.15` | 0 – 1 | Atraso (s) antes do kick ao entrar em ADS. Sincroniza o kick com o fim da animação de mira. |
| Stance Overshoot Damping (Menos gera Mais Quicada) | float | `12.0` | 1 – 30.0 | Damping da mola. Menor = mais overshoot/quicada. Padrão 12. |

### Positions

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Camera Position | bool | `true` | — | Liga os offsets de posição de câmera. |
| Forward/Backward Offset | float | `0` | -0.5 – 0.5 | Câmera para frente/trás (positivo = frente). |
| Up/Down Offset | float | `0.02` | -0.5 – 0.5 | Câmera para cima/baixo (positivo = cima). |
| Sideways Offset | float | `0` | -0.5 – 0.5 | Câmera para os lados (positivo = direita). |

### Field of View

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Expanded FOV Range | bool | `false` | — | Permite estender o slider de FOV além do 50-75 padrão. |
| Minimum FOV | int | `20` | 1 – 50 | FOV mínimo (padrão do jogo = 50). |
| Maximum FOV | int | `150` | 75 – 170 | FOV máximo (padrão do jogo = 75). |

---

## B — Poses das stances e mira (ADS)

Cada stance é **uma seção única** no F12, agregando: animação de sprint, rotação/posição das mãos, multiplicadores de ADS e velocidade/stamina/snap.

### Stance 0 - Vanilla

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Stance 0 Modifies Movement Speed | bool | `true` | — | Aplica um teto de velocidade nesta stance. |
| Stance 0 Movement Speed Multiplier | int | `90` | 50 – 100 | Teto de velocidade em %. Só redução (limitação do sistema do EFT). |
| Stance 0 Apply When Prone | bool | `false` | — | Aplica os efeitos (drain/recovery e teto) em prone. Desligado por padrão (pode conflitar com animações de prone). |
| ADS Pitch Multiplier | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway (Pitch) durante ADS a partir da Stance 0. |
| ADS Yaw Multiplier | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway (Yaw) durante ADS a partir da Stance 0. |
| ADS Roll Multiplier | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway (Roll) durante ADS a partir da Stance 0. |
| ADS Pos Y Multiplier (Forward/Back) | float | `1.0` | -5.0 – 5.0 | Multiplicador do sway posicional (eixo Y) durante ADS a partir da Stance 0. |
| ADS Pos Z Multiplier (Up/Down) | float | `1.0` | -5.0 – 5.0 | Multiplicador do sway posicional (eixo Z) durante ADS a partir da Stance 0. |

### Stance 1 - High Ready

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 1 Sprint Animation | bool | `true` | — | Usa a animação compacta de sprint (tac sprint) ao correr na Stance 1. |
| Stance 1 Pitch (Cano Sobe/Desce) | float | `-34.0` | -45 – 45 | Rotação pitch das mãos/braços (graus). |
| Stance 1 Roll (Tombar Arma) | float | `0.0` | -45 – 45 | Rotação yaw das mãos/braços (key legado — eixo interno é yaw). |
| Stance 1 Yaw (Apontar Esq/Dir) | float | `0.0` | -45 – 45 | Rotação roll das mãos/braços (key legado — eixo interno é roll). |
| Stance 1 Forward/Backward (Frente/Trás) | float | `0.02` | -0.5 – 0.5 | Posição frente/trás (positivo = frente). |
| Stance 1 Up/Down (Coronha Sobe/Desce) | float | `-0.01` | -0.5 – 0.5 | Posição cima/baixo (positivo = cima). |
| Stance 1 Sideways (Coronha Esq/Dir) | float | `0.02` | -0.5 – 0.5 | Posição lateral (positivo = direita). |
| ADS Pitch / Yaw / Roll Multiplier | float | `1.0` | 0.0 – 5.0 | Multiplicadores das curvas de sway durante ADS a partir da Stance 1. |
| ADS Pos Y / Z Multiplier | float | `1.0` | -5.0 – 5.0 | Multiplicadores do sway posicional (Y/Z) durante ADS a partir da Stance 1. |
| Stance 1 Modifies Movement Speed | bool | `true` | — | Aplica teto de velocidade nesta stance. |
| Stance 1 Movement Speed Multiplier | int | `95` | 50 – 100 | Teto de velocidade em % (só redução). |
| Stance 1 Apply When Prone | bool | `false` | — | Aplica os efeitos em prone (desligado por padrão). |
| Stance 1 Snap to Stance 0 on Fire | bool | `true` | — | Atirar nesta stance faz snap p/ Stance 0. Clique < threshold = sem tiro; segurar = snap + 1 tiro. Não dispara em ADS nem com item não-arma. |

### Stance 2 - Low Ready

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 2 Sprint Animation | bool | `false` | — | Animação compacta de sprint na Stance 2. |
| Stance 2 Pitch (Cano Sobe/Desce) | float | `25.0` | -45 – 45 | Rotação pitch (graus). |
| Stance 2 Roll (Tombar Arma) | float | `0.0` | -45 – 45 | Rotação yaw (key legado). |
| Stance 2 Yaw (Apontar Esq/Dir) | float | `0.0` | -45 – 45 | Rotação roll (key legado). |
| Stance 2 Forward/Backward (Frente/Trás) | float | `0.015` | -0.5 – 0.5 | Posição frente/trás. |
| Stance 2 Up/Down (Coronha Sobe/Desce) | float | `-0.02` | -0.5 – 0.5 | Posição cima/baixo. |
| Stance 2 Sideways (Coronha Esq/Dir) | float | `0.05` | -0.5 – 0.5 | Posição lateral. |
| ADS Pitch / Yaw / Roll Multiplier | float | `1.0` | 0.0 – 5.0 | Multiplicadores de sway em ADS a partir da Stance 2. |
| ADS Pos Y / Z Multiplier | float | `1.0` | -5.0 – 5.0 | Sway posicional (Y/Z) em ADS a partir da Stance 2. |
| Stance 2 Modifies Movement Speed | bool | `true` | — | Teto de velocidade. |
| Stance 2 Movement Speed Multiplier | int | `90` | 50 – 100 | Teto em % (só redução). |
| Stance 2 Apply When Prone | bool | `false` | — | Efeitos em prone (desligado por padrão). |
| Stance 2 Snap to Stance 0 on Fire | bool | `false` | — | Snap p/ Stance 0 ao atirar (desligado por padrão nesta stance). |

### Stance 3 - Custom

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 3 Sprint Animation | bool | `false` | — | Animação compacta de sprint na Stance 3. |
| Stance 3 Pitch (Cano Sobe/Desce) | float | `0` | -45 – 45 | Rotação pitch (graus). |
| Stance 3 Roll (Tombar Arma) | float | `-30` | -45 – 45 | Rotação yaw (key legado). |
| Stance 3 Yaw (Apontar Esq/Dir) | float | `0` | -45 – 45 | Rotação roll (key legado). |
| Stance 3 Forward/Backward (Frente/Trás) | float | `0` | -0.5 – 0.5 | Posição frente/trás. |
| Stance 3 Up/Down (Coronha Sobe/Desce) | float | `0` | -0.5 – 0.5 | Posição cima/baixo. |
| Stance 3 Sideways (Coronha Esq/Dir) | float | `0` | -0.5 – 0.5 | Posição lateral. |
| Stance 3 Modifies Movement Speed | bool | `true` | — | Teto de velocidade. |
| Stance 3 Movement Speed Multiplier | int | `100` | 50 – 100 | Teto em % (100 = sem redução). |
| Stance 3 Apply When Prone | bool | `false` | — | Efeitos em prone (desligado por padrão). |
| Stance 3 Snap to Stance 0 on Fire | bool | `true` | — | Snap p/ Stance 0 ao atirar. |

### 8. Wiggle (Q/E) Dynamics (Stance Based)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Animation Curve Duration | float | `0.35` | 0.1 – 10.0 | Duração (s) da transição cinemática de stance. |
| Stance Pitch Multiplier (Cano sobe/desce) | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway Pitch nas transições de STANCE. |
| Stance Yaw Multiplier (Apontar Esq/Dir) | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway Yaw nas transições de STANCE. |
| Stance Roll Multiplier (Tombar Arma) | float | `1.0` | 0.0 – 5.0 | Multiplicador da curva de sway Roll nas transições de STANCE. |
| Stance Position Multiplier (Coronha no peito) | float | `1.0` | 0.0 – 5.0 | Multiplicador das curvas de sway posicional nas transições de STANCE. |

### Advanced ADS Transitions

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Advanced ADS Transitions | bool | `false` | — | Arremessa a arma para frente e depois puxa de volta ao mirar (simula "shouldering"). |

### ADS Default Values (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Reset Positions When Aiming | bool | `true` | — | Transiciona todas as posições para os defaults ao entrar em ADS. |
| ADS Pitch (Cano Sobe/Desce) | float | `0` | -45 – 45 | Pitch das mãos em ADS com "Reset On ADS" ligado. 0 = posição padrão. |
| ADS Roll (Tombar Arma) | float | `0` | -45 – 45 | Yaw das mãos em ADS (key legado). |
| ADS Yaw (Apontar Esq/Dir) | float | `0` | -45 – 45 | Roll das mãos em ADS (key legado). |
| ADS Forward/Backward (Frente/Trás) | float | `0` | -0.5 – 0.5 | Posição frente/trás em ADS. |
| ADS Up/Down (Coronha Sobe/Desce) | float | `0` | -0.5 – 0.5 | Posição cima/baixo em ADS. |
| ADS Sideways (Coronha Esq/Dir) | float | `0` | -0.5 – 0.5 | Posição lateral em ADS. |

### Default Hands/Arms Positions (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Default Hands/Arms Position | bool | `false` | — | Liga os offsets de posição padrão das mãos quando NÃO está em stance. |
| Default Forward/Backward (Frente/Trás) | float | `0` | -0.5 – 0.5 | Posição hip-fire padrão frente/trás (positivo = frente). |
| Default Up/Down (Coronha Sobe/Desce) | float | `0` | -0.5 – 0.5 | Posição hip-fire padrão cima/baixo (positivo = cima). |
| Default Sideways (Coronha Esq/Dir) | float | `0` | -0.5 – 0.5 | Posição hip-fire padrão lateral (positivo = direita). |

---

## C — Apoio de arma (mount) e stamina

### Weapon Mount (Active) — Item 015

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Block Active Mount In Stance | bool | `true` | — | Impede apoiar a arma em superfícies (mount) em Stance 1/2/3 sem mirar. Em Stance 0, mirando ou deitado (prone), funciona normal. |

### Weapon Mount (Passive) — Item 011

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Passive Mount | bool | `true` | — | Apoio passivo: encostar a arma numa superfície (sem tecla) dá um benefício leve de estabilidade. Desligado = só o mount nativo. |
| Passive Recoil Multiplier | float | `0.7` | 0.1 – 1 | Recuo enquanto apoiado (passivo). 0.7 = 30% menos. Deve ser MAIOR que o do mount ativo (o passivo é mais fraco). |
| Passive Sway Multiplier | float | `0.65` | 0 – 1 | Sway (respiração) apoiado. 0.65 = 35% menos. |
| Passive Stamina Save | bool | `true` | — | Apoiado, pausa/reduz o drain de stamina de braço (mais fraco que o mount nativo). |
| Show Mount Icon | bool | `true` | — | Mostra o ícone direcional (esq/dir/baixo) no canto inferior direito quando o apoio passivo está ativo. |

### Stamina Management — Item 012

Multiplicador de stamina de braço por cenário. Semântica: **< 1 drena · 1 mantém · > 1 recupera.**

| Propriedade (EN) | Tipo | Padrão | Faixa | Cenário |
|---|---|---|---|---|
| Stance 0 Stamina Multiplier | float | `0.5` | 0 – 10 | Em pé, sem mount, Stance 0 (hipfire). |
| Stance 1 Stamina Multiplier | float | `1.5` | 0 – 10 | Em pé, sem mount, Stance 1. |
| Stance 2 Stamina Multiplier | float | `1.0` | 0 – 10 | Em pé, sem mount, Stance 2. |
| Stance 3 Stamina Multiplier | float | `2.0` | 0 – 10 | Em pé, sem mount, Stance 3. |
| ADS - Stand up Multiplier | float | `0.7` | 0 – 10 | Em pé, mirando (ADS). |
| Hold Breath - Stand up Multiplier | float | `0.5` | 0 – 10 | Em pé, segurando a respiração. |
| Prone Stamina Multiplier | float | `1.5` | 0 – 10 | Deitado, hipfire. |
| ADS - Prone Multiplier | float | `0.9` | 0 – 10 | Deitado, mirando. |
| Hold Breath - Prone Multiplier | float | `0.7` | 0 – 10 | Deitado, segurando a respiração. |
| Passive Mount Multiplier | float | `1.5` | 0 – 10 | Apoio passivo (encostado), Stance 0. |
| ADS - Passive Mount Multiplier | float | `1.0` | 0 – 10 | Apoio passivo, mirando. |
| Hold Breath - Passive Mount Multiplier | float | `0.9` | 0 – 10 | Apoio passivo, segurando a respiração. |
| Active Mount Multiplier | float | `3.0` | 0 – 10 | Mount nativo (montado), Stance 0. |
| ADS - Active Mount Multiplier | float | `1.5` | 0 – 10 | Mount nativo, mirando. |
| Hold Breath - Active Mount Multiplier | float | `1.0` | 0 – 10 | Mount nativo, segurando a respiração. |
| Debug Stamina State | bool | `false` | — | Mostra na tela + loga o cenário de stamina ativo (`STAMINA STATE: ...`). |

---

## D — Movimento e velocidade

### Movement & Inertia — Item 007

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Inertia Multiplier | float | `1.2` | 0.1 – 3.0 | Multiplicador global de inércia (sensação de peso). 1.0 = padrão. |
| Walk Speed Multiplier | float | `0.85` | 0.1 – 2.0 | Multiplicador da velocidade máx. de caminhada. 1.0 = padrão. |
| Sprint Speed Multiplier | float | `0.9` | 0.1 – 2.0 | Multiplicador da velocidade máx. de corrida. 1.0 = padrão. |

### Tac Sprint Settings (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Tac Sprint Weight Limit | float | `5.1` | 1 – 15 | Peso máx. da arma (kg) para permitir a animação de tac sprint. |
| Tac Sprint Weight Limit (Bullpup) | float | `5.75` | 1 – 15 | Peso máx. (kg) para bullpups (limite maior). |
| Tac Sprint Length Limit | int | `6` | 1 – 10 | Comprimento máx. da arma (células do inventário). |
| Tac Sprint Ergo Limit | float | `35` | 0 – 100 | Ergonomia mínima da arma para permitir tac sprint. |
| Tac Sprint Reset Delay | float | `0.35` | 0 – 1 | Atraso (s) após o sprint antes de a arma voltar ao tamanho normal. 0 = instantâneo. |

### Animations & Transitions (Item 005)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Crouch Speed Multiplier | float | `1.5` | 1 – 5 | Multiplicador da velocidade das animações de agachar e deitar. |
| Lean Speed Multiplier | float | `1.5` | 1 – 5 | Multiplicador da velocidade de inclinar (Q/E). |

---

## E — Mecânicas de arma

### Manual Chambering Settings (Item 010)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Manual Chambering | bool | `true` | — | Master toggle. Desligado = vanilla em todos os cenários (kill-switch). Puxe o ferrolho com a tecla nativa 'Chamber/Unload' quando a câmara estiver vazia e houver munição no carregador. |
| Manual Chambering On Raid Start | bool | `true` | — | Arma que inicia a raid com câmara vazia NÃO carrega a 1ª bala no spawn — puxe manualmente. Efetivo na PRÓXIMA raid. |
| Manual Chambering On Reload | bool | `true` | — | Recarregar com câmara vazia NÃO carrega a 1ª bala automaticamente — puxe manualmente. Tempo real. |

### Action Stances — Item 008

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Action Stance Swap | bool | `true` | — | Levanta a arma p/ Stance 0 automaticamente ao recarregar, checar munição/câmara, examinar a arma, checar modo de fogo e esvaziar a câmara — e retorna à postura anterior ao fim. |

---

## F — Respiração, UI e debug

### 9. Respiração (Hold Breath)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Oxygen Drain / sec | float | `5.0` | 0 – 50 | Oxigênio extra drenado por segundo enquanto segura a respiração. |
| Enable Custom Breath Audio | bool | `true` | — | Toca os áudios de respiração da pasta do mod ao segurar a respiração. |
| Breath In Volume | float | `1.0` | 0 – 2 | Volume do áudio de inspirar. |
| Breath Out Volume | float | `1.0` | 0 – 2 | Volume do áudio de expirar. |
| Heartbeat Volume | float | `1.0` | 0 – 2 | Volume do loop de batimento cardíaco. |

### 10. Barra de Oxigênio (UI)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Oxygen UI Bar | bool | `true` | — | Mostra uma barra branca acima da stamina de mãos que drena ao segurar a respiração. |
| UI X Position | float | `20` | 0 – 3000 | Posição horizontal da barra (px da esquerda). |
| UI Y Position | float | `120` | 0 – 2000 | Posição vertical (px de BAIXO). |
| UI Width | float | `260` | 10 – 1000 | Largura da barra. |
| UI Height | float | `4` | 1 – 20 | Altura (espessura) da barra. |

### Debug (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Debug Apply In Hideout | bool | `false` | — | Permite os efeitos de stamina/velocidade rodarem no hideout (firing range). Para testes offline; DESLIGAR no jogo normal. |

---

## Ordem no menu F12

A ordem real das seções no ConfigurationManager (por ordem de descoberta no `Plugin.cs`):

1. Manual Chambering Settings (Item 010) · 2. Positions · 3. Settings · 4. General · 5. Advanced ADS Transitions · 6. ADS Default Values (Advanced) · 7. Default Hands/Arms Positions (Advanced) · 8. Stance 0 - Vanilla · 9. Stance 1 - High Ready · 10. Stance 2 - Low Ready · 11. Stance 3 - Custom · 12. Weapon Mount (Active) · 13. Weapon Mount (Passive) · 14. Stamina Management · 15. 9. Respiração (Hold Breath) · 16. 10. Barra de Oxigênio (UI) · 17. Animations & Transitions (Item 005) · 18. Movement & Inertia · 19. Action Stances · 20. 8. Wiggle (Q/E) Dynamics · 21. Tac Sprint Settings (Advanced) · 22. Field of View · 23. Debug (Advanced)

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-09 | Guilherme | Regeneração completa a partir de `Plugin.cs` (143 opções / 23 seções). Removidas ~41 entradas órfãs (sistema "Shoulder Throw" inexistente + keys renomeados); adicionadas ~70 faltantes (Manual Chambering, ADS multipliers por stance, Wiggle, Hold Breath, Oxygen UI, Movement, etc.). Reorganizado por tema. |
