# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça (+ Auditoria Fika)

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Hoje, ao morrer, um bot ou jogador mantém a arma equipada nas mãos presa ao corpo/inventário do cadáver — ela não cai automaticamente no chão como item solto. Da mesma forma, o desmembramento real da cabeça (evento que efetivamente remove a cabeça do corpo, distinto de simplesmente levar um tiro na área da cabeça) não força a queda do capacete nem dos óculos. Este item adiciona os dois comportamentos de drop e audita se ambos — e o próprio desmembramento de cabeça — replicam corretamente em raids coop via FIKA.

## Comportamento atual

- **Arma na morte:** ao morrer (bot ou jogador), a arma que estava em mãos permanece anexada ao inventário do cadáver; não há nenhum drop automático dela no chão. O mod já tem um padrão equivalente para outro item (capacete solto por chance em qualquer hit na cabeça), mas nada equivalente existe para a arma na morte.
- **Capacete em hit de cabeça (mecanismo já existente, e que continua intocado):** qualquer impacto na área da cabeça de um **bot** tem uma chance configurável (padrão 15%, ajustável no F12) de derrubar o capacete equipado. Esse gatilho dispara em **qualquer hit na cabeça**, morte ou não, e nunca dispara para jogadores humanos.
- **Óculos:** não existe hoje nenhum mecanismo de drop de óculos, nem por hit nem por desmembramento.
- **Desmembramento real de cabeça:** existe um evento distinto — o desmembramento efetivo da cabeça (remoção física do modelo, ativado tanto em jogadores/bots recém-mortos por tiro direto na cabeça quanto em cadáveres já mortos atingidos posteriormente na cabeça) — que hoje não aciona queda de nenhum equipamento; ele só troca a malha visual da cabeça por um efeito de decepamento.
- **FIKA:** o mod já é ciente de coop (handshake de recurso compartilhado, pacotes de rede próprios para sincronizar desmembramento e ragdoll entre host e clientes), mas o comportamento de queda de itens em si (arma, capacete, óculos) nunca foi auditado quanto à replicação correta entre os clientes de um raid coop.

## Comportamento desejado

1. **Drop de arma na morte:** no momento em que um bot ou jogador morre, a arma que estava equipada em mãos deve cair como item solto no chão/mundo (mesmo padrão de "soltar item" já usado no mod para o capacete). A faca (arma branca) é uma exceção explícita e nunca cai — o cadáver permanece com ela.
2. **Drop de capacete + óculos no desmembramento real de cabeça:** quando o evento de desmembramento efetivo da cabeça ocorrer (não um hit qualquer — o evento de remoção da cabeça em si), tanto o capacete quanto os óculos equipados devem cair no chão com **100% de chance**, tanto para bots quanto para jogadores humanos. Esse gatilho é **adicional** e **coexiste** com o mecanismo de chance configurável já existente (hit de cabeça sem desmembrar) — implementar sem alterar o comportamento, a chance ou o escopo (somente bots) desse mecanismo existente.
3. **Sincronismo FIKA:** em um raid coop, quando a arma cai na morte ou o capacete/óculos caem no desmembramento de cabeça, todos os clientes conectados devem ver o mesmo resultado — o item aparece no chão (ou ausente do cadáver) de forma consistente para quem observa de outra máquina, sem duplicar o item nem dessincronizar o inventário do cadáver entre host e clientes.
4. **Sem gate de paridade Fika:** decidido que os dois drops novos funcionam **incondicionalmente**, sem checar `AllPlayersHaveVisceralCombat` — mesmo precedente do desmembramento de cabeça pós-morte já existente e do `ShootOffHelmetPatch`, nenhum dos quais checa esse gate hoje (só a feature de desmembramento de perna em bots vivos, item 001, o faz). Num raid coop misto (nem todos com o mod), o drop ainda ocorre normalmente.

## Critérios de aceite

