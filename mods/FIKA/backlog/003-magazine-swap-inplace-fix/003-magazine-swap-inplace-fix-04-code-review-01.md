# 003 — Fix rejeição de swap de magazine in-place no Headless (GClass1561) · Code Review 01

**Mod:** FIKA
**Spec funcional:** [003-magazine-swap-inplace-fix-01-spec.md](003-magazine-swap-inplace-fix-01-spec.md)
**Spec técnica:** [003-magazine-swap-inplace-fix-02-spec-tech.md](003-magazine-swap-inplace-fix-02-spec-tech.md)
**Asbuild:** [003-magazine-swap-inplace-fix-05-asbuild.md](003-magazine-swap-inplace-fix-05-asbuild.md)
**Data:** 2026-09-06

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-09-03 (Sessão 2) de `mods/FIKA/memory/sessions.md` · pendências que afetam: nenhuma.
**Docs técnicos:** `spt-antipatterns.md` relido (AP-03, AP-08) — nenhuma violação encontrada no código final (ver PA-01-01/02 já resolvidos e reconfirmados abaixo).

**Nota:** o desvio de design descoberto durante o próprio `/code-mod` (`ObservedInventoryController.InProcess` sobrescrito, nunca passa por `Player.TrySetInHands`) já está resolvido e documentado em `003-magazine-swap-inplace-fix-05-asbuild.md` — não é re-levantado aqui como achado novo, só reconfirmado como correto na leitura desta review (ver verificação abaixo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟠 | Registro dos novos `ModulePatch` aninhados depende de auto-discovery não verificado do `PatchManager` | ✅ Aplicado em 2026-09-06 |
| CR-01-02 | E — Legibilidade | 🟢 | Stub §5 da spec técnica ainda mostra o patch `CaptureMovedItemOnSet` descartado | ✅ Aplicado em 2026-09-06 |

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

### CR-01-01 · D — Arquitetura · 🟠 Forte · ✅ Aplicado em 2026-09-06

**Registro dos novos patches depende de auto-discovery não verificado**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs:76-131`](../../modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs#L76) — classes `CaptureMovedItemOnRemove` e `RecordBeginSucceed`, ambas **aninhadas** dentro de `InOutHandsProcessTimestampPatch`.

**Problema:** O asbuild afirma que nenhum `ModulePatch` do mod precisa de `.Enable()` manual porque `_patchManager.EnablePatches()` ([`FikaPlugin.cs:179`](../../modded/Fika-Plugin/Fika.Core/FikaPlugin.cs#L179)) descobre tudo via reflection. Isso é verdade para o único precedente confirmado no código (`ObservedPlayer_DropBackpackSafety_Patch`) — mas esse precedente é uma classe de **nível superior** (top-level), não aninhada. `PatchManager` é um tipo externo (`SPT.Reflection.Patching`, sem fonte neste repo) — não há como confirmar estaticamente se o `EnablePatches()` dessa versão específica enumera **tipos aninhados** (`Assembly.GetTypes()` inclui aninhados por padrão, mas a implementação do `PatchManager` pode filtrar por outro critério, ex. só top-level, ou por namespace). `FikaPlugin.cs:144` também mostra que o mod já usa registro **explícito** (`_patchManager.EnablePatch(new ItemContext_Patch())`) para pelo menos um patch condicional — evidência de que o autor original não confia cegamente no auto-discovery para tudo.

**Por que importa:** Se `EnablePatches()` não pegar as classes aninhadas, os dois patches novos **nunca são habilitados**, e a correção inteira deste item vira um no-op silencioso — sem erro de compilação, sem exceção, só o bug original continuando a acontecer exatamente como antes. Esse é o pior tipo de falha possível para este item (parece pronto, mas não faz nada).

**Sugestão:** Registrar os dois patches explicitamente em `FikaPlugin.cs`, no mesmo estilo já usado na linha 144 — por exemplo, dentro de `EnableModulePatches()` (linha 177-180) ou logo depois, sem depender de condição:
```csharp
_patchManager.EnablePatch(new InOutHandsProcessTimestampPatch.CaptureMovedItemOnRemove());
_patchManager.EnablePatch(new InOutHandsProcessTimestampPatch.RecordBeginSucceed());
```
Isso custa zero risco (registro explícito nunca conflita com auto-discovery — na pior hipótese, checar se `EnablePatch` é idempotente/tolera dupla habilitação antes de aplicar) e elimina a incerteza por completo. Alternativa mais barata, se preferir não mexer em `FikaPlugin.cs` agora: validar via log temporário (`Plugin.Instance.Logger.LogInfo` no `GetTargetMethod()` de cada patch, que só roda se o patch for de fato habilitado) durante a primeira sessão de teste em Headless, antes de assumir que funciona.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` — adicionado `using Fika.Core.Main.Patches.InventoryPatches;` e, em `EnableModulePatches()`, duas chamadas explícitas `_patchManager.EnablePatch(...)` para `CaptureMovedItemOnRemove` e `RecordBeginSucceed`, com comentário `// ref: CR-01-01`.

---

### CR-01-02 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-06

**Stub §5 da spec técnica ainda mostra o patch descartado**

**Local:** [`003-magazine-swap-inplace-fix-02-spec-tech.md`](003-magazine-swap-inplace-fix-02-spec-tech.md) §5 (stub de `InOutHandsProcessTimestampPatch.cs`).

**Problema:** O código real (`InOutHandsProcessTimestampPatch.cs`) não tem mais a classe `CaptureMovedItemOnSet` (Prefix em `Player.TrySetInHands`) — foi descartada durante o `/code-mod` porque `ObservedInventoryController.InProcess` nunca chama esse método (ver asbuild). O checklist §8 e o Histórico da spec técnica documentam essa mudança, mas o **stub de código em si**, em §5, ainda mostra a versão antiga com `CaptureMovedItemOnSet` e sem a chamada direta `SetPendingMovedItem` em `HandleInProcess`.

**Por que importa:** Um leitor futuro que abra só a spec técnica (sem cruzar com o asbuild) vai ler um stub que não bate com o código implementado — pode tentar "completar" um patch que nunca deveria existir, ou ficar confuso sobre por que `HandleInProcess` tem uma linha que o stub não menciona.

**Sugestão:** Atualizar o bloco de código em §5 da spec técnica para refletir o estado real (remover `CaptureMovedItemOnSet`, adicionar a chamada `SetPendingMovedItem(item)` dentro do trecho editado de `HandleInProcess`) — ou, mais simples, adicionar uma nota em negrito logo acima do bloco `CaptureMovedItemOnSet` dizendo `**Descartado durante o /code-mod — ver 003-magazine-swap-inplace-fix-05-asbuild.md.**` sem reescrever o stub inteiro.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto (opção mais simples: nota + comentário, sem reescrever o stub inteiro).
**Aplicação:** `003-magazine-swap-inplace-fix-02-spec-tech.md` §2 (linha da tabela de `TrySetInHands` marcada como descartada, nova linha para `HandleInProcess`) e §5 (bloco `CaptureMovedItemOnSet` comentado com nota `// ref: CR-01-02` explicando o descarte e apontando para o asbuild).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-06 | Code review 01 criada via `/code-review` |
| 2026-09-06 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02; rejeitados: nenhum |
