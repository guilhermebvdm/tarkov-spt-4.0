# 004 — Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine) · Code Review 01

**Mod:** FIKA
**Spec funcional:** [004-colisao-cura-swap-magazine-01-spec.md](004-colisao-cura-swap-magazine-01-spec.md)
**Spec técnica:** [004-colisao-cura-swap-magazine-02-spec-tech.md](004-colisao-cura-swap-magazine-02-spec-tech.md)
**Asbuild:** [004-colisao-cura-swap-magazine-05-asbuild.md](004-colisao-cura-swap-magazine-05-asbuild.md)
**Data:** 2026-09-08

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-09-06 (Sessão 3) de `mods/FIKA/memory/sessions.md` + entradas que citam o item `004` (nenhuma própria ainda) · pendências que afetam: `P-3.1` (janela de graça não recalibrada — não é achado desta review, já documentado como débito aceito na spec técnica), `P-3.2` (não relacionada).
**Docs técnicos:** `spt-antipatterns.md` conferido — nenhuma violação nova introduzida pelo diff (AP-01/02/03/04/07/08 seguem N/A ou ✅ pelas mesmas razões já documentadas na spec técnica §9, que reconferi contra o código real).
**Assembly reconfirmado:** `Player.cs:32223-32263` (`TryRemoveFromHands`) e `Player.cs:32294-32337` (`TrySetInHands`) batem com as citações da spec técnica; código implementado não introduz nenhum novo ponto de patch no Assembly.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | E | 🟡 Médio | Comentário de documentação do método ainda cita o caminho completo da spec técnica (PA-01-03 aplicado parcialmente) | ✅ Aplicado |

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

---

## Pontos

### CR-01-01 · E — Legibilidade/manutenção · 🟡 Médio · ✅ Aplicado em 2026-09-08

**Comentário de documentação do método ainda cita o caminho completo da spec técnica (PA-01-03 aplicado parcialmente)**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs:211-217`](../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L211)

**Problema:** `003-magazine-swap-inplace-fix-03-spec-tech-review-01.md` ponto `PA-01-03` (aceito e marcado ✅ Resolvido) pediu para trocar as referências inline por um ID curto e estável (`// ref: item 004 ...`) em vez do caminho completo do arquivo de spec técnica, justamente porque um nome de arquivo por extenso quebra silenciosamente se o arquivo for renomeado. O `/code-mod` aplicou essa troca no comentário **dentro** do corpo do método (linha 235: `"ver spec técnica do item 004 §1.3"`, forma curta correta) e no comentário do call-site em `CheckItemAction` (linha 169: `"ref: item 004 (colisão cura + swap magazine)"`, também correto) — mas o comentário de documentação **acima da assinatura do método** (linhas 211-217) manteve a forma antiga:

```csharp
    // ... (linhas 211-216 abreviadas)
    // TraderControllerClass = por jogador). Ver 004-colisao-cura-swap-magazine-02-spec-tech.md §1.3.
    private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
```

**Por que importa:** É puramente cosmético (não afeta comportamento em runtime), mas é exatamente o cenário que `PA-01-03` descreveu como risco: se este arquivo de spec técnica for renomeado no futuro (procedimento coberto por `.agents/conventions.md`), este comentário específico fica órfão, enquanto os outros dois (já corrigidos) permaneceriam válidos. Inconsistência dentro do mesmo método — dois comentários já seguem o padrão curto acordado, um ainda não — também é um sinal de "meio-aplicado" que confunde quem ler o código depois.

**Sugestão:** Trocar a última frase do comentário de documentação (linha 217) de `"Ver 004-colisao-cura-swap-magazine-02-spec-tech.md §1.3."` para `"Ver spec técnica do item 004 §1.3."` — mesma forma já usada nas linhas 169 e 235, fechando a aplicação de `PA-01-03` nos 3 pontos do arquivo (não só 2 de 3).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `ObservedInventoryController.cs:217` — comentário de documentação do método reescrito para "Ver spec técnica do item 004 §1.3.", com `// ref: CR-01-01` marcando o ponto tocado.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-08 | Code review 01 criada via `/code-review` |
| 2026-09-08 | Aplicação automática de 1 achado via `/apply-code-review` — ID aplicado: CR-01-01 |
