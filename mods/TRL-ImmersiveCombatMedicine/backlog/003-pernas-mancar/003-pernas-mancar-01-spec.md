# 003 — Pernas: Mancar N1/N2 + agachar involuntário

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-18

## Visão geral

Primeiro consumidor do motor (002): implementa as linhas de PERNA da matriz que não envolvem queda — Mancar N1/N2 contínuos e o agachar involuntário one-shot do Zerar 2 — com rebaixamento por analgésico, para humanos e bots. Entrega a **primitiva compartilhada de AGACHAR involuntário** (reusada pelo 006); o derrubar forçado e a arbitragem D2 nascem no 004 (primeiro consumidor real).

## Comportamento atual

O "Sistema de Pernas" legado só reage ao caso extremo (2 pernas zeradas → prone forçado + punição por levantar, já aposentada na decisão 21). Perder 1 perna ou ter fraturas não produz nenhum efeito do mod — só as penalidades vanilla. Não há níveis, lado, analgésico nem bots mancando por regra do mod.

## Comportamento desejado

1. **Estados contínuos de mancar** (matriz completa de pernas, com analgésico rebaixando):
   - Zerar 1 · Quebrar 1 → N1 / (analgésico) nada
   - Zerar 2 → N2 + agachar involuntário na ENTRADA / (analgésico) N1
   - Zerar 1+Quebrar 1 → N2 / (analgésico) N1
   - Quebrar 2 → **interim N2** (ver item 6) / (analgésico) N1 — a coluna com-analgésico é responsabilidade PERMANENTE do 003 (mesmo após o 004)
   - Zerar 2+Quebrar 2 → **interim N2** / (analgésico) N2
   Entrada/saída dirigidas pelos eventos+snapshot do motor; reversão por cura própria, remota ou cirurgia acompanha o motor.
2. **Calibração N1/N2 = TOTAL experienciado** (decisão 18): a config F12 define o **total-alvo em % da velocidade baseline** (iniciais: N1 = 80%, N2 = 55% — refináveis pelo inventário do P1); o mod computa o delta em RUNTIME sobre a penalidade vanilla vigente. **Delta nunca negativo:** total experienciado = `max(alvo, penalidade vanilla)` (nunca acelerar o jogador); clamps logados para calibração. Com classe/skill buffada (D12), o total é relativo ao baseline composto.
3. **Agachar involuntário one-shot** (decisão 5): agacha sem travar pose (pode levantar em seguida); **só transiciona para BAIXO** — pose corrente já ≤ agachado = no-op sem consumir cooldown; respeita anti-thrash do motor (decisão 19), guards D7 integrais (escada/corda/BTR/vault — contexto não-detectável vira limitação registrada pelo spike/P4) e avaliação inicial estabelecedora (spawn ferido não agacha).
4. **Primitiva de agachar compartilhada:** nasce aqui como utilitário (006 reusa); derrubar + arbitragem D2 ficam no 004.
5. **Bots inclusos** (decisão 11): mancar equivalente para locomoção SAIN-driven (mecanismo do P1/P6); agachar one-shot em bot via dip de pose com devolução imediata de controle (decisão 16); funcional no headless (dono dos bots).
6. **Interim até o 004 (decisão de projeto, registrada AQUI e no PROPRIEDADES na entrega):** o motor SEMPRE publica a linha real da matriz (Cair+ciclo — contrato do 002); o CONSUMIDOR 003 mapeia o estado Cair para o efeito N2 enquanto o 004 não existir, **sem** disparar o agachar (o one-shot pertence à linha Zerar 2, não à linha Cair). O 004 assume o estado ao entregar.
7. **Substituição incremental (D10):** na entrega, o Sistema de Pernas legado fica permanentemente inerte (independente de config antiga); o toggle do consumidor 003 nasce **ON** (o master Trauma 2.0 governa).
8. **Feedback:** toast de 1ª ocorrência via infra do motor (EN/PT — decisão 22); mancar sem som próprio (agachar pode usar voz de dor leve se o P5 recomendar).

## Critérios de aceite

