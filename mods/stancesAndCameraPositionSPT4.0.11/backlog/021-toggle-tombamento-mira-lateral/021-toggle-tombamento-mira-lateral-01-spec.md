# 021 — Toggle para tombamento de mira lateral

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-09-08

## Visão geral

O mod sobrescreve, todo frame, a rotação final da arma que o jogo vanilla calcula para o jogador local — inclusive quando a arma usa uma mira montada em trilho lateral (offset/canted), cujo tombamento normalmente aparece ao mirar. Hoje essa sobrescrita é incondicional: não existe nenhuma opção no F12 para preservar o tombamento nativo dessas miras. Este item adiciona um checkbox no F12, **desativado por padrão**, que restaura o comportamento nativo quando desligado.

## Comportamento atual

Ao mirar (ADS) com qualquer arma, o mod recalcula a rotação final aplicada à arma a partir do sistema de posturas próprio do mod, e essa rotação **substitui** a que o jogo calcularia sozinho. Para a maioria das miras isso não é perceptível (o tombamento nativo é ~zero). Mas para miras montadas em trilho lateral — cujo mecanismo nativo do jogo tomba a arma ao mirar, para trazer a mira até a linha de visão — o resultado é que a arma **fica sempre reta**, mesmo mirando com essa mira, mesmo fora de qualquer postura customizada do mod (ou seja, mesmo com o jogador na postura padrão/vanilla). Não há nenhuma opção no F12 para desativar esse comportamento hoje.

## Comportamento desejado

Adicionar uma opção no F12 (checkbox), com **valor padrão DESATIVADO**:

- **Desativada (padrão):** o mod preserva o tombamento nativo do jogo para miras de trilho lateral. Fora de qualquer postura customizada, a arma tomba ao mirar com essas miras como aconteceria sem o mod instalado; dentro de uma postura customizada, o tombamento nativo aparece **combinado** com o ângulo da postura (nenhum dos dois anula o outro).
- **Ativada:** mantém o comportamento atual do mod — a arma permanece sempre alinhada/reta ao mirar, ignorando o tombamento nativo da mira, para quem preferir esse visual.

<!-- review: em qualquer valor da opção, esta feature NÃO pode reintroduzir o problema que motivou o código atual a abandonar o tombamento nativo — o comentário existente no patch registra que "algumas miras possuem valores extremos [de rotação nativa] que viram a câmera". Ver critério de aceite e corner case dedicados abaixo; decisão de qual proteção/limite é aceitável fica para a spec técnica, mas o comportamento observável (câmera nunca vira/trava) é requisito funcional. -->

## Critérios de aceite

- [ ] Com a opção desativada (padrão) e uma arma equipada com mira de trilho lateral, ao entrar em ADS a arma tomba visualmente como aconteceria no jogo sem o mod.
- [ ] Com a opção desativada, usar uma postura customizada do mod (Stance 1/2/3) junto com uma mira de trilho lateral mantém o tombamento da mira visível, combinado com a postura — não anulado por ela.
- [ ] Com a opção ativada, o comportamento é idêntico ao atual: a arma permanece reta ao usar mira de trilho lateral, em qualquer postura.
- [ ] A opção aparece no F12 como checkbox, segue o padrão de tooltip bilíngue do mod (inglês em cima, português embaixo) e tem efeito imediato ao ser alterada (sem precisar reabrir raid).
- [ ] Trocar de arma (para uma sem mira de trilho lateral, ou sem mira nenhuma, ou vice-versa) não deixa nenhum estado "preso" — o resultado reflete a arma/mira equipada a cada frame, com a opção em qualquer um dos dois valores.
- [ ] **Com a opção desativada, nenhuma combinação de arma + mira faz a câmera virar de cabeça para baixo, travar, ou "piscar" para uma posição absurda** — mesmo risco que hoje é citado como o motivo de o mod ter abandonado o tombamento nativo (mira com valor de rotação fora do normal). O mod já tem proteção equivalente contra rotação inválida por outras causas hoje (ex.: entrada NaN); essa proteção não pode regredir com a opção desativada.
- [ ] **Fika/multiplayer:** N/A a sincronização de rede — esta é uma preferência client-side que afeta apenas a pose renderizada do "seu" jogador local (mesmo escopo do restante do sistema de postura, que já só age sobre o jogador local). Não deve alterar nada visível para outros jogadores no raid.
- [ ] **Estado entre raids:** a opção é uma config persistente do BepInEx, igual às demais do F12 — o valor escolhido permanece entre raids e reinícios do jogo; nenhum estado de uma raid anterior deve influenciar o comportamento na raid seguinte.

## Corner cases

- [ ] Mira sem tombamento relevante (a maioria das miras comuns, abaixo do limiar nativo de rotação): com a opção desativada, nenhuma mudança visível em relação ao comportamento de hoje.
- [ ] Alternar a opção no F12 no meio de uma transição de ADS em andamento (mola de interpolação ainda se movendo): não deve causar um salto abrupto/pop na rotação da arma.
- [ ] Arma sem controlador de fogo válido no momento (trocou para granada, faca, item médico etc.) enquanto a opção está em qualquer um dos dois valores: nenhuma exceção; mesmo early-return já existente hoje.
- [ ] Jogador observado (outro player no raid, não o local): a opção não deve afetar a pose renderizada de jogadores que não são o jogador local.
- [ ] Interação com a transição suave de ADS já existente (item 017 — waypoint/atenuação ao entrar em mira): o tombamento nativo da mira, quando preservado, deve seguir a mesma transição suave, sem popping perceptível.
- [ ] Mira com rotação nativa extrema/mal configurada (ver `<!-- review -->` acima): com a opção desativada, o resultado deve ser no máximo uma pose visualmente estranha (arma num ângulo incomum) — nunca uma câmera invertida ou travada.
- [ ] Troca rápida de arma (ex.: scroll do mouse) no meio do ADS, de uma arma com mira de trilho lateral para outra sem: nenhum resíduo perceptível do tombamento da arma anterior deve "vazar" para a nova arma por mais que um frame de transição.
- [ ] Fora de raid (hideout / estande de tiro): a opção deve se comportar da mesma forma que em raid — o item não é exclusivo de contexto de raid, assim como o restante do sistema de postura do mod.

## Fora de escopo

- [ ] A definir

## Referências

- Item relacionado: [017-transicao-ads-cirurgica/](../017-transicao-ads-cirurgica/) — mesmo sistema de interpolação de rotação da arma ao entrar em ADS.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Item criado via `/add-backlog-item`, a partir de investigação de chat sobre mira lateral/canted ficando reta com o mod ativo |
| 2026-09-08 | Revisão `/review-spec` — 2 gaps + 3 corner cases corrigidos |
