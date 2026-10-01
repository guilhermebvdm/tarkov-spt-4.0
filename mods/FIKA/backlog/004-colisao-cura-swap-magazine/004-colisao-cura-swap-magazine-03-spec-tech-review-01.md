# 004 — Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine) · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [004-colisao-cura-swap-magazine-02-spec-tech.md](004-colisao-cura-swap-magazine-02-spec-tech.md)
**Data:** 2026-09-08

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-06 (Sessão 3) de `mods/FIKA/memory/sessions.md` + entradas que citam o item `004` (nenhuma própria ainda) · pendências que afetam: `P-3.1` (janela de graça não recalibrada — a spec técnica já documenta que reaproveita sem recalibrar, não é gap novo), `P-3.2` (não relacionada). Cross-mod: `mods/UIFixes/memory/sessions.md` Sessão 9b conferida — a spec técnica reconfirma (não só cita) o diagnóstico por leitura direta do código nos 3 mods envolvidos.
**Docs técnicos:** `spt-antipatterns.md` conferido — AP-03/AP-08/AP-09 corretamente aplicados (ver pontos abaixo para nuances). Nenhuma contradição encontrada entre a spec e o doc.
**Assembly reconfirmado nesta review:** `Player.cs:19441-19660` (`ObservedMedsControllerClass`/`method_9`), `Player.cs:32223-32263` (`TryRemoveFromHands`), `Player.cs:32294-32337` (`TrySetInHands`), `Player.cs:1441-1495` (`method_33/34/35`) — todas as linhas citadas na spec técnica batem com o dump atual. `ObservedInventoryController.cs` e `InOutHandsProcessTimestampPatch.cs` do mod também reconferidos linha a linha contra os stubs da §5 — nenhuma divergência.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B | 🟡 Importante | Checklist não valida a sequência repetida/múltiplos aliados exigida pela spec funcional | ✅ Resolvido |
| PA-01-02 | B | 🟢 Menor | Checklist não inclui a checagem do observador (jogador B) exigida pelo critério Fika/multiplayer | ✅ Resolvido |
| PA-01-03 | C | 🟢 Menor | Comentário inline do stub cita o caminho completo da spec técnica em vez de um ID curto e estável | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### ✅ PA-01-01 · B — Edge Case · 🟡 Importante — Resolvido em 2026-09-08

**Checklist de implementação não cobre a validação de sequência repetida/múltiplos aliados**

**Problema:** A spec funcional (`004-colisao-cura-swap-magazine-01-spec.md`, seção "Critérios de aceite") exige explicitamente: *"Repetir a sequência cura → swap de carregador várias vezes seguidas (inclusive curando a si mesmo, e curando mais de um aliado em sequência) não introduz travamento nem divergência."* O §8 (Checklist de implementação) da spec técnica lista validação do cenário principal e do corner case "self-heal" isoladamente, mas não inclui um item de validação para a sequência **repetida** (múltiplas curas seguidas, inclusive a aliados diferentes) na mesma raid.

**Por que importa:** O dicionário `_state` (`InOutHandsProcessTimestampPatch.cs:40`) é sobrescrito a cada novo `Begin` para a mesma arma (`RecordBeginSucceed.Postfix`, linha 115-117: `perWeapon[args.Item] = new Entry {...}` — atribuição, não acumulação). Isso sugere que repetições devem funcionar, mas é exatamente o tipo de suposição que só uma reprodução real confirma (o mesmo padrão de raciocínio que já levou ao `TODO confirmar` da janela de graça no item 003). Sem esse item explícito no checklist, fica fácil "esquecer" de testar o cenário repetido durante a validação in-game e dar o item por fechado sem cobrir um critério de aceite obrigatório.

**Sugestão:** Adicionar ao §8 da spec técnica um item de checklist explícito: *"Validar in-game a sequência repetida: curar 2+ aliados diferentes em sequência (ou o mesmo aliado 2+ vezes) trocando o carregador após cada cura, confirmando ausência de travamento/divergência em qualquer repetição — não só na primeira."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** item de checklist adicionado à spec técnica §8 (`// ref: PA-01-01`).

---

### ✅ PA-01-02 · B — Edge Case · 🟢 Menor — Resolvido em 2026-09-08

**Checklist não inclui a checagem do observador (jogador B) exigida pelo critério Fika/multiplayer**

**Problema:** A spec funcional exige no critério padrão **Fika/multiplayer**: *"jogador B (observador) não deve ver nenhuma anomalia visual na arma ou no inventário de A."* O §8 da spec técnica tem um item para "dois jogadores diferentes mexendo em armas diferentes" (proteção estrutural), mas nenhum item cobre especificamente a perspectiva visual do jogador B assistindo à sequência cura→swap de A.

**Por que importa:** É um critério de aceite explícito e nomeado da spec funcional; a ausência no checklist técnico é o tipo de lacuna que passa despercebida numa validação apressada (o foco natural vai para "A recebeu o item de volta", não para "B viu algo estranho"). Impacto é menor porque a correção em si não toca em nenhum código de replicação/renderização — é puramente uma checagem de servidor — mas vale registrar explicitamente para não depender de lembrança.

**Sugestão:** Adicionar ao §8: *"Validar in-game, com um segundo jogador observando (B), que a sequência cura→swap de carregador de A não produz nenhuma anomalia visual na arma ou inventário de A do ponto de vista de B."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** item de checklist adicionado à spec técnica §8 (`// ref: PA-01-02`).

---

### ✅ PA-01-03 · C — Erro de Lógica (convenção) · 🟢 Menor — Resolvido em 2026-09-08

**Comentário inline do stub cita o caminho completo da spec técnica em vez de um ID curto e estável**

**Problema:** No stub §5 (segundo bloco, novo `IsSelfReferentialHandsTransition`), o comentário inline referencia `004-colisao-cura-swap-magazine-02-spec-tech.md §1.3` por extenso. `repo-workflow-best-practices` §4 estabelece que comentários inline no código devem citar IDs curtos e permanentes (`PA-NN-MM`, `CR-NN-MM`) — o padrão que o próprio item `003` usa em seu código já construído (`// ref: PA-01-02`), não o nome do arquivo de spec por extenso.

**Por que importa:** Um comentário citando o nome completo do arquivo de spec técnica quebra silenciosamente se o arquivo for renomeado (procedimento coberto por `.agents/conventions.md`, mas o comentário no `.cs` não é atualizado automaticamente) — e é mais verboso do que o necessário para quem só quer saber "por que isso existe" durante leitura de código. Impacto é puramente de manutenibilidade/estilo, não de comportamento.

**Sugestão:** Trocar o comentário inline do stub para citar `// ref: item 004 (colisão cura + swap magazine)` — um identificador curto, estável mesmo que o arquivo de spec seja renomeado ou uma review numerada seja adicionada depois. Se a próxima review (`/review-technical-spec`) gerar um PA-NN-MM relevante para este trecho, trocar para esse ID (mesmo padrão do item 003).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** comentário inline reescrito na spec técnica §5 (ambos os blocos) para `// ref: item 004 (colisão cura + swap magazine)`.
