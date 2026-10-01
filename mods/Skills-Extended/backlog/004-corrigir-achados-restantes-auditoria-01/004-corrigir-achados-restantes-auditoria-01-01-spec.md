# 004 — Corrigir achados restantes da auditoria 01

**Mod:** Skills-Extended
**Status:** Backlog
**Criado:** 2026-09-07

## Visão geral

A auditoria técnica de código (`mods/Skills-Extended/docs/relatorio-auditoria-codigo-01.md`) encontrou 28 achados. Os 6 críticos e os 9 altos já foram tratados nos itens [001](../001-corrigir-bugs-criticos-auditoria-01/), [002](../002-corrigir-cap-medicamento-instancia/) e [003](../003-corrigir-achados-altos-auditoria-01/). Este item fecha a auditoria agrupando os 13 achados restantes: 6 de severidade Média (🟡), 5 de severidade Baixa (🔵) e 2 de Otimização (💡). Eles cobrem cinco categorias distintas: (a) um bug comportamental remanescente da mesma família sistêmica dos itens anteriores, sem dono real disponível de forma barata; (b) duas leituras de skill que escondem ou tratam de forma inconsistente a possibilidade de retornar nulo; (c) três pontos de alocação evitável em código que roda com frequência (por quadro ou por interação repetida); (d) cinco itens de higiene de recursos e código dormente (uma assinatura de patch desatualizada e desativada, um catch vazio sem log, um cliente HTTP sem descarte, uma tarefa em segundo plano sem limite de tempo, e uma referência não zerada ao desativar); e (e) duas otimizações triviais de reflection/reasserção redundante.

## Comportamento atual

- Ao golpear com arma branca, a velocidade do golpe de **qualquer** jogador ou bot reflete a skill de furtividade do jogador local, porque o objeto que anima a arma nas mãos não guarda referência a quem a está segurando — o mesmo padrão sistêmico já corrigido em outros pontos, mas sem um dono barato de resolver aqui.
- Uma propriedade interna do minigame de arrombamento esconde o fato de que a skill do jogador pode ser nula, fazendo o código de quem a usa presumir (incorretamente) que ela nunca falha. Separadamente, alguns comandos de depuração acessam o jogador e o mundo do jogo sem checar se estão disponíveis, de forma inconsistente com outros comandos no mesmo arquivo que já fazem essa checagem corretamente; um desses comandos também pode iterar sobre uma lista de armas que na verdade é nula.
- Três trechos de código alocam objetos desnecessariamente com frequência: uma função anônima é recriada a cada quadro enquanto o jogador está de bruços; uma cópia completa de um objeto de efeito é feita a cada atualização de texto de interface (tooltip); e os itens de menu de interação com portas trancadas são recriados do zero toda vez que o jogador olha para uma porta, mesmo sem nada ter mudado.
- Uma assinatura de correção para portas com fechadura de cartão está desatualizada em relação ao jogo atual e a funcionalidade está desativada (comentada) — inofensivo hoje, mas quebraria a inicialização do mod se alguém reativasse sem atualizar a assinatura primeiro. O minigame de arrombamento mantém uma referência a um retorno de chamada da última tentativa sem limpá-la ao ser desativado (pequeno, não cumulativo). O verificador de atualização do mod (lado servidor) cria um cliente de rede sem descartá-lo corretamente, tem um bloco de tratamento de erro completamente silencioso (sem nenhum registro), e dispara uma tarefa de rede em segundo plano sem limite de tempo — se a fonte externa nunca responder, a tarefa fica pendurada indefinidamente.
- Duas pequenas ineficiências de baixo risco: uma referência de reflection é resolvida repetidamente em vez de ser guardada uma única vez; e um conjunto de valores de cursor/entrada é reescrito a cada quadro enquanto uma tela do minigame está aberta, mesmo sem necessidade aparente de reescrevê-los tão frequentemente.

## Comportamento desejado

- O golpe de arma branca deve, na medida do possível, refletir a skill de quem realmente desferiu o golpe — e, onde isso não for tecnicamente viável a um custo razoável, o comportamento atual deve ficar documentado explicitamente no código como uma limitação conhecida, não como um descuido.
- Toda leitura de skill que pode retornar nulo deve deixar isso visível no tipo/assinatura, e todo código que a consome deve tratar esse caso de forma segura e consistente com o resto do mod.
- Os três pontos de alocação evitável devem parar de recriar objetos desnecessariamente quando o valor não mudou desde a última vez.
- A assinatura desatualizada da correção de porta com cartão deve ser corrigida antes de qualquer reativação futura, ou documentada como pendente de correção caso a funcionalidade continue desativada. O verificador de atualização deve descartar corretamente seus recursos de rede, registrar (mesmo que em nível de depuração) qualquer erro que hoje é silenciado, e não ficar pendurado indefinidamente esperando uma resposta externa. A referência de retorno de chamada do minigame de arrombamento deve ser limpa ao desativar.
- As duas otimizações triviais devem ser aplicadas sem alterar o comportamento observável.

## Critérios de aceite

