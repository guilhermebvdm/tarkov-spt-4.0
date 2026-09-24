# 023 — Atraso na checagem de carregador (Magazine Check Delay)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-09-22

## Visão geral

Atrasa a exibição do painel de munição na tela (o HUD que aparece ao checar o carregador) por um
tempo configurável, em vez de mostrar o resultado instantaneamente assim que a checagem é acionada —
sincronizando a revelação da informação com a animação de inspeção do personagem. **Esta spec é
retroativa:** a feature já foi implementada e está em produção (build 2.25.x) sem ter passado pelo
fluxo formal de backlog (sem spec funcional, spec técnica ou asbuild prévios). O objetivo deste item é
documentar o comportamento real já implementado, revisar contra o Assembly, e achar gaps antes de
considerar o item fechado.

## Comportamento atual

Antes desta feature (vanilla / mod sem ela): ao checar o carregador (e, via item 019 deste mesmo mod,
também ao checar a câmara), o painel de munição na tela aparece **instantaneamente**, no exato
momento em que a checagem é acionada — antes mesmo de o personagem terminar (ou começar de verdade) a
animação de olhar para a arma.

## Comportamento desejado

Ao checar o carregador, o painel de munição só aparece na tela depois de um tempo configurável (padrão
observado: 2 segundos), dando a impressão de que a informação só fica disponível depois que o
personagem efetivamente girou a arma e "olhou" pro carregador — não no instante em que o jogador
apertou o botão. O atraso pode ser desligado (volta ao comportamento instantâneo) ou ter sua duração
ajustada, ambos via F12.

## Critérios de aceite

- [ ] Checar o carregador com o atraso ativado NÃO mostra o painel de munição no frame em que a
      checagem começa — o painel só aparece depois do tempo configurado.
- [ ] Com o atraso desativado no F12, checar o carregador volta a mostrar o painel instantaneamente
      (comportamento idêntico ao de antes desta feature existir).
- [ ] A duração do atraso é ajustável no F12 dentro de uma faixa sensata (a implementação observada
      usa 0.5–4.0 segundos) e o valor configurado é o que efetivamente decorre antes do painel aparecer.
- [ ] Trocar de arma (ou a arma em mãos deixar de ser a que estava sendo checada) enquanto o atraso
      ainda está contando **não** deixa um painel desatualizado aparecer depois — a checagem cancelada
      não produz HUD.
- [ ] **Verificar (gap suspeito, não assumir):** confirmar se este atraso também se aplica à checagem
      de CÂMARA (item 019 deste mod, `Show Chamber Ammo On Check`) — o nome da config nova sugere que é
      só para carregador, mas o ponto de patch pode ser compartilhado entre as duas checagens. Se for
      compartilhado, decidir se isso é intencional ou se precisa de uma config separada / mais clara.
- [ ] **Fika/multiplayer:** checar o carregador não produz nenhum efeito visível pros outros jogadores
      no raid (nem HUD, nem atraso, nem qualquer sincronização) — é puramente uma experiência local do
      jogador que checou. Você também não vê o painel atrasado de outro jogador quando ele checa o
      carregador dele. <!-- review: comportamento local-only é o esperado pra um painel de HUD, mas a
      spec técnica precisa CONFIRMAR com evidência do Assembly (não presumir) que
      EftBattleUIScreen.ShowAmmoDetails não tem nenhum componente sincronizado/observado antes de fechar
      este critério como validado. -->
- [ ] **Estado entre raids:** o atraso pendente de uma checagem (Coroutine ativa) não pode sobreviver ao
      fim da raid nem vazar para a raid seguinte — nenhum painel atrasado de uma raid anterior aparece
      numa raid nova. O valor configurado no F12 (ligado/desligado, duração) persiste normalmente entre
      raids e reinícios do jogo, como as demais props do mod.

## Corner cases

- [ ] Checar o carregador, morrer (ou a raid terminar) antes do atraso decorrer — a Coroutine pendente
      não deve tentar mostrar um painel num player/HUD que não existe mais, nem lançar exceção.
- [ ] Checar o carregador duas vezes em sequência rápida (antes do primeiro atraso terminar) — o
      segundo check deve substituir o primeiro de forma limpa (sem dois paineis concorrentes, sem a
      Coroutine antiga "vencer" com dado desatualizado depois da nova).
- [ ] Checar o carregador, trocar de arma durante a contagem do atraso, e a checagem original não deve
      aplicar-se à arma nova quando o atraso terminar.
- [ ] Alternar entre primeira e terceira pessoa (ou o jogador observado por outro Fika peer) durante o
      atraso — o painel (que é HUD, presumivelmente 1ª pessoa local) não deve tentar aparecer fora de
      contexto.
- [ ] Config `Magazine Check Delay Seconds` no valor mínimo (0.5s) ou máximo (4.0s) da faixa — sem
      comportamento degenerado (atraso de 0 efetivo, ou travamento).
- [ ] Mudar o valor de `Magazine Check Delay Seconds` no F12 **enquanto um atraso já está em contagem**
      — definir se o atraso em andamento deve terminar com a duração antiga (a que valia quando começou)
      ou adotar a nova imediatamente. Comportamento esperado: termina com a duração que valia no início
      da checagem (não recalcula no meio) — evita um atraso que "pula" para trás ou para frente.
- [ ] Checar o carregador (ou a câmara, se o gap acima confirmar que é compartilhado) com
      `Show Chamber Ammo On Check` desligado — o atraso não deve produzir um painel vazio/fantasma numa
      checagem que originalmente não mostraria nada.

## Fora de escopo

- [ ] A definir

## Referências

- Item relacionado: [019-checar-camara-ui/](../019-checar-camara-ui/) — introduziu o painel de munição
  também para checagem de câmara (`Show Chamber Ammo On Check`), reusando o mesmo `ShowAmmoDetails`
  que este item agora atrasa. A relação entre os dois itens precisa ser esclarecida na spec técnica.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-22 | Item criado via `/add-backlog-item`, retroativo — feature já implementada em build 2.25.x fora do fluxo formal (achada ao usuário perguntar "o que mudou de 2.24 pra 2.25"). Spec funcional documenta o comportamento observado no código (`MagCheckDelayPatch.cs`) para revisão formal. |
| 2026-09-22 | Revisão `/review-spec` — 2 gaps + 3 corner cases corrigidos |
