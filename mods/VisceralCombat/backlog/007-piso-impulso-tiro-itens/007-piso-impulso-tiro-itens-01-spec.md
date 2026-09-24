# 007 — Impulso de Tiro Menos Dependente do Peso do Item

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-21

## Visão geral

A propriedade "Item Physics" aplica força física em itens largados quando atingidos por tiro. Hoje essa força é calculada como um momento físico realista (massa do projétil × velocidade), e o quanto o item efetivamente se move depende do peso cadastrado dele no jogo — quanto mais pesado o item, menos ele reage ao mesmo tiro. Na prática isso faz itens leves (máscara, óculos) reagirem de forma bem visível, enquanto itens pesados (capacete, arma) praticamente não se mexem com o mesmo impacto, dando a impressão de que a propriedade "não funciona" para eles.

## Comportamento atual

- Ao atingir um item largado com "Item Physics" ligado, o jogo calcula um impulso baseado no peso e na velocidade do projétil (independente do item alvo) e aplica esse impulso sobre o item.
- A reação visual do item depende diretamente do peso real cadastrado dele: itens leves recebem uma "velocidade ganha" grande com o mesmo impulso; itens pesados recebem uma velocidade ganha pequena, quase imperceptível.
- Resultado observado: atirar numa máscara produz um empurrão nítido; atirar num capacete ou numa arma, com o mesmo tiro, praticamente não produz reação visível — mesmo com "Item Physics" ligado e funcionando corretamente por baixo dos panos.
- Esse desbalanceamento sempre existiu na fórmula, mas só ficou perceptível depois que outra correção (item `006`) passou a manter os itens capazes de reagir a força por mais tempo — antes, itens pesados perdiam a capacidade de reagir tão rápido que a diferença nunca era notada.
- Uma explosão de granada, que usa uma escala de força totalmente separada e muito maior, já consegue mover qualquer item (leve ou pesado) de forma visível — esse caminho não apresenta o problema e não faz parte deste item.

## Comportamento desejado

- Atirar em qualquer item largado com "Item Physics" ligado produz uma reação física visivelmente perceptível, **independente do peso cadastrado do item** — itens pesados (capacete, arma) deixam de parecer "imunes" a tiro.
- Itens leves continuam reagindo de forma perceptível (sem regressão) — o ajuste não deve fazer com que eles passem a reagir de menos.
- O ajuste não deve fazer itens leves reagirem de forma exageradamente mais forte do que hoje (evitar "itens virando foguete" por overcorreção).
- A intensidade geral da reação continua ajustável pelo jogador via configuração do F12 (reaproveitando "Item Force Intensity" já existente, ou um parâmetro novo, a definir na spec técnica).
- O comportamento de granada em itens (força numa escala totalmente separada da usada por tiro) não muda — já funciona bem hoje e está fora do escopo.
- Com "Item Physics" desligado, nenhuma mudança de comportamento em relação ao que existe hoje.

## Critérios de aceite

- [ ] Atirar num item pesado (arma ou capacete) largado, com "Item Physics" ligado, produz um deslocamento/reação visualmente perceptível, de ordem de grandeza comparável à reação que um item leve já produz hoje com o mesmo tipo de munição — não apenas um tremor imperceptível.
- [ ] Atirar num item leve (máscara, óculos, fone) continua produzindo reação perceptível, sem regressão nem exagero perceptível em relação ao comportamento atual (não deve sair "voando" mais longe do que já sai hoje).
- [ ] Com "Item Physics" desligado, o comportamento permanece idêntico ao atual — nenhuma mudança perceptível.
- [ ] A intensidade da reação de tiro continua configurável pelo F12, sem exigir que o jogador recalibre a granada (comportamento de granada em itens permanece inalterado).
- [ ] **Fika/multiplayer:** todo jogador da sala vê o item reagindo à mesma magnitude de força ao ser atingido, independente de quem disparou — mesmo comportamento de sincronização que já existe hoje (cada peer aplica a força localmente sobre a própria cópia observada do item); este item não introduz nenhuma sincronização de rede nova.
- [ ] **Estado entre raids:** N/A — a força aplicada por um tiro é instantânea e não gera nenhum estado que precise sobreviver entre raids.

## Corner cases

- [ ] Item extremamente pesado (ex.: mochila largada, se aplicável ao mesmo mecanismo) — precisa continuar reagindo de forma perceptível, sem gerar uma força tão grande que produza um comportamento físico absurdo (item atravessando o chão, girando descontroladamente etc.).
- [ ] Item extremamente leve — o ajuste não pode fazer esse tipo de item sair "voando" de forma exagerada a partir de um único tiro comum.
- [ ] Item atingido por múltiplos tiros seguidos, em sequência rápida — cada tiro deve continuar aplicando reação de forma consistente, sem acúmulo ou comportamento errático.
- [ ] Interação com o item `006` (Rigidbody preservado/"dormindo" em itens acomodados há muito tempo) — o ajuste de fórmula precisa funcionar igual tanto em itens recém-largados quanto em itens que já estão no estado "dormindo, física preservada" havia mais tempo.
- [ ] Interação com a força de granada já existente — confirmar que o ajuste no impulso de bala não altera, mesmo que indiretamente, o comportamento já validado da granada sobre os mesmos itens.
- [ ] <!-- review: gap adicionado — o cálculo de impulso de tiro hoje é uma base compartilhada entre o ramo de item largado e o ramo de corpo/ragdoll (mesma origem de cálculo, ramos diferentes de aplicação, cada um com sua própria configuração de intensidade). Qualquer mudança na base desse cálculo precisa preservar, sem regressão, o comportamento já calibrado do ramo de corpo (força de tiro em ragdoll) — a spec técnica deve confirmar que o ajuste fica isolado no ramo de item, sem afetar o ramo de corpo. --> Mudança na fórmula de impulso de item não pode alterar, mesmo que indiretamente, a força de tiro já calibrada em corpos/ragdoll (cálculo de origem compartilhado entre os dois ramos).

## Fora de escopo

- [ ] Alterar o comportamento de força de granada em itens — já funciona bem, fora do escopo deste item.
- [ ] Alterar a fórmula de impulso aplicada a corpos/ragdoll (mesma função de origem, mas ramo diferente) — este item cobre só o ramo de itens largados.
- [ ] Adicionar limite de raio/distância para a aplicação da força (já discutido e descartado numa investigação anterior desta sessão — custo já é autolimitado, sem necessidade de um teto adicional).

## Referências

- Investigação e discussão na mesma sessão que entregou o item `006` (Item Physics Sem Expirar): usuário reportou, após validar o item 006, que tiros não moviam capacete/fone/arma mas moviam máscara, e uma granada movia tudo — investigação confirmou matematicamente a causa (dependência de peso no cálculo de impulso). Ver `mods/VisceralCombat/memory/sessions.md`, entrada mais recente.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Item criado via `/add-backlog-item` |
| 2026-09-21 | Revisão `/review-spec` — 1 gap corrigido (critérios de aceite reforçados com limite superior explícito pra evitar overcorreção) + 1 corner case adicionado (risco de cálculo de impulso compartilhado com o ramo de corpo/ragdoll, marcado `<!-- review: -->` pra confirmação na spec técnica) + limpeza de identificadores de código (`GrenadeItemsPatch`) removidos da spec funcional |
