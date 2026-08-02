# 001 — Morte desligada com timer

**Mod:** TRL-PvpMode
**Status:** Backlog
**Criado:** 2026-08-01

## Visão geral

Fundação do modo "vidas por raid". Hoje, quando o jogador morre, ou a partida acaba na hora, ou — com o
mod de terceiro que serviu de referência — o personagem continua se mexendo pelo mapa parecendo vivo,
sem que ninguém mais na partida saiba que ele caiu. Este item entrega o **estado de caído**: ao morrer
com vidas disponíveis, o personagem é desligado no lugar onde caiu, todos os outros participantes veem
o corpo no chão, e uma contagem regressiva configurável decide o destino — se ela terminar sem o jogador
agir, a morte é definitiva e a partida encerra normalmente.

A escolha de renascer (a tecla, o sorteio do local, o teleporte) é o item **002**; aqui entrega-se apenas
o estado de caído, a contagem e o desfecho por tempo esgotado.

## Comportamento atual

- **No servidor hoje:** a mecânica de resgate cooperativo está desligada na configuração. Morrer encerra
  a partida imediatamente — tela de morte, câmera de espectador e volta ao menu.
- **Com o mod de referência instalado** (o que motivou este trabalho): a morte é bloqueada e o jogador
  entra num estado "crítico", mas o travamento de movimento **expira depois de 2 segundos**. Passado
  esse tempo, o personagem volta a se mover deitado pelo mapa. Pior: como a morte é bloqueada antes de
  qualquer aviso ser enviado, **nada disso é comunicado aos outros participantes** — para o anfitrião,
  para os outros jogadores e para as IAs, ele segue vivo, andando e sendo um alvo válido.

## Comportamento desejado

Ao sofrer dano fatal **tendo vidas disponíveis**, o jogador entra em **estado de caído**:

1. O personagem para de responder a comandos de movimento e de postura — **sem prazo de validade**. Ele
   fica exatamente onde caiu até o desfecho.
2. A arma é guardada e a tela do jogador escurece, sinalizando que ele está fora de combate.
3. Para todos os outros participantes da partida, o corpo aparece **caído no chão**, imóvel, e deixa de
   ser tratado como um alvo em pé se movendo.
4. Enquanto caído, o jogador **não sofre mais dano** — o corpo no chão não pode ser "morto de novo".
5. **Ninguém pode levantá-lo.** Mirar no corpo não oferece opção de resgate a nenhum companheiro.
6. Uma **contagem regressiva** aparece na tela do caído, com duração configurável. Ao chegar a zero sem
   que ele escolha renascer, a morte se consuma e a partida encerra pelo caminho normal do jogo — tela
   de fim de raid, estatísticas e destino do inventário como em qualquer morte.

Morrer **sem vidas disponíveis** não aciona nada disso: é morte comum, imediata.

### Opções configuráveis

Uma chave no menu de configuração, **desligada por padrão** — o comportamento de saída é o mais permissivo
(toda morte dá chance, e o caído é intocável):

| Opção | Desligada (padrão) | Ligada |
|---|---|---|
| **Tiro na cabeça mata direto** | Headshot leva ao estado de caído como qualquer dano | Headshot encerra a partida na hora, ignorando as vidas |

> **Movidas para o item 005 após o review técnico:** *permitir finalizar o caído* e *granada mata direto*.
> A primeira não funciona pelo caminho previsto — ao cair, o dano do jogador é zerado e o corpo vai para a
> camada de cadáver, então provavelmente o tiro nem gera evento de dano; exige outro mecanismo e validação
> em partida. A segunda depende da mesma fonte de informação indisponível e acompanha a primeira.

Além dessas, a **duração da contagem** e o **número de vidas por partida** são configuráveis (o contador
em si é o item 004; aqui basta a chave existir e ser respeitada).

**Duração da contagem igual a zero significa "sem limite"** — o jogador fica caído indefinidamente até
escolher renascer. É a mesma convenção da mecânica de origem, onde tempo zero desliga o sangramento.

### Quando a vida é debitada

A vida é debitada **no momento em que o jogador escolhe renascer** (item 002), não ao cair. Cair apenas
consulta se **há** vida disponível. Consequência: quem cai e deixa a contagem estourar morre sem gastar
vida — o que é irrelevante para ele (a partida acabou), mas mantém o número exibido honesto enquanto ele
está caído decidindo.

## Critérios de aceite

- [ ] Ao morrer com vidas disponíveis, o personagem não se move nem troca de postura **em nenhum momento**
      após cair — verificável tentando andar, agachar e deitar por mais de 30 segundos seguidos.
- [ ] A tela do caído escurece e a arma sai das mãos assim que ele cai.
- [ ] A contagem regressiva aparece na tela do caído, parte do valor configurado nas opções e, ao zerar,
      encerra a partida pelo caminho normal (tela de fim de raid com estatísticas — não um travamento nem
      um retorno abrupto ao menu).
- [ ] Mirar no corpo de um companheiro caído **não** oferece opção de levantá-lo, em nenhuma distância.
- [ ] Morrer sem vidas disponíveis produz exatamente a morte comum do jogo, sem estado de caído e sem
      contagem.
