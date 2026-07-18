# 005 — Braços: Tremor + cancelamento de ADS escalonado

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-18

## Visão geral

Consumidor de braços do motor (002): tremor contínuo gerenciado pelo mod e, com 2 braços comprometidos, cancelamento de ADS após tempo sustentado (4 s Zerar 2 / 3 s Quebrar 2 / 2 s Z2+Q2 — fratura pior que zerado por design, decisão 3) com **lockout de re-ADS** (default 1,5 s; faixa 1–1,5 s configurável) e voz de dor. Analgésico rebaixa conforme a matriz. Substitui a fadiga de mira legada (1 s).

## Comportamento atual

Fadiga de mira legada: com os 2 braços ZERADOS, mirar por 1 s solta a mira (polling) — sem tremor, sem fratura como condição, sem analgésico, sem lockout, sem voz.

## Comportamento desejado

1. **Tremor re-derivado do ESTADO** (linhas de braço da matriz): o analgésico nunca mexe no efeito diretamente — ele rebaixa o estado, e o mod re-deriva o tremor: em Z1/Q1/Z1+Q1 com analgésico o estado vira **Nada e o tremor É removido**; em Z2/Q2/Z2+Q2 o estado vira Tremor e o efeito persiste. O que nunca acontece é o painkiller apagar o NOSSO efeito por baixo (D11) — como faz com o tremor-por-dor vanilla, que segue independente e coexiste sem intensificação dupla.
2. **Cancelamento de ADS escalonado:** com estado de 2 braços ativo, mirar continuamente por N s (4/3/2 conforme a linha; configuráveis, tolerância ±0,25 s em TODOS os N) cancela o ADS pelo caminho vanilla (P9); soltar a mira reseta o timer. **Mudança de linha mid-ADS** (qualquer causa: dano, cura, analgésico): o timer REINICIA com o N da nova linha (sem cancelamento retroativo); mudar para linha sem cancela-ADS descarta o timer.
3. **Lockout de re-ADS** (decisão 17): após o cancelamento, re-mirar bloqueado pelo lockout; tentativa durante o lockout dispara voz de dor (P5, som leve).
4. **Bots:** tremor aplicado (cosmético — D9) no dono (host/headless); cancela-ADS NÃO se aplica a bots (D9).
5. **Substituição incremental (D10):** na entrega, a fadiga de mira legada é removida; toggle do consumidor nasce ON (master governa).
6. **Feedback:** toast de 1ª ocorrência via infra do motor (EN/PT); log de cancelamentos/lockouts (infra D19).
7. **Compat (D13):** o cancelamento usa o caminho que RecoilRework/FOV-Fix respeitam (mapa de escritores do P9); sem estado de FOV/mira preso.

## Critérios de aceite

- [ ] Zerar 1 braço (sem analgésico) liga tremor; tomar analgésico REMOVE o tremor (estado Nada); expirar re-aplica; curar o braço remove ≤1 s (própria/remota/cirurgia via motor).
- [ ] Com 2 braços zerados: ADS sustentado cancela em 4 s ±0,25; soltar e re-mirar antes reseta; com Z2+Q2 cancela em 2 s ±0,25.
- [ ] Mirando com Z2 há ~3 s, quebrar o 2º braço (vira Z2+Q2): cancela ~2 s APÓS a mudança (timer reiniciado — não instantâneo); tomar analgésico com timer correndo (vira linha Tremor): timer descartado, tremor persiste, mira livre.
- [ ] Após cancelamento, re-ADS bloqueado pelo lockout com voz de dor em CADA tentativa (respeitando o espaçamento mínimo do P5 — nenhuma tentativa silenciosa); passado o lockout, ciclo recomeça.
- [ ] Fadiga legada inerte; com RecoilRework + FOV-Fix ativos, 3 ciclos seguidos de cancelamento sem FOV/zoom preso.
- [ ] Bot com braços feridos treme — aplicado no dono (host/headless, log) e VISÍVEL no processo de um peer olhando o bot; bot nunca sofre cancela-ADS (log confirma exclusão).
- [ ] **Fika/multiplayer:** tremor do dono visível ao peer (sync de condição/efeito — P2); cancelamento/lockout locais do dono (peer não percebe glitch).
- [ ] **Estado entre raids:** reset via motor; spawn com braço ferido estabelece tremor sem toast (avaliação inicial).

## Corner cases

- [ ] Cancelamento no meio de rajada: arma funcional hip-fire; sem travar bolt/animação.
- [ ] Trocar de arma durante o lockout: lockout persiste (no jogador, não na arma).
- [ ] **Desmaio durante ADS/lockout (D3):** a queda da mira pelo desmaio reseta o timer como soltar normal; lockout expira em tempo real sem efeito colateral; nenhuma voz do 005 durante inconsciência; ao acordar com estado ativo, tremor re-estabelecido do snapshot.
- [ ] Tremor-por-dor vanilla ativo + estado de braço: curar o braço remove SÓ o nosso efeito — o vanilla permanece; coexistência sem tremor "duplo".
- [ ] Scopes com PiP/FOV mods: cancelar dentro de scope sem resolução/PiP inconsistente (suíte D20).
- [ ] Desligar o toggle do 005 mid-raid: tremor removido e lockout cancelado; religar: tremor estabelecido do snapshot SEM toast; toast volta a valer para transições novas.

## Fora de escopo

- [x] Efeito mecânico de tremor na dispersão (o nativo já faz o que faz — sem scatter custom).
- [x] Progressão de lockout (rejeitada na validação — lockout fixo configurável).

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisões 3, 11, 13, 14, 17, 22; D3, D9, D10, D11, D13, D19, D20
- [002-motor-estados/](../002-motor-estados/) — eventos/snapshot/i18n/log
- [001-spike-primitivas/](../001-spike-primitivas/) — P2 (tremor), P5 (vozes), P9 (ADS/lockout)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Item criado via backlog Trauma 2.0; spec funcional criada via `/create-spec` (rodada 1 embutida) |
| 2026-07-18 | Revisão rodada 2 (adversarial) — 8 achados aplicados: tremor re-derivado do estado (resolve contradição com a matriz: Z1+analgésico REMOVE), regra geral de mudança de linha mid-ADS + AC dedicado (exemplo impossível corrigido), corner de desmaio D3, coexistência com tremor-por-dor vanilla, lockout default 1,5 s + ±0,25 s em todos os N, AC de bot com processo de validação (dono host/headless + peer), religar toggle sem toast, throttle de voz verificável |
