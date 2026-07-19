# 006 — Estômago: agachar probabilístico

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Quarto consumidor do motor (002) — após pernas (003), ciclo de queda (004) e braços (005) — e o menor da matriz: ao ZERAR o estômago, agachar involuntário com p=75% (25% sob analgésico), re-rolado a cada zerada (decisão 7). **O roll não existe no motor: é entrega deste item**, consumindo a transição de linha que o motor já publica com o analgésico congelado do instante da zerada (D8). Reusa a primitiva de agachar do 003 (`InvoluntaryCrouch`) e a arbitragem D2 do 004 (absorção com ciclo de queda ativo). Substitui o "sem ar" legado do Sistema de Estomago.

## Comportamento atual

Legado (Sistema de Estomago, `modded/Patches/Trauma/HealthPatches.cs:98-122`): hit ≥35 no estômago (bala/explosão/sniper) com o jogador **fora de prone** (agachado também dispara) → stamina zerada + agachar forçado + voz "Gut" — por HIT, sem probabilidade, sem analgésico, sem re-roll por zerada, e **inclui bots do processo dono** (sem filtro de IA). Desde o 004 o bloco é suprimido com ciclo de queda engajado (fronteira documentada até o 006).

Motor 002 (v1.6.0): publica a **transição de linha** do estômago (entrada/saída de zerado) carregando o analgésico LATCHED do instante da zerada (D8) — mas **não publica one-shot de estômago nem rola probabilidade** (`modded/Patches/Trauma/TraumaEngine.cs:567-573` só publica one-shot para linhas de pernas; `modded/Patches/Trauma/TraumaEngineState.cs:29` declara o roll p=75/25 como entrega do 006). Infra já pronta, sem call site: log de roll em formato estável (`modded/Patches/Trauma/TraumaObservability.cs:41`), texto de toast EN/PT da linha de estômago (`modded/Patches/Trauma/TraumaLocale.cs:21`), id de consumidor de estômago no registry e placeholder de config "Stomach Effects (item 006)" OFF.

## Comportamento desejado

1. **Gatilho = transição real para ZERADO** (estado, não hit; não-establishing): na transição publicada pelo motor, rolar p=75% sem analgésico / p=25% com — usando o **analgésico LATCHED que a própria transição carrega** (D8: instante da detecção da zerada; sem re-consulta e sem re-roll por mudança posterior). Sucesso → agachar involuntário one-shot via primitiva do 003 (só-para-baixo, sem lock, guards D7 com adiamento/cancelamento e refund). Falha → nenhum efeito físico; o log registra o roll (D19) e o toast de 1ª ocorrência ainda pode disparar — ele é da LINHA, não do roll (comportamento 10).
2. **Re-roll a cada zerada** (decisão 7): curou → zerou de novo → rola de novo. Sem transição, sem roll: estômago que PERMANECE zerado não re-rola — nem por dano adicional na região, nem por mudança de analgésico (latch D8, contrato do 002).
3. **Arbitragem D2** (herdada do 004): ciclo de queda engajado absorve o agachar (refund, log ABSORB); prone/agachado atual = NOOP com refund (contrato do 003).
4. **Dedup da fila de adiados — corner residual**: o hit de dedup por `(player, kind)` já atualiza `Region` (fix CR-02-03 do 003, entregue), o que evita re-validar a linha ERRADA. O corner que RESTA: um adiado de agachar de PERNAS pendente (ex.: jogador em BTR com pernas N2) que recebe por cima a intenção do ESTÔMAGO (mesmo kind) vira UMA entrada re-alvejada — se a região que a entrada aponta for curada antes da execução, o cancelamento derruba TAMBÉM a intenção da outra região ainda válida (a linha dela não re-publica; one-shot perdido para sempre). Requisito comportamental: **enquanto qualquer região que pediu o agachar ainda o exigir, a cura da OUTRA região não pode cancelar o adiado**. A técnica escolhe o mecanismo (chave `(player, kind, region)` com o segundo virando NOOP natural pós-execução, ou re-validação multi-região da entrada mesclada).
5. **Bots inclusos** (decisão 11): mesmo roll e mesmo log; agachar de bot via dip do 003 (`BotCrouchDip`), com a mesma absorção D2 para bot em hold; funcional no headless (dono dos bots).
6. **Cooldown anti-thrash por tipo — compartilhado com pernas** (decisão 19): o agachar do estômago respeita o cooldown por (jogador, tipo=agachar), que é o MESMO do agachar de pernas do 003 — dois agachares (pernas+estômago) dentro da janela de 3-5s colapsam em um (mesma ação física; aceito). Sucesso de roll suprimido por cooldown é LOGADO como supressão e não re-tenta (a zerada é evento único). Premissa registrada para o item 011.
7. **Sem vazamento entre consumidores — independência bidirecional dos toggles**: com o toggle do 006 OFF (e pernas/003 ON), zerada de estômago não gera roll nem agachar por NENHUM caminho; com o 006 ON e o 003 OFF, o agachar do estômago funciona normalmente (não pode depender do consumidor de pernas como executor). Atenção da técnica: o consumidor de pernas hoje reage a qualquer one-shot de agachar publicado sem discriminar região (`modded/Patches/Trauma/TraumaLegsConsumer.cs:130`) — o mecanismo de publicação/consumo escolhido não pode fazer o 003 executar o agachar do estômago, nem o inverso.
8. **Substituição do legado (D10):** na entrega, o bloco "sem ar" por hit fica permanentemente inerte — stamina zerada, pose forçada E voz "Gut", inclusive para bots — independente da key legada "Sistema de Estomago" (marcada INERTE no tooltip; remoção no 010). O agachar novo NÃO ganha voz dedicada (paridade com o agachar do 003, que é silencioso — feedback é a pose + toast; premissa registrada para o 011).
9. **Config**: probabilidades expostas (0-100%, defaults 75/25, independentes entre si — sem clamp; inverter é permitido; premissa para o 011); toggle do consumidor via rename-at-delivery do placeholder "Stomach Effects (item 006)" → "Stomach Effects" (ON, órfã deletada — padrão 003/004/005).
10. **Feedback**: toast de 1ª ocorrência EN/PT via infra do motor — dispara na ENTRADA da linha de estômago zerado com consumidor ativo, **independente do resultado do roll** (decisão 20 é feedback de estado; texto já existe na tabela). Roll logado com p usado e resultado no formato estável da infra (D19; contexto de dano fica no log verbose do motor — o campo "dano" do D19 pertence aos rolls por hit do desmaio/007; premissa para o 011).