- [ ] Zerar 1 perna (sem analgésico) aplica N1 imediatamente; reverter (cura própria, remota ou cirurgia) remove ≤1 s; velocidade volta ao baseline vanilla.
- [ ] Zerar 2 pernas: agachar involuntário 1× na entrada + N2 contínuo; analgésico rebaixa para N1 na hora; expiração re-escala e re-agacha (decisão 14) respeitando cooldown.
- [ ] 2 pernas QUEBRADAS + analgésico ativo → N1 aplicado; expiração re-escala para o interim N2 (e para Cair após o 004).
- [ ] Medição da decisão 18: log do multiplicador efetivo publicado pelo pipeline + spot-check cronometrado em percurso fixo → total dentro de **±5 p.p.** do alvo configurado (classe vanilla; casos de clamp `max(alvo, vanilla)` excluídos da medição e visíveis no log).
- [ ] Bot com 1 perna zerada manca (velocidade reduzida no dono host/headless, visível ao peer) e normaliza ao ser curado (host ou médico client via cura coop); bot com 2 zeradas faz o dip de agachar 1× e retoma o comportamento SAIN imediatamente (log + observação).
- [ ] Sistema de Pernas legado inerte após a entrega; interim do Cair documentado e ativo (Q2 → N2 sem agachar).
- [ ] **Fika/multiplayer:** peer vê mancar (velocidade/condição sync) e agachar (pose sync) do dono — incluindo bots do host/headless vistos por clients; espelhos não aplicam efeito próprio.
- [ ] **Estado entre raids:** reset via motor; spawn ferido estabelece N1/N2 sem one-shot nem toast.

## Corner cases

- [ ] Transição N1↔N2 sem passar por "nada": atualiza sem flicker de velocidade.
- [ ] Agachar durante sprint: transição segura; durante vault/BTR/escada-corda: adiado (D7) e executado no próximo contexto válido (cooldown conta da execução); **disparo adiado é CANCELADO se o snapshot não mais exigir o one-shot na execução** (curado/analgésico/toggle-off/fim de raid) — cooldown não consumido.
- [ ] Religar o toggle do 003 mid-raid: contínuos estabelecidos do snapshot SEM one-shot e SEM toast (paridade com avaliação inicial).
- [ ] Bot mancando em combate: cap prevalece sobre pedido de sprint do SAIN sem quebrar locomoção (sem rubber-banding no peer); headless sem render = comportamento idêntico (log).
- [ ] Analgésico flicker: contínuos seguem; agachar respeita cooldown (motor).
- [ ] Desligar o toggle do 003 mid-raid: caps/efeitos desfeitos na hora.
- [ ] Compat de velocidade: CustomClasses + Skills Extended ativos → multiplicadores compõem (D12); sem overwrite mudo.

## Fora de escopo

- [x] Cair + ciclo de levantar + derrubar/arbitragem D2 (item 004).
- [x] Estômago (006) — só a primitiva de agachar nasce aqui.
- [x] Balanceamento fino de N1/N2 (calibração inicial + configs; ajuste pós-validação).

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisões 2, 4, 5, 11, 14, 16, 18, 19, 20, 22; D1, D2, D7, D10, D12, D16, D17, D19
- [002-motor-estados/](../002-motor-estados/) — eventos/snapshot/anti-thrash/i18n/log
- [001-spike-primitivas/](../001-spike-primitivas/) — P1 (mancar/velocidade), P4 (pose/guards), P5 (voz leve), P6 (bots)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Item criado via backlog Trauma 2.0; spec funcional criada via `/create-spec` (rodada 1 embutida) |
| 2026-07-18 | Revisão rodada 2 (adversarial) — 12 achados aplicados: interim vira decisão do CONSUMIDOR cobrindo as 2 linhas Cair sem one-shot (resolve bloqueador), semântica total-alvo + delta runtime + ±5 p.p. + método de medição, escopo da primitiva reduzido ao agachar (derrubar+D2 → 004), colunas com-analgésico das linhas Cair enumeradas + AC, clamp de delta negativo, cancelamento de disparo adiado, religar toggle, agachar só-para-baixo, ACs de bot dip + headless, guards D7 integrais, referências completadas, toggle default ON na entrega |
