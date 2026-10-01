# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-09-08

## Visão geral

Hoje, segurar a tecla de correr do jogo enquanto agachado ou deitado (prone) faz o personagem se levantar automaticamente antes de sprintar — não existe nenhuma forma de se mover mais rápido permanecendo nessas posturas. Este item adiciona duas variantes independentes dessa mecânica — "correr agachado" e "rastejar rápido" (prone) — reaproveitando a mesma tecla de correr já configurada pelo jogador, com um custo extra de stamina que continua respeitando a progressão de habilidades do personagem.

## Comportamento atual

Ao segurar a tecla de correr (sprint) nativa enquanto o jogador está agachado ou deitado, o jogo levanta o personagem automaticamente antes de iniciar a corrida — a tecla de correr não tem nenhum efeito na velocidade enquanto o jogador permanece nessas posturas. O mod já acelera as **transições** entre posturas (item 005 — agachar/deitar/inclinar mais rápido) e oferece posturas customizadas (Stances 1/2/3), mas nenhuma dessas features afeta a velocidade de deslocamento sustentado enquanto o jogador já está parado numa postura baixa (agachado padrão do jogo, ou prone).

## Comportamento desejado

Ao segurar a mesma tecla de correr já configurada pelo jogador:

- **Agachado:** o personagem permanece agachado (não levanta) e passa a se mover mais rápido nessa postura, até um teto configurável sempre abaixo da velocidade de sprint em pé.
- **Prone:** o personagem permanece deitado e passa a rastejar mais rápido, também com teto configurável.

Mover-se dessa forma consome stamina — para cada postura, o jeito como isso acontece depende de o jogo **já ter ou não** um custo nativo de stamina para "correr" naquela postura especificamente (confirmado durante a implementação: o jogo já cobra stamina nativamente ao correr agachado, mas não ao correr rastejando — ver corner case correspondente). Onde o jogo já cobra, o mod não adiciona nada por cima. Onde o jogo não cobra nada, o mod adiciona um custo configurável para essa postura não ficar de graça — e esse custo, onde existir, continua menor para personagens com habilidades relevantes mais desenvolvidas (não é um valor fixo que ignora a progressão do personagem). As duas variantes podem ser ativadas/desativadas de forma independente no F12, e ambas vêm **desativadas por padrão** até serem validadas in-game.

## Critérios de aceite

