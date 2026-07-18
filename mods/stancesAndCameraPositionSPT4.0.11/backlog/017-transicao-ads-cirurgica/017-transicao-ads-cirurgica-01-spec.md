# 017 — Transição Low/High Ready → ADS cirúrgica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Em progresso (spec refinada com a investigação técnica)
**Criado:** 2026-07-17
**Sandbox:** `modded/` canônico (o fork do realism foi descartado — ver [016](../016-transicao-realism-fork/)).
**Investigação técnica:** [00-investigacao-tecnica.md](017-transicao-ads-cirurgica-00-investigacao-tecnica.md)
(fatos confirmados via `ilspycmd` na DLL real).

## Visão geral

Ataca os **2 bugs reais de transição** que motivavam o item 016, com **abordagem própria** (o usuário testou o
Fontaine standalone e não gostou; portar as curvas dele herdaria o rejeitado). Aqui **não se troca o motor** —
opera-se sobre a mola existente, de forma cirúrgica. Os dois problemas são **independentes** e viram **fases
separadas** (F1, F2) sob um passo 0 de instrumentação.

## F0 — Régua de medição (reinstrumentar)

A régua `TransitionMetrics` (feita e descartada com o fork do realism) volta como **`Debug Transition Metrics`**
no `modded/` canônico. Sem número, "sobe demais" e "+30% do baseline" não são auditáveis.

- [ ] Portar a régua (versão já endurecida pelo code-review do 016: marcadores `(kick)`/`(chained)`/`(interrupted)`,
      assentamento por tempo, origem por corpo).
- [ ] **GATE (usuário): baseline do 2.5.0** — com `Debug Transition Metrics` on e `Stance Kick Intensity = 0`:
      pico vertical (Z local) em **Stance2→ADS** e **Stance1→ADS**, arma leve (ex.: MP5/pistola) e arma longa;
      ≥5 amostras/rota. Os "~5 cm" viram número. É contra ESTE baseline que a F1 é medida.

## F1 — Problema A: overshoot ao mirar (waypoint por Stance 0)

**Sintoma:** Low Ready → ADS: a mira **sobe demais antes de descer** (pior em armas leves). High Ready → ADS:
**"onda" de cima para baixo** até assentar.
**Causa:** o alvo salta em 1 frame da pose de Ready para a de ADS na mesma mola sub-amortecida, com velocidade
acumulada (ver investigação).
**Solução:** ao entrar em ADS vindo de stance, **assentar a velocidade da mola** para o alvo de ADS ser
alcançado sem overshoot. Implementado por corpo, **sem tocar `CurrentStance`**.

> ⚠️ **DESCOBERTA do review + config real (2026-07-17):** com a config do usuário (e o default de fábrica), **todos
> os offsets de ADS = 0** → o alvo de ADS é `Vector3.zero`, que é **o mesmo** alvo da Stance 0. Ou seja: "passar por
> Stance 0 antes do ADS" e "ir direto ao ADS" têm o **mesmo alvo** — um waypoint de *alvo* seria no-op. O que de
> fato mata o overshoot é **zerar/amortecer `_rotVelocity`/`_posVelocity` ao entrar em ADS** (a mola vem da pose de
> Ready com velocidade acumulada e passa do zero). Então a ideia do usuário ("waypoint por Stance 0") traduz-se
> tecnicamente em **amortecer a velocidade no início do ADS** — e isso é *melhor* do que um waypoint literal: não
> adiciona latência nem uma 2ª perna de trajetória. **Só quando o usuário configurar offsets de ADS ≠ 0** é que a
> distinção "Stance 0 vs ADS" volta a existir e um waypoint de alvo teria efeito próprio (tratar como caso extra).

**Critérios de aceite:**
- [ ] **Amortecer a velocidade no início do ADS é REQUISITO, não opcional** (achado do review): funciona mesmo com
      os offsets de ADS no default `0f`, medido pela régua no baseline default E com ADS custom.
