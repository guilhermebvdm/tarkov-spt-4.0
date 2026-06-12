# 012 — Identidade da classe no menu + tela de Skills · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [012-identidade-menu-skills-01-spec.md](012-identidade-menu-skills-01-spec.md)
**Spec técnica:** [012-identidade-menu-skills-02-spec-tech.md](012-identidade-menu-skills-02-spec-tech.md)
**Asbuild:** [012-identidade-menu-skills-05-asbuild.md](012-identidade-menu-skills-05-asbuild.md)
**Data:** 2026-06-08

> Análise crítica do `/code-mod`. IDs `CR-01-MM`. 0 bloqueadores. O ponto central é **validação visual in-game** (posições).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟡 | Posição do selo (menu/Skills) não validada — ajuste in-game | Pendente |
| CR-01-02 | F — Melhoria | 🟢 | Coroutines concorrentes ao reabrir o menu | Pendente |
| CR-01-03 | B — Bug latente | 🟢 | `GetTargetMethod` via `First` lança se o EFT mudar o `Show` | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · C — Gap vs. spec · 🟡 Médio

**Posição do selo (menu/Skills) é um chute inicial — precisa de ajuste in-game**

**Local:** [`MenuClassIdentityPatch.cs`](../../modded/Client/Patches/MenuClassIdentityPatch.cs), [`SkillsScreenIdentityPatch.cs`](../../modded/Client/Patches/SkillsScreenIdentityPatch.cs)

**Problema:** com o Menu-Overhaul, o selo é anexado como filho de `BottomField` (que tem `VerticalLayoutGroup`) → ele entra na pilha vertical (Level/Nickname/Exp) numa posição não calibrada. Sem MO e na tela de Skills, os `anchoredPosition` (canto/topo) são valores iniciais. **Nada disso foi visto in-game.**

**Por que importa:** o selo pode aparecer fora do lugar / sobre outro elemento — é o critério visual da spec.

**Sugestão:** validar no playtest e ajustar: (a) com MO, decidir se o selo vai dentro do `BottomField` (vertical) ou como irmão posicionado ao lado; (b) calibrar os `anchoredPosition` do canto fixo e do topo da tela de Skills. São constantes isoladas — iteração rápida a cada build. (Este é o foco do próximo teste.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (validar in-game e calibrar posições)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

### CR-01-02 · F — Melhoria · 🟢 Menor

**Coroutines concorrentes ao reabrir o menu**

**Local:** [`MenuClassIdentityPatch.cs`](../../modded/Client/Patches/MenuClassIdentityPatch.cs)

**Problema:** cada `MenuScreen.Show` inicia uma `PlaceCoroutine`. Reabrir o menu várias vezes acumula coroutines simultâneas (curtas, ~30 frames). A idempotência (`Find` do selo) garante **1 selo**, mas há trabalho redundante.

**Sugestão:** opcional — guardar uma flag "coroutine em andamento" ou checar se o selo já existe no início da coroutine e sair cedo. Aceitar como está também é razoável (coroutines são curtas).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

### CR-01-03 · B — Bug latente · 🟢 Menor

**`GetTargetMethod` via `First(...)` lança se o EFT remover/renomear o `Show`**

**Local:** ambos os patches (`AccessTools.GetDeclaredMethods(...).First(...)`)

**Problema:** se um patch futuro do EFT mudar a assinatura do `Show`, `First` lança `InvalidOperationException` no `GetTargetMethod` → o `Enable()` falha no `Awake` → pode impedir o carregamento do plugin.

**Por que importa:** fragilidade entre versões do EFT (mesma de qualquer patch, mas `First` é mais barulhento que `AccessTools.Method` retornando null).

**Sugestão:** opcional — `FirstOrDefault` + log claro se null (o ModulePatch lida com target nulo melhor que uma exceção). Baixa prioridade (só quebra se o EFT mudar).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Code review 01 criada via `/code-review` — 0 🔴 · 1 🟡 (validação visual) · 2 🟢. |
