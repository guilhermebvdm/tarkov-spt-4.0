# 008 — Acordar/Empurrar Corpos e Itens por Contato Físico (Atropelar)

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-21

## Visão geral

Um corpo morto (ragdoll) ou item físico largado que já "acomodou" (parou de se mexer e entra num estado de repouso físico de baixo custo) hoje só volta a reagir a força se for atingido por tiro ou explosão de granada. Simplesmente encostar ou passar por cima com o próprio personagem ("atropelar") não produz nenhuma reação — mesmo que o corpo tivesse reagido normalmente a esse mesmo contato há poucos segundos, antes de acomodar. Este item propõe que o contato físico do personagem também seja capaz de acordar e empurrar corpos/itens em repouso, com um empurrão proporcional à velocidade de movimento do jogador (esbarrão leve quase não desloca; correndo produz um empurrão perceptível), sem depender de tiro prévio.

## Comportamento atual

- Um corpo recém-morto (ainda não "acomodou") reage normalmente a ser atropelado pelo personagem — se move/empurra ao contato, comportamento já correto hoje.
- Depois que o corpo acomoda (entra em repouso físico após alguns segundos parado), atropelá-lo não produz mais nenhuma reação — o corpo fica "duro" mesmo sendo tocado/empurrado pelo personagem.
- Confirmado pelo usuário: se o corpo já acomodado for atingido por um tiro antes (o que já acorda o corpo pra reagir à bala), o mesmo corpo volta a reagir normalmente a ser atropelado logo depois — ou seja, o corpo é fisicamente capaz de reagir a contato, só não existe hoje nenhum código que faça o simples encostão do personagem "acordar"/empurrar um corpo em repouso sem passar primeiro por um tiro.
- O mesmo tipo de ausência (nenhum código aplica força a partir de um toque físico do personagem) provavelmente afeta também itens físicos largados no chão — ainda não confirmado por teste direto do usuário, só extrapolado do mesmo mecanismo (ver corner case abaixo).

## Comportamento desejado

- Encostar ou passar por cima de um corpo/item em repouso com o personagem, mesmo sem nenhum tiro prévio, acorda e empurra o corpo/item.
- A intensidade do empurrão é proporcional à velocidade de movimento do personagem no momento do contato: um esbarrão leve (andando devagar) desloca pouco; passar correndo produz um empurrão nitidamente mais forte.
- Ficar parado encostado num corpo/item (sem se mover) não aplica força repetidamente nem produz nenhum tremor/vibração contínua.
- O empurrão não deve ser exagerado a ponto de lançar o corpo/item de forma irreal (sem "chute de futebol") — a intensidade máxima fica limitada, mesmo com o jogador correndo o mais rápido possível.
- Corpos que ainda não acomodaram continuam reagindo normalmente (sem regressão no comportamento já correto hoje).
- **Decisão do usuário (2026-09-21):** este comportamento reaproveita as duas opções do F12 que já existem, em vez de criar uma nova — a reação de corpos fica condicionada à opção que já controla se o personagem consegue fisicamente esbarrar em corpos ("Player Body Collision"); a reação de itens fica condicionada à opção "Item Physics" já existente. Ambas as opções passam a também cobrir esse novo tipo de reação (contato), além do que já cobriam antes.

## Critérios de aceite

- [ ] Encostar levemente (andando devagar) num corpo ou item físico já em repouso, sem nenhum tiro prévio, produz um deslocamento pequeno mas perceptível.
- [ ] Passar correndo por cima do mesmo corpo/item em repouso produz um empurrão nitidamente mais forte que o encostão leve, proporcional à velocidade do jogador no momento do contato.
- [ ] Ficar parado encostado, sem se mover, não aplica força repetida nem gera vibração/tremor contínuo no corpo/item.
- [ ] Um corpo ou item nunca atingido por bala ou granada reage normalmente ao ser atropelado (não depende de nenhum tiro prévio pra funcionar).
- [ ] Um bot atropelando um corpo/item (sem ser o jogador) não dispara nenhuma reação de contato nova — comportamento permanece o mesmo de hoje pra bots (fora de escopo, ver seção correspondente).
- [ ] **Fika/multiplayer:** cada jogador que fisicamente encosta no corpo/item vê a reação aplicada localmente, no mesmo padrão já usado pelas outras reações de física deste mod (tiro, granada) — sem introduzir nenhuma sincronização de rede nova além da que já existe hoje.
- [ ] **Estado entre raids:** N/A — a reação de contato é instantânea e não gera nenhum estado que precise sobreviver entre raids.

