# 006 — Item Physics Sem Expirar (Manter Rigidbody Adormecido em Vez de Destruir)

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-20

## Visão geral

A propriedade "Item Physics" (categoria "Physics | Item Physical Properties" no F12, desligada por padrão) faz itens largados no chão reagirem fisicamente a tiro e explosão. Na prática, o usuário reportou que atirar num capacete/arma largado por um bot morto há um tempo não produz nenhum efeito, mesmo com a propriedade ligada. Investigação nesta sessão encontrou a causa: o próprio jogo remove o suporte físico de qualquer item largado poucos segundos depois dele parar de se mexer, como otimização de desempenho independente deste mod — deixando o item "duro", sem reagir a mais nenhuma força depois disso. Este item propõe interceptar esse comportamento pra manter o item continuamente apto a reagir, sem reintroduzir um custo de desempenho crescente.

## Comportamento atual

- Com "Item Physics" ligado, um item recém-largado (arma, capacete, etc.) reage a tiro (empurrão) e a explosão de granada durante os primeiros instantes após cair no chão.
- Assim que o item "acomoda" (para de se mexer, ou passa de um tempo limite curto), o jogo remove o suporte físico dele — decisão automática, tomada pelo próprio jogo, independente do estado da propriedade "Item Physics" deste mod.
- A partir desse ponto, o item para de reagir a qualquer força (tiro ou explosão), mesmo com "Item Physics" ligado — na prática, a propriedade só tem efeito perceptível numa janela de tempo curta logo após o drop, o que faz parecer que ela "não funciona", já que o cenário mais comum é o jogador atirar num item bem depois dele já ter caído (ex.: lootando o corpo de um bot morto há minutos).
- Esse comportamento (remover o suporte físico após acomodar) é uma otimização de desempenho legítima do próprio jogo, que reduz a quantidade de itens sendo simulados fisicamente a cada momento no mapa — não é um bug isolado, é intencional do lado do jogo.

## Comportamento desejado

- Com "Item Physics" ligado, um item largado continua apto a reagir a tiro/explosão **independente de quanto tempo já se passou** desde que ele caiu no chão — não só nos primeiros instantes.
- Isso deve ser alcançado preservando o suporte físico do item (sem removê-lo), mas em um estado de repouso de baixo custo (o item não fica sendo simulado ativamente frame a frame enquanto está parado — só volta a se mover quando algo aplica força nele).
- O mecanismo de checagem contínua que o jogo usa pra decidir "esse item ainda precisa ser verificado?" deve parar de rodar depois que o item acomoda, mesmo com o suporte físico preservado — ou seja, o custo por item some depois de acomodar, só o suporte físico em si (de custo já baixo em repouso) continua.
- Com "Item Physics" desligado, nenhuma mudança de comportamento em relação ao que existe hoje.
- **Escopo:** o comportamento vale pra qualquer item já afetado por "Item Physics" hoje (qualquer item largado com física, não só armas/equipamento derrubados especificamente por este mod) — este item não restringe nem amplia quais itens a propriedade já afeta, só muda por quanto tempo eles continuam reagindo.

## Critérios de aceite

- [ ] Atirar num item largado há mais de alguns segundos (já acomodado) aplica força física visível nele, com "Item Physics" ligado — igual ao que já acontece hoje só nos primeiros instantes após o drop.
- [ ] Uma granada explodindo perto de um item acomodado há mais tempo também aplica força nele, com "Item Physics" ligado.
- [ ] Com "Item Physics" desligado, o comportamento permanece idêntico ao atual — item para de reagir a força depois de acomodar, sem nenhuma mudança perceptível.
- [ ] A checagem contínua de "esse item ainda precisa de física?" que o jogo roda a cada quadro pra um item recém-largado deixa de rodar depois que ele acomoda (não fica verificando pra sempre) — comportamento observável indiretamente por ausência de queda de desempenho perceptível numa raid com muitos itens largados e acomodados ao longo do tempo.
- [ ] **Fika/multiplayer:** todo jogador da sala vê o item reagindo a tiro/explosão do mesmo jeito, independente de quanto tempo já passou desde o drop — sem introduzir nenhuma sincronização de rede nova além do que "Item Physics" já faz hoje pra itens recém-largados. <!-- review: spec técnica precisa confirmar se a preservação do suporte físico se comporta igual em quem tem autoridade sobre a física do item (provavelmente o host) e em quem só observa (clientes) — se houver assimetria real (ex.: só o host consegue "acordar" o item), documentar isso explicitamente como limitação conhecida, não deixar como suposição -->
- [ ] **Estado entre raids:** N/A — cada raid recria o mundo do zero; nenhum item ou estado "acomodado" de uma raid sobrevive pra próxima.

## Corner cases

- [ ] Item que nunca "acomoda" de verdade (ex.: cai num vão/buraco do mapa e fica rolando/oscilando sem nunca ficar totalmente parado) — precisa continuar se comportando de forma razoável, sem ficar preso num estado estranho.
- [ ] Jogador pega o item do chão (loot) enquanto ele ainda está no estado "acomodado, física preservada" — a remoção/transferência do item precisa continuar funcionando normalmente, sem deixar nenhum resquício.
- [ ] Fim da raid com muitos itens simultaneamente no estado "acomodado, física preservada" (ex.: depois de um combate grande, com vários mortos e itens largados) — não pode causar acúmulo perceptível de memória/objetos ao longo de uma raid longa.
- [ ] Toggle mestre do mod (F12) ou a própria propriedade "Item Physics" sendo desligada no meio da raid, depois de itens já terem passado por esse tratamento — os itens já afetados devem continuar se comportando de forma razoável (não é esperado que a mudança de toggle "desfaça" o que já aconteceu com itens já largados, só que novos itens não passem mais por esse tratamento a partir da mudança).
- [ ] Mesmo item sendo atingido várias vezes ao longo de um período longo (ex.: um tiro logo após o drop, outro minutos depois, outro ainda mais tarde) — cada impacto precisa continuar aplicando força corretamente, não só o primeiro.
- [ ] Interação com a correção já aplicada nesta mesma sessão pra granada não empurrar item já reatribuído de camada física (`PhysicalItemsPatch`/`GrenadeItemsPatch`) — confirmar que as duas correções continuam funcionando juntas sem reintroduzir a mesma inconsistência.

## Fora de escopo

- [ ] A definir

## Referências

- Achado e discutido na mesma sessão que corrigiu um bug relacionado na mesma feature "Item Physics": granada não aplicava força em item já reatribuído pra uma camada física diferente (`mods/VisceralCombat/memory/sessions.md`, entrada de 2026-09-20).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Item criado via `/add-backlog-item` |
| 2026-09-20 | Revisão `/review-spec` — 1 gap corrigido (escopo do "Comportamento desejado" não especificado) + critério Fika/multiplayer reescrito pra ser verificável (com marcação `<!-- review: -->` pra confirmação técnica de autoridade host/cliente) + 2 corner cases adicionados (impactos repetidos ao longo do tempo; interação com o fix de camada já aplicado na mesma sessão) |