- [ ] Segurar a tecla de correr enquanto agachado (com a opção habilitada) mantém o jogador agachado e aumenta sua velocidade de deslocamento, em vez de levantá-lo.
- [ ] Segurar a tecla de correr enquanto rastejando (com a opção habilitada) mantém o jogador deitado e aumenta a velocidade do rastejamento.
- [ ] ~~O ganho de velocidade é gradual~~ **Revertido em 2026-09-09** — testado in-game e não fez sentido no mod (ganho e perda de velocidade são ambos imediatos, junto com a ativação/desativação, sem rampa).
- [ ] A velocidade alcançada em qualquer uma das duas posturas aceleradas nunca iguala nem ultrapassa a velocidade de sprint em pé — o próprio mod impede isso (clamp), não é só uma recomendação de valor para o jogador respeitar ao configurar o F12. <!-- review: confirmar se o clamp deve ser um teto travado no código (a faixa do slider no F12 já nasce sem permitir passar do sprint em pé) ou um clamp em runtime que aceita qualquer valor no F12 mas nunca aplica acima do sprint — afeta como a spec técnica define o AcceptableValueRange. -->
- [ ] Onde o mod precisa adicionar um custo de stamina (porque o jogo não cobra nada nativamente naquela postura), esse custo é configurável no F12 — mesmo padrão de configurabilidade das demais props do mod. **Confirmado in-game (2026-09-09):** o jogo já cobra stamina nativamente ao correr agachado (o mod não precisa adicionar nada) — a config de sobretaxa do agachado foi removida. O rastejar acelerado (prone) **não** tem custo nativo de stamina — a config de sobretaxa do prone foi mantida por esse motivo.
- [ ] Soltar a tecla de correr (ou parar de se mover) faz o personagem voltar imediatamente ao ritmo normal daquela postura, sem o jogo tentar levantá-lo sozinho.
- [ ] Usar qualquer uma das duas posturas aceleradas consome stamina mais rápido do que caminhar normalmente naquela postura (nativamente no agachado, via sobretaxa do mod no prone), e onde há sobretaxa do mod, ela é proporcionalmente menor para personagens com as habilidades relevantes mais desenvolvidas — não é um valor fixo que ignora a progressão de habilidades do personagem.
- [ ] As duas variantes (agachado acelerado, prone acelerado) podem ser ativadas/desativadas de forma independente no F12; ambas vêm **desativadas por padrão**.
- [ ] Correr agachado numa postura muito baixa sobe a postura para um nível mínimo configurável enquanto a corrida estiver ativa (evita a sensação estranha de "correr agachado até o chão"), e restaura exatamente a postura que o jogador tinha **antes** de começar a correr assim que ele solta a tecla ou para de se mover — mesmo que a postura tenha sido ajustada manualmente durante a corrida.
- [ ] **Fika/multiplayer:** outros jogadores no raid devem ver o jogador local se movendo na velocidade correta (agachado/prone acelerado) enquanto a feature está ativa, do mesmo jeito que qualquer mudança de velocidade de movimento já é replicada hoje. Se a investigação técnica encontrar alguma limitação real de sincronização para esse caso específico, ela deve ser documentada explicitamente na spec técnica antes de fechar o item — não presumida como resolvida.
- [ ] **Estado entre raids:** os dois toggles e seus multiplicadores/tetos são configs persistentes do F12, como as demais do mod — o valor escolhido permanece entre raids e reinícios do jogo; nenhuma raid anterior deixa a feature "presa" ativa ou inativa numa raid nova.

## Corner cases

- [ ] Soltar a tecla de correr no meio da aceleração (agachado ou prone) não deixa o personagem preso numa velocidade intermediária, nem dispara a subida automática que o jogo tentaria fazer nativamente ao detectar o sprint.
- [ ] Trocar de postura durante a aceleração (ex.: levantar no meio do crouch-run, ou deitar no meio dele) encerra a aceleração da postura anterior de forma limpa, sem misturar os dois multiplicadores nem herdar velocidade de uma postura para a outra.
- [ ] Ficar sem stamina no meio da aceleração interrompe a postura acelerada (volta ao ritmo normal daquela postura), da mesma forma que ficar sem stamina interrompe o sprint em pé hoje — não trava o jogador numa velocidade que ele não tem mais fôlego para sustentar.
- [ ] Tentar ativar a aceleração enquanto mirando (ADS) ou com a arma apoiada/montada segue a mesma regra que outras transições de postura do mod já aplicam nesses estados (bloquear a ativação, ou cancelar a mira/montagem de forma consistente) — não abre um caminho novo e inconsistente.
- [ ] Um espaço apertado onde o jogo normalmente não deixaria o personagem ficar em pé não pode virar, sem querer, uma forma de "destravar" a corrida (a aceleração agachada/prone precisa continuar respeitando qualquer restrição de ambiente que hoje impede levantar ali).
- [ ] Iniciar a raid já em Stance 2 - Low Ready (feature existente do mod) ou já agachado por outro motivo não deve impedir nem confundir a detecção da tecla de correr para esta feature.
- [ ] A feature reage apenas à tecla de correr **do jogador local** — bots e outros jogadores observados num raid Fika executando ações parecidas (correr, agachar) não acionam nem são afetados pela aceleração de ninguém além de si mesmos.
- [ ] Sair de uma raid com a aceleração momentaneamente ativa (tecla ainda segurada no instante da saída) não deixa nenhum estado "preso" ativo ao entrar na raid seguinte — toda raid nova começa com a aceleração desligada até o jogador segurar a tecla de novo, independente do toggle do F12 estar habilitado.
- [ ] Nenhuma causa de limitação de velocidade introduzida por esta feature fica "presa" ativa depois que o jogador solta a tecla ou perde a condição de uso (ex.: fica sem stamina) — o mod já teve um bug parecido registrado na sua memória de projeto (velocidade que fica lenta sem motivo aparente e só destrava ao mirar) e esta feature não deve reintroduzir esse padrão.
- [ ] Ativar o agachado-acelerado enquanto uma postura customizada do mod (Stance 1/2/3) está ativa não gera conflito — a postura acelerada e a Stance customizada continuam combinando do mesmo jeito que já combinam hoje sem a aceleração.
- [ ] Prone já força a Stance 0 (comportamento existente do mod) — o rastejar rápido precisa continuar respeitando essa regra, sem reintroduzir uma Stance customizada enquanto deitado.
- [ ] Correr agachado bem baixo num espaço com teto baixo demais até para o nível mínimo configurado: a corrida continua funcionando normalmente (só a velocidade), sem forçar a postura a subir e sem nenhum erro — a mesma restrição de ambiente que hoje impede levantar também vale para o piso de postura.
- [ ] Segurar a tecla de correr durante uma transição de agachar/deitar ainda em andamento (o multiplicador de transição do item 005 ainda rodando) não produz uma velocidade combinada incorreta nem um estado intermediário travado — a aceleração só passa a valer quando a postura-alvo (agachado ou prone) é efetivamente alcançada.
- [ ] **(Bug real encontrado e corrigido em 2026-09-09)** Segurar a tecla de correr enquanto já está deitado e PARADO, e só depois começar a se mover, funciona igual a segurar a tecla já em movimento — a detecção da tecla não pode depender de o jogador estar parado ou andando no momento exato do aperto.

