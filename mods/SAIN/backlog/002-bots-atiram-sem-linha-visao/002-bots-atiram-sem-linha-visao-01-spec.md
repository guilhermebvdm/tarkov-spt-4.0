# 002 — Bots atiram sem linha de visão e ficam travados como inimigos indefinidamente

**Mod:** SAIN
**Status:** Backlog
**Criado:** 2026-09-24

## Visão geral

Bots do mesmo esquadrão avisam uns aos outros a posição de um inimigo que qualquer um deles detectou (visto ou ouvido), e esse aviso é aceito por quem recebe sem nenhuma validação própria — nem de que o alcance de comunicação é plausível, nem de que quem recebe já teve, alguma vez, contato visual real com aquele inimigo. Combinado com o fato de que não existe nenhuma checagem geométrica real (parede, prédio, terreno) antes de um bot abrir fogo numa posição, e de que um bot que leva um tiro perto já se sente liberado a atirar de volta na direção sem nunca ter visto quem atirou, o resultado observado em raid real foi dois bots trocando tiro sustentado e preciso, mirados exatamente um no outro, através de três prédios inteiros, carros e um muro — nunca havendo, em momento algum, uma linha de visão real possível entre os dois. Além disso, uma vez que dois bots se consideram mutuamente inimigos, eles permanecem travados nesse estado por muito tempo, sem se desengajar, mesmo sem nunca restabelecer contato visual.

## Comportamento atual

- Um bot que detecta um inimigo (por visão ou por som) informa a posição pro resto do esquadrão dele.
- Quem recebe esse aviso aceita a posição como válida sem checar se está dentro de um alcance de comunicação plausível, e sem nunca ter validado visualmente aquele inimigo por conta própria.
- Um bot pode abrir fogo numa posição reportada sem que exista nenhuma checagem de que o caminho até lá está livre de obstáculos sólidos (paredes, prédios, terreno) — a única checagem existente hoje é evitar acertar outro jogador/bot que esteja no meio do caminho (fogo amigo), e mesmo essa checagem está, na prática, desativada.
- Um bot que sofre um tiro por perto passa a se considerar liberado pra atirar de volta na direção de onde veio o tiro, mesmo sem nunca ter visto quem disparou.
- Uma vez que dois bots se consideram inimigos um do outro, esse estado permanece por um tempo muito longo, sem se desfazer, mesmo que nenhum dos dois restabeleça contato visual depois do engajamento inicial.

## Comportamento desejado

- Um bot que recebe do esquadrão a posição de um possível inimigo **não abre fogo direto e preciso nessa posição** — ele se desloca/investiga o local reportado, e só passa a atirar mirado de verdade quando estabelecer contato visual confirmado por conta própria.
- **Exceção — fogo de supressão pra apoiar o avanço do esquadrão:** é aceitável um bot dar tiro de supressão (fogo de cobertura impreciso, não mirado) na direção de uma posição reportada por um tempo limitado, especificamente pra ajudar o esquadrão a se movimentar/avançar. Esse apoio de fogo só continua enquanto o membro do esquadrão que detectou o inimigo **continuar atualizando/reportando a posição dele**. Se o membro que reportou parar de atualizar a informação (perdeu ele de vista, morreu, ficou em silêncio), o bot que está dando supressão **não tem mais informação nova pra justificar continuar** — o apoio de fogo cessa, em vez de continuar baseado numa posição antiga e potencialmente desatualizada.
- Nenhum bot consegue abrir fogo mirado numa posição/alvo se existir um obstáculo sólido (parede, prédio, terreno) bloqueando o caminho — a checagem de obstrução volta a ser aplicada de verdade antes de qualquer tiro mirado (a supressão em área, por natureza, pode continuar sendo direcionada a uma área aproximada, não a um ponto preciso através de um obstáculo).
- Um bot só passa a considerar outro como inimigo ativo pra fins de mira precisa (e libera fogo de retorno mirado) depois de ter, ele mesmo, confirmado contato visual — sofrer um tiro por perto, sozinho, não é suficiente pra abrir fogo mirado de volta numa direção não confirmada.
- Bots que perdem contato visual por um tempo razoável, **e cujo esquadrão para de fornecer atualização de posição**, se desengajam do estado de "inimigo travado" e voltam a se comportar de acordo com a situação atual (buscar, investigar, patrulhar), em vez de permanecerem mirando indefinidamente numa posição antiga.

## Critérios de aceite

