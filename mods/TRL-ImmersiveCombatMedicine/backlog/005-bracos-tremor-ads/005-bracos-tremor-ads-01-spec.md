# 005 — Braços: Tremor + cancelamento de ADS escalonado

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-18

## Visão geral

Consumidor de braços do motor (002): tremor contínuo gerenciado pelo mod e, com 2 braços comprometidos, cancelamento de ADS após tempo sustentado (4 s Zerar 2 / 3 s Quebrar 2 / 2 s Z2+Q2 — fratura pior que zerado por design, decisão 3) com **lockout de re-ADS** (default 1,5 s; faixa 1–1,5 s configurável) e voz de dor. Analgésico rebaixa conforme a matriz. Substitui a fadiga de mira legada (1 s).

## Comportamento atual

Fadiga de mira legada: com os 2 braços ZERADOS, mirar por 1 s solta a mira (polling) — sem tremor, sem fratura como condição, sem analgésico, sem lockout, sem voz.

## Comportamento desejado

1. **Tremor re-derivado do ESTADO** (linhas de braço da matriz): o analgésico nunca mexe no efeito diretamente — ele rebaixa o estado, e o mod re-deriva o tremor: em Z1/Q1/Z1+Q1 com analgésico o estado vira **Nada e o tremor É removido**; em Z2/Q2/Z2+Q2 o estado vira Tremor, o efeito persiste **e permanece VISÍVEL**. Correção de premissa do D11 (spike P2): o analgésico vanilla NÃO apaga efeito nenhum — nem o tremor-por-dor, que morre com a dor —, ele apenas **suprime o VISUAL de qualquer tremor** enquanto ativo, inclusive o nosso; nas linhas com-analgésico→Tremor o mod contorna essa supressão para o efeito gerenciado, enquanto o tremor-por-dor vanilla segue o comportamento vanilla (suprimido sob analgésico) e coexiste sem intensificação dupla.
2. **Lifecycle de instância única** (D11 pós-spike): cada aplicação do efeito cria instância NOVA (o jogo não funde tremores) — o mod mantém **no máximo 1 instância gerenciada ativa por player** (re-aplicações idempotentes), remove **a própria instância** ao reverter (nunca remoção "por tipo", que pode acertar tremor de dor/stim de terceiros) e **re-estabelece** o efeito se uma remoção externa o comer com o estado ainda ativo.
3. **Cancelamento de ADS escalonado:** com estado de 2 braços ativo, mirar continuamente por N s (4/3/2 conforme a linha; config F12 com faixas, tolerância ±0,25 s em TODOS os N) cancela o ADS pelo **caminho vanilla — confirmado pelo P9**: existe um funil único de mira, e o cancel por ele faz o desmonte completo (respiração/sensibilidade/animação) e é visível ao peer nativamente; detecção da mira sustentada por evento (substitui o polling do legado); soltar a mira reseta o timer. **Mudança de linha mid-ADS** (qualquer causa: dano, cura, analgésico): o timer REINICIA com o N da nova linha (sem cancelamento retroativo); mudar para linha sem cancela-ADS descarta o timer.
4. **Lockout de re-ADS** (decisão 17 — mecanismo confirmado pelo P9): após o cancelamento, re-mirar bloqueado em **todas as rotas de re-entrada** (input, troca de scope, re-aim automático de troca rápida, restauração pós-sobreposição de cano), persistindo à troca de arma (é do jogador); tentativa durante o lockout dispara voz de dor pelo **mecanismo prioritário do P5** (audível mesmo sob vozes de combate), com **throttle de 1 voz por janela de lockout** (cobre mira em modo hold sem spam); tentativas bloqueadas são invisíveis aos peers (nenhum tráfego — sem glitch).
5. **Bots: EXCLUÍDOS do tremor** (decisão deste item — supersede o "tremor cosmético" do D9 pré-spike, refutado como escrito pela pesquisa): em bot o tremor é invisível aos peers, ignorado pela IA de decisão e no-op total no headless; o único canal restante (possível desvio de pontaria do bot no host player-hosted) é condicional, não medido e assimétrico por topologia — portanto sem tremor em bots. Cancela-ADS/lockout seguem não se aplicando a bots (D9). **Premissa p/ item 011**; eventual experimento de pontaria de bot fica fora deste item.
6. **Autoridade dono-only (D16):** tremor, detecção, cancel e lockout rodam só no processo do jogador local; espelhos nunca aplicam efeito; nada arma no headless (lá só existem bots, excluídos).
7. **Substituição incremental (D10):** na entrega, a fadiga de mira legada é removida e o toggle placeholder do motor ("Arms Effects (item 005)") é assumido/renomeado pelo consumidor (rename-at-delivery — padrão do 003); nasce ON (master governa).
8. **Feedback:** toast de 1ª ocorrência via infra do motor (EN/PT — decisão 22); log de cancelamentos/lockouts/timers com valores efetivos (infra D19).
9. **Compat (D13 — confirmado pelo P9):** RecoilRework e FOV-Fix tocam o mesmo funil de mira apenas cosmeticamente — cancel vanilla + lockout no mesmo ponto convivem sem ordenação de patch; o AC de 3 ciclos permanece como validação (sem estado de FOV/mira preso).

## Critérios de aceite

