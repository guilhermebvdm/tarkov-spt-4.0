# 017 — Transição Low/High Ready → ADS cirúrgica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog (⚪ — spec inicial, ainda não refinada)
**Criado:** 2026-07-17
**Sandbox:** `modded/` canônico (o fork do realism foi descartado — ver [016](../016-transicao-realism-fork/)).

## Visão geral

Ataca os **2 bugs reais de transição** que motivavam o item 016, mas **sem** as curvas do Fontaine (o usuário
testou o Fontaine standalone e não gostou). A abordagem aqui é **cirúrgica e própria**, desenhada pelo usuário
(2026-07-17), operando sobre a mola existente em vez de trocar o motor.

Os dois problemas são **independentes** (podem virar sub-itens/fases separadas) e a régua de medição
(`TransitionMetrics`, feita e descartada no 016) deve ser o **primeiro ataque** — reinstrumentar para medir
antes de mexer.

## Problema A — overshoot ao entrar em ADS (a mira "sobe demais" antes de assentar)

**Sintoma (usuário):**
- **Low Ready → ADS:** a mira **sobe demais antes de descer** para posicionar no ponto correto do ADS.
  **Pior em armas menores / mais leves.**
- **High Ready → ADS:** problema análogo, mas invertido — faz uma **"onda" de cima para baixo** até assentar na
  mira.

**Causa (já diagnosticada no 016):** ao mirar, o alvo troca **na mesma mola** com **velocidade acumulada**; a mola
é sub-amortecida (ζ≈0,49) e passa do alvo antes de assentar. A pose de Ready (cano baixo/alto) vai **direto** para
o alvo de ADS (≈ zero), então o primeiro lóbulo da oscilação cruza a linha de mira.

**Ideia do usuário — waypoint por Stance 0:** em vez de ir **direto** Ready → ADS, fazer uma transição **rápida e
smooth** para a **Stance 0 (Vanilla)** primeiro, e **depois** Ready→Stance 0→ADS. A passagem por Stance 0 (pose
neutra, perto de zero) **assenta a velocidade** da mola antes do trecho final, então o trecho Stance 0 → ADS parte
"do repouso", sem velocidade herdada que cause overshoot. Mesma solução vale para High Ready.

**Notas de implementação (a refinar na tech-spec):**
- O mod **já** tem precedente de forçar Stance 0: o item 013 (prone/mount) e o snap-on-fire. O waypoint reusa esse
  conceito, mas como estágio de **transição**, não como troca de estado permanente — o `CurrentStance` não pode
  virar Default (senão mexe em snap-on-fire/stamina/Fika/mount).
- Provável forma: um alvo intermediário no pipeline de pose (o alvo vira Stance 0 por T ms, depois ADS), sem
  tocar `CurrentStance`. Ou um 2º estágio na própria mola.
- ⚠️ Verificar que não conflita com o `TransitionSpeedTracker` (stance vs ADS) nem com o kick de ADS-in.

## Problema B — braço esquerdo "quebra" em Low Ready → Stance 0 (armas longas)

**Sintoma (usuário):** **apenas** de Low Ready → Stance 0 (não é o ADS), com **armas longas**. Ao mover para
Stance 0, a arma **desloca um pouco para frente**; em arma longa, o braço esquerdo (que já está estendido na
empunhadura dianteira) **hiperestende e "quebra"** a animação.

**Ideia do usuário — atenuar o offset longitudinal por comprimento de arma:** existem **pontos de fixação** do
player na arma (os IK markers da mão no EFT). Se soubermos a **distância desses pontos** em relação ao boneco (ou
em relação ao **tamanho da arma**), dá para fazer o movimento Low Ready → Stance 0 **não empurrar a arma para
frente** tanto — atenuando o offset longitudinal em armas longas, evitando a hiperextensão.

**Notas de implementação (a refinar na tech-spec):**
- Investigar no Assembly real: `HandsContainer.LeftHandIkMarker`/`RightHandIkMarker` (ou equivalente 0.16.x),
  `Weapon.length`/comprimento efetivo, e como o EFT calcula o alcance do braço. O offset de posição da transição
  é aplicado no `Weapon_Root_Anim` (validado no item 014) — o eixo "para frente" é o **Y local (longitudinal, o
  cano)**.
- Fórmula provável: escalar o componente longitudinal do offset da transição por um fator que cai com o
  comprimento da arma (arma curta = offset cheio; arma longa = offset reduzido). Calibrar o ponto de corte
  in-game.
- **Distinguir de P-11.2** (braço deformado em **High Ready + G36 ao MIRAR**): aquele é na entrada de ADV; este
  é Low Ready → Stance 0. Podem ter causa comum (offset longitudinal × alcance do braço em arma longa) — a
  tech-spec deve verificar se um fix resolve os dois.

## Fora de escopo

- As curvas / o modelo de ADS do Fontaine (descartados no 016).
- P-11.1 (velocidade presa devagar — speed limit stale): bug ortogonal, item próprio.

## Referências

- [016 (cancelado)](../016-transicao-realism-fork/016-transicao-realism-fork-01-spec.md) — diagnóstico da causa do
  overshoot (mola sub-amortecida + velocidade herdada) que o Problema A ataca, e o estudo do Fontaine.
- [014 — Sync stances Fika](../014-sync-stances-fika/014-sync-stances-fika-01-spec.md) — o ponto de aplicação
  pré-IK (`Weapon_Root_Anim`) onde o offset de posição do Problema B é aplicado.
- Memória: P-11.2 (braço G36 High Ready) e o overshoot — bugs de gameplay reportados.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Spec inicial criada ao cancelar o 016. Captura os 2 problemas + as 2 ideias do usuário. Ainda ⚪ backlog — falta `/create-spec` refinar critérios de aceite e a tech-spec investigar os IK markers / weapon length. |