- [ ] Dois bots posicionados sem nenhuma linha de visão real entre si (bloqueados por construções) não trocam tiro **mirado e preciso**, mesmo que um tenha ouvido ou seu esquadrão tenha reportado a posição do outro.
- [ ] Um bot que recebe a posição de um inimigo reportada pelo esquadrão se movimenta em direção ao local reportado (investiga) em vez de abrir fogo mirado imediatamente numa posição sem visão confirmada.
- [ ] Um bot só abre fogo mirado depois de ter contato visual próprio e confirmado com o alvo — nenhum tiro mirado é disparado baseado só em informação de terceiros ou em ter sido atingido sem ver o atirador.
- [ ] Um bot pode dar fogo de supressão (impreciso, em área) na direção de uma posição reportada pelo esquadrão por tempo limitado, **mas esse apoio para assim que o membro que reportou parar de atualizar a posição** — não continua indefinidamente numa informação antiga.
- [ ] Um bot não consegue acertar/mirar com precisão, de forma sustentada, num alvo atrás de um obstáculo sólido intransponível (parede espessa, prédio inteiro) — a checagem de obstrução funciona de fato pra tiro mirado, não apenas evita fogo amigo.
- [ ] Dois bots que se engajaram mas perdem contato visual por tempo suficiente, sem atualização de posição do esquadrão, se desengajam do estado de inimigo travado (não ficam mirados um no outro indefinidamente sem visão nem informação nova).
- [ ] **Fika/multiplayer:** comportamento idêntico em coop — bots não devem atirar através de obstáculos nem em jogadores humanos, nem em outros bots, independente de quantos jogadores humanos estão na raid ou de quão longe estão do combate.
- [ ] **Estado entre raids:** N/A — o comportamento de engajamento/desengajamento é decidido a cada raid, a partir de zero, sem nenhum estado herdado de raids anteriores.

## Corner cases

- [ ] Bot que recebe um aviso do esquadrão sobre uma posição, se desloca até lá, mas o inimigo real já não está mais nesse lugar (se moveu) — o bot não deve continuar atirando na posição antiga; deve reavaliar quando chegar lá sem encontrar ninguém.
- [ ] Combate legítimo, com linha de visão real e parcialmente obstruída por cobertura rasa (ex.: uma moita, uma grade fina) — o comportamento correto de "atirar através de cobertura fraca" que já existe hoje (se houver) não deve ser destruído junto com a correção; só obstáculos verdadeiramente sólidos devem bloquear.
- [ ] Múltiplos bots do mesmo esquadrão recebendo o mesmo aviso simultaneamente — todos devem investigar/validar por conta própria, não bastar um confirmar visualmente pra todos os outros também abrirem fogo sem ver.
- [ ] Jogador humano (não bot) sendo alvo do mesmo mecanismo — um bot não deve atirar num jogador humano atrás de um prédio só porque outro bot do esquadrão reportou a posição aproximada dele.
- [ ] Interação com o mecanismo de supressão em área (fogo de cobertura aproximado), que é uma feature legítima do mod — a correção não pode eliminar supressão tática válida quando há contexto real de combate, só o caso de abrir fogo direcionado e preciso sem nenhuma visão jamais estabelecida.
- [ ] Membro do esquadrão que reportou a posição do inimigo morre, fica inconsciente ou perde o inimigo de vista logo depois de reportar — os outros bots que estavam recebendo aquela atualização pra dar supressão precisam parar de considerá-la válida (não travar supressão eterna numa fonte de informação que já não existe mais).
- [ ] Múltiplos membros do esquadrão reportando a MESMA posição em sequência rápida (ex.: todos viram o mesmo flash) — não deve ser tratado como "informação renovada" indefinidamente além do tempo razoável de supressão, só porque várias fontes concordaram uma vez.

## Fora de escopo

- [ ] A definir

## Referências

- Investigação técnica realizada nesta sessão (não formalizada em documento próprio ainda) — mecanismos identificados: compartilhamento de posição de inimigo entre membros do esquadrão sem validação de alcance/visão; checagem de tiro limpo com o bloqueio por falta de linha de visão desativado no código e, mesmo ativo, sem checagem real de obstrução por terreno/construções (só evita fogo amigo); liberação de supressão de retorno baseada em "sofreu tiro perto" sem exigir visão do atirador.
- Confirmado como comportamento pré-existente do SAIN (presente igualmente em `original/`, `modded/` e `modded-multithread/`) — não foi introduzido pela otimização multithread/LOD, só ficou mais exposto/perceptível por ela.
- Reproduzido em raid real: dois bots trocando tiro sustentado e preciso através de 3 prédios, carros e um muro, nunca havendo linha de visão real entre eles.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-24 | Item criado via `/add-backlog-item`, a partir de investigação de bug de IA em raid real (bots atirando através de múltiplas construções sem nunca ter linha de visão, e permanecendo travados como inimigos por muito tempo sem se desengajar) |
| 2026-09-24 | Refinamento do usuário: fogo de supressão (impreciso, em área) continua permitido pra apoiar avanço do esquadrão, mas só enquanto o membro que reportou o inimigo continuar atualizando a posição — sem atualização nova, o apoio de fogo cessa. Distinção entre "tiro mirado" (exige visão própria) e "supressão em área" (tolera informação de terceiros, mas com validade temporal) aplicada em Comportamento desejado, critérios e corner cases |
