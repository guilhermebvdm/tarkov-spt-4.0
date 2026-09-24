# 004 — Gotas na Lente Reativas ao Ângulo da Câmera · Code Review 01

**Mod:** TRL-WeatherSync  
**Spec funcional:** [004-gotas-lente-reativas-angulo-camera-01-spec.md](004-gotas-lente-reativas-angulo-camera-01-spec.md)  
**Spec técnica:** [004-gotas-lente-reativas-angulo-camera-02-spec-tech.md](004-gotas-lente-reativas-angulo-camera-02-spec-tech.md)  
**Asbuild:** [004-gotas-lente-reativas-angulo-camera-05-asbuild.md](004-gotas-lente-reativas-angulo-camera-05-asbuild.md)  
**Data:** 2026-09-12  

> Análise crítica do código implementado no item 004. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F | 🟢 | Auditoria de segurança de filas e acesso a coleções no Prefix de renderização | ✅ Resolvido em 2026-09-12 |
| CR-01-02 | E | 🟢 | Sincronização estrita de SemVer (1.3.0) em todos os pontos do mod | ✅ Resolvido em 2026-09-12 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional / qualidade.

---

## Pontos

### CR-01-01 · F — Melhoria Opcional · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Auditoria de segurança de filas e acesso a coleções no Prefix de renderização**

**Local:** [`mods/TRL-WeatherSync/modded/Client/Patches/CameraLensRainDropsPatch.cs:64-80`](../../modded/Client/Patches/CameraLensRainDropsPatch.cs#L64)

**Problema:** O método nativo `__instance.method_1` faz `Queue_0.Dequeue()` sem checagem de tamanho se chamado diretamente. Se a fila estivesse vazia, lançaria `InvalidOperationException`.

**Avaliação:** O loop no patch verifica explicitamente `if (__instance.Queue_0.Count > 0)` antes de cada chamada, além de checar `if (__instance.List_0.Count > 0)` antes de ajustar o `Lifetime` da última gota.

**Sugestão:** Confirmar que a proteção está ativa e cobre todos os caminhos.

**Decisão:**
- `[x]` Aceitar sugestão (auditado e aprovado)

**Resolução:** Nenhuma alteração necessária; checagens defensivas presentes e robustas.

---

### CR-01-02 · E — Legibilidade/Manutenção · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Sincronização estrita de SemVer (1.3.0) em todos os pontos do mod**

**Local:** `Plugin.cs:9`, `TRL-WeatherSync.csproj:9`, `PROPRIEDADES.md:1`

**Problema:** Divergências de versão no assembly compilado, no BepInEx ou na documentação.

**Avaliação:** Todos os três arquivos foram atualizados de forma uniforme para a versão `1.3.0` (bump minor correspondente a uma nova feature funcional visível).

**Sugestão:** Validar igualdade de versão.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Versão `1.3.0` consistente em todos os arquivos.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Auto code-review 01 concluída com zero bloqueadores 🔴. |
