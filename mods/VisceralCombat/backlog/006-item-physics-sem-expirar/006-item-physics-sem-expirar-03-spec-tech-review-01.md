# 006 — Item Physics Sem Expirar (Manter Rigidbody Adormecido em Vez de Destruir) · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [006-item-physics-sem-expirar-02-spec-tech.md](006-item-physics-sem-expirar-02-spec-tech.md)
**Data:** 2026-09-20

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-20 (Sessão 14), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam esta revisão:** nenhuma. **Docs técnicos conferidos:** `spt-antipatterns.md` (sempre) — nenhuma contradição encontrada entre a spec e a taxonomia (ver §9 da spec técnica, todos os 11 checks preenchidos com evidência ou N/A justificado).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟢 Menor | Cast duplo desnecessário no stub (`(MonoBehaviour)(object)__instance`) | ✅ Resolvido em 2026-09-21 |
| PA-01-02 | A — Gap | 🟡 Importante | §7 não declara o comportamento quando "Item Physics" é desligado no meio da raid pra itens já preservados | ✅ Resolvido em 2026-09-21 |
| PA-01-03 | A — Gap | 🟢 Menor | §7 não explicita por que não há conflito com `PhysicalItemsPatch` (que intercepta `IsRigidbodyDone`) | ✅ Resolvido em 2026-09-21 |

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

### PA-01-01 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-21

**Cast duplo desnecessário no stub (`(MonoBehaviour)(object)__instance`)**

**Problema:** O stub de `LootItemStopPhysicsPatch.Prefix` (§5) usa `((MonoBehaviour)(object)__instance).StopCoroutine(enumerator)` — um cast duplo através de `object`. `LootItem` não herda `MonoBehaviour` diretamente (`LootItem : InteractableObject, MovingPlatform.GInterface459`, [LootItem.cs:19](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L19)), mas a chamada não-qualificada `StartCoroutine(ienumerator_0)` dentro do próprio `method_3()` ([LootItem.cs:410](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L410)) confirma que `MonoBehaviour` está na cadeia de herança (via `InteractableObject` ou um ancestral dele) — o compilador resolve isso normalmente com um cast único, direto, sem precisar passar por `object`. O padrão já usado neste mesmo mod (`VisceralShotProcessor.cs:28`: `((MonoBehaviour)StaticManager.Instance).StartCoroutine(...)`) usa cast único.

**Por que importa:** Não é um erro que impede compilar (o cast duplo também é válido em C#), só é mais verboso/confuso que o necessário e diverge do padrão já estabelecido no resto do mod — código morto de "dúvida" que fica sem explicação pra quem ler depois.

**Sugestão:** Trocar `((MonoBehaviour)(object)__instance).StopCoroutine(enumerator)` por `((MonoBehaviour)__instance).StopCoroutine(enumerator)` no stub, mantendo consistência com `VisceralShotProcessor.cs:28`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — stub em `006-item-physics-sem-expirar-02-spec-tech.md` §5 corrigido pra cast único.

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-21

**§7 não declara o comportamento quando "Item Physics" é desligado no meio da raid pra itens já preservados**

**Problema:** A spec funcional já aceita como corner case que desligar o toggle no meio da raid "não desfaz o que já aconteceu com itens já largados" — mas a spec técnica não confirma tecnicamente *como* isso se comporta na prática. Pela lógica de `LootItemStopPhysicsPatch` (§5): uma vez que `StopPhysics()` roda com "Item Physics" ativo pra um item específico, a corrotina daquele item **já foi parada** (`StopCoroutine`) — não existe nenhum mecanismo de "re-checagem" que rode de novo depois, então desligar "Item Physics" mais tarde **não tem como** retroativamente destruir o `Rigidbody` já preservado daquele item específico. Ele fica "dormindo, vivo" pelo resto da raid, independente do estado do toggle a partir daquele ponto.

**Por que importa:** Sem essa frase explícita na spec técnica, um futuro `/code-review` ou um teste do usuário ("desliguei o toggle e o item continuou reagindo!") pode ser lido como bug, quando na verdade é a consequência natural e já aceita do design (a spec funcional já autoriza esse comportamento, só não estava tecnicamente confirmado/citado aqui).

**Sugestão:** Adicionar um bullet em §7 (Riscos e dependências): "Desligar 'Item Physics' no meio da raid não afeta itens cujo `StopPhysics()` já rodou com a categoria ativa (a corrotina já foi parada pra eles, sem re-checagem futura) — eles continuam com o Rigidbody preservado (dormindo, custo baixo) até o fim da raid, mesmo com o toggle desligado depois. Isso já é o comportamento aceito no corner case correspondente da spec funcional (`006-item-physics-sem-expirar-01-spec.md`)."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — bullet adicionado em `006-item-physics-sem-expirar-02-spec-tech.md` §7.

---

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-21

**§7 não explicita por que não há conflito com `PhysicalItemsPatch` (que intercepta `IsRigidbodyDone`)**

**Problema:** `PhysicalItemsPatch.cs` (já existente no mod, não modificado por este item) já intercepta `LootItem.IsRigidbodyDone()` (Prefix, sempre retorna `true`, só reatribui a camada física do item) — método DIFERENTE do que este item intercepta (`StopPhysics()`), mas ambos operam no mesmo objeto (`LootItem`) e no mesmo fluxo (a corrotina de assentamento, [LootItem.cs:446-457](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L446-L457)). A spec menciona `PhysicalItemsPatch` en passant em §7, mas não afirma explicitamente que os dois patches não conflitam por operarem em pontos diferentes da mesma cadeia (`IsRigidbodyDone` decide *quando* parar; `StopPhysics` decide *o que fazer* quando chega a hora).

**Por que importa:** Um leitor futuro (ou o `/code-review` desta implementação) pode levantar essa dúvida como se fosse um risco não-investigado, quando na verdade já foi confirmado nesta revisão que não há sobreposição.

**Sugestão:** Adicionar uma frase em §7: "`PhysicalItemsPatch.cs:29` intercepta `IsRigidbodyDone()` (decide *quando* a corrotina considera o item pronto), enquanto este item intercepta `StopPhysics()` (decide *o que acontece* quando isso acontece) — pontos diferentes da mesma cadeia, sem sobreposição ou conflito confirmado."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — frase adicionada em `006-item-physics-sem-expirar-02-spec-tech.md` §7.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Review 01 criada — 0 bloqueadores, 1 importante, 2 menores |
| 2026-09-21 | Aplicação dos 3 achados via decisão do usuário ("Sim") — IDs: PA-01-01, PA-01-02, PA-01-03. Spec técnica atualizada. |
