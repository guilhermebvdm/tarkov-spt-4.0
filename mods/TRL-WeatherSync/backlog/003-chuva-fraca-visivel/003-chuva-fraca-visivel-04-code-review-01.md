# 003 — Chuva Fraca Mais Visível (Tamanho Mínimo de Gota) · Code Review 01

**Mod:** TRL-WeatherSync  
**Spec funcional:** [003-chuva-fraca-visivel-01-spec.md](003-chuva-fraca-visivel-01-spec.md)  
**Spec técnica:** [003-chuva-fraca-visivel-02-spec-tech.md](003-chuva-fraca-visivel-02-spec-tech.md)  
**Asbuild:** [003-chuva-fraca-visivel-05-asbuild.md](003-chuva-fraca-visivel-05-asbuild.md)  
**Data:** 2026-09-12  

> Análise crítica do código implementado. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F | 🟢 | Verificação de impacto de alocação de memória (GC Alloc) no Postfix executado a cada frame | ✅ Resolvido em 2026-09-12 |
| CR-01-02 | E | 🟢 | Sincronização estrita de SemVer entre Plugin.cs e TRL-WeatherSync.csproj | ✅ Resolvido em 2026-09-12 |

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
- 🟢 **Menor** — opcional / qualidade.

---

## Pontos

### CR-01-01 · F — Melhoria Opcional · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Verificação de impacto de alocação de memória (GC Alloc) no Postfix executado a cada frame**

**Local:** [`mods/TRL-WeatherSync/modded/Client/Patches/RainDropVisibilityPatch.cs:22-54`](../../modded/Client/Patches/RainDropVisibilityPatch.cs#L22)

**Problema:** Métodos invocados no loop `Update()` todo frame podem gerar pausas de garbage collection caso aloquem objetos gerenciados na heap.

**Avaliação:** O código do Postfix manipula estritamente structs no stack (`float`, `Vector4`), campos estáticos primitivos em cache (`_sizePropertyId`, `_hasLoggedError`) e referências já existentes (`Material ___material_0`). Não há instanciação de classes nem boxing (`0 B GC Alloc` por chamada).

**Sugestão:** Manter a implementação atual, confirmando que a performance é ótima e ideal para métodos de renderização por frame.

**Decisão:**
- `[x]` Aceitar sugestão (auditado e aprovado)

**Resolução:** Nenhuma alteração necessária; design confirmado com alocação zero.

---

### CR-01-02 · E — Legibilidade/Manutenção · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Sincronização estrita de SemVer entre Plugin.cs e TRL-WeatherSync.csproj**

**Local:** [`mods/TRL-WeatherSync/modded/Client/Plugin.cs:9`](../../modded/Client/Plugin.cs#L9) e [`mods/TRL-WeatherSync/modded/Client/TRL-WeatherSync.csproj:9`](../../modded/Client/TRL-WeatherSync.csproj#L9)

**Problema:** Divergência de versão entre metadados do BepInEx e atributos do assembly compilado causa inconsistência no log e na identificação de bugs pelo usuário.

**Avaliação:** Ambos os arquivos foram incrementados de forma idêntica para `1.2.0`, refletindo adequadamente o acréscimo de uma nova feature funcional e visível (bump minor `y`).

**Sugestão:** Confirmar integridade de ambas as declarações.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Validada a igualdade de versão (`1.2.0`) em ambos os arquivos e na documentação `PROPRIEDADES.md`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Auto code-review 01 concluída com 0 bloqueadores 🔴 e conformidade plena. |