- [ ] Vindo de Stance 1/2/3, mirar produz **desvio máximo vertical em relação à pose final de ADS ao longo de TODA
      a trajetória ≤ 0,5 cm** (baseline F0 ~5 cm) — não só "além do alvo"; **≤ 1 cruzamento de sinal** na trajetória
      inteira; tempo de assentamento **≤ +30%** do baseline.
- [ ] **Responsividade não piora**: tempo até a mira atingir ≥ 90% do deslocamento rumo ao alvo de ADS **não pode
      aumentar** vs. o baseline sem amortecimento (matar o overshoot não pode trocar por uma mira "mole"/em 2 tempos).
- [ ] Sem a "onda" perceptível na High Ready → ADS (validação visual).
- [ ] **Não toca `CurrentStance`**: snap-on-fire, stamina, speed-caps, mount e o pacote Fika inalterados (o
      waypoint é trajetória visual, no molde do timer de ADS-kick — nunca `SetStance`).
- [ ] **Só ativo com `_ResetOnADS = true`** (com `false` a pose continua em ADS, sem salto — waypoint desligado) e
      **fora de prone**.
- [ ] **Paridade Fika:** o observado passa pelo mesmo waypoint (helper compartilhado, armado na borda de mira do
      pacote) — 1ª e 3ª pessoa não divergem. Pacote de rede **inalterado**.
- [ ] Coexiste com o kick de ADS-in (0,15s) sem que um anule o outro (coordenar T × delay do kick / zeramento de
      velocidade — decisão na tech-spec).
- [ ] Toggle F12 `ADS Waypoint` (bool, default true) + `ADS Waypoint Time` (o T, faixa a definir). Nomes sem `=`.

## F2 — Problema B: braço esquerdo quebra (atenuar offset por comprimento)

**Sintoma:** **só** de Low Ready → Stance 0, **armas longas**: o braço esquerdo **hiperestende e "quebra"**.

> ⚠️ **CAUSA NÃO CONFIRMADA (achado do review + config real):** o usuário descreveu "a arma desloca para frente".
> Mas a config real mostra Low Ready (Stance 2) com **Forward/Backward = +0.015** (positivo = frente) → ir para
> Stance 0 (=0) move a arma **para TRÁS**, não para frente. E a Stance 2 tem **Up/Down = +0.07** e **Pitch = 25°** —
> ao ir para Stance 0 a arma **desce 7 cm e roda 25° de pitch**. Portanto o "empurrão" percebido pode ser da
> **rotação de pitch** ou do **Up/Down**, não do Forward/Backward. **A F2 começa com um diagnóstico (gate humano):
> com a régua, medir QUAL eixo tem a maior excursão em Low Ready → Stance 0 com arma longa** — só então a atenuação
> ataca o eixo certo. (Isto pode invalidar "atenuar Y local" e redirecionar para pitch/Up-Down.)
**Solução:** ler `FirearmController.WeaponLn` (o comprimento que o EFT já calcula) e **escalar a componente
longitudinal do offset da transição** por um fator que cai com o comprimento (arma longa = menos empurrão para
frente). ⚠️ **Só LER o `WeaponLn`, nunca reescrever** (ele define a origem do projétil — erro que o Fontaine
cometeu e reverteu).

**Critérios de aceite:**
- [ ] **GATE (usuário): diagnóstico do eixo** — régua on, arma longa, Low Ready → Stance 0: qual eixo (pitch /
      Up-Down / Forward-Backward) tem a maior excursão? A atenuação ataca ESSE eixo, não "Y local" por suposição.
- [ ] Low Ready → Stance 0 com arma longa (ex.: rifle full-length / DMR): o braço esquerdo **não hiperestende**
      (validação visual, vídeo antes/depois).
- [ ] Armas curtas (pistola/PDW) **inalteradas** — o fator só atenua acima de um limiar de comprimento.
- [ ] A **pose final** (t=1) da transição continua a do slider — a atenuação afeta só a **trajetória**, não o
      destino. (Régua confirma delta residual ≈ 0.)