- [ ] Zerar 1 braço (sem analgésico) liga tremor; tomar analgésico REMOVE o tremor (estado Nada); expirar re-aplica; curar o braço remove ≤1 s (própria/remota/cirurgia via motor).
- [ ] Com 2 braços zerados + analgésico: estado rebaixa para a linha Tremor e o tremor permanece **visível** sob o analgésico (supressão visual vanilla contornada — P2); tremor-por-dor vanilla, se presente, segue suprimido.
- [ ] Ciclos repetidos ferir→analgésico→expirar→curar mantêm **no máximo 1 instância** gerenciada do efeito (log aplicar/remover pareado); ao reverter, remoção limpa sem tremor residual; remoção externa com estado ativo é re-estabelecida (verificável por log).
- [ ] Com 2 braços zerados: ADS sustentado cancela em 4 s ±0,25; soltar e re-mirar antes reseta; com Z2+Q2 cancela em 2 s ±0,25.
- [ ] Mirando com Z2 há ~3 s, quebrar o 2º braço (vira Z2+Q2): cancela ~2 s APÓS a mudança (timer reiniciado — não instantâneo); tomar analgésico com timer correndo (vira linha Tremor): timer descartado, tremor persiste, mira livre.
- [ ] Após cancelamento, re-ADS bloqueado pelo lockout em qualquer rota (input, troca de scope, re-aim automático); voz de dor prioritária na tentativa (audível em combate — P5) com **1 voz por janela de lockout**; comportamento correto nos DOIS modos de mira (hold e toggle — sem spam de voz nem bloqueio furado); passado o lockout, ciclo recomeça.
- [ ] Fadiga legada inerte; com RecoilRework + FOV-Fix ativos, 3 ciclos seguidos de cancelamento sem FOV/zoom preso (validação do D13 confirmado).
- [ ] Bot com braços feridos NÃO recebe tremor (exclusão registrada — log confirma) e nunca sofre cancela-ADS/lockout (D9; log).
- [ ] **Fika/multiplayer:** peer NÃO vê o tremor do dono (feedback first-person; o estado sinca como dado, sem visual — limitação aceita, P2); cancel forçado VISÍVEL ao peer nativamente (arma do dono abaixa no espelho); tentativas bloqueadas no lockout invisíveis ao peer (sem glitch); voz de dor audível aos peers (P5).
- [ ] **Estado entre raids:** reset via motor; spawn com braço ferido estabelece tremor sem toast nem voz (avaliação inicial estabelecedora — padrão do motor).

## Corner cases

- [ ] Cancelamento no meio de rajada: arma funcional hip-fire; sem travar bolt/animação.
- [ ] Trocar de arma durante o lockout: lockout persiste (no jogador, não na arma).
- [ ] **Desmaio durante ADS/lockout (D3):** a queda da mira pelo desmaio reseta o timer como soltar normal; lockout expira em tempo real sem efeito colateral; nenhuma voz do 005 durante inconsciência; ao acordar com estado ativo, tremor re-estabelecido do snapshot.
- [ ] Tremor-por-dor vanilla ativo + estado de braço: curar o braço remove SÓ o nosso efeito — o vanilla permanece com o comportamento vanilla (suprimido enquanto houver analgésico; morre com a dor); coexistência sem tremor "duplo".
- [ ] Tentativa de re-ADS durante o lockout pode recolher bússola/item da mão esquerda (colateral cosmético raro do caminho vanilla — P9): aceito como limitação; reavaliar no playtest.
- [ ] Scopes com PiP/FOV mods: cancelar dentro de scope sem resolução/PiP inconsistente (suíte D20).
- [ ] Desligar o toggle do 005 mid-raid: tremor removido e lockout cancelado; religar: tremor estabelecido do snapshot SEM toast; toast volta a valer para transições novas.

## Fora de escopo

- [x] Efeito mecânico de tremor na dispersão (o nativo já faz o que faz — sem scatter custom).
- [x] Progressão de lockout (rejeitada na validação — lockout fixo configurável).
- [x] Tremor em bots (excluído — ver Comportamento desejado 5; eventual experimento de pontaria de bot em item futuro).
- [x] Lockout sobre "mira" de itens usáveis (funil separado — P9; armas de fogo e estacionárias/montadas cobertas).

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisões 3, 11, 13, 14, 17, 22; D3, D9, D10, D11, D13, D16, D19, D20
- [002-motor-estados/](../002-motor-estados/) — eventos/snapshot/i18n/log
- [001-spike-primitivas/](../001-spike-primitivas/) — P2 (tremor), P5 (vozes), P9 (ADS/lockout)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Item criado via backlog Trauma 2.0; spec funcional criada via `/create-spec` (rodada 1 embutida) |
| 2026-07-18 | Revisão rodada 2 (adversarial) — 8 achados aplicados: tremor re-derivado do estado (resolve contradição com a matriz: Z1+analgésico REMOVE), regra geral de mudança de linha mid-ADS + AC dedicado (exemplo impossível corrigido), corner de desmaio D3, coexistência com tremor-por-dor vanilla, lockout default 1,5 s + ±0,25 s em todos os N, AC de bot com processo de validação (dono host/headless + peer), religar toggle sem toast, throttle de voz verificável |
| 2026-07-19 | Rodada de alinhamento pós-spike (P2/P9) — 10 ajustes: D11 corrigido (analgésico = supressão VISUAL, tremor gerenciado visível nas linhas com-analgésico→Tremor + AC); lifecycle de instância única com re-estabelecimento + AC; cancel confirmado no funil vanilla com detecção por evento; lockout com rotas de re-entrada + voz prioritária com throttle 1/janela + validação hold/toggle; bots EXCLUÍDOS do tremor (D9 refutado como escrito — premissa p/ item 011) + AC invertido; AC Fika realista (peer não vê tremor; cancel visível nativo; lockout invisível); rename-at-delivery do toggle placeholder (padrão 003); D13 confirmado (compat sem ordenação); limitações P9 registradas (itens usáveis fora do lockout; colateral bússola/mão esquerda); autoridade D16 dono-only + headless explícita, decisão 22 e coexistência vanilla ajustadas |
