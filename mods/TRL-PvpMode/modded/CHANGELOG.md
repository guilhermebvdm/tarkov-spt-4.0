# Changelog — TRL-PvpMode

Formato baseado em [Keep a Changelog](https://keepachangelog.com/); versionamento [SemVer](https://semver.org/).

## [0.8.0] — não lançado

Review completo do mod inteiro, com três lentes independentes (integração/estado · coop/Fika ·
padrões do repo). **33 achados, todos aplicados.** Duas lentes chegaram ao mesmo bloqueador por
caminhos diferentes.

### Corrigido — bloqueadores

- **O renascimento nunca teria funcionado.** A guarda que impede renascer em cima do próprio cadáver
  testava se o jogador estava vivo, partindo da premissa de que quem está caído está "vivo". Não
  está: o Fika marca o jogador como morto ao entrar no estado de caído e só reverte ao sair. A guarda
  ficou com a polaridade invertida — bloqueava todo renascimento legítimo e deixava passar
  exatamente a janela que deveria barrar. O discriminante agora é o estado terminal real.
- **O contador sumia justo quando importa.** Pela mesma premissa, o indicador destacado no estado
  caído era código inalcançável — e é o único momento para o qual ele existe.
- **O pacote de build levava 22 MB de bibliotecas da BSG e do Fika.** Faltava marcar as referências
  como não-copiáveis. Redistribuí-las reprovaria na publicação e causaria conflito de identidade de
  tipo. A saída caiu para 68 KB.

### Corrigido — graves

- **Um par sem o mod não é só uma brecha de jogabilidade, é falha de rede.** O anfitrião retransmite
  o aviso de renascimento em bytes crus **antes** de decodificá-lo; a máquina sem o mod não sabe o
  que é aquilo e derruba a fila de eventos de rede daquele quadro — para todos os pares e todos os
  mods. Elevado a requisito duro na documentação, com a razão explicada.
- **Dependência do TRL-Fixes declarada**, com aviso na tela quando ausente. Sem ele, todo jogador que
  renasce fica impossível de acertar para os outros: o mod transforma um bug ocasional do Fika no
  caminho único e garantido.
- **Renascimento que falha no meio agora se cancela.** O aviso de rede é o primeiro passo; sem
  cancelamento, uma falha nos passos seguintes deixaria o corpo cravado, para os outros, num ponto
  onde o jogador nunca chegou.
- Vidas ilimitadas passam a ser fotografadas no início da raid, como o total — antes metade da mesma
  opção reagia ao vivo, e mudá-la no meio da partida zerava o contador.
- A referência ao gerenciador de rede é solta no fim da raid; antes retinha o objeto e todo o grafo
  de participantes até a raid seguinte.
- O indicador não fica mais desligado pelo resto da **sessão** após um erro isolado.
- A proteção ao renascer não remove mais a invulnerabilidade de quem caiu de novo dentro da janela.
- Falha de carregamento agora desativa o mod por inteiro, em vez de deixá-lo meio-carregado mexendo
  em participantes sem nenhum patch instalado.
- Só o alvo caído aceita o corte seco de posição, e a leitura do teclado ficou mais barata.

### Limitações declaradas nesta rodada

Configuração diferente entre jogadores não é suportada (a assimetria pode deixar o caído sem saída);
`grenadesKills` do servidor deixa de valer; e as opções passam a ser explicitamente por partida.

## [0.7.0] — não lançado

Correções do review adversarial dos itens 003 e 004 (12 achados, todos aplicados).

### Corrigido

- **O conserto do deslize reintroduzia o próprio defeito.** Limpar o histórico de posições também
  apaga a única informação que o Fika usa para rejeitar um estado atrasado, e os dois fluxos correm
  em canais sem ordem garantida entre si. Agora o aviso é o primeiro passo do renascimento e o
  receptor defende a posição por 1,5s.
- Posições implausíveis (`NaN`, infinito, fora do mapa) são rejeitadas — uma delas deixaria o corpo
  daquele participante permanentemente inválido.
- Os ícones de efeito da plaquinha de vida também são limpos, como o próprio Fika faz na reconexão.
- Falha de registro do pacote desiste após 5 tentativas, em vez de registrar erro a cada quadro.
- O indicador de vidas só roda no evento de pintura, e some na tela de fim de raid.
- Campo de rotação removido do pacote: era carga morta, revertida no quadro seguinte.

## [0.6.0] — não lançado

Correções do review adversarial do item 002 (13 achados, todos aplicados).

### Corrigido

- **O ponto de renascimento agora é sorteado de verdade.** A versão anterior chamava a busca de spawn
  do jogo acreditando que ela variava o resultado — não varia: devolve sempre o ponto mais distante de
  todos, então o jogador renascia no mesmo canto do mapa morte após morte, e o laço de cinco tentativas
  era custo puro. Agora montamos a lista de candidatos, filtramos por lado e distância de quem está
  vivo, e escolhemos ao acaso. Nova opção `Min Spawn Distance (m)`.
- **O teto de revives do Fika não corta mais as vidas em silêncio.** Cada renascimento incrementa um
  contador do Fika que, com `maxRevives` definido no servidor, encerrava a partida com vidas ainda no
  indicador. O teto passa a ser liberado no início da raid.
- **Não é mais possível renascer em cima do próprio cadáver.** O Fika não limpa o estado de caído
  quando o prazo acaba; por 1–2 quadros dava para completar a tecla já morto.
- **O jogador levanta de pé.** A pose deitada aplicada ao cair não era desfeita ao renascer.
- **Fratura, dor e intoxicação não sobrevivem mais ao renascimento** — só o sangramento era removido.
- **A vida só é debitada depois do ponto de não-retorno.** Antes, uma falha no meio da sequência
  deixava "vida gasta, teleportado e ainda caído".
- **Digitar no chat não gasta mais uma vida** com a tecla rebindada para uma letra.

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
