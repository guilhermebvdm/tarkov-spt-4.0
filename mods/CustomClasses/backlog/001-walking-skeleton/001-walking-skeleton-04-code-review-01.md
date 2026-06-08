# 001 — Scaffold + 1 classe (walking skeleton) · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [001-walking-skeleton-01-spec.md](001-walking-skeleton-01-spec.md)
**Spec técnica:** [001-walking-skeleton-02-spec-tech.md](001-walking-skeleton-02-spec-tech.md)
**Asbuild:** [001-walking-skeleton-05-asbuild.md](001-walking-skeleton-05-asbuild.md)
**Data:** 2026-06-07

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 0 pendentes · 🟢 0 pendentes · ✅ Aplicados: 2 (CR-01-02, CR-01-03) · ⏭️ Deferidos/rejeitados: 3 (CR-01-01→002, CR-01-04→002, CR-01-05→007) · Total: 5

**Contexto positivo:** o caminho golden está **validado empiricamente** (edition aparece em isolamento; Endurance 5 / Strength 3 in-game; build 0 warn/err). Os achados abaixo são latentes/qualidade — nenhum bloqueia o fechamento do 001. Vários só importam a partir do item 002 (quando `BaseEdition`/skills viram dados de JSON arbitrários).

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | `CommonSkill` novo com `LastAccess=0` / `Max`/`Min` ausentes | ⏭️ Deferido (002) |
| CR-01-02 | C — Gap vs. spec | 🟡 | Skip silencioso se um lado tem `Character`/`Skills` null; log conta intenção, não resultado | ✅ Aplicado |
| CR-01-05 | D — Arquitetura | 🟡 | Coexistência com RZCustomProfiles (clobber) — deferir ao item 007 | ⏭️ Deferido (007) |
| CR-01-03 | D — Arquitetura | 🟢 | Dependência de `ICloner.Clone` ser deep não documentada | ✅ Aplicado |
| CR-01-04 | E — Legibilidade | 🟢 | Magic strings hardcoded (aceitável no 001) | ⏭️ Resolve no 002 |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/manutenção** · **F — Melhoria opcional**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio

**`CommonSkill` novo criado com `LastAccess=0` e sem `Max`/`Min`**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) — `ApplySkills`, ramo `entry is null`.

**Problema:** quando a skill da classe não existe na `Skills.Common` do template base, o código adiciona `new CommonSkill { Id = skill, Progress = progress }` — deixando `LastAccess = 0`, `PointsEarnedDuringSession = 0`, `Max = null`, `Min = null`.

**Por que importa:** `LastAccess = 0` (epoch 1970) pode bagunçar cálculos de fadiga/decay de skill que usam o tempo desde o último acesso. No 001 **não dispara** (o base "SPT Zero to hero" já traz toda a lista de skills, então cai sempre no ramo `else`). Vira risco real no item 002, quando classes definem skills arbitrárias e/ou bases diferentes.

**Sugestão:** preferir sempre mutar a entrada existente (o base padrão tem todas); para skill genuinamente nova, clonar a forma de uma entrada existente ou setar `LastAccess` via `TimeUtil` (injetar `TimeUtil`). Tratar concretamente no item 002; aqui basta registrar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): **deferido ao item 002 (skills arbitrárias + `TimeUtil`); não dispara no 001**

---

### CR-01-02 · C — Gap vs. spec · 🟡 Médio

**Skip silencioso quando um lado não tem `Character`/`Skills`; log "(N skills)" reporta intenção, não resultado**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) — `ApplySkills` (guarda `character?.Skills?.Common is null → return`) e o log final de `OnLoad` (`{TestClass.Skills.Count} skills`).

**Problema:** se o template base tiver `Usec`/`Bear` com `Character` ou `Skills` nulos, `ApplySkills` retorna sem aplicar nada, mas a edition é registrada e o log ainda diz `(2 skills, ...)`. A descrição/edition aparecem, porém o personagem nasce sem as skills — falha silenciosa.

