# 001 — Swap de magazine no mesmo slot ao recarregar com R sem espaço

**Mod:** UIFixes
**Status:** Backlog
**Criado:** 2026-09-06

## Visão geral

O mod já oferece uma opção (existente, hoje ligada por padrão) que faz o carregador antigo ocupar o mesmo espaço do colete de onde saiu o carregador novo, ao recarregar apertando "R" fora da tela de inventário. Isso funciona quando há espaço livre em outro lugar do colete. Porém, quando o colete está 100% sem espaço livre (nenhuma vaga sobrando em nenhum outro slot), a troca simplesmente não acontece — o comportamento cai de volta para o padrão do próprio jogo, que derruba o carregador antigo no chão em vez de colocá-lo de volta no mesmo espaço vago.

## Comportamento atual

- Apertar "R" com a arma empunhada e um carregador compatível disponível no colete/bolsos executa a recarga, movendo o carregador antigo para uma vaga livre no equipamento, quando existe uma.
- Quando não existe nenhuma vaga livre em lugar nenhum do colete, a troca no mesmo espaço não acontece, e o comportamento observado é o padrão do jogo: o carregador antigo é derrubado no chão.
- Esse cenário é justamente o mais comum na prática: o jogador costuma reabastecer carregadores exatamente quando o colete já está lotado, então é quando a funcionalidade de troca deveria atuar com mais força — e é exatamente quando ela falha.
- O mesmo tipo de troca, feita arrastando o carregador manualmente pelo inventário até a arma, já funciona corretamente mesmo com o colete cheio (corrigido em item anterior do mod FIKA, backlog `003-magazine-swap-inplace-fix`).

## Comportamento desejado

- Apertar "R" com o colete 100% sem espaço livre deve colocar o carregador antigo exatamente no espaço de onde saiu o carregador novo — nunca derrubar no chão — reproduzindo o mesmo resultado que já existe hoje quando arrastando manualmente pelo inventário.
- O comportamento com espaço livre disponível (que já funciona) não pode regredir.
- A troca continua condicionada à opção do mod que já ativa essa funcionalidade — sem ela ligada, o comportamento padrão do jogo permanece.

## Critérios de aceite

- [ ] Apertar "R" com o colete 100% sem espaço livre resulta no carregador antigo ocupando exatamente o espaço de onde saiu o carregador novo, sem cair no chão.
- [ ] O mesmo teste, com pelo menos uma vaga livre no colete, continua funcionando como hoje (sem regressão).
- [ ] Com a opção de troca do mod desligada, apertar "R" com o colete cheio mantém o comportamento padrão do jogo (derruba no chão) — a correção não deve ignorar a configuração existente.
- [ ] A animação de recarga tocada é a mesma animação canônica já usada hoje (nenhuma mudança perceptível na forma como a recarga é exibida).
- [ ] **Fika/multiplayer:** o mesmo comportamento (troca no mesmo slot, colete cheio) funciona igual em partida coop conectada a um servidor dedicado (Headless) — sem travar o gatilho/mãos do jogador, sem o carregador ficar piscando e sem a ação ser desfeita/recusada. <!-- review: N/A frágil evitado — critério mantido verificável e sem presumir que a causa é a mesma do item 003-magazine-swap-inplace-fix (FIKA). O caminho de recarga por "R" pode não passar pelo mesmo mecanismo de swap usado no drag-and-drop pelo inventário; qual é o mecanismo real e se ele precisa da mesma correção é pergunta da spec técnica, não algo a presumir aqui. -->
- [ ] **Estado entre raids:** N/A — o comportamento reage apenas à ação de recarregar no momento em que ela acontece; não mantém nem depende de nenhum estado guardado entre uma raid e outra.

## Corner cases

- [ ] Arma sem nenhum carregador inserido no momento do "R" (recarga do zero, não uma troca) — deve continuar carregando normalmente, sem tentar aplicar a lógica de troca.
- [ ] Disparo às cegas ("hip-fire"/blindfire) no momento de apertar "R" — a troca não deve ser tentada nesse estado (mesma restrição que já existe hoje para a recarga comum nesse estado).
- [ ] Trocar de arma rapidamente durante a animação de recarga em andamento — não deve deixar o jogador com as mãos travadas nem com o carregador antigo "perdido".
- [ ] Bots/IA recarregando — o comportamento de troca é exclusivo do jogador humano; bots continuam com o comportamento padrão de recarga.
- [ ] Coop: dois jogadores mexendo no mesmo carregador/item ao mesmo tempo — deve continuar sendo tratado como uma colisão real, do mesmo jeito que já é para o swap pelo inventário.
- [ ] Apertar "R" uma segunda vez enquanto a animação/confirmação da primeira recarga ainda está em andamento — não deve travar gatilho/mãos, duplicar ou perder um carregador.

## Fora de escopo

- [ ] A definir

## Referências

- Correção equivalente já entregue para o caminho de arrastar pelo inventário: [mods/FIKA/backlog/003-magazine-swap-inplace-fix/](../../../FIKA/backlog/003-magazine-swap-inplace-fix/).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-06 | Item criado via `/add-backlog-item` |
| 2026-09-06 | Revisão `/review-spec` — 1 gap corrigido (critério Fika/multiplayer presumia o mesmo mecanismo técnico do item 003 do FIKA; reescrito para ficar verificável sem presumir causa) + 1 corner case adicionado (duplo-toque de "R" durante recarga em andamento) |