## Critérios de aceite

- [ ] **Determinístico (config nos extremos):** probabilidade a 100% → toda zerada real com jogador de pé desfecha pelos contratos do 003: agacha, OU adia (D7), OU absorve (ciclo do 004), OU é suprimida por cooldown — sempre com log do desfecho; pose já baixa = NOOP logado. A 0% → nenhuma agacha, mas TODO roll continua logado (p=0, falha). Cada roll no log registra p usado e resultado.
- [ ] **Estatístico (defaults 75/25):** série de exatamente 20 rolls logados (pode somar várias raids — o log agrega) sem analgésico → 11 a 19 sucessos; série de 20 com analgésico ativo no instante da zerada → 1 a 9 sucessos. Fora da faixa: repetir a série 1×; segunda falha consecutiva reprova.
- [ ] Curar o estômago e zerar de novo → novo roll (log mostra 2 rolls). Estômago permanecendo zerado → nenhum roll adicional: nem por novo hit na região zerada, nem por tomar/expirar analgésico (latch D8).
- [ ] Roll com sucesso durante ciclo de queda (004) ativo → ABSORB com refund (log); em pé normal → agacha via primitiva (animação vanilla, levanta livre em seguida).
- [ ] Bot dono (host/headless) zerando estômago rola (mesmo log) e dipa conforme resultado; bot em hold do 004 absorve (log ABSORB).
- [ ] **Cooldown compartilhado com pernas:** agachar de pernas (003) executado e zerada de estômago com roll-sucesso dentro da janela de 3-5s → supressão LOGADA, sem segundo agachar (e vice-versa). Nenhum caso de dois agachares na mesma janela.
- [ ] **Independência dos toggles:** 006 OFF / 003 ON → zerada de estômago não gera roll (log ausente) nem agachar por nenhum caminho — inclusive nenhum efeito executado pelo consumidor de pernas. 006 ON / 003 OFF → zerada de estômago rola e agacha normalmente.
- [ ] **Legado "sem ar" inerte:** hit ≥35 no estômago (com HP suficiente para NÃO zerar) → sem queda de stamina, sem pose forçada, sem voz "Gut" — para jogador E bot, com a key legada "Sistema de Estomago" em qualquer estado. Só a regra nova por zerada existe.
- [ ] **Fika/multiplayer:** peer vê o agachar do dono via pose sync nativo; espelhos não rolam nem aplicam (D16 — nenhum log de roll no processo não-dono para aquele jogador); client zerando estômago = roll e efeito no próprio client (dono), pose visível ao host; toast é feedback local (bots/headless não toastam).
- [ ] **Estado entre raids:** reset via motor; spawn com estômago já zerado NÃO rola, não agacha e não toasta (establishing); na MESMA raid, curar o estômago estabelecido e re-zerar → roll normal (primeira transição real). Raid seguinte zera contadores de toast/cooldown.

