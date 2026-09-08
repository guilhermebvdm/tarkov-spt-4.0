# 001 — Corrigir bugs críticos da auditoria 01

**Mod:** Skills-Extended
**Status:** Backlog
**Criado:** 2026-09-07

## Visão geral

A auditoria técnica de código (`mods/Skills-Extended/docs/relatorio-auditoria-codigo-01.md`) encontrou 28 achados, dos quais 6 são classificados como críticos. Este item agrupa 5 desses 6 achados: dois compartilham a mesma causa raiz (mecânicas de skill aplicadas ao jogador local em vez do verdadeiro dono da ação/entidade), um é uma checagem de segurança ausente num processo de vários quadros, e dois são condições de corrida no lado do servidor que afetam sessões cooperativas com múltiplos jogadores. O 6º achado crítico (limite de eficácia de medicamento, `AUD-01-03`) foi desmembrado para o item [002](../002-corrigir-cap-medicamento-instancia/002-corrigir-cap-medicamento-instancia-01-spec.md), por exigir uma mudança arquitetural mais trabalhosa (mover o ponto de patch para um método de instância em vez do utilitário estático atual).

## Comportamento atual

- O som de uma porta sendo aberta é sempre ajustado pela skill de furtividade (Silent Ops) do jogador local, independentemente de quem realmente abriu a porta — incluindo bots e outros jogadores numa sessão cooperativa.
- A velocidade de movimento em terreno difícil (pântano/vegetação densa) de **qualquer** entidade (bot, outro jogador) é calculada usando a skill de força (Strength) do jogador local, não a da entidade que está se movendo. Em alguns casos essa checagem pode falhar de forma não tratada durante a inicialização de bots numa raid.
- Um processo de ajuste de estatísticas de armas, que roda em segundo plano ao longo de vários quadros após o jogador trocar de tela no menu, verifica a existência da skill do jogador apenas uma vez no início do processo — se o jogador sair da raid (extração, morte, desconexão) enquanto o processo ainda está em andamento, ele pode falhar de forma não tratada.
- Duas ações que envolvem o servidor — sacrificar itens no altar cultista e gerar um personagem "scav" para o jogador — usam uma variável temporária que é compartilhada entre **todas** as requisições em andamento no processo do servidor, não isolada por jogador/sessão. Em uma sessão cooperativa (Fika) com mais de um jogador, se duas dessas ações forem disparadas por jogadores diferentes em um intervalo de tempo muito curto, o resultado calculado para um jogador pode ser aplicado ao outro.

## Comportamento desejado

- Cada mecânica de skill deve identificar corretamente qual entidade (jogador local, bot, ou outro jogador em coop) é a dona real da ação sendo processada, e aplicar o bônus/penalidade daquela entidade específica — nunca presumir que é sempre o jogador local.
- O processo de ajuste de armas em segundo plano deve verificar a validade do contexto (jogador ainda em raid) a cada etapa, não só no início, e encerrar de forma limpa e silenciosa se o contexto deixar de ser válido no meio do processo.
- As duas ações do servidor devem processar corretamente o resultado de cada requisição de forma isolada, mesmo quando duas requisições de jogadores diferentes acontecem simultaneamente na mesma sessão cooperativa — sem que uma "vaze" ou sobrescreva o resultado da outra.

## Critérios de aceite

- [ ] O som de uma porta aberta por um bot ou por outro jogador (não o jogador local) não reflete a skill de furtividade do jogador local.
- [ ] A velocidade de movimento em terreno difícil de um bot ou de outro jogador reflete a skill de força da própria entidade em movimento, não a do jogador local.
- [ ] Uma raid com bots inicializa normalmente sem interrupção relacionada ao cálculo de velocidade de movimento em terreno difícil.
- [ ] O processo de ajuste de armas em segundo plano encerra sem erro caso o jogador saia da raid (por qualquer meio) enquanto o processo ainda está em andamento.
- [ ] Duas requisições simultâneas de sacrifício no altar cultista, feitas por jogadores diferentes na mesma sessão cooperativa, aplicam o desconto de tempo correto a cada jogador respectivamente, sem trocar os valores entre si.
- [ ] Duas requisições simultâneas de geração de personagem scav, feitas por jogadores diferentes na mesma sessão cooperativa, produzem cada scav com a aparência e vida correspondentes ao resultado sorteado para aquele jogador específico, sem misturar com o resultado de outro jogador.
- [ ] **Fika/multiplayer:** todos os critérios acima descrevem especificamente comportamento observável apenas em sessões com mais de uma entidade presente (bots ou outros jogadores) — é o cenário que este item existe para corrigir. Em uma sessão solo sem bots próximos, nenhum dos 5 problemas é observável (mas os dois bugs de servidor continuam existindo estruturalmente, mesmo que sem outro jogador para colidir).
- [ ] **Estado entre raids:** o processo de ajuste de armas deve encerrar corretamente ao sair da raid por qualquer meio (extração, morte, desaparecimento, fechar o jogo) sem deixar processamento pendurado que afete a próxima raid.

