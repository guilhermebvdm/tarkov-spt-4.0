# 002 — Schema de classe + loader multi-classe · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [002-class-schema-loader-01-spec.md](002-class-schema-loader-01-spec.md)
**Spec técnica:** [002-class-schema-loader-02-spec-tech.md](002-class-schema-loader-02-spec-tech.md)
**Asbuild:** [002-class-schema-loader-05-asbuild.md](002-class-schema-loader-05-asbuild.md)
**Data:** 2026-06-07

> Análise crítica do código implementado por `/code-mod`. IDs `CR-01-MM` permanentes.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 1 · 🟢 2 · ✅ Aplicados: 3 · Pendentes: 0 · Total: 3

**Contexto positivo:** loader validado no log (`Loaded 2 class(es), skipped 0`; base/skills por arquivo; JSONC + `enabled` default OK). PA-01-01..05 da review técnica aplicados. Achados abaixo são todos não-bloqueadores (robustez/polish); o item pode fechar.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | `baseEdition` apontando p/ outra classe custom é dependente de ordem | ✅ Aplicado |
| CR-01-02 | F — Melhoria | 🟢 | Dedupe de arquivos por path (defesa vs overlap `*.json`/`*.jsonc`) | ✅ Aplicado |
| CR-01-03 | E — Legibilidade | 🟢 | `Trim()` em `name`/`baseEdition` + default de base num só lugar | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria opcional**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

## CR-01-01 · B — Bug latente · 🟡 Médio

**`baseEdition` apontando para outra classe custom depende da ordem de carregamento**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClasses.Server.csproj) — `RegisterClass` (`TryGetValue(baseKey, ...)`).

**Problema:** se um arquivo definir `baseEdition` igual ao `name` de **outra classe custom** (não uma edição vanilla), o sucesso depende de aquela classe já ter sido registrada — e a ordem de iteração de `FileUtil.GetFiles` é dependente do sistema de arquivos. Ex.: classe B com `baseEdition: "Classe A"` só funciona se A for processada antes.

**Por que importa:** comportamento não-determinístico (B carrega às vezes, falha "base não encontrada" outras) — confuso de depurar. Não dispara hoje (exemplos usam base vanilla), mas é plausível quando o usuário cria classes.

**Sugestão:** documentar (no `exampleClass.jsonc` e no README do mod) que **`baseEdition` deve ser uma edição vanilla** (ex.: "SPT Zero to hero", "Standard"). Opcional/futuro: se quisermos suportar base = classe custom, fazer 2 passadas (registrar todas as de base vanilla, depois as que referenciam custom). Para o 002, basta a doc.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## CR-01-02 · F — Melhoria · 🟢 Menor

**Dedupe de arquivos por path (defesa contra overlap de glob)**

**Local:** `CustomClassesMod.OnLoad` — `GetFiles(..., "*.json").Concat(GetFiles(..., "*.jsonc"))`.

**Problema:** em alguns sistemas o glob `*.json` do .NET pode casar também `*.jsonc` (matching legado de extensão). Hoje **não** ocorre (log: `skipped 0` com 2 arquivos `.jsonc`), mas se `FileUtil.GetFiles`/plataforma mudar, cada `.jsonc` seria processado 2× (a 2ª cai no guard de colisão → warning "already exists").

**Por que importa:** baixo — o guard de colisão já previne registro duplo; seria só ruído no log. Mas é uma defesa barata.

**Sugestão:** deduplicar por path antes de iterar: `.Concat(...).Distinct().ToList()` (ou `DistinctBy` no path). 1 linha.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## CR-01-03 · E — Legibilidade · 🟢 Menor

**`Trim()` em `name`/`baseEdition` + default de base centralizado**

**Local:** `CustomClassesMod` — validação de `def.Name` e `DefaultBaseEdition` (const no loader) vs `enabled` default (no DTO).

**Problema:** (1) `name`/`baseEdition` vindos de JSON editado à mão podem ter espaço acidental (`" Test Class "`) → vira chave/edition com espaço sobrando. (2) O default de `enabled` mora no DTO (`= true`) mas o default de `baseEdition` mora no loader (`DefaultBaseEdition`) — dois lugares.

**Por que importa:** polish/consistência; baixo risco.

**Sugestão:** `Trim()` em `name` e `baseEdition` ao usar; opcionalmente mover o default de `baseEdition` para o DTO (`= "SPT Zero to hero"`) para concentrar defaults num lugar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Resolução (apply-code-review — 2026-06-07)

Os 3 aceitos e aplicados em `CustomClassesMod.cs` / `exampleClass.jsonc`; recompilado (0 warn/err):
- **CR-01-01** ✅ — comentário no `exampleClass.jsonc` deixando claro que `baseEdition` deve ser uma **edição vanilla** (não outra classe custom).
- **CR-01-02** ✅ — `.Distinct()` na lista de arquivos (defesa contra overlap de glob).
- **CR-01-03** ✅ — `Trim()` em `name` e `baseEdition`.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 01 criada via `/code-review` |
| 2026-06-07 | 3 achados aplicados via `/apply-code-review` (CR-01-01/02/03) + rebuild |
