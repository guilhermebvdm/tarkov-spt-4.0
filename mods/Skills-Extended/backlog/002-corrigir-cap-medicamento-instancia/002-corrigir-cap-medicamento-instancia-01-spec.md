# 002 — Corrigir cap de medicamento por instância

**Mod:** Skills-Extended
**Status:** Backlog
**Criado:** 2026-09-07

## Visão geral

Este item foi desmembrado do item [001](../001-corrigir-bugs-criticos-auditoria-01/) por decisão do usuário, já que exige uma mudança arquitetural mais trabalhosa que os demais achados críticos da auditoria 01. Corrige o achado `AUD-01-03` do relatório de auditoria (`mods/Skills-Extended/docs/relatorio-auditoria-codigo-01.md`): o limite de eficácia de medicamentos injetáveis (estimulantes) é sempre calculado com a skill de Field Medicine do jogador local, não da entidade que efetivamente usou o item.

## Comportamento atual

O limite (cap) de eficácia de um estimulante — a parte da skill de Field Medicine que limita o quanto um buff de estimulante pode ser reforçado — é sempre calculado a partir da skill de Field Medicine do jogador local, independentemente de quem realmente usou o item. Isso acontece porque o cálculo está associado a um ponto do código que processa o efeito de forma genérica, sem receber informação de qual jogador/entidade é o dono do efeito — diferente da maioria dos outros achados de mesma causa (que têm acesso direto à entidade e só deixam de usá-la), aqui a informação da entidade dona não está disponível no mesmo ponto onde o cálculo acontece, exigindo mover o ajuste para um ponto do fluxo onde essa informação existe.

## Comportamento desejado

O limite de eficácia de um estimulante deve ser calculado usando a skill de Field Medicine da entidade que efetivamente usou o item (jogador local, bot, ou outro jogador em coop) — nunca presumindo que é sempre o jogador local.

## Critérios de aceite

- [ ] O limite de eficácia de um estimulante usado por um bot ou outro jogador (não o jogador local) reflete a skill de Field Medicine da entidade que usou o item, não a do jogador local.
- [ ] O limite de eficácia de um estimulante usado pelo próprio jogador local continua refletindo corretamente a skill de Field Medicine dele mesmo (a correção não pode regredir o caso já funcional).
- [ ] Nenhum erro ocorre ao usar um estimulante em nenhum dos cenários acima.
- [ ] **Fika/multiplayer:** o comportamento incorreto só é observável quando outra entidade (bot ou outro jogador) usa um estimulante numa sessão com o jogador local presente — é o cenário que este item existe para corrigir. Em uma sessão solo sem bots por perto usando estimulante, o problema não é observável, mas o mecanismo do bug continua presente estruturalmente.
- [ ] **Estado entre raids:** N/A — o cálculo do cap acontece no momento do uso do item, dentro de uma única raid, sem estado que persista entre raids.

## Corner cases

- [ ] O que acontece se a entidade que usou o estimulante não tiver a skill de Field Medicine disponível no momento do cálculo (ex: perfil ainda carregando)?
- [ ] Dois estimulantes usados por entidades diferentes (jogador local e um bot) em instantes muito próximos — cada cálculo deve usar a skill da entidade correta, sem misturar.
- [ ] Nenhuma das correções gera erro quando executada num servidor dedicado (headless) sem jogador local nenhum — mesmo corner case obrigatório confirmado no item 001, aplicável aqui pelo mesmo motivo.
- [ ] A mudança do ponto de patch (do método utilitário estático para o método de instância) não deve alterar o comportamento de nenhum outro efeito/buff que passe pelo mesmo método utilitário além do cap de Field Medicine — se o método estático for usado por outras skills/mecânicas do mod, confirmar que elas continuam funcionando após a mudança do ponto de patch.

## Fora de escopo

- [ ] Os demais achados da auditoria 01 — cobertos nos itens 001 (críticos restantes) ou ficam registrados como dívida técnica pra rodada futura (não-críticos).

## Referências

- [Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)](../../docs/relatorio-auditoria-codigo-01.md) — achado `AUD-01-03`.
- [001-corrigir-bugs-criticos-auditoria-01](../001-corrigir-bugs-criticos-auditoria-01/) — item irmão de onde este foi desmembrado.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Item criado via `/add-backlog-item`, desmembrado do item 001 por decisão do usuário |
| 2026-09-07 | Revisão `/review-spec` — spec já escrita com a lente crítica aplicada na revisão do item 001 (corner case de headless obrigatório incluído desde a criação); nenhum gap adicional encontrado |