**Por que importa:** o corner case "os dois lados (USEC/BEAR)" da spec funcional fica sem rede de proteção. No 001 não dispara ("Zero to hero" é válido), mas com `BaseEdition` arbitrário (item 002) um base mal configurado passaria despercebido.

**Sugestão:** logar `Warning` quando um lado não tiver skills aplicáveis; opcionalmente contar skills efetivamente aplicadas e logar esse número (em vez de `TestClass.Skills.Count`). Melhora a observabilidade do PA-01 (confirmar no boot).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-05 · D — Arquitetura · 🟡 Médio

**Coexistência com RZCustomProfiles (clobber do dicionário de templates)**

**Local:** comportamento de runtime (não um trecho específico) — `OnLoad` injeta em `GetProfileTemplates()`; o RZCustomProfiles roda depois e reconstrói o dicionário.

**Problema:** com o RZCustomProfiles ativo, a edition "Test Class" some do launcher (validado no playtest: só aparece com o RZ desabilitado).

**Por que importa:** num ambiente real do usuário (que hoje roda o RZ), a feature não aparece. Mas o 001 é "walking skeleton" validado **em isolamento**, e a coexistência/aposentadoria do RZ é **explicitamente o item 007**.

**Sugestão:** **Rejeitar/deferir para o item 007** (coexistência → aposentar RZ). Não mexer no 001. Registrado aqui só para rastreabilidade.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): **deferido ao item 007 (coexistência/retire RZ)**

---

### CR-01-03 · D — Arquitetura · 🟢 Menor

**Dependência de `ICloner.Clone` ser deep não está documentada**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) — `var sides = cloner.Clone(baseSides);` + mutação de `sides.Usec.Character.Skills.Common`.

**Problema:** mutamos coleções aninhadas do clone (`Skills.Common`, `CommonSkill`). Isso só é seguro se `Clone` for **deep** (senão mutaríamos o template vanilla compartilhado).

**Por que importa:** é deep (o `CreateProfileService` do SPT depende disso), mas a premissa fica implícita; um leitor futuro pode não perceber.

**Sugestão:** comentário de 1 linha junto ao `Clone` deixando explícito "deep clone — mutação segura sem afetar o template base".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · E — Legibilidade · 🟢 Menor

**Magic strings hardcoded (`"Test Class"`, `"SPT Zero to hero"`)**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) — `TestClass`.

**Problema:** identificadores e níveis estão hardcoded.

**Por que importa:** baixo impacto — é o walking skeleton; o `ClassDefinition` já é a semente do schema JSON que o item 002 externaliza.

**Sugestão:** **nenhuma ação no 001** (vira JSON no 002). Registrado para rastreabilidade; pode rejeitar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): **resolvido naturalmente no item 002 (schema JSON)**

---

---

## Resolução (apply-code-review — 2026-06-07)

| ID | Resultado | Detalhe |
| --- | --- | --- |
| CR-01-02 | ✅ Aplicado | `ApplySkills` agora retorna nº de skills aplicadas (`-1` se o lado não tem skills); `OnLoad` loga `Warning` se um lado faltar e reporta a contagem real (em vez de `TestClass.Skills.Count`). `CustomClassesMod.cs` |
| CR-01-03 | ✅ Aplicado | Comentário no `cloner.Clone` documentando deep clone (mutação de `Skills.Common` é segura). `CustomClassesMod.cs` |
| CR-01-01 | ⏭️ Deferido → item 002 | Não dispara no 001 (base "SPT Zero to hero" tem todas as skills); tratar `LastAccess`/`Max`/`Min` quando skills/bases virarem dados de JSON. |
| CR-01-04 | ⏭️ Resolve no item 002 | Magic strings saem para o JSON dinâmico. |
| CR-01-05 | ⏭️ Deferido → item 007 | Coexistência/clobber do RZCustomProfiles. |

Recompilado: `CustomClasses-Server.dll` (0 warn/err) e reinstalado.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 01 criada via `/code-review` |
| 2026-06-07 | Aplicação via `/apply-code-review` — aplicados: CR-01-02, CR-01-03; deferidos: CR-01-01 (→002), CR-01-04 (→002), CR-01-05 (→007) |