- [ ] A velocidade de golpe de arma branca de um bot ou de outro jogador reflete a skill de furtividade do dono real do golpe — ou, se isso não for viável, o código documenta explicitamente essa limitação com a razão técnica, sem exceção sem tratamento (o valor "errado" nunca deve causar erro, só imprecisão conhecida).
- [ ] O tipo de retorno da skill usada pelo minigame de arrombamento deixa explícito que ela pode ser nula, e todo uso subsequente checa antes de acessar.
- [ ] Os comandos de depuração que acessam o jogador/mundo do jogo sem checagem passam a usar o mesmo padrão defensivo já usado corretamente por outro comando no mesmo arquivo; o comando que itera uma lista de armas não falha quando essa lista é nula.
- [ ] Ficar de bruços por um período prolongado não aloca uma nova função a cada quadro para a mesma finalidade.
- [ ] Abrir o tooltip de um item injetável repetidamente não faz uma cópia completa do objeto de efeito a cada abertura.
- [ ] Olhar repetidamente para a mesma porta trancada não recria os itens de menu de interação a cada olhar, apenas quando o estado da porta muda.
- [ ] A assinatura da correção de porta com cartão (hoje desativada) está corrigida para bater com o jogo atual, ou permanece desativada com uma nota clara de que precisa ser atualizada antes de qualquer reativação.
- [ ] O verificador de atualização do mod descarta corretamente o cliente de rede que cria, registra (ao menos em nível de depuração) qualquer erro no bloco hoje silencioso, e não fica pendurado indefinidamente se a fonte externa não responder.
- [ ] O minigame de arrombamento não mantém uma referência de retorno de chamada de uma tentativa anterior depois de ser desativado.
- [ ] A referência de reflection resolvida repetidamente passa a ser resolvida uma única vez e reaproveitada.
- [ ] Os valores de cursor/entrada do minigame de arrombamento não são reescritos a cada quadro sem necessidade — a menos que se confirme em teste que algum outro sistema do jogo os reimpõe por conta própria, caso em que a reasserção contínua deve ser mantida e documentada como intencional.
- [ ] **Fika/multiplayer:** o critério de velocidade de golpe é o único observável apenas em sessão cooperativa (com bots ou outros jogadores presentes). Os demais critérios (alocações, higiene de recursos, otimizações, nullability) são comportamento interno do cliente/servidor, observável tanto solo quanto em coop, sem diferença entre os dois modos.
- [ ] **Estado entre raids:** a referência de retorno de chamada do minigame de arrombamento (achado de higiene) deve ser limpa ao desativar o minigame, não só ao reativá-lo na tentativa seguinte — evita reter o contexto de uma raid anterior por mais tempo que o necessário caso a raid termine com o minigame aberto.

## Corner cases

- [ ] O jogador ataca com arma branca no exato momento em que a skill do jogador local ainda não está disponível (transição de tela, raid recém-iniciada) — não deve gerar erro não tratado, independente da solução escolhida para o dono do golpe.
- [ ] O minigame de arrombamento é aberto e fechado repetidamente em sucessão rápida — a limpeza da referência de retorno de chamada ao desativar não deve quebrar uma tentativa legítima em andamento se o jogador reabrir o minigame rapidamente.
- [ ] A raid termina abruptamente (extração, morte, alt-F4) com o minigame de arrombamento aberto — a referência de retorno de chamada da raid anterior não deve vazar para a próxima sessão.
- [ ] O verificador de atualização roda em uma sessão de servidor sem acesso à internet (ambiente de desenvolvimento local, firewall) — o limite de tempo adicionado não deve travar o boot do servidor SPT nem gerar uma exceção não tratada.
- [ ] Os comandos de depuração corrigidos (`ConsoleCommands`) são executados no menu principal, antes de qualquer raid ter começado — devem falhar de forma segura (mensagem, não exceção), mesmo sem jogador/mundo disponível.
- [ ] Interação com o achado do minigame de arrombamento já corrigido no item 003 (`AUD-01-15`, limpeza de tentativas de fechadura no headless) — a correção deste item (limpeza da referência de callback) não deve duplicar ou conflitar com aquela limpeza já existente.
- [ ] O jogador é removido do mundo (morte, desconexão) enquanto o minigame de arrombamento está ativo, entre a abertura do minigame e o próximo uso da skill — a correção da nullability escondida (achado da property que esconde o `null`) deve cobrir exatamente esse intervalo, não só o caso de abertura inicial.

## Fora de escopo

- [ ] A definir

## Referências

- [Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)](../../docs/relatorio-auditoria-codigo-01.md) — achados `AUD-01-16` a `AUD-01-28`.
- [001-corrigir-bugs-criticos-auditoria-01](../001-corrigir-bugs-criticos-auditoria-01/), [002-corrigir-cap-medicamento-instancia](../002-corrigir-cap-medicamento-instancia/) e [003-corrigir-achados-altos-auditoria-01](../003-corrigir-achados-altos-auditoria-01/) — itens irmãos que trataram os 15 achados críticos e altos da mesma auditoria; este item fecha os 28 achados totais.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Item criado via `/add-backlog-item` |
| 2026-09-07 | Revisão `/review-spec` — 1 corner case adicionado (jogador removido do mundo entre a abertura do minigame de arrombamento e o próximo uso da skill, mencionado explicitamente no relatório de auditoria original mas ausente da primeira versão desta spec) |
