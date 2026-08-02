# Changelog — TRL-PvpMode

Formato baseado em [Keep a Changelog](https://keepachangelog.com/); versionamento [SemVer](https://semver.org/).

## [0.3.0] — não lançado

Item de backlog **002 — Renascer em spawn aleatório**.

### Adicionado

- **Renascer segurando uma tecla** (padrão `F5`, tempo configurável). Soltar antes cancela sem gastar
  vida. A leitura do teclado é própria do mod — não pode depender do componente de contagem do Fika,
  que com tempo `0` sai antes de ler o teclado.
- **Sorteio do ponto de nascimento** entre os mesmos que a partida usa no início, respeitando o lado
  (PMC/Scav). O pedido usa um identificador aleatório a cada tentativa, o que faz o sistema de spawn
  aplicar sozinho o afastamento de quem já está no mapa.
- **Invulnerabilidade após renascer**, configurável (padrão 5s, `0` desliga).
- Vida restaurada e equipamento intacto; sem cadáver no local da morte.

### Notas

- A ordem dos passos é obrigatória: teleportar → religar → curar → proteger. Religar antes de teleportar
  faz o corpo reaparecer na posição antiga; curar antes de religar opera sobre um controlador que se
  considera morto; proteger antes de religar é sobrescrito pela restauração do dano.
- A posição nova ainda chega aos outros clientes pelo fluxo normal de estado, com interpolação — pode
  aparecer um deslize. É o que o item 003 resolve.

## [0.2.0] — não lançado

Item de backlog **001 — Morte desligada com timer**.

### Adicionado

- **Modo de vidas por raid.** Morrer com vida disponível não encerra mais a partida: o personagem entra
  no estado de caído do Fika — travado onde caiu, sem prazo, com a tela escurecida e a arma guardada — e
  o corpo aparece desligado no chão para todos os outros participantes.
- **Contagem regressiva configurável** para decidir. Ao zerar, a morte é definitiva e a partida encerra
  pelo caminho normal. `0` = sem limite.
- **Sem resgate por companheiro:** a opção "levantar" some do menu de interação; a única saída é
  renascer (item 002).
- Funciona **jogando sozinho** e sendo o **último vivo** do grupo — cenários que o mecanismo nativo do
  Fika bloqueia por exigir alguém para te resgatar.
- Quatro opções no F12, seção `Lives` — ver [PROPRIEDADES.md](../PROPRIEDADES.md).
- Avisos na tela quando um pré-requisito falta: `reviveConfig.enabled` desligado no servidor ou mod
  PlayerLives instalado (conflito no mesmo ponto de morte).

### Notas

- Morte por fome, desidratação ou overdose de estimulante **sempre** encerra a partida na hora. Sem esse
  tratamento, o destravamento do estado de caído deixaria o jogador num limbo: morto para o sistema de
  vida, sem estado de caído e sem tela de fim de raid.
- Reinício do mod: a tentativa anterior de respawn (destruir e recriar o `LocalPlayer`) foi arquivada em
  `mods/TRL-PvpMode-deprecated/` e não é base deste código.
