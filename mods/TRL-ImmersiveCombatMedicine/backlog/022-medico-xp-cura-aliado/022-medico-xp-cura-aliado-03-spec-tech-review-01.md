# 022 — Médico ganha XP ao curar aliado · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [022-medico-xp-cura-aliado-02-spec-tech.md](022-medico-xp-cura-aliado-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-05 (Sessão 10) · pendências que afetam: nenhuma diretamente.

**Docs técnicos conferidos:** `spt-antipatterns.md` (sempre), `fika-packet-desync-prevention-plan.md` (mod estende pacote `INetSerializable`) — nenhuma contradição encontrada entre a spec e esses docs.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Decisão de escopo pendente: XP de HP puro (`ExpForHeal`) fica fora? | ✅ Resolvido 2026-09-09 |
| PA-01-02 | A — Gap | 🟡 Importante | Caminho B: dupla leitura por reflection (existência + XP) em vez de uma só | ✅ Resolvido 2026-09-09 |
| PA-01-03 | C — Erro de lógica | 🟢 Menor | Campo `_subscribedPatientHcForXp` declarado e nunca usado no stub §5.1 | ✅ Resolvido 2026-09-09 |
| PA-01-04 | C — Erro de lógica | 🟢 Menor | `ReadHealExperience` usa os tipos "concretos" de remoção em vez dos tipos "de leitura" já convencionados no arquivo | ✅ Resolvido 2026-09-09 |
| PA-01-05 | A — Gap | 🟢 Menor | Padrão de decisão compartilhado (`method_6` vs 3 linhas duplicadas) sem decisão registrada | ✅ Resolvido 2026-09-09 |

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

### PA-01-01 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-09

**Decisão de escopo pendente: XP de HP puro (`ExpForHeal`) fica fora?**

**Problema:** A spec técnica (§1, penúltimo bullet) identifica corretamente que restaurar HP puro (sem remover um efeito) usa um mecanismo vanilla DIFERENTE — `GClass2266.OnHealthChanged` ([GClass2266.cs:169-189](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L169)), disparado por `HealthChangedEvent` (não `HealerDoneEvent`), com fórmula `GClass1720_0.Heal.ExpForHeal * diff` — e conclui que esse mecanismo fica fora do escopo deste item. Essa conclusão está marcada com `<!-- review: -->` na própria spec, mas nunca foi promovida a um ponto formal de decisão nesta review.

**Por que importa:** A spec funcional (022-01, critério 1) diz "o médico ganha XP idêntico ao que o vanilla concede ao tratar aquele mesmo sangramento" — um leitor apressado pode entender isso como "toda cura, incluindo o HP restaurado, deveria contar". Se o usuário esperava o pacote completo (HealExperience por efeito **+** ExpForHeal por HP), a implementação atual entregaria só a metade sem que isso apareça em nenhum critério de aceite marcado como não-atendido — o gap ficaria invisível até o teste in-game.

**Sugestão:** Confirmar com o usuário: a spec funcional 022-01 fala explicitamente em "`HealExperience` por efeito curável (sangramento, fratura, intoxicação, dor)" na Visão Geral — não menciona XP por HP restaurado. Se essa leitura for a pretendida (mais provável, dado o texto), fechar este ponto como "aceitar sugestão" e adicionar uma linha explícita em "Fora de escopo" da spec funcional 022-01: "Recompensa de XP por HP restaurado (`ExpForHeal`, mecanismo `OnHealthChanged` separado de `HealExperience`) — não coberto por este item." Se o usuário quiser o pacote completo, é um **item de backlog adicional** (mecanismo, evento e fórmula diferentes, Caminho A e B próprios) — não cabe expandir esta spec técnica sem reabrir a funcional.

**Decisão:**
- `[x]` Caminho alternativo: **incluir também o XP de HP restaurado (`ExpForHeal`)** — usuário optou por cobrir o pacote completo, não só `HealExperience` por efeito.

