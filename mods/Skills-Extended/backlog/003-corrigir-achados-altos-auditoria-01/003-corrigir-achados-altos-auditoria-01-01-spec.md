# 003 — Corrigir achados altos da auditoria 01

**Mod:** Skills-Extended
**Status:** Backlog
**Criado:** 2026-09-07

## Visão geral

A auditoria técnica de código (`mods/Skills-Extended/docs/relatorio-auditoria-codigo-01.md`) encontrou 28 achados, dos quais 9 são classificados como severidade Alta (🟠). Este item agrupa esses 9 achados, que cobrem quatro problemas distintos: (a) duas mecânicas de skill que alteram permanentemente um valor compartilhado por todos os itens/armas do mesmo tipo, em vez de ajustar apenas a instância específica em uso; (b) um processo de reprocessamento que dispara em qualquer navegação de menu, mesmo fora de raid, sem necessidade; (c) quatro pontos onde uma skill é resolvida a partir do jogador local (ou de forma insegura) em contextos onde isso é ambíguo, desnecessário ou arriscado; e (d) dois problemas de limpeza de estado — um eventos de skill que nunca são desinscritos ao sair de raid, outro um registro de tentativas de abrir fechaduras que nunca é limpo em sessões de servidor dedicado (headless) de longa duração — mais um bug de "copy-paste" isolado numa lista de referências de reflection que hoje não é usada por nenhum código.

## Comportamento atual

- Quando um item médico ou uma arma tem seu custo/estatística ajustado pela skill do jogador local, o ajuste é escrito diretamente no molde (template) compartilhado por **todos os itens daquele mesmo tipo** no jogo, em vez de ser aplicado apenas ao item específico em uso. Em uma sessão cooperativa, isso pode fazer com que o ajuste calculado a partir da skill do jogador local "vaze" visualmente para itens do mesmo tipo pertencentes a outros jogadores, no cliente de quem tem a skill.
- O processo de reprocessamento de estatísticas de armas é disparado toda vez que o jogador troca de tela em qualquer menu (baú, hideout, comerciante) — inclusive fora de uma partida — e reinicia do zero a cada disparo, mesmo que o nível da skill não tenha mudado desde a última vez. Isso desperdiça processamento e, em navegação rápida entre telas, pode iniciar múltiplos processamentos concorrentes sobre a mesma estrutura de dados compartilhada.
- Em quatro pontos distintos do mod, uma skill é lida a partir do jogador local sem verificar se essa leitura é válida no contexto atual: (1) o nível de skill mostrado em uma tela de resumo pode não ser o do jogador realmente associado àquela tela; (2) o cálculo de preço/quantidade de troca com um comerciante pode falhar sem tratamento se a skill não estiver disponível no momento em que a tela abre; (3) um manipulador de evento de experiência médica refaz uma busca pelo jogador local mesmo já tendo uma referência válida disponível; (4) uma lista de referências de reflection contém sete campos que não são usados por nenhum código do mod, dois dos quais apontam, por erro de cópia, para o tipo errado.
- Um conjunto de assinaturas a eventos de skill feitas no início de cada partida nunca é desfeito ao sair da partida, mantendo uma referência ao jogador da partida anterior até a assinatura ser sobrescrita na partida seguinte. Separadamente, um registro de tentativas de abrir fechaduras por mapa nunca é limpo em sessões de servidor dedicado (headless), acumulando entradas de mapas diferentes ao longo de uma sessão longa.

## Comportamento desejado

- Ajustes de skill em itens médicos e armas devem ser aplicados de forma isolada à instância específica em uso, nunca escritos permanentemente no molde compartilhado por todos os itens daquele tipo.
- O reprocessamento de estatísticas de armas só deve rodar quando necessário: dentro de uma partida, e apenas quando o nível de skill relevante realmente mudou desde o último processamento — não a cada troca de tela de menu.
- Toda leitura de skill deve identificar corretamente a quem ela pertence (o jogador associado ao contexto específico, não presumir sempre o jogador local) e deve lidar de forma segura com os casos em que a skill não está disponível no momento da leitura, sem falhar de forma não tratada. A lista de referências de reflection não utilizadas deve ser corrigida ou removida, para que não vire uma armadilha caso um código futuro passe a usá-la.
- Assinaturas a eventos de skill feitas no início de uma partida devem ser desfeitas ao final dela. O registro de tentativas de fechaduras deve ser limpo a cada início de partida, inclusive em sessões headless.

## Critérios de aceite

