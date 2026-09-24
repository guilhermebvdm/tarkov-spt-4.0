# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · Code Review 01

**Mod:** FIKA
**Spec funcional:** [006-colisao-maos-item-nao-carregador-01-spec.md](006-colisao-maos-item-nao-carregador-01-spec.md)
**Spec técnica:** [006-colisao-maos-item-nao-carregador-02-spec-tech.md](006-colisao-maos-item-nao-carregador-02-spec-tech.md)
**Asbuild:** [006-colisao-maos-item-nao-carregador-05-asbuild.md](006-colisao-maos-item-nao-carregador-05-asbuild.md)
**Data:** 2026-09-11

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** topo de `mods/FIKA/memory/sessions.md` (última entrada Sessão 6, 2026-09-10) · pendências: `P-6.1` (🔴, exatamente o que este item resolve — validação empírica já feita) e outras não-bloqueantes (`P-5.1`, `P-4.1`, `P-3.1`, `P-3.2`, `P-1.x`). Nenhuma pendência bloqueante nova.
**Docs técnicos:** `spt-antipatterns.md` reconferido contra o código real — ver achados AP-03/AP-09 abaixo, ambos já tratados corretamente no código (não geram achado novo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | E | 🟢 Menor | Comentário do checklist da spec técnica cita faixa de linha que já mudou após o próprio `/code-mod` | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

## Observação geral

Este item passou por 2 rodadas de review técnica (a primeira derrubou a hipótese original e forçou reescrita completa da spec; a segunda revisou o desenho novo) e por auto-correção ativa durante a própria redação da spec (3 erros reais encontrados e corrigidos antes mesmo de qualquer rodada formal: dimensionamento da janela de graça, `using` faltando, comportamento de exceção do `GetTargetMethod() == null`). O `/code-mod` seguiu os stubs já corrigidos **sem nenhum desvio** — build local (`dotnet build -c Release`) confirma 0 erros, e `check-packet-hashes.js` confirma 0 regressão de rede. Por isso esta rodada de code-review encontrou só 1 achado cosmético, não um problema de lógica ou arquitetura — o volume de revisão já aconteceu na fase de spec técnica, como pretendido pelo ciclo do repo (pegar erros antes de codar, não depois).

Conferido especificamente (sem achado):
- **AP-03** (virtual dispatch): `method_19` é não-virtual, `ObservedFirearmController` não sobrescreve `Drop` — auditoria já documentada na spec técnica, reconfirmada aqui contra o código real.
- **AP-09** (patch-point): todas as linhas citadas nos comentários do código batem com o Assembly (reconferido nesta revisão pontualmente em `Player.cs:13506`, `TraderControllerClass.cs:1822`).
- **Interação Fix 1a × Fix 1b:** ambos tocam o mesmo laço de `CheckItemAction` (linhas 174 e 186) — verificado que não há colisão de lógica entre os dois blocos: um `GEventArgs17` nunca satisfaz `item == geventArgs2.Item` a menos que `item` seja a própria arma (caso em que o bloqueio continua correto por dois caminhos independentes, sem regressão); um `GEventArgs9`/`GEventArgs10` nunca é interpretado como `inOutHandsProcess` (cast `as GEventArgs17` falha, bloco Fix 1a não roda).
- **Registro protegido em `FikaPlugin.cs`:** o `try/catch` em volta de `EnablePatch(new HandsBookkeepingTimestampPatch.RecordHandsBookkeepingEvent())` está exatamente onde precisa estar (call site do registro individual, não só dentro do `GetTargetMethod()`).

---

## Pontos

### CR-01-01 · E — Legibilidade/manutenção · 🟢 Menor · ✅ Aplicado em 2026-09-11

**Comentário do checklist da spec técnica cita faixa de linha que já mudou após o próprio `/code-mod`**

**Local:** [`mods/FIKA/backlog/006-colisao-maos-item-nao-carregador/006-colisao-maos-item-nao-carregador-02-spec-tech.md`](../006-colisao-maos-item-nao-carregador-02-spec-tech.md) §8 (não é código, é o próprio artefato de planejamento)

**Problema:** O item de checklist "Editar `ObservedInventoryController.CheckItemAction` (linha 182-188 atuais)..." cita a faixa de linha **de antes** do `/code-mod`. Depois da edição (Fix 1a adicionou linhas em `IsSelfReferentialHandsTransition`, e o próprio bloco de Fix 1b ganhou um comentário de 4 linhas antes da condição), o bloco relevante em `ObservedInventoryController.cs` real está agora em `186-192`, não mais `182-188`.

**Por que importa:** É só um artefato de planejamento (spec técnica), não afeta o código em produção. Mas se alguém usar essa citação de linha pra navegar até o trecho depois do build, vai cair no lugar errado por poucas linhas.

**Sugestão:** Não vale a pena editar a spec técnica retroativamente só por isso (ela já documenta "linha 182-188 atuais" no sentido de "no momento em que este checklist foi escrito", e o `05-asbuild.md` é quem tem a responsabilidade de refletir o estado real pós-build). Registrar aqui como nota, sem ação — ou, se preferir consistência, adicionar uma linha ao `05-asbuild.md` confirmando a faixa de linha final (`186-192`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `006-colisao-maos-item-nao-carregador-05-asbuild.md` — linha adicionada na tabela "Arquivos alterados" confirmando a faixa final (`186-192`) do bloco de Fix 1b em `ObservedInventoryController.cs`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-11 | Code review 01 criada via `/code-review` |
