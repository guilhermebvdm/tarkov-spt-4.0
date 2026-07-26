# 019 — Feedback de dor: falas de dor ao ferir membro

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-26

## Visão geral

Achado **S3.2** do 1º teste in-game: *"Adicionar som de dor, o 'My arm is fucked', o trauma antigo tinha várias implementações de dor"* + *"Fazer uma revisão completa de falas e possíveis danos quando estiver com membros zerados/quebrados"*.

A investigação achou duas coisas que mudam o item por completo:

**1. As falas do mod antigo, em boa parte, nunca funcionaram.** Ele resolvia o gatilho de fala por nome de texto, dentro de um bloco que engolia qualquer erro em silêncio. Três dos quatro nomes que ele usava **não existem** no jogo — o de perna quebrada, o de mão quebrada e o de dor de estômago. Resultado: metade das falas de perna e de braço era silêncio absoluto (a outra metade caía num grito genérico, por sorteio), e a de estômago **nunca tocou uma única vez**. O "my arm is fucked" que se lembra do mod antigo era o grito genérico, não a fala dedicada.

**2. O jogo já tem o mapa pronto, e é melhor que o nosso.** As falas dedicadas de perna e mão quebrada existem, têm áudio, e o próprio jogo as usa **exatamente neste evento**: quando um bot ganha uma fratura, ele fala a de perna ou a de mão conforme o membro. As duas também aparecem no menu de voz do jogador, no grupo de estado de saúde — o que garante que há gravação de voz de jogador para elas.

**A parte de "possíveis danos" foi resolvida sem código.** O jogo já pune usar a perna comprometida: tira vida continuamente enquanto se corre e mais um tanto a cada pouso de pulo, e já grita de dor no pouso. O que ele **não** pune é o ato de levantar — que era justamente o que o mod antigo punia. Decisão do usuário em 2026-07-26: **não duplicar**; o jogo já cobra, e o bloqueio de levantar do ciclo de queda já é a punição por estar com as duas pernas destruídas. A decisão 21 fica preservada.

## Comportamento atual

- Só duas falas, em quatro momentos, todos ligados a perna: grito forte na queda forçada e na tentativa negada de levantar; grito leve na liberação; e grito forte no bloqueio de re-mira.
- Ferir um membro **não produz som nenhum**. Zerar um braço, quebrar uma perna, zerar o estômago: silêncio.
- O anti-spam tem **dois canais** (forte e leve) por jogador. O mod antigo era pior: canal único, então um grito de perna calava o de braço pelos 2 segundos seguintes.
- Sobra no código o sistema de voz morto do mod antigo, sem ninguém chamando, carregando os três nomes inválidos.
- A mordaça de voz do desmaio tem uma exceção órfã que deixa passar duas falas — resquício de quando o mod antigo usava falas como canal de rede, antes de existir pacote próprio.

## Comportamento desejado

- **Ferir um membro produz a fala certa**, no momento em que o ferimento passa a valer:
  - membro **fraturado** → a fala dedicada do jogo para aquele membro (a mesma que o bot usa);
  - membro **zerado** sem fratura → grito de agonia (o jogo não tem fala dedicada para "membro destruído");
  - **estômago** zerado → grito de agonia (idem).
- **Perder a mira por fraqueza do braço** produz ofego de esforço, não agonia — são coisas diferentes e devem soar diferentes. É a fala que o mod antigo tocava nesse momento, e a única dos quatro nomes dele que existia de verdade.
- **Analgésico cala a dor** e **bots são sempre mudos**. Nenhuma das duas é invenção nossa: é literalmente a condição que o jogo usa para gritar no pouso com a perna ferida.
- **Entrar na raid já ferido não fala** — é reconhecimento de um estado que já existia, não um ferimento novo. Mesma regra que já vale para o aviso de tela e para o agachar involuntário.
- **Curar não fala.** Só agravamento produz som.
- Um canal de anti-spam **por tipo de dor**, para que uma perna quebrada e um braço quebrado no mesmo instante produzam as duas falas em vez de uma calar a outra.
- Um interruptor no F12 desliga só as falas, sem afetar nenhum efeito de jogo.

## Critérios de aceite