**Resolução:** Escopo expandido. Spec funcional `022-medico-xp-cura-aliado-01-spec.md` recebeu critérios/corner cases novos para o mecanismo `OnHealthChanged`/`ExpForHeal` ([GClass2266.cs:169-189](../../../../references/eft-decompiled/Assembly-CSharp/GClass2266.cs#L169), formula em `BackendConfigSettingsClass.GClass1720.Heal.ExpForHeal` = `expForHeal` em `globals.json:36324`, valor padrão `1`). Spec técnica `022-medico-xp-cura-aliado-02-spec-tech.md` reescrita com o 2º mecanismo nos dois caminhos (§1/§2/§5/§6/§8/§9), incluindo o cuidado de acumular o `diff` de HP por operação de cura ANTES de truncar para `int` (Caminho A) — replicar o truncamento vanilla ingênuo por tick perderia quase todo o XP em curas graduais (cada tick de `MedEffect` cura poucos HP; `(int)(ExpForHeal * diffPequeno)` trunca pra 0 repetidamente).

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-09

**Caminho B: dupla leitura por reflection (existência + XP) em vez de uma só**

**Problema:** Em `ApplyFullTreatmentLocally` (código atual, `BandAidNetworkHandler.cs:499-501`), `hadHeavy`/`hadLight`/`hadFracture` já são computados chamando `HasEffect(activeHc, target, _heavyBleedType)` etc. — que internamente já faz `FindActiveEffect<T>` via reflection só para checar `!= null` ([BandAidNetworkHandler.cs:601-616](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs#L601), citação da própria spec técnica). O stub `ReadHealExperience` proposto em 02-spec-tech §5.3 faz uma **segunda** chamada `FindActiveEffect<T>` via reflection para o MESMO efeito, só para ler `HealExperience` — dobra o custo de reflection por efeito tratado sem necessidade.

**Por que importa:** Não é hot path (uma chamada por tratamento, não por frame), então não é um bloqueador de performance — mas é exatamente o antipadrão que `csharp-mod-best-practices` §1/§3 pede para evitar ("cache toda resolução de reflection"; aqui o problema não é falta de cache, é uma chamada redundante que a spec técnica introduz por não reaproveitar o resultado que `HasEffect` já obteve). Fica mais fácil de o `/code-mod` corrigir agora do que depois de codado.

**Sugestão:** Substituir `HasEffect(activeHc, target, _heavyBleedType) : bool` por uma variante que retorne o **efeito encontrado** (ou `null`), ex. `FindEffectForRead(activeHc, target, effectType) : IEffect`. `hadHeavy = FindEffectForRead(...) != null` continua funcionando igual; `ReadHealExperience` deixa de existir como função separada — em vez disso, guardar o resultado de `FindEffectForRead` numa variável local (ex. `var heavyEffect = FindEffectForRead(...)`) ANTES do `RemoveEffectNative`, e ler `(heavyEffect as GInterface326)?.HealExperience` dessa mesma variável. Atualizar §5.3 da spec técnica com essa reestruturação antes do `/code-mod`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Spec técnica reescrita (§5.3) com `FindEffectForRead(activeHc, bodyPart, effectType) : IEffect` — uma única chamada de reflection por efeito, guardada em variável local; `hadHeavy`/etc. e a leitura de `HealExperience` usam a mesma instância. `ReadHealExperience` foi removido do stub.

---

### PA-01-03 · C — Erro de lógica · 🟢 Menor · ✅ Resolvido em 2026-09-09

**Campo `_subscribedPatientHcForXp` declarado e nunca usado no stub §5.1**

**Problema:** O stub em 02-spec-tech §5.1 declara `private static IHealthController _subscribedPatientHcForXp = null;` com o comentário "mesmo objeto de `_subscribedPatientHc`; nome próprio só para clareza do handler" — mas nenhum trecho subsequente do mesmo stub (nem a assinatura, nem a desassinatura) usa essa variável; ambas usam `_subscribedPatientHc` diretamente, que já existe no arquivo (`MedicHealPatch.cs:40`).

**Por que importa:** Se o `/code-mod` copiar o stub literalmente, introduz um campo estático morto — pequeno, mas é exatamente o tipo de resíduo que as sessões anteriores do mod já removeram como débito técnico (ex. Sessão 9, unificação/limpeza).

**Sugestão:** Remover a declaração de `_subscribedPatientHcForXp` do stub em §5.1 — ela não é necessária; `_subscribedPatientHc` (já existente) é suficiente para os dois handlers (`OnPatientEffectRemoved` e o novo `OnPatientHealerDone`), já que ambos os eventos são assinados/desassinados no mesmo par `MedicHealPatch.cs:412-416`/`:194-202`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Campo removido do stub reescrito em §5.1 da spec técnica; ambos os handlers usam `_subscribedPatientHc`.

---

### PA-01-04 · C — Erro de lógica · 🟢 Menor · ✅ Resolvido em 2026-09-09

**`ReadHealExperience` usa os tipos "concretos" de remoção em vez dos tipos "de leitura" já convencionados no arquivo**

**Problema:** `BandAidNetworkHandler.cs:23-26` documenta explicitamente a convenção do próprio arquivo: `_heavyBleedType`/`_lightBleedType`/`_fractureType` são "GInterfaces para LEITURA", enquanto `_heavyBleedConcreteType`/`_lightBleedConcreteType`/`_fractureConcreteType` são "para REMOÇÃO" (usados só em `method_15`/`RemoveEffectNative`). O stub de `ReadHealExperience` em 02-spec-tech §5.3 chama `ReadHealExperience(activeHc, target, _heavyBleedConcreteType)` — usando o tipo de REMOÇÃO para uma operação de LEITURA, invertendo a convenção documentada no próprio arquivo (ainda que funcionalmente o `FindActiveEffect<T>` genérico provavelmente aceite qualquer um dos dois, já que o tipo concreto também satisfaz a constraint `where TEffect : IEffect`).

**Por que importa:** Não é um erro de compilação nem, aparentemente, de comportamento — mas quebra a legibilidade que o próprio código do mod já estabeleceu, e um code-review (`/code-review`) provavelmente pegaria isso como inconsistência de estilo, gerando retrabalho.

**Sugestão:** Trocar, no stub de §5.3 (ou na função unificada sugerida em PA-01-02), os argumentos de tipo para `_heavyBleedType`, `_lightBleedType`, `_fractureType` (os "de leitura"), mantendo `_heavyBleedConcreteType` etc. exclusivamente para as chamadas de `RemoveEffectNative`, como o arquivo já faz hoje.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `FindEffectForRead` (PA-01-02) usa `_heavyBleedType`/`_lightBleedType`/`_fractureType` ("de leitura"), reservando os tipos "concretos" exclusivamente para `RemoveEffectNative`.

---

### PA-01-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-09

**Padrão de decisão compartilhado (`method_6` vs 3 linhas duplicadas) sem decisão registrada**

**Problema:** A spec técnica já sinaliza a assimetria com um `<!-- review: -->` em §5.3: o Caminho A chama `GClass2266.method_6(effect)` diretamente (uma linha, reuso total), enquanto o Caminho B precisa replicar manualmente as 3 linhas internas de `method_6` (`ExperienceGained` + `SessionCounters.AddInt` + `ShowStatNotification`) porque o objeto `IEffect` não atravessa a rede — só o inteiro `XpAwarded` chega. Isso é uma decisão de design (duplicar 3 linhas simples vs. extrair um helper único chamado pelos dois caminhos) que ainda não foi formalmente decidida.

**Por que importa:** Duplicar lógica em dois lugares é o tipo de coisa que diverge silenciosamente se um dos dois for alterado no futuro (ex. se `method_6` ganhar um 4º efeito colateral no vanilla, só o Caminho A "herda" automaticamente; o Caminho B ficaria defasado até alguém notar).

**Sugestão:** Extrair um helper único, ex. `internal static void CreditHealXp(Player doctor, int amount)` em algum lugar compartilhado (ex. `Helpers/` do mod), contendo as 3 linhas (`ExperienceGained` + `AddInt(ExpHeal)` + `ShowStatNotification`) — Caminho A chama `CreditHealXp(doctor, effect.HealExperience)` em vez de `method_6(effect)` diretamente (perde só a checagem `effect is GInterface326`, que o Caminho A já faz implicitamente ao só disparar para efeitos que TÊM esse valor); Caminho B chama o mesmo helper com `packet.XpAwarded`. Um único ponto de manutenção para os dois caminhos.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Helper `CreditHealXp(Player doctor, int amount)` extraído em `Helpers/` (spec técnica §5, novo arquivo `HealXpCredit.cs`); Caminho A e B (para AMBOS os mecanismos, efeito e HP) chamam o mesmo helper.