- [ ] O custo de uso de um item médico ajustado pela skill de primeiros socorros do jogador local não altera o comportamento do mesmo tipo de item quando usado por outra entidade (bot ou outro jogador) na mesma sessão.
- [ ] As estatísticas de ergonomia/recuo de uma arma ajustadas pela skill de armas do jogador local não alteram o comportamento da mesma arma (mesmo tipo/modelo) quando usada por outra entidade na mesma sessão.
- [ ] Navegar rapidamente entre 10 telas de menu no hideout, fora de uma partida, não dispara o processo de reprocessamento de estatísticas de armas nenhuma vez.
- [ ] Dentro de uma partida, o processo de reprocessamento de estatísticas de armas só reprocessa quando o nível da skill de armas relevante muda — não a cada troca de tela.
- [ ] Abrir a tela de comerciante/pulga em um estado de transição (ex: logo após carregar o hideout) não trava ou gera erro não tratado relacionado ao cálculo de preço/quantidade de troca.
- [ ] O nível de skill exibido em uma tela de resumo reflete o dono real associado àquela tela, não presume sempre o jogador local.
- [ ] A lista de referências de reflection não utilizadas está corrigida (referências certas) ou removida, sem nenhum campo restante apontando para o tipo errado.
- [ ] Uma partida encerrada por qualquer meio desfaz as assinaturas de eventos de skill feitas no início dela, sem depender apenas da próxima partida para sobrescrevê-las.
- [ ] Em uma sessão de servidor dedicado (headless) de longa duração, com múltiplas partidas em mapas diferentes, o registro de tentativas de fechaduras não acumula indefinidamente entradas de mapas anteriores.
- [ ] **Fika/multiplayer:** os dois critérios de mutação de molde compartilhado (item médico e arma) descrevem comportamento observável apenas quando há mais de uma entidade usando o mesmo tipo de item/arma na mesma sessão cooperativa — é o cenário que motiva a correção. Os critérios de limpeza de estado (assinatura de eventos, registro de fechaduras) aplicam-se tanto a sessões solo quanto cooperativas, e o critério de registro de fechaduras headless é específico de sessões dedicadas Fika.
- [ ] **Estado entre raids:** as assinaturas de eventos de skill (achado de vazamento de evento) devem ser desfeitas ao sair de uma partida por qualquer meio (extração, morte, desaparecimento, fechar o jogo) antes que a próxima partida comece. O registro de tentativas de fechaduras deve ser limpo no início de cada partida nova, incluindo em sessões headless.

## Corner cases

- [ ] O que acontece quando dois jogadores em uma sessão cooperativa usam simultaneamente itens médicos ou armas do mesmo tipo, um com a skill correspondente treinada e outro sem? A correção não deve fazer com que um "contamine" o resultado do outro no molde compartilhado, nem em nenhuma ordem de execução.
- [ ] O jogador troca de tela de menu dentro de uma partida (não fora dela) — o gating por partida não deve impedir o reprocessamento legítimo de estatísticas de armas quando isso é esperado (ex: nível de skill mudou durante a partida).
- [ ] A tela de comerciante/pulga é aberta e fechada repetidamente em sucessão rápida — a correção do cálculo de preço/quantidade não deve introduzir um estado inconsistente entre aberturas.
- [ ] Uma partida termina de forma anômala (alt-F4, crash do jogo, desconexão de rede em coop) em vez de extração/morte normal — a limpeza das assinaturas de eventos de skill e do registro de fechaduras deve, na medida do possível dentro do ciclo de vida disponível do mod, não depender exclusivamente de um caminho de encerramento "feliz".
- [ ] Uma sessão headless processa múltiplas partidas consecutivas em mapas diferentes sem nunca reiniciar o processo — o registro de tentativas de fechaduras precisa ser limpo a cada início de partida, não só uma vez no boot do servidor.
- [ ] Remover ou corrigir os campos de reflection não utilizados não deve quebrar nenhum outro código do mod que dependa (mesmo que indiretamente, via reflection dinâmica) desses campos — confirmar ausência de uso antes de remover.
- [ ] O jogo é fechado e reaberto (ou o servidor headless reinicia) entre uma sessão e outra — qualquer mecanismo de "só reprocessar quando o nível mudou" baseado em cache em memória não deve, por partir de um estado vazio, deixar de processar armas que nunca foram vistas nesta execução do processo (o cache vazio não pode ser confundido com "nada mudou").
- [ ] O jogador troca de arma (equipa uma arma de um tipo ainda não processado nesta sessão) entre uma partida e a seguinte, sem que o nível da skill relevante tenha mudado — o mecanismo de gating não deve impedir o primeiro processamento dessa arma nova só porque o nível de skill está inalterado.
- [ ] O manipulador de evento de experiência médica (achado do "campo já validado ignorado") dispara numa janela de transição de cena (fim de partida, troca de perfil) — a correção não deve introduzir uma falha não tratada nesse instante, já que é justamente o cenário que motivou a leitura redundante e insegura original.

## Fora de escopo

- [x] Os 6 achados críticos (🔴) da auditoria 01 — já tratados nos itens [001](../001-corrigir-bugs-criticos-auditoria-01/) e [002](../002-corrigir-cap-medicamento-instancia/).
- [x] Os 13 achados restantes de severidade Média (🟡), Baixa (🔵) e Otimização (💡) da auditoria 01 — ficam registrados como dívida técnica conhecida para uma rodada futura, não fazem parte deste item.
- [x] O achado `AUD-01-16` (bug comportamental remanescente em `MeleeSpeedPatch`, severidade Média) — mesma família de causa raiz dos achados deste item, mas com severidade Média; fica fora, junto com o restante dos achados Médios/Baixos/Otimização.

## Referências

- [Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)](../../docs/relatorio-auditoria-codigo-01.md) — achados `AUD-01-07` a `AUD-01-15`.
- [001-corrigir-bugs-criticos-auditoria-01](../001-corrigir-bugs-criticos-auditoria-01/) e [002-corrigir-cap-medicamento-instancia](../002-corrigir-cap-medicamento-instancia/) — itens irmãos que trataram os 6 achados críticos da mesma auditoria.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Item criado via `/add-backlog-item` |
| 2026-09-07 | Revisão `/review-spec` — 2 gaps de critério de aceite corrigidos (cap de skill exibido sem dono definido; ausência de critério observável pro achado `AUD-01-12`) + 3 corner cases adicionados (reinício de processo vs. cache de gating; arma nova sem mudança de nível; janela de transição de cena no handler de XP médico) |
