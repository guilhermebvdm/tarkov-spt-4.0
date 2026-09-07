# 001 — Swap de magazine no mesmo slot ao recarregar com R sem espaço · Code Review 01

**Mod:** UIFixes
**Spec funcional:** [001-reload-swap-sem-espaco-01-spec.md](001-reload-swap-sem-espaco-01-spec.md)
**Spec técnica:** [001-reload-swap-sem-espaco-02-spec-tech.md](001-reload-swap-sem-espaco-02-spec-tech.md)
**Asbuild:** [001-reload-swap-sem-espaco-05-asbuild.md](001-reload-swap-sem-espaco-05-asbuild.md)
**Data:** 2026-09-06

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** sem memória prévia relevante ao item · pendências que afetam: nenhuma.
**Docs técnicos:** `spt-antipatterns.md` relido (AP-04, AP-08, AP-09) — nenhuma violação encontrada.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | E — Legibilidade | 🟢 | Comentário inline não segue o formato canônico `// ref: PA-NN-MM` | ✅ Aplicado em 2026-09-06 |

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

## Verificações que passaram (sem achado)

- **Ordem de operações preservada:** o fallback (linhas 199-206) roda **antes** de `operation.Value.RollBack()` (linha 209) — confirmado lendo o arquivo — então a checagem em `gridMagAddress.Grid.FindLocationForItem(...)` ainda vê a vacância temporária de `magazine`, exatamente como desenhado na spec técnica §1.4/§5.
- **Zero mudança no caso que já funciona:** o fallback só executa quando `candidateAddress == null` (busca ampla já falhou) — confirmado que a busca ampla (linhas 192-197) não foi tocada, resolve PA-01-01 como planejado.
- **`GridItemAddress` resolve sem `using` adicional:** `EFT.InventoryLogic` já importado (linha 7 do arquivo, não alterada) — mesmo padrão já usado em `SwapPatches.cs` (`gridItemAddress.Grid`) no mesmo mod.
- **Guards pré-existentes intocados:** `____player.IsAI` (bots), `__instance.Blindfire` (disparo às cegas) e o guard de entrada (`itemAddress != null && !AlwaysSwapMags`, vanilla-first) continuam exatamente como estavam — cobrem os corner cases da spec funcional sem precisar de código novo.
- **Sem novo ponto de patch:** `SwapIfNoSpacePatch` continua sendo o único patch em `ReloadMag`; nenhum risco de AP-03 (a correção é só lógica interna do `Prefix`).

## Pontos

### CR-01-01 · E — Legibilidade · 🟢 ✅ Aplicado em 2026-09-06

**Comentário inline não segue o formato canônico `// ref: PA-NN-MM`**

**Local:** [`mods/UIFixes/modded/src/Patches/ReloadInPlacePatches.cs:199`](../../modded/src/Patches/ReloadInPlacePatches.cs#L199)

**Problema:** O comentário adicionado é:
```csharp
// ref: 001-reload-swap-sem-espaco (PA-01-01) - GetPrioritizedGridsForUnloadedObject(false) never
// considers the backpack, ...
```
A convenção do repo (`repo-workflow-best-practices` §4) define o formato padrão como `// ref: PA-01-01` (ou `CR-NN-MM`) — sem o slug do item prefixado.

**Por que importa:** Ferramentas/greps que procuram `// ref: PA-` para navegar de código → review não encontram a referência com esse formato (o texto extra antes do ID quebra o padrão esperado). É puramente cosmético — não afeta a lógica nem a compilação — mas reduz a rastreabilidade que a convenção existe pra garantir.

**Sugestão:** Simplificar para o formato padrão, mantendo a explicação técnica na linha seguinte:
```csharp
// ref: PA-01-01 — GetPrioritizedGridsForUnloadedObject(false) never considers the backpack, so if
// the vest/pockets are full and the new magazine came from the backpack, the spot it just vacated
// is invisible to the search above. Fallback: only runs when the search above already failed, so
// it never changes the existing priority/behavior.
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/UIFixes/modded/src/Patches/ReloadInPlacePatches.cs:199-202` — comentário reescrito no formato `// ref: CR-01-01 — ...`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-06 | Code review 01 criada via `/code-review` |
| 2026-09-06 | Aplicação automática de 1 achado via `/apply-code-review` — IDs aplicados: CR-01-01; rejeitados: nenhum |
