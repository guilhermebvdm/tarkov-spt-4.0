# 024 — Descarte instantâneo das mãos no EmergencyDrop

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-09-12

## Visão geral

O drop de emergência existe pra deixar o jogador pronto pra combate **imediatamente** quando algo dá errado no meio de uma cura (ex: usando um kit cirúrgico e sendo atacado) — a proposta é largar o item na hora, independente do que estava acontecendo, pra ter as mãos livres e puxar a arma sem esperar animação nenhuma. Hoje isso não acontece: o jogador vê a mesma animação de guardar o kit tocar do início ao fim, igual a um cancelamento normal, porque a tentativa do mod de trazer a arma de volta mais rápido nunca funcionou de verdade — ela sempre esbarra numa trava de segurança do próprio jogo e é ignorada silenciosamente.

**Atualização (investigação técnica):** inicialmente se pensou em *acelerar* a animação de guardar o item. Investigação mais profunda encontrou que existe uma forma de **pular essa animação inteiramente** (não só acelerar) sem precisar mexer em código central do jogo — usando um recurso que o próprio jogo já expõe publicamente pra descartar o item das mãos de forma instantânea, o mesmo tipo de recurso que o jogo usa internamente em outras situações. Isso muda a meta deste item de "mais rápido" pra "instantâneo", que é o que a proposta original do drop de emergência sempre quis ser.

## Comportamento atual

- Ao acionar o drop de emergência durante o uso de um item médico, o mod interrompe o efeito de cura e "força" o fim da animação — mas essa forma de forçar só avisa a lógica do jogo que a operação pode ser encerrada; ela **não pula** o gesto visual de guardar o item, que continua tocando pelo tempo normal configurado na animação (o mesmo tempo de um cancelamento comum).
- Logo em seguida, o mod tenta trazer a arma de volta pras mãos — mas essa tentativa esbarra numa trava de segurança nativa do jogo que impede iniciar uma nova ação de mãos enquanto o gesto de guardar o item anterior ainda está em andamento. Como a tentativa acontece bem antes desse gesto terminar, ela é **ignorada silenciosamente**: sem erro, sem log, sem efeito nenhum.
- Resultado observável: o jogador vê a mesma animação de guardar o kit tocar do início ao fim, igual a um cancelamento normal (segurar/clicar pra cancelar sem dropar) — a arma só volta ao normal quando essa animação termina sozinha, não antes.
- O drop do item em si (tirá-lo do inventário e jogá-lo no chão) **já funciona corretamente e de forma imediata hoje** — o problema é só a parte de "mãos livres pra usar arma de novo".
- <!-- review: achado técnico confirmado por leitura direta do código do jogo — o tempo de "guardar o kit" não vem de uma animação configurável nem de uma trava que dá pra acelerar por parâmetro: é um tempo de espera FIXO, escrito no código do próprio jogo, que ignora qualquer tentativa de acelerar por fora. Por outro lado, existe um recurso público e já usado pelo próprio jogo em outras situações que descarta o item das mãos de forma totalmente instantânea, sem passar por esse tempo de espera — é esse recurso que este item deve usar, ao invés de tentar acelerar a espera fixa (que não é acelerável). -->
- <!-- review: escopo restrito a itens de cirurgia — decisão do usuário, 2026-09-12 --> O drop de emergência só faz sentido pra itens que têm uma animação de encerramento longa o bastante pra incomodar — hoje isso é só o kit cirúrgico (CMS/Surv12). Bandagem, tala, torniquete e medkit comum têm um encerramento curto o suficiente pra ser confortável manter o comportamento vanilla — por isso, a tecla de drop de emergência passa a **só fazer efeito durante o uso de um item de cirurgia**; pra qualquer outro item médico, apertar a tecla não tem efeito nenhum (o jogador cancela do jeito de sempre, com Mouse0, ou espera terminar).
- Toda essa sequência (drop de emergência, cancelamento normal, animação simulada) só roda quando quem está curando é o **próprio jogador local** — curar um bot ou ser curado por outro jogador não passa por nenhum desse código, então este item não afeta esses casos.
- O cancelamento normal (sem dropar) já aplica uma penalidade — perde 1 carga/uso do item se o cancelamento acontecer depois de já ter passado 1 segundo de uso — mas o drop de emergência hoje **não aplica** essa mesma penalidade, dropando sem custo algum independente do tempo decorrido.
- <!-- review: validado em conversa (leitura direta do código, não suposição) — o drop de emergência hoje não aplica NENHUM consumo de carga em nenhum ponto do seu fluxo (nem o "consumo final" de tratamento bem-sucedido, nem a penalidade de cancelamento tardio). Adicionar a penalidade neste item não corre risco de duplo-custo, porque hoje não existe custo nenhum sendo aplicado por esse caminho. -->

