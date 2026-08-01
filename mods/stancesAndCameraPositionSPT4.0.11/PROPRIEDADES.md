# Propriedades F12 — stancesAndCameraPositionSPT4.0.11

> Todas as opções do menu **F12** (BepInEx ConfigurationManager). **19 seções · 111 opções.**
> Regenerado de `modded/Plugin.cs` em **2026-07-12** (fonte de verdade), para a **v2.5.0**.
>
> ⚠️ **A contagem acima é da v2.5.0.** As versões 2.6.0–2.13.0 acrescentaram opções (waypoint de ADS por
> postura, compressão de ADS-speed, UI de checagem de câmara, `Debug ADS Speed`) e removeram a seção
> `Action Stances`. O documento recebeu edições pontuais, mas **só volta a bater exatamente com o jogo na
> próxima regeneração completa** — prevista na faxina de preparação para publicação. Os tooltips do jogo são bilíngues (EN + PT); aqui a coluna **Descrição** traz a versão em português resumida.
>
> **v2.2.0 corrigiu os eixos Yaw/Roll** (estavam trocados em todas as stances e no ADS — a rotação é aplicada nos eixos LOCAIS da arma, onde Y = cano/roll e Z = vertical/yaw) e **traduziu os nomes para inglês**. Tooltips seguem bilíngues.
>
> **v2.1.0 removeu 7 opções que não faziam efeito** (de 120 → 113; de 21 → 20 seções): a seção `Default Hands/Arms Positions` inteira (4) e `Stance 1/2/3 Apply When Prone` (3). Ver [review 02](./PROPRIEDADES-review-02.md) (`MP-02-01`, `MP-02-02`).
>
> **Ordem no menu F12:** o ConfigurationManager ordena as seções por **ordem de descoberta** (primeira `Config.Bind`), não por nome. Aqui elas estão agrupadas **por tema** para leitura; a ordem real no jogo está na tabela ["Ordem no F12"](#ordem-no-menu-f12) ao fim.

## Índice por tema

| Tema | Seções |
|---|---|
| [A. Troca de stances e câmera](#a--troca-de-stances-e-câmera) | Stance Cycle & Hotkeys · Stance Transition & Kick · Camera Position |
| [B. Poses e mira (ADS)](#b--poses-e-mira-ads) | Stance 0/1/2/3 · ADS Default Values |
| [C. Mount e stamina](#c--mount-e-stamina) | Weapon Mount (Active) · Weapon Mount (Passive) · Stamina Management |
| [D. Movimento](#d--movimento) | Movement & Inertia · Tac Sprint · Animation Speed |
| [E. Mecânicas de arma](#e--mecânicas-de-arma) | Manual Chambering |
| [F. Respiração, UI e debug](#f--respiração-ui-e-debug) | Hold Breath · Oxygen Bar (UI) · Debug |

---

## A — Troca de stances e câmera

### Stance Cycle & Hotkeys

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Include Stance 0 - Vanilla in Cycle | bool | `false` | — | Inclui a Stance 0 (Vanilla) no ciclo de posturas. Substitui o antigo `Use Only Stances`. Sempre afeta o ciclo da tecla V; afeta o scroll do mouse só quando Mouse Wheel Scroll Mode = Cycle. |
| Enable Stance 1 - High Ready in Cycle | bool | `true` | — | Inclui a Stance 1 no ciclo; desligado = pulada. |
| Enable Stance 2 - Low Ready in Cycle | bool | `true` | — | Inclui a Stance 2 no ciclo; desligado = pulada. |
| Enable Stance 3 - Custom in Cycle | bool | `true` | — | Inclui a Stance 3 no ciclo; desligado = pulada. |
| Stance Toggle Hotkey | KeyCode | `V` | — | Tecla para percorrer as posturas ativas: Default → 1 → 2 → 3 → Default. |
| Enable Mouse Wheel Stance Cycle | bool | `false` | — | Segurar a tecla modificadora + girar a roda do mouse percorre as posturas. |
| Mouse Wheel Modifier Key | KeyCode | `LeftAlt` | — | Tecla a segurar durante o scroll para ciclar (quando o ciclo por roda está ligado). |
| Mouse Wheel Scroll Mode | enum | `Linear` | — | Cycle = circular, respeita os toggles de cada postura. Linear = eixo fixo: Stance 1 (topo) ↔ 0 (centro) ↔ 2 (base); a Stance 3 fica fora do eixo (só via hotkey dedicada). |
| Stance 0 - Vanilla Hotkey | KeyCode | `None` | — | Tecla dedicada para voltar à Stance 0. Sem toggle. Bloqueada no sprint e ignorada em ADS. |
| Stance 1 - High Ready Hotkey | KeyCode | `None` | — | Tecla dedicada para a Stance 1. Toggle: apertar já em Stance 1 volta à 0. Bloqueada no sprint, ignorada em ADS. |
| Stance 2 - Low Ready Hotkey | KeyCode | `None` | — | Tecla dedicada para a Stance 2. Toggle. Bloqueada no sprint, ignorada em ADS. |
| Stance 3 - Custom Hotkey | KeyCode | `O` | — | Tecla dedicada para a Stance 3. Toggle. Bloqueada no sprint, ignorada em ADS. |
| Snap Fire Threshold (ms) | int | `600` | 50 – 1000 | Tempo máx. entre pressionar e soltar (ms) classificado como clique único. Clique = snap p/ Stance 0 sem atirar; segurar = snap + 1 tiro natural. |
| Snap Stale Timeout (s) | float | `2` | 0.5 – 10 | Tempo máx. (s) que o intercept do snap fica ativo sem soltar o botão antes de auto-limpar. Reduz risco de estado preso em troca de arma durante o hold. |
| Start In Low Ready On Raid Begin | bool | `true` | — | Começa toda raid já na Stance 2 - Low Ready, sem animação (set imediato). Vale mesmo se a Stance 2 estiver fora do ciclo. |
| Enable Action Stance Swap | bool | `true` | — | Levanta a arma para a Stance 0 (Pronto) automaticamente ao recarregar, checar munição/câmara, examinar a arma, checar modo de fogo e esvaziar a câmara — e retorna à postura anterior ao fim. (Origem: item 008 do backlog.) |

### Stance Transition & Kick

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Stance Transition Speed | float | `1.0` | 0.1 – 5.0 | Velocidade da troca **entre posturas** (e volta à visão padrão). **Não** afeta a mira. |
| ADS Transition Speed | float | `1.0` | 0.1 – 5.0 | Velocidade de **levantar/baixar a mira** (entrar e sair do ADS). Separada do Stance Transition Speed na v2.4.0. |
| Stance Kick Intensity (Toward the Chest) | float | `-0.05` | -0.3 – 0.3 | Quanto a arma recua contra o peito ao trocar de postura ou mirar (ADS). Negativo puxa a arma em sua direção. |
| ADS Kick Delay (In) | float | `0.15` | 0 – 2 | Atraso (s) antes de aplicar o kick ao entrar em ADS. Sincroniza o kick com o fim da animação de mira. |
| Stance Overshoot Damping (Lower Means More Bounce) | float | `12.0` | 1 – 30.0 | Amortecimento da física de mola. Menor = mais overshoot/quicada. Padrão 12. |

### Camera Position

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Camera Position | bool | `true` | — | Liga ou desliga os ajustes de posição da câmera. |
| Forward/Backward Offset | float | `0` | -0.5 – 0.5 | Posição da câmera para frente/trás (positivo = frente). |
| Up/Down Offset | float | `0.02` | -0.5 – 0.5 | Posição da câmera para cima/baixo (positivo = cima). |
| Sideways Offset | float | `0` | -0.5 – 0.5 | Posição da câmera para os lados (positivo = direita). |

### ~~Field of View~~ — REMOVIDA na v2.5.0

Era um FOV de **viewmodel** (perspectiva só dos braços/arma, não do mundo): deformava os braços e o valor ficava
gravado nas configurações do jogo, sem desfazer ao desligar a opção. Removida inteira — o jogo volta a limitar o
FOV em 50-75.

---

## B — Poses e mira (ADS)

Cada stance é **uma seção única** no F12, agregando: animação de sprint, rotação/posição das mãos, teto de velocidade, prone e snap-on-fire.

### Stance 0 - Vanilla

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Stance 0 Modifies Movement Speed | bool | `true` | — | Aplica um teto de velocidade nesta postura. |
| Stance 0 Movement Speed Multiplier | int | `90` | 50 – 100 | Teto de velocidade em %. 100 = sem redução. Só reduções (limitação do sistema de speed limit do EFT). |
| Stance 0 Apply When Prone | bool | `false` | — | Aplica os efeitos (drain/recuperação e teto) em prone. Desligado por padrão (pode conflitar com animações de prone). |

### Stance 1 - High Ready

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 1 Sprint Animation | bool | `true` | — | Usa uma animação compacta de sprint (tac sprint) ao correr na Stance 1. |
| Stance 1 Pitch (Muzzle Up/Down) | float | `-34.0` | -45 – 45 | Rotação de pitch das mãos/braços (graus) — inclina o cano p/ cima/baixo. |
| Stance 1 Yaw (Point Left/Right) | float | `0.0` | -45 – 45 | Rotação de yaw das mãos/braços (graus) — aponta p/ esquerda/direita. |
| Stance 1 Roll (Cant Weapon) | float | `0.0` | -45 – 45 | Rotação de roll das mãos/braços (graus) — tomba a arma. |
| Stance 1 Forward/Backward | float | `0.02` | -0.5 – 0.5 | Posição frente/trás (positivo = frente). |
| Stance 1 Up/Down (Stock Up/Down) | float | `-0.01` | -0.5 – 0.5 | Posição cima/baixo (positivo = cima). |
| Stance 1 Sideways (Stock Left/Right) | float | `0.02` | -0.5 – 0.5 | Posição lateral (positivo = direita). |
| Stance 1 Modifies Movement Speed | bool | `true` | — | Aplica teto de velocidade nesta postura. |
| Stance 1 Movement Speed Multiplier | int | `95` | 50 – 100 | Teto de velocidade em % (só redução). |
| Stance 1 Snap to Stance 0 on Fire | bool | `true` | — | Atirar nesta postura faz snap p/ Stance 0. Clique < limiar = sem tiro; segurar = snap + 1 tiro natural. Não dispara em ADS nem com item não-arma. |

### Stance 2 - Low Ready

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 2 Sprint Animation | bool | `false` | — | Animação compacta de sprint (tac sprint) ao correr na Stance 2. |
| Stance 2 Pitch (Muzzle Up/Down) | float | `25.0` | -45 – 45 | Rotação de pitch (graus) — inclina o cano p/ cima/baixo. |
| Stance 2 Yaw (Point Left/Right) | float | `0.0` | -45 – 45 | Rotação de yaw (graus) — aponta p/ esquerda/direita. |
| Stance 2 Roll (Cant Weapon) | float | `0.0` | -45 – 45 | Rotação de roll (graus) — tomba a arma. |
| Stance 2 Forward/Backward | float | `0.015` | -0.5 – 0.5 | Posição frente/trás (positivo = frente). |
| Stance 2 Up/Down (Stock Up/Down) | float | `-0.02` | -0.5 – 0.5 | Posição cima/baixo (positivo = cima). |
| Stance 2 Sideways (Stock Left/Right) | float | `0.05` | -0.5 – 0.5 | Posição lateral (positivo = direita). |
| Stance 2 Modifies Movement Speed | bool | `true` | — | Aplica teto de velocidade nesta postura. |
| Stance 2 Movement Speed Multiplier | int | `90` | 50 – 100 | Teto de velocidade em % (só redução). |
| Stance 2 Snap to Stance 0 on Fire | bool | `false` | — | Snap p/ Stance 0 ao atirar (desligado por padrão nesta postura). |

### Stance 3 - Custom

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Stance 3 Sprint Animation | bool | `false` | — | Animação compacta de sprint (tac sprint) ao correr na Stance 3. |
| Stance 3 Pitch (Muzzle Up/Down) | float | `0` | -45 – 45 | Rotação de pitch (graus) — inclina o cano p/ cima/baixo. |
| Stance 3 Yaw (Point Left/Right) | float | `-30` | -45 – 45 | Rotação de yaw (graus) — aponta p/ esquerda/direita. |
| Stance 3 Roll (Cant Weapon) | float | `0` | -45 – 45 | Rotação de roll (graus) — tomba a arma. |
| Stance 3 Forward/Backward | float | `0` | -0.5 – 0.5 | Posição frente/trás (positivo = frente). |
| Stance 3 Up/Down (Stock Up/Down) | float | `0` | -0.5 – 0.5 | Posição cima/baixo (positivo = cima). |
| Stance 3 Sideways (Stock Left/Right) | float | `0` | -0.5 – 0.5 | Posição lateral (positivo = direita). |
| Stance 3 Modifies Movement Speed | bool | `true` | — | Aplica teto de velocidade nesta postura. |
| Stance 3 Movement Speed Multiplier | int | `100` | 50 – 100 | Teto de velocidade em % (100 = sem redução). |
| Stance 3 Snap to Stance 0 on Fire | bool | `true` | — | Snap p/ Stance 0 ao atirar. Clique < limiar = sem tiro; segurar = snap + 1 tiro. Não dispara em ADS nem com item não-arma. |

### ADS Default Values (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Reset Positions When Aiming | bool | `true` | — | Faz a transição suave de todas as posições para os padrões ao mirar (ADS). |
| ADS Pitch (Muzzle Up/Down) | float | `0` | -45 – 45 | Pitch das mãos ao mirar com 'Reset On ADS' ligado — inclina o cano p/ cima/baixo. 0 = padrão do jogo. |
| ADS Yaw (Point Left/Right) | float | `0` | -45 – 45 | Yaw das mãos ao mirar — aponta p/ esquerda/direita. 0 = padrão do jogo. |
| ADS Roll (Cant Weapon) | float | `0` | -45 – 45 | Roll das mãos ao mirar — tomba a arma. 0 = padrão do jogo. |
| ADS Forward/Backward | float | `0` | -0.5 – 0.5 | Posição das mãos frente/trás ao mirar. |
| ADS Up/Down (Stock Up/Down) | float | `0` | -0.5 – 0.5 | Posição das mãos cima/baixo ao mirar — coronha sobe/desce. |
| ADS Sideways (Stock Left/Right) | float | `0` | -0.5 – 0.5 | Posição das mãos esquerda/direita ao mirar — coronha esq/dir. |

### ~~Default Hands/Arms Positions (Advanced)~~ — REMOVIDA na v2.1.0

As 4 opções desta seção **não faziam nada** e foram removidas (`MP-02-01`, [review 02](./PROPRIEDADES-review-02.md)):
alimentavam o branch `_ =>` de `GetTargetPosition` (stance = Default), mas todos os call-sites desse método são
gated em `isInStance` — o branch era inalcançável. Para ajuste de posição **fora** de postura, use
[`Camera Position`](#camera-position).

---

## C — Mount e stamina

### Weapon Mount (Active)

> Origem: item 015 do backlog.

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Block Active Mount In Stance | bool | `true` | — | Impede apoiar a arma em superfícies (mount) em Stance 1/2/3 sem mirar. Em Stance 0, mirando ou deitado (prone), funciona normal. |

### Weapon Mount (Passive)

> Origem: item 011 do backlog.

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Passive Mount | bool | `true` | — | Apoio passivo: encostar a arma numa superfície (sem a tecla de mount) dá um benefício leve de estabilidade. Desligado = só o mount nativo. |
| Passive Recoil Multiplier | float | `0.7` | 0.1 – 1 | Recuo enquanto apoiado (passivo). 0.7 = 30% menos. Deve ser MAIOR que o do mount ativo (o passivo é mais fraco). |
| Passive Sway Multiplier | float | `0.65` | 0 – 1 | Sway (respiração) enquanto apoiado. 0.65 = 35% menos. |
| Passive Stamina Save | bool | `true` | — | Enquanto apoiado, pausa/reduz o drain de stamina de braço (mais fraco que o mount nativo). |
| Show Mount Icon | bool | `true` | — | Mostra o ícone direcional (esq/dir/baixo) no canto inferior direito quando o apoio passivo está ativo. |

### Stamina Management

> Origem: item 012 do backlog.

Multiplicador de stamina de braço por cenário. Semântica: **< 1 drena · 1 mantém · > 1 recupera.**

| Propriedade (EN) | Tipo | Padrão | Faixa | Cenário |
|---|---|---|---|---|
| Stance 0 Stamina Multiplier | float | `0.5` | 0 – 10 | Em pé, sem mount, Stance 0 (hipfire). |
| Stance 1 Stamina Multiplier | float | `1.5` | 0 – 10 | Em pé, sem mount, Stance 1. |
| Stance 2 Stamina Multiplier | float | `1.0` | 0 – 10 | Em pé, sem mount, Stance 2. |
| Stance 3 Stamina Multiplier | float | `2.0` | 0 – 10 | Em pé, sem mount, Stance 3. |
| ADS - Stand up Multiplier | float | `0.7` | 0 – 10 | Em pé, sem mount, mirando (ADS). |
| Hold Breath - Stand up Multiplier | float | `0.5` | 0 – 10 | Em pé, sem mount, segurando a respiração. |
| Prone Stamina Multiplier | float | `1.5` | 0 – 10 | Deitado (prone), sem mount, hipfire. |
| ADS - Prone Multiplier | float | `0.9` | 0 – 10 | Deitado, mirando. |
| Hold Breath - Prone Multiplier | float | `0.7` | 0 – 10 | Deitado, segurando a respiração. |
| Passive Mount Multiplier | float | `1.5` | 0 – 10 | Apoio passivo (encostado), Stance 0. |
| ADS - Passive Mount Multiplier | float | `1.0` | 0 – 10 | Apoio passivo, mirando (segura, não recupera). |
| Hold Breath - Passive Mount Multiplier | float | `0.9` | 0 – 10 | Apoio passivo, segurando a respiração. |
| Active Mount Multiplier | float | `3.0` | 0 – 10 | Mount nativo (montado), Stance 0. |
| ADS - Active Mount Multiplier | float | `1.5` | 0 – 10 | Mount nativo, mirando. |
| Hold Breath - Active Mount Multiplier | float | `1.0` | 0 – 10 | Mount nativo, segurando a respiração. |
| Debug Stamina State | bool | `false` | — | Mostra na tela + loga o cenário de stamina ativo (`STAMINA STATE: ...`). |

---

## D — Movimento

### Movement & Inertia

> Origem: item 007 do backlog.

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Inertia Multiplier | float | `1.2` | 0.1 – 3.0 | Multiplicador global da inércia do personagem (sensação de peso). 1.0 = padrão. |
| Walk Speed Multiplier | float | `0.85` | 0.1 – 2.0 | Multiplicador da velocidade máx. de caminhada. 1.0 = padrão. |
| Sprint Speed Multiplier | float | `0.9` | 0.1 – 2.0 | Multiplicador da velocidade máx. de corrida (sprint). 1.0 = padrão. |

### Tac Sprint Settings (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Tac Sprint Weight Limit | float | `5.1` | 1 – 15 | Peso máx. da arma (kg) para permitir a animação de tac sprint. |
| Tac Sprint Weight Limit (Bullpup) | float | `5.75` | 1 – 15 | Peso máx. (kg) para bullpups permitirem tac sprint (limite maior). |
| Tac Sprint Length Limit | int | `6` | 1 – 10 | Comprimento máx. da arma (células de inventário). |
| Tac Sprint Ergo Limit | float | `35` | 0 – 100 | Ergonomia mínima da arma para permitir tac sprint. |
| Tac Sprint Reset Delay | float | `0.35` | 0 – 2 | Atraso (s) após o fim do sprint antes da arma voltar ao tamanho normal. 0 = instantâneo. |

### Animation Speed

> Origem: item 005 do backlog.

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Crouch Speed Multiplier | float | `1.5` | 1 – 5 | Multiplicador da velocidade das animações de agachar e deitar (prone). |
| Lean Speed Multiplier | float | `1.5` | 1 – 5 | Multiplicador da velocidade de inclinar o corpo (Q/E). |

---

## E — Mecânicas de arma

### Manual Chambering

> Origem: item 010 do backlog.

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Manual Chambering | bool | `true` | — | Interruptor mestre. Desligado = vanilla em TODOS os cenários (kill-switch seguro). Puxe o ferrolho com a tecla nativa 'Chamber/Unload' quando a câmara estiver vazia e houver munição no carregador. |
| Manual Chambering On Raid Start | bool | `true` | — | Arma que inicia a raid com câmara vazia NÃO carrega a 1ª bala no spawn — puxe manualmente. Efetivo na PRÓXIMA raid. |
| Manual Chambering On Reload | bool | `true` | — | Recarregar com câmara vazia NÃO carrega a 1ª bala automaticamente após inserir o carregador — puxe manualmente. Tempo real. |

## F — Respiração, UI e debug

### Hold Breath

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Oxygen Drain / sec | float | `5.0` | 0 – 50 | Quanto de oxigênio extra é drenado por segundo enquanto segura a respiração. |
| Enable Custom Breath Audio | bool | `true` | — | Toca os arquivos breath_in.wav e breath_out.wav personalizados da pasta do mod ao segurar a respiração. |
| Breath In Volume | float | `1.0` | 0 – 2 | Volume do áudio de inspiração (breath_in). |
| Breath Out Volume | float | `1.0` | 0 – 2 | Volume do áudio de expiração (breath_out). |
| Heartbeat Volume | float | `1.0` | 0 – 2 | Volume do áudio em loop dos batimentos cardíacos. |

### Oxygen Bar (UI)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Enable Oxygen UI Bar | bool | `true` | — | Exibe uma barra branca acima da stamina de braço que esvazia enquanto segura a respiração. |
| UI X Position | float | `20` | 0 – 3000 | Posição horizontal da barra (pixels a partir da esquerda). |
| UI Y Position | float | `120` | 0 – 2000 | Posição vertical da barra (pixels a partir da BASE). |
| UI Width | float | `260` | 10 – 1000 | Largura da barra de oxigênio. |
| UI Height | float | `4` | 1 – 20 | Altura (espessura) da barra de oxigênio. |

### Debug (Advanced)

| Propriedade (EN) | Tipo | Padrão | Faixa | Descrição |
|---|---|---|---|---|
| Debug Apply In Hideout | bool | `false` | — | Permite os efeitos de stamina/velocidade rodarem no hideout (estande de tiro). Útil para testes offline; DESATIVE para o jogo normal. |
| Debug ADS Speed | bool | `false` | — | Mostra na tela a velocidade de mira da arma em mãos, nativa → comprimida, e o tempo de mira em segundos. Use para calibrar o `ADS Speed Pivot`. |

---

## Ordem no menu F12

A ordem real das seções no ConfigurationManager (por ordem de descoberta no `Plugin.cs`):

1. Manual Chambering · 2. Camera Position · 3. Stance Cycle & Hotkeys · 4. Stance Transition & Kick · 5. ADS Default Values (Advanced) · 6. Stance 0 - Vanilla · 7. Stance 1 - High Ready · 8. Stance 2 - Low Ready · 9. Stance 3 - Custom · 10. Weapon Mount (Active) · 11. Weapon Mount (Passive) · 12. Stamina Management · 13. Hold Breath · 14. Oxygen Bar (UI) · 15. Animation Speed · 16. Movement & Inertia · 17. Tac Sprint Settings (Advanced) · 18. Debug (Advanced)

> A seção `Action Stances` deixou de existir na **v2.13.0** — sua única opção (`Enable Action Stance Swap`)
> foi absorvida pelo rodapé de `Stance Cycle & Hotkeys`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-09 | Guilherme | Regeneração completa a partir de `Plugin.cs` (143 opções / 23 seções). Removidas ~41 entradas órfãs (sistema "Shoulder Throw" inexistente + keys renomeados); adicionadas ~70 faltantes (Manual Chambering, ADS multipliers por stance, Wiggle, Hold Breath, Oxygen UI, Movement, etc.). Reorganizado por tema. |
| 2026-07-11 | Guilherme | Limpeza (23 props mortas removidas) + reorganização (nomes de seção EN intuitivos, eixos Roll/Yaw corrigidos, tooltips bilíngues). Ver PROPRIEDADES-review-01.md. |
</content>
</invoke>