## Corner cases

- [ ] O que acontece quando a skill do jogador local ainda não está disponível (transição de tela, raid recém-iniciada) no exato momento em que um bot abre uma porta ou entra em terreno difícil?
- [ ] Três ou mais jogadores disparam a ação de sacrifício cultista ou de geração de scav dentro do mesmo intervalo curto de tempo na mesma sessão cooperativa (não só dois).
- [ ] O jogador local também está realizando a mesma ação (ex: também está de bruços em terreno difícil, ou também abrindo uma porta) no mesmo instante em que a correção está sendo aplicada a outra entidade — a correção não deve fazer a skill do jogador local parar de funcionar para ele mesmo.
- [ ] Interação com outros mods que rodam no mesmo contexto de raid (ex: mods de recarga de munição já corrigidos nesta mesma sessão de trabalho) — nenhuma correção aqui deve alterar o comportamento desses mods, já que apenas compartilham o mesmo ciclo de vida de raid, sem dependência direta.
- [ ] Nenhuma das correções gera erro quando executada num servidor dedicado (headless) sem jogador local nenhum — nesse contexto, o comportamento correto é simplesmente não aplicar bônus algum (não há jogador local cuja skill deva ser refletida), não travar ou lançar exceção. **(Confirmado como corner case obrigatório — este time roda mods dependentes de "jogador local" em headless, e já identificamos nesta mesma sessão que o headless deste repo não tem "MainPlayer" da forma que o restante do código costuma assumir.)**

## Fora de escopo

- [x] Os demais 22 achados do relatório de auditoria 01 (severidade Alta, Média, Baixa e Otimização) — ficam registrados como dívida técnica conhecida para uma rodada futura, não fazem parte deste item.
- [x] Os outros dois problemas do mesmo processo de ajuste de armas (achados `AUD-01-08` e `AUD-01-09` do relatório) — mutação de valores compartilhados entre itens do mesmo tipo, e disparo desnecessário fora de raid — ficam fora deste item mesmo estando no mesmo trecho de código do achado `AUD-01-04` (que É crítico e está dentro do escopo). Quem for implementar não deve expandir o escopo pra "consertar o arquivo inteiro" sem que isso seja decidido explicitamente.
- [x] O achado de "medicamento" (`AUD-01-03`) — decisão confirmada pelo usuário: desmembrado para o item [002-corrigir-cap-medicamento-instancia](../002-corrigir-cap-medicamento-instancia/), por exigir mudar o ponto de patch para um método de instância em vez do utilitário estático atual (mudança arquitetural maior que os demais achados deste item).

## Referências

- [Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)](../../docs/relatorio-auditoria-codigo-01.md) — achados `AUD-01-01`, `AUD-01-02`, `AUD-01-04`, `AUD-01-05`, `AUD-01-06`.
- [002-corrigir-cap-medicamento-instancia](../002-corrigir-cap-medicamento-instancia/) — item irmão com o 6º achado crítico (`AUD-01-03`), desmembrado desta spec.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Item criado via `/add-backlog-item` |
| 2026-09-07 | Revisão `/review-spec` — 1 gap de escopo corrigido (ambiguidade sobre os outros 2 achados no mesmo arquivo do `AUD-01-04`) + 1 corner case adicionado (comportamento em servidor headless) + 2 trechos marcados com `<!-- review: -->` para decisão humana |
| 2026-09-07 | Decisão do usuário: achado do "medicamento" (`AUD-01-03`) desmembrado para o item 002; corner case de headless confirmado como obrigatório |
