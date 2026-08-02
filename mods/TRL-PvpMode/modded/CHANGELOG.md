# Changelog — TRL-PvpMode

Formato baseado em [Keep a Changelog](https://keepachangelog.com/); versionamento [SemVer](https://semver.org/).

## [0.5.0] — não lançado

Item de backlog **004 — Contador de vidas na tela**.

### Adicionado

- **Indicador de vidas restantes.** Discreto no canto durante a partida; **destacado no centro** quando
  o jogador está caído — que é o momento em que a informação decide a próxima ação. Vidas ilimitadas
  aparecem como `∞`. Opção `Show Lives Counter` no F12 para esconder.

### Notas

- Desenhado via `OnGUI` em vez de um elemento da interface do jogo: é um texto só, não depende de tipos
  internos do Fika ou do EFT (que mudam entre versões) e some sozinho quando a guarda de contexto
  reprova. O corpo do método sai na primeira linha fora de partida.

## [0.4.0] — não lançado

Item de backlog **003 — Sincronização do respawn em coop**.

### Adicionado

- **Corte seco na posição ao renascer.** Um aviso de rede próprio faz os outros clientes limparem o
  histórico de posições e cravarem a posição nova. Sem ele, o corpo alheio percorre o trajeto entre o
  ponto da morte e o do renascimento em linha reta, porque o Fika interpola entre estados sem nenhuma
  detecção de teleporte.

### Notas de rede (AP-11)

O pacote segue o padrão canônico do repo: envelope de comprimento, leitura só com `TryGet*`, marca de
validade antes de processar, todos os campos zerados na entrada do `Deserialize` (a instância é
reutilizada entre recepções), envio só da linha principal e registro rastreado por **instância** do
gerenciador de rede — o Fika o recria a cada troca de sessão e o novo nasce vazio. Nenhum
`UnregisterPacket`: fora de raid o callback sai pela guarda.

Isso não é zelo excessivo: no Fika, uma exceção que escape do caminho do pacote **descarta a fila de
eventos daquele quadro para todos os pares e todos os mods** — o sintoma clássico é jogador "patinando".

- `TargetFramework` passou de `net472` para `netstandard2.1`, necessário para usar os tipos de rede do
  Fika. Mesmo alvo que os outros mods do repo que tocam a rede.

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