## Fora de escopo

- [ ] A definir

## Referências

- Item relacionado: [005-velocidade-agachar-inclinar/](../005-velocidade-agachar-inclinar/) — mesmo espírito (multiplicador de velocidade por postura), mas para transição, não deslocamento sustentado.
- Item relacionado: [012-controlador-central-stamina/](../012-controlador-central-stamina/) — controlador de stamina de **braço** já existente; a stamina de **pernas** (sprint) usada por este item é um sistema diferente, a mapear na spec técnica.
- Padrão de toggle default-seguro: [021-toggle-tombamento-mira-lateral/](../021-toggle-tombamento-mira-lateral/) — feature nova sensível vem desligada por padrão até validação in-game.
- Investigação técnica preliminar (via chat, 2026-09-08): [018-rastejar-rapido-00-ideia.md](018-rastejar-rapido-00-ideia.md) — confirma que o jogo força o personagem a ficar em pé ao entrar em sprint, incondicionalmente, hoje.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Item criado via `/add-backlog-item` (ideia bruta, só prone). |
| 2026-09-08 | Escopo ampliado (agachado + prone) e investigação técnica preliminar registrada em `00-ideia.md`. Spec funcional criada via `/create-spec`. |
| 2026-09-08 | Revisão `/review-spec` — 2 gaps + 7 corner cases corrigidos |
| 2026-09-09 | Adicionado critério de aceite: ganho de velocidade gradual (não instantâneo), pedido do usuário durante a spec técnica. Perda continua imediata (sem contradição com o critério de soltar a tecla). |
| 2026-09-09 | Adicionados critério de aceite (piso de postura mínima durante o crouch-run, com restauração exata ao parar) e corner case correspondente — pedido do usuário durante a spec técnica. |
| 2026-09-09 | Validação in-game do usuário: crouch-run funcionou; prone-run não mudava a velocidade. Causa raiz corrigida (detecção de tecla dependia do sub-estado nativo ativo no momento do aperto). Removida a rampa de velocidade (não fez sentido no mod) e a sobretaxa de stamina do agachado (o jogo já cobra nativamente) — mantida a do prone (o jogo não cobra nada nativamente ali). Critérios e corner cases atualizados de acordo. |
