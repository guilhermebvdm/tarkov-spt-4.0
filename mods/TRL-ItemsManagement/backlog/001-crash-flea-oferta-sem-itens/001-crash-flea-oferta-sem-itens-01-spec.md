# 001 — Crash flea oferta sem itens

**Mod:** TRL-ItemsManagement
**Status:** Backlog
**Criado:** 2026-09-23

## Visão geral

A busca na flea market quebra com uma falha genérica para o jogador ("erro 0") sempre que existe, entre as ofertas ativas do servidor, pelo menos uma oferta cuja lista de itens ficou vazia. O servidor registra `Object reference not set to an instance of an object` no log ao processar a requisição de busca. O problema já foi confirmado em produção (servidor com ~65 jogadores/mods) através de logs reais e persiste mesmo após reiniciar o `SPT.Server`.

## Comportamento atual

- Buscar na flea (endpoint de busca, qualquer categoria/filtro) falha de forma intermitente: alguns jogadores nunca veem o erro, outros sempre veem, outros alternam — dependendo de qual categoria/filtro de vendedor (todos / jogadores / traders) a busca deles inclui.
- O erro só acontece quando a busca processa uma oferta específica cuja lista de itens está vazia. Buscas que não incluem essa oferta (por filtro de categoria ou tipo de vendedor) funcionam normalmente.
- Reiniciar o servidor **não resolve** — a lista de ofertas ativas da flea vive inteiramente em memória e é reconstruída do zero a cada boot, então o problema reaparecer após um restart indica que a oferta nasce **válida** (com itens) e perde os itens depois, durante a operação normal do servidor, e não que ela já nasce quebrada.
- A causa exata de o que esvazia os itens de uma oferta já criada ainda não foi identificada. Não é a geração de ofertas de trader/preset quebrado (isso derrubaria o servidor num ponto diferente, na criação da oferta, não na busca). O suspeito mais provável é um dos muitos outros mods do servidor que manipula listas de itens de inventário/economia diretamente.

## Comportamento desejado

- A busca na flea nunca deve falhar por causa de uma oferta com itens vazios/nulos — o servidor descarta essa oferta específica ao montar os resultados da busca, e continua retornando normalmente as demais ofertas válidas.
- Toda vez que uma oferta assim é descartada, o servidor registra uma linha de log identificando a oferta (id, tipo de vendedor — trader / jogador / npc-fake —, apelido/id do vendedor quando disponível, horário de expiração da oferta), para permitir investigar depois qual fluxo a deixou sem itens.
- O registro de log não deve se repetir a cada busca para a mesma oferta já conhecida como quebrada (evitar inundar o log enquanto a oferta continuar presa no servidor).

## Critérios de aceite

- [ ] Buscar na flea, em qualquer categoria e qualquer filtro de tipo de vendedor, nunca retorna erro genérico ao jogador, mesmo havendo uma oferta ativa sem itens no servidor.
- [ ] Ao encontrar uma oferta sem itens durante uma busca, o servidor a exclui do resultado, mas continua retornando normalmente as demais ofertas da mesma busca.
- [ ] Na primeira vez que uma oferta sem itens é descartada, o servidor grava uma linha de log com o id da oferta, tipo de vendedor e horário de expiração; buscas repetidas que encontrem a mesma oferta não geram log duplicado.
- [ ] Comportamento normal da flea (buscar, comprar, listar oferta de jogador, oferta de trader) permanece byte-a-byte idêntico quando não há nenhuma oferta quebrada — a checagem faz uma única passada sobre a lista de ofertas já carregada, sem consulta adicional a disco/rede nem repetição de trabalho já feito pelo servidor.
- [ ] **Fika/multiplayer:** o filtro vale igualmente para qualquer jogador conectado à sessão coop buscando na flea — nenhum jogador do grupo consegue disparar o erro, independente de quem entrou primeiro ou de qual raid está em andamento (a flea só é acessível fora de raid, mas o processo do servidor é compartilhado por todos).
- [ ] **Estado entre raids:** N/A — a lista de ofertas da flea vive inteiramente em memória do processo do servidor; não é afetada por entrar/sair de raid, morte do personagem ou alt-F4.

## Corner cases

- [ ] Todas as ofertas ativas no momento da busca estarem quebradas (lista final fica vazia após o filtro) — a busca deve retornar "nenhum resultado" normalmente, sem erro, em vez de falhar em outro ponto do cálculo de categorias/preços.
- [ ] Uma oferta ter a lista de itens `null` (não apenas vazia) — o filtro trata os dois casos (nulo e lista vazia) da mesma forma.
- [ ] A mesma oferta quebrada é encontrada em dezenas de buscas sucessivas de vários jogadores ao longo de horas (ela não expira nem é removida automaticamente do servidor) — o log não deve crescer sem limite; ver critério de log único por oferta acima.
- [ ] A oferta quebrada pertence a um jogador (não trader/npc) — o filtro remove ela da busca de qualquer comprador igual às outras, mas isso deixa o jogador dono sem conseguir ver/cancelar a própria oferta pela aba "Minhas ofertas". Este item resolve apenas o crash da busca; a experiência do dono de uma oferta quebrada fica registrada como limitação conhecida, não como algo a corrigir aqui.
- [ ] Dois ou mais jogadores buscando ao mesmo tempo encontram a mesma oferta quebrada simultaneamente (servidor com múltiplos jogadores concorrentes, caso normal num servidor Fika coop populoso) — nem a filtragem nem o mecanismo de "logar só uma vez por oferta" podem falhar, travar ou lançar exceção sob acesso concorrente; na pior hipótese é aceitável logar a mesma oferta 2x (uma race benigna), nunca quebrar a busca de nenhum dos dois jogadores.
- [ ] A solução não pode presumir a ausência nem a presença de outros mods que também leem ou modificam o comportamento da flea no servidor (ex: mods que sobrescrevem a atualização/expiração de ofertas) — deve funcionar de forma independente e não exigir nenhuma alteração nesses outros mods.
- [ ] <!-- review: decisão humana — o registro de "já logado" para uma oferta quebrada deve persistir só em memória (reseta a cada restart do servidor, logando de novo a mesma oferta uma vez por boot) ou deve ser permanente entre restarts? Assumindo memória (mais simples) até confirmação em contrário. -->

## Fora de escopo

- [ ] A definir — a causa raiz de o que esvazia os itens de uma oferta já criada não está confirmada. Corrigir essa causa raiz é um item separado, a ser aberto depois que o log de diagnóstico desta entrega apontar a origem exata.

## Referências

- Investigação da sessão atual: log de produção (`Object reference not set to an instance of an object` em processamento de busca da flea) + inspeção de 71 perfis de jogador (nenhum com oferta própria sem itens) + leitura do código-fonte oficial do SPT (vendorizado em `references/spt-source/`), onde a criação de oferta já falharia num ponto anterior se a lista de itens nascesse vazia — o que descarta bug de geração/preset e aponta para perda de itens pós-criação.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Item criado via `/add-backlog-item` |
| 2026-09-23 | Revisão `/review-spec` — 1 critério reescrito para verificabilidade + 3 corner cases adicionados (1 marcado para decisão humana) |