## Corner cases

- [ ] Zerada dupla no mesmo frame (2 pellets): 1 transição → 1 roll (consolidação por dirty-flag do motor, contrato do 002).
- [ ] Analgésico tomado ENTRE o hit e a avaliação do motor (mesmo tick): vale o valor LATCHED que a transição publicada carrega (contrato do 002 — o instante canônico é a detecção da zerada pelo motor); o 006 nunca re-consulta o predicado por conta própria.
- [ ] Roll com sucesso mas contexto D7 (escada/BTR/vault): adia; curado antes da execução → cancelado com refund (contratos do 003).
- [ ] Toggle do 006 OFF mid-raid: rolls param; adiados do ESTÔMAGO cancelados com refund SEM varrer adiados de agachar de PERNAS (paridade com o cancel-por-kind do 003 — e sem matar a intenção de pernas de uma entrada mesclada, ver comportamento 4); legado NÃO volta (inerte permanente).
- [ ] Desmaio/downed no frame do roll: nenhuma pose forçada em jogador inconsciente. Realidade dos contratos: o cinto de pausa do 004 cobre SÓ o tipo queda (cancel na entrada da pausa é por kind); a proteção do agachar hoje é o NOOP pose-baixa com refund (o desmaio força prone na entrada) — verificável pelo log NOOP. A técnica garante que nenhum caminho de agachar force pose em inconsciente; premissa para o 011: se o 007 mudar a pose do desmaio, revisitar.
- [ ] Cura do estômago com adiado pendente (D7): re-validação da fila cancela com refund; não há efeito contínuo a desfazer (one-shot puro).
- [ ] Cirurgia FullRestore do estômago (sem evento) → reconciliação do motor detecta a saída; próxima zerada re-rola normalmente.

## Fora de escopo

- [x] Migração/remoção da key "Sistema de Estomago" (010).
- [x] Voz para o agachar do estômago (decisão deste item: SEM voz dedicada — paridade com o agachar silencioso do 003; a voz "Gut" morre com o bloco legado; sem áudio novo).
- [x] Efeitos contínuos de estômago (hidratação/energia vanilla intocados).

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — matriz estômago, decisões 7, 11, 13, 19, 20, 22; D2, D7, D8, D10, D16, D19
- [003-pernas-mancar/](../003-pernas-mancar/) — primitiva de agachar + dedup CR-02-03
- [004-pernas-cair-ciclo/](../004-pernas-cair-ciclo/) — absorção D2 + predicado de pausa
- [002-motor-estados/](../002-motor-estados/) — latch D8 do analgésico, barramento de transições/one-shot, establishing, toasts

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec funcional criada via `/create-spec` (memória: P-3.5/P-3.6; contratos reais do 003/004 citados como vinculantes) |
| 2026-07-19 | Revisão `/review-spec` rodada 1 — alinhado ao código v1.6.0: roll NÃO existe no motor (é entrega do 006; motor publica só a linha com analgésico latched); corner residual do dedup concretizado (entrada mesclada × cura de uma região); cooldown anti-thrash compartilhado com pernas explicitado; corner de vazamento p/ consumidor de pernas; cinto de pausa do 004 corrigido (cobre só quedas; agachar protege por NOOP pose-baixa); legado corrigido (fora-de-prone, inclui bots, voz "Gut" morre); 006 é o QUARTO consumidor; ACs de probabilidade desdobrados em determinístico (0/100%) + estatístico (série 20, faixas 11-19 e 1-9); ACs novos (cooldown compartilhado, toggle-off sem vazamento, re-roll pós-establishing); decisão registrada: agachar sem voz dedicada |
| 2026-07-19 | Revisão `/review-spec` rodada 2 (passada adversarial sobre a rodada 1) — série estatística fixada em exatamente 20 rolls (faixas eram calibradas p/ n=20, "≥20" as invalidava; série pode somar raids); AC determinístico enumera os desfechos legítimos (agacha/adia/absorve/suprime/NOOP, todos logados); contradição "falha → nada" × toast resolvida (toast é da linha, não do roll); independência BIDIRECIONAL dos toggles (006 ON / 003 OFF funciona — não pode depender do consumidor de pernas como executor); referência ao 002 corrigida ("roll D8" → latch D8; o 002 não rola). 0 marcadores `<!-- review: -->` pendentes |