## Comportamento desejado

- **Escopo: exclusivo para itens de cirurgia (CMS/Surv12).** A tecla de drop de emergência só produz efeito quando o item em uso é de cirurgia (`stats.IsSurgery`); durante o uso de qualquer outro item médico, apertar a tecla não faz nada — não cancela, não dropa, não interfere no uso normal.
- Ao acionar o drop de emergência durante uma cirurgia, as mãos do jogador ficam livres e a arma volta a ser utilizável **imediatamente** — sem esperar nenhuma animação de guardar o item, mesmo que isso pareça abrupto/brusco visualmente (é exatamente a intenção: uma ação de emergência, não uma transição suave).
- O drop do item continua imediato, exatamente como já é hoje — essa mudança não pode atrasar isso.
- A mesma penalidade de "perda de carga por cancelamento tardio" que já existe no cancelamento normal passa a valer também pro drop de emergência, pelas mesmas regras (consistência entre os dois jeitos de cancelar).

## Critérios de aceite

- [ ] Ao acionar o drop de emergência durante o uso de um item de **cirurgia** (CMS/Surv12), a arma está pronta pra atirar **no mesmo instante** (sem espera perceptível), não só "mais rápido que antes".
- [ ] Ao acionar a mesma tecla durante o uso de um item **não-cirúrgico** (bandagem, tala, torniquete, medkit comum), nada acontece — sem cancelar, sem dropar, sem qualquer efeito colateral no uso em andamento.
- [ ] O item de cura continua sendo dropado imediatamente ao acionar o drop de emergência durante cirurgia — sem regressão nesse comportamento já existente.
- [ ] A penalização de perda de carga por cancelamento tardio (≥ 1s de uso) passa a se aplicar também ao drop de emergência, com a mesma regra já usada no cancelamento normal.
- [ ] **Fika/multiplayer:** validado curando (ou tentando curar) um aliado em raid coop e acionando o drop de emergência nesse meio-tempo — nenhuma trava de mãos nem log de colisão do lado FIKA (itens 003/004/006) como consequência desta mudança.
- [ ] **Estado entre raids:** N/A — a mudança é só de sequenciamento/tempo dentro de uma única ativação do drop de emergência; não introduz nem depende de estado persistente entre raids.

## Corner cases

- [ ] Jogador aciona o drop de emergência bem no início do uso do item (quase nenhum tempo decorrido) — precisa funcionar igual (instantâneo), sem aplicar a penalidade de cancelamento tardio (< 1s).
- [ ] Jogador aciona o drop de emergência e, no exato momento, morre ou é derrubado — o descarte abrupto das mãos não pode deixar o jogo num estado inconsistente (item duplicado, mãos "fantasma", etc.) nesse cruzamento de eventos.
- [ ] Jogador aciona o drop de emergência duas vezes seguidas rapidamente, ou aciona logo depois de um cancelamento normal — não pode gerar comportamento inconsistente, log duplicado, ou aplicar a penalidade de carga mais de uma vez pra uma única cura.
- [ ] Item sendo usado não tem carga/recurso rastreável (item de uso único sem contador) — a lógica de penalização por cancelamento tardio precisa se comportar do mesmo jeito que já se comporta hoje no cancelamento normal para esse tipo de item.
- [ ] O descarte abrupto das mãos é estritamente local (só na visão/mãos do jogador que acionou o drop de emergência) — não pode afetar como outros jogadores observando essa cura em coop veem a transição.
- [x] ~~Item de uma mão só (bandagem, tala) usado no drop de emergência~~ — **resolvido pela decisão de escopo acima:** esses itens não são cirúrgicos, então a tecla de drop de emergência não tem mais efeito neles — não há mais o que confirmar aqui.
- <!-- review: decisão técnica a confirmar na spec técnica — descartar as mãos de forma abrupta (sem passar pelo fluxo normal de "soltar o item") pode interagir de forma não totalmente previsível com o controle interno do jogo de "existe uma troca de mãos pendente". Precisa de validação (idealmente em raid real) de que isso não deixa o jogo pensando que ainda há uma operação de mãos em andamento depois do descarte abrupto. -->

