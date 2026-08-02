# 002 — Renascer em spawn aleatório

**Mod:** TRL-PvpMode
**Status:** Backlog
**Criado:** 2026-08-01
**Depende de:** [001 — Morte desligada com timer](../001-morte-desligada-timer/001-morte-desligada-timer-01-spec.md)

## Visão geral

O item 001 entregou o estado de caído e a contagem regressiva, mas com uma única saída: deixar o tempo
acabar e morrer. Este item entrega **a outra saída** — segurar uma tecla e renascer em outro ponto do
mapa, gastando uma vida, com o equipamento intacto.

## Comportamento atual

Depois do 001, o jogador caído fica travado no chão vendo a contagem correr. A tecla nativa de "desistir"
do Fika ainda existe e apenas antecipa a morte. Não há como voltar à partida.

Com a contagem configurada como `0` (sem limite), o componente de contagem do Fika sai antes de ler o
teclado — nem a tecla de desistir funciona. Hoje isso deixa o jogador **sem nenhuma saída**, e é este
item que fecha esse buraco.

## Comportamento desejado

Estando caído e com vida disponível:

1. **Segurar** uma tecla configurável (padrão sugerido: a mesma que o Fika usa para desistir) por um
   tempo curto e contínuo faz o jogador renascer. Soltar antes cancela, sem gastar nada.
2. Uma vida é debitada **neste momento** — não ao cair.
3. O jogo sorteia um **ponto de nascimento de jogador do mapa**, entre os mesmos que a partida usa no
   início, respeitando o lado (PMC ou Scav), e **evitando pontos próximos de outros jogadores e bots**.
4. O personagem reaparece de pé nesse ponto: vida restaurada, membros destruídos recuperados
   parcialmente, arma de volta às mãos, tela normal e movimento liberado.
5. **O equipamento continua o mesmo** — nada é perdido, nada é duplicado, e não fica cadáver no local
   da morte.
6. Por alguns segundos após renascer o jogador **não sofre dano**, para não morrer de imediato caso o
   ponto sorteado seja quente.
7. Sem vida disponível, segurar a tecla não faz nada além de um aviso curto na tela.

## Critérios de aceite

- [ ] Segurar a tecla estando caído e com vida disponível devolve o jogador ao jogo **em outro ponto do
      mapa**, verificável comparando as coordenadas antes e depois.
- [ ] Soltar a tecla antes de completar cancela sem debitar vida e sem mover o personagem.
- [ ] O número de vidas cai exatamente **um** por renascimento; com vidas ilimitadas, nunca cai.
- [ ] Após renascer, o jogador anda, agacha, atira e recarrega normalmente — sem resíduo do estado de
      caído (tela escura, travamento de eixos, arma guardada).
- [ ] O inventário após renascer é **idêntico** ao do momento da morte, e não existe cadáver saqueável
      no local onde caiu.
- [ ] A janela de invulnerabilidade dura o tempo configurado e termina — o jogador volta a levar dano.
- [ ] Com a contagem configurada como `0` (sem limite), a tecla de renascer **continua funcionando** —
      ela não pode depender do componente de contagem do Fika.
- [ ] **Fika/multiplayer:** o jogador reaparece no ponto novo para os outros participantes e para as
      IAs, sem atravessar o mapa deslizando; o corpo caído some do lugar antigo. *(A garantia forte de
      sincronia é o item 003; aqui basta verificar que não regride.)*
- [ ] **Estado entre raids:** vidas voltam ao valor configurado a cada partida; renascer numa partida
      não afeta a seguinte.

## Corner cases

- [ ] **O ponto sorteado é o mesmo onde o jogador morreu** (mapas pequenos, poucos pontos). Deve haver
      tentativa de sortear outro; se não houver alternativa, renascer no mesmo lugar é aceitável, mas
      não pode falhar.
- [ ] **Mapa com pouquíssimos pontos de nascimento** (Factory, Labs). O sorteio não pode entrar em laço
      nem travar a partida.
- [ ] **Todos os pontos estão perto de inimigos.** O filtro de distância precisa ceder em vez de não
      devolver ponto nenhum.
- [ ] **A contagem zera no exato instante em que o jogador completa o segurar da tecla.** Só um dos dois
      desfechos pode acontecer — nunca renascer e morrer ao mesmo tempo.
- [ ] **Segurar a tecla sem estar caído** (em pé, no menu, no esconderijo) não pode ter efeito nenhum.
- [ ] **Renascer com a última vida** deixa o contador em zero; a próxima morte tem que ser definitiva.
- [ ] **Renascer duas vezes muito rápido** (cair logo após a invulnerabilidade acabar) não pode acumular
      estado nem debitar duas vidas de uma vez.
- [ ] **O ponto sorteado é dentro de geometria** (dentro de uma parede, no ar, sob o chão). O jogador
      não pode ficar preso nem cair para fora do mapa.

## Fora de escopo

- [ ] O aviso de rede que crava a nova posição para os outros participantes e evita o deslize — item **003**.
- [ ] O contador de vidas na tela — item **004**.
- [ ] Cadáver saqueável no local da morte — descartado por decisão de produto.
- [ ] Escolher o ponto de renascimento manualmente (mapa/menu). O sorteio é automático.

## Referências

- [001 — Morte desligada com timer](../001-morte-desligada-timer/001-morte-desligada-timer-01-spec.md)
- [Review técnico do 001](../001-morte-desligada-timer/001-morte-desligada-timer-03-spec-tech-review-01.md) — R-05 explica por que a tecla precisa de leitura própria

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/add-backlog-item` |
| 2026-08-01 | Spec funcional criada via `/create-spec` |