- [ ] Ao matar um bot com a arma equipada em mãos (não-faca), a arma aparece como item solto no mundo após a morte, e o slot correspondente no cadáver fica vazio.
- [ ] Ao matar um bot com uma faca equipada em mãos, a faca **não** cai — permanece no cadáver.
- [ ] O mesmo comportamento de drop de arma (exceto faca) se aplica à morte do jogador humano (não só bots).
- [ ] Quando o desmembramento real de cabeça ocorre (em qualquer dos gatilhos existentes que levam a essa remoção), o capacete equipado cai no chão com 100% de chance — tanto em bot quanto em jogador.
- [ ] Quando o desmembramento real de cabeça ocorre, os óculos equipados caem no chão com 100% de chance — tanto em bot quanto em jogador.
- [ ] O mecanismo de chance configurável existente (capacete caindo por hit simples na cabeça, sem desmembrar, só em bots) continua funcionando sem alteração de comportamento, chance ou escopo.
- [ ] **Fika/multiplayer:** em raid coop, o drop da arma (morte) e o drop de capacete/óculos (desmembramento de cabeça) aparecem de forma consistente para todos os clientes — quem observa de outra máquina vê o mesmo item no chão (ou a mesma ausência no cadáver) que o host, sem duplicação e sem itens "fantasmas" que só existem localmente em um cliente.
- [ ] **Estado entre raids:** os drops (arma, capacete, óculos) são efeitos pontuais do momento da morte/desmembramento dentro da raid atual — não persistem nem afetam o estado de nenhum sistema entre uma raid e a próxima (sem flags, contadores ou itens retidos após sair do raid).

## Corner cases

- [ ] Bot/jogador morre sem nenhuma arma equipada em mãos (mãos vazias, ex.: acabou de trocar de arma e ainda não terminou a animação, ou está desarmado) — não deve haver erro nem tentativa de dropar item nulo.
- [ ] Bot/jogador morre com a faca ativamente em mãos (não uma arma de fogo) — a faca não cai, mas se o bot também tiver uma arma de fogo guardada em outro slot, essa arma guardada não é afetada (só a peça em mãos no momento da morte é avaliada).
- [ ] Cabeça é desmembrada em um bot/jogador que já está sem capacete e/ou sem óculos equipados (morreu com a cabeça descoberta) — não deve haver erro; simplesmente não há o que cair.
- [ ] Cabeça é desmembrada em um cadáver que já está morto há algum tempo (desmembramento pós-morte, não no instante da morte) — capacete/óculos ainda devem cair a 100%, mesmo fora do instante exato do óbito.
- [ ] Explosão ou outro dano em massa desmembra a cabeça junto de outros membros no mesmo evento — o drop de capacete/óculos deve disparar exatamente uma vez para aquela cabeça, sem duplicar itens.
- [ ] Interação com o mecanismo de chance já existente: um hit de cabeça primeiro aciona a chance configurável (capacete cai por sorte) e, em seguida, um hit posterior desmembra a cabeça — o sistema não deve tentar dropar o capacete uma segunda vez (item já não está mais equipado).
- [ ] Em raid coop, o bot/jogador morre/desmembra em uma máquina que não é a que está processando fisicamente aquele corpo (observado remotamente) — o drop não deve ser disparado de forma duplicada nem ausente para o observador remoto.
- [ ] Bot é reciclado/despawnado (fora de alcance, fim de raid) logo após a morte, antes do drop terminar de processar — não deve haver exceção nem item órfão.
- [ ] Capacete ou arma equipados possuem acessórios anexados (ex.: viseira/NVG mount no capacete; mira, coldre de carregador, silenciador na arma) — o item cai por inteiro com seus acessórios, sem desmontar a árvore nem perder anexos pelo caminho.
- [ ] Nem todos os jogadores humanos do raid coop têm o mod instalado (`AllPlayersHaveVisceralCombat == false`) — o drop de arma/capacete/óculos ocorre normalmente mesmo assim (decisão: sem gate de paridade — ver "Comportamento desejado" item 4); apenas a sincronização de rede depende de o observador remoto também ter o mod instalado para processar o pacote correspondente.

## Fora de escopo

- [ ] A definir

## Referências

- `ShootOffHelmetPatch.cs` (mecanismo de chance configurável já existente para capacete em hit de cabeça — não alterar)
- `CreateCorpsePatch.cs`, `CreateBSGRagdollPatch.cs` (candidatos ao hook de morte)
- `LivingDismembermentController.cs`, `Utils.cs`, `GoreObjectPool.cs`, `BleedPatch.cs` (candidatos ao hook de desmembramento de cabeça)
- `docs/technical/fika-packet-desync-prevention-plan.md` (regras de pacote/replicação a seguir na auditoria FIKA)
- Item de backlog anterior: `001-alive-leg-dismemberment` (padrão de handshake FIKA e gate `AllPlayersHaveVisceralCombat`)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` |
| 2026-09-09 | Revisão `/review-spec` — 1 gap (decisão de gate FIKA) + 2 corner cases corrigidos |
| 2026-09-09 | Decisão do usuário: drops sem gate de paridade Fika (incondicional) — marcadores `<!-- review -->` resolvidos |