## Corner cases

- [ ] Vários jogadores encostando/atropelando o mesmo corpo ao mesmo tempo (coop) — os empurrões não podem se acumular numa força absurda; cada contato aplica sua própria reação de forma independente e limitada.
- [ ] Contato constante/prolongado (personagem parado dentro do volume do corpo, ex.: forçado por colisão de outro jogador) — não pode gerar aplicação de força a cada quadro (custo sem teto); precisa de algum tipo de limite/cooldown por contato.
- [x] **Decisão do usuário (2026-09-21):** só o personagem do jogador humano dispara a reação de contato — bots NÃO disparam, justamente pra evitar processamento extra rodando pra cada bot da raid.
- [x] **Decisão do usuário (2026-09-21):** itens físicos ficam confirmados no escopo deste item, junto com corpos — não é mais condicional a uma confirmação técnica prévia; a spec técnica implementa os dois.
- [ ] Corpo/item empurrado em direção a uma parede, obstáculo ou fora dos limites do mapa — o empurrão não pode ser forte o bastante pra atravessar geometria do cenário.
- [ ] Interação com o item `006` (Rigidbody preservado/"dormindo" em itens acomodados) e com o item `007` (intensidade de reação a tiro) — a reação por contato precisa se comportar de forma consistente com as outras formas de força já existentes, sem exigir do jogador uma configuração separada confusa.
- [ ] Parte de corpo já desmembrada e largada separadamente (ex.: um membro cortado, cabeça, tronco) — atropelar essa parte separada precisa reagir de forma consistente com o corpo inteiro, sem tratamento especial que a deixe "imune" ao contato.

## Fora de escopo

- [ ] Empurrar jogadores ou bots **vivos** por contato — esse é um sistema de movimento/colisão totalmente diferente (personagem vivo), não faz parte deste item, que cobre só corpos mortos e itens físicos largados.
- [ ] Veículos ("atropelar" no sentido literal de veículo) — não existe mecânica de veículo dirigível neste jogo/versão; "atropelar" aqui se refere só ao contato do personagem a pé.
- [ ] Alterar a forma como tiro/explosão já acordam corpos/itens hoje — esse mecanismo já funciona corretamente; este item só adiciona um gatilho novo (contato), sem mudar os existentes.
- [ ] Bots disparando a mesma reação de contato — decisão do usuário: só o jogador humano dispara, pra evitar custo de processamento extra por bot na raid.

## Referências

- Investigação e discussão na mesma sessão dos itens `006` e `007`: usuário reportou que corpos só reagem a atropelamento depois de terem sido atingidos por um tiro antes; investigação confirmou, via leitura direta do código, que o mecanismo de "dormir" do corpo é sono físico real (não kinematic) e que o callback de contato do personagem existe mas nunca aplica força — só tiro/granada acordam o corpo hoje. Ver `mods/VisceralCombat/memory/sessions.md`, entrada mais recente.
- Pendência `[P-10.2]` da memória do mod: histórico de bugs sutis no sistema de ragdoll ativo desta sessão — motivo pelo qual este item segue o fluxo formal de spec/review antes de codar, em vez de fix direto.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Item criado via `/add-backlog-item` |
| 2026-09-21 | Revisão `/review-spec` — 1 gap corrigido (decisão de configuração/toggle marcada `<!-- review: -->`) + 1 corner case adicionado (parte de corpo já desmembrada e largada separadamente) |
| 2026-09-21 | Decisões do usuário aplicadas — 3 pontos `<!-- review: -->` resolvidos: (1) só jogador humano dispara a reação, bots não; (2) itens físicos confirmados no escopo junto com corpos; (3) reaproveita "Player Body Collision" (corpos) e "Item Physics" (itens) já existentes, sem toggle novo. Critérios de aceite e "Fora de escopo" atualizados de acordo. |