- [ ] `WeaponLn` lido por reflection **cacheada** (na troca de arma, não por frame); **nunca escrito**.
- [ ] **Verificar parentesco com P-11.2** (braço G36 em High Ready ao mirar): se a mesma atenuação cobrir o G36,
      fecha a P-11.2 junto; se não, P-11.2 segue como item separado.
- [ ] Toggle F12 `Attenuate Push By Weapon Length` (bool, default true) + limiares de comprimento (faixa a definir).
- [ ] Determinístico em Fika (por-arma local) — sem depender de estado de IK sincronizado.

## Corner cases

- [ ] **Trocar de arma no meio da transição** (curta↔longa): o fator de atenuação segue o `WeaponLn` cacheado da
      arma atual; sem salto brusco.
- [ ] **Waypoint interrompido** (soltar o ADS no meio do T; snap-on-fire durante o waypoint; trocar de stance
      mirando): recaptura limpa, sem estado preso — a régua marca `(interrupted)`.
- [ ] **Reset de raid / morte / extração:** o estado do waypoint e o cache de `WeaponLn` zeram (via
      `StanceManager.ResetState`).
- [ ] **Interação F1 × F2 (achado do review):** se a F1 assentar velocidade mas o alvo de ADS for custom (≠0) e
      a arma for longa, a atenuação da F2 deve valer também para o trecho do ADS — senão a F1 pode reintroduzir a
      hiperextensão da F2 por outro caminho. Verificar quando as duas fases estiverem juntas.
- [ ] **`ApplySimpleRotationPatch`:** confirmar na tech-spec se o caminho "simples" é alcançável na prática (qual
      arma/mira roteia p/ lá); se sim, os dois fixes valem lá também — **critério**, não corner opcional.
- [ ] **Mira telescópica de alto zoom** (FOV reduzido muda a percepção do pico), **troca de ombro (lean) durante o
      amortecimento**, **`ADS Transition Speed` nos extremos** (muito alto/baixo) — cobrir na validação.

## Fora de escopo

- Curvas / modelo de ADS do Fontaine (descartados no 016).
- **P-11.1** (velocidade presa devagar — speed limit stale): bug ortogonal, item próprio.
- Reescrever `WeaponLn` ou a origem do projétil.
- O refino do Problema B pela "folga real do braço" (`_limbs[0]`) — só se o `WeaponLn` sozinho não resolver
  (fica como possível F3/dívida).

## Referências

- [00 — Investigação técnica](017-transicao-ads-cirurgica-00-investigacao-tecnica.md) — os fatos e refs do Assembly.
- [016 (cancelado)](../016-transicao-realism-fork/016-transicao-realism-fork-01-spec.md) — diagnóstico da causa do
  overshoot; a régua `TransitionMetrics` a reaproveitar.
- [014 — Sync stances Fika](../014-sync-stances-fika/014-sync-stances-fika-01-spec.md) — ponto de aplicação pré-IK.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Spec inicial ao cancelar o 016. |
| 2026-07-17 | Refinada com a investigação técnica (waypoint plugável sem tocar `CurrentStance`; `WeaponLn` como sinal confirmado). Fases F0/F1/F2 + critérios mensuráveis. |
| 2026-07-17 | Review adversarial (sub-agent) + conferência da config REAL do usuário: **2 achados 🔴 confirmados pelos dados**. (1) offsets de ADS = 0 → o "waypoint por Stance 0" é, na verdade, **amortecer a velocidade da mola** (o alvo já é zero); zerar velocidade virou requisito. (2) a causa da F2 **não bate** com "empurra p/ frente" (Low Ready tem F/B +0.015, vai p/ TRÁS ao ir a Stance 0; o forte é Up/Down +0.07 e Pitch 25°) → a F2 abre com diagnóstico do eixo. + interação F1×F2, responsividade, métrica de trajetória inteira. |