## Fora de escopo

- [x] Mudar o comportamento do cancelamento normal (botão esquerdo do mouse / segurar) — esse já funciona bem hoje e não é tocado por este item.
- [x] Oferecer qualquer versão do drop de emergência (instantâneo ou não) pra itens não-cirúrgicos — decisão explícita: o encerramento vanilla desses itens já é curto o bastante pra ser confortável, então a tecla simplesmente não age neles.
- [ ] A definir: se a mesma técnica de descarte instantâneo deveria (ou não) também ser oferecida como opção pro cancelamento normal (hoje mais suave, de propósito) — decisão de design que fica fora do escopo deste item; aqui é só pro drop de emergência.

## Referências

- [023-sequencia-maos-cura-sem-confirmacao/](../023-sequencia-maos-cura-sem-confirmacao/) — item anterior que corrigiu a falta de confirmação nas operações de mãos da cura; este item aplica o mesmo princípio (esperar confirmação real, não tempo fixo nem tentativa às cegas) a um sinal diferente: o gesto de guardar o item, não o callback de início de uso.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Item criado via `/add-backlog-item`, a partir de investigação em conversa sobre por que o `EmergencyDrop` não conseguia acelerar a devolução da arma — achado: a tentativa de reequipar sempre falhava silenciosamente porque nunca esperava o gesto de guardar terminar, e o jogo não tem como pular esse gesto — só acelerá-lo, por uma via que o mod hoje não usa. |
| 2026-09-12 | Revisão `/review-spec` — 1 gap fechado (confirmado por leitura de código que não há risco de duplo-custo ao adicionar a penalidade) + 2 corner cases adicionados (itens de uma mão só podem não ter o mesmo problema; escopo MainPlayer-only documentado) + 1 nota de escopo adicionada em Comportamento atual. |
| 2026-09-12 | Pivô de abordagem após investigação técnica mais profunda: descoberto que o tempo de "guardar o kit" é uma espera fixa no código do jogo (não uma animação acelerável), mas existe um recurso público do jogo que descarta as mãos de forma totalmente instantânea, pulando essa espera por completo. Spec revisada de "acelerar + esperar confirmação" pra "descarte instantâneo" — meta que era "fora de escopo" (instantâneo) agora é a própria meta do item. |
| 2026-09-12 | Escopo restrito a itens de cirurgia (CMS/Surv12) por decisão do usuário — a espera fixa de 600ms existe pra qualquer item médico, mas só nos itens cirúrgicos ela corresponde a uma animação visível de "fechar o kit" incômoda; nos demais (bandagem, tala, torniquete, medkit comum) o encerramento já é curto o bastante pra ser confortável mantendo vanilla. A tecla de drop de emergência passa a só funcionar durante o uso de um item de cirurgia — resolve de quebra o corner case sobre itens de uma mão só. |
| 2026-09-12 | **Fix pós-implementação (02):** o critério de aceite "a arma está pronta pra atirar no mesmo instante" deixou de ser válido. `TrySetLastEquippedWeapon()` (a chamada que reequipava a arma) causava, em raid real: (1) a arma sendo puxada e guardada sozinha logo em seguida, sem nenhuma ação do jogador; (2) travamento total das mãos (`HandsController` nulo permanente) ao repetir o drop de emergência rápido demais. Causa raiz: essa chamada cria um processo assíncrono nativo do jogo (`Player.SpawnController`) que colide com qualquer operação de mãos iniciada antes dele confirmar — inclusive o próprio jogador reusando o item médico. Removida por completo (experimento proposto pelo usuário, ver `025-emergencydrop-autocirurgia-06-fix-02.md`). **Comportamento atual:** o drop de emergência solta as mãos instantaneamente, mas não reequipa a arma — o jogador aperta a própria tecla de arma se quiser puxá-la de volta. v1.14.5. |
