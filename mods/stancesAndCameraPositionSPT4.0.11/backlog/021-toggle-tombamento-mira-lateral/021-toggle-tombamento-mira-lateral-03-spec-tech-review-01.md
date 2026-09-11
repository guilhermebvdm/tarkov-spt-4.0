# 021 — Toggle para tombamento de mira lateral · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [021-toggle-tombamento-mira-lateral-02-spec-tech.md](021-toggle-tombamento-mira-lateral-02-spec-tech.md)
**Data:** 2026-09-08

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: nenhuma ([P-13.1]/[P-13.3] abertas, não relacionadas).
**Docs técnicos:** `spt-antipatterns.md` conferido (gatilho sempre-ativo) — nenhuma contradição encontrada entre a spec e AP-01…AP-09.
**Assembly:** as 8 refs citadas na spec técnica (`ProceduralWeaponAnimation.cs:276, 278, 1749, 1774, 1790, 1799, 2322-2342`) foram reconferidas linha a linha no dump local — todas batem exatamente com o texto da spec. Nenhum erro de linha encontrado.
**Grafo:** MCP `graphify-eft` seguiu indisponível — fallback por Grep manual, mesma disciplina do AP-03: confirmado que `ApplyComplexRotation`/`ApplySimpleRotation` têm declaração única (não-virtual) em `ProceduralWeaponAnimation.cs:1749`/`:1790`, sem overrides em todo `references/eft-decompiled/Assembly-CSharp/`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Ordem de composição do quaternion não validada | ✅ Resolvido 2026-09-08 |
| PA-01-02 | A — Gap | 🟢 Menor | Corner case "troca de arma" sem evidência citada | ✅ Resolvido 2026-09-08 |
| PA-01-03 | A — Gap | 🟢 Menor | Corner case "hideout" sem evidência citada | ✅ Resolvido 2026-09-08 |

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

### PA-01-01 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-08

**Ordem de composição do quaternion (`scopeRotation` antes de `CurrentRotation`) é uma escolha de design não validada**

**Problema:** A spec técnica (§1, "Estratégia") decide compor `weapRotation * scopeRotation * CurrentRotation` (scope primeiro, postura depois), justificando por analogia com a cadeia nativa (`_temporaryRotation * _scopeRotation`, [ProceduralWeaponAnimation.cs:1775](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1775)/[:1800](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1800)). Mas a própria memória do mod registra que os eixos são locais e não-canônicos (`mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md`, snapshot: *"girar em torno de Y = TOMBAR (roll) e em torno de Z = APONTAR (yaw)"*) — ou seja, `scopeRotation` (roll em Y) e `CurrentRotation` (pitch em X / yaw em Z, conforme as Stances 1/2/3) giram em eixos diferentes, e multiplicação de quaternions em eixos diferentes **não comuta**. A ordem escolhida (`scopeRotation * CurrentRotation`) produz um resultado geometricamente diferente de `CurrentRotation * scopeRotation` sempre que os dois ângulos não forem pequenos — e a spec não diz o que fazer se a ordem escolhida "parecer errada" ao validar in-game (critério de aceite #2 da spec funcional: "tombamento aparece combinado com a postura").

**Por que importa:** Se a ordem escolhida produzir uma pose visualmente estranha ao combinar uma Stance customizada (ex.: Stance 3, Yaw -30°) com uma mira de tombamento acentuado, o critério de aceite #2 da spec funcional falha na validação in-game, e não há uma segunda opção já pensada para tentar — o implementador teria que redescobrir o problema e a alternativa (inverter a ordem) do zero.

**Sugestão:** Adicionar uma frase ao final da §1 (ou uma nota na checklist §8) registrando explicitamente: *"Se a validação in-game do item 8(b) do checklist mostrar uma pose incorreta ao combinar Stance customizada + mira lateral, a alternativa a testar é inverter a ordem para `weapRotation * CurrentRotation * scopeRotation`."* Isso não muda a decisão atual (mantém `scopeRotation` antes, por ser a mais próxima da cadeia nativa), só documenta o plano B para não perder a linha de raciocínio se a validação in-game reprovar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Plano B documentado na spec técnica §1 (nota "Plano B para a ordem de composição (ref: PA-01-01)") — se a validação in-game 8(b) mostrar pose incorreta, inverter para `weapRotation * CurrentRotation * scopeRotation`.

### PA-01-02 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-08

**Corner case "troca de arma no meio do ADS" resolvido pelo nativo, mas sem citar a evidência**

**Problema:** A spec funcional exige (corner case) que trocar de arma rapidamente não deixe "resíduo" do tombamento da arma anterior. A spec técnica não demonstra por que isso já é coberto — mas o mecanismo existe: `method_11()` ([ProceduralWeaponAnimation.cs:1538-1553](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1538-L1553)) chama `method_24()` (linha 1551) toda vez que o conjunto de miras muda (dispara `AvailableScopesChanged`), recalculando `_targetScopeRotation` para a mira nova — e `_scopeRotation` (o campo suavizado que a spec consome) faz `Lerp` em direção a esse novo alvo a cada frame subsequente, exatamente como já faz para qualquer outra transição de ADS. Isso resolve o corner case de graça, mas a spec técnica não cita essa cadeia — fica implícito.

**Por que importa:** Sem essa citação, um revisor futuro (ou o `/code-review`) não tem como confirmar rapidamente que o corner case está coberto sem refazer essa investigação — é exatamente o tipo de claim técnica que a `memory-curation` §11 pede para vir com `arquivo:linha`.

**Sugestão:** Acrescentar ao §7 ("Riscos e dependências") uma linha: *"Troca de arma/mira: `method_11()` chama `method_24()` (`ProceduralWeaponAnimation.cs:1551`) toda vez que `AvailableScopesChanged` dispara, recalculando `_targetScopeRotation` para a mira nova; `_scopeRotation` então faz Lerp até o novo alvo nos frames seguintes — o mesmo mecanismo de suavização documentado na §1, sem necessidade de lógica adicional no mod."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Linha adicionada na spec técnica §7 ("Troca de arma/mira (ref: PA-01-02)"), citando `method_11()`/`method_24()` (`ProceduralWeaponAnimation.cs:1551`).

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-08

**Corner case "hideout / estande de tiro" sem evidência de que o Postfix roda nesse contexto**

**Problema:** A spec funcional pede que a opção se comporte igual dentro e fora de raid (hideout/estande de tiro). A spec técnica não confirma explicitamente que `ApplyComplexRotation`/`ApplySimpleRotation` — e portanto os Postfix patcheados — rodam no hideout. Não é um risco introduzido por este item (o resto do mod já roda sem guard de raid nesses dois Postfix, conforme confirmado nas leituras anteriores desta thread de trabalho), mas a spec técnica atual não registra essa constatação, deixando o corner case funcional sem uma linha correspondente na spec técnica.

**Por que importa:** Sem essa nota, fica sem lastro documental o motivo de a spec técnica não ter nenhuma seção dedicada a "comportamento em hideout" — o `/code-review` poderia levantar isso de novo como se fosse gap não visto.

**Sugestão:** Acrescentar ao §7 uma linha curta: *"Sem guard de raid nestes dois Postfix hoje (nenhum `if (!Singleton<GameWorld>.Instantiated) return`); o `ProceduralWeaponAnimation` do jogador roda igual em hideout/estande de tiro, então o corner case 'hideout' da spec funcional é herdado do comportamento pré-existente do mod, não precisa de tratamento novo."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Linha adicionada na spec técnica §7 ("Hideout / estande de tiro (ref: PA-01-03)").