- [ ] Com a opção no padrão (desligada), headshot leva ao estado de caído normalmente; ligando-a, headshot
      encerra a partida na hora.
- [ ] Morte por fome, desidratação ou overdose de estimulante encerra a partida pelo caminho normal, com
      tela de fim de raid — **nunca** deixa o jogador num estado sem caído e sem morte.
- [ ] **Fika/multiplayer:** os outros participantes veem o corpo caído na posição da morte; as IAs deixam
      de atirar nele e passam a tratá-lo como abatido; o estado de caído afeta **somente** quem morreu —
      nenhum outro jogador ou bot tem movimento, postura ou tela alterados; e funciona também quando o
      jogador está **sozinho na partida** ou é o **último humano vivo** do grupo.
- [ ] **Estado entre raids:** a contagem de vidas e o estado de caído são zerados no início de cada
      partida; encerrar por extração, morte, desaparecido em combate ou fechamento forçado do jogo não
      deixa resíduo que altere o comportamento da partida seguinte.

## Corner cases

- [ ] **Único humano na partida ou último vivo.** A mecânica de resgate cooperativa em que este item se
      apoia condiciona o estado de caído a existir alguém vivo para resgatar. Como aqui **não há resgate
      por companheiro**, essa condição perde o sentido e não pode bloquear o estado de caído — do
      contrário, jogar sozinho (o caso mais comum de teste) morreria direto.
- [ ] **Todos os companheiros morrem enquanto um jogador está caído.** A mecânica de origem força a morte
      do caído nesse cenário ("acabou para todos"). Com vidas próprias, isso precisa deixar de acontecer.
- [ ] **Dois danos fatais no mesmo instante** (explosão + tiro, ou dois atiradores). O jogador não pode
      entrar em caído duas vezes, consumir duas vidas, nem ter a animação de guardar a arma interrompida
      no meio.
- [ ] **Morte por fome, desidratação ou overdose de estimulante.** São mortes por desgaste, não por
      combate — devem matar direto, sem estado de caído.
- [ ] **Cair durante transição de cena** (extração em andamento, dentro do veículo blindado, mudança de
      mapa). O estado de caído não pode prender o jogador num limbo do qual a partida não consiga sair.
- [ ] **O anfitrião encerra a partida enquanto alguém está caído.** O encerramento tem que acontecer
      normalmente, sem depender de o caído responder.
- [ ] **Sair pelo fechamento forçado do jogo (alt-F4) estando caído.** Não pode deixar o perfil num estado
      em que o jogador aparece vivo na partida seguinte.
- [ ] **O anfitrião é quem cai.** Se quem hospeda a partida entra em caído e deixa a contagem estourar, o
      encerramento não pode arrastar a partida dos demais de forma diferente do que uma morte comum do
      anfitrião já faria hoje.
- [ ] **Reconexão estando caído.** Se o jogador cair e reconectar à partida, ele não pode voltar em pé e
      ileso — nem ficar preso num estado caído sem contagem.
- [ ] **Estar no esconderijo (hideout) ou no menu.** Nenhuma parte da mecânica pode agir fora de uma
      partida: sem estado de caído, sem contagem, sem consumo de vida.
- [ ] **Contagem configurada como zero.** Significa "sem limite" — o jogador fica caído até decidir, e a
      contagem não aparece na tela. Não pode ser lido como "morre instantaneamente".

## Fora de escopo

- [ ] A tecla de renascer, o sorteio do ponto de renascimento e o teleporte — item **002**.
- [ ] O aviso de rede que crava a nova posição para os outros participantes — item **003**.
- [ ] O contador de vidas na tela — item **004**.
- [ ] Cadáver saqueável no local da morte (o jogador mantém o equipamento — decisão de produto fechada).
- [ ] Regras de alternância entre partidas PVP e PVE.

## Pré-requisitos operacionais

Não são código, mas travam o teste se esquecidos:

- [ ] Ligar a mecânica de resgate cooperativo na configuração do servidor (`fika.jsonc` →
      `reviveConfig.enabled: true`). Sem isso, o estado de caído não existe para o mod se apoiar.
- [ ] **Desinstalar o mod de referência** (`somtam.PlayerLives.dll`, hoje presente em
      `BepInEx/plugins/`). Os dois disputam o mesmo ponto de interceptação da morte e o resultado é
      imprevisível.

## Referências

- [mod-backlog.md](../mod-backlog.md) — decisões de produto fechadas no cabeçalho
- [DEPRECATED.md](../../../TRL-PvpMode-deprecated/DEPRECATED.md) — abordagem anterior e por que foi descartada

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/add-backlog-item` |
| 2026-08-01 | Spec funcional criada via `/create-spec` |
| 2026-08-01 | Decisões de produto do host: morte direta (cabeça/granada) e finalização do caído viram opções no F12, desligadas por padrão |
| 2026-08-01 | Revisão `/review-spec` — 4 gaps + 4 corner cases corrigidos (débito da vida, contagem zero, critério Fika verificável, hideout/reconexão/anfitrião) |
