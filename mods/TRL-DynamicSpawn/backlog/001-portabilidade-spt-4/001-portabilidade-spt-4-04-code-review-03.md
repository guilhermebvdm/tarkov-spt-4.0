# 001 — portabilidade-spt-4 · Code Review 03

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** Não aplicável (fallback legado)
**Data:** 2026-07-17T22:27:00-03:00

> Análise crítica do código implementado. Cada achado recebe um ID `CR-03-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | D — Arquitetura | 🟠 Forte | Desativação de Follower de Bosses Incorreta | ✅ Resolvido |
| CR-02-02 | E — Manutenção | 🟡 Médio | Reflection Insegura de Campo OnPlayerDead | ✅ Resolvido |

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

### ✅ CR-02-01 · D — Arquitetura · 🟠 Forte (Resolvido)
*Desativação de Follower de Bosses Incorreta*
* **Resolução**: Implementada lógica de pesquisa dinâmica das ondas nativas do mapa para extrair e instanciar os seguidores e quantidades originais de cada Boss.

### ✅ CR-02-02 · E — Manutenção · 🟡 Médio (Resolvido)
*Reflection Insegura de Campo OnPlayerDead*
* **Resolução**: Substituído o acesso manual via System.Reflection pelo wrapper oficial `HarmonyLib.AccessTools.Field`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 03 criada via `/code-review` |