- [x] Quebrar uma perna produz a fala dedicada de perna quebrada; quebrar um braço, a de mão quebrada.
- [x] Zerar um membro sem fratura, e zerar o estômago, produzem grito de agonia.
- [x] Nenhuma fala é resolvida por nome de texto — se um gatilho deixar de existir num update do jogo, isso vira **erro de compilação**, não silêncio.
- [x] Com analgésico ativo, ferir membro não produz fala.
- [x] Bot ferido não fala em nenhuma circunstância.
- [x] Entrar na raid já ferido não produz fala.
- [x] Curar não produz fala.
- [x] Perder a mira por braço ferido produz ofego, audivelmente distinto do grito de agonia do bloqueio de re-mira que vem em seguida.
- [x] Dois ferimentos de regiões diferentes no mesmo instante produzem as duas falas.
- [x] O interruptor do F12 desligado silencia as falas novas sem alterar nenhum efeito de jogo.
- [x] O sistema de voz morto do mod antigo e a exceção órfã da mordaça de desmaio saem do código.
- [x] **Fika/multiplayer:** o aliado ouve a fala. Nenhum pacote novo — o jogo já sincroniza fala de jogador, e o mod fala pelo dono do personagem, então a propagação é a nativa.
- [x] **Estado entre raids:** as janelas de anti-spam são limpas na fronteira de raid, junto do resto (item 020 audita isso).

## Corner cases

- [x] **Membro zerado E fraturado ao mesmo tempo** → prevalece a fala de fratura, que é a mais específica.
- [x] **Rajada de espingarda destruindo um membro** → uma fala só. O mod fala na mudança de estado, não no impacto, e o motor consolida vários impactos numa transição por quadro. O mod antigo falava no impacto e dependia do anti-spam para não metralhar.
- [x] **Ferimento durante o desmaio** → a mordaça de voz do desmaio continua valendo; inconsciente não fala.
- [x] **Analgésico expirando com o membro ainda ferido** → a reavaliação reconhece o agravamento e a fala sai então. É coerente: a dor volta quando o efeito passa.
- [x] **Piorar dentro da mesma região** (zerar a segunda perna) → fala de novo, porque a severidade aumentou; respeitando o anti-spam do tipo.
- [x] **Peer observando** → ouve pelo caminho nativo do jogo. O mod não toca em fala de personagem que não é dele.

## Fora de escopo

- [x] **Qualquer dano ou fratura aplicado pelo mod** — decisão do usuário: o jogo já pune usar a perna ferida, e a punição por levantar do mod antigo não tem equivalente no jogo. Decisão 21 preservada.
- [x] **Falas de gravidade por vida total** (o jogo tem quatro, por faixa de vida restante). Ficam de fora porque são falas do menu de voz, para o jogador acionar quando quiser, e automatizá-las viraria tagarelice. A gravidade já entra de graça: o jogo escolhe a variação do clipe pelo estado de saúde de quem fala.
- [x] **Falas de desmaio e de acordar** — o item 015 vai reescrever esse trecho por inteiro; mexer aqui geraria conflito.
- [x] **Áudio próprio** (arquivo de som do mod) — só entra se as falas do jogo se mostrarem indistinguíveis em teste, e tem um custo conhecido: o aliado deixa de ouvir.
- [x] **Falas para bots** — decisão do usuário.

## Referências

- [Patches/Trauma/TraumaPainVoice.cs](../../modded/Patches/Trauma/TraumaPainVoice.cs) (novo — mapa evento→fala)
- [Patches/Trauma/TraumaVoice.cs](../../modded/Patches/Trauma/TraumaVoice.cs) (gatilhos tipados + anti-spam por tipo)
- [Helpers/VoiceAndHealthUtils.cs](../../modded/Helpers/VoiceAndHealthUtils.cs) (código morto removido + mordaça sem a exceção órfã)
- [docs/happy-flow-test-plan.md](../../docs/happy-flow-test-plan.md) (cenário **H11**)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado a partir do achado S3.2. Descoberto durante a investigação que três dos quatro gatilhos de fala do mod antigo não existem no jogo e falhavam em silêncio — o mod novo usa gatilhos tipados justamente para o bug não poder voltar. A parte de "possíveis danos" foi fechada sem código: o jogo já pune usar a perna ferida (dreno ao correr, dano ao pousar, e já grita no pouso), e não pune levantar; decisão do usuário foi não duplicar. |
